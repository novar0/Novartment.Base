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
		/// Инициализирует новый экземпляр класса DelimitedElement на основе указанного начального символа и фиксированной длины.
		/// </summary>
		/// <param name="startUtf32Char">Начальный код символа (в UTF-32) элемента.</param>
		/// <param name="fixedLength">Фиксированный размер элемента, либо 0 если размер не фиксирован.</param>
		public DelimitedElement (int startUtf32Char, int fixedLength)
		{
			this.StartChar = startUtf32Char;
			this.EndChar = Char.MinValue;
			this.AllowNesting = false;
			this.FixedLength = fixedLength;
			this.IgnoreElement = null;
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса DelimitedElement на основе указанного начального и конечного символов
		/// и возможной вложенности.
		/// </summary>
		/// <param name="startUtf32Char">Начальный код символа (в UTF-32) элемента.</param>
		/// <param name="endUtf32Char">Конечный код символа (в UTF-32) элемента.</param>
		/// <param name="allowNesting">Разрешены ли вложенные элементы.</param>
		public DelimitedElement (int startUtf32Char, int endUtf32Char, bool allowNesting)
		{
			this.StartChar = startUtf32Char;
			this.EndChar = endUtf32Char;
			this.AllowNesting = allowNesting;
			this.FixedLength = 0;
			this.IgnoreElement = null;
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса DelimitedElement на основе указанного начального и конечного символов,
		/// игнорируемого элемента и возможной вложенности.
		/// </summary>
		/// <param name="startUtf32Char">Начальный код символа (в UTF-32) элемента.</param>
		/// <param name="endUtf32Char">Конечный код символа (в UTF-32) элемента.</param>
		/// <param name="ignoreElement">Элемент, который пропускается при поиске границ.</param>
		/// <param name="allowNesting">Разрешены ли вложенные элементы.</param>
		public DelimitedElement (int startUtf32Char, int endUtf32Char, DelimitedElement ignoreElement, bool allowNesting)
		{
			if (ignoreElement == null)
			{
				throw new ArgumentNullException (nameof (ignoreElement));
			}
			Contract.EndContractBlock ();

			this.StartChar = startUtf32Char;
			this.EndChar = endUtf32Char;
			this.AllowNesting = allowNesting;
			this.FixedLength = 0;
			this.IgnoreElement = ignoreElement;
		}
	}
}
