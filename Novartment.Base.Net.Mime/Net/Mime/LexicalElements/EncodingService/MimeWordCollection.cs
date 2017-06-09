using System;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Коллекция информации об отдельных словах текста,
	/// позволяющая формировать их однородные последовательности.
	/// </summary>
	internal class MimeWordCollection
	{
		private byte[] _source;

		/// <summary>
		/// Инициализирует новый экземпляр класс MimeWordCollection используя указанное максимальное количество слов.
		/// </summary>
		/// <param name="maxWords">Максимальное количество слов.</param>
		internal MimeWordCollection (int maxWords)
		{
			// TODO: подавляющее большинство исходных данных будет представлять собой единственное слово
			this.WordPositions = new int[maxWords + 1]; // дополнительное фиктивное слово нужно чтобы расчитывать размер предыдущего слова
			this.WordEncodeNeeds = new bool[maxWords];
		}

		/// <summary>
		/// Количество слов.
		/// </summary>
		internal int Count { get; private set; }

		/// <summary>
		/// Позиции первого символа слов.
		/// </summary>
		internal int[] WordPositions { get; }

		/// <summary>
		/// Признаки необходимости кодированного представления слова.
		/// </summary>
		internal bool[] WordEncodeNeeds { get; }

		/// <summary>
		/// Разбирает указанный текст (представленный в кодированном байтовом виде)
		/// на отдельные слова используя указанный вид разделителя
		/// и учитывая указанное ограничение на длину одного слова.
		/// Каждое слово содержит хотя бы один непробельный символ
		/// Каждое слово кроме первого начинается с пробельного символа
		/// </summary>
		/// <param name="source">Текст для разбивки на слова, представленный в кодированном байтовом виде.</param>
		/// <param name="separatorIsSpaceOnly">Признак использования в качестве разделителя только единичного пробела.</param>
		/// <param name="maxWordLength">Максимально допустимый размер слова.</param>
		internal void ParseWords (byte[] source, bool separatorIsSpaceOnly, int maxWordLength)
		{
			_source = source;
			var wordNumber = 0;
			int pos = 0;
			var endPos = source.Length;
			var wordStartPos = 0;
			var subWordStartPos = -1;
			var count = 0;
			var encodeNeed = false;
			var printableFound = false;
			while (pos < endPos)
			{
				var currentChar = (char)source[pos];
				if (printableFound && ((currentChar == ' ') || (currentChar == '\t')))
				{
					encodeNeed = encodeNeed || ((subWordStartPos >= 0) && IsLikeEncodedWord (subWordStartPos, pos));
					subWordStartPos = -1;
					if (!separatorIsSpaceOnly || (currentChar == ' '))
					{
						if (wordNumber >= this.WordEncodeNeeds.Length)
						{
							throw new FormatException (FormattableString.Invariant ($"Specified value contain too many words. Maximum words allowed = {this.WordEncodeNeeds.Length}."));
						}

						this.WordPositions[wordNumber] = wordStartPos;
						this.WordEncodeNeeds[wordNumber] = encodeNeed;
						wordNumber++;
						wordStartPos = pos;
						count = 1;
						encodeNeed = false;
						printableFound = false;
					}
					else
					{
						count++;
					}
				}
				else
				{
					count++;
					if ((currentChar != ' ') && (currentChar != '\t'))
					{
						encodeNeed = encodeNeed || !AsciiCharSet.IsCharOfClass (currentChar, AsciiCharClasses.WhiteSpace | AsciiCharClasses.Visible);
						printableFound = true;
						if (subWordStartPos < 0)
						{
							subWordStartPos = pos;
						}
					}
				}

				pos++;
				if ((pos - wordStartPos) > maxWordLength)
				{
					throw new FormatException (FormattableString.Invariant ($"Too long word. Maximum allowed length = {maxWordLength}."));
				}
			}

			if (count > 0)
			{
				encodeNeed = encodeNeed || ((subWordStartPos >= 0) && IsLikeEncodedWord (subWordStartPos, pos));

				// добавляем остаток в виде еще одного слова если остаток содержит непробельные символы
				if (printableFound || (wordNumber < 1))
				{
					if (wordNumber >= this.WordEncodeNeeds.Length)
					{
						throw new FormatException (FormattableString.Invariant ($"Specified value contain too many words. Maximum words allowed = {this.WordEncodeNeeds.Length}."));
					}

					this.WordPositions[wordNumber] = wordStartPos;
					this.WordEncodeNeeds[wordNumber] = encodeNeed;
					wordNumber++;
				}
			}

			this.WordPositions[wordNumber] = endPos; // дополнительное фиктивное слово нужно чтобы расчитывать размер предыдущего слова
			this.Count = wordNumber;
		}

		/// <summary>
		/// Считает количество символов, требующих квотирования в указанном слове
		/// </summary>
		/// <param name="wordNumber">Номер слова.</param>
		/// <returns>Количество символов, требующих квотирования в указанном слове.</returns>
		internal int GetToQuoteCount (int wordNumber)
		{
			var start = this.WordPositions[wordNumber];
			var end = this.WordPositions[wordNumber + 1];
			if (wordNumber > 0)
			{
				// во всех словах кроме первого первым символом идет разделитель, который не учитываем при подсчете
				start++;
			}

			var toQuoteCount = 0;
			for (var idx = start; idx < end; idx++)
			{
				var isAtom = AsciiCharSet.IsCharOfClass ((char)_source[idx], AsciiCharClasses.Atom);
				if (!isAtom)
				{
					toQuoteCount++;
				}
			}

			return toQuoteCount;
		}

		/// <summary>
		/// Подбирает конечное слово последовательности (начинающейся со слова требующего кодирования) так чтобы она:
		/// 1. кончалась словом, требующем кодирования.
		/// 2. содержала как можно больше слов суммарной длинной не более MaxEncodedWordLength.
		/// </summary>
		/// <param name="startWord">Начальное слово последовательности.</param>
		/// <param name="maxLength">Максимальное количество символов в последовательности.</param>
		/// <returns>Номер найденного конечного слова последовательности.</returns>
		internal int FindEndOfSequenceToEncode (int startWord, int maxLength)
		{
			var endWord = startWord;
			while (true)
			{
				// ищем ближайшее слово требующее кодирования, пропуская нетребующие кодирования
				int nonEncodedWordsToSkip = 0;
				int testWord;
				while (true)
				{
					testWord = endWord + 1 + nonEncodedWordsToSkip;
					if (testWord >= this.Count)
					{
						return endWord;
					}

					if (this.WordEncodeNeeds[testWord])
					{
						break;
					}

					nonEncodedWordsToSkip++;
				}

				// вычисляем уложится ли последовательность в указанный лимит
				var bytesNeeded = this.WordPositions[testWord + 1] - this.WordPositions[startWord];
				var groupsNeeded = (int)Math.Ceiling ((double)bytesNeeded / 3.0D);
				if ((groupsNeeded * 4) > maxLength)
				{
					// каждые 3 байта исходника будут занимать 4 байта в кодированном виде
					return endWord;
				}

				endWord += nonEncodedWordsToSkip + 1;
			}
		}

		/// <summary>
		/// Подбирает конечное слово последовательности (начинающейся со слова НЕ требующего кодирования) так чтобы она:
		/// 1. кончалась словом, НЕ требующим кодирования.
		/// 2. содержала как можно больше слов суммарной длинной не более MaxEncodedWordLength.
		/// 3. Содержала символы, пригодные для представления в квотированном виде.
		/// </summary>
		/// <param name="startWord">Начальное слово последовательности.</param>
		/// <param name="maxLength">Максимальное количество символов в последовательности.</param>
		/// <returns>Номер найденного конечного слова последовательности.</returns>
		internal int FindEndOfSequenceToQuote (int startWord, int maxLength)
		{
			var toQuoteCount = GetToQuoteCount (startWord);
			var endWord = startWord;

			while (true)
			{
				int nonEncodedWordsToSkip = 0;
				int endWordToQuoteCount;
				int testWord;
				while (true)
				{
					testWord = endWord + 1 + nonEncodedWordsToSkip;
					if ((testWord >= this.Count) || this.WordEncodeNeeds[testWord])
					{
						// больше слов нет или встретилось слово, которое не может быть представлено в кавычках
						return endWord;
					}

					endWordToQuoteCount = GetToQuoteCount (testWord);
					if (endWordToQuoteCount > 0)
					{
						break;
					}

					nonEncodedWordsToSkip++;
				}

				// вычисляем уложится ли последовательность в указанный лимит
				toQuoteCount += endWordToQuoteCount;
				var bytesNeeded =
					this.WordPositions[testWord + 1] - this.WordPositions[startWord] + // общее кол-во знаков
					toQuoteCount; // каждый знак для квотирования будет дополнен знаком '\', тоесть будет занимать 2 знака
				if (bytesNeeded > maxLength)
				{
					return endWord;
				}

				endWord += nonEncodedWordsToSkip + 1;
			}
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
