namespace Novartment.Base.UI
{
	/// <summary>
	/// Source for drag-and-drop objects from, including to other applications.
	/// </summary>
	public interface IDragDropSource
	{
		/// <summary>
		/// Creates an object that will be dragged when dragging starts.
		/// </summary>
		/// <param name="positionX">The x-axis coordinate of the point where the drag started.</param>
		/// <param name="positionY">The y-axis coordinate of the point where the drag started.</param>
		/// <param name="dragControl">The control used to start dragging.</param>
		/// <returns>
		/// The data for the start of the drag-and-drop, which consists of an object to be dragged and a set of drag-and-drop effects allowed.
		/// Drag-and-drop will be canceled if there are no allowed effects (the value specified is Dragdropeffects.None).
		/// </returns>
		DragStartData DragStart (double positionX, double positionY, DragControl dragControl);

		/// <summary>
		/// Displays the drag-and-drop effect for the user.
		/// </summary>
		/// <param name="effects">The drag effect selected by the target.</param>
		/// <returns>Indicates whether default cursors are used.
		/// If True, the default cursors will be automatically set;
		/// otherwise the required cursors are set by the drag source.</returns>
		bool GiveFeedback (DragDropEffects effects);

		/// <summary>
		/// Determines the next action after changing the drag and drop parameters.
		/// </summary>
		/// <param name="escapePressed">Indicates whether the ESC key is pressed.</param>
		/// <param name="keyStates">The state of controls that affect drag-and-drop.</param>
		/// <returns>The required drag action.</returns>
		DragDropAction QueryContinueDrag (bool escapePressed, DragDropKeyStates keyStates);

		/// <summary>
		/// Handles the completion of the drag and drop.
		/// Typically, you only need an action for the DragDropEffect.Move effect.
		/// </summary>
		/// <param name="effects">The drag effect selected by the target.</param>
		void DragEnd (DragDropEffects effects);
	}
}
