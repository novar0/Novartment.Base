using System;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Smtp
{
	internal class BytesChunkEnumerator
	{
		private readonly ReadOnlyMemory<byte> _value;

		private readonly int _endOffset;

		private int _currentOffset;

		private int _nextOffset;

		internal BytesChunkEnumerator (ReadOnlyMemory<byte> value, int offset, int count)
		{
			_value = value;
			_currentOffset = offset;
			_nextOffset = offset;
			_endOffset = offset + count;
		}

		internal int ChunkSize => _nextOffset - _currentOffset;

		internal bool MoveToNextBracketedValue (byte whiteSpace, byte openingBracket, byte closingBracket)
		{
			var isClosingBracketFound = MoveToNextChunk (whiteSpace, closingBracket, true);
			return isClosingBracketFound &&
				((_nextOffset - _currentOffset) > 1) &&
				(_value.Span[_currentOffset] == openingBracket);
		}

		// В начале пропускает skipMark,
		// потом пропускает дальнейшие символы до конца или пока не встретится endingMark.
		// возвращает False если после skipMark данные кончились.
		internal bool MoveToNextChunk (byte skipMark, byte endingMark, bool includeEndingMark = false)
		{
			var span = _value.Span;
			while ((_nextOffset < _endOffset) && (span[_nextOffset] == skipMark))
			{
				_nextOffset++;
			}

			_currentOffset = _nextOffset;
			if (_currentOffset >= _endOffset)
			{
				return false;
			}

			while ((_nextOffset < _endOffset) && !(span[_nextOffset] == endingMark))
			{
				_nextOffset++;
			}

			if (includeEndingMark && (_nextOffset < _endOffset))
			{
				_nextOffset++;
			}

			return true;
		}

		internal string GetString ()
		{
			return AsciiCharSet.GetString (_value.Span.Slice (_currentOffset, _nextOffset - _currentOffset));
		}

		internal string GetStringMaskingInvalidChars ()
		{
			return AsciiCharSet.GetStringMaskingInvalidChars (_value.Span.Slice (_currentOffset, _nextOffset - _currentOffset), '?');
		}

		internal string GetStringInBrackets ()
		{
			return AsciiCharSet.GetString (_value.Span.Slice (_currentOffset + 1, _nextOffset - _currentOffset - 2));
		}
	}
}
