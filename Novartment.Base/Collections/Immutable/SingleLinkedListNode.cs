namespace Novartment.Base.Collections.Immutable
{
	/// <summary>
	/// A single-linked list node.
	/// </summary>
	/// <typeparam name="TItem">The type of the value of the node.</typeparam>
	/// <remarks>A null-value is correct and means an empty list.</remarks>
	public class SingleLinkedListNode<TItem> :
		IValueHolder<TItem>
	{
		/// <summary>
		/// Initializes a new instance of the SingleLinkedListNode class that contains specified value.
		/// </summary>
		/// <param name="value">The value of the node.</param>
		public SingleLinkedListNode (TItem value)
		{
			this.Value = value;
		}

		internal SingleLinkedListNode (TItem value, SingleLinkedListNode<TItem> next)
		{
			this.Value = value;
			this.Next = next;
		}

		/// <summary>
		/// Gets the node value.
		/// </summary>
		public TItem Value { get; }

		/// <summary>
		/// Gets the next node of list.
		/// </summary>
		public SingleLinkedListNode<TItem> Next { get; } = null;
	}
}
