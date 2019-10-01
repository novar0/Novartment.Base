using System.Threading;
using Novartment.Base.Collections.Immutable;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Defines a class for managing <see cref="TargetsTrackingExtensionBase"/> objects.
	/// </summary>
	/// <remarks>
	/// This class provides a single point for updating all markup targets that use the given Markup Extension managed by this class.
	/// </remarks>
	public class MarkupExtensionManager
	{
		/// <summary>The interval at which to cleanup and remove extensions.</summary>
		private readonly int _cleanupInterval = 40;

		/// <summary>List of active extensions.</summary>
		private SingleLinkedListNode<TargetsTrackingExtensionBase> _extensions = null;

		/// <summary>The number of extensions added since the last cleanup.</summary>
		private int _cleanupCount;

		/// <summary>
		/// Initializes a new instance of the MarkupExtensionManager.
		/// </summary>
		/// <param name="cleanupInterval">
		/// The interval at which to cleanup and remove extensions associated with garbage collected targets.
		/// This specifies the number of new Markup Extensions that are created before a cleanup is triggered.
		/// </param>
		public MarkupExtensionManager (int cleanupInterval)
		{
			_cleanupInterval = cleanupInterval;
		}

		/// <summary>
		/// Return a list of the currently active extensions.
		/// </summary>
		public SingleLinkedListNode<TargetsTrackingExtensionBase> ActiveExtensions => _extensions;

		/// <summary>
		/// Force the update of all active targets that use the markup extension.
		/// </summary>
		public virtual void UpdateAllTargets ()
		{
			var node = _extensions;
			while (node != null)
			{
				node.Value.UpdateTargets ();
				node = node.Next;
			}
		}

		/// <summary>
		/// Cleanup references to extensions for targets which have been garbage collected.
		/// </summary>
		/// <remarks>
		/// This method is called periodically as new <see cref="TargetsTrackingExtensionBase"/> objects
		/// are registered to release <see cref="TargetsTrackingExtensionBase"/> objects which are no longer
		/// required (because their target has been garbage collected).
		/// This method does not need to be called externally, however it can be useful to call it prior to calling
		/// GC.Collect to verify that objects are being garbage collected correctly.
		/// </remarks>
		public void CleanupInactiveExtensions ()
		{
			SingleLinkedListNode<TargetsTrackingExtensionBase> newExtensions = null;
			var node = _extensions;
			while (node != null)
			{
				var ext = node.Value;
				if (ext.IsTargetAlive)
				{
					newExtensions = newExtensions.AddItem (ext);
				}

				node = node.Next;
			}

			_extensions = newExtensions;
		}

		/// <summary>
		/// Register a new extension and remove extensions which reference target objects
		/// that have been garbage collected.
		/// </summary>
		/// <param name="extension">The extension to register.</param>
		internal void RegisterExtension (TargetsTrackingExtensionBase extension)
		{
			// Cleanup extensions for target objects which have been garbage collected for performance only do this periodically
			if (_cleanupCount > _cleanupInterval)
			{
				CleanupInactiveExtensions ();
				_cleanupCount = 0;
			}

			SpinWait spinWait = default;
			while (true)
			{
				var state1 = _extensions;
				var newState = state1.AddItem (extension);

				// заменяем состояние если оно не изменилось с момента вызова
				var state2 = Interlocked.CompareExchange (ref _extensions, newState, state1);
				if (state1 == state2)
				{
					_cleanupCount++;
					return;
				}

				// состояние изменилось за время вызова, поэтому повторим попытку после паузы
				spinWait.SpinOnce ();
			}
		}
	}
}
