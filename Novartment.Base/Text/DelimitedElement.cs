using System;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Набор параметров, определяющих способ выделения элемента текста.
	/// </summary>
	/// <remarks>
	/// Все члены используют для символов кодировку UTF-32.
	/// Суррогатная пара (char, char) считается одним символом.
	/// </remarks>
	public class DelimitedElement
	{
		private DelimitedElement (int startUtf32Char, int endUtf32Char, int fixedLength, DelimitedElement ignoreElement, bool allowNesting)
		{
			this.StartChar = startUtf32Char;
			this.EndChar = endUtf32Char;
			this.FixedLength = fixedLength;
			this.IgnoreElement = ignoreElement;
			this.AllowNesting = allowNesting;
		}

		/// <summary>
		/// Получат элемент, представляющий собой один символ с предшествующим знаком обратной косой черты.
		/// </summary>
		public static DelimitedElement OneEscapedChar { get; } = CreatePrefixedFixedLength ('\\', 2);

		/// <summary>Начальный код символа (в UTF-32) элемента.</summary>
		public int StartChar { get; }

		/// <summary>Конечный код символа (в UTF-32) элемента.</summary>
		public int EndChar { get; }

		/// <summary>Разрешены ли вложенные элементы.</summary>
		public bool AllowNesting { get; }

		/// <summary>Фиксированный размер элемента, либо 0 если размер не фиксирован.</summary>
		public int FixedLength { get; }

		/// <summary>Элемент, который пропускается при поиске границ.</summary>
		public DelimitedElement IgnoreElement { get; }

		/// <summary>
		/// Создаёт экземпляр класса DelimitedElement на основе указанного начального символа и фиксированной длины.
		/// </summary>
		/// <param name="startUtf32Char">Начальный код символа (в UTF-32) элемента.</param>
		/// <param name="fixedLength">Фиксированный размер элемента, либо 0 если размер не фиксирован.</param>
		/// <returns>Созданный экземпляр класса DelimitedElement на основе указанного начального символа и фиксированной длины.</returns>
		public static DelimitedElement CreatePrefixedFixedLength (int startUtf32Char, int fixedLength)
		{
			if (fixedLength < 0)
			{
				throw new ArgumentOutOfRangeException (nameof (fixedLength));
			}

			Contract.EndContractBlock ();

			return new DelimitedElement (startUtf32Char, char.MinValue, fixedLength, null, false);
		}

		/// <summary>
		/// Создаёт экземпляр класса DelimitedElement на основе указанного начального и конечного символов
		/// и возможной вложенности.
		/// </summary>
		/// <param name="startUtf32Char">Начальный код символа (в UTF-32) элемента.</param>
		/// <param name="endUtf32Char">Конечный код символа (в UTF-32) элемента.</param>
		/// <param name="allowNesting">Разрешены ли вложенные элементы.</param>
		/// <returns>Созданный экземпляр класса DelimitedElement на основе указанного начального и конечного символов.</returns>
		public static DelimitedElement CreateBracketed (int startUtf32Char, int endUtf32Char, bool allowNesting)
		{
			return new DelimitedElement (startUtf32Char, endUtf32Char, 0, null, allowNesting);
		}

		/// <summary>
		/// Создаёт экземпляр класса DelimitedElement на основе указанного начального и конечного символов,
		/// игнорируемого элемента и возможной вложенности.
		/// </summary>
		/// <param name="startUtf32Char">Начальный код символа (в UTF-32) элемента.</param>
		/// <param name="endUtf32Char">Конечный код символа (в UTF-32) элемента.</param>
		/// <param name="ignoreElement">Элемент, который пропускается при поиске границ.</param>
		/// <param name="allowNesting">Разрешены ли вложенные элементы.</param>
		/// <returns>Созданный экземпляр класса DelimitedElement на основе указанного начального и конечного символов,
		/// игнорируемого элемента и возможной вложенности.</returns>
		public static DelimitedElement CreateBracketed (int startUtf32Char, int endUtf32Char, DelimitedElement ignoreElement, bool allowNesting)
		{
			if (ignoreElement == null)
			{
				throw new ArgumentNullException (nameof (ignoreElement));
			}

			Contract.EndContractBlock ();

			return new DelimitedElement (startUtf32Char, endUtf32Char, 0, ignoreElement, allowNesting);
		}
	}
}
