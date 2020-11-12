using System.ComponentModel;

namespace Novartment.Base.Tasks
{
	/// <summary>
	/// Provides data for a task starting event.
	/// </summary>
	public sealed class TaskStartingEventArgs : CancelEventArgs
	{
		/// <summary>
		///  Initializes a new instance of the TaskStartingEventArgs class
		///  with the specified state object.
		/// </summary>
		/// <param name="state">The state object of the task.</param>
		public TaskStartingEventArgs (object state)
		{
			this.State = state;
		}

		/// <summary>
		/// Gets or sets the state object of the task.
		/// </summary>
		public object State { get; set; }
	}
}
