using System;

namespace Novartment.Base.UI
{
	/// <summary>
	/// Состояние элементов управления, влияющих на перетаскивание.
	/// </summary>
	/// <remarks>
	/// Значение передаётся в параметрах при вызовах COM-интерфейсов IDropTarget и IDropSource.
	/// Создано чтобы не привязываться к конкретным технологиям,
	/// в которых есть аналоги (System.Windows.DragDropKeyStates).
	/// </remarks>
	[Flags]
	public enum DragDropKeyStates : int
	{
		/// <summary>Не активирован ни один элемент управления.</summary>
		None = 0,

		/// <summary>Активирована левая кнопка мыши.</summary>
		/// <remarks>Константа MK_LBUTTON.</remarks>
		LeftMouseButton = 1,

		/// <summary>Активирована правая кнопка мыши.</summary>
		/// <remarks>Константа MK_RBUTTON.</remarks>
		RightMouseButton = 2,

		/// <summary>Активирована клавиша SHIFT.</summary>
		/// <remarks>Константа MK_SHIFT.</remarks>
		ShiftKey = 4,

		/// <summary>Активирована клавиша CTRL.</summary>
		/// <remarks>Константа MK_CONTROL.</remarks>
		ControlKey = 8,

		/// <summary>Активирована средняя кнопка мыши.</summary>
		/// <remarks>Константа MK_MBUTTON.</remarks>
		MiddleMouseButton = 16,

		/// <summary>Активирована клавиша ALT.</summary>
		/// <remarks>Константа MK_ALT.</remarks>
		AltKey = 32
	}
}
