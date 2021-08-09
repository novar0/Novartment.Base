using System;
using System.Globalization;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Collections;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Построитель поля заголовка из отдельных частей значения и параметров.
	/// Наследованные классы хранят нужные им значения и возвращаеют их частями в методе EncodeNextPart().
	/// </summary>
	public abstract class HeaderFieldBuilder
	{
		/// <summary>
		/// Максимально допустимая длина одной строки.
		/// Определено в RFC 2822 часть 2.1.1.
		/// </summary>
		public static readonly int MaxLineLengthRequired = 998;

		/// <summary>
		/// Максимально рекомендуемая длина одной строки.
		/// Определено в RFC 2822 часть 2.1.1.
		/// </summary>
		public static readonly int MaxLineLengthRecommended = 78;

		/// <summary>
		/// Максимально рекомендуемая длина 'encoded-word'.
		/// Определено в RFC 2047 часть 2.
		/// </summary>
		/// <remarks>
		/// An 'encoded-word' may not be more than 75 characters long, including 'charset', 'encoding', 'encoded-text', and delimiters.
		/// </remarks>
		public static readonly int MaxEncodedWordLength = 75;

		private readonly IAdjustableList<HeaderFieldBodyParameter> _parameters = new ArrayList<HeaderFieldBodyParameter> ();
		private readonly HeaderFieldName _name;

		/// <summary>
		/// Инициализирует новый экземпляр класса HeaderFieldBuilder с указанным именем и набором частей значения поля.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		protected HeaderFieldBuilder (HeaderFieldName name)
		{
			if (name == HeaderFieldName.Unspecified)
			{
				throw new ArgumentOutOfRangeException (nameof (name));
			}

			_name = name;
		}

		/// <summary>
		/// Добавляет параметр с указанным именем и значением.
		/// </summary>
		/// <param name="name">Имя параметра.</param>
		/// <param name="value">Значение параметра.</param>
		public void AddParameter (string name, string value)
		{
			_parameters.Add (new HeaderFieldBodyParameter (name, value));
		}

		/// <summary>
		/// Генерирует двоичное представление поле заголовка для передачи по протоколу.
		/// Производится фолдинг по указанной длине строки.
		/// </summary>
		/// <param name="destination">Буфер куда будет сгнерировано тело.</param>
		/// <param name="oneLineBuffer">Буфер для временного сохранения одной строки (максимально MaxLineLengthRequired байт).</param>
		/// <returns>Количество байт, записанных в буфер.</returns>
		public int EncodeToBinaryTransportRepresentation (Span<byte> destination, byte[] oneLineBuffer)
		{
			// RFC 5322:
			// 1) FWS (the folding white space token) indicates a place where folding may take place.
			// 1) token for "CFWS" is defined for places where comments and/or FWS can occur.
			// 2) a CarriageReturnLinefeed may be inserted before any WSP in FWS or CFWS.
			// 3) CarriageReturnLinefeed MUST NOT be inserted in such a way that any line of a folded header field is made up entirely of WSP characters and nothing else.

			// формируем склеенную из частей строку вставляя где надо переводы строки и пробелы
			var name = HeaderFieldNameHelper.GetName (_name);
			var lineLen = name.Length;
			AsciiCharSet.GetBytes (name.AsSpan (), destination);
			destination[lineLen++] = (byte)':';
			var outPos = lineLen;

			// кодируем все части значения тела
			PrepareToEncode (oneLineBuffer);
			bool isLast;
			do
			{
				var bufSlice = destination[outPos..];

				// тут наследованные классы вернут все содержащиеся в них части
				var partSize = EncodeNextPart (bufSlice, out isLast);

				if (partSize > 0)
				{
					// если есть параметры то последнюю часть значения дополняем знаком ';'
					if ((_parameters.Count > 0) && isLast)
					{
						bufSlice[partSize++] = (byte)';';
					}

					var foldedPartSize = FoldPartByLength (bufSlice, partSize, ref lineLen);
					outPos += foldedPartSize;
				}
			}
			while (!isLast);

			// кодируем все параметры
			for (var idx = 0; idx < _parameters.Count; idx++)
			{
				var parameter = _parameters[idx];
				var isLastParameter = idx >= (_parameters.Count - 1);

				// кодируем все сегменты параметра
				var isOneLine = false;
				var isToken = false;
				var isAscii = AsciiCharSet.IsAllOfClass (parameter.Value, AsciiCharClasses.Visible | AsciiCharClasses.WhiteSpace);
				if (isAscii)
				{
					var maxOneLineChars = MaxLineLengthRecommended - parameter.Name.Length - " =".Length;
					isToken = AsciiCharSet.IsAllOfClass (parameter.Value, AsciiCharClasses.Token);
					if (!isToken)
					{
						maxOneLineChars -= "\"\"".Length;
					}
					isOneLine = parameter.Value.Length <= maxOneLineChars;
				}

				outPos += (isOneLine) ?
					EncodeRegularParameter (parameter, isToken, isLastParameter, destination[outPos..], ref lineLen) :
					EncodeExtendedParameter (parameter,         isLastParameter, destination[outPos..], ref lineLen);
			}

			destination[outPos++] = (byte)'\r';
			destination[outPos++] = (byte)'\n';

			return outPos;
		}

		/// <summary>
		/// Записывает коллекцию полей в указанный получатель двоичных данных.
		/// </summary>
		/// <param name="fields">Коллекция полей, которая будет записана в получатель двоичных данных.</param>
		/// <param name="destination">Получатель двоичных данных, в который будет записана указанная коллекция полей.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Суммарное количество байтов, записанных в получатель двоичных данных.</returns>
		internal static async Task<int> SaveHeaderAsync (IReadOnlyCollection<HeaderFieldBuilder> fields, IBinaryDestination destination, CancellationToken cancellationToken = default)
		{
			if (fields == null)
			{
				throw new ArgumentNullException (nameof (fields));
			}

			if (destination == null)
			{
				throw new ArgumentNullException (nameof (destination));
			}

			// TODO: сделать сохранение оптом, а не по одному полю
			int totalSize = 0;
			var fieldBuffer = ArrayPool<byte>.Shared.Rent (HeaderDecoder.MaximumHeaderFieldBodySize);
			var oneLineBuffer = ArrayPool<byte>.Shared.Rent (MaxLineLengthRequired);
			try
			{
				foreach (var fieldBuilder in fields)
				{
					cancellationToken.ThrowIfCancellationRequested ();
					var size = fieldBuilder.EncodeToBinaryTransportRepresentation (fieldBuffer, oneLineBuffer);
					await destination.WriteAsync (fieldBuffer.AsMemory (0, size), cancellationToken).ConfigureAwait (false);
					totalSize += size;
				}
			}
			finally
			{
				ArrayPool<byte>.Shared.Return (oneLineBuffer);
				ArrayPool<byte>.Shared.Return (fieldBuffer);
			}

			return totalSize;
		}

		/// <summary>
		/// Подготавливает поле заголовка для вывода в двоичное представление.
		/// </summary>
		/// <param name="oneLineBuffer">Буфер для временного сохранения одной строки (максимально MaxLineLengthRequired байт).</param>
		protected virtual void PrepareToEncode (byte[] oneLineBuffer)
		{
		}

		/// <summary>
		/// Создаёт в указанном буфере очередную часть тела поля заголовка в двоичном представлении.
		/// Возвращает 0 если частей больше нет.
		/// Тело разбивается на части так, чтобы они были пригодны для фолдинга.
		/// </summary>
		/// <param name="buf">Буфер, куда будет записана чать.</param>
		/// <param name="isLast">Получает признак того, что полученная часть является последней.</param>
		/// <returns>Количество байтов, записанных в буфер.</returns>
		protected abstract int EncodeNextPart (Span<byte> buf, out bool isLast);

		// кодируем простое одностроковое значение (в кавычках если не токен)
		private static int EncodeRegularParameter (HeaderFieldBodyParameter parameter, bool isToken, bool isLastParameter, Span<byte> destination, ref int lineLen)
		{
			AsciiCharSet.GetBytes (parameter.Name.AsSpan (), destination);
			var outOffset = parameter.Name.Length;
			destination[outOffset++] = (byte)'=';
			if (!isToken)
			{
				destination[outOffset++] = (byte)'"';
			}

			AsciiCharSet.GetBytes (parameter.Value.AsSpan (), destination[outOffset..]);
			outOffset += parameter.Value.Length;
			if (!isToken)
			{
				destination[outOffset++] = (byte)'"';
			}

			// дополняем знаком ';' все параметры кроме последнего
			if (!isLastParameter)
			{
				destination[outOffset++] = (byte)';';
			}

			return FoldPartByLength (destination, outOffset, ref lineLen);
		}

		// кодируем сложное (требующее кодирования или многостроковое) значение согласно RFC 2231
		private static int EncodeExtendedParameter (HeaderFieldBodyParameter parameter, bool isLastParameter, Span<byte> destination, ref int lineLen)
		{
			/*
			RFC 2231 часть 4.1:
			1. Language and character set information only appear at the beginning of a given parameter value.
			2. Continuations do not provide a facility for using more than one character set or language in the same parameter value.
			3. A value presented using multiple continuations may contain a mixture of encoded and unencoded segments.
			4. The first segment of a continuation MUST be encoded if language and character set information are given.
			5. If the first segment of a continued parameter value is encoded the language and character set field delimiters MUST be present even when the fields are left blank.
			*/

			var encodingName = "utf-8";
			var hexOctets = Hex.OctetsUpper.Span;
			var asciiClasses = AsciiCharSet.ValueClasses.Span;

			var outPos = 0;
			var segmentIdx = 0;
			var maxOutCount = MaxLineLengthRecommended - " ;".Length;
			var outOffset = 0;
			AsciiCharSet.GetBytes (parameter.Name.AsSpan (), destination);
			outOffset += parameter.Name.Length;
			destination[outOffset++] = (byte)'*';
			var idxStr = segmentIdx.ToString (CultureInfo.InvariantCulture);
			AsciiCharSet.GetBytes (idxStr.AsSpan (), destination[outOffset..]);
			outOffset += idxStr.Length;
			destination[outOffset++] = (byte)'*';
			destination[outOffset++] = (byte)'=';
			AsciiCharSet.GetBytes (encodingName.AsSpan (), destination[outOffset..]);
			outOffset += encodingName.Length;
			destination[outOffset++] = (byte)'\'';
			destination[outOffset++] = (byte)'\'';

			// проходим по рунам чтобы не разрезать суррогатные пары
			// проходя по char, может получиться что суррогатная пара окажется разрезана и разнесена в разные сегменты
			Span<byte> buffer = stackalloc byte[8];
			foreach (var rune in parameter.Value.EnumerateRunes ())
			{
				var runeSize = rune.EncodeToUtf8 (buffer);
				var runePos = 0;
				var runeStartOffset = outOffset;
				while (runePos < runeSize)
				{
					var octet = buffer[runePos];
					var isToken = (octet != '%') && (octet < asciiClasses.Length) && ((asciiClasses[octet] & AsciiCharClasses.Token) != 0);
					var octetSize = isToken ? 1 : 3;

					// если очередной octet не влезает, то завершаем сегмент до начала руны и потом начинаем новый
					if ((outOffset + octetSize) > maxOutCount)
					{
						// дополняем знаком ';' все сегменты всех параметров кроме последнего
						destination[runeStartOffset++] = (byte)';';
						var foldedPartSize = FoldPartByLength (destination, runeStartOffset, ref lineLen);
						outPos += foldedPartSize;
						segmentIdx++;
						destination = destination[foldedPartSize..];
						outOffset = 0;
						AsciiCharSet.GetBytes (parameter.Name.AsSpan (), destination[outOffset..]);
						outOffset += parameter.Name.Length;
						destination[outOffset++] = (byte)'*';
						idxStr = segmentIdx.ToString (CultureInfo.InvariantCulture);
						AsciiCharSet.GetBytes (idxStr.AsSpan (), destination[outOffset..]);
						outOffset += idxStr.Length;
						destination[outOffset++] = (byte)'*';
						destination[outOffset++] = (byte)'=';
						runePos = 0;
						continue;
					}

					if (isToken)
					{
						destination[outOffset++] = octet;
					}
					else
					{
						// знак процента вместо символа, потом два шест.знака
						destination[outOffset++] = (byte)'%';
						var hex = hexOctets[octet];
						destination[outOffset++] = (byte)hex[0];
						destination[outOffset++] = (byte)hex[1];
					}

					runePos++;
				}
			}

			// дополняем знаком ';' все параметры кроме последнего
			if (!isLastParameter)
			{
				destination[outOffset++] = (byte)';';
			}

			var foldedPartSize2 = FoldPartByLength (destination, outOffset, ref lineLen);
			outPos += foldedPartSize2;

			return outPos;
		}

		// вставляет пробелы и переводы строки по мере фолдинга по указанной длине
		private static int FoldPartByLength (Span<byte> part, int partLength, ref int lineLen)
		{
			var needWhiteSpace = (part[0] != (byte)' ') && (part[0] != (byte)'\t');
			if (needWhiteSpace)
			{
				part.Slice (0, partLength).CopyTo (part[1..]);
				part[0] = (byte)' ';
				partLength++;
			}

			lineLen += partLength;
			if (lineLen > MaxLineLengthRecommended)
			{
				// если накопленная строка с добавлением новой части превысит maxLineLength, то перед новой частью добавляем перевод строки
				lineLen = partLength + 1; // плюс пробел
				part.Slice (0, partLength).CopyTo (part[2..]);
				part[0] = (byte)'\r';
				part[1] = (byte)'\n';
				partLength += 2;
			}

			return partLength;
		}
	}
}
