using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Collections;
using Novartment.Base.Collections.Immutable;
using Novartment.Base.Collections.Linq;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Кодер для семантических элементов MIME-сообщений.
	/// На входе получает объект с исходными параметрами,
	/// на выходе выдаёт его MIME-кодированное текстовое представление.
	/// </summary>
	internal static class HeaderEncoder
	{
		internal static readonly int MaxOneFieldSize = 10000;

		/// <summary>
		/// Максимально допустимая длина одной строки.
		/// Определено в RFC 2822 часть 2.1.1.
		/// </summary>
		internal static readonly int MaxLineLengthRequired = 998;

		/// <summary>
		/// Максимально рекомендуемая длина одной строки.
		/// Определено в RFC 2822 часть 2.1.1.
		/// </summary>
		internal static readonly int MaxLineLengthRecommended = 78;

		/// <summary>
		/// Максимально допустимая длина 'encoded-word'.
		/// Определено в RFC 2047 часть 2.
		/// </summary>
		internal static readonly int MaxEncodedWordLength = 75;

		private static Encoding _encoding = Encoding.UTF8;

		/// <summary>
		/// Encoding, used to create string representation of header field.
		/// </summary>
		internal static Encoding DefaultEncoding
		{
			get => _encoding;
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException (nameof (value));
				}

				Contract.EndContractBlock ();

				_encoding = value;
			}
		}

		/// <summary>
		/// Кодирует указанное значение в значение поля заголовка типа 'unstructured'.
		/// </summary>
		/// <param name="text">Значение, которое надо представить в ограничениях типа 'unstructured'.</param>
		/// <returns>Набор элементов значения поля заголовка.</returns>
		/// <remarks>
		/// Используется значение поля DefaultEncoding.
		/// Возвращаются отдельные слова, а склеиваются только если это даёт большой выигрыш по размеру.
		/// </remarks>
		internal static IReadOnlyList<string> EncodeUnstructured (string text)
		{
			// unstructured (*text по старому) = произвольный набор Visible,SP,HTAB либо 'encoded-word' (отделённые FWSP), может быть пустым.
			// WSP окруженные Visible являются пригодными для фолдинга.
			// Элементы в кавычках не распознаются как quoted-string.
			// Элементы в круглых скобках не распознаются как коменты.

			// важно сохранять читаемость после кодировки, поэтому в ущерб размеру используем более читаемую кодировку
			if (text == null)
			{
				throw new ArgumentNullException (nameof (text));
			}

			if (text.Length >= MaxLineLengthRequired)
			{
				throw new ArgumentOutOfRangeException (nameof (text));
			}

			Contract.EndContractBlock ();

			if (text.Length < 1)
			{
				return ReadOnlyList.Empty<string> ();
			}

			var bytes = _encoding.GetBytes (text);
			var words = new MimeWordCollection ((text.Length / 2) + 1); // каждое слово занимает мин. 2 байта (знак + разделитель)
			words.ParseWords (bytes, false, MaxLineLengthRequired);
			var encoder = new EncodedWordBEstimatingEncoder (_encoding);
			var maxRequiredSizeForBEncoding =
				(words.Count * (encoder.PrologSize + encoder.EpilogSize)) + // каждое слово может потребовать пролог+эпилог для кодированного вида
				((bytes.Length - words.Count) * 4); // каждый символ кроме разделяющих слова может превратиться в 4 в закодированном виде
			var outBuf = ArrayPool<byte>.Shared.Rent (maxRequiredSizeForBEncoding);
			var result = new ArrayList<string> ();

			// текст рабивается на последовательности слов, которые удобно представить в одном виде (напрямую или в виде encoded-word)
			int sequenceStartWord = 0;
			var prevSequenceIsWordEncoded = false;
			while (sequenceStartWord < words.Count)
			{
				var charIndex = words.WordPositions[sequenceStartWord];
				if (!words.WordEncodeNeeds[sequenceStartWord])
				{
					var wordStr = _encoding.GetString (
						bytes,
						charIndex,
						words.WordPositions[sequenceStartWord + 1] - charIndex);
					result.Add (wordStr);
					prevSequenceIsWordEncoded = false;
					sequenceStartWord++;
				}
				else
				{
					// здесь мы имеем в позиции sequenceStartWord слово требующее кодирования
					// подбираем размер последовательности: как можно больше слов, но суммарно не длиннее MaxEncodedWordLength
					var sequenceEndWord = (sequenceStartWord == (words.Count - 1)) ? // оптимизация для единственного слова
						sequenceStartWord :
						words.FindEndOfSequenceToEncode (sequenceStartWord, MaxEncodedWordLength - (_encoding.WebName.Length + 5 + 2)); // для B-encoding вычитаем размер эпилога

					var charCount = words.WordPositions[sequenceEndWord + 1] - charIndex;
					int outPos = 0;
					if (prevSequenceIsWordEncoded)
					{
						// добавляем лишний пробел, который при декодировании будет проигнорирован между кодированными словами
						outBuf[outPos++] = (byte)' ';
					}
					else
					{
						// напрямую копируем в результат не требующее кодирования пробельное пространство
						while (bytes[charIndex] == ' ' || bytes[charIndex] == '\t')
						{
							outBuf[outPos++] = bytes[charIndex++];
							charCount--;
						}
					}

					var (bytesProduced, bytesConsumed) = encoder.Encode (bytes, charIndex, charCount, outBuf, outPos, outBuf.Length - outPos, 0, true);
					outPos += bytesProduced;
					result.Add (_encoding.GetString (outBuf, 0, outPos));
					prevSequenceIsWordEncoded = true;
					sequenceStartWord = sequenceEndWord + 1;
				}
			}

			ArrayPool<byte>.Shared.Return (outBuf);

			return result;
		}

		/// <summary>
		/// Кодирует указанное '*tokens' значение в значение поля заголовка.
		/// </summary>
		/// <param name="value">'*tokens' значение.</param>
		/// <returns>Набор элементов значения поля заголовка.</returns>
		/// <remarks>Все последовательности пробельных символов будут удалены.</remarks>
		internal static IAdjustableList<string> EncodeTokens (string value)
		{
			// An 'encoded-word' MUST NOT be used in a '*tokens' header field.
			var result = new ArrayList<string> ();
			var isEmpty = string.IsNullOrEmpty (value);
			if (isEmpty)
			{
				return result;
			}

			var pos = 0;
			int wordStart = -1;
			while (pos < value.Length)
			{
				var currentChar = value[pos];
				var charClass = (currentChar < AsciiCharSet.Classes.Count) ?
					(AsciiCharClasses)AsciiCharSet.Classes[currentChar] :
					AsciiCharClasses.None;
				if ((charClass & AsciiCharClasses.WhiteSpace) != 0)
				{
					if (wordStart >= 0)
					{
						result.Add (value.Substring (wordStart, pos - wordStart));
						wordStart = -1;
					}
				}
				else
				{
					if ((charClass & AsciiCharClasses.Visible) != 0)
					{
						if (wordStart < 0)
						{
							wordStart = pos;
						}
					}
					else
					{
						if ((currentChar != 0x0d) || ((pos + 1) >= value.Length) || (value[pos + 1] != 0x0a))
						{
							throw new FormatException ("Value contains invalid for 'token' character U+" +
								Hex.OctetsUpper[currentChar >> 8] + Hex.OctetsUpper[currentChar & 0xff] +
								". Expected characters are U+0009 and U+0020...U+007E.");
						}

						if (wordStart >= 0)
						{
							result.Add (value.Substring (wordStart, pos - wordStart));
							wordStart = -1;
							pos++;
						}
					}
				}

				pos++;
			}

			if (wordStart >= 0)
			{
				// добавляем только куски со словами (пробельные куски в конце отбрасываем)
				result.Add (value.Substring (wordStart, pos - wordStart));
			}

			return result;
		}

		/// <summary>
		/// Кодирует указанное значение в значение поля заголовка типа 'phrase'.
		/// </summary>
		/// <param name="text">Значение, которое надо представить в ограничениях типа 'phrase'.</param>
		/// <returns>Набор элементов значения поля заголовка.</returns>
		/// <remarks>
		/// Используется значение поля DefaultEncoding.
		/// Приоритет представления элементов значения: атом / квотированная строка / encoded-word.
		/// Возвращаются отдельные слова, а склеиваются только если это даёт большой выигрыш по размеру.
		/// </remarks>
		internal static IReadOnlyList<string> EncodePhrase (string text)
		{
			/*
			RFC 5322
			phrase = набор atom, encoded-word или quoted-string разделенных WSP или comment, не может быть пустой.
			WSP между элементами являются пригодными для фолдинга.
			семантически phrase - это набор значений не включающий пробельное пространство между ними:
			*3.2.3 Atom. Semantically, the optional comments and FWS surrounding the rest of the characters are not part of the atom;
			*3.2.4 Quoted Strings.  Semantically, neither the optional CFWS outside of the quote characters nor the quote characters themselves are part of the quoted-string;
			*/
			if (text == null)
			{
				throw new ArgumentNullException (nameof (text));
			}

			if ((text.Length < 1) || (text.Length >= MaxLineLengthRequired))
			{
				throw new ArgumentOutOfRangeException (nameof (text));
			}

			Contract.EndContractBlock ();

			// TODO: разбивать слова если результат превышает ограничение в 75 символов
			// RFC 2047 часть 2: An 'encoded-word' may not be more than 75 characters long
			var bytes = _encoding.GetBytes (text);
			var words = new MimeWordCollection ((text.Length / 2) + 1); // каждое слово занимает мин. 2 байта (знак + разделитель)
			words.ParseWords (bytes, true, MaxLineLengthRequired);

			var encoderS = new QuotedStringEstimatingEncoder (AsciiCharClasses.Visible | AsciiCharClasses.WhiteSpace);
			var encoderB = new EncodedWordBEstimatingEncoder (_encoding);
			var maxRequiredSizeForBEncoding =
				(words.Count * (encoderB.PrologSize + encoderB.EpilogSize)) + // каждое слово может потребовать пролог+эпилог для кодированного вида
				((bytes.Length - words.Count) * 4); // каждый символ кроме разделяющих слова может превратиться в 4 в закодированном виде
			var maxRequiredSizeForSEncoding =
				(words.Count * (encoderS.PrologSize + encoderS.EpilogSize)) + // каждое слово может потребовать пролог+эпилог для кодированного вида
				((bytes.Length - words.Count) * 2); // каждый символ кроме разделяющих слова может превратиться в 2 в закодированном виде
			var outBuf = ArrayPool<byte>.Shared.Rent (Math.Min (Math.Max (maxRequiredSizeForBEncoding, maxRequiredSizeForSEncoding), MaxLineLengthRequired));
			var result = new ArrayList<string> ();

			// Приоритет представления: 1. макс. читабельность, 2. мин. размер, 3. макс. мест для фолдинга.
			// текст разбивается на последовательности слов, которые удобно представить в одном виде (напрямую или в виде encoded-word)
			int sequenceStartWord = 0;
			var prevSequenceIsWordEncoded = false;
			while (sequenceStartWord < words.Count)
			{
				var mustEncode = words.WordEncodeNeeds[sequenceStartWord];
				var charIndex = words.WordPositions[sequenceStartWord];
				var isCodingNotNeeded = !mustEncode && (words.GetToQuoteCount (sequenceStartWord) < 1);
				if (isCodingNotNeeded)
				{ // если нет ни одного символа требующего квотирования или кодирования
					var wordStr = _encoding.GetString (
						bytes,
						charIndex,
						words.WordPositions[sequenceStartWord + 1] - charIndex);
					result.Add (wordStr);
					prevSequenceIsWordEncoded = false;
					sequenceStartWord++;
				}
				else
				{
					// здесь мы имеем в позиции sequenceStartWord слово требующее кодирования или квотирования
					// подбираем размер последовательности: как можно больше слов, но суммарно не длиннее MaxEncodedWordLength
					var sequenceEndWord = (sequenceStartWord == (words.Count - 1)) ? // оптимизация для единственного слова
						sequenceStartWord :
							mustEncode ?
								words.FindEndOfSequenceToEncode (sequenceStartWord, MaxEncodedWordLength - (encoderB.PrologSize + encoderB.EpilogSize)) : // для B-encoding вычитаем размер эпилога
								words.FindEndOfSequenceToQuote (sequenceStartWord, MaxLineLengthRecommended - (encoderS.PrologSize + encoderS.EpilogSize)); // для quoted-string вычитаем кавычки

					int outPos = 0;
					var charCount = words.WordPositions[sequenceEndWord + 1] - charIndex;
					if (prevSequenceIsWordEncoded && mustEncode)
					{
						// добавляем лишний пробел, который при декодировании будет проигнорирован между кодированными словами
						outBuf[outPos++] = (byte)' ';
					}
					else
					{
						if (result.Count > 0)
						{
							// для всех слов кроме первого
							// напрямую копируем в результат не требующее кодирования пробельное пространство
							// (для phrase это единственный пробел)
							outBuf[outPos++] = bytes[charIndex++];
							charCount--;
						}
					}

					var encoder = mustEncode ? (IEstimatingEncoder)encoderB : (IEstimatingEncoder)encoderS;
					var (bytesProduced, bytesConsumed) = encoder.Encode (bytes, charIndex, charCount, outBuf, outPos, outBuf.Length - outPos, 0, true);
					outPos += bytesProduced;
					result.Add (_encoding.GetString (outBuf, 0, outPos));
					prevSequenceIsWordEncoded = mustEncode;
					sequenceStartWord = sequenceEndWord + 1;
				}
			}

			ArrayPool<byte>.Shared.Return (outBuf);

			return result;
		}

		/// <summary>
		/// Кодирует Mailbox в набор элементов значения поля заголовка.
		/// </summary>
		/// <param name="mailbox">Mailbox который будет представлен в виде значения поля заголовка.</param>
		/// <returns>
		/// Набор элементов значения поля заголовка.
		/// Mailbox будет представлен одной или двумя строками (в зависимости от наличия Name).
		/// </returns>
		internal static IReadOnlyList<string> EncodeMailbox (Mailbox mailbox)
		{
			if (mailbox == null)
			{
				throw new ArgumentNullException (nameof (mailbox));
			}

			Contract.EndContractBlock ();

			var address = mailbox.Address.ToAngleString ();
			var isEmpty = string.IsNullOrEmpty (mailbox.Name);
			return isEmpty ?
				ReadOnlyList.Repeat (address, 1) :
				EncodePhrase (mailbox.Name).Concat (ReadOnlyList.Repeat (address, 1));
		}

		internal static void EncodeHeaderFieldParameter (IAdjustableCollection<string> result, HeaderFieldParameter parameter)
		{
			/*
			RFC 2231 часть 4.1:
			1. Language and character set information only appear at the beginning of a given parameter value.
			2. Continuations do not provide a facility for using more than one character set or language in the same parameter value.
			3. A value presented using multiple continuations may contain a mixture of encoded and unencoded segments.
			4. The first segment of a continuation MUST be encoded if language and character set information are given.
			5. If the first segment of a continued parameter value is encoded the language and character set field delimiters MUST be present even when the fields are left blank.
			*/

			// выясняем понадобится ли применение кодировки
			var name = parameter.Name;
			var value = parameter.Value;
			var isEncodingNeeded = !AsciiCharSet.IsAllOfClass (value, AsciiCharClasses.Visible | AsciiCharClasses.WhiteSpace);

			var extendedParameterEncoder = new ExtendedParameterValueEstimatingEncoder (_encoding);
			var encodersOne = ReadOnlyList.Repeat<IEstimatingEncoder> (extendedParameterEncoder, 1);
			var encodersAll = new ReadOnlyArray<IEstimatingEncoder> (new IEstimatingEncoder[]
			{
				new AsciiCharClassEstimatingEncoder (AsciiCharClasses.Token),
				new QuotedStringEstimatingEncoder (AsciiCharClasses.Visible | AsciiCharClasses.WhiteSpace),
				extendedParameterEncoder,
			});
			var bytes = _encoding.GetBytes (value);
			var extra = " *=;".Length + name.Length;
			var valueParts = EstimatingEncoder.CutBySize (
				segmentNumber => (isEncodingNeeded && (segmentNumber == 0)) ?
					encodersOne :
					encodersAll, // если нужна кодировка то первый кусок принудительно делаем в виде 'extended-value'
				bytes,
				MaxLineLengthRecommended,
				(segmentNumber, encoder) => extra + ((segmentNumber == 0) ? 1 : (1 + (int)Math.Log10 (segmentNumber))) + ((encoder is ExtendedParameterValueEstimatingEncoder) ? 1 : 0));
			var outBytes = ArrayPool<byte>.Shared.Rent (MaxLineLengthRequired);
			if ((valueParts.Length < 2) && !(valueParts[0].Encoder is ExtendedParameterValueEstimatingEncoder))
			{
				// значение из одной части не требующей кодирования
				AsciiCharSet.GetBytes (name, 0, name.Length, outBytes, 0);
				var pos = name.Length;
				outBytes[pos++] = (byte)'=';
				var (bytesProduced, bytesConsumed) = valueParts[0].Encoder.Encode (
					bytes,
					valueParts[0].Offset,
					valueParts[0].Count,
					outBytes,
					pos,
					MaxLineLengthRequired,
					0,
					false);
				pos += bytesProduced;
				result.Add (AsciiCharSet.GetString (outBytes, 0, pos));
			}
			else
			{
				// значение из многих частей либо требует кодирования
				for (var idx = 0; idx < valueParts.Length; idx++)
				{
					AsciiCharSet.GetBytes (name, 0, name.Length, outBytes, 0);
					var pos = name.Length;
					outBytes[pos++] = (byte)'*';
					var idxStr = idx.ToString (CultureInfo.InvariantCulture);
					AsciiCharSet.GetBytes (idxStr, 0, idxStr.Length, outBytes, pos);
					pos += idxStr.Length;
					if (valueParts[idx].Encoder == extendedParameterEncoder)
					{
						outBytes[pos++] = (byte)'*';
					}

					outBytes[pos++] = (byte)'=';
					var (bytesProduced, bytesConsumed) = valueParts[idx].Encoder.Encode (
						bytes,
						valueParts[idx].Offset,
						valueParts[idx].Count,
						outBytes,
						pos,
						MaxLineLengthRequired,
						idx,
						idx < (valueParts.Length - 1));
					pos += bytesProduced;
					if (idx != (valueParts.Length - 1))
					{
						outBytes[pos++] = (byte)';';
					}

					result.Add (AsciiCharSet.GetString (outBytes, 0, pos));
				}
			}

			ArrayPool<byte>.Shared.Return (outBytes);
		}

		/// <summary>
		/// Записывает коллекцию полей в указанный получатель двоичных данных.
		/// </summary>
		/// <param name="fields">Коллекция полей, которая будет записана в получатель двоичных данных.</param>
		/// <param name="destination">Получатель двоичных данных, в который будет записана указанная коллекция полей.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Суммарное количество байтов, записанных в получатель двоичных данных.</returns>
		/// <remarks>Значения полей будут записаны "как есть", без фолдинга.</remarks>
		internal static Task<int> SaveHeaderAsync (
			IReadOnlyCollection<HeaderFieldBuilder> fields,
			IBinaryDestination destination,
#pragma warning disable CA1801 // Review unused parameters
			CancellationToken cancellationToken)
#pragma warning restore CA1801 // Review unused parameters
		{
			if (fields == null)
			{
				throw new ArgumentNullException (nameof (fields));
			}

			if (destination == null)
			{
				throw new ArgumentNullException (nameof (destination));
			}

			Contract.EndContractBlock ();

			return SaveHeaderAsyncStateMachine ();

			async Task<int> SaveHeaderAsyncStateMachine ()
			{
				int totalSize = 0;
				var bytes = ArrayPool<byte>.Shared.Rent (MaxOneFieldSize);
				foreach (var fieldBuilder in fields)
				{
					cancellationToken.ThrowIfCancellationRequested ();
					var field = fieldBuilder.ToHeaderField (HeaderEncoder.MaxLineLengthRecommended);
					var name = HeaderFieldNameHelper.GetName (field.Name);
					var size = name.Length;
					AsciiCharSet.GetBytes (name, 0, size, bytes, 0);
					bytes[size++] = (byte)':';
					var isEmpty = string.IsNullOrEmpty (field.Value);
					if (!isEmpty)
					{
						if ((field.Value[0] != ' ') && (field.Value[0] != '\t') && ((field.Value[0] != '\r') || (field.Value.Length < 2) || (field.Value[1] != '\n')))
						{
							bytes[size++] = (byte)' ';
						}

						AsciiCharSet.GetBytes (field.Value, 0, field.Value.Length, bytes, size);
						size += field.Value.Length;
					}

					bytes[size++] = (byte)'\r';
					bytes[size++] = (byte)'\n';
					await destination.WriteAsync (bytes, 0, size, cancellationToken).ConfigureAwait (false);
					totalSize += size;
				}

				ArrayPool<byte>.Shared.Return (bytes);

				return totalSize;
			}
		}
	}
}
