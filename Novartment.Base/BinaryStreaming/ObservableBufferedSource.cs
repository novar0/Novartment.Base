using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// A repeater that duplicates a data source for sequential reading, represented by a byte buffer,
	/// and sends notifications of consumed data.
	/// </summary>
	[DebuggerDisplay ("{Offset}...{Offset+Count} ({BufferMemory.Length}) exhausted={IsExhausted}")]
	public sealed class ObservableBufferedSource :
		IFastSkipBufferedSource
	{
		private readonly IBufferedSource _source;
		private readonly IProgress<long> _progress;
		private Action _onCompleted;

		/// <summary>
		/// Initializes a new instance of the ObservableBufferedSource class,
		/// which will relay the specified data source and sends notifications to the specified recipient.
		/// </summary>
		/// <param name="source">A data source to relay.</param>
		/// <param name="progress">Recipient of progress notifications. Specify null-reference if it is not needed.</param>
		/// <param name="onCompleted">An action that will be called after the data source is exhausted. Specify null-reference if it is not needed.</param>
		public ObservableBufferedSource (IBufferedSource source, IProgress<long> progress = null, Action onCompleted = null)
		{
			_source = source ?? throw new ArgumentNullException (nameof (source));
			_progress = progress;
			if ((onCompleted != null) && source.IsExhausted && (source.Count < 1))
			{
				// вызываем уведомление об исчерпании источника если он изначально пуст
				onCompleted.Invoke ();
				_onCompleted = null;
			}
			else
			{
				_onCompleted = onCompleted;
			}
		}

		/// <summary>
		/// Gets the buffer that contains some of the source data.
		/// The current offset and the amount of available data are in the Offset and Count properties.
		/// The buffer remains unchanged throughout the lifetime of the source.
		/// </summary>
		public ReadOnlyMemory<byte> BufferMemory => _source.BufferMemory;

		/// <summary>
		/// Gets the offset of available source data in the BufferMemory.
		/// The amount of available source data is in the Count property.
		/// </summary>
		public int Offset => _source.Offset;

		/// <summary>
		/// Gets the amount of source data available in the BufferMemory.
		/// The offset of available source data is in the Offset property.
		/// </summary>
		public int Count => _source.Count;

		/// <summary>
		/// Gets a value indicating whether the source is exhausted.
		/// Returns True if the source no longer supplies data.
		/// In that case, the data available in the buffer remains valid, but will no longer change.
		/// </summary>
		public bool IsExhausted => _source.IsExhausted;

		/// <summary>
		/// Skips specified amount of data from the start of available data in the buffer.
		/// Properties Offset and Count may be changed in the process.
		/// </summary>
		/// <param name="size">Size of data to skip from the start of available data in the buffer.
		/// Must be less than or equal to the size of available data in the buffer.</param>
		public void Skip (int size)
		{
			if ((size < 0) || (size > this.Count))
			{
				throw new ArgumentOutOfRangeException (nameof (size));
			}

			if (size > 0)
			{
				_source.Skip (size);
				_progress?.Report (size);
				if ((_onCompleted != null) && _source.IsExhausted && (_source.Count < 1))
				{
					_onCompleted.Invoke ();
					_onCompleted = null;
				}
			}
		}

		/// <summary>
		/// Asynchronously requests the source to load more data in the buffer.
		/// As a result, the buffer may not be completely filled if the source supplies data in blocks,
		/// or it may be empty if the source is exhausted.
		/// Properties Offset, Count and IsExhausted may be changed in the process.
		/// </summary>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>A task that represents the asynchronous load operation.
		/// If Count property equals zero after completion,
		/// this means that the source is exhausted and there will be no more data in the buffer.</returns>
		public ValueTask LoadAsync (CancellationToken cancellationToken = default)
		{
			return _source.LoadAsync (cancellationToken);
		}

		/// <summary>
		/// Asynchronously requests the source to load the specified amount of data in the buffer.
		/// As a result, there may be more data in the buffer than requested.
		/// Properties Offset, Count and IsExhausted may be changed in the process.
		/// </summary>
		/// <param name="size">Amount of data required in the buffer.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>A task that represents the operation.</returns>
		public ValueTask EnsureAvailableAsync (int size, CancellationToken cancellationToken = default)
		{
			if ((size < 0) || (size > this.BufferMemory.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (size));
			}

			return (size == 0) ?
				default :
				_source.EnsureAvailableAsync (size, cancellationToken);
		}

		/// <summary>
		/// Asynchronously tries to skip specified amount of source data, including data already available in the buffer.
		/// Properties Offset, Count and IsExhausted may be changed in the process.
		/// </summary>
		/// <param name="size">Size of data to skip, including data already available in the buffer.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>
		/// A task that represents the asynchronous skip operation.
		/// The result of a task will indicate the number of actually skipped bytes of data, including data already available in the buffer.
		/// It may be less than specified if the source is exhausted.
		/// Upon completion of a task, regardless of the result, the source will provide data coming right after skipped.
		/// </returns>
		public ValueTask<long> SkipWihoutBufferingAsync (long size, CancellationToken cancellationToken = default)
		{
			if (size < 0L)
			{
				throw new ArgumentOutOfRangeException (nameof (size));
			}

			if (size < 1L)
			{
				return default;
			}

			if (_source is IFastSkipBufferedSource fastSkipSource)
			{
				return TryFastSkipAsyncFinalizer (fastSkipSource.SkipWihoutBufferingAsync (size, cancellationToken));
			}

			// источник не поддерживает быстрый пропуск,
			// поэтому будем пропускать путём последовательного считывания и пропуска буфера
			var available = _source.Count;
			if (size <= (long)available)
			{
				// достаточно доступных данных буфера
				_source.Skip ((int)size);
				_progress?.Report (size);
				if ((_onCompleted != null) && _source.IsExhausted && (_source.Count < 1))
				{
					_onCompleted.Invoke ();
					_onCompleted = null;
				}

				return new ValueTask<long> (size);
			}

			if (_source.IsExhausted)
			{
				// источник исчерпан
				_source.Skip (available);
				_progress?.Report (available);
				if ((_onCompleted != null) && (_source.Count < 1))
				{
					_onCompleted.Invoke ();
					_onCompleted = null;
				}

				return new ValueTask<long> ((long)available);
			}

			return TrySkipAsyncStateMachine ();

			async ValueTask<long> TrySkipAsyncStateMachine ()
			{
				long skipped = 0L;
				do
				{
					// пропускаем всё что в буфере
					available = _source.Count;
					_source.Skip (available);
					_progress?.Report (available);
					size -= (long)available;
					skipped += (long)available;

					// заполняем буфер
					await _source.LoadAsync (cancellationToken).ConfigureAwait (false);
				}
				while (!_source.IsExhausted && (size > (long)_source.Count));

				// пропускаем частично буфер
				var reminder = (int)Math.Min (size, (long)_source.Count);
				_source.Skip (reminder);
				_progress?.Report (reminder);
				skipped += reminder;
				if ((_onCompleted != null) && _source.IsExhausted && (_source.Count < 1))
				{
					_onCompleted.Invoke ();
					_onCompleted = null;
				}

				return skipped;
			}

			async ValueTask<long> TryFastSkipAsyncFinalizer (ValueTask<long> tsk)
			{
				size = await tsk.ConfigureAwait (false);
				if (size > 0)
				{
					_progress?.Report (size);
					if ((_onCompleted != null) && _source.IsExhausted && (_source.Count < 1))
					{
						_onCompleted.Invoke ();
						_onCompleted = null;
					}
				}

				return size;
			}
		}
	}
}
