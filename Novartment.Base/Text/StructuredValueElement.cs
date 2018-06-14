using System;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Элемент структурированного значения.
	/// </summary>
	public struct StructuredValueElement
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса StructuredValueElement на основе указанного типа и кодированного значения.
		/// </summary>
		/// <param name="type">Тип тип, определяющий способ кодирования элемента.</param>
		/// <param name="startPosition">Позиция в source.</param>
		/// <param name="length">Количество элементов в source.</param>
		public StructuredValueElement (StructuredValueElementType type, int startPosition, int length)
		{
			if (type == StructuredValueElementType.Unspecified)
			{
				throw new ArgumentOutOfRangeException (nameof (type));
			}

			Contract.EndContractBlock ();

			this.ElementType = type;
			this.StartPosition = startPosition;
			this.Length = length;
		}

		/// <summary>
		/// Получает тип, определяющий способ кодирования элемента.
		/// </summary>
		public StructuredValueElementType ElementType { get; }

		/// <summary>
		/// Получает начальную позицию элемента.
		/// </summary>
		public int StartPosition { get; }

		/// <summary>
		/// Получает количество байт элемента.
		/// </summary>
		public int Length { get; }
	}
}
