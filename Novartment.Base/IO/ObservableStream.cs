using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.Collections.Immutable;

namespace Novartment.Base.IO
{
	/// <summary>
	/// Поток-обёртка с уведомлением о текущей позиции.
	/// </summary>
	[SuppressMessage (
		"Microsoft.Naming",
		"CA1711:IdentifiersShouldNotHaveIncorrectSuffix",
		Justification = "'Stream' suffix intended because of base type is System.IO.Stream.")]
	public class ObservableStream : Stream,
		IObservable<FileStreamStatus>
	{
		private readonly float _minPercentToReport = 0.1F; // минимальное изменение позиции (в % от размера файла) которое надо рапортовать
		private readonly Stream _stream;
		private readonly object _state;
		private long _previousPosition = long.MinValue;
		private long _length; // тут кэшируем размер потока чтобы не вызывать дополнительного I/O
		private AvlBinarySearchHashTreeNode<IObserver<FileStreamStatus>> _observers;

		/// <summary>
		/// Инициализирует новый экземпляр ObservableStream на основе указанного потока.
		/// Опционально можно указать объект-состояние, который будет передан вместе с данными уведомления.
		/// </summary>
		/// <param name="stream">Исходный поток, на основе которого будет создана обёртка.</param>
		/// <param name="state">Объект-состояние, который будет передаваться вместе с данными уведомления.</param>
		[SuppressMessage (
		"Microsoft.Design",
			"CA1026:DefaultParametersShouldNotBeUsed",
			Justification = "Parameter have clear right 'default' value and there is no plausible reason why the default might need to change.")]
		public ObservableStream (Stream stream, object state = null)
		{
			if (stream == null)
			{
				throw new ArgumentNullException (nameof (stream));
			}

			if (!stream.CanSeek)
			{
				throw new ArgumentOutOfRangeException (nameof (stream));
			}

			Contract.EndContractBlock ();

			_stream = stream;
			_state = state;
			_length = stream.Length;
			ReportProgressInternal ();
		}

		/// <summary>Получает значение, показывающее, поддерживает ли поток возможность чтения.</summary>
		public override bool CanRead => _stream.CanRead;

		/// <summary>Получает значение, которое показывает, поддерживается ли в потоке возможность поиска.</summary>
		public override bool CanSeek => _stream.CanSeek;

		/// <summary>Получает значение, которое показывает, поддерживает ли поток возможность записи.</summary>
		public override bool CanWrite => _stream.CanWrite;

		/// <summary>Получает длину потока в байтах.</summary>
		public override long Length => _stream.Length;

		/// <summary>Получает значение, которое показывает, может ли для данного потока истечь время ожидания.</summary>
		public override bool CanTimeout => _stream.CanTimeout;

		/// <summary>Получает или задает позицию в потоке.</summary>
		public override long Position
		{
			get => _stream.Position;
			set
			{
				_stream.Position = value;
				_length = _stream.Length;
				ReportProgressInternal();
			}
		}

		/// <summary>
		/// Получает или задает значение в миллисекундах, определяющее период,
		/// в течение которого поток будет пытаться выполнить операцию чтения, прежде чем истечет время ожидания.
		/// </summary>
		public override int ReadTimeout
		{
			get => _stream.ReadTimeout;

			set
			{
				_stream.ReadTimeout = value;
			}
		}

		/// <summary>
		/// Получает или задает значение в миллисекундах, определяющее период,
		/// в течение которого поток будет пытаться выполнить операцию записи, прежде чем истечет время ожидания.
		/// </summary>
		public override int WriteTimeout
		{
			get => _stream.WriteTimeout;

			set
			{
				_stream.WriteTimeout = value;
			}
		}

		/// <summary>
		/// Очищает все буферы данного потока и вызывает запись данных буферов в базовое устройство.
		/// </summary>
		public override void Flush () => _stream.Flush ();

		/// <summary>
		/// Асинхронно очищает все буферы для этого потока и вызывает запись всех буферизованных данных в базовое устройство.
		/// </summary>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая асинхронную операцию очистки.</returns>
		public override Task FlushAsync (CancellationToken cancellationToken)
		{
			return _stream.FlushAsync (cancellationToken);
		}

		/// <summary>
		/// Асинхронно считывает байты из потока и записывает их в другой поток, используя указанный размер буфера и токен отмены.
		/// </summary>
		/// <param name="destination">Поток, в который будет скопировано содержимое текущего потока.</param>
		/// <param name="bufferSize">Размер (в байтах) буфера. Это значение должно быть больше нуля.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая асинхронную операцию копирования.</returns>
		public override Task CopyToAsync (Stream destination, int bufferSize, CancellationToken cancellationToken)
		{
			return _stream.CopyToAsync (destination, bufferSize, cancellationToken);
		}

		/// <summary>
		/// Задает длину потока.
		/// </summary>
		/// <param name="value">Необходимая длина потока в байтах.</param>
		public override void SetLength (long value)
		{
			_stream.SetLength (value);
			_length = value;
			ReportProgressInternal ();
		}

		/// <summary>
		/// Задает позицию в потоке.
		/// </summary>
		/// <param name="offset">Смещение в байтах относительно параметра origin.</param>
		/// <param name="origin">определяет точку ссылки, которая используется для получения новой позиции.</param>
		/// <returns>Новая позиция в текущем потоке.</returns>
		public override long Seek (long offset, SeekOrigin origin)
		{
			var result = _stream.Seek (offset, origin);
			_length = _stream.Length;
			ReportProgressInternal ();
			return result;
		}

		/// <summary>
		/// Считывает последовательность байтов из потока и перемещает позицию в потоке на число считанных байтов.
		/// </summary>
		/// <param name="buffer">Буфер, в который записываются данные.</param>
		/// <param name="offset">Смещение байтов в buffer, с которого начинается запись данных из потока.</param>
		/// <param name="count">Максимальное число байтов, предназначенных для чтения.</param>
		/// <returns>Общее количество байтов, считанных в буфер.Это число может быть меньше количества запрошенных байтов, если столько байтов в настоящее время недоступно, а также равняться нулю (0), если был достигнут конец потока.</returns>
		public override int Read (byte[] buffer, int offset, int count)
		{
			var result = _stream.Read (buffer, offset, count);
			ReportProgressInternal ();
			return result;
		}

		/// <summary>
		/// Считывает байт из потока и перемещает позицию в потоке на один байт или возвращает -1, если достигнут конец потока.
		/// </summary>
		/// <returns>Байт без знака, приведенный к Int32, или значение -1, если достигнут конец потока.</returns>
		public override int ReadByte ()
		{
			var result = _stream.ReadByte ();
			ReportProgressInternal ();
			return result;
		}

		/// <summary>
		/// Асинхронно считывает последовательность байтов из потока,
		/// перемещает позицию в потоке на число считанных байтов и отслеживает запросы отмены.
		/// </summary>
		/// <param name="buffer">Буфер, в который записываются данные.</param>
		/// <param name="offset">Смещение байтов в buffer, с которого начинается запись данных из потока.</param>
		/// <param name="count">Максимальное число байтов, предназначенных для чтения.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, результатом которой является общее число байтов, считанных в буфер.
		/// Значение результата может быть меньше запрошенного числа байтов,
		/// если число доступных в данный момент байтов меньше запрошенного числа,
		/// или результат может быть равен 0 (нулю), если был достигнут конец потока.</returns>
		public override Task<int> ReadAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			var task = _stream.ReadAsync (buffer, offset, count, cancellationToken);
			task.ContinueWith (this.ReportProgressInternal1, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
			return task;
		}

		/// <summary>
		/// Записывает последовательность байтов в поток и перемещает позицию в нём вперед на число записанных байтов.
		/// </summary>
		/// <param name="buffer">Буфер, из которого записываются данные.</param>
		/// <param name="offset">Смещение байтов (начиная с нуля) в buffer, с которого начинается копирование байтов в поток.</param>
		/// <param name="count">Максимальное число байтов для записи.</param>
		public override void Write (byte[] buffer, int offset, int count)
		{
			_stream.Write (buffer, offset, count);
			_length = _stream.Length;
			ReportProgressInternal ();
		}

		/// <summary>
		/// Записывает байт в текущее положение в потоке и перемещает позицию в потоке вперед на один байт.
		/// </summary>
		/// <param name="value">Байт, записываемый в поток.</param>
		public override void WriteByte (byte value)
		{
			_stream.WriteByte (value);
			_length = _stream.Length;
			ReportProgressInternal ();
		}

		/// <summary>
		/// Асинхронно записывает последовательность байтов в поток,
		/// перемещает текущую позицию внутри потока на число записанных байтов и отслеживает запросы отмены.
		/// </summary>
		/// <param name="buffer">Буфер, из которого записываются данные.</param>
		/// <param name="offset">Смещение байтов (начиная с нуля) в buffer, с которого начинается копирование байтов в поток.</param>
		/// <param name="count">Максимальное число байтов для записи.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая асинхронную операцию записи.</returns>
		public override Task WriteAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			var task = _stream.WriteAsync (buffer, offset, count, cancellationToken);
			task.ContinueWith (this.ReportProgressInternal2, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
			return task;
		}

		/// <summary>
		/// Уведомляет поставщика о том, что наблюдатель должен получать уведомления.
		/// </summary>
		/// <param name="observer">Объект, который должен получать уведомления.</param>
		/// <returns>Ссылка на интерфейс, которая позволяет наблюдателям прекратить получение уведомлений до того,
		/// как поставщик закончил отправлять их.</returns>
		public IDisposable Subscribe (IObserver<FileStreamStatus> observer)
		{
			if (observer == null)
			{
				throw new ArgumentNullException (nameof (observer));
			}

			Contract.EndContractBlock ();

			var spinWait = default (SpinWait);
			while (true)
			{
				var state1 = _observers;
				var newState = _observers.AddItem (observer, ReferenceEqualityComparer.Default);

				// заменяем состояние если оно не изменилось с момента вызова
				var state2 = Interlocked.CompareExchange (ref _observers, newState, state1);
				if (state1 == state2)
				{
					return DisposeAction.Create (this.UnSubscribe, observer);
				}

				// состояние изменилось за время вызова, поэтому повторим попытку после паузы
				spinWait.SpinOnce ();
			}
		}

		/// <summary>
		/// Освобождает неуправляемые ресурсы, используемые объектом, а при необходимости освобождает также управляемые ресурсы.
		/// </summary>
		/// <param name="disposing">Значение true позволяет освободить управляемые и неуправляемые ресурсы;
		/// значение false позволяет освободить только неуправляемые ресурсы.</param>
		protected override void Dispose (bool disposing)
		{
			_observers = null;
			_stream.Dispose ();
			base.Dispose (disposing);
		}

		/// <summary>
		/// Вызывает уведомление наблюдателей о событии с указанным параметром.
		/// </summary>
		/// <param name="value">Значение параметра для уведомления.</param>
		protected void ObserversNotifyNext (FileStreamStatus value)
		{
			using (var enumerator = _observers.GetEnumerator ())
			{
				while (enumerator.MoveNext ())
				{
					enumerator.Current.OnNext (value);
				}
			}
		}

		private void ReportProgressInternal1 (Task task)
		{
			ReportProgressInternal ();
		}

		private void ReportProgressInternal2 (Task task)
		{
			_length = _stream.Length;
			ReportProgressInternal ();
		}

		private void ReportProgressInternal ()
		{
			var current = _stream.Position;
			var max = _length; // если тут брать _stream.Length то это вызовет дополнительный I/O-вызов
			if (current == _previousPosition)
			{
				return; // рапортовать нечего, позиция не изменилась
			}

			if (_previousPosition < 0)
			{
				// ещё ни разу не рапортовали
				_previousPosition = current;
				ObserversNotifyNext (new FileStreamStatus (current, max, _state));
				return;
			}

			var deltaAbsolute = Math.Abs (current - _previousPosition);

			if ((float)max >= (100.0F / _minPercentToReport))
			{
				// проверка на процент изменения нужна только для достаточно крупных файлов
				var deltaRelativePercent = ((float)deltaAbsolute / (float)max) * 100.0F;
				if (deltaRelativePercent < _minPercentToReport)
				{
					// изменение слишком мало чтобы рапортовать
					return;
				}
			}

			_previousPosition = current;
			ObserversNotifyNext (new FileStreamStatus (current, max, _state));
		}

		private void UnSubscribe (IObserver<FileStreamStatus> observer)
		{
			var spinWait = default (SpinWait);
			while (true)
			{
				var state1 = _observers;
				var newState = _observers.RemoveItem (observer, ReferenceEqualityComparer.Default);

				// заменяем состояние если оно не изменилось с момента вызова
				var state2 = Interlocked.CompareExchange (ref _observers, newState, state1);
				if (state1 == state2)
				{
					return;
				}

				// состояние изменилось за время вызова, поэтому повторим попытку после паузы
				spinWait.SpinOnce ();
			}
		}
	}
}