using System;
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

	internal static class HeaderFieldBodyEncoder
	{
		/// <summary>
		/// Получает следующий элемент из последовательности, образующей исходную строку.
		/// Элемент закодирован для использования в виде значения поля заголовка и пригоден для фолдинга с другими элементами.
		/// Элементы выбираются из исходной строки по следующим правилам:
		/// 1. Объединяет несколько слов если первое и последнее слово требуют кодирования/квотирования (для экономии на накладных расходах кодирования).
		/// 2. Имеет суммарную длину не более HeaderEncoder.MaxLineLengthRecommended (для читабельности результата).
		/// </summary>
		/// <returns>Следующий элемент из последовательности, образующей исходную строку. Если вся исходная строка уже выдана, то null.</returns>
		internal static int EncodeNextElement (ReadOnlySpan<byte> source, Span<byte> buf, TextSemantics semantics, ref int startPos, ref bool prevSequenceIsWordEncoded)
		{
			var pos = startPos;

			if (pos >= source.Length)
			{
				return 0;
			}

			var mustEncode = ScanIfNextTokenMustBeEncoded (source, semantics, ref pos);

			IEstimatingEncoder encoder;
			int maxEndocdedBytesForOneSourceByte;
			var wordCount = 1;
			int sequenceEndPos = pos;
			if (!mustEncode)
			{
				prevSequenceIsWordEncoded = false;
				var size = pos - startPos;
				if (semantics != TextSemantics.Phrase)
				{
					source.Slice (startPos, size).CopyTo (buf);
					startPos = pos;
					return size;
				}

				// семантика 'phrase' предоставляет дополнительный способ представления 'Quoted Strings'
				var toQuoteCount = GetToQuoteCount (source, startPos, pos);
				if (toQuoteCount < 1)
				{
					source.Slice (startPos, size).CopyTo (buf);
					startPos = pos;
					return size;
				}

				// ищем последнее слово для квотирования
				encoder = new QuotedStringEstimatingEncoder (AsciiCharClasses.Visible | AsciiCharClasses.WhiteSpace);
				var maxSequenceLength = HeaderFieldBuilder.MaxLineLengthRecommended - (encoder.PrologSize + encoder.EpilogSize);
				var stopSearch = false;
				while (true)
				{
					var testWordToQuoteCount = 0;
					while (true)
					{
						int testWordStartPos = pos;
						if (pos >= source.Length)
						{
							// больше слов нет
							stopSearch = true;
							break;
						}

						var mustEncode2 = ScanIfNextTokenMustBeEncoded (source, semantics, ref pos);
						if (mustEncode2)
						{
							// встретилось слово, которое не может быть представлено в кавычках
							stopSearch = true;
							break;
						}

						testWordToQuoteCount = GetToQuoteCount (source, testWordStartPos, pos);
						if (testWordToQuoteCount > 0)
						{
							break;
						}
					}

					if (stopSearch)
					{
						break;
					}

					// вычисляем уложится ли последовательность в указанный лимит
					toQuoteCount += testWordToQuoteCount;
					var bytesNeeded =
						(pos - startPos) + // общее кол-во знаков
						toQuoteCount; // каждый знак для квотирования будет дополнен знаком '\', тоесть будет занимать 2 знака
					if (bytesNeeded > maxSequenceLength)
					{
						break;
					}

					wordCount++;
					sequenceEndPos = pos;
				}

				maxEndocdedBytesForOneSourceByte = 2;
			}
			else
			{
				// ищем последнее слово для кодирования
				encoder = new EncodedWordBEstimatingEncoder (Encoding.UTF8);
				var maxSequenceLength = HeaderFieldBuilder.MaxEncodedWordLength - (encoder.PrologSize + encoder.EpilogSize);
				var stopSearch = false;
				while (true)
				{
					// ищем ближайшее слово требующее кодирования, пропуская нетребующие кодирования
					while (true)
					{
						int testWordStartPos = pos;
						if (pos >= source.Length)
						{
							stopSearch = true;
							break;
						}

						var mustEncode2 = ScanIfNextTokenMustBeEncoded (source, semantics, ref pos);
						if (mustEncode2)
						{
							break;
						}
					}

					if (stopSearch)
					{
						break;
					}

					// вычисляем уложится ли последовательность в указанный лимит
					var bytesNeeded = pos - startPos;
					var groupsNeeded = (int)Math.Ceiling ((double)bytesNeeded / 3.0D);
					if ((groupsNeeded * 4) > maxSequenceLength)
					{
						// каждые 3 байта исходника будут занимать 4 байта в кодированном виде
						break;
					}

					wordCount++;
					sequenceEndPos = pos;
				}

				maxEndocdedBytesForOneSourceByte = 4;
			}

			// кодируем результат
			var byteIndex = startPos;
			var byteCount = sequenceEndPos - startPos;

			// каждый символ кроме разделяющих слова может быть закодирован, итого может быть закодировано (byteCount - wordCount + 1) символов
			var maxSizeOfEncodedSequence = encoder.PrologSize + ((byteCount - wordCount + 1) * maxEndocdedBytesForOneSourceByte) + encoder.EpilogSize;

			int outPos = 0;
			if (prevSequenceIsWordEncoded && mustEncode)
			{
				// добавляем лишний пробел, который при декодировании будет проигнорирован между кодированными словами
				buf[outPos++] = (byte)' ';
			}
			else
			{
				if (semantics == TextSemantics.Unstructured)
				{
					// напрямую копируем в результат не требующее кодирования пробельное пространство
					while (source[byteIndex] == ' ' || source[byteIndex] == '\t')
					{
						buf[outPos++] = source[byteIndex++];
						byteCount--;
					}
				}
				else
				{
					if (startPos > 0)
					{
						// для всех слов кроме первого
						// напрямую копируем в результат не требующее кодирования пробельное пространство
						// (для phrase это единственный пробел)
						buf[outPos++] = source[byteIndex++];
						byteCount--;
					}
				}
			}

			var (bytesProduced, bytesConsumed) = encoder.Encode (source.Slice (byteIndex, byteCount), buf.Slice (outPos, buf.Length - outPos), 0, true);
			outPos += bytesProduced;

			prevSequenceIsWordEncoded = mustEncode;
			startPos = sequenceEndPos;

			return outPos;
		}

		/// <summary>
		/// Сканирует токен в указанном источнике начиная с указанной позиции на предмет возможности его представления без кодировки.
		/// Правила разбиения на токены:
		/// Каждый токен содержит хотя бы один непробельный символ.
		/// Каждый токен кроме первого (pos==0) начинается с пробельного символа.
		/// </summary>
		/// <param name="source">Текст для разбивки на слова.</param>
		/// <param name="semantics">Признак, определяющий семантику текста (unstructured или phrase).</param>
		/// <param name="pos">Позиция в source.</param>
		/// <returns>True если токен требует кодирования. Позиция pos указывает на следующий токен.</returns>
		private static bool ScanIfNextTokenMustBeEncoded (ReadOnlySpan<byte> source, TextSemantics semantics, ref int pos)
		{
			// TODO: разбивать токены на части если результат превышает ограничение в 75 символов (RFC 2047 часть 2: An 'encoded-word' may not be more than 75 characters long)

			// пропускаем начальные пробелы
			while ((pos < source.Length) && ((source[pos] == ' ') || (source[pos] == '\t')))
			{
				pos++;
			}

			var tokenStartPos = pos;
			var encodeNeed = false;
			var wordPrintableStartPos = -1; // отдельно запоминаем позицию первого непробельного символа (внутри phrase могут быть табы)
			while (pos < source.Length)
			{
				var octet = source[pos];
				var isWhiteSpace = (octet == (byte)' ') || (octet == (byte)'\t');
				if (isWhiteSpace)
				{
					// найден конец слова
					encodeNeed = encodeNeed ||
						((wordPrintableStartPos >= 0) &&
						((pos - wordPrintableStartPos) > 4) &&
						(source[wordPrintableStartPos] == '=') &&
						(source[wordPrintableStartPos + 1] == '?') &&
						(source[pos - 2] == '?') &&
						(source[pos - 1] == '='));
					wordPrintableStartPos = -1;

					// для фразы разделителем токенов является только пробел (таб не считается)
					if ((semantics != TextSemantics.Phrase) || (octet == ' '))
					{
						var tokenEndPos = pos + 1;

						// проверяем: если до конца строки только пробельное пространство, то включаем его в токен
						while ((tokenEndPos < source.Length) && ((source[tokenEndPos] == ' ') || (source[tokenEndPos] == '\t')))
						{
							tokenEndPos++;
						}

						if (tokenEndPos >= source.Length)
						{
							pos = tokenEndPos;
						}

						return encodeNeed;
					}
				}
				else
				{
					encodeNeed = encodeNeed ||
						(octet >= AsciiCharSet.Classes.Count) ||
						((AsciiCharSet.Classes[octet] & (short)(AsciiCharClasses.WhiteSpace | AsciiCharClasses.Visible)) == 0);
					if (wordPrintableStartPos < 0)
					{
						wordPrintableStartPos = pos;
					}
				}

				pos++;
			}

			// сканирование строки окончено, возвращем хвост
			encodeNeed = encodeNeed || (
				((wordPrintableStartPos >= 0) &&
				(pos - wordPrintableStartPos) > 4) &&
				(source[wordPrintableStartPos] == '=') &&
				(source[wordPrintableStartPos + 1] == '?') &&
				(source[pos - 2] == '?') &&
				(source[pos - 1] == '='));

			return encodeNeed;
		}

		/// <summary>
		/// Считает количество символов, требующих квотирования в указанном диапазоне.
		/// </summary>
		private static int GetToQuoteCount (ReadOnlySpan<byte> source, int start, int end)
		{
			if (start > 0)
			{
				// во всех словах кроме первого, первым символом идет разделитель, который не учитываем при подсчете
				start++;
			}

			var toQuoteCount = 0;
			for (var idx = start; idx < end; idx++)
			{
				var isAtom = (source[idx] < AsciiCharSet.Classes.Count) && ((AsciiCharSet.Classes[source[idx]] & (short)AsciiCharClasses.Atom) != 0);
				if (!isAtom)
				{
					toQuoteCount++;
				}
			}

			return toQuoteCount;
		}
	}
}
