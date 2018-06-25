using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using Novartment.Base.Collections.Immutable;

namespace Novartment.Base
{
	/// <summary>
	/// Провайдер-ретранслятор уведомлений прогресса,
	/// соблюдающий указанный минимальный интервал времени между отдельными уведомлениями.
	/// Задержанные уведомления одной категории затираются новыми.
	/// Группировка по категориями является необязательной, определяется по реализации параметром уведомлений интерфейса ICategory.
	/// </summary>
	/// <typeparam name="T">
	/// Тип параметра, передаваемого при уведомлении.
	/// Проверяется реализация ICategory для группировки.
	/// </typeparam>
	/// <remarks>
	/// Доставка всех уведомлений не гарантируется, старые могут быть пропущены в случае прихода новых.
	/// Можно быть уверенным только в передаче последнего уведомления,
	/// что является приемлемым для отображения изменений в пользовательском интерфейсе.
	/// </remarks>
	public sealed class CategoryTimeThrottledProgressProvider<T> :
		IProgress<T>,
		IDisposable
	{
		private readonly IProgress<T> _progress;
		private readonly object _dictionaryLocker = new object ();
		private readonly IComparer<int> _categoryComparer = Comparer<int>.Default;
		private readonly Func<Action, ITimer> _timerFactory;
		private readonly TimeSpan _intervalToSendProgressUpdates;
		private AvlBinarySearchTreeDictionaryNode<int, ReportEvent> _categoryPostponedData = null;
		private int _reportingInProgress = 0;
		private ITimer _stateTimer = null;

		/// <summary>
		/// Инициализирует новый экземпляр CategoryTimeThrottledProgressProvider
		/// с указанными делегатом уведомления и фабрикой создания таймера.
		/// </summary>
		/// <param name="progress">Объект, получающий уведомления о новых данных.</param>
		/// <param name="timerFactory">Фабрика создания таймера, периодически вызывающего указанное действие.</param>
		public CategoryTimeThrottledProgressProvider (IProgress<T> progress, Func<Action, ITimer> timerFactory)
			: this (progress, timerFactory, 100)
		{
		}

		/// <summary>
		/// Инициализирует новый экземпляр CategoryTimeThrottledProgressProvider
		/// с указанными делегатом уведомления, фабрикой создания таймера и интервалом выдачи уведомлений.
		/// </summary>
		/// <param name="progress">Объект, получающий уведомления о новых данных.</param>
		/// <param name="timerFactory">Фабрика создания таймера, периодически вызывающего указанное действие.</param>
		/// <param name="minimumInterval">Интервал выдачи уведомлений в миллисекундах.
		/// Для обновления пользовательского интерфейса используйте значение 100 миллисекунд.</param>
		public CategoryTimeThrottledProgressProvider (
			IProgress<T> progress,
			Func<Action, ITimer> timerFactory,
			int minimumInterval)
		{
			if (progress == null)
			{
				throw new ArgumentNullException (nameof (progress));
			}

			if (timerFactory == null)
			{
				throw new ArgumentNullException (nameof (timerFactory));
			}

			if (minimumInterval < 1)
			{
				throw new ArgumentOutOfRangeException (nameof (minimumInterval));
			}

			Contract.EndContractBlock ();

			_progress = progress;
			_intervalToSendProgressUpdates = new TimeSpan (0, 0, 0, 0, minimumInterval);
			_timerFactory = timerFactory;
		}

		/// <summary>
		/// Освобождает занятые объектом ресурсы, прекращает приём уведомлений.
		/// </summary>
		public void Dispose ()
		{
			_stateTimer?.Dispose ();
		}

		/// <summary>
		/// Обрабатывает новые данные от поставщика.
		/// </summary>
		/// <param name="value">Значение параметра для уведомления.</param>
		public void Report (T value)
		{
			var category = (!(value is ICategory categorizedData)) ? 0 : categorizedData.Category;
			bool needReport = false;
			lock (_dictionaryLocker)
			{
				var found = _categoryPostponedData.TryGetItem (category, _categoryComparer, out ReportEvent reportEvent);
				if (found && ((DateTime.Now - reportEvent.Time) < _intervalToSendProgressUpdates))
				{
					// минимальный интервал от прошлого уведомления такой категории ещё не прошёл, поэтому заменяем отложенное ранее
					reportEvent.Data = value;
					reportEvent.IsDataPresent = true;

					if (_stateTimer == null)
					{
						_stateTimer = _timerFactory.Invoke (ReportPostponedData);
						_stateTimer.Interval = _intervalToSendProgressUpdates;
						_stateTimer.Start ();
					}
				}
				else
				{
					if (found)
					{
						reportEvent.Time = DateTime.Now;
						reportEvent.Data = default;
						reportEvent.IsDataPresent = false;
					}
					else
					{
						// уведомлений такой категории не было отложено, либо уже пришло время их отправлять
						_categoryPostponedData = _categoryPostponedData.SetValue (
							category,
							new ReportEvent { Time = DateTime.Now },
							_categoryComparer);
					}

					needReport = true;
				}
			}

			if (needReport)
			{
				_progress.Report (value);
			}
		}

		private void ReportPostponedData ()
		{
			// защищаемся от реитерации
			var oldValue = Interlocked.CompareExchange (ref _reportingInProgress, 1, 0);
			if (oldValue != 0)
			{
				return;
			}

			// перебираем все категории отложенных уведомлений
			var enumerator = _categoryPostponedData.GetEnumerator ();
			while (enumerator.MoveNext ())
			{
				bool needReport = false;
				T data;
				lock (_dictionaryLocker)
				{
					var reportEvent = enumerator.Current.Value;
					data = reportEvent.Data;
					if (reportEvent.IsDataPresent && (DateTime.Now - reportEvent.Time) >= _intervalToSendProgressUpdates)
					{
						// если уведомление было отложено давно, то отсылаем его
						reportEvent.Time = DateTime.Now;
						reportEvent.Data = default;
						reportEvent.IsDataPresent = false;
						needReport = true;
					}
				}

				if (needReport)
				{
					_progress.Report (data);
				}
			}

			_reportingInProgress = 0;
		}

		private class ReportEvent
		{
			internal DateTime Time { get; set; }

			internal bool IsDataPresent { get; set; }

			internal T Data { get; set; }
		}
	}
}
