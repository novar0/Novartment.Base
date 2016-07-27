using System;
using static System.Linq.Enumerable;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using Novartment.Base.Collections;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Управляемая командами повторяемая задача для привязки к элементам интерфейса (FrameworkElement)
	/// чтобы передавать задаче контекст элемента, для которого вызвана команда,
	/// а также источник списка элементов, частью которого является этот элемент и
	/// коллекцию выбранных в списке элементов.
	/// </summary>
	/// <typeparam name="T">Тип элементов коллекции.</typeparam>
	public class CollectionContextCommandedRepeatableTask<T> : CommandedRepeatableTask
	{
		/// <summary>
		/// Инициализирует новый экземпляр CollectionContextCommandedRepeatableTask на основе указанной фабрики по производству задач.
		/// </summary>
		/// <param name="taskFactory">Функция, создающая задачу. Будет вызвана при старте.
		/// В функцию будут переданы контекст элемента, для которого вызвана команда,
		/// а также список, частью которого является этот элемент и список выбранных в списке элементов.
		/// Возвращённая функцией задача должна быть уже запущена.</param>
		[SuppressMessage ("Microsoft.Design",
			"CA1006:DoNotNestGenericTypesInMemberSignatures",
			Justification = "The caller doesn't have to cope with nested generics, he is just passing a lambda expression.")]
		public CollectionContextCommandedRepeatableTask (Func<ContextCollectionData<T>, CancellationToken, Task> taskFactory)
			: base (taskFactory.ParameterAsObject)
		{
		}

		/// <summary>
		/// Инициализирует новый экземпляр CollectionContextCommandedRepeatableTask на основе указанного делегата и планировщика задач.
		/// </summary>
		/// <param name="taskAction">
		/// Делегат, который будут вызывать запускаемые задачи.
		/// В делегат будут переданы контекст элемента, для которого вызвана команда,
		/// а также список, частью которого является этот элемент и список выбранных в списке элементов.
		/// </param>
		/// <param name="taskScheduler">Планировщик, в котором будут выполняться запускаемые задачи.</param>
		[SuppressMessage (
			"Microsoft.Design",
			"CA1006:DoNotNestGenericTypesInMemberSignatures",
			Justification = "The caller doesn't have to cope with nested generics, he is just passing a lambda expression.")]
		public CollectionContextCommandedRepeatableTask (
			Action<ContextCollectionData<T>, CancellationToken> taskAction,
			TaskScheduler taskScheduler)
			: base (taskAction.ParameterAsObject, taskScheduler)
		{
		}

		/// <summary>
		/// Инициализирует новый экземпляр CollectionContextCommandedRepeatableTask на основе указанной фабрики по производству задач и предыдущей задачи в цепи.
		/// </summary>
		/// <param name="taskFactory">Функция, создающая задачу. Будет вызвана при старте.
		/// В функцию будут переданы контекст элемента, для которого вызвана команда,
		/// а также список, частью которого является этот элемент и список выбранных в списке элементов.
		/// Возвращённая функцией задача должна быть уже запущена.</param>
		/// <param name="previousTask">Предыдущая задача, в цепь команд которой будут добавлены команды создаваемой задачи.</param>
		[SuppressMessage ("Microsoft.Design",
			"CA1006:DoNotNestGenericTypesInMemberSignatures",
			Justification = "The caller doesn't have to cope with nested generics, he is just passing a lambda expression.")]
		protected CollectionContextCommandedRepeatableTask (
			Func<ContextCollectionData<T>, CancellationToken, Task> taskFactory,
			CommandedRepeatableTask previousTask)
			: base (taskFactory.ParameterAsObject, previousTask)
		{
		}

		/// <summary>
		/// Инициализирует новый экземпляр CollectionContextCommandedRepeatableTask на основе указанного делегата, планировщика задач и предыдущей задачи в цепи.
		/// </summary>
		/// <param name="taskAction">
		/// Делегат, который будут вызывать запускаемые задачи.
		/// В делегат будут переданы контекст элемента, для которого вызвана команда,
		/// а также список, частью которого является этот элемент и список выбранных в списке элементов.
		/// </param>
		/// <param name="taskScheduler">Планировщик, в котором будут выполняться запускаемые задачи.</param>
		/// <param name="previousTask">Предыдущая задача, в цепь команд которой будут добавлены команды создаваемой задачи.</param>
		[SuppressMessage ("Microsoft.Design",
			"CA1006:DoNotNestGenericTypesInMemberSignatures",
			Justification = "The caller doesn't have to cope with nested generics, he is just passing a lambda expression.")]
		protected CollectionContextCommandedRepeatableTask (
			Action<ContextCollectionData<T>, CancellationToken> taskAction,
			TaskScheduler taskScheduler,
			CommandedRepeatableTask previousTask)
			: base (taskAction.ParameterAsObject, taskScheduler, previousTask)
		{
		}

		/// <summary>
		/// Запускает задачу. Преобразовывает объект-состояние.
		/// </summary>
		/// <param name="state">Объект-состояние, передаваемый в запускаемую задачу.</param>
		protected override void StartInternal (object state)
		{
			var frameworkElement = state as FrameworkElement;
			if (frameworkElement != null)
			{
				var itemsControl = frameworkElement.FindVisualAncestor<ItemsControl> ();

				IEnumerable list = null;
				if (itemsControl != null)
				{
					var view = itemsControl?.ItemsSource as ICollectionView;
					list = view?.SourceCollection ?? itemsControl.ItemsSource;
				}

				// формируем неизменяемое множество выбранных элементов, потому что:
				// 1. исходный список выбранных меняется в любое время
				// 2. обращаться к исходному списку можно только здесь, в теле задачи (другом потоке) - нельзя
				// 3. исходный список не содержит ссылок на коллекцию-источник (индексов), что затрудняет поиск соответствий
				var set = new HashSet<T> ();
				var selectedItems = itemsControl.GetValue (ListBox.SelectedItemsProperty) as IEnumerable;
				if (selectedItems != null)
				{
					foreach (var item in selectedItems)
					{
						set.Add ((T)item);
					}
				}

				var startData = new ContextCollectionData<T> (
					frameworkElement.DataContext,
					list.Cast<T> (),
					new ReadOnlySet (set));
				base.StartInternal (startData);
			}
		}

		// простейший непосредственный декоратор
		private class ReadOnlySet : IReadOnlyFiniteSet<T>
		{
			private readonly ISet<T> _source;
			internal ReadOnlySet (ISet<T> source) { _source = source; }
			public bool Contains (T item) { return _source.Contains (item); }
			public int Count => _source.Count;
			public IEnumerator<T> GetEnumerator () { return _source.GetEnumerator (); }
			IEnumerator IEnumerable.GetEnumerator () { return _source.GetEnumerator (); }
		}
	}
}
