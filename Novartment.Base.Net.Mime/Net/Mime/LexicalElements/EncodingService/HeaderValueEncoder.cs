using System;
using System.Buffers;
using System.Text;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Mime
{
	internal enum TextSemantics
	{
		/*
		RFC 5322
		phrase = набор atom, encoded-word или quoted-string разделенных WSP или comment, не может быть пустой.
		WSP между элементами являются пригодными для фолдинга.
		семантически phrase - это набор значений не включающий пробельное пространство между ними:
		*3.2.3 Atom. Semantically, the optional comments and FWS surrounding the rest of the characters are not part of the atom;
		*3.2.4 Quoted Strings.  Semantically, neither the optional CFWS outside of the quote characters nor the quote characters themselves are part of the quoted-string;
		*/
		Phrase,

		/*
		unstructured (*text по старому) = произвольный набор Visible,SP,HTAB либо 'encoded-word' (отделённые FWSP), может быть пустым.
		WSP окруженные Visible являются пригодными для фолдинга.
		Элементы в кавычках не распознаются как quoted-string.
		Элементы в круглых скобках не распознаются как коменты.
		*/
		Unstructured,
	}

	/// <summary>
	/// Строковое значение, позволяющее получать информацию об отдельных частях, разделённых пробелами.
	/// </summary>
	internal class HeaderValueEncoder
	{
		private readonly byte[] _source;
		private readonly TextSemantics _semantics;
		private readonly int[] _wordPositions; // Позиции первого символа слов.
		private readonly bool[] _wordEncodeNeeds; // Признаки необходимости кодированного представления слова.
		private int _startWord = 0;
		private bool _prevSequenceIsWordEncoded = false;
		private int _count; // Количество слов.

		private HeaderValueEncoder (byte[] source, TextSemantics textSemantics)
		{
			_source = source;
			var maxWords = (source.Length / 2) + 1; // каждое слово занимает мин. 2 байта (знак + разделитель);
			_wordPositions = new int[maxWords + 1]; // дополнительное фиктивное слово нужно чтобы расчитывать размер предыдущего слова
			_wordEncodeNeeds = new bool[maxWords];
			_semantics = textSemantics;
		}

		/// <summary>
		/// Разбирает указанный текст на отдельные слова.
		/// Каждое слово содержит хотя бы один непробельный символ.
		/// Каждое слово кроме первого начинается с пробельного символа.
		/// </summary>
		/// <param name="source">Текст для разбивки на слова.</param>
		/// <param name="semantics">Признак, определяющий семантику текста (unstructured или phrase).</param>
		internal static HeaderValueEncoder Parse (string source, TextSemantics semantics)
		{
			var bytes = Encoding.UTF8.GetBytes (source);
			var data = new HeaderValueEncoder (bytes, semantics);

			// TODO: оптимизировать для случая когда исходные данные представляют собой единственное слово
			// TODO: разбивать слова если результат превышает ограничение в 75 символов (RFC 2047 часть 2: An 'encoded-word' may not be more than 75 characters long)
			var wordNumber = 0;
			int pos = 0;
			var endPos = bytes.Length;
			var wordStartPos = 0;
			var wordPrintableStartPos = -1;
			var encodeNeed = false;
			var wordPrintableCharFound = false;
			while (pos < endPos)
			{
				var currentChar = bytes[pos];
				if (wordPrintableCharFound && ((currentChar == ' ') || (currentChar == '\t')))
				{
					// найден пробел после непробельного символа, то есть конец слова
					encodeNeed = encodeNeed || ((wordPrintableStartPos >= 0) && data.IsLikeEncodedWord (wordPrintableStartPos, pos));
					wordPrintableStartPos = -1;
					if ((semantics != TextSemantics.Phrase) || (currentChar == ' '))
					{
						if (wordNumber >= data._wordEncodeNeeds.Length)
						{
							throw new FormatException (FormattableString.Invariant ($"Specified value contain too many words. Maximum words allowed = {data._wordEncodeNeeds.Length}."));
						}

						data._wordPositions[wordNumber] = wordStartPos;
						data._wordEncodeNeeds[wordNumber] = encodeNeed;
						wordNumber++;
						wordStartPos = pos;
						encodeNeed = false;
						wordPrintableCharFound = false;
					}
				}
				else
				{
					if ((currentChar != ' ') && (currentChar != '\t'))
					{
						encodeNeed = encodeNeed ||
							(currentChar >= AsciiCharSet.Classes.Count) ||
							((AsciiCharSet.Classes[currentChar] & (short)(AsciiCharClasses.WhiteSpace | AsciiCharClasses.Visible)) == 0);
						wordPrintableCharFound = true;
						if (wordPrintableStartPos < 0)
						{
							wordPrintableStartPos = pos;
						}
					}
				}

				pos++;
			}

			// строка окончена, но остаток ещё не внесён
			// остаток определяется переменными wordStartPos, wordPrintableStartPos и wordPrintableCharFound
			if (endPos > 0)
			{
				encodeNeed = encodeNeed || ((wordPrintableStartPos >= 0) && data.IsLikeEncodedWord (wordPrintableStartPos, pos));

				// добавляем остаток в виде еще одного слова если: 1.он содержит непробельные символы ИЛИ 2.кроме него слов не было
				if (wordPrintableCharFound || (wordNumber < 1))
				{
					if (wordNumber >= data._wordEncodeNeeds.Length)
					{
						throw new FormatException (FormattableString.Invariant ($"Specified value contain too many words. Maximum words allowed = {data._wordEncodeNeeds.Length}."));
					}

					data._wordPositions[wordNumber] = wordStartPos;
					data._wordEncodeNeeds[wordNumber] = encodeNeed;
					wordNumber++;
				}
			}

			data._wordPositions[wordNumber] = endPos; // дополнительное фиктивное слово нужно чтобы расчитывать размер предыдущего слова
			data._count = wordNumber;

			return data;
		}

		/// <summary>
		/// Получает следующий элемент из последовательности, образующей исходную строку.
		/// Элемент закодирован для использования в виде значения поля заголовка и пригоден для фолдинга с другими элементами.
		/// Элементы выбираются из исходной строки по следующим правилам:
		/// 1. Объединяет несколько слов если первое и последнее слово требуют кодирования/квотирования (для экономии на накладных расходах кодирования).
		/// 2. Имеет суммарную длину не более HeaderEncoder.MaxLineLengthRecommended (для читабельности результата).
		/// </summary>
		/// <returns>Следующий элемент из последовательности, образующей исходную строку. Если вся исходная строка уже выдана, то null.</returns>
		internal string GetNextElement ()
		{
			if (_startWord >= _count)
			{
				return null;
			}

			var endWord = _startWord;
			IEstimatingEncoder encoder;
			int maxEndocdedBytesForOneSourceByte;
			var mustEncode = _wordEncodeNeeds[_startWord];
			if (!mustEncode)
			{
				if (_semantics != TextSemantics.Phrase)
				{
					var start = _wordPositions[_startWord];
					var end = _wordPositions[endWord + 1];
					_prevSequenceIsWordEncoded = mustEncode;
					_startWord = endWord + 1;
					return Encoding.UTF8.GetString (_source, start, end - start);
				}

				// семантика 'phrase' предоставляет дополнительный способ представления 'Quoted Strings'
				var toQuoteCount = GetToQuoteCount (_startWord);
				if (toQuoteCount < 1)
				{
					var start = _wordPositions[_startWord];
					var end = _wordPositions[endWord + 1];
					_prevSequenceIsWordEncoded = mustEncode;
					_startWord = endWord + 1;
					return Encoding.UTF8.GetString (_source, start, end - start);
				}

				// ищем последнее слово для квотирования
				encoder = new QuotedStringEstimatingEncoder (AsciiCharClasses.Visible | AsciiCharClasses.WhiteSpace);
				var maxSequenceLength = HeaderEncoder.MaxLineLengthRecommended - (encoder.PrologSize + encoder.EpilogSize);
				var stopSearch = false;
				while (true)
				{
					int nonEncodedWordsToSkip = 0;
					int endWordToQuoteCount = 0;
					int testWord;
					while (true)
					{
						testWord = endWord + 1 + nonEncodedWordsToSkip;
						if ((testWord >= _count) || _wordEncodeNeeds[testWord])
						{
							// больше слов нет или встретилось слово, которое не может быть представлено в кавычках
							stopSearch = true;
							break;
						}

						endWordToQuoteCount = GetToQuoteCount (testWord);
						if (endWordToQuoteCount > 0)
						{
							break;
						}

						nonEncodedWordsToSkip++;
					}

					if (stopSearch)
					{
						break;
					}

					// вычисляем уложится ли последовательность в указанный лимит
					toQuoteCount += endWordToQuoteCount;
					var bytesNeeded =
						_wordPositions[testWord + 1] - _wordPositions[_startWord] + // общее кол-во знаков
						toQuoteCount; // каждый знак для квотирования будет дополнен знаком '\', тоесть будет занимать 2 знака
					if (bytesNeeded > maxSequenceLength)
					{
						break;
					}

					endWord += nonEncodedWordsToSkip + 1;
				}

				maxEndocdedBytesForOneSourceByte = 2;
			}
			else
			{
				// ищем последнее слово для кодирования
				encoder = new EncodedWordBEstimatingEncoder (Encoding.UTF8);
				var maxSequenceLength = HeaderEncoder.MaxEncodedWordLength - (encoder.PrologSize + encoder.EpilogSize);
				var stopSearch = false;
				while (true)
				{
					// ищем ближайшее слово требующее кодирования, пропуская нетребующие кодирования
					int nonEncodedWordsToSkip = 0;
					int testWord;
					while (true)
					{
						testWord = endWord + 1 + nonEncodedWordsToSkip;
						if (testWord >= _count)
						{
							stopSearch = true;
							break;
						}

						if (_wordEncodeNeeds[testWord])
						{
							break;
						}

						nonEncodedWordsToSkip++;
					}

					if (stopSearch)
					{
						break;
					}

					// вычисляем уложится ли последовательность в указанный лимит
					var bytesNeeded = _wordPositions[testWord + 1] - _wordPositions[_startWord];
					var groupsNeeded = (int)Math.Ceiling ((double)bytesNeeded / 3.0D);
					if ((groupsNeeded * 4) > maxSequenceLength)
					{
						// каждые 3 байта исходника будут занимать 4 байта в кодированном виде
						break;
					}

					endWord += nonEncodedWordsToSkip + 1;
				}

				maxEndocdedBytesForOneSourceByte = 4;
			}

			// кодируем результат
			var wordCount = endWord - _startWord + 1;
			var byteIndex = _wordPositions[_startWord];
			var byteCount = _wordPositions[endWord + 1] - byteIndex;

			// каждый символ кроме разделяющих слова может быть закодирован, итого может быть закодировано (byteCount - wordCount + 1) символов
			var maxSizeOfEncodedSequence = encoder.PrologSize + ((byteCount - wordCount + 1) * maxEndocdedBytesForOneSourceByte) + encoder.EpilogSize;
			var outBuf = ArrayPool<byte>.Shared.Rent (maxSizeOfEncodedSequence);

			int outPos = 0;
			if (_prevSequenceIsWordEncoded && mustEncode)
			{
				// добавляем лишний пробел, который при декодировании будет проигнорирован между кодированными словами
				outBuf[outPos++] = (byte)' ';
			}
			else
			{
				if (_semantics == TextSemantics.Unstructured)
				{
					// напрямую копируем в результат не требующее кодирования пробельное пространство
					while (_source[byteIndex] == ' ' || _source[byteIndex] == '\t')
					{
						outBuf[outPos++] = _source[byteIndex++];
						byteCount--;
					}
				}
				else
				{
					if (_startWord > 0)
					{
						// для всех слов кроме первого
						// напрямую копируем в результат не требующее кодирования пробельное пространство
						// (для phrase это единственный пробел)
						outBuf[outPos++] = _source[byteIndex++];
						byteCount--;
					}
				}
			}

			var (bytesProduced, bytesConsumed) = encoder.Encode (_source, byteIndex, byteCount, outBuf, outPos, outBuf.Length - outPos, 0, true);
			outPos += bytesProduced;
			var result = Encoding.UTF8.GetString (outBuf, 0, outPos);

			ArrayPool<byte>.Shared.Return (outBuf);

			_prevSequenceIsWordEncoded = mustEncode;
			_startWord = endWord + 1;

			return result;
		}

		/// <summary>
		/// Считает количество символов, требующих квотирования в указанном слове.
		/// </summary>
		/// <param name="wordNumber">Номер слова.</param>
		/// <returns>Количество символов, требующих квотирования в указанном слове.</returns>
		private int GetToQuoteCount (int wordNumber)
		{
			var start = _wordPositions[wordNumber];
			var end = _wordPositions[wordNumber + 1];
			if (wordNumber > 0)
			{
				// во всех словах кроме первого первым символом идет разделитель, который не учитываем при подсчете
				start++;
			}

			var toQuoteCount = 0;
			for (var idx = start; idx < end; idx++)
			{
				var isAtom = (_source[idx] < AsciiCharSet.Classes.Count) && ((AsciiCharSet.Classes[_source[idx]] & (short)AsciiCharClasses.Atom) != 0);
				if (!isAtom)
				{
					toQuoteCount++;
				}
			}

			return toQuoteCount;
		}

		private bool IsLikeEncodedWord (int startPos, int endPos)
		{
			return ((endPos - startPos) > 4) &&
					(_source[startPos] == '=') &&
					(_source[startPos + 1] == '?') &&
					(_source[endPos - 2] == '?') &&
					(_source[endPos - 1] == '=');
		}
	}
}
