using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// A base class of data source for sequential reading, represented by a byte buffer,
	/// that represents data in individual parts.
	/// Data will come from other source and will be divided into parts
	/// according to the results of calling the ValidatePartData() method.
	/// </summary>
	[DebuggerDisplay ("{Offset}...{Offset+Count} ({BufferMemory.Length}) exhausted={IsExhausted}")]
	public abstract class EvaluatorPartitionedBufferedSourceBase :
		IPartitionedBufferedSource
	{
		private readonly IBufferedSource _source;
		private int _partValidatedLength = 0;

		/// <summary>
		/// Initializes a new instance of the EvaluatorPartitionedBufferedSourceBase class
		/// receiving data from the specified source.
		/// </summary>
		/// <param name="source">The source of data, which will be didived into parts.</param>
		protected EvaluatorPartitionedBufferedSourceBase (IBufferedSource source)
		{
			_source = source ?? throw new ArgumentNullException (nameof (source));
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
		public int Count => _partValidatedLength;

		/// <summary>
		/// Gets a value indicating whether the source is exhausted.
		/// Returns True if the source no longer supplies data.
		/// In that case, the data available in the buffer remains valid, but will no longer change.
		/// </summary>
		public bool IsExhausted =>
			this.IsEndOfPartFound ||
			(_source.IsExhausted && (_partValidatedLength >= _source.Count)); // проверен весь остаток источника

		/// <summary>
		/// In inherited classes gets a value indicating whether the buffer contains the end of the current part.
		/// </summary>
		protected abstract bool IsEndOfPartFound { get; }

		/// <summary>
		/// In inherited classes gets the size of the epilogue of the current part,
		/// that is, the portion that will be skipped when moving to the next part.
		/// </summary>
		protected abstract int PartEpilogueSize { get; }

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
				_partValidatedLength -= size;
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
		public async ValueTask LoadAsync (CancellationToken cancellationToken = default)
		{
			if (!this.IsEndOfPartFound)
			{
				await _source.LoadAsync (cancellationToken).ConfigureAwait (false);
				_partValidatedLength = ValidatePartData (_partValidatedLength);
			}
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

			if ((size <= _partValidatedLength) || this.IsEndOfPartFound || _source.IsExhausted)
			{
				if (size > _partValidatedLength)
				{
					throw new NotEnoughDataException (size - _partValidatedLength);
				}

				return default;
			}

			return EnsureBufferAsyncStateMachine ();

			async ValueTask EnsureBufferAsyncStateMachine ()
			{
				while ((size > _partValidatedLength) && !_source.IsExhausted)
				{
					await _source.LoadAsync (cancellationToken).ConfigureAwait (false);
					_partValidatedLength = ValidatePartData (_partValidatedLength);
				}

				if (size > _partValidatedLength)
				{
					throw new NotEnoughDataException (size - _partValidatedLength);
				}
			}
		}

		/// <summary>
		/// Asynchronously tries to skip all source data belonging to the current part,
		/// and transition to the next part.
		/// </summary>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>A task that represents the asynchronous skip and transition operation. 
		/// The result of a task will indicate success of the transition.
		/// It will be True if the source has been transitioned to the next part,
		/// and False if the source has been exhausted.
		/// </returns>
		public async ValueTask<bool> TrySkipPartAsync (CancellationToken cancellationToken = default)
		{
			if (_source.IsExhausted && (_source.Count < 1))
			{
				// источник пуст
				return false;
			}

			// необходимо найти конец части если еще не найден
			while (!this.IsEndOfPartFound)
			{
				// пропускаем проверенные данные
				if (_partValidatedLength > 0)
				{
					Skip (_partValidatedLength);
				}

				await LoadAsync (cancellationToken).ConfigureAwait (false);
				if ((_partValidatedLength <= 0) && !this.IsEndOfPartFound)
				{
					// в полном буфере не найдено ни подходящих данных, ни полного разделителя/эпилога
					// означает что разделитель не вместился в буфер
					throw new InvalidOperationException ("Buffer insufficient for detecting end of part.");
				}
			}

			// Пропускает разделитель (и всё до него) когда он найден.
			var sizeToSkip = _partValidatedLength + this.PartEpilogueSize;
			if (sizeToSkip > 0)
			{
				_source.Skip (sizeToSkip);
				_partValidatedLength = 0;
			}

			_partValidatedLength = ValidatePartData (_partValidatedLength);

			return true;
		}

		/// <summary>
		/// When inherited, checks the data in the buffer for belonging to one part.
		/// Also updates IsEndOfPartFound and PartEpilogueSize properties.
		/// </summary>
		/// <param name="validatedPartLength">
		/// Size of already verified data that is indicated as belonging to one part in previous calls.
		/// </param>
		/// <returns>The size of the data in the buffer that belongs to one part.</returns>
		protected abstract int ValidatePartData (int validatedPartLength);
	}
}
