namespace Novartment.Base.UI
{
#pragma warning disable CA1815 // Override equals and operator equals on value types
	/// <summary>
	/// Данные для старта перетаскивания.
	/// </summary>
	public readonly struct DragStartData
#pragma warning restore CA1815 // Override equals and operator equals on value types
	{
		/// <summary>
		/// Инициализирует новый экземпляр DragStartData для указанного объекта с указанным набором разрешённых для перетаскивания эффектов.
		/// </summary>
		/// <param name="dragObject">Объект, который будет перетаскиваться.</param>
		/// <param name="allowedEffects">Набор разрешённых для перетаскивания эффектов.</param>
		public DragStartData (IDataContainer dragObject, DragDropEffects allowedEffects)
		{
			this.DragObject = dragObject;
			this.AllowedEffects = allowedEffects;
		}

		/// <summary>
		/// Объект, который будет перетаскиваться.
		/// </summary>
		public readonly IDataContainer DragObject { get; }

		/// <summary>
		/// Набор разрешённых для перетаскивания эффектов.
		/// </summary>
		public readonly DragDropEffects AllowedEffects { get; }
	}
}
