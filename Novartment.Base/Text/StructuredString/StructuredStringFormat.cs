using System;
using System.Collections.Generic;
using Novartment.Base.Collections;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Данные о структуре строки, позволяющие получить из неё отдельные лексические токены.
	/// </summary>
	public sealed class StructuredStringFormat
	{
		/// <summary>
		/// Формат токена-разделителя (любого символа, недопустимого для значений токена).
		/// </summary>
		public static readonly StructuredStringTokenFormat SeparatorFormat = new StructuredStringTokenSeparatorFormat ();

		/// <summary>
		/// Формат токена-значения (фрагмент, состоящий только из допустимых для значения символов).
		/// </summary>
		public static readonly StructuredStringTokenFormat ValueFormat = new StructuredStringTokenValueFormat ();

		private readonly StructuredStringTokenCustomFormat[] _customTokenFormats;

		/// <summary>
		/// Получает класс символов, которые игнорируются между токенами.
		/// </summary>
		public AsciiCharClasses WhiteSpaceClasses { get; }

		/// <summary>
		/// Получает класс символов, допустимых для токенов-значений.
		/// </summary>
		public AsciiCharClasses ValueClasses { get; }

		/// <summary>
		/// Получает символ, который допустим только между отдельными частями внутри одного токена-значения,
		/// но не допустим в начале, в конце или несколько подряд.
		/// </summary>
		public char TokenValuePartsLinkChar { get; }


		/// <param name="whiteSpaceClasses">Класс символов, которые игнорируются между токенами.</param>
		/// <param name="valueClasses">Класс символов, допустимых для токенов-значений.</param>
		/// <param name="tokenValuePartsLinkChar">
		/// Символ, который допустим только между отдельными частями внутри одного токена-значения,
		/// но не допустим в начале, в конце или несколько подряд.
		/// Укажите char.MaxValue если не используется.
		/// </param>
		/// <param name="customTokenFormats">Дополнительные форматы для распознавания. Укажите null-ссылку если не нужны.</param>
		public StructuredStringFormat (
			AsciiCharClasses whiteSpaceClasses,
			AsciiCharClasses valueClasses,
			char tokenValuePartsLinkChar = char.MaxValue,
			IReadOnlyCollection<StructuredStringTokenCustomFormat> customTokenFormats = null)
		{
			this.WhiteSpaceClasses = whiteSpaceClasses;
			this.ValueClasses = valueClasses;
			this.TokenValuePartsLinkChar = tokenValuePartsLinkChar;
			_customTokenFormats = customTokenFormats?.DuplicateToArray ();
		}

		/// <summary>
		/// Получает следующий лексический токен начиная с указанной позиции в указанной строке.
		/// Встроено распознавание трёх форматов токенов:
		/// null - не удалось считать токен из-за окончания строки,
		/// StructuredStringTokenSeparatorFormat - любой символ, недопустимый для значений,
		/// StructuredStringTokenValueFormat - значение.
		/// Дополнительные форматы токенов для распознавания указаны при создании.
		/// </summary>
		/// <param name="source">Структурированная строка, состоящая из лексических токенов.</param>
		/// <param name="position">
		/// Позиция в source, начиная с которой будет получен токен.
		/// После получения токена, position будет указывать на позицию, следующую за найденным токеном.
		/// </param>
		/// <returns>Лексический токен из указанной позиции в source.</returns>
		public StructuredStringToken ParseToken (ReadOnlySpan<char> source, ref int position)
		{
			if ((position < 0) || (position > source.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (position));
			}

			var charClasses = AsciiCharSet.ValueClasses.Span;
			var whiteSpaceClasses = this.WhiteSpaceClasses;
			var valueClasses = this.ValueClasses;
			var linkChar = this.TokenValuePartsLinkChar;
			var customTokenFormats = _customTokenFormats;
			while (position < source.Length)
			{
				char octet;
				// пропускаем пробельные символы
				while (true)
				{
					octet = source[position];
					if ((octet >= charClasses.Length) || ((charClasses[octet] & whiteSpaceClasses) == 0))
					{
						break;
					}

					position++;
					if (position >= source.Length)
					{
						return default;
					}
				}

				// проверяем все пользовательские форматы
				if (customTokenFormats != null)
				{
					for (var i = 0; i < customTokenFormats.Length; i++)
					{
						var tokenFormat = customTokenFormats[i];
						if (tokenFormat.StartMarker == octet)
						{
							var len = tokenFormat.FindTokenLength (source[position..]);
							var token = new StructuredStringToken (customTokenFormats[i], position, len);
							position += len;
							return token;
						}
					}
				}

				// захватываем все символы, допустимые для значения
				var valuePos = position;
				while (position < source.Length)
				{
					octet = source[position];
					if ((octet >= charClasses.Length) || ((charClasses[octet] & valueClasses) == 0))
					{
						break;
					}
					position++;
				}

				if (position <= valuePos)
				{
					// допустимые для значения символы не встретились, значит это разделитель
					position++;
					return new StructuredStringToken (SeparatorFormat, valuePos, 1);
				}
				else
				{
					// continue if dot followed by atom
					while (((position + 1) < source.Length) &&
						(source[position] == linkChar) &&
						((source[position + 1] < charClasses.Length) && ((charClasses[source[position + 1]] & valueClasses) != 0)))
					{
						position++;
						while (position < source.Length)
						{
							octet = source[position];
							if ((octet >= charClasses.Length) || ((charClasses[octet] & valueClasses) == 0))
							{
								break;
							}

							position++;
						}
					}

					return new StructuredStringToken (ValueFormat, valuePos, position - valuePos);
				}
			}

			return default;
		}
	}
}
