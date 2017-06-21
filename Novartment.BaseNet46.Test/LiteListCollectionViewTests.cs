using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using Novartment.Base.UI.Wpf;
using Xunit;

namespace Novartment.Base.Net46.Test
{
	public class LiteListCollectionViewTests
	{
		private int _eventN;

		[Fact]
		[Trait ("Category", "LiteListCollectionView")]
		public void DeferRefresh ()
		{
			var events = new List<NotifyCollectionChangedEventArgs> ();
			int eventN = 0;

			var list = new ObservableCollection<Mock4> ();
			var item1 = new Mock4 { Number = 1, Prop1 = "aab", Prop2 = 91 };
			var item2 = new Mock4 { Number = 2, Prop1 = "ZZZ", Prop2 = 110 };
			var item3 = new Mock4 { Number = 3, Prop1 = "aaa", Prop2 = 444 };
			var item4 = new Mock4 { Number = 4, Prop1 = "шшш", Prop2 = 273 };
			list.Add (item1);

			var view = new LiteListCollectionView<Mock4> (list);
			if (view.NeedsRefresh)
			{
				view.Refresh ();
			}

			(view as ICollectionView).CollectionChanged += (sender, args) => events.Add (args);
			Assert.Equal (1, view.Count);
			Assert.Equal (item1, view.GetItemAt (0));
			Assert.False (view.NeedsRefresh);
			view.Refresh ();
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Reset, events[eventN].Action);
			Assert.False (view.NeedsRefresh);

			// add with refresh NOT deffered
			list.Add (item2);
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Add, events[eventN].Action);
			Assert.Equal (1, events[eventN].NewStartingIndex);
			Assert.Equal (1, events[eventN].NewItems.Count);
			Assert.Equal (item2, events[eventN].NewItems[0]);
			Assert.Equal (-1, events[eventN].OldStartingIndex);
			Assert.Null (events[eventN].OldItems);
			Assert.Equal (2, view.Count);
			Assert.Equal (item1, view.GetItemAt (0));
			Assert.Equal (item2, view.GetItemAt (1));

			// add with refresh deffered
			var disposer1 = view.DeferRefresh ();
			Assert.NotNull (disposer1);
			Assert.False (view.NeedsRefresh);
			view.Filter = item => (item as Mock4).Prop2 < 0;
			Assert.Equal (eventN + 1, events.Count); // no new events
			Assert.True (view.NeedsRefresh);

			var disposer2 = view.DeferRefresh (); // nested defer
			Assert.NotNull (disposer2);
			Assert.True (view.NeedsRefresh);
			view.Filter = item => (item as Mock4).Prop2 > 100;
			Assert.True (view.NeedsRefresh);
			Assert.Equal (eventN + 1, events.Count); // no new events
			disposer2.Dispose ();
			Assert.Equal (eventN + 1, events.Count); // no new events
			Assert.True (view.NeedsRefresh);

			disposer1.Dispose ();
			Assert.False (view.NeedsRefresh);
			eventN++;
			Assert.Equal (eventN + 1, events.Count); // one new event (reset)
			Assert.Equal (NotifyCollectionChangedAction.Reset, events[eventN].Action);
			Assert.Equal (-1, events[eventN].NewStartingIndex);
			Assert.Null (events[eventN].NewItems);
			Assert.Equal (-1, events[eventN].OldStartingIndex);
			Assert.Null (events[eventN].OldItems);
			Assert.Equal (1, view.Count);
			Assert.Equal (item2, view.GetItemAt (0));

			// defer without modification
			var disposer3 = view.DeferRefresh ();
			Assert.NotNull (disposer3);
			var result = list[0].Prop2 + list[1].Prop2 + list.Count;
			Assert.Equal (eventN + 1, events.Count);
			disposer3.Dispose ();
			Assert.Equal (eventN + 1, events.Count); // no new events

			// clear with refresh NOT deffered
			list.Clear ();
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Reset, events[eventN].Action);
			Assert.Equal (-1, events[eventN].NewStartingIndex);
			Assert.Null (events[eventN].NewItems);
			Assert.Equal (-1, events[eventN].OldStartingIndex);
			Assert.Null (events[eventN].OldItems);

			Assert.Equal (0, view.Count);
			Assert.False (view.NeedsRefresh);

			Thread.Sleep ((result / 100000) + 1); // используем result чтобы компилятор не удалил его вычисление как ненужное
		}

		[Fact]
		[Trait ("Category", "LiteListCollectionView")]
		public void FilteredOperations ()
		{
			var events = new List<NotifyCollectionChangedEventArgs> ();
			int eventN = 0;
			var list = new ObservableCollection<Mock4> ();
			var item1 = new Mock4 { Number = 1, Prop1 = "aab", Prop2 = 91 };
			var item2 = new Mock4 { Number = 2, Prop1 = "ZZZ", Prop2 = 110 };
			var item3 = new Mock4 { Number = 3, Prop1 = "aaa", Prop2 = 444 };
			var item4 = new Mock4 { Number = 4, Prop1 = "шшш", Prop2 = 273 };
			list.Add (item1);
			list.Add (item2);
			list.Add (item3);
			var view = new LiteListCollectionView<Mock4> (list);
			if (view.NeedsRefresh)
			{
				view.Refresh ();
			}

			(view as ICollectionView).CollectionChanged += (sender, args) => events.Add (args);

			Assert.Equal (3, view.Count);
			Assert.Equal (item2, view.GetItemAt (1));
			Assert.Equal (item3, view.GetItemAt (2));
			Assert.Equal (item1, view.GetItemAt (0));
			Assert.False (view.Contains (item4));
			Assert.True (view.Contains (item2));
			Assert.True (view.Contains (item1));
			Assert.True (view.Contains (item3));
			Assert.Equal (2, view.IndexOf (item3));
			Assert.Equal (0, view.IndexOf (item1));
			Assert.Equal (1, view.IndexOf (item2));
			Assert.Equal (-1, view.IndexOf (item4));

			// set filter
			view.Filter = item => (item as Mock4).Prop2 > 100;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Reset, events[eventN].Action);
			Assert.Equal (-1, events[eventN].NewStartingIndex);
			Assert.Equal (-1, events[eventN].OldStartingIndex);
			Assert.Null (events[eventN].NewItems);
			Assert.Null (events[eventN].OldItems);
			Assert.Equal (2, view.Count);
			Assert.Equal (item2, view.GetItemAt (0));
			Assert.Equal (item3, view.GetItemAt (1));
			Assert.False (view.Contains (item1));
			Assert.True (view.Contains (item2));
			Assert.True (view.Contains (item3));
			Assert.False (view.Contains (item4));
			Assert.Equal (-1, view.IndexOf (item1));
			Assert.Equal (0, view.IndexOf (item2));
			Assert.Equal (1, view.IndexOf (item3));
			Assert.Equal (-1, view.IndexOf (item4));

			// change filter
			view.Filter = item => (item as Mock4).Prop1 != "ZZZ";
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Reset, events[eventN].Action);
			Assert.Equal (-1, events[eventN].NewStartingIndex);
			Assert.Equal (-1, events[eventN].OldStartingIndex);
			Assert.Null (events[eventN].NewItems);
			Assert.Null (events[eventN].OldItems);
			Assert.Equal (2, view.Count);
			Assert.Equal (item1, view.GetItemAt (0));
			Assert.Equal (item3, view.GetItemAt (1));
			Assert.True (view.Contains (item1));
			Assert.False (view.Contains (item2));
			Assert.True (view.Contains (item3));
			Assert.False (view.Contains (item4));
			Assert.Equal (0, view.IndexOf (item1));
			Assert.Equal (-1, view.IndexOf (item2));
			Assert.Equal (1, view.IndexOf (item3));
			Assert.Equal (-1, view.IndexOf (item4));

			// SourceCollection.Add passes filter
			list.Add (item4);
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Add, events[eventN].Action);
			Assert.Equal (2, events[eventN].NewStartingIndex);
			Assert.Equal (1, events[eventN].NewItems.Count);
			Assert.Equal (item4, events[eventN].NewItems[0]);
			Assert.Equal (-1, events[eventN].OldStartingIndex);
			Assert.Null (events[eventN].OldItems);
			Assert.Equal (3, view.Count);
			Assert.Equal (item1, view.GetItemAt (0));
			Assert.Equal (item3, view.GetItemAt (1));
			Assert.Equal (item4, view.GetItemAt (2));
			Assert.True (view.Contains (item1));
			Assert.False (view.Contains (item2));
			Assert.True (view.Contains (item3));
			Assert.True (view.Contains (item4));
			Assert.Equal (0, view.IndexOf (item1));
			Assert.Equal (-1, view.IndexOf (item2));
			Assert.Equal (1, view.IndexOf (item3));
			Assert.Equal (2, view.IndexOf (item4));

			// SourceCollection.Replace from pass to pass
			list[2] = item4;
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Replace, events[eventN].Action);
			Assert.Equal (1, events[eventN].NewStartingIndex);
			Assert.Equal (1, events[eventN].NewItems.Count);
			Assert.Equal (item4, events[eventN].NewItems[0]);
			Assert.Equal (1, events[eventN].OldStartingIndex);
			Assert.Equal (1, events[eventN].OldItems.Count);
			Assert.Equal (item3, events[eventN].OldItems[0]);
			Assert.Equal (item1, view.GetItemAt (0));
			Assert.Equal (item4, view.GetItemAt (1));
			Assert.Equal (item4, view.GetItemAt (2));
			Assert.True (view.Contains (item1));
			Assert.False (view.Contains (item2));
			Assert.False (view.Contains (item3));
			Assert.True (view.Contains (item4));
			Assert.Equal (0, view.IndexOf (item1));
			Assert.Equal (-1, view.IndexOf (item2));
			Assert.Equal (-1, view.IndexOf (item3));
			Assert.Equal (1, view.IndexOf (item4));

			// SourceCollection.Replace from pass to no-pass
			list[2] = item2;
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Remove, events[eventN].Action);
			Assert.Equal (-1, events[eventN].NewStartingIndex);
			Assert.Null (events[eventN].NewItems);
			Assert.Equal (1, events[eventN].OldStartingIndex);
			Assert.Equal (1, events[eventN].OldItems.Count);
			Assert.Equal (item4, events[eventN].OldItems[0]);
			Assert.Equal (item1, view.GetItemAt (0));
			Assert.Equal (item4, view.GetItemAt (1));
			Assert.True (view.Contains (item1));
			Assert.False (view.Contains (item2));
			Assert.False (view.Contains (item3));
			Assert.True (view.Contains (item4));
			Assert.Equal (0, view.IndexOf (item1));
			Assert.Equal (-1, view.IndexOf (item2));
			Assert.Equal (-1, view.IndexOf (item3));
			Assert.Equal (1, view.IndexOf (item4));

			// SourceCollection.Replace from no-pass to pass
			list[2] = item3;
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Add, events[eventN].Action);
			Assert.Equal (2, events[eventN].NewStartingIndex);
			Assert.Equal (1, events[eventN].NewItems.Count);
			Assert.Equal (item3, events[eventN].NewItems[0]);
			Assert.Equal (-1, events[eventN].OldStartingIndex);
			Assert.Null (events[eventN].OldItems);
			Assert.Equal (item1, view.GetItemAt (0));
			Assert.Equal (item4, view.GetItemAt (1));
			Assert.Equal (item3, view.GetItemAt (2));
			Assert.True (view.Contains (item1));
			Assert.False (view.Contains (item2));
			Assert.True (view.Contains (item3));
			Assert.True (view.Contains (item4));
			Assert.Equal (0, view.IndexOf (item1));
			Assert.Equal (-1, view.IndexOf (item2));
			Assert.Equal (2, view.IndexOf (item3));
			Assert.Equal (1, view.IndexOf (item4));

			// Refresh
			view.Refresh ();
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Reset, events[eventN].Action);
			Assert.Equal (-1, events[eventN].NewStartingIndex);
			Assert.Equal (-1, events[eventN].OldStartingIndex);
			Assert.Null (events[eventN].NewItems);
			Assert.Null (events[eventN].OldItems);
			Assert.Equal (item1, view.GetItemAt (0));
			Assert.Equal (item3, view.GetItemAt (1));
			Assert.Equal (item4, view.GetItemAt (2));
			Assert.True (view.Contains (item1));
			Assert.False (view.Contains (item2));
			Assert.True (view.Contains (item3));
			Assert.True (view.Contains (item4));
			Assert.Equal (0, view.IndexOf (item1));
			Assert.Equal (-1, view.IndexOf (item2));
			Assert.Equal (1, view.IndexOf (item3));
			Assert.Equal (2, view.IndexOf (item4));

			// set filter to pass none
			view.Filter = item => false;
			Assert.Equal (0, view.Count);
			Assert.False (view.Contains (item1));
			Assert.False (view.Contains (item2));
			Assert.False (view.Contains (item3));
			Assert.False (view.Contains (item4));
			Assert.Equal (-1, view.IndexOf (item1));
			Assert.Equal (-1, view.IndexOf (item2));
			Assert.Equal (-1, view.IndexOf (item3));
			Assert.Equal (-1, view.IndexOf (item4));

			// SourceCollection.Add not passes filter
			list.Add (item2);
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Reset, events[eventN].Action);
			Assert.Equal (-1, events[eventN].NewStartingIndex);
			Assert.Equal (-1, events[eventN].OldStartingIndex);
			Assert.Null (events[eventN].NewItems);
			Assert.Null (events[eventN].OldItems);
			Assert.Equal (0, view.Count);
			Assert.False (view.Contains (item1));
			Assert.False (view.Contains (item2));
			Assert.False (view.Contains (item3));
			Assert.False (view.Contains (item4));
			Assert.Equal (-1, view.IndexOf (item1));
			Assert.Equal (-1, view.IndexOf (item2));
			Assert.Equal (-1, view.IndexOf (item3));
			Assert.Equal (-1, view.IndexOf (item4));

			// remove filter
			view.Filter = null;
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Reset, events[eventN].Action);
			Assert.Equal (-1, events[eventN].NewStartingIndex);
			Assert.Equal (-1, events[eventN].OldStartingIndex);
			Assert.Null (events[eventN].NewItems);
			Assert.Null (events[eventN].OldItems);
			Assert.Equal (5, view.Count);
			Assert.Equal (item1, view.GetItemAt (0));
			Assert.Equal (item2, view.GetItemAt (1));
			Assert.Equal (item3, view.GetItemAt (2));
			Assert.Equal (item4, view.GetItemAt (3));
			Assert.Equal (item2, view.GetItemAt (4));
			Assert.True (view.Contains (item1));
			Assert.True (view.Contains (item2));
			Assert.True (view.Contains (item3));
			Assert.True (view.Contains (item4));
			Assert.Equal (0, view.IndexOf (item1));
			Assert.Equal (1, view.IndexOf (item2));
			Assert.Equal (2, view.IndexOf (item3));
			Assert.Equal (3, view.IndexOf (item4));
		}

		[Fact]
		[Trait ("Category", "LiteListCollectionView")]
		public void SortedOperations ()
		{
			var events = new List<NotifyCollectionChangedEventArgs> ();
			int eventN = 0;
			var list = new ObservableCollection<Mock4> ();
			var item1 = new Mock4 { Number = 1, Prop1 = "aab", Prop2 = 91 };
			var item2 = new Mock4 { Number = 2, Prop1 = "ZZZ", Prop2 = 110 };
			var item3 = new Mock4 { Number = 3, Prop1 = "aaa", Prop2 = 444 };
			var item4 = new Mock4 { Number = 4, Prop1 = "шшш", Prop2 = 273 };
			list.Add (item1);
			list.Add (item2);
			list.Add (item3);
			var view = new LiteListCollectionView<Mock4> (list);
			if (view.NeedsRefresh)
			{
				view.Refresh ();
			}

			(view as ICollectionView).CollectionChanged += (sender, args) => events.Add (args);

			// Count
			Assert.Equal (3, view.Count);

			// indexer
			Assert.Equal (item2, view.GetItemAt (1));
			Assert.Equal (item3, view.GetItemAt (2));
			Assert.Equal (item1, view.GetItemAt (0));
			Assert.True (view.Contains (item1));
			Assert.True (view.Contains (item2));
			Assert.True (view.Contains (item3));
			Assert.False (view.Contains (item4));
			Assert.Equal (0, view.IndexOf (item1));
			Assert.Equal (1, view.IndexOf (item2));
			Assert.Equal (2, view.IndexOf (item3));
			Assert.Equal (-1, view.IndexOf (item4));

			// set SortingComparer
			view.SortingComparer = ComparerFactory.CreateFromPropertySelector<Mock4, string> (item => item.Prop1);
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Reset, events[eventN].Action);
			Assert.Equal (-1, events[eventN].NewStartingIndex);
			Assert.Equal (-1, events[eventN].OldStartingIndex);
			Assert.Null (events[eventN].NewItems);
			Assert.Null (events[eventN].OldItems);
			Assert.Equal (item3, view.GetItemAt (0));
			Assert.Equal (item1, view.GetItemAt (1));
			Assert.Equal (item2, view.GetItemAt (2));
			Assert.True (view.Contains (item1));
			Assert.True (view.Contains (item2));
			Assert.True (view.Contains (item3));
			Assert.False (view.Contains (item4));
			Assert.Equal (1, view.IndexOf (item1));
			Assert.Equal (2, view.IndexOf (item2));
			Assert.Equal (0, view.IndexOf (item3));
			Assert.Equal (-1, view.IndexOf (item4));

			// change SortingComparer
			view.SortingComparer = ComparerFactory.CreateFromPropertySelector<Mock4, int> (item => item.Prop2, true);
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Reset, events[eventN].Action);
			Assert.Equal (-1, events[eventN].NewStartingIndex);
			Assert.Equal (-1, events[eventN].OldStartingIndex);
			Assert.Null (events[eventN].NewItems);
			Assert.Null (events[eventN].OldItems);
			Assert.Equal (item3, view.GetItemAt (0));
			Assert.Equal (item2, view.GetItemAt (1));
			Assert.Equal (item1, view.GetItemAt (2));
			Assert.True (view.Contains (item1));
			Assert.True (view.Contains (item2));
			Assert.True (view.Contains (item3));
			Assert.False (view.Contains (item4));
			Assert.Equal (2, view.IndexOf (item1));
			Assert.Equal (1, view.IndexOf (item2));
			Assert.Equal (0, view.IndexOf (item3));
			Assert.Equal (-1, view.IndexOf (item4));

			// SourceCollection.Add (to sorted)
			list.Add (item4);
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Add, events[eventN].Action);
			Assert.Equal (3, events[eventN].NewStartingIndex);
			Assert.Equal (1, events[eventN].NewItems.Count);
			Assert.Equal (item4, events[eventN].NewItems[0]);
			Assert.Equal (-1, events[eventN].OldStartingIndex);
			Assert.Null (events[eventN].OldItems);
			Assert.Equal (4, view.Count);
			Assert.Equal (item3, view.GetItemAt (0));
			Assert.Equal (item2, view.GetItemAt (1));
			Assert.Equal (item1, view.GetItemAt (2));
			Assert.Equal (item4, view.GetItemAt (3));
			Assert.True (view.Contains (item1));
			Assert.True (view.Contains (item2));
			Assert.True (view.Contains (item3));
			Assert.True (view.Contains (item4));
			Assert.Equal (2, view.IndexOf (item1));
			Assert.Equal (1, view.IndexOf (item2));
			Assert.Equal (0, view.IndexOf (item3));
			Assert.Equal (3, view.IndexOf (item4));

			// SourceCollection.RemoveAt (to sorted)
			list.RemoveAt (1);
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Remove, events[eventN].Action);
			Assert.Equal (-1, events[eventN].NewStartingIndex);
			Assert.Null (events[eventN].NewItems);
			Assert.Equal (1, events[eventN].OldStartingIndex);
			Assert.Equal (1, events[eventN].OldItems.Count);
			Assert.Equal (item2, events[eventN].OldItems[0]);
			Assert.Equal (item3, view.GetItemAt (0));
			Assert.Equal (item1, view.GetItemAt (1));
			Assert.Equal (item4, view.GetItemAt (2));
			Assert.True (view.Contains (item1));
			Assert.False (view.Contains (item2));
			Assert.True (view.Contains (item3));
			Assert.True (view.Contains (item4));
			Assert.Equal (1, view.IndexOf (item1));
			Assert.Equal (-1, view.IndexOf (item2));
			Assert.Equal (0, view.IndexOf (item3));
			Assert.Equal (2, view.IndexOf (item4));

			// SourceCollection.Insert (to sorted)
			list.Insert (1, item2);
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Add, events[eventN].Action);
			Assert.Equal (3, events[eventN].NewStartingIndex);
			Assert.Equal (1, events[eventN].NewItems.Count);
			Assert.Equal (item2, events[eventN].NewItems[0]);
			Assert.Equal (-1, events[eventN].OldStartingIndex);
			Assert.Equal (item3, view.GetItemAt (0));
			Assert.Equal (item1, view.GetItemAt (1));
			Assert.Equal (item4, view.GetItemAt (2));
			Assert.Equal (item2, view.GetItemAt (3));
			Assert.True (view.Contains (item1));
			Assert.True (view.Contains (item2));
			Assert.True (view.Contains (item3));
			Assert.True (view.Contains (item4));
			Assert.Equal (1, view.IndexOf (item1));
			Assert.Equal (3, view.IndexOf (item2));
			Assert.Equal (0, view.IndexOf (item3));
			Assert.Equal (2, view.IndexOf (item4));

			// SourceCollection.Replace (to sorted)
			list[1] = item3;
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Replace, events[eventN].Action);
			Assert.Equal (3, events[eventN].NewStartingIndex);
			Assert.Equal (1, events[eventN].NewItems.Count);
			Assert.Equal (item3, events[eventN].NewItems[0]);
			Assert.Equal (3, events[eventN].OldStartingIndex);
			Assert.Equal (1, events[eventN].OldItems.Count);
			Assert.Equal (item2, events[eventN].OldItems[0]);
			Assert.Equal (item3, view.GetItemAt (0));
			Assert.Equal (item1, view.GetItemAt (1));
			Assert.Equal (item4, view.GetItemAt (2));
			Assert.Equal (item3, view.GetItemAt (3));
			Assert.True (view.Contains (item1));
			Assert.False (view.Contains (item2));
			Assert.True (view.Contains (item3));
			Assert.True (view.Contains (item4));
			Assert.Equal (1, view.IndexOf (item1));
			Assert.Equal (-1, view.IndexOf (item2));

			// item3 в исходном списке есть в двух позициях 1 и 2, что соответствует позициям 3 и 0 в предствлении.
			// метод IndexOf() осуществляется поиск в исходном списке,
			// поэтому item3 в представлении будет найден в позиции 3 (исходная позиция 1), а не 0 (исходная позиция 2)
			Assert.Equal (3, view.IndexOf (item3));
			Assert.Equal (2, view.IndexOf (item4));

			// Refresh
			view.Refresh ();
			eventN++;
			Assert.Equal (eventN + 1, events.Count);
			Assert.Equal (NotifyCollectionChangedAction.Reset, events[eventN].Action);
			Assert.Equal (-1, events[eventN].NewStartingIndex);
			Assert.Equal (-1, events[eventN].OldStartingIndex);
			Assert.Null (events[eventN].NewItems);
			Assert.Null (events[eventN].OldItems);
			Assert.Equal (item3, view.GetItemAt (0));
			Assert.Equal (item3, view.GetItemAt (1));
			Assert.Equal (item4, view.GetItemAt (2));
			Assert.Equal (item1, view.GetItemAt (3));
			Assert.True (view.Contains (item1));
			Assert.False (view.Contains (item2));
			Assert.True (view.Contains (item3));
			Assert.True (view.Contains (item4));
			Assert.Equal (3, view.IndexOf (item1));
			Assert.Equal (-1, view.IndexOf (item2));
			Assert.Equal (0, view.IndexOf (item3));
			Assert.Equal (2, view.IndexOf (item4));
		}

		[Fact]
		[Trait ("Category", "LiteListCollectionView")]
		public void ManualMoveCurrent ()
		{
			var list = new ObservableCollection<Mock4> ();
			var item1 = new Mock4 { Number = 1, Prop1 = "aab", Prop2 = 91 };
			var item2 = new Mock4 { Number = 2, Prop1 = "ZZZ", Prop2 = 110 };
			var item3 = new Mock4 { Number = 3, Prop1 = "aaa", Prop2 = 444 };
			list.Add (item1);
			list.Add (item2);
			list.Add (item3);

			var view = new LiteListCollectionView<Mock4> (list);
			if (view.NeedsRefresh)
			{
				view.Refresh ();
			}

			view.CurrentChanged += (sender, args) => _eventN++;
			_eventN = 0;

			Assert.Equal (3, view.Count);
			Assert.Equal (item2, view[1]);
			Assert.Equal (item3, view[2]);
			Assert.Equal (item1, view[0]);

			// default values
			Assert.Equal (0, view.CurrentPosition);
			Assert.Equal (item1, view.CurrentItem);
			Assert.False (view.IsCurrentBeforeFirst);
			Assert.False (view.IsCurrentAfterLast);

			view.MoveCurrentToLast ();
			Assert.Equal (1, _eventN); // есть событие
			Assert.Equal (2, view.CurrentPosition);
			Assert.Equal (item3, view.CurrentItem);
			Assert.False (view.IsCurrentBeforeFirst);
			Assert.False (view.IsCurrentAfterLast);

			view.MoveCurrentToFirst ();
			Assert.Equal (2, _eventN); // есть событие
			Assert.Equal (0, view.CurrentPosition);
			Assert.Equal (item1, view.CurrentItem);
			Assert.False (view.IsCurrentBeforeFirst);
			Assert.False (view.IsCurrentAfterLast);

			view.MoveCurrentToNext ();
			Assert.Equal (3, _eventN); // есть событие
			Assert.Equal (1, view.CurrentPosition);
			Assert.Equal (item2, view.CurrentItem);
			Assert.False (view.IsCurrentBeforeFirst);
			Assert.False (view.IsCurrentAfterLast);

			// MoveCurrentToPosition to another position
			view.MoveCurrentToPosition (2);
			Assert.Equal (4, _eventN); // есть событие
			Assert.Equal (2, view.CurrentPosition);
			Assert.Equal (item3, view.CurrentItem);
			Assert.False (view.IsCurrentBeforeFirst);
			Assert.False (view.IsCurrentAfterLast);

			view.MoveCurrentToPrevious ();
			Assert.Equal (5, _eventN); // есть событие
			Assert.Equal (1, view.CurrentPosition);
			Assert.Equal (item2, view.CurrentItem);
			Assert.False (view.IsCurrentBeforeFirst);
			Assert.False (view.IsCurrentAfterLast);

			// MoveCurrentToPosition to same position
			view.MoveCurrentToPosition (1);
			Assert.Equal (5, _eventN); // нет события
			Assert.Equal (1, view.CurrentPosition);
			Assert.Equal (item2, view.CurrentItem);
			Assert.False (view.IsCurrentBeforeFirst);
			Assert.False (view.IsCurrentAfterLast);

			// Отменённое перемещение
			view.CurrentChanging += (obj, args) => args.Cancel = true;
			view.MoveCurrentToLast ();
			Assert.Equal (5, _eventN); // нет события
			Assert.Equal (1, view.CurrentPosition); // позиция не изменилась
			Assert.Equal (item2, view.CurrentItem);
			Assert.False (view.IsCurrentBeforeFirst);
			Assert.False (view.IsCurrentAfterLast);
		}

		[Fact]
		[Trait ("Category", "LiteListCollectionView")]
		public void AutomaticChangeCurrent ()
		{
			var list = new ObservableCollection<Mock4> ();
			var item1 = new Mock4 { Number = 1, Prop1 = "aab", Prop2 = 91 };
			var item2 = new Mock4 { Number = 2, Prop1 = "ZZZ", Prop2 = 110 };
			var item3 = new Mock4 { Number = 3, Prop1 = "aaa", Prop2 = 444 };
			var item4 = new Mock4 { Number = 4, Prop1 = "шшш", Prop2 = 273 };
			list.Add (item1);
			list.Add (item2);
			list.Add (item3);

			var view = new LiteListCollectionView<Mock4> (list);
			if (view.NeedsRefresh)
			{
				view.Refresh ();
			}

			view.CurrentChanged += (sender, args) => _eventN++;
			_eventN = 0;

			Assert.Equal (3, view.Count);
			Assert.Equal (item1, view[0]);
			Assert.Equal (item2, view[1]);
			Assert.Equal (item3, view[2]);

			// default values
			Assert.Equal (0, view.CurrentPosition);
			Assert.Equal (item1, view.CurrentItem);
			Assert.False (view.IsCurrentBeforeFirst);
			Assert.False (view.IsCurrentAfterLast);

			// устанавливаем текущим последний элемент
			view.MoveCurrentToLast ();
			Assert.Equal (1, _eventN); // есть событие
			Assert.Equal (2, view.CurrentPosition);
			Assert.Equal (item3, view.CurrentItem);
			Assert.False (view.IsCurrentBeforeFirst);
			Assert.False (view.IsCurrentAfterLast);

			// SourceCollection.RemoveAt before current
			list.RemoveAt (1);
			Assert.Equal (1, _eventN); // нет события
			Assert.Equal (1, view.CurrentPosition);
			Assert.Equal (item3, view.CurrentItem);
			Assert.False (view.IsCurrentBeforeFirst);
			Assert.False (view.IsCurrentAfterLast);

			// SourceCollection.Insert before current (в представлении добавится в конец)
			list.Insert (1, item2);
			Assert.Equal (1, _eventN); // нет события
			Assert.Equal (1, view.CurrentPosition);
			Assert.Equal (item3, view.CurrentItem);
			Assert.False (view.IsCurrentBeforeFirst);
			Assert.False (view.IsCurrentAfterLast);

			// Refresh с изменением порядка элементов
			view.Refresh ();
			Assert.Equal (2, _eventN); // есть событие
			Assert.Equal (2, view.CurrentPosition);
			Assert.Equal (item3, view.CurrentItem);
			Assert.False (view.IsCurrentBeforeFirst);
			Assert.False (view.IsCurrentAfterLast);

			// SourceCollection.Add
			list.Add (item4);
			Assert.Equal (2, _eventN); // нет события
			Assert.Equal (2, view.CurrentPosition);
			Assert.Equal (item3, view.CurrentItem);
			Assert.False (view.IsCurrentBeforeFirst);
			Assert.False (view.IsCurrentAfterLast);

			// SourceCollection.RemoveAt current
			list.RemoveAt (2);
			Assert.Equal (3, _eventN); // есть событие
			Assert.Equal (2, view.CurrentPosition);
			Assert.Equal (item4, view.CurrentItem);
			Assert.False (view.IsCurrentBeforeFirst);
			Assert.False (view.IsCurrentAfterLast);

			// SourceCollection[] set current
			list[2] = item3;
			Assert.Equal (4, _eventN); // есть событие
			Assert.Equal (2, view.CurrentPosition);
			Assert.Equal (item3, view.CurrentItem);
			Assert.False (view.IsCurrentBeforeFirst);
			Assert.False (view.IsCurrentAfterLast);
		}

		[DebuggerDisplay ("#{Number} Prop1 = {Prop1} Prop2 = {Prop2}")]
		internal class Mock4
		{
			public int Number { get; set; }

			public string Prop1 { get; set; }

			public int Prop2 { get; set; }
		}
	}
}
