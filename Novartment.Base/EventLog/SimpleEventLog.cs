using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.Extensions.Logging;
using Novartment.Base.Collections;
using Novartment.Base.Collections.Immutable;
using Novartment.Base.Text;

namespace Novartment.Base
{
	/// <summary>
	/// The in-memory event log suitable for multi-threaded, asynchronous use.
	/// Supports replacing (hiding) sensetive information.
	/// Supports property and collection change notification.
	/// </summary>
	public class SimpleEventLog :
		ILogger,
		IReadOnlyList<SimpleEventRecord>,
		IPatternsReplacer,
		IArrayDuplicableCollection<SimpleEventRecord>,
		INotifyCollectionChanged,
		INotifyPropertyChanged,
		IDisposable
	{
		private readonly ConcurrentList<SimpleEventRecord> _events = new ();
		private SingleLinkedListNode<Regex> _templatesToHide = null;
		private int _recordLimit = 1000;
		private bool _replacementEnabled;
		private LogLevel _logLevel = LogLevel.Trace;

		/// <summary>
		/// Initializes a new instance of the SimpleEventLog class.
		/// </summary>
		public SimpleEventLog ()
		{
			_replacementEnabled = true;
			this.ReplacementValue = "***";
			_events.CollectionChanged += EventsChanged;
		}

		/// <summary>Occurs when an event is added, removed, changed, moved, or the entire log is refreshed.</summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		/// <summary>Occurs when a property value changes.</summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Occurs when the configuration of the event log changes.
		/// </summary>
		public event EventHandler<EventArgs> LoggerReconfigured;

		/// <summary>
		/// Gets the number of log records.
		/// </summary>
		public int Count => _events.Count;

		/// <summary>
		/// Gets or sets a limit on the number of records stored in the log.
		/// Excess records are deleted when new ones are added.
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

				if (_recordLimit != value)
				{
					_recordLimit = value;
					OnPropertyChanged (new PropertyChangedEventArgs (nameof (this.RecordLimit)));
				}
			}
		}

		/// <summary>
		/// Gets or sets the level of detail of information recorded by the event log.
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
		/// Gets or sets whether the replacement is enabled.
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
		/// Gets or sets the string to be inserted in place of the encountered patterns.
		/// </summary>
		public string ReplacementValue { get; set; }

		/// <summary>
		/// Gets a record with the specified number.
		/// </summary>
		/// <param name="index">The zero-based index of the record to get.</param>
		public SimpleEventRecord this[int index] => _events[index];

		/// <summary>
		/// Clears the log.
		/// </summary>
		public void Clear ()
		{
			_events.Clear ();
			OnPropertyChanged (new PropertyChangedEventArgs (nameof (this.Count)));
			OnPropertyChanged (new PropertyChangedEventArgs ("Item[]"));
		}

		/// <summary>
		/// Adds a string to the list of patterns to replace.
		/// </summary>
		/// <param name="pattern">The string template for search and replace.</param>
		public void AddReplacementStringPattern (string pattern)
		{
			if (pattern == null)
			{
				throw new ArgumentNullException (nameof (pattern));
			}

			AddReplacementRegexPattern (Regex.Escape (pattern));
		}

		/// <summary>
		/// Adds a regular expression to the list of patterns to replace.
		/// </summary>
		/// <param name="pattern">The regular expression for search and replace.</param>
		public void AddReplacementRegexPattern (string pattern)
		{
			if (pattern == null)
			{
				throw new ArgumentNullException (nameof (pattern));
			}

			var newRegex = new Regex (pattern, RegexOptions.ExplicitCapture);

			SpinWait spinWait = default;
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
		/// Clears the list of patterns.
		/// </summary>
		public void ClearReplacementPatterns ()
		{
			_templatesToHide = null;
		}

		/// <summary>
		/// Releases all resources of the log.
		/// </summary>
		public void Dispose ()
		{
			_events.CollectionChanged -= EventsChanged;
			CollectionChanged = null;
			PropertyChanged = null;
			LoggerReconfigured = null;
			GC.SuppressFinalize (this);
		}

		/// <summary>
		/// Tries to get the first record in a log.
		/// </summary>
		/// <param name="item">
		/// When this method returns, the first record in a log, if log was not empty;
		/// otherwise, the default value for the type of the record.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <returns>True if the log was not empty; otherwise, False.</returns>
		public bool TryPeekFirst (out SimpleEventRecord item)
		{
			return _events.TryPeekFirst (out item);
		}

		/// <summary>
		/// Tries to get the last itrecordem in a log.
		/// </summary>
		/// <param name="item">
		/// When this method returns, the last record in a log, if log was not empty;
		/// otherwise, the default value for the type of the record.
		/// This parameter is passed uninitialized.
		/// </param>
		/// <returns>True if the log was not empty; otherwise, False.</returns>
		public bool TryPeekLast (out SimpleEventRecord item)
		{
			return _events.TryPeekLast (out item);
		}

		/// <summary>
		/// Copies the log to a one-dimensional array,
		/// starting at the specified index of the target array.
		/// </summary>
		/// <param name="array">The one-dimensional System.Array that is the destination of the records copied.</param>
		/// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
		public void CopyTo (SimpleEventRecord[] array, int arrayIndex)
		{
			_events.CopyTo (array, arrayIndex);
		}

		/// <summary>
		/// Returns an enumerator for the log.
		/// </summary>
		/// <returns>An enumerator for the log.</returns>
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
		/// Returns an enumerator for the list.
		/// </summary>
		/// <returns>An enumerator for the list.</returns>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}

		/// <summary>
		/// Calls the PropertyChanged event with the specified arguments.
		/// </summary>
		/// <param name="args">The arguments of PropertyChanged event.</param>
		protected virtual void OnPropertyChanged (PropertyChangedEventArgs args)
		{
			this.PropertyChanged?.Invoke (this, args);
		}

		/// <summary>
		/// Calls the LoggerReconfigured event.
		/// </summary>
		protected void OnLoggerReconfigured ()
		{
			LoggerReconfigured?.Invoke (this, EventArgs.Empty);
		}

		private void LogEvent (LogLevel verbosity, string message)
		{
			if (_replacementEnabled)
			{
				var enumerator = _templatesToHide.GetEnumerator ();
				while (enumerator.MoveNext ())
				{
					var template = enumerator.Current;
					if (message != null)
					{
						message = template.Replace (message, this.ReplacementValue);
					}
				}
			}

			var record = new SimpleEventRecord (verbosity, message);
			AddInternal (record);
		}

		private void AddInternal (SimpleEventRecord record)
		{
			while (_events.Count >= _recordLimit)
			{
				var isItemTaken = _events.TryTakeFirst (out _);
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
