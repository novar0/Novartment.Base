using System;

namespace Novartment.Base.UI
{
	/// <summary>
	/// Эффект перетаскивания объекта,
	/// действие которое должно быть произведено при успешном завершении перетаскивания.
	/// </summary>
	/// <remarks>
	/// Соответствует Win32-константам DROPEFFECT_*.
	/// Значение передаётся в параметрах при вызовах COM-интерфейсов IDropTarget и IDropSource.
	/// Создано чтобы не привязываться к конкретным технологиям,
	/// в которых есть аналоги (System.Windows.DragDropEffects и System.Windows.Forms.DragDropEffects).
	/// </remarks>
	[Flags]
	public enum DragDropEffects : int
	{
		/// <summary>Нет эффекта.</summary>
		/// <remarks>Константа DROPEFFECT_NONE.</remarks>
		None = 0,

		/// <summary>Объект копируется.</summary>
		/// <remarks>Константа DROPEFFECT_COPY.</remarks>
		Copy = 1,

		/// <summary>Объект переносится.</summary>
		/// <remarks>Константа DROPEFFECT_MOVE.</remarks>
		Move = 2,

		/// <summary>Создаётся ссылка на объект.</summary>
		/// <remarks>Константа DROPEFFECT_LINK.</remarks>
		Link = 4,

		/// <summary>Объект прокручивается.</summary>
		/// <remarks>Константа DROPEFFECT_SCROLL.</remarks>
		Scroll = -2147483648, // 0x80000000,

		/// <summary>Все возможные действия.</summary>
		All = -2147483641, // 0x80000007
	}
}
