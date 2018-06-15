using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Mime
{
	internal class HeaderFieldParameterPart
	{
		private readonly string _value;

		// для параметров regular-parameter и extended-other-parameter
		internal HeaderFieldParameterPart (string name, string value, int section, bool isExtendedValue)
		{
			this.Name = name;
			_value = value;
			this.Section = section;
			this.IsExtendedValue = isExtendedValue;
		}

		// для параметров extended-initial-parameter
		internal HeaderFieldParameterPart (string name, string value, string encoding, string language)
		{
			this.Name = name;
			_value = value;
			this.Section = 0;
			this.Encoding = encoding;
			this.Language = language;
			this.IsExtendedValue = true;
		}

		internal string Name { get; }

		internal int Section { get; }

		internal string Encoding { get; }

		internal string Language { get; }

		internal bool IsExtendedValue { get; }

		internal static HeaderFieldParameterPart Parse (ReadOnlySpan<byte> source, IReadOnlyList<StructuredValueElement> tokens)
		{
			/*
			RFC 2184 part 7:

			parameters             := *(";" parameter)
			parameter              := regular-parameter / extended-parameter

			regular-parameter      := regular-name "=" value
			extended-parameter     := (extended-initial-name "=" extended-initial-value) / (extended-other-names "=" extended-other-values)

			regular-name := attribute [section]

			extended-initial-name  := attribute [initial-section] "*"
			extended-initial-value := [charset] "'" [language] "'" extended-other-values

			extended-other-names   := attribute other-sections "*"
			extended-other-values  := *(ext-octet / attribute-char)
			ext-octet              := "%" 2(DIGIT / "A" / "B" / "C" / "D" / "E" / "F")

			section                := initial-section / other-sections
			initial-section        := "*0"
			other-sections         := "*" ("1" / "2" / "3" / "4" / "5" / "6" / "7" / "8" / "9") *DIGIT)
			*/

			if ((tokens.Count < 3) ||
				(tokens[0].ElementType != StructuredValueElementType.Value) ||
				(tokens[1].ElementType != StructuredValueElementType.Separator))
			{
				throw new FormatException ("Invalid format of header field parameter.");
			}

			var parameterName = AsciiCharSet.GetString (source.Slice (tokens[0].StartPosition, tokens[0].Length));
			var section = 0;
			string encoding = null;
			var isExtendedValue = false;

			int idx = 1;
			var isSeparator = (tokens[idx].ElementType == StructuredValueElementType.Separator) && (tokens[idx].Length == 1) && (source[tokens[idx].StartPosition] == (byte)'*');
			if (isSeparator)
			{
				idx++;
				if (tokens[idx].ElementType != StructuredValueElementType.Value)
				{
					throw new FormatException ("Invalid format of header field parameter.");
				}

				section = int.Parse (
					AsciiCharSet.GetString (source.Slice (tokens[idx].StartPosition, tokens[idx].Length)),
					NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite,
					CultureInfo.InvariantCulture);
				idx++;
				if (tokens[idx].ElementType != StructuredValueElementType.Separator)
				{
					throw new FormatException ("Invalid format of header field parameter.");
				}
			}

			isSeparator = (tokens[idx].ElementType == StructuredValueElementType.Separator) && (tokens[idx].Length == 1) && (source[tokens[idx].StartPosition] == (byte)'*');
			if (isSeparator)
			{
				isExtendedValue = true;
				idx++;
			}
			else
			{
				var isEqualitySign = (tokens[idx].ElementType == StructuredValueElementType.Separator) && (tokens[idx].Length == 1) && (source[tokens[idx].StartPosition] == (byte)'=');
				if (!isEqualitySign)
				{
					throw new FormatException ("Invalid format of header field parameter.");
				}
			}

			idx++;
			var isExtendedInitialValue = isExtendedValue && (section == 0);
			if (isExtendedInitialValue)
			{
				string language = null;
				if (tokens[idx].ElementType == StructuredValueElementType.Value)
				{ // charset
					encoding = AsciiCharSet.GetString (source.Slice (tokens[idx].StartPosition, tokens[idx].Length));
					idx++;
				}

				if ((tokens[idx].ElementType != StructuredValueElementType.Separator) || (tokens[idx].Length != 1) || (source[tokens[idx].StartPosition] != (byte)'\''))
				{
					throw new FormatException ("Invalid format of header field parameter.");
				}

				idx++;
				if (tokens[idx].ElementType == StructuredValueElementType.Value)
				{ // language
					language = AsciiCharSet.GetString (source.Slice (tokens[idx].StartPosition, tokens[idx].Length));
					idx++;
				}

				if ((tokens[idx].ElementType != StructuredValueElementType.Separator) || (tokens[idx].Length != 1) || (source[tokens[idx].StartPosition] != (byte)'\''))
				{
					throw new FormatException ("Invalid format of header field parameter.");
				}

				idx++;
				if (tokens[idx].ElementType != StructuredValueElementType.Value)
				{
					throw new FormatException ("Invalid format of header field parameter.");
				}

				return new HeaderFieldParameterPart (parameterName, AsciiCharSet.GetString (source.Slice (tokens[idx].StartPosition, tokens[idx].Length)), encoding, language);
			}

			if (isExtendedValue)
			{
				if (tokens[idx].ElementType != StructuredValueElementType.Value)
				{
					throw new FormatException ("Invalid format of header field parameter.");
				}

				return new HeaderFieldParameterPart (parameterName, AsciiCharSet.GetString (source.Slice (tokens[idx].StartPosition, tokens[idx].Length)), section, true);
			}

			if ((tokens[idx].ElementType != StructuredValueElementType.Value) && (tokens[idx].ElementType != StructuredValueElementType.QuotedValue))
			{
				throw new FormatException ("Invalid format of header field parameter.");
			}

			return new HeaderFieldParameterPart (
				parameterName,
				StructuredValueDecoder.DecodeElement (source.Slice (tokens[idx].StartPosition, tokens[idx].Length), tokens[idx].ElementType),
				section,
				false);
		}

		internal string GetValue (Encoding encoding)
		{
			if (!this.IsExtendedValue)
			{
				return _value;
			}

			int offset = 0;
			var decodedBuffer = ArrayPool<byte>.Shared.Rent (_value.Length);
			for (var i = 0; i < _value.Length; i++)
			{
				if (_value[i] != '%')
				{
					decodedBuffer[offset++] = (byte)_value[i];
				}
				else
				{
					decodedBuffer[offset++] = Hex.ParseByte (_value, i + 1);
					i += 2;
				}
			}

			var result = encoding.GetString (decodedBuffer, 0, offset);
			ArrayPool<byte>.Shared.Return (decodedBuffer);
			return result;
		}
	}
}
