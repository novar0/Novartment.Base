﻿using System;
using System.Buffers;
using System.Text;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Mime
{
	internal readonly ref struct HeaderFieldBodyParameterPart
	{
		private readonly StructuredStringToken _value;
		private readonly bool _isExtendedValue;

		// для параметров regular-parameter и extended-other-parameter
		internal HeaderFieldBodyParameterPart (StructuredStringToken name, StructuredStringToken value, bool isFirstSection, bool isExtendedValue)
		{
			this.Name = name;
			_value = value;
			this.IsFirstSection = isFirstSection;
			this.Encoding = null;
			_isExtendedValue = isExtendedValue;
		}

		// для параметров extended-initial-parameter
		internal HeaderFieldBodyParameterPart (StructuredStringToken name, StructuredStringToken value, string encoding)
		{
			this.Name = name;
			_value = value;
			this.IsFirstSection = true;
			this.Encoding = encoding;
			_isExtendedValue = true;
		}

		internal readonly StructuredStringToken Name { get; }

		internal readonly bool IsFirstSection { get; }

		internal readonly string Encoding { get; }

		internal static HeaderFieldBodyParameterPart Parse (ReadOnlySpan<char> source, ref int parserPos)
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

			StructuredStringToken nameToken;
			do
			{
				nameToken = HeaderDecoder.TokenParser.Parse (source, ref parserPos);
			} while (nameToken.IsRoundBracketedValue (source));

			StructuredStringToken separatorToken;
			do
			{
				separatorToken = HeaderDecoder.TokenParser.Parse (source, ref parserPos);
			} while (separatorToken.IsRoundBracketedValue (source));

			if ((nameToken.TokenType != StructuredStringTokenType.Value) ||
				(separatorToken.TokenType != StructuredStringTokenType.Separator))
			{
				throw new FormatException ("Invalid format of header field parameter.");
			}

			bool isZeroSection = true;
			var isExtendedValue = false;
			if (source[separatorToken.Position] == '*')
			{
				StructuredStringToken sectionToken;
				do
				{
					sectionToken = HeaderDecoder.TokenParser.Parse (source, ref parserPos);
				} while (sectionToken.IsRoundBracketedValue (source));

				if (sectionToken.TokenType != StructuredStringTokenType.Value)
				{
					throw new FormatException ("Invalid format of header field parameter.");
				}

				isZeroSection = ((sectionToken.Length == 1) && (source[sectionToken.Position] == '0')) ||
					((sectionToken.Length == 2) && (source[sectionToken.Position] == '0') && (source[sectionToken.Position + 1] == '0'));
				do
				{
					separatorToken = HeaderDecoder.TokenParser.Parse (source, ref parserPos);
				} while (separatorToken.IsRoundBracketedValue (source));

				if (separatorToken.TokenType != StructuredStringTokenType.Separator)
				{
					throw new FormatException ("Invalid format of header field parameter.");
				}
			}

			if (separatorToken.IsSeparator (source, '*'))
			{
				isExtendedValue = true;
				parserPos++;
			}
			else
			{
				if (!separatorToken.IsSeparator (source, '='))
				{
					throw new FormatException ("Invalid format of header field parameter.");
				}
			}

			var isExtendedInitialValue = isExtendedValue && isZeroSection;
			StructuredStringToken token;
			do
			{
				token = HeaderDecoder.TokenParser.Parse (source, ref parserPos);
			} while (token.IsRoundBracketedValue (source));

			if (isExtendedInitialValue)
			{
				string encoding = null;
				if (token.TokenType == StructuredStringTokenType.Value)
				{
					// charset
#if NETSTANDARD2_0
					encoding = new string (source.Slice (token.Position, token.Length).ToArray ());
#else
					encoding = new string (source.Slice (token.Position, token.Length));
#endif
					token = HeaderDecoder.TokenParser.Parse (source, ref parserPos);
				}

				if (!token.IsSeparator (source, '\''))
				{
					throw new FormatException ("Invalid format of header field parameter.");
				}

				do
				{
					token = HeaderDecoder.TokenParser.Parse (source, ref parserPos);
				} while (token.IsRoundBracketedValue (source));

				if (token.TokenType == StructuredStringTokenType.Value)
				{
					// skip language
					do
					{
						token = HeaderDecoder.TokenParser.Parse (source, ref parserPos);
					} while (token.IsRoundBracketedValue (source));
				}

				if (!token.IsSeparator (source, '\''))
				{
					throw new FormatException ("Invalid format of header field parameter.");
				}

				do
				{
					token = HeaderDecoder.TokenParser.Parse (source, ref parserPos);
				} while (token.IsRoundBracketedValue (source));
				if (token.TokenType != StructuredStringTokenType.Value)
				{
					throw new FormatException ("Invalid format of header field parameter.");
				}

				return new HeaderFieldBodyParameterPart (nameToken, token, encoding);
			}

			if (isExtendedValue)
			{
				if (token.TokenType != StructuredStringTokenType.Value)
				{
					throw new FormatException ("Invalid format of header field parameter.");
				}

				return new HeaderFieldBodyParameterPart (nameToken, token, isZeroSection, true);
			}

			if ((token.TokenType != StructuredStringTokenType.Value) && !token.IsDoubleQuotedValue (source))
			{
				throw new FormatException ("Invalid format of header field parameter.");
			}

			// regular-parameter
			return new HeaderFieldBodyParameterPart (nameToken, token, isZeroSection, false);
		}

		// нельзя пользоваться this.Encoding, потому что кодировка указывается только для первой части параметра
		internal int GetValue (ReadOnlySpan<char> source, Encoding encoding, char[] destination, int destinationPos)
		{
			if (!_isExtendedValue)
			{
				return HeaderDecoder.DecodeStructuredHeaderFieldBodyToken (_value, source, destination.AsSpan (destinationPos));
			}

			int offset = 0;
			var decodedBuffer = ArrayPool<byte>.Shared.Rent (_value.Length);
			try
			{
				for (var i = 0; i < _value.Length; i++)
				{
					if (source[_value.Position + i] != '%')
					{
						decodedBuffer[offset++] = (byte)source[_value.Position + i];
					}
					else
					{
						decodedBuffer[offset++] = Hex.ParseByte (source[_value.Position + i + 1], source[_value.Position + i + 2]);
						i += 2;
					}
				}

				var size = encoding.GetChars (decodedBuffer, 0, offset, destination, destinationPos);
				return size;
			}
			finally
			{
				ArrayPool<byte>.Shared.Return (decodedBuffer);
			}
		}
	}
}
