using System;
using Novartment.Base.Collections.Linq;
using System.ComponentModel;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Data;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Novartment.Base.Collections;

namespace Novartment.Base.UI.Wpf
{
	/* WPF-логика создания ICollectionView
	ViewRecord GetViewRecord (object collection)
	{
		var vr = GetExistingView (collection);
		if (vr != null) return vr;

		var listSource = collection as IListSource;
		IList list = null;
		if (listSource != null)
		{
			list = listSource.GetList ();
			vr = GetExistingView (list);
			if (vr != null) return CacheView (collection, (CollectionView)vr.View, vr);
		}
		var collectionView = collection as ICollectionView;
		if (collectionView != null) collectionView = new CollectionViewProxy (collectionView);
		else
		{
			var factory = collection as ICollectionViewFactory;
			if (factory != null) collectionView = factory.CreateView ();
			else
			{
				var list2 = (list != null) ? list : (collection as IList);
				if (list2 != null)
				{
					var bindingList = list2 as IBindingList;
					if (bindingList != null) collectionView = new BindingListCollectionView (bindingList);
					else collectionView = new ListCollectionView (list2);
				}
				else
				{
					var enumerable = collection as IEnumerable;
					if (enumerable != null) collectionView = new EnumerableCollectionView (enumerable);
				}
			}
		}

		if (collectionView != null)
		{
			var cv = collectionView as CollectionView;
			if (cv == null) cv = new CollectionViewProxy (collectionView);
			if (list != null) vr = CacheView (list, cv, null);
			vr = CacheView (collection, cv, vr);
			BindingOperations.OnCollectionViewRegistering (cv);
		}

		return vr;
	}
	*/

	/// <summary>
	/// Представление только для чтения с поддержкой фильтрации и сортировки,
	/// оптимизированное для списка с произвольным доступом по номеру позиции.
	/// </summary>
	/// <typeparam name="TItem">Тип элементов списка.</typeparam>
	/// <remarks>
	/// По сравнению с ListCollectionView:
	/// * не создаёт копий исходного списка, то есть не нагружает сборщик мусора;
	/// * для эффективной сортировки содержит свойство SortingComparer, содержащее объект с методом для сравнения двух элементов списка;
	/// * для привязки к элементам управления содержит команду SortingCommand, принимающую аргументом имя свойства для сортировки;
	/// * поддерживает диапазонные уведомления об изменении исходного списка;
	/// * кэширует результат последнего поиска элемента.
	/// Ограничение: уведомление типа NotifyCollectionChangedAction.Move не поддерживается.
	/// </remarks>
	// TODO: обеспечить конкурентный доступ. lock _source2view ?

	// Согласно логике работы WPF, наследование CollectionView обязательно,
	// иначе при использовании наше представление будет обёрнуто в CollectionViewProxy с потерей всех преимуществ.
	// Помечен как sealed потому что создержит обращение к virtual-членам в конструкторе.
	[SuppressMessage ("Microsoft.Naming",
		"CA1710:IdentifiersShouldHaveCorrectSuffix",
		Justification = "Implemented interfaces has no association with class name.")]
	public sealed class LiteListCollectionView<TItem> : CollectionView,
		IReadOnlyList<TItem>
	{
		// таблица соответствия индексов исходной коллекции индексам представления
		private readonly ArrayList<int> _source2view;
		// таблица соответствия индексов представления индексам исходной коллекции
		private readonly ArrayList<int> _view2source;
		private readonly ICommand _sortingCommand;
		private IComparer<TItem> _sortingComparer;
		private string _lastSortingPropertyName;
		// для оптимизации повторяющегося поиска
		private TItem _lastSearchedItem;
		private int _lastSearchedItemIndex = -1;

		/// <summary>
		/// Инициализирует новый экземпляр класса LiteListCollectionView
		/// в качестве представления указанного списка.
		/// </summary>
		/// <param name="list">Список, на основе которого будет создано представление.</param>
		public LiteListCollectionView (IReadOnlyList<TItem> list)
			: base (ValidateListArgument (list))
		{
			_sortingCommand = new ChainedRelayCommand<string> (SortingCommandInternal);

			int count = 0;
			if (this.AllowsCrossThreadChanges)
			{
				BindingOperations.AccessCollection (
					this.SourceCollection,
					() => count = (this.SourceCollection as IReadOnlyCollection<TItem>).Count,
					false);
			}
			else
			{
				count = list.Count;
			}
			var source2view = new int[count];
			var view2source = new int[count];
			for (var i = 0; i < count; i++)
			{
				source2view[i] = i;
				view2source[i] = i;
			}
			_source2view = new ArrayList<int> (source2view);
			_view2source = new ArrayList<int> (view2source);
		}
		private static IReadOnlyList<TItem> ValidateListArgument (IReadOnlyList<TItem> list)
		{
			if (list == null)
			{
				throw new ArgumentNullException (nameof (list));
			}
			Contract.EndContractBlock ();
			return list;
		}

		/// <summary>
		/// Получает компаратор, используемый для сортировки элементов представления.
		/// </summary>
		public override IComparer Comparer
		{
			get
			{
				return (_sortingComparer as IComparer);
			}
		}

		/// <summary>
		/// Получает или устанавливает компаратор, используемый для сортировки элементов представления.
		/// </summary>
		public IComparer<TItem> SortingComparer
		{
			get
			{
				return _sortingComparer;
			}
			set
			{
				_sortingComparer = value;
				RefreshOrDefer ();
			}
		}

		/// <summary>
		/// Получает элемент представления в указанной позиции.
		/// </summary>
		/// <param name="index">Позиция в представлении.</param>
		/// <remarks>Не работает с рассинхронизированным представлением (если установлен флаг NeedsRefresh).</remarks>
		public TItem this[int index]
		{
			get
			{
				if (index < 0)
				{
					throw new ArgumentOutOfRangeException (nameof (index));
				}
				Contract.EndContractBlock ();

				if (this.NeedsRefresh)
				{
					throw new InvalidOperationException ("Access to view denied while it needs refresh.");
				}

				var result = default (TItem);
				if (this.AllowsCrossThreadChanges)
				{
					BindingOperations.AccessCollection (
						this.SourceCollection,
						() => result = (this.SourceCollection as IReadOnlyList<TItem>)[_view2source[index]],
						false);
				}
				else
				{
					result = (this.SourceCollection as IReadOnlyList<TItem>)[_view2source[index]];
				}
				return result;
			}
		}

		/// <summary>
		/// Получает команду сортировки, принимающую параметр-имя свойства по которому будет производится сортировка.
		/// При повторном выполнении с тем-же параметром происходит инвертирование направления сортировки.
		/// Чтобы отключить сортировку вызовите команду с null-параметром.
		/// </summary>
		public ICommand SortingCommand => _sortingCommand;

		/// <summary>
		/// Обрабатывает изменения исходного списка в соответствии с указанными параметрами уведомления.
		/// </summary>
		/// <param name="args">Параметры уведомления об изменении исходного списка.</param>
		protected override void ProcessCollectionChanged (NotifyCollectionChangedEventArgs args)
		{
			if (args == null)
			{
				throw new ArgumentNullException (nameof (args));
			}
			Contract.EndContractBlock ();

			if (this.NeedsRefresh)
			{
				return; // уведомления не нужны если запланировано полное пересоздание представления
			}

			// запоминаем значения свойств об изменении которых надо будет послать уведомление
			var currentPosition = this.CurrentPosition;
			var currentItem = this.CurrentItem;
			var isCurrentAfterLast = this.IsCurrentAfterLast;
			var isCurrentBeforeFirst = this.IsCurrentBeforeFirst;

			switch (args.Action)
			{
				case NotifyCollectionChangedAction.Reset:
					_lastSearchedItem = default (TItem);
					_lastSearchedItemIndex = -1;
					_source2view.Clear ();
					_view2source.Clear ();
					OnCurrentChanging (new CurrentChangingEventArgs (false));
					SetCurrent (null, -1);
					OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset));
					OnCurrentChanged ();
					break;
				case NotifyCollectionChangedAction.Add:
					InsertRange (args.NewItems, args.NewStartingIndex);
					break;
				case NotifyCollectionChangedAction.Remove:
					RemoveRange (args.OldItems, args.OldStartingIndex);
					break;
				case NotifyCollectionChangedAction.Replace:
					ReplaceRange (args.OldItems, args.NewItems, args.NewStartingIndex);
					break;
				case NotifyCollectionChangedAction.Move:
					throw new NotSupportedException ("Notification of type NotifyCollectionChangedAction.Move not supported.");
			}
			// уведомляем об изменениях
			if (this.IsCurrentAfterLast != isCurrentAfterLast)
			{
				OnPropertyChanged (new PropertyChangedEventArgs (nameof (this.IsCurrentAfterLast)));
			}
			if (this.IsCurrentBeforeFirst != isCurrentBeforeFirst)
			{
				OnPropertyChanged (new PropertyChangedEventArgs (nameof (this.IsCurrentBeforeFirst)));
			}
			if (currentPosition != this.CurrentPosition)
			{
				OnPropertyChanged (new PropertyChangedEventArgs (nameof (this.CurrentPosition)));
			}
			if (currentItem != this.CurrentItem)
			{
				OnPropertyChanged (new PropertyChangedEventArgs (nameof (this.CurrentItem)));
			}
		}

		// TODO: реализовать учёт свойства AllowsCrossThreadChanges
		//protected override void OnAllowsCrossThreadChangesChanged () { base.OnAllowsCrossThreadChangesChanged (); }

		/// <summary>
		/// Пересоздаёт представление.
		/// </summary>
		protected override void RefreshOverride ()
		{
			// очищаем очередь задержанных уведомлений, потому что все изменения исходной коллекции будут учтены при пересоздании представления
			ClearPendingChanges ();

			_lastSearchedItem = default (TItem);
			_lastSearchedItemIndex = -1;

			if (this.AllowsCrossThreadChanges)
			{
				BindingOperations.AccessCollection (this.SourceCollection, RecreateIndexes, false);
			}
			else
			{
				RecreateIndexes ();
			}

			var currentItem = this.CurrentItem;
			var currentPosition = (_view2source.Count < 1) ? -1 : this.CurrentPosition;
			var isCurrentAfterLast = this.IsCurrentAfterLast;
			var isCurrentBeforeFirst = this.IsCurrentBeforeFirst;
			OnCurrentChanging (new CurrentChangingEventArgs (false));

			// корректирует выбранный элемент
			if ((_view2source.Count < 1) || this.IsCurrentBeforeFirst)
			{
				SetCurrent (null, -1);
			}
			else
			{
				if (this.IsCurrentAfterLast)
				{
					SetCurrent (null, _view2source.Count);
				}
				else
				{
					if (this.CurrentItem != null)
					{
						var index = this.IndexOfWithoutChecks (this.CurrentItem);
						if (index >= 0)
						{
							SetCurrent (this.CurrentItem, index);
						}
					}
				}
			}
			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Reset));
			OnCurrentChanged ();

			// уведомляем об изменениях
			if (this.IsCurrentAfterLast != isCurrentAfterLast)
			{
				OnPropertyChanged (new PropertyChangedEventArgs (nameof (this.IsCurrentAfterLast)));
			}
			if (this.IsCurrentBeforeFirst != isCurrentBeforeFirst)
			{
				OnPropertyChanged (new PropertyChangedEventArgs (nameof (this.IsCurrentBeforeFirst)));
			}
			if (currentPosition != this.CurrentPosition)
			{
				OnPropertyChanged (new PropertyChangedEventArgs (nameof (this.CurrentPosition)));
			}
			if (currentItem != this.CurrentItem)
			{
				OnPropertyChanged (new PropertyChangedEventArgs (nameof (this.CurrentItem)));
			}
		}

		/// <summary>
		/// Получает перечислитель элементов списка.
		/// </summary>
		/// <returns>Перечислитель элементов списка.</returns>
		protected override IEnumerator GetEnumerator ()
		{
			if (this.IsRefreshDeferred)
			{
				throw new InvalidOperationException ("Can't create enumerator when deferred refresh pending.");
			}
			return new _SourceWithMapEnumerator (this);
		}
		/// <summary>
		/// Получает перечислитель элементов списка.
		/// </summary>
		/// <returns>Перечислитель элементов списка.</returns>
		IEnumerator<TItem> IEnumerable<TItem>.GetEnumerator ()
		{
			if (this.IsRefreshDeferred)
			{
				throw new InvalidOperationException ("Can't create enumerator when deferred refresh pending.");
			}
			return new _SourceWithMapEnumerator (this);
		}

		/// <summary>
		/// Получает значение, означающее поддержку представлением сортировки.
		/// </summary>
		public override bool CanSort => true;

		/// <summary>
		/// Получает количество элементов в представлении.
		/// </summary>
		public override int Count => _view2source.Count;

		/// <summary>
		/// Возвращает true если представление пусто.
		/// </summary>
		public override bool IsEmpty => _view2source.Count < 1;

		/// <summary>
		/// Получает коллекцию объектов, описывающих сортировку представления.
		/// </summary>
		public override SortDescriptionCollection SortDescriptions
		{
			get
			{
				return SortDescriptionCollection.Empty;
			}
		}

		/// <summary>
		/// Определяет, входит ли элемент в состав представления.
		/// </summary>
		/// <param name="item">Объект, поиск которого осуществляется в представлении.</param>
		/// <returns> Значение true, если элемент item найден в представлении; в противном случае — значение false.</returns>
		public override bool Contains (object item)
		{
			if (this.NeedsRefresh)
			{
				throw new InvalidOperationException ("Access to view denied while it needs refresh.");
			}

			var itemT = (TItem)item;

			// оптимизация повторяющегося поиска
			var isItemEqualsLastSearchedItem = EqualityComparer<TItem>.Default.Equals (itemT, _lastSearchedItem);
			if (isItemEqualsLastSearchedItem)
			{
				return (_lastSearchedItemIndex >= 0);
			}

			bool result = false;
			if (this.AllowsCrossThreadChanges)
			{
				BindingOperations.AccessCollection (
					this.SourceCollection,
					() => result = (IndexOf (this.SourceCollection, itemT) >= 0),
					false);
			}
			else
			{
				result = (IndexOf (this.SourceCollection, itemT) >= 0);
			}

			return (result && PassesFilter (item));
		}

		/// <summary>
		/// Ищет первое соответствие указанному образцу в представлении.
		/// </summary>
		/// <param name="item">Элемент-образец для поиска.</param>
		/// <returns>Номер позиции в представлении первого встретившегося соответствия образцу,
		/// либо -1 если соответствия не найдено.</returns>
		/// <remarks>Не работает с рассинхронизированным представлением (если установлен флаг NeedsRefresh).</remarks>
		public override int IndexOf (object item)
		{
			if (this.NeedsRefresh)
			{
				throw new InvalidOperationException ("Access to view denied while it needs refresh.");
			}
			return IndexOfWithoutChecks (item);
		}

		private int IndexOfWithoutChecks (object item)
		{
			var itemT = (TItem)item;

			// оптимизация повторяющегося поиска
			var isItemEqualsLastSearchedItem = EqualityComparer<TItem>.Default.Equals (itemT, _lastSearchedItem);
			if (isItemEqualsLastSearchedItem)
			{
				return _lastSearchedItemIndex;
			}

			var sourceIndex = -1;
			if (this.AllowsCrossThreadChanges)
			{
				BindingOperations.AccessCollection (
					this.SourceCollection,
					() => sourceIndex = IndexOf ((IReadOnlyList<TItem>)this.SourceCollection, itemT),
					false);
			}
			else
			{
				sourceIndex = IndexOf ((IReadOnlyList<TItem>)this.SourceCollection, itemT);
			}

			var viewIndex = (sourceIndex >= 0) ? _source2view[sourceIndex] : -1;
			_lastSearchedItem = itemT;
			_lastSearchedItemIndex = viewIndex;

			return viewIndex;
		}

		private static int IndexOf (IEnumerable enumerable, TItem item)
		{
			var list = enumerable as IReadOnlyList<TItem>;
			var comparer = EqualityComparer<TItem>.Default;
			for (var i = 0; i < list.Count; i++)
			{
				var isCurrentItemEqualsItem = comparer.Equals (list[i], item);
				if (isCurrentItemEqualsItem)
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>
		/// Получает элемент представления в указанной позиции.
		/// </summary>
		/// <param name="index">Номер позиции в представлении.</param>
		/// <returns>Элемент представления в указанной позиции.</returns>
		/// <remarks>Не работает с рассинхронизированным представлением (если установлен флаг NeedsRefresh).</remarks>
		public override object GetItemAt (int index)
		{
			if (index < 0)
			{
				throw new ArgumentOutOfRangeException (nameof (index));
			}
			Contract.EndContractBlock ();

			if (this.NeedsRefresh)
			{
				throw new InvalidOperationException ("Access to view denied while it needs refresh.");
			}

			var result = default (TItem);
			if (this.AllowsCrossThreadChanges)
			{
				BindingOperations.AccessCollection (
					this.SourceCollection,
					() => result = (this.SourceCollection as IReadOnlyList<TItem>)[_view2source[index]],
					false);
			}
			else
			{
				result = (this.SourceCollection as IReadOnlyList<TItem>)[_view2source[index]];
			}

			return result;
		}

		/// <summary>
		/// Устанавливает выбранным элементом представления элемент из указанной позиции.
		/// </summary>
		/// <param name="position">Номер позиции в представлении.</param>
		/// <returns>True если выбранный элемент успешно установлен, иначе False.</returns>
		public override bool MoveCurrentToPosition (int position)
		{
			if ((position < -1) || (position > _view2source.Count))
			{
				throw new ArgumentOutOfRangeException (nameof (position));
			}

			if (position != this.CurrentPosition)
			{
				var args = new CurrentChangingEventArgs ();
				OnCurrentChanging (args);
				if (!args.Cancel)
				{
					var isCurrentAfterLast = this.IsCurrentAfterLast;
					var isCurrentBeforeFirst = this.IsCurrentBeforeFirst;
					if ((position < 0) || (position >= _view2source.Count))
					{
						SetCurrent (null, position);
					}
					else
					{
						SetCurrent (GetItemAt (position), position);
					}
					OnCurrentChanged ();
					if (this.IsCurrentAfterLast != isCurrentAfterLast)
					{
						OnPropertyChanged (new PropertyChangedEventArgs (nameof (this.IsCurrentAfterLast)));
					}
					if (this.IsCurrentBeforeFirst != isCurrentBeforeFirst)
					{
						OnPropertyChanged (new PropertyChangedEventArgs (nameof (this.IsCurrentBeforeFirst)));
					}
					OnPropertyChanged (new PropertyChangedEventArgs (nameof (this.CurrentPosition)));
					OnPropertyChanged (new PropertyChangedEventArgs (nameof (this.CurrentItem)));
				}
			}
			return ((0 <= this.CurrentPosition) && (this.CurrentPosition < _view2source.Count));
		}

		private void RecreateIndexes ()
		{
			var source = this.SourceCollection as IReadOnlyList<TItem>;
			var count = source.Count;

			// убеждаемся что массив индексов имеет тот же размер что и коллекция-источник
			if (_source2view.Count != count)
			{
				_source2view.Clear ();
				_source2view.InsertRange (0, count);
				var idx = 0;
				while (idx < count)
				{
					_source2view[idx++] = -1;
				}
				_source2view.TrimExcess ();
			}
			_view2source.Clear ();

			// если нет фильтрации то можно заранее знать кол-во элементов в представлении
			if (this.Filter == null)
			{
				_view2source.EnsureCapacity (count);
			}

			// фильтрация
			for (var i = 0; i < count; i++)
			{
				var isFilterPassed = PassesFilter (source[i]);
				if (isFilterPassed)
				{
					_source2view[i] = _view2source.Count;
					_view2source.Add (i);
				}
				else
				{
					_source2view[i] = -1;
				}
			}
			_view2source.TrimExcess ();

			if (_sortingComparer != null)
			{
				// сортировка _view2source
				_view2source.Sort (new _ComparerByIndexes (source, _sortingComparer));

				// корректировка _source2view
				for (var i = 0; i < _view2source.Count; i++)
				{
					_source2view[_view2source[i]] = i;
				}
			}
		}

		private void SortingCommandInternal (string propertyName)
		{
			if (propertyName == null)
			{
				_sortingComparer = null;
			}
			{
				var isPropertyNameEqualsLastSortingPropertyName = string.Equals (_lastSortingPropertyName, propertyName, StringComparison.Ordinal);
				if (isPropertyNameEqualsLastSortingPropertyName)
				{
					var comparerWithSortDirection = _sortingComparer as ISortDirectionVariable;
					if (comparerWithSortDirection != null)
					{
						comparerWithSortDirection.DescendingOrder = !comparerWithSortDirection.DescendingOrder;
					}
				}
				else
				{
					_lastSortingPropertyName = propertyName;
					_sortingComparer = ComparerFactory.CreateFromPropertyName<TItem> (propertyName);
				}
			}
			Refresh ();
		}

		#region корректировка индексов в соответствии с уведомлением об изменениях в исходной коллекции

		private void ReplaceRange (IList oldItems, IList newItems, int startingIndex)
		{
			var count = newItems.Count;
			for (var i = 0; i < count; i++)
			{
				var oldViewIndex = _source2view[startingIndex + i];
				int newViewIndex = -1;
				var isFilterPassed = PassesFilter (newItems[i]);
				if (isFilterPassed)
				{
					newViewIndex = (oldViewIndex >= 0) ? oldViewIndex : _view2source.Count;
				}
				if ((oldViewIndex >= 0) && (newViewIndex >= 0))
				{ // элемент до изменения и после проходит фильтрацию. уведомляем что он изменился
					ViewReplaceOne (oldItems[i], newItems[i], newViewIndex);
				}
				else
				{
					if ((oldViewIndex >= 0) && (newViewIndex < 0))
					{ // элемент до изменения проходил фильтрацию, а после - не проходит. уведомляем что он удалился
						_source2view[startingIndex + i] = -1;
						ViewRemoveOne (oldItems[i], oldViewIndex);
					}
					else
					{
						if ((oldViewIndex < 0) && (newViewIndex >= 0))
						{ // элемент до изменения не проходил фильтрацию, а после - проходит. уведомляем что он добавился
							ViewInsertOne (newItems[i], startingIndex + i, newViewIndex);
						}
					}
				}
			}
		}

		private void RemoveRange (IList items, int startingIndex)
		{
			var count = items.Count;

			if (count == 1)
			{
				var removedViewIndex = SourceRemoveOne (startingIndex);
				if (removedViewIndex >= 0)
				{
					ViewRemoveOne (items[0], removedViewIndex);
				}
			}
			else
			{
				var removedIndexes = SourceRemoveRange (startingIndex, count);

				// уведомляем об удалённых элементах прошедших фильтрацию,
				// перебирая с конца отсортированный список удаляемых индексов
				Array.Sort (removedIndexes);
				for (var i = count - 1; i >= 0; i--)
				{
					var viewIndex = removedIndexes[i];
					// если элемент проходил фильтрацию, то удаляем его из представления
					if (viewIndex >= 0)
					{
						ViewRemoveOne (items[i], viewIndex);
					}
				}
			}
		}

		private void InsertRange (IList items, int startingIndex)
		{
			var count = items.Count;

			// добавляем всегда в конец как есть (только фильтрация без сортировки)
			var startingIndexView = _view2source.Count;

			SourceInsertRange (startingIndex, count);

			// фильтруем добавленное, заполняем таблицу индексов и уведомляем о прошедших фильтрацию
			for (var i = 0; i < count; i++)
			{
				var item = (TItem)items[i];
				var isFilterPassed = PassesFilter (item);
				if (isFilterPassed)
				{
					ViewInsertOne (item, startingIndex + i, startingIndexView + i);
				}
				else
				{
					_source2view[startingIndex + i] = -1;
				}
			}
		}

		private void SourceInsertRange (int sourceIndex, int count)
		{
			// резервируем место в таблице индексов
			if (count == 1)
			{
				_source2view.Insert (sourceIndex, -1);
			}
			else
			{
				_source2view.InsertRange (sourceIndex, ReadOnlyList.Repeat (-1, count)); // TODO: заменить на повторитель с заранее известным кол-вом элементов
			}

			// корректируем индексы представления для учёта добавленных, увеличивая на count все равные и большие
			// TODO: если нет сортировки то можно перебирать не все
			for (var i = 0; i < _view2source.Count; i++)
			{
				var idx = _view2source[i];
				if (idx >= sourceIndex)
				{
					_view2source[i] = idx + count;
				}
			}
		}

		private int SourceRemoveOne (int sourceIndex)
		{
			var removedIndex = _source2view[sourceIndex];
			// удаляем диапазон из таблицы индексов источника
			_source2view.RemoveAt (sourceIndex);
			// схлопываем дыру удалённых индексов, уменьшая на count все равные и большие
			// TODO: если нет сортировки то можно перебирать не все
			for (var i = 0; i < _view2source.Count; i++)
			{
				var idx = _view2source[i];
				if (idx >= sourceIndex + 1)
				{
					_view2source[i] = idx - 1;
				}
			}
			return removedIndex;
		}

		private int[] SourceRemoveRange (int sourceIndex, int count)
		{
			var removedIndexes = new int[count];
			// создаём временный список содержащий только удаляемый диапазон (будет использоваться тотже массив, без копирования)
			var offset = _source2view.Offset + sourceIndex;
			if (offset >= _source2view.Array.Length)
			{
				offset -= _source2view.Array.Length;
			}
			var tmp = new ArrayList<int> (_source2view.Array, offset, count);
			tmp.CopyTo (removedIndexes, 0);

			// удаляем диапазон из таблицы индексов источника
			_source2view.RemoveRange (sourceIndex, count);

			// схлопываем дыру удалённых индексов, уменьшая на count все равные и большие
			var limit = sourceIndex + count;
			// TODO: если нет сортировки то можно перебирать не все
			for (var i = 0; i < _view2source.Count; i++)
			{
				var idx = _view2source[i];
				if (idx >= limit)
				{
					_view2source[i] = idx - count;
				}
			}
			return removedIndexes;
		}

		private void ViewReplaceOne (object oldItem, object newItem, int viewIndex)
		{
			// если последний поиск был неудачным, то после замены тот же поиск может стать удачным, поэтому аннулируем последний поиск
			if (_lastSearchedItemIndex < 0)
			{
				_lastSearchedItem = default (TItem);
			}
			else
			{
				// если элемент заменяется в той позиции, которая была найдена в последнем поиске, то аннулируем последний поиск
				if (viewIndex == _lastSearchedItemIndex)
				{
					_lastSearchedItem = default (TItem);
					_lastSearchedItemIndex = -1;
				}
			}

			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Replace, newItem, oldItem, viewIndex));

			// корректирует выбранный элемент
			if (this.CurrentPosition == viewIndex)
			{ // текущий элемент заменён
				OnCurrentChanging (new CurrentChangingEventArgs (false));
				SetCurrent (newItem, viewIndex);
				OnCurrentChanged ();
			}
		}

		private void ViewInsertOne (object item, int sourceIndex, int viewIndex)
		{
			// если последний поиск был неудачным, то после добавления тот же поиск может стать удачным, поэтому аннулируем последний поиск
			if (_lastSearchedItemIndex < 0)
			{
				_lastSearchedItem = default (TItem);
			}
			else
			{
				// если элемент вставляется ближе того, который был найден в последнем поиске, то результат последнего поиска станет дальше
				if (viewIndex <= _lastSearchedItemIndex)
				{
					_lastSearchedItemIndex++;
				}
			}

			_view2source.Insert (viewIndex, sourceIndex);
			// создаём дыру для вставляемого индекса, увеличивая на 1 все равные и большие
			// если нет сортировки то можно перебирать не всё, а только часть после вставляемой
			for (var i = (_sortingComparer != null) ? 0 : sourceIndex + 1; i < _source2view.Count; i++)
			{
				var idx = _source2view[i];
				if (idx >= viewIndex)
				{
					_source2view[i] = idx + 1;
				}
			}
			_source2view[sourceIndex] = viewIndex;
			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Add, item, viewIndex));

			// корректирует выбранный элемент
			// если вставка ДО выбранного то отодвигаем выбранный элемент на количество вставленных
			if (viewIndex <= this.CurrentPosition)
			{
				var newPosition = this.CurrentPosition + 1;
				var count = _view2source.Count;
				if (newPosition < _view2source.Count)
				{
					SetCurrent (this.CurrentItem, newPosition);
				}
				else
				{
					SetCurrent (null, count);
				}
			}
		}

		private void ViewRemoveOne (object item, int viewIndex)
		{
			// если элемент удаляется в той позиции, которая была найдена в последнем поиске, то аннулируем последний поиск
			if (viewIndex == _lastSearchedItemIndex)
			{
				_lastSearchedItem = default (TItem);
				_lastSearchedItemIndex = -1;
			}
			else
			{
				// если элемент удаляется ближе того, который был найден в последнем поиске, то результат последнего поиска станет ближе
				if (viewIndex < _lastSearchedItemIndex)
				{
					_lastSearchedItemIndex--;
				}
			}

			_view2source.RemoveAt (viewIndex);
			// схлопываем дыру удалённого индекса, уменьшая на 1 все равные и большие
			// если нет сортировки то можно перебирать не всё, а только часть после удалённой
			for (var i = 0; i < _source2view.Count; i++)
			{
				var idx = _source2view[i];
				if (idx >= viewIndex)
				{
					_source2view[i] = idx - 1;
				}
			}
			OnCollectionChanged (new NotifyCollectionChangedEventArgs (NotifyCollectionChangedAction.Remove, item, viewIndex));

			// корректирует выбранный элемент
			if (this.CurrentPosition == viewIndex)
			{ // выбранный элемент внутри удаляемого диапазона
				OnCurrentChanging (new CurrentChangingEventArgs (false));
				var newPosition = viewIndex;
				if (newPosition >= _view2source.Count)
				{
					newPosition = _view2source.Count - 1;
				}
				SetCurrent ((newPosition >= 0) ? GetItemAt (newPosition) : default (TItem), newPosition);
				OnCurrentChanged ();
			}
			else
			{ // если удаление ДО выбранного то придвигаем выбранный элемент на количество удалённых
				if (viewIndex < this.CurrentPosition)
				{
					var newPosition = this.CurrentPosition - 1;
					if (newPosition >= 0)
					{
						SetCurrent (this.CurrentItem, newPosition);
					}
					else
					{
						SetCurrent (null, -1);
					}
				}
			}
		}

		#endregion

		#region struct _ComparerByIndexes

		internal struct _ComparerByIndexes :
			IComparer<int>,
			IComparer
		{
			private readonly IReadOnlyList<TItem> _source;
			private readonly IComparer<TItem> _comparer;

			internal _ComparerByIndexes (IReadOnlyList<TItem> source, IComparer<TItem> comparer)
			{
				_source = source;
				_comparer = comparer;
			}

			int IComparer.Compare (object x, object y)
			{
				return Compare ((int)x, (int)y);
			}

			public int Compare (int x, int y)
			{
				if (x < 0)
				{
					throw new ArgumentOutOfRangeException (nameof (x));
				}
				if (y < 0)
				{
					throw new ArgumentOutOfRangeException (nameof (y));
				}
				Contract.EndContractBlock ();

				return _comparer.Compare (_source[x], _source[y]);
			}
		}

		#endregion

		#region struct _SourceWithMapEnumerator

		internal struct _SourceWithMapEnumerator : IEnumerator<TItem>, IDisposable, IEnumerator
		{
			private readonly CollectionView _source;

			private int _index;
			private TItem _currentElement;

			internal _SourceWithMapEnumerator (CollectionView source)
			{
				_source = source;
				_index = -1;
				_currentElement = default (TItem);
			}

			/// <summary>
			/// Перемещает перечислитель к следующему элементу строки.
			/// </summary>
			/// <returns>true, если перечислитель был успешно перемещен к следующему элементу;
			/// false, если перечислитель достиг конца.</returns>
			public bool MoveNext ()
			{
				if (_index == -2)
				{
					return false;
				}

				_index++;
				if (_index == _source.Count)
				{
					_index = -2;
					_currentElement = default (TItem);
					return false;
				}
				_currentElement = (TItem)_source.GetItemAt (_index);
				return true;
			}

			/// <summary>
			/// Получает текущий элемент перечислителя.
			/// </summary>
			public TItem Current
			{

				get
				{
					if (_index == -1)
					{
						throw new InvalidOperationException ("Can not get current element of enumeration because it not started.");
					}
					if (_index == -2)
					{
						throw new InvalidOperationException ("Can not get current element of enumeration because it already ended.");
					}
					return _currentElement;
				}
			}
			object IEnumerator.Current => this.Current;

			/// <summary>
			/// Возвращает перечислитель в исходное положение.
			/// </summary>
			public void Reset ()
			{
				_index = -1;
				_currentElement = default (TItem);
			}

			/// <summary>
			/// Освобождает занятые объектом ресурсы.
			/// </summary>
			public void Dispose ()
			{
				_index = -2;
				_currentElement = default (TItem);
			}
		}

		#endregion
	}
}
