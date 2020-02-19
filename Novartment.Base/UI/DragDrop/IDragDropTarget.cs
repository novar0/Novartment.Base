namespace Novartment.Base.UI
{
	/// <summary>
	/// A target that you can drag-and-drop objects to, including from other applications.
	/// </summary>
	public interface IDragDropTarget
	{
		/// <summary>
		/// Handles entering of the dragged object in the target area.
		/// </summary>
		/// <param name="data">Еру object that provides drag-and-drop data.</param>
		/// <param name="keyStates">The state of controls that affect drag-and-drop.</param>
		/// <param name="positionX">The x-axis coordinate of the point where the object to be dragged is located.</param>
		/// <param name="positionY">The y-axis coordinate of the point where the object to be dragged is located.</param>
		/// <param name="allowedEffects">The drag-and-drop effects allowed by the source.</param>
		/// <returns>The drag effect selected by the target from the effects suggested by the source.</returns>
		DragDropEffects DragEnter (IDataContainer data, DragDropKeyStates keyStates, double positionX, double positionY, DragDropEffects allowedEffects);

		/// <summary>
		/// Handles when the dragged object leaves the target area.
		/// </summary>
		void DragLeave ();

		/// <summary>
		/// Handles changing the position or keyboard modifiers when dragging the object over the target area.
		/// </summary>
		/// <param name="keyStates">The state of controls that affect drag-and-drop.</param>
		/// <param name="positionX">The x-axis coordinate of the point where the object to be dragged is located.</param>
		/// <param name="positionY">The y-axis coordinate of the point where the object to be dragged is located.</param>
		/// <param name="allowedEffects">The drag-and-drop effects allowed by the source.</param>
		/// <returns>The drag effect selected by the target from the effects suggested by the source.</returns>
		DragDropEffects DragOver (DragDropKeyStates keyStates, double positionX, double positionY, DragDropEffects allowedEffects);

		/// <summary>
		/// Handles releasing the dragged object on the target.
		/// </summary>
		/// <param name="data">Еру object that provides drag-and-drop data.</param>
		/// <param name="keyStates">The state of controls that affect drag-and-drop.</param>
		/// <param name="positionX">The x-axis coordinate of the point where the object to be dragged is located.</param>
		/// <param name="positionY">The y-axis coordinate of the point where the object to be dragged is located.</param>
		/// <param name="allowedEffects">The drag-and-drop effects allowed by the source.</param>
		/// <returns>The drag effect selected by the target from the effects suggested by the source.</returns>
		DragDropEffects Drop (IDataContainer data, DragDropKeyStates keyStates, double positionX, double positionY, DragDropEffects allowedEffects);
	}
}
