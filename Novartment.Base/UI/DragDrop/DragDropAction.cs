namespace Novartment.Base.UI
{
	/// <summary>
	/// Требуемое действие во время перетаскивания.
	/// </summary>
	/// <remarks>
	/// Значение возвращается при вызове COM-интерфейса IDropSource::QueryContinueDrag().
	/// Создано чтобы не привязываться к конкретным технологиям,
	/// в которых есть аналоги (System.Windows.DragAction и System.Windows.Forms.DragAction).
	/// </remarks>
	public enum DragDropAction : int
	{
		/// <summary>Операция перетаскивания должна продолжиться.</summary>
		/// <remarks>Константа S_OK.</remarks>
		Continue = 0,

		/// <summary>Должно произойти отпускание перетаскиваемого объекта, успешно завершающее операцию перетаскивания.</summary>
		/// <remarks>Константа DRAGDROP_S_DROP.</remarks>
		Drop = 262400,

		/// <summary>Операция перетаскивания должна быть отменена без отпускания перетаскиваемого объекта.</summary>
		/// <remarks>Константа DRAGDROP_S_CANCEL.</remarks>
		Cancel = 262401,
	}
}
