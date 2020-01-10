using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// A data source for sequential reading, represented by a byte buffer, that represents data in individual parts.
	/// Data will come from other source and will be divided into parts
	/// according to the specified separator pattern.
	/// </summary>
	[DebuggerDisplay ("{Offset}...{Offset+Count} ({BufferMemory.Length}) exhausted={IsExhausted}")]
	public class TemplateSeparatedBufferedSource :
		IPartitionedBufferedSource
	{
		private readonly IBufferedSource _source;
		private readonly ReadOnlyMemory<byte> _template;
		private readonly bool _throwIfNoSeparatorFound;
		private int _foundTemplateOffset;
		private int _foundTemplateLength = 0;

		/// <summary>
		/// Initializes a new instance of the TemplateSeparatedBufferedSource class,
		/// receiving data from the specified source and
		/// dividing it into parts according to the specified separator pattern.
		/// </summary>
		/// <param name="source">The source of data, which will be didived into parts according to the specified separator pattern.</param>
		/// <param name="separator">Separator pattern that divides the source into parts.</param>
		/// <param name="throwIfNoSeparatorFound">
		/// A value indicating whether to throw an exception when the source is exhausted but the separator pattern is not found.
		/// </param>
		public TemplateSeparatedBufferedSource (IBufferedSource source, ReadOnlyMemory<byte> separator, bool throwIfNoSeparatorFound)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if ((separator.Length < 1) || (separator.Length > source.BufferMemory.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (separator));
			}

			Contract.EndContractBlock ();

			_source = source;
			_template = separator;
			_throwIfNoSeparatorFound = throwIfNoSeparatorFound;
			SearchBuffer (true);
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
		public int Count => _foundTemplateOffset - _source.Offset;

		/// <summary>
		/// Gets a value indicating whether the source is exhausted.
		/// Returns True if the source no longer supplies data.
		/// In that case, the data available in the buffer remains valid, but will no longer change.
		/// </summary>
		public bool IsExhausted =>
				(!_throwIfNoSeparatorFound && _source.IsExhausted) || (_foundTemplateLength >= _template.Length);

		/// <summary>
		/// Skips specified amount of data from the start of available data in the buffer.
		/// Properties Offset and Count may be changed in the process.
		/// </summary>
		/// <param name="size">Size of data to skip from the start of available data in the buffer.
		/// Must be less than total size of available data in the buffer.</param>
		public void SkipBuffer (int size)
		{
			if ((size < 0) || (size > this.Count))
			{
				throw new ArgumentOutOfRangeException (nameof (size));
			}

			Contract.EndContractBlock ();

			if (size > 0)
			{
				_source.SkipBuffer (size);
			}
		}

		/// <summary>
		/// Asynchronously fills the buffer with source data, appending already available data.
		/// As a result, the buffer may not be completely filled if the source supplies data in blocks,
		/// or empty if the source is exhausted.
		/// Properties Offset, Count and IsExhausted may be changed in the process.
		/// </summary>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>A task that represents the asynchronous fill operation.
		/// If Count property equals zero after completion,
		/// this means that the source is exhausted and there will be no more data in the buffer.</returns>
		public async ValueTask FillBufferAsync (CancellationToken cancellationToken = default)
		{
			if (_foundTemplateLength >= _template.Length)
			{
				// further reading limited by found separator
				return;
			}

			bool isSourceExhausted;
			var prevOffset = _source.Offset;
			try
			{
				await _source.FillBufferAsync (cancellationToken).ConfigureAwait (false);
			}
			finally
			{
				// признак изменения позиции данных в буфере.
				// данные, доступные ранее, могут передвинуться, но не могут измениться (могут лишь добавиться новые)
				var resetSearch = prevOffset != _source.Offset;
				isSourceExhausted = SearchBuffer (resetSearch);
			}

			if (isSourceExhausted && _throwIfNoSeparatorFound)
			{
				throw new NotEnoughDataException (
					"Source exhausted, but separator not found.",
					_template.Length - _foundTemplateLength);
			}
		}

		/// <summary>
		/// Asynchronously requests the source to provide the specified amount of data in the buffer.
		/// As a result, there may be more data in the buffer than requested.
		/// Properties Offset, Count and IsExhausted may be changed in the process.
		/// </summary>
		/// <param name="size">Amount of data required in the buffer.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>A task that represents the operation.</returns>
		public ValueTask EnsureBufferAsync (int size, CancellationToken cancellationToken = default)
		{
			if ((size < 0) || (size > this.BufferMemory.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (size));
			}

			Contract.EndContractBlock ();

			if ((size <= (_foundTemplateOffset - _source.Offset)) || _source.IsExhausted || (_foundTemplateLength >= _template.Length))
			{
				if (size > (_foundTemplateOffset - _source.Offset))
				{
					throw new NotEnoughDataException (size - (_foundTemplateOffset - _source.Offset));
				}

				return default;
			}

			return EnsureBufferAsyncStateMachine();

			async ValueTask EnsureBufferAsyncStateMachine ()
			{
				while ((size > (_foundTemplateOffset - _source.Offset)) && !_source.IsExhausted)
				{
					var prevOffset = _source.Offset;
					await _source.FillBufferAsync(cancellationToken).ConfigureAwait(false);
					var resetSearch = prevOffset != _source.Offset; // изменилась позиция данных в буфере. при этом сами данные, доступные ранее, не могут измениться, могут лишь добавиться новые
					SearchBuffer(resetSearch);
				}

				if (size > (_foundTemplateOffset - _source.Offset))
				{
					throw new NotEnoughDataException(size - (_foundTemplateOffset - _source.Offset));
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
			int sizeToSkip;

			// find separator if not already found
			while (_foundTemplateLength < _template.Length)
			{
				sizeToSkip = _foundTemplateOffset - _source.Offset;
				if (sizeToSkip > 0)
				{
					SkipBuffer (sizeToSkip); // skip all data to found separator
				}

				var prevOffset = _source.Offset;
				await _source.FillBufferAsync (cancellationToken).ConfigureAwait (false);
				var resetSearch = prevOffset != _source.Offset; // изменилась позиция данных в буфере. при этом сами данные, доступные ранее, не могут измениться, могут лишь добавиться новые
				var isSourceExhausted = SearchBuffer (resetSearch);
				if (isSourceExhausted)
				{
					// разделитель не найден, источник ичерпался
					if (_source.Count > 0)
					{
						_source.SkipBuffer (_source.Count);
					}

					return false;
				}
			}

			// jump over found separator
			sizeToSkip = _foundTemplateOffset - _source.Offset + _foundTemplateLength;
			_source.SkipBuffer (sizeToSkip);
			SearchBuffer (true);

			return true;
		}

		/// <summary>
		/// Ищет шаблон в текущем буфере.
		/// </summary>
		/// <param name="resetFromStart">Признак сброса поиска.
		/// Если True, то поиск начнется с начала данных буфера,
		/// иначе продолжится с последней позиции где искали в предыдущий раз.</param>
		/// <returns>Если True, то шаблон уже никогда не будет найден (источник исчерпался) и запускать поиск повторно не нужно.</returns>
		private bool SearchBuffer (bool resetFromStart)
		{
			if (resetFromStart)
			{
				_foundTemplateOffset = _source.Offset;
				_foundTemplateLength = 0;
			}

			var buf = _source.BufferMemory.Span;
			var template = _template.Span;
			while (((_foundTemplateOffset + _foundTemplateLength) < (_source.Offset + _source.Count)) && (_foundTemplateLength < _template.Length))
			{
				if (buf[_foundTemplateOffset + _foundTemplateLength] == template[_foundTemplateLength])
				{
					_foundTemplateLength++;
				}
				else
				{
					_foundTemplateOffset++;
					_foundTemplateLength = 0;
				}
			}

			// no more data from source, separator not found
			if (_source.IsExhausted && (_foundTemplateLength < _template.Length))
			{ // stops searching
				_foundTemplateOffset = _source.Offset + _source.Count;
				_foundTemplateLength = 0;
				return true;
			}

			return false;
		}
	}
}
