using System;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Элемент структурированного значения.
	/// </summary>
	public class StructuredValueElement
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса StructuredValueElement представляющий указанный знак-разделитель.
		/// </summary>
		/// <param name="separator">Значение создаваемого элемента.</param>
		public StructuredValueElement (byte separator)
			: this (StructuredValueElementType.Separator, new byte[1] { separator })
		{
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса StructuredValueElement на основе указанного типа и кодированного значения.
		/// </summary>
		/// <param name="type">Тип тип, определяющий способ кодирования элемента.</param>
		/// <param name="value">Кодированное в соответствии с типом значение элемента.</param>
		public StructuredValueElement (StructuredValueElementType type, ReadOnlySpan<byte> value)
		{
			if (type == StructuredValueElementType.Unspecified)
			{
				throw new ArgumentOutOfRangeException (nameof (type));
			}

			if (value.Length < 1)
			{
				throw new ArgumentOutOfRangeException (nameof (value));
			}

			Contract.EndContractBlock ();

			switch (type)
			{
				case StructuredValueElementType.SquareBracketedValue:
				case StructuredValueElementType.QuotedValue:
					this.IsWordEncoded = Rfc2047EncodedWord.IsValid (UnquoteString (value));
					break;
				case StructuredValueElementType.Value:
					this.IsWordEncoded = (value.Length > 8) &&
						(value[0] == '=') &&
						(value[1] == '?') &&
						(value[value.Length - 2] == '?') &&
						(value[value.Length - 1] == '=');
					break;
				case StructuredValueElementType.Separator:
				default:
					this.IsWordEncoded = false;
					break;
			}

			this.ElementType = type;
			var buf = new byte[value.Length];
			value.CopyTo (buf);
			this.Value = buf;
		}

		/// <summary>
		/// Получает тип, определяющий способ кодирования элемента.
		/// </summary>
		public StructuredValueElementType ElementType { get; }

		/// <summary>
		/// Получает кодированное в соответствии с типом значение элемента.
		/// </summary>
		public ReadOnlyMemory<byte> Value { get; }

		/// <summary>
		/// Получает признак того, что исходная строка храниться в виде "word-encoded".
		/// </summary>
		public bool IsWordEncoded { get; }

		/// <summary>
		/// Декодирует значение элемента в соответствии с его типом.
		/// </summary>
		/// <returns>Декодировенное значение элемента.</returns>
		public string Decode ()
		{
			switch (this.ElementType)
			{
				case StructuredValueElementType.SquareBracketedValue:
				case StructuredValueElementType.QuotedValue:
					var value = UnquoteString (this.Value.Span);
					return this.IsWordEncoded ? Rfc2047EncodedWord.Parse (value) : value;
				case StructuredValueElementType.Value:
					return this.IsWordEncoded ?
						Rfc2047EncodedWord.Parse (AsciiCharSet.GetString (this.Value.Span)) :
						AsciiCharSet.GetString (this.Value.Span);
				case StructuredValueElementType.Separator:
					return AsciiCharSet.GetString (this.Value.Span);
				default:
					throw new InvalidOperationException (FormattableString.Invariant (
						$"Element of type '{this.ElementType}' is complex and can not be decoded to discrete value."));
			}
		}

		/// <summary>
		/// Проверяется, является ли элемент разделителем, совпадающим с указанным символом.
		/// </summary>
		/// <param name="separator">Символ-разделитель, с которым сравнивается элемент.</param>
		/// <returns>True если элемент является разделителем, совпадающим с указанным символом.</returns>
		public bool EqualsSeparator (byte separator)
		{
			return (this.ElementType == StructuredValueElementType.Separator) &&
				(this.Value.Length == 1) &&
				(this.Value.Span[0] == separator);
		}

		/// <summary>
		/// Преобразовывает значение объекта в эквивалентное ему строковое представление.
		/// </summary>
		/// <returns>Строковое представление значения объекта.</returns>
		public override string ToString ()
		{
			return $"<{this.ElementType}> {this.Value}";
		}

		private static string UnquoteString (ReadOnlySpan<byte> value)
		{
			int idx = 0;
			var result = new char[value.Length];
			for (var i = 0; i < value.Length; i++)
			{
				if (value[i] == '\\')
				{
					i++;
				}

				result[idx++] = (char)value[i];
			}

			return new string (result, 0, idx);
		}
	}
}
