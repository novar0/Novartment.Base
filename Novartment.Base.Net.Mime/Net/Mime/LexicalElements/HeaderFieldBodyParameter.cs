using System;
using System.Diagnostics.Contracts;
using System.Text;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// The parameter of the field of the header of the generic message format defined in RFC 822.
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
			string parameterName = null;
			var outPos = 0;
			Encoding encoding = null;
			while (true)
			{
				// запоминам позицию на случай если считанная часть окажется уже для другого параметра (индикации последней части никакой нет)
				var lastParserPos = parserPos;
				StructuredStringToken token;
				do
				{
					token = StructuredStringToken.Parse (HeaderDecoder.TokenFormat, source, ref parserPos);
				} while (token.Format is TokenFormatComment);

				if (token.Format == null)
				{
					// строка кончилась, возвращаем накопленные части как параметр
					if (parameterName == null)
					{
						// ничего не накопилось, означает конец строки
						return null;
					}

					return new HeaderFieldBodyParameter (parameterName, new string (outBuf, 0, outPos));
				}

				var isSeparator = token.IsSeparator (source, ';');
				if (!isSeparator)
				{
					throw new FormatException ("Value does not conform to 'atom *(; parameter)' format.");
				}

				var part = HeaderFieldBodyParameterPart.Parse (source, ref parserPos);
				if (part.IsFirstSection)
				{
					// начался новый параметр, возвращаем предыдущий параметр если был
					if (parameterName != null)
					{
						// поймали первую секцию следующего параметра, поэтому откатываем позицию до его начала
						parserPos = lastParserPos;
						return new HeaderFieldBodyParameter (parameterName, new string (outBuf, 0, outPos));
					}

#if NETSTANDARD2_0
					parameterName = new string (source.Slice (part.Name.Position, part.Name.Length).ToArray ());
#else
					parameterName = new string (source.Slice (part.Name.Position, part.Name.Length));
#endif
					outPos = 0;
					try
					{
						encoding = (part.Encoding != null) ? Encoding.GetEncoding (part.Encoding) : Encoding.ASCII;
					}
					catch (ArgumentException excpt)
					{
						throw new FormatException (
							FormattableString.Invariant ($"'{part.Encoding}' is not valid code page name."),
							excpt);
					}
				}

				outPos += part.GetValue (source, encoding, outBuf, outPos);
			}
		}
	}
}
