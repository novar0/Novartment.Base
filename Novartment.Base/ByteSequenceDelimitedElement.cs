using System;
using System.Diagnostics.Contracts;

namespace Novartment.Base
{
	/// <summary>
	/// Набор параметров, определяющих способ выделения элемента байтовой последовательности.
	/// </summary>
	public class ByteSequenceDelimitedElement
	{
		private ByteSequenceDelimitedElement (byte startMarker, byte endMarker, int fixedLength, ByteSequenceDelimitedElement ignoreElement, bool allowNesting)
		{
			this.StarMarker = startMarker;
			this.EndMarker = endMarker;
			this.FixedLength = fixedLength;
			this.IgnoreElement = ignoreElement;
			this.AllowNesting = allowNesting;
		}

		/// <summary>Начальный байт элемента.</summary>
		public byte StarMarker { get; }

		/// <summary>Конечный байт элемента.</summary>
		public byte EndMarker { get; }

		/// <summary>Разрешены ли вложенные элементы.</summary>
		public bool AllowNesting { get; }

		/// <summary>Фиксированный размер элемента, либо 0 если размер не фиксирован.</summary>
		public int FixedLength { get; }

		/// <summary>Элемент, который пропускается при поиске границ.</summary>
		public ByteSequenceDelimitedElement IgnoreElement { get; }

		/// <summary>
		/// Создаёт экземпляр класса DelimitedElement на основе указанного начального байта и фиксированной длины.
		/// </summary>
		/// <param name="startMarker">Начальный байт элемента.</param>
		/// <param name="fixedLength">Фиксированный размер элемента, либо 0 если размер не фиксирован.</param>
		/// <returns>Созданный экземпляр класса DelimitedElement на основе указанного начального байта и фиксированной длины.</returns>
		public static ByteSequenceDelimitedElement CreatePrefixedFixedLength (byte startMarker, int fixedLength)
		{
			if (fixedLength < 0)
			{
				throw new ArgumentOutOfRangeException (nameof (fixedLength));
			}

			Contract.EndContractBlock ();

			return new ByteSequenceDelimitedElement (startMarker, byte.MinValue, fixedLength, null, false);
		}

		/// <summary>
		/// Создаёт экземпляр класса DelimitedElement на основе указанного начального и конечного байтов
		/// и возможной вложенности.
		/// </summary>
		/// <param name="startMarker">Начальный байт элемента.</param>
		/// <param name="endMarker">Конечный байт элемента.</param>
		/// <param name="allowNesting">Разрешены ли вложенные элементы.</param>
		/// <returns>Созданный экземпляр класса DelimitedElement на основе указанного начального и конечного байтов.</returns>
		public static ByteSequenceDelimitedElement CreateMarkered (byte startMarker, byte endMarker, bool allowNesting)
		{
			return new ByteSequenceDelimitedElement (startMarker, endMarker, 0, null, allowNesting);
		}

		/// <summary>
		/// Создаёт экземпляр класса DelimitedElement на основе указанного начального и конечного байтов,
		/// игнорируемого элемента и возможной вложенности.
		/// </summary>
		/// <param name="startMarker">Начальный байт элемента.</param>
		/// <param name="endMarker">Конечный байт элемента.</param>
		/// <param name="ignoreElement">Элемент, который пропускается при поиске границ.</param>
		/// <param name="allowNesting">Разрешены ли вложенные элементы.</param>
		/// <returns>Созданный экземпляр класса DelimitedElement на основе указанного начального и конечного байтов,
		/// игнорируемого элемента и возможной вложенности.</returns>
		public static ByteSequenceDelimitedElement CreateMarkered (byte startMarker, byte endMarker, ByteSequenceDelimitedElement ignoreElement, bool allowNesting)
		{
			if (ignoreElement == null)
			{
				throw new ArgumentNullException (nameof (ignoreElement));
			}

			Contract.EndContractBlock ();

			return new ByteSequenceDelimitedElement (startMarker, endMarker, 0, ignoreElement, allowNesting);
		}
	}
}
