using System;
using System.Buffers;
using System.Diagnostics.Contracts;
using System.Text;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// The parameter of the header field body of the generic message format defined in RFC 822.
	/// </summary>
	public class HeaderFieldBodyParameter :
		IValueHolder<string>,
		IEquatable<HeaderFieldBodyParameter>
	{
		private string _value;

		/// <summary>
		/// Initializes a new instance of the HeaderFieldParameter class with a specified name and a value.
		/// </summary>
		/// <param name="name">The name of the parameter.</param>
		/// <param name="value">The value of the parameter.</param>
		public HeaderFieldBodyParameter (string name, string value)
		{
			if (name == null)
			{
				throw new ArgumentNullException (nameof (name));
			}

			if ((name.Length < 1) ||
				!AsciiCharSet.IsAllOfClass (name, AsciiCharClasses.Token))
			{
				throw new ArgumentOutOfRangeException (nameof (name));
			}

			Contract.EndContractBlock ();

			this.Name = name;
			_value = value;
		}

		/// <summary>
		/// Gets the name of the parameter.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Gets the value of the parameter.
		/// </summary>
		public string Value
		{
			get => _value;
			set { _value = value; }
		}

		/// <summary>
		/// Returns a value that indicates whether two HeaderFieldParameter objects are equal.
		/// </summary>
		/// <param name="first">The first segment to compare.</param>
		/// <param name="second">The second segment to compare.</param>
		/// <returns>True if the two HeaderFieldParameter objects are equal; otherwise, False.</returns>
		public static bool operator == (HeaderFieldBodyParameter first, HeaderFieldBodyParameter second)
		{
			return first is null ?
				second is null :
				first.Equals (second);
		}

		/// <summary>
		/// Returns a value that indicates whether two HeaderFieldParameter objects are not equal.
		/// </summary>
		/// <param name="first">The first segment to compare.</param>
		/// <param name="second">The second segment to compare.</param>
		/// <returns>True if the two HeaderFieldParameter objects are not equal; otherwise, False.</returns>
		public static bool operator != (HeaderFieldBodyParameter first, HeaderFieldBodyParameter second)
		{
			return !(first is null ?
				second is null :
				first.Equals (second));
		}

		/// <summary>
		/// Returns a string that represents the this object.
		/// </summary>
		/// <returns>A string that represents the current object.</returns>
		public override string ToString ()
		{
			return this.Name + "=" + _value;
		}

		/// <summary>
		/// Returns the hash code for this object.
		/// </summary>
		/// <returns>A 32-bit signed integer hash code.</returns>
		public override int GetHashCode ()
		{
#if NETSTANDARD2_0
			return StringComparer.OrdinalIgnoreCase.GetHashCode (this.Name) ^ (_value?.GetHashCode () ?? 0);
#else
			return StringComparer.OrdinalIgnoreCase.GetHashCode (this.Name) ^ (_value?.GetHashCode (StringComparison.Ordinal) ?? 0);
#endif
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="obj">Объект, который требуется сравнить с текущим объектом. </param>
		/// <returns>True if the current object is equal to the other parameter; otherwise, False.</returns>
		public override bool Equals (object obj)
		{
			var typedOther = obj as HeaderFieldBodyParameter;
			return (typedOther != null) && Equals (typedOther);
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">Объект, который требуется сравнить с текущим объектом. </param>
		/// <returns>True if the current object is equal to the other parameter; otherwise, False.</returns>
		public bool Equals (HeaderFieldBodyParameter other)
		{
			if (other == null)
			{
				return false;
			}

			return
				string.Equals (this.Name, other.Name, StringComparison.OrdinalIgnoreCase) &&
				string.Equals (_value, other._value, StringComparison.Ordinal);
		}

		internal static HeaderFieldBodyParameter Parse (ReadOnlySpan<char> source, char[] outBuf, ref int parserPos)
		{
			/*
			RFC 2045 part 5.1:
			content := "Content-Type" ":" type "/" subtype *(";" parameter)

			RFC 2183 part 2:
			disposition := "Content-Disposition" ":" disposition-type *(";" disposition-parm)

			RFC 2184 part 7 (extensions to the RFC 2045 media type and RFC 2183):

			parameter              := regular-parameter / extended-parameter

			regular-parameter      := regular-name "=" value
			extended-parameter     := (extended-initial-name "=" extended-initial-value) / (extended-other-names "=" extended-other-values)

			regular-name           := attribute [section]
			extended-initial-name  := attribute [initial-section] "*"
			extended-other-names   := attribute other-sections "*"

			section                := initial-section / other-sections
			initial-section        := "*0"
			other-sections         := "*" ("1" / "2" / "3" / "4" / "5" / "6" / "7" / "8" / "9") *DIGIT)

			extended-initial-value := [charset] "'" [language] "'" extended-other-values
			extended-other-values  := *(ext-octet / attribute-char)
			ext-octet              := "%" 2(DIGIT / "A" / "B" / "C" / "D" / "E" / "F")
			*/

			string parameterName = null;
			var outPos = 0;
			var encoding = Encoding.ASCII;
			// цикл по накоплению секций значения
			// каждая секция является полноценным параметром, поэтому начинается со знака ';' и содержит имя параметра
			// значение будет накапливаться в outBuf, а outPos будет содержать текущую позицию
			while (true)
			{
				var startPos = parserPos;
				StructuredStringToken semicolonToken;
				do
				{
					semicolonToken = HeaderDecoder.TokenFormat.ParseToken (source, ref parserPos);
				} while (semicolonToken.Format is TokenFormatComment);

				if (semicolonToken.Format == null)
				{
					// строка кончилась, возвращаем накопленные секции как параметр
					if (parameterName == null)
					{
						// ничего не накопилось, означает конец строки
						return null;
					}

					return new HeaderFieldBodyParameter (parameterName, new string (outBuf, 0, outPos));
				}

				var isSeparator = semicolonToken.IsSeparator (source, ';');
				if (!isSeparator)
				{
					throw new FormatException ("Value does not conform to 'atom *(; parameter)' format.");
				}

				StructuredStringToken nameToken;
				do
				{
					nameToken = HeaderDecoder.TokenFormat.ParseToken (source, ref parserPos);
				} while (nameToken.Format is TokenFormatComment);

				if (!(nameToken.Format is StructuredStringTokenValueFormat))
				{
					throw new FormatException ("Invalid format of header field parameter.");
				}

#if NETSTANDARD2_0
				var sectionName = new string (source.Slice (nameToken.Position, nameToken.Length).ToArray ());
#else
				var sectionName = new string (source.Slice (nameToken.Position, nameToken.Length));
#endif
				if (parameterName == null)
				{
					parameterName = sectionName;
				}

				if (!sectionName.Equals (parameterName, StringComparison.OrdinalIgnoreCase))
				{
					// начался новый параметр, возвращаем накопленные секции как параметр
					parserPos = startPos;
					return new HeaderFieldBodyParameter (parameterName, new string (outBuf, 0, outPos));
				}

				outPos += DecodeSection (source, ref parserPos, ref encoding, outBuf, outPos); 
			}
		}

		// source должен указывать на знак, следующий после имени параметра (attribute)
		private static int DecodeSection (
			ReadOnlySpan<char> source,
			ref int parserPos,
			ref Encoding encoding,
			char[] destination,
			int destinationPos)
		{
			var separatorToken = HeaderDecoder.TokenFormat.ParseToken (source, ref parserPos);
			if (!(separatorToken.Format is StructuredStringTokenSeparatorFormat))
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
					sectionToken = HeaderDecoder.TokenFormat.ParseToken (source, ref parserPos);
				} while (sectionToken.Format is TokenFormatComment);

				if (!(sectionToken.Format is StructuredStringTokenValueFormat))
				{
					throw new FormatException ("Invalid format of header field parameter.");
				}

				isZeroSection = ((sectionToken.Length == 1) && (source[sectionToken.Position] == '0')) ||
					((sectionToken.Length == 2) && (source[sectionToken.Position] == '0') && (source[sectionToken.Position + 1] == '0'));
				separatorToken = HeaderDecoder.TokenFormat.ParseToken (source, ref parserPos);
				if (!(separatorToken.Format is StructuredStringTokenSeparatorFormat))
				{
					throw new FormatException ("Invalid format of header field parameter.");
				}
			}

			if (source[separatorToken.Position] == '*')
			{
				isExtendedValue = true;
				parserPos++;
			}
			else
			{
				if (source[separatorToken.Position] != '=')
				{
					throw new FormatException ("Invalid format of header field parameter.");
				}
			}

			var isExtendedInitialValue = isExtendedValue && isZeroSection;
			StructuredStringToken token;
			do
			{
				token = HeaderDecoder.TokenFormat.ParseToken (source, ref parserPos);
			} while (token.Format is TokenFormatComment);

			if (isExtendedInitialValue)
			{
				string encodingName = null;
				if (token.Format is StructuredStringTokenValueFormat)
				{
					// charset
#if NETSTANDARD2_0
					encodingName = new string (source.Slice (token.Position, token.Length).ToArray ());
#else
					encodingName = new string (source.Slice (token.Position, token.Length));
#endif
					encoding = GetEncoding (encodingName);
					token = HeaderDecoder.TokenFormat.ParseToken (source, ref parserPos);
				}

				if (!token.IsSeparator (source, '\''))
				{
					throw new FormatException ("Invalid format of header field parameter.");
				}

				token = HeaderDecoder.TokenFormat.ParseToken (source, ref parserPos);
				if (token.Format is StructuredStringTokenValueFormat)
				{
					// skip language
					token = HeaderDecoder.TokenFormat.ParseToken (source, ref parserPos);
				}

				if (!token.IsSeparator (source, '\''))
				{
					throw new FormatException ("Invalid format of header field parameter.");
				}

				// value
				token = HeaderDecoder.TokenFormat.ParseToken (source, ref parserPos);
			}

			if (isExtendedValue)
			{
				if (!(token.Format is StructuredStringTokenValueFormat))
				{
					throw new FormatException ("Invalid format of header field parameter.");
				}

				return HeaderDecoder.DecodeParameterExtendedValue (source.Slice (token.Position, token.Length), destination.AsSpan (destinationPos), encoding);
			}

			if (!(token.Format is StructuredStringTokenValueFormat) && !(token.Format is StructuredStringTokenQuotedStringFormat))
			{
				throw new FormatException ("Invalid format of header field parameter.");
			}

			// regular-parameter
			return token.Format.DecodeToken (source.Slice (token.Position, token.Length), destination.AsSpan (destinationPos));
		}

		private static Encoding GetEncoding (string name)
		{
			Encoding encoding;
			try
			{
				encoding = Encoding.GetEncoding (name);
			}
			catch (ArgumentException excpt)
			{
				throw new FormatException (
					FormattableString.Invariant ($"'{name}' is not valid code page name."),
					excpt);
			}

			return encoding;
		}
	}
}
