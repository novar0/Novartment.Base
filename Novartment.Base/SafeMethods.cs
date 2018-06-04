using System;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Novartment.Base
{
	/// <summary>
	/// Методы для использования интерфейсов IDisposable, PropertyChangedEventHandler и NotifyCollectionChangedEventHandler,
	/// которые могут быть не реализованы используемым объектом.
	/// </summary>
	public static class SafeMethods
	{
		/// <summary>Вызывает метод Dispose() если объект реализует интерфейс System.IDisposable.</summary>
		/// <param name="disposableValue">Объект, который необходимо освободить.</param>
		/// <returns>True если был вызван метод освобождения объекта.</returns>
		public static bool TryDispose (object disposableValue)
		{
			var disposable = disposableValue as IDisposable;
			var success = disposable != null;
			if (success)
			{
				disposable.Dispose ();
			}

			return success;
		}

		/// <summary>
		/// Добавляет обработчик события CollectionChanged
		/// если объект реализует интерфейс System.Collections.Specialized.INotifyCollectionChanged.</summary>
		/// <param name="collection">Объект, на событие которого необходимо добавить обработчик.</param>
		/// <param name="handler">Обработчик события.</param>
		/// <returns>True если обработчик был добавлен к объекту.</returns>
		public static bool TryAddCollectionChangedHandler (object collection, NotifyCollectionChangedEventHandler handler)
		{
			if (!(collection is INotifyCollectionChanged ncc))
			{
				return false;
			}

			ncc.CollectionChanged += handler;
			return true;
		}

		/// <summary>
		/// Удаляет обработчик события CollectionChanged
		/// если объект реализует интерфейс System.Collections.Specialized.INotifyCollectionChanged.</summary>
		/// <param name="collection">Объект, от события которого необходимо удалить обработчик.</param>
		/// <param name="handler">Обработчик события.</param>
		/// <returns>True если обработчик был удалён от объекта.</returns>
		public static bool TryRemoveCollectionChangedHandler (object collection, NotifyCollectionChangedEventHandler handler)
		{
			if (!(collection is INotifyCollectionChanged ncc))
			{
				return false;
			}

			ncc.CollectionChanged -= handler;
			return true;
		}

		/// <summary>
		/// Добавляет обработчик события PropertyChanged
		/// если объект реализует интерфейс System.ComponentModel.INotifyPropertyChanged.</summary>
		/// <param name="observable">Объект, на событие которого необходимо добавить обработчик.</param>
		/// <param name="handler">Обработчик события.</param>
		/// <returns>True если обработчик был добавлен к объекту.</returns>
		public static bool TryAddPropertyChangedHandler (object observable, PropertyChangedEventHandler handler)
		{
			if (!(observable is INotifyPropertyChanged npc))
			{
				return false;
			}

			npc.PropertyChanged += handler;
			return true;
		}

		/// <summary>
		/// Удаляет обработчик события PropertyChanged
		/// если объект реализует интерфейс System.ComponentModel.INotifyPropertyChanged.</summary>
		/// <param name="observable">Объект, от события которого необходимо удалить обработчик.</param>
		/// <param name="handler">Обработчик события.</param>
		/// <returns>True если обработчик был удалён от объекта.</returns>
		public static bool TryRemovePropertyChangedHandler (object observable, PropertyChangedEventHandler handler)
		{
			if (!(observable is INotifyPropertyChanged npc))
			{
				return false;
			}

			npc.PropertyChanged -= handler;
			return true;
		}
	}
}
