using Novartment.Base.Text;

namespace Novartment.Base.Net.Smtp
{
	internal class BytesChunkEnumerator
	{

		private readonly byte[] _value;

		private readonly int _endOffset;

		private int _currentOffset;

		private int _nextOffset;

		internal int ChunkSize => _nextOffset - _currentOffset;

		internal BytesChunkEnumerator (byte[] value, int offset, int count)
		{
			_value = value;
			_currentOffset = offset;
			_nextOffset = offset;
			_endOffset = offset + count;
		}

		internal bool MoveToNextBracketedValue (byte whiteSpace, byte openingBracket, byte closingBracket)
		{
			var isClosingBracketFound = MoveToNextChunk (whiteSpace, closingBracket, true);
			return (isClosingBracketFound &&
				((_nextOffset - _currentOffset) > 1) &&
				(_value[_currentOffset] == openingBracket));
		}

		/// <summary>
		/// В начале пропускает skipMark,
		/// потом пропускает дальнейшие символы до конца или пока не встретится endingMark.
		/// </summary>
		/// <returns>False если после skipMark данные кончились.</returns>
		internal bool MoveToNextChunk (byte skipMark, byte endingMark, bool includeEndingMark = false)
		{
			while ((_nextOffset < _endOffset) && (_value[_nextOffset] == skipMark))
			{
				_nextOffset++;
			}
			_currentOffset = _nextOffset;
			if (_currentOffset >= _endOffset)
			{
				return false;
			}
			while ((_nextOffset < _endOffset) && !(_value[_nextOffset] == endingMark))
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
			return AsciiCharSet.GetString (
				_value,
				_currentOffset,
				_nextOffset - _currentOffset);
		}

		internal string GetStringMaskingInvalidChars ()
		{
			return AsciiCharSet.GetStringMaskingInvalidChars (
				_value,
				_currentOffset,
				_nextOffset - _currentOffset,
				'?');
		}

		internal string GetStringInBrackets ()
		{
			return AsciiCharSet.GetString (
				_value,
				_currentOffset + 1,
				_nextOffset - _currentOffset - 2);
		}
	}
}
