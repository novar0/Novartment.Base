using System;
using System.Buffers;
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

		internal static HeaderFieldParameterPart Parse (ReadOnlySpan<char> source, ref int parserPos)
		{
			/*
			RFC 2184 part 7:

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

			var nameElement = StructuredHeaderFieldLexicalToken.ParseToken (source, ref parserPos);
			var separatorElement = StructuredHeaderFieldLexicalToken.ParseToken (source, ref parserPos);

			if (
				(nameElement.TokenType != StructuredHeaderFieldLexicalTokenType.Value) ||
				(separatorElement.TokenType != StructuredHeaderFieldLexicalTokenType.Separator))
			{
				throw new FormatException ("Invalid format of header field parameter.");
			}

#if NETCOREAPP2_1
			var parameterName = new string (source.Slice (nameElement.Position, nameElement.Length));
#else
			var parameterName = new string (source.Slice (nameElement.Position, nameElement.Length).ToArray ());
#endif
			var section = 0;
			string encoding = null;
			var isExtendedValue = false;

			if (source[separatorElement.Position] == '*')
			{
				var sectionElement = StructuredHeaderFieldLexicalToken.ParseToken (source, ref parserPos);
				if (sectionElement.TokenType != StructuredHeaderFieldLexicalTokenType.Value)
				{
					throw new FormatException ("Invalid format of header field parameter.");
				}

#if NETCOREAPP2_1
				section = int.Parse (
					source.Slice (sectionElement.Position, sectionElement.Length),
					NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite,
					CultureInfo.InvariantCulture);
#else
				section = int.Parse (
					new string (source.Slice (sectionElement.Position, sectionElement.Length).ToArray ()),
					NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite,
					CultureInfo.InvariantCulture);
#endif
				separatorElement = StructuredHeaderFieldLexicalToken.ParseToken (source, ref parserPos);
				if (separatorElement.TokenType != StructuredHeaderFieldLexicalTokenType.Separator)
				{
					throw new FormatException ("Invalid format of header field parameter.");
				}
			}

			var isStarSeparator = (separatorElement.TokenType == StructuredHeaderFieldLexicalTokenType.Separator) && (source[separatorElement.Position] == '*');
			if (isStarSeparator)
			{
				isExtendedValue = true;
				separatorElement = StructuredHeaderFieldLexicalToken.ParseToken (source, ref parserPos);
			}
			else
			{
				var isEqualitySign = (separatorElement.TokenType == StructuredHeaderFieldLexicalTokenType.Separator) && (source[separatorElement.Position] == '=');
				if (!isEqualitySign)
				{
					throw new FormatException ("Invalid format of header field parameter.");
				}
			}

			var isExtendedInitialValue = isExtendedValue && (section == 0);
			var item = StructuredHeaderFieldLexicalToken.ParseToken (source, ref parserPos);
			if (isExtendedInitialValue)
			{
				string language = null;
				if (item.TokenType == StructuredHeaderFieldLexicalTokenType.Value)
				{ // charset
#if NETCOREAPP2_1
					encoding = new string (source.Slice (item.Position, item.Length));
#else
					encoding = new string (source.Slice (item.Position, item.Length).ToArray ());
#endif
					item = StructuredHeaderFieldLexicalToken.ParseToken (source, ref parserPos);
				}

				if ((item.TokenType != StructuredHeaderFieldLexicalTokenType.Separator) || (source[item.Position] != '\''))
				{
					throw new FormatException ("Invalid format of header field parameter.");
				}

				item = StructuredHeaderFieldLexicalToken.ParseToken (source, ref parserPos);
				if (item.TokenType == StructuredHeaderFieldLexicalTokenType.Value)
				{ // language
#if NETCOREAPP2_1
					language = new string (source.Slice (item.Position, item.Length));
#else
					language = new string (source.Slice (item.Position, item.Length).ToArray ());
#endif
					item = StructuredHeaderFieldLexicalToken.ParseToken (source, ref parserPos);
				}

				if ((item.TokenType != StructuredHeaderFieldLexicalTokenType.Separator) || (source[item.Position] != '\''))
				{
					throw new FormatException ("Invalid format of header field parameter.");
				}

				item = StructuredHeaderFieldLexicalToken.ParseToken (source, ref parserPos);
				if (item.TokenType != StructuredHeaderFieldLexicalTokenType.Value)
				{
					throw new FormatException ("Invalid format of header field parameter.");
				}

#if NETCOREAPP2_1
				var valueStr = new string (source.Slice (item.Position, item.Length));
#else
				var valueStr = new string (source.Slice (item.Position, item.Length).ToArray ());
#endif
				return new HeaderFieldParameterPart (parameterName, valueStr, encoding, language);
			}

			if (isExtendedValue)
			{
				if (item.TokenType != StructuredHeaderFieldLexicalTokenType.Value)
				{
					throw new FormatException ("Invalid format of header field parameter.");
				}

#if NETCOREAPP2_1
				var valueStr = new string (source.Slice (item.Position, item.Length));
#else
				var valueStr = new string (source.Slice (item.Position, item.Length).ToArray ());
#endif
				return new HeaderFieldParameterPart (parameterName, valueStr, section, true);
			}

			if ((item.TokenType != StructuredHeaderFieldLexicalTokenType.Value) && (item.TokenType != StructuredHeaderFieldLexicalTokenType.QuotedValue))
			{
				throw new FormatException ("Invalid format of header field parameter.");
			}

			return new HeaderFieldParameterPart (
				parameterName,
				item.Decode (source),
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
