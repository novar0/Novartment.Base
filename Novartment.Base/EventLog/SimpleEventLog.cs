using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Extensions.Logging;
using Novartment.Base.Collections;
using Novartment.Base.Collections.Immutable;
using Novartment.Base.Text;

namespace Novartment.Base
{
	/// <summary>
	/// Журнал событий, пригодный для многопоточного асинхронного использования.
	/// </summary>
	[SuppressMessage (
		"Microsoft.Naming",
		"CA1710:IdentifiersShouldHaveCorrectSuffix",
		Justification = "Implemented interfaces has no association with class name.")]
	public class SimpleEventLog :
		ILogger,
		IReadOnlyList<SimpleEventRecord>,
		IPatternsReplacer,
		IArrayDuplicableCollection<SimpleEventRecord>,
		INotifyCollectionChanged,
		INotifyPropertyChanged
	{
		private readonly ConcurrentList<SimpleEventRecord> _events = new ConcurrentList<SimpleEventRecord> ();
		private SingleLinkedListNode<Regex> _templatesToHide = null;
		private int _recordLimit = 1000;
		private bool _replacementEnabled;
		private LogLevel _logLevel = LogLevel.Trace;

		/// <summary>
		/// Инициализирует новый экземпляр SimpleEventLog.
		/// </summary>
		public SimpleEventLog ()
		{
			_replacementEnabled = true;
			ReplacementValue = "***";
			_events.CollectionChanged += EventsChanged;
		}

		/// <summary>Происходит, когда коллекция изменяется.</summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		/// <summary>Происходит, когда свойство коллекции изменяется.</summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Происходит когда изменяется конфигурация журнала событий.
		/// </summary>
		public event EventHandler<EventArgs> LoggerReconfigured;

		/// <summary>
		/// Получает количество записей в журнале.
		/// </summary>
		public int Count => _events.Count;

		/// <summary>
		/// Получает или устанавливает ограничение на количество записей, хранимых в журнале.
		/// Лишние старые записи удаляются когда добавляются новые.
		/// </summary>
		public int RecordLimit
		{
			get => _recordLimit;
			set
			{
				if (value <= 0)
				{
					throw new ArgumentOutOfRangeException (nameof (value));
				}

				Contract.EndContractBlock ();

				if (_recordLimit != value)
				{
					_recordLimit = value;
					OnPropertyChanged (new PropertyChangedEventArgs (nameof (this.RecordLimit)));
				}
			}
		}

		/// <summary>
		/// Получает или устанавливает уровень детализации информации, регистрируемой журналом событий.
		/// </summary>
		public LogLevel LoggingLevel
		{
			get => _logLevel;
			set
			{
				if (value != _logLevel)
				{
					_logLevel = value;
					OnPropertyChanged (new PropertyChangedEventArgs (nameof (this.LoggingLevel)));
					OnLoggerReconfigured ();
				}
			}
		}

		/// <summary>
		/// Получает или устанавливает признак включения замены.
		/// </summary>
		public bool ReplacementEnabled
		{
			get => _replacementEnabled;
			set
			{
				if (_replacementEnabled != value)
				{
					_replacementEnabled = value;
					OnPropertyChanged (new PropertyChangedEventArgs (nameof (this.ReplacementEnabled)));
				}
			}
		}

		/// <summary>
		/// Получает или устанавливает строку которая будет вставлена вместо встреченных шаблонов.
		/// </summary>
		public string ReplacementValue { get; set; }

		/// <summary>
		/// Получает запись с указанным номером.
		/// </summary>
		/// <param name="index">Номер записи.</param>
		public SimpleEventRecord this[int index] => _events[index];

		/// <summary>
		/// Очищает журнал.
		/// </summary>
		public void Clear ()
		{
			_events.Clear ();
			OnPropertyChanged (new PropertyChangedEventArgs (nameof (this.Count)));
			OnPropertyChanged (new PropertyChangedEventArgs ("Item[]"));
		}

		/// <summary>
		/// Добавляет строку в список заменяемых шаблонов.
		/// </summary>
		/// <param name="pattern">Строка-шаблон для поиска и замены.</param>
		public void AddReplacementStringPattern (string pattern)
		{
			if (pattern == null)
			{
				throw new ArgumentNullException (nameof (pattern));
			}

			Contract.EndContractBlock ();

			AddReplacementRegexPattern (Regex.Escape (pattern));
		}

		/// <summary>
		/// Добавляет регулярное выражение в список заменяемых шаблонов.
		/// </summary>
		/// <param name="pattern">Строка-шаблон для поиска и замены.</param>
		public void AddReplacementRegexPattern (string pattern)
		{
			if (pattern == null)
			{
				throw new ArgumentNullException (nameof (pattern));
			}

			Contract.EndContractBlock ();

			var newRegex = new Regex (pattern, RegexOptions.ExplicitCapture);

			var spinWait = default (SpinWait);
			while (true)
			{
				var state1 = _templatesToHide;
				var newState = state1.AddItem (newRegex);

				// заменяем состояние если оно не изменилось с момента вызова
				var state2 = Interlocked.CompareExchange (ref _templatesToHide, newState, state1);
				if (state1 == state2)
				{
					return;
				}

				// состояние изменилось за время вызова, поэтому повторим попытку после паузы
				spinWait.SpinOnce ();
			}
		}

		/// <summary>
		/// Очищает список заменяемых шаблонов.
		/// </summary>
		public void ClearReplacementPatterns ()
		{
			_templatesToHide = null;
		}

		/// <summary>
		/// Освобождает все ресурсы, занятые объектом.
		/// </summary>
		public void Dispose ()
		{
			_events.CollectionChanged -= EventsChanged;
			CollectionChanged = null;
			PropertyChanged = null;
			LoggerReconfigured = null;
		}

		/// <summary>
		/// Пытается получить первый элемент журнала.
		/// </summary>
		/// <param name="item">Значение первого элемента если он успешно получен,
		/// либо значение по умолчанию если нет.</param>
		/// <returns>True если первый элемент успешно получен, False если нет.</returns>
		public bool TryPeekFirst (out SimpleEventRecord item)
		{
			return _events.TryPeekFirst (out item);
		}

		/// <summary>
		/// Пытается получить последний элемент журнала.
		/// </summary>
		/// <param name="item">Значение последнего элемента если он успешно получен,
		/// либо значение по умолчанию если нет.</param>
		/// <returns>True если последний элемент успешно получен, False если нет.</returns>
		public bool TryPeekLast (out SimpleEventRecord item)
		{
			return _events.TryPeekLast (out item);
		}

		/// <summary>
		/// Копирует элементы журнала в указанный массив,
		/// начиная с указанной позиции конечного массива.
		/// </summary>
		/// <param name="array">Массив, в который копируются элементы журнала.</param>
		/// <param name="arrayIndex">Отсчитываемая от нуля позиция в массиве array, указывающий начало копирования.</param>
		public void CopyTo (SimpleEventRecord[] array, int arrayIndex)
		{
			_events.CopyTo (array, arrayIndex);
		}

		/// <summary>
		/// Получает перечислитель элементов списка.
		/// </summary>
		/// <returns>Перечислитель элементов списка.</returns>
		public IEnumerator<SimpleEventRecord> GetEnumerator ()
		{
			return _events.GetEnumerator ();
		}

		/// <summary>
		/// Writes a log entry.
		/// </summary>
		/// <typeparam name="TState">Type of state.</typeparam>
		/// <param name="logLevel">Entry will be written on this level.</param>
		/// <param name="eventId">Id of the event.</param>
		/// <param name="state">The entry to be written. Can be also an object.</param>
		/// <param name="exception">The exception related to this entry.</param>
		/// <param name="formatter">Function to create a string message of the state and exception.</param>
		public void Log<TState> (LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
		{
			if (formatter == null)
			{
				throw new ArgumentNullException (nameof (formatter));
			}

			Contract.EndContractBlock ();

			var message = formatter.Invoke (state, exception);

			LogEvent (logLevel, message);
		}

		/// <summary>
		/// Checks if the given logLevel is enabled.
		/// </summary>
		/// <param name="logLevel">level to be checked.</param>
		/// <returns>true if enabled.</returns>
		public bool IsEnabled (LogLevel logLevel) => _logLevel <= logLevel;

		/// <summary>
		/// Begins a logical operation scope.
		/// </summary>
		/// <typeparam name="TState">Type of state.</typeparam>
		/// <param name="state">The identifier for the scope.</param>
		/// <returns>An IDisposable that ends the logical operation scope on dispose.</returns>
		public IDisposable BeginScope<TState> (TState state) => null;

		/// <summary>
		/// Получает перечислитель элементов списка.
		/// </summary>
		/// <returns>Перечислитель элементов списка.</returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		/// <summary>
		/// Вызывает событие PropertyChanged с указанными аргументами.
		/// </summary>
		/// <param name="args">Аргументы события PropertyChanged.</param>
		protected virtual void OnPropertyChanged (PropertyChangedEventArgs args)
		{
			this.PropertyChanged?.Invoke (this, args);
		}

		/// <summary>
		/// Вызывает событие LoggerReconfigured.
		/// </summary>
		protected void OnLoggerReconfigured ()
		{
			LoggerReconfigured?.Invoke (this, EventArgs.Empty);
		}

		private void LogEvent (LogLevel verbosity, string message)
		{
			var enumerator = _templatesToHide.GetEnumerator ();
			while (enumerator.MoveNext ())
			{
				var template = enumerator.Current;
				if (message != null)
				{
					message = template.Replace (message, ReplacementValue);
				}
			}

			var record = new SimpleEventRecord (verbosity, message);
			AddInternal (record);
		}

		private void AddInternal (SimpleEventRecord record)
		{
			while (_events.Count >= _recordLimit)
			{
				var isItemTaken = _events.TryTakeFirst (out SimpleEventRecord removedItem);
				if (!isItemTaken)
				{
					break;
				}

				OnPropertyChanged (new PropertyChangedEventArgs (nameof (this.Count)));
				OnPropertyChanged (new PropertyChangedEventArgs ("Item[]"));
			}

			_events.Add (record);
			OnPropertyChanged (new PropertyChangedEventArgs (nameof (this.Count)));
			OnPropertyChanged (new PropertyChangedEventArgs ("Item[]"));
		}

		private void EventsChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			this.CollectionChanged?.Invoke (this, e);
		}
	}
}
