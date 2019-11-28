using System.Collections.Generic;
using System.Collections.Specialized;
using Novartment.Base.Collections;
using Novartment.Base.Collections.Immutable;
using Xunit;

namespace Novartment.Base.Test
{
	public class ConcurrentListTests
	{

#pragma warning disable xUnit2013 // Do not use equality check to check for collection size.

		[Fact]
		[Trait ("Category", "Collections.ConcurrentList")]
		public void AddTake ()
		{
			var events = new List<NotifyCollectionChangedEventArgs> ();
			int eventN = 0;
			var t1 = new int[] { 1, 2, 2, 3 };
			var c1 = new ConcurrentList<int> (t1);
			var c2 = new ConcurrentList<int> ();
			c2.CollectionChanged += (sender, args) => events.Add (args);

			// Add
			c2.Add (4);
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Add, events[eventN].Action);
			Assert.Equal (0, events[eventN].NewStartingIndex);
			Assert.Equal (1, events[eventN].NewItems.Count);
			Assert.Equal (4, events[eventN].NewItems[0]);
			Assert.Equal (-1, events[eventN].OldStartingIndex);
			Assert.Null (events[eventN].OldItems);

			// Add
			c2.Add (1);
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Add, events[eventN].Action);
			Assert.Equal (eventN, events[eventN].NewStartingIndex);
			Assert.Equal (1, events[eventN].NewItems.Count);
			Assert.Equal (1, events[eventN].NewItems[0]);
			Assert.Equal (-1, events[eventN].OldStartingIndex);
			Assert.Null (events[eventN].OldItems);

			// Add
			c2.Add (2);
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Add, events[eventN].Action);
			Assert.Equal (eventN, events[eventN].NewStartingIndex);
			Assert.Equal (1, events[eventN].NewItems.Count);
			Assert.Equal (2, events[eventN].NewItems[0]);
			Assert.Equal (-1, events[eventN].OldStartingIndex);
			Assert.Null (events[eventN].OldItems);

			// Add
			c2.Add (2);
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Add, events[eventN].Action);
			Assert.Equal (eventN, events[eventN].NewStartingIndex);
			Assert.Equal (1, events[eventN].NewItems.Count);
			Assert.Equal (2, events[eventN].NewItems[0]);
			Assert.Equal (-1, events[eventN].OldStartingIndex);
			Assert.Null (events[eventN].OldItems);

			// Add
			c2.Add (-5);
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Add, events[eventN].Action);
			Assert.Equal (eventN, events[eventN].NewStartingIndex);
			Assert.Equal (1, events[eventN].NewItems.Count);
			Assert.Equal (-5, events[eventN].NewItems[0]);
			Assert.Equal (-1, events[eventN].OldStartingIndex);
			Assert.Null (events[eventN].OldItems);
			Assert.Equal (5, c2.Count);
			Assert.Equal (-5, c2[4]);

			// set this[]
			c2[4] = 3;
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Replace, events[eventN].Action);
			Assert.Equal (4, events[eventN].NewStartingIndex);
			Assert.Equal (1, events[eventN].NewItems.Count);
			Assert.Equal (3, events[eventN].NewItems[0]);
			Assert.Equal (4, events[eventN].OldStartingIndex);
			Assert.Equal (1, events[eventN].OldItems.Count);
			Assert.Equal (-5, events[eventN].OldItems[0]);
			Assert.Equal (3, c2[4]);
			Assert.NotEqual<int> (c1, c2);

			// TryTakeFirst
			Assert.True (c2.TryTakeFirst (out int v1));
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Remove, events[eventN].Action);
			Assert.Equal (-1, events[eventN].NewStartingIndex);
			Assert.Null (events[eventN].NewItems);
			Assert.Equal (0, events[eventN].OldStartingIndex);
			Assert.Equal (1, events[eventN].OldItems.Count);
			Assert.Equal (4, events[eventN].OldItems[0]);
			Assert.Equal (4, c2.Count);
			Assert.Equal (4, v1);
			Assert.Equal<int> (c1, c2);
			int[] t2 = new int[c2.Count];
			c2.CopyTo (t2, 0);
			Assert.Equal<int> (t1, c2);

			// TryTakeLast
			Assert.True (c2.TryTakeLast (out v1));
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Remove, events[eventN].Action);
			Assert.Equal (-1, events[eventN].NewStartingIndex);
			Assert.Null (events[eventN].NewItems);
			Assert.Equal (3, events[eventN].OldStartingIndex);
			Assert.Equal (1, events[eventN].OldItems.Count);
			Assert.Equal (3, events[eventN].OldItems[0]);
			Assert.Equal (3, c2.Count);
			Assert.Equal (3, v1);

			// Clear
			c2.Clear ();
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Reset, events[eventN].Action);
			Assert.Equal (-1, events[eventN].NewStartingIndex);
			Assert.Equal (-1, events[eventN].OldStartingIndex);
			Assert.Null (events[eventN].NewItems);
			Assert.Null (events[eventN].OldItems);
			Assert.Equal (0, c2.Count);

			// TryPeekLast
			v1 = int.MaxValue;
			Assert.False (c2.TryPeekLast (out v1));
			Assert.Equal (0, v1);
			Assert.Equal (eventN + 1, events.Count);

			// TryTakeFirst
			v1 = int.MaxValue;
			Assert.False (c2.TryTakeFirst (out v1));
			Assert.Equal (0, v1);
			Assert.Equal (eventN + 1, events.Count);
		}

		[Fact]
		[Trait ("Category", "Collections.ConcurrentList")]
		public void InsertRemove ()
		{
			// создаём такое состояние массива чтобы он переходил через край
			var events = new List<NotifyCollectionChangedEventArgs> ();
			int eventN = 0;
			var c2 = new ConcurrentList<int> ();
			c2.CollectionChanged += (sender, args) => events.Add (args);

			// Insert
			c2.Insert (0, -1);
			var c1 = new ConcurrentList<int> (new int[] { -1 });
			Assert.Equal<int> (c1, c2);
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Add, events[eventN].Action);
			Assert.Equal (0, events[eventN].NewStartingIndex);
			Assert.Equal (1, events[eventN].NewItems.Count);
			Assert.Equal (-1, events[eventN].NewItems[0]);
			Assert.Equal (-1, events[eventN].OldStartingIndex);
			Assert.Null (events[eventN].OldItems);

			// Insert
			c2.Insert (0, -2);
			c1 = new ConcurrentList<int> (new int[] { -2, -1 });
			Assert.Equal<int> (c1, c2);
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Add, events[eventN].Action);
			Assert.Equal (0, events[eventN].NewStartingIndex);
			Assert.Equal (1, events[eventN].NewItems.Count);
			Assert.Equal (-2, events[eventN].NewItems[0]);
			Assert.Equal (-1, events[eventN].OldStartingIndex);
			Assert.Null (events[eventN].OldItems);

			// Insert
			c2.Insert (1, -3);
			c1 = new ConcurrentList<int> (new int[] { -2, -3, -1 });
			Assert.Equal<int> (c1, c2);
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Add, events[eventN].Action);
			Assert.Equal (1, events[eventN].NewStartingIndex);
			Assert.Equal (1, events[eventN].NewItems.Count);
			Assert.Equal (-3, events[eventN].NewItems[0]);
			Assert.Equal (-1, events[eventN].OldStartingIndex);
			Assert.Null (events[eventN].OldItems);

			// Insert
			c2.Insert (3, -4);
			c1 = new ConcurrentList<int> (new int[] { -2, -3, -1, -4 });
			Assert.Equal<int> (c1, c2);
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Add, events[eventN].Action);
			Assert.Equal (3, events[eventN].NewStartingIndex);
			Assert.Equal (1, events[eventN].NewItems.Count);
			Assert.Equal (-4, events[eventN].NewItems[0]);
			Assert.Equal (-1, events[eventN].OldStartingIndex);
			Assert.Null (events[eventN].OldItems);

			// RemoveAt
			c2.RemoveAt (0);
			c1 = new ConcurrentList<int> (new int[] { -3, -1, -4 });
			Assert.Equal<int> (c1, c2);
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Remove, events[eventN].Action);
			Assert.Equal (-1, events[eventN].NewStartingIndex);
			Assert.Null (events[eventN].NewItems);
			Assert.Equal (0, events[eventN].OldStartingIndex);
			Assert.Equal (1, events[eventN].OldItems.Count);
			Assert.Equal (-2, events[eventN].OldItems[0]);

			// RemoveAt
			c2.RemoveAt (1);
			c1 = new ConcurrentList<int> (new int[] { -3, -4 });
			Assert.Equal<int> (c1, c2);
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Remove, events[eventN].Action);
			Assert.Equal (-1, events[eventN].NewStartingIndex);
			Assert.Null (events[eventN].NewItems);
			Assert.Equal (1, events[eventN].OldStartingIndex);
			Assert.Equal (1, events[eventN].OldItems.Count);
			Assert.Equal (-1, events[eventN].OldItems[0]);

			// RemoveAt
			c2.RemoveAt (1);
			c1 = new ConcurrentList<int> (new int[] { -3 });
			Assert.Equal<int> (c1, c2);
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Remove, events[eventN].Action);
			Assert.Equal (-1, events[eventN].NewStartingIndex);
			Assert.Null (events[eventN].NewItems);
			Assert.Equal (1, events[eventN].OldStartingIndex);
			Assert.Equal (1, events[eventN].OldItems.Count);
			Assert.Equal (-4, events[eventN].OldItems[0]);

			// Insert
			c2.Insert (1, -5);
			c1 = new ConcurrentList<int> (new int[] { -3, -5 });
			Assert.Equal<int> (c1, c2);
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Add, events[eventN].Action);
			Assert.Equal (1, events[eventN].NewStartingIndex);
			Assert.Equal (1, events[eventN].NewItems.Count);
			Assert.Equal (-5, events[eventN].NewItems[0]);
			Assert.Equal (-1, events[eventN].OldStartingIndex);
			Assert.Null (events[eventN].OldItems);
		}

		[Fact]
		[Trait ("Category", "Collections.ConcurrentList")]
		public void InsertRemoveRange ()
		{
			var events = new List<NotifyCollectionChangedEventArgs> ();
			int eventN = 0;
			var c2 = new ConcurrentList<int> (new LoopedArraySegment<int> (new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, 7, 4));
			Assert.Equal<int> (new int[] { 8, 9, 1, 2 }, c2);
			c2.CollectionChanged += (sender, args) => events.Add (args);

			// InsertRange
			c2.InsertRange (0, 3);
			Assert.Equal<int> (new int[] { 0, 0, 0, 8, 9, 1, 2 }, c2);
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Add, events[eventN].Action);
			Assert.Equal (0, events[eventN].NewStartingIndex);
			Assert.Equal (3, events[eventN].NewItems.Count);
			Assert.Equal (0, events[eventN].NewItems[0]);
			Assert.Equal (0, events[eventN].NewItems[1]);
			Assert.Equal (0, events[eventN].NewItems[2]);
			Assert.Equal (-1, events[eventN].OldStartingIndex);
			Assert.Null (events[eventN].OldItems);

			// InsertRange
			c2.InsertRange (7, 1);
			Assert.Equal<int> (new int[] { 0, 0, 0, 8, 9, 1, 2, 0 }, c2);
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Add, events[eventN].Action);
			Assert.Equal (7, events[eventN].NewStartingIndex);
			Assert.Equal (1, events[eventN].NewItems.Count);
			Assert.Equal (0, events[eventN].NewItems[0]);
			Assert.Equal (-1, events[eventN].OldStartingIndex);
			Assert.Null (events[eventN].OldItems);

			// InsertRange
			c2.InsertRange (3, 12);
			Assert.Equal<int> (new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 9, 1, 2, 0 }, c2);
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Add, events[eventN].Action);
			Assert.Equal (3, events[eventN].NewStartingIndex);
			Assert.Equal (12, events[eventN].NewItems.Count);
			Assert.Equal (0, events[eventN].NewItems[0]);
			Assert.Equal (0, events[eventN].NewItems[11]);
			Assert.Equal (-1, events[eventN].OldStartingIndex);
			Assert.Null (events[eventN].OldItems);

			// RemoveRange
			c2.RemoveRange (3, 12);
			Assert.Equal<int> (new int[] { 0, 0, 0, 8, 9, 1, 2, 0 }, c2);
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Remove, events[eventN].Action);
			Assert.Equal (3, events[eventN].OldStartingIndex);
			Assert.Equal (12, events[eventN].OldItems.Count);
			Assert.Equal (0, events[eventN].OldItems[0]);
			Assert.Equal (0, events[eventN].OldItems[11]);
			Assert.Equal (-1, events[eventN].NewStartingIndex);
			Assert.Null (events[eventN].NewItems);

			// RemoveRange
			c2.RemoveRange (7, 1);
			Assert.Equal<int> (new int[] { 0, 0, 0, 8, 9, 1, 2 }, c2);
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Remove, events[eventN].Action);
			Assert.Equal (7, events[eventN].OldStartingIndex);
			Assert.Equal (1, events[eventN].OldItems.Count);
			Assert.Equal (0, events[eventN].OldItems[0]);
			Assert.Equal (-1, events[eventN].NewStartingIndex);
			Assert.Null (events[eventN].NewItems);

			// RemoveRange
			c2.RemoveRange (0, 7);
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Remove, events[eventN].Action);
			Assert.Equal (0, events[eventN].OldStartingIndex);
			Assert.Equal (7, events[eventN].OldItems.Count);
			Assert.Equal (0, events[eventN].OldItems[0]);
			Assert.Equal (2, events[eventN].OldItems[6]);
			Assert.Equal (-1, events[eventN].NewStartingIndex);
			Assert.Null (events[eventN].NewItems);
		}

#pragma warning restore xUnit2013 // Do not use equality check to check for collection size.

	}
}
