using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.Collections.Immutable;

namespace Novartment.Base.IO
{
	/// <summary>
	/// A wrapper for other System.IO.Stream with notification of the position within the stream.
	/// </summary>
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
		/// Initializes a new instance of the BufferedSourceBinaryDestinationStream class wrapping the specified stream.
		/// Optionally, you can specify a state object that will be passed along with the notification data.
		/// </summary>
		/// <param name="stream">The source stream from which the wrapper will be created.</param>
		/// <param name="state">The state object that will be passed along with the notification data.</param>
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

			_stream = stream;
			_state = state;
			_length = stream.Length;
			ReportProgressInternal ();
		}

		/// <summary>Gets a value indicating whether the stream supports reading.</summary>
		public override bool CanRead => _stream.CanRead;

		/// <summary>Gets a value indicating whether the stream supports seeking.</summary>
		public override bool CanSeek => _stream.CanSeek;

		/// <summary>Gets a value indicating whether the stream supports writing.</summary>
		public override bool CanWrite => _stream.CanWrite;

		/// <summary>Gets the length in bytes of the stream.</summary>
		public override long Length => _stream.Length;

		/// <summary>Gets a value indicating whether the stream can time out.</summary>
		public override bool CanTimeout => _stream.CanTimeout;

		/// <summary>Gets or sets the position within the stream.</summary>
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
		/// Gets or sets a value, in milliseconds, that determines how long the stream will
		/// attempt to read before timing out.
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
		/// Gets or sets a value, in milliseconds, that determines how long the stream will
		/// attempt to write before timing out.
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
		/// Clears all buffers for stream and causes
		/// any buffered data to be written to the underlying device.
		/// </summary>
		public override void Flush () => _stream.Flush ();

		/// <summary>
		/// Asynchronously clears all buffers for this stream, causes any buffered data to
		/// be written to the underlying device, and monitors cancellation requests.
		/// </summary>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>A task that represents the asynchronous flush operation.</returns>
		public override Task FlushAsync (CancellationToken cancellationToken = default)
		{
			return _stream.FlushAsync (cancellationToken);
		}

		/// <summary>
		/// Asynchronously reads the bytes from the stream and writes them to another stream,
		/// using a specified buffer size and cancellation token.
		/// </summary>
		/// <param name="destination">The stream to which the contents of the stream will be copied.</param>
		/// <param name="bufferSize">The size, in bytes, of the buffer. This value must be greater than zero.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>A task that represents the asynchronous copy operation.</returns>
		public override Task CopyToAsync (Stream destination, int bufferSize, CancellationToken cancellationToken = default)
		{
			return _stream.CopyToAsync (destination, bufferSize, cancellationToken);
		}

		/// <summary>
		/// Sets the length of the stream.
		/// </summary>
		/// <param name="value">The desired length of the stream in bytes</param>
		public override void SetLength (long value)
		{
			_stream.SetLength (value);
			_length = value;
			ReportProgressInternal ();
		}

		/// <summary>
		/// Sets the position within the stream.
		/// </summary>
		/// <param name="offset">A byte offset relative to the origin parameter.</param>
		/// <param name="origin">A value indicating the reference point used to obtain the new position.</param>
		/// <returns>The new position within the stream.</returns>
		public override long Seek (long offset, SeekOrigin origin)
		{
			var result = _stream.Seek (offset, origin);
			_length = _stream.Length;
			ReportProgressInternal ();
			return result;
		}

		/// <summary>
		/// Reads a sequence of bytes from the stream.
		/// </summary>
		/// <param name="buffer">The buffer to write the data into.</param>
		/// <param name="offset">The byte offset in buffer at which to begin writing data from the stream.</param>
		/// <param name="count">The maximum number of bytes to read.</param>
		/// <returns>
		///  The total number of bytes read into the buffer. This can be less than the number
		///  of bytes requested if that many bytes are not currently available, or zero (0)
		///  if the end of the stream has been reached.
		/// </returns>
		public override int Read (byte[] buffer, int offset, int count)
		{
			var result = _stream.Read (buffer, offset, count);
			ReportProgressInternal ();
			return result;
		}

		/// <summary>
		/// Reads a byte from the stream and advances the position within the stream by one byte,
		/// or returns -1 if at the end of the stream.
		/// </summary>
		/// <returns>The unsigned byte cast to an Int32, or -1 if at the end of the stream.</returns>
		public override int ReadByte ()
		{
			var result = _stream.ReadByte ();
			ReportProgressInternal ();
			return result;
		}

		/// <summary>
		/// Asynchronously reads a sequence of bytes from the stream and monitors cancellation requests.
		/// </summary>
		/// <param name="buffer">The buffer to write the data into.</param>
		/// <param name="offset">The byte offset in buffer at which to begin writing data from the stream.</param>
		/// <param name="count">The maximum number of bytes to read.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>
		/// A task that represents the asynchronous read operation.
		/// The value of the TResult parameter contains the total number of bytes read into the buffer.
		/// The result value can be less than the number of bytes requested if the number of bytes currently
		/// available is less than the requested number, or it can be 0 (zero) if the end of the stream has been reached.
		/// </returns>
		public override Task<int> ReadAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
		{
			var task = _stream.ReadAsync (buffer, offset, count, cancellationToken);
			task.ContinueWith (this.ReportProgressInternal1, default, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
			return task;
		}

#if !NETSTANDARD2_0
		/// <summary>
		/// Asynchronously reads a sequence of bytes from the stream and monitors cancellation requests.
		/// </summary>
		/// <param name="buffer">The region of memory to write the data into.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>
		/// A task that represents the asynchronous read operation.
		/// The result value contains the total number of bytes read into the buffer.
		/// It can be less than the number of bytes allocated in the buffer if that many
		/// bytes are not currently available, or it can be 0 (zero) if the end of the stream has been reached.
		/// </returns>
		public override ValueTask<int> ReadAsync (Memory<byte> buffer, CancellationToken cancellationToken = default)
		{
			var task = _stream.ReadAsync (buffer, cancellationToken);
			if (task.IsCompleted)
			{
				ReportProgressInternal ();
			}
			else
			{
				task.AsTask ().ContinueWith (this.ReportProgressInternal1, default, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
			}

			return task;
		}
#endif

		/// <summary>
		/// Writes a sequence of bytes to the stream.
		/// </summary>
		/// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the stream.</param>
		/// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the stream.</param>
		/// <param name="count">The number of bytes to be written to the stream.</param>
		public override void Write (byte[] buffer, int offset, int count)
		{
			_stream.Write (buffer, offset, count);
			_length = _stream.Length;
			ReportProgressInternal ();
		}

		/// <summary>
		/// Writes a byte to the current position in the stream.
		/// </summary>
		/// <param name="value">The byte to write to the stream.</param>
		public override void WriteByte (byte value)
		{
			_stream.WriteByte (value);
			_length = _stream.Length;
			ReportProgressInternal ();
		}

		/// <summary>
		/// Asynchronously writes a sequence of bytes to the stream and monitors cancellation requests.
		/// </summary>
		/// <param name="buffer">An array of bytes. This method copies count bytes from buffer to the stream.</param>
		/// <param name="offset">The zero-based byte offset in buffer at which to begin copying bytes to the stream.</param>
		/// <param name="count">The number of bytes to be written to the stream.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>A task that represents the asynchronous write operation.</returns>
		public override Task WriteAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken = default)
		{
			var task = _stream.WriteAsync (buffer, offset, count, cancellationToken);
			task.ContinueWith (this.ReportProgressInternal2, default, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
			return task;
		}

#if !NETSTANDARD2_0
		/// <summary>
		/// Asynchronously writes a sequence of bytes to the stream and monitors cancellation requests.
		/// </summary>
		/// <param name="buffer">The region of memory to write data from.</param>
		/// <param name="cancellationToken"> The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>A task that represents the asynchronous write operation.</returns>
		public override ValueTask WriteAsync (ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
		{
			var task = _stream.WriteAsync (buffer, cancellationToken);
			if (task.IsCompleted)
			{
				_length = _stream.Length;
				ReportProgressInternal ();
			}
			else
			{
				task.AsTask ().ContinueWith (this.ReportProgressInternal2, default, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
			}

			return task;
		}
#endif

		/// <summary>
		/// Notifies the provider that an observer is to receive notifications.
		/// </summary>
		/// <param name="observer">The object that is to receive notifications.</param>
		/// <returns>A reference to an interface that allows observers to stop receiving notifications before the provider has finished sending them.</returns>
		public IDisposable Subscribe (IObserver<FileStreamStatus> observer)
		{
			if (observer == null)
			{
				throw new ArgumentNullException (nameof (observer));
			}

			SpinWait spinWait = default;
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
		/// Releases the resources used by stream.
		/// </summary>
		/// <param name="disposing">
		/// True to release both managed and unmanaged resources;
		/// false to release only unmanaged resources.
		/// </param>
		protected override void Dispose (bool disposing)
		{
			_observers = null;
			_stream.Dispose ();
			base.Dispose (disposing);
		}

		/// <summary>
		/// Causes observers to be notified of an event with the specified parameter.
		/// </summary>
		/// <param name="value">The value of the parameter for notification.</param>
		protected void ObserversNotifyNext (FileStreamStatus value)
		{
			using var enumerator = _observers.GetEnumerator ();
			while (enumerator.MoveNext ())
			{
				enumerator.Current.OnNext (value);
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
			SpinWait spinWait = default;
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