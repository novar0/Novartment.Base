using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Text
{
	/// <summary>
	/// An individual lexical token as part of RFC 5322 'Structured Header Field Body'.
	/// Contains the type, position, and number of characters of an individual token.
	/// </summary>
	[DebuggerDisplay ("{TokenType}: {Position}...{Position+Length}")]
	public readonly ref struct StructuredStringToken
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса StructuredHeaderFieldLexicalToken с указанным типом, позицией и количеством знаков.
		/// </summary>
		/// <param name="type">Тип токена.</param>
		/// <param name="position">Позиция токена.</param>
		/// <param name="length">Количество знаков токена.</param>
		public StructuredStringToken (StructuredStringTokenType type, int position, int length)
		{
			if (position < 0)
			{
				throw new ArgumentOutOfRangeException (nameof (position));
			}

			if ((length < 0) || ((type == StructuredStringTokenType.Separator) && (length != 1)))
			{
				throw new ArgumentOutOfRangeException (nameof (length));
			}

			Contract.EndContractBlock ();

			this.TokenType = type;
			this.Position = position;
			this.Length = length;
		}

		/// <summary>
		/// Получает тип токена.
		/// </summary>
		public readonly StructuredStringTokenType TokenType { get; }

		/// <summary>
		/// Получает начальную позицию токена.
		/// </summary>
		public readonly int Position { get; }

		/// <summary>
		/// Получает количество знаков токена.
		/// </summary>
		public readonly int Length { get; }

		/// <summary>
		/// Получает признак валидности токена.
		/// </summary>
		public readonly bool IsValid => this.TokenType != StructuredStringTokenType.Unspecified;

		/// <summary>
		/// Проверяет, является ли токен "encoded-word" согласно RFC 2047.
		/// </summary>
		/// <param name="source">Строка типа RFC 5322 'Structured Header Field Body', в которой выделен токен.</param>
		/// <returns>Признак того, что токен является "encoded-word" согласно RFC 2047.</returns>
		public bool IsWordEncoded (ReadOnlySpan<char> source)
		{
			var pos = this.Position;
			var len = this.Length;
			return (len > 8) &&
				(source[pos] == '=') &&
				(source[pos + 1] == '?') &&
				(source[pos + len - 2] == '?') &&
				(source[pos + len - 1] == '=');
		}

		public bool IsSingleQuotedValue (ReadOnlySpan<char> source)
		{
			return
				(this.TokenType == StructuredStringTokenType.DelimitedValue) &&
				(source[this.Position] == '\'') &&
				(source[this.Position + this.Length - 1] == '\'');
		}

		public bool IsDoubleQuotedValue (ReadOnlySpan<char> source)
		{
			return
				(this.TokenType == StructuredStringTokenType.DelimitedValue) &&
				(source[this.Position] == '\"') &&
				(source[this.Position + this.Length - 1] == '\"');
		}

		public bool IsRoundBracketedValue (ReadOnlySpan<char> source)
		{
			return
				(this.TokenType == StructuredStringTokenType.DelimitedValue) &&
				(source[this.Position] == '(') &&
				(source[this.Position + this.Length - 1] == ')');
		}

		public bool IsAngleBracketedValue (ReadOnlySpan<char> source)
		{
			return
				(this.TokenType == StructuredStringTokenType.DelimitedValue) &&
				(source[this.Position] == '<') &&
				(source[this.Position + this.Length - 1] == '>');
		}

		public bool IsSquareBracketedValue (ReadOnlySpan<char> source)
		{
			return
				(this.TokenType == StructuredStringTokenType.DelimitedValue) &&
				(source[this.Position] == '[') &&
				(source[this.Position + this.Length - 1] == ']');
		}

		public bool IsCurlyBracketedValue (ReadOnlySpan<char> source)
		{
			return
				(this.TokenType == StructuredStringTokenType.DelimitedValue) &&
				(source[this.Position] == '{') &&
				(source[this.Position + this.Length - 1] == '}');
		}

		public bool IsSeparator (ReadOnlySpan<char> source, char value)
		{
			return
				(this.TokenType == StructuredStringTokenType.Separator) &&
				(source[this.Position] == value);
		}
	}
}
