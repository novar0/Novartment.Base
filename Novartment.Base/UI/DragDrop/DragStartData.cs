namespace Novartment.Base.UI
{
#pragma warning disable CA1815 // Override equals and operator equals on value types
	/// <summary>
	/// The data for the start of the drag-and-drop.
	/// </summary>
	public readonly struct DragStartData
#pragma warning restore CA1815 // Override equals and operator equals on value types
	{
		/// <summary>
		///  Initializes a new instance of the DragStartData class
		///  with the specified drag-and-drop object and the set of effects allowed.
		/// </summary>
		/// <param name="dragObject">The object to be dragged.</param>
		/// <param name="allowedEffects">The set of drag-and-drop effects allowed.</param>
		public DragStartData (IDataContainer dragObject, DragDropEffects allowedEffects)
		{
			this.DragObject = dragObject;
			this.AllowedEffects = allowedEffects;
		}

		/// <summary>
		/// Gets the object to be dragged.
		/// </summary>
		public readonly IDataContainer DragObject { get; }

		/// <summary>
		/// Gets the set of drag-and-drop effects allowed.
		/// </summary>
		public readonly DragDropEffects AllowedEffects { get; }
	}
}
