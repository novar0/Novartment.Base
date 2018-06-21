using System;

namespace Novartment.Base.Net.Smtp
{
	internal class BytesChunkEnumerator
	{
		private int _currentOffset;

		private int _nextOffset;

		internal BytesChunkEnumerator ()
		{
			_currentOffset = 0;
			_nextOffset = 0;
		}

		internal int ChunkSize => _nextOffset - _currentOffset;

		internal bool MoveToNextAngleBracketedValue (ReadOnlySpan<char> value)
		{
			var isClosingBracketFound = MoveToNextChunk (value, true, '>', true);
			return isClosingBracketFound &&
				((_nextOffset - _currentOffset) > 1) &&
				(value[_currentOffset] == '<');
		}

		// В начале пропускает skipMark,
		// потом пропускает дальнейшие символы до конца или пока не встретится endingMark.
		// возвращает False если после skipMark данные кончились.
		internal bool MoveToNextChunk (ReadOnlySpan<char> value, bool skipTrailingSpaces, char endingMark, bool includeEndingMark = false)
		{
			if (skipTrailingSpaces)
			{
				while ((_nextOffset < value.Length) && (value[_nextOffset] == ' '))
				{
					_nextOffset++;
				}
			}

			_currentOffset = _nextOffset;
			if (_currentOffset >= value.Length)
			{
				return false;
			}

			while ((_nextOffset < value.Length) && !(value[_nextOffset] == endingMark))
			{
				_nextOffset++;
			}

			if (includeEndingMark && (_nextOffset < value.Length))
			{
				_nextOffset++;
			}

			return true;
		}

		internal bool ExtendToNextChunk (ReadOnlySpan<char> value, char endingMark)
		{
			if (_nextOffset >= value.Length)
			{
				return false;
			}

			while (_nextOffset < value.Length)
			{
				if (value[_nextOffset++] == endingMark)
				{
					return true;
				}
			}

			return false;
		}

		internal ReadOnlySpan<char> GetString (ReadOnlySpan<char> value)
		{
			return value.Slice (_currentOffset, _nextOffset - _currentOffset);
		}

		internal ReadOnlySpan<char> GetStringInBrackets (ReadOnlySpan<char> value)
		{
			return value.Slice (_currentOffset + 1, _nextOffset - _currentOffset - 2);
		}
	}
}
