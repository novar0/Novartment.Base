using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
	/// Наследованные классы хранят нужные им значения и возвращаеют их частями в методе GetNextPart().
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
		/// Максимально допустимая длина 'encoded-word'.
		/// Определено в RFC 2047 часть 2.
		/// </summary>
		public static readonly int MaxEncodedWordLength = 75;

		private readonly IAdjustableList<HeaderFieldParameter> _parameters = new ArrayList<HeaderFieldParameter> ();
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

			Contract.EndContractBlock ();

			_name = name;
		}

		/// <summary>
		/// Добавляет параметр с указанным именем и значением.
		/// </summary>
		/// <param name="name">Имя параметра.</param>
		/// <param name="value">Значение параметра.</param>
		public void AddParameter (string name, string value)
		{
			_parameters.Add (new HeaderFieldParameter (name, value));
		}

		/// <summary>
		/// Генерирует двоичное представление поле заголовка для передачи по протоколу.
		/// Производится фолдинг по указанной длине строки.
		/// </summary>
		/// <param name="buf">Буфер куда будет сгнерировано тело.</param>
		/// <param name="maxLineLength">Максимальная длина строки, по которой будет производиться фолдинг значения поля заголовка.</param>
		/// <returns>Количество байт, записанных в буфер.</returns>
		public int EncodeToBinaryTransportRepresentation (Span<byte> buf, int maxLineLength)
		{
			// RFC 5322:
			// 1) FWS (the folding white space token) indicates a place where folding may take place.
			// 1) token for "CFWS" is defined for places where comments and/or FWS can occur.
			// 2) a CarriageReturnLinefeed may be inserted before any WSP in FWS or CFWS.
			// 3) CarriageReturnLinefeed MUST NOT be inserted in such a way that any line of a folded header field is made up entirely of WSP characters and nothing else.

			// формируем склеенную из частей строку вставляя где надо переводы строки и пробелы
			var name = HeaderFieldNameHelper.GetName (_name);
			var lineLen = name.Length;
			AsciiCharSet.GetBytes (name.AsSpan (), buf);
			buf[lineLen++] = (byte)':';
			var outPos = lineLen;

			// кодируем все части значения тела
			bool isLast;
			do
			{
				var bufSlice = buf.Slice (outPos);

				// тут наследованные классы вернут все содержащиеся в них части
				var partSize = GetNextPart (bufSlice, out isLast);

				if (partSize > 0)
				{
					// если есть параметры то последнюю часть значения дополняем знаком ';'
					if ((_parameters.Count > 0) && isLast)
					{
						bufSlice[partSize++] = (byte)';';
					}

					var foldedPartSize = FoldPartByLength (bufSlice, partSize, maxLineLength, ref lineLen);
					outPos += foldedPartSize;
				}
			} while (!isLast);

			// кодируем все параметры
			for (var idx = 0; idx < _parameters.Count; idx++)
			{
				var parameter = _parameters[idx];

				// кодируем все сегменты параметра
				var parameterValueBytes = Encoding.UTF8.GetBytes (parameter.Value);

				var segmentPos = 0;
				var segmentIdx = 0;
				while (true)
				{
					var bufSlice = buf.Slice (outPos);
					var partSize = HeaderFieldBodyParameterEncoder.GetNextSegment (parameter.Name, parameterValueBytes, bufSlice, segmentIdx, ref segmentPos);
					segmentIdx++;
					if (partSize < 1)
					{
						break;
					}

					var isLastSegment = segmentPos >= parameterValueBytes.Length;

					// дополняем знаком ';' все сегменты всех параметров кроме последнего
					if (isLastSegment && (idx != (_parameters.Count - 1)))
					{
						bufSlice[partSize++] = (byte)';';
					}

					var foldedPartSize = FoldPartByLength (bufSlice, partSize, maxLineLength, ref lineLen);
					outPos += foldedPartSize;
				}
			}

			buf[outPos++] = (byte)'\r';
			buf[outPos++] = (byte)'\n';

			return outPos;
		}

		/// <summary>
		/// Записывает коллекцию полей в указанный получатель двоичных данных.
		/// </summary>
		/// <param name="fields">Коллекция полей, которая будет записана в получатель двоичных данных.</param>
		/// <param name="destination">Получатель двоичных данных, в который будет записана указанная коллекция полей.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Суммарное количество байтов, записанных в получатель двоичных данных.</returns>
		internal static Task<int> SaveHeaderAsync (IReadOnlyCollection<HeaderFieldBuilder> fields, IBinaryDestination destination, CancellationToken cancellationToken = default)
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

			// TODO: сделать сохранение оптом, а не по одному полю
			async Task<int> SaveHeaderAsyncStateMachine ()
			{
				int totalSize = 0;
				var bytes = ArrayPool<byte>.Shared.Rent (HeaderDecoder.MaximumHeaderFieldBodySize);
				try
				{
					foreach (var fieldBuilder in fields)
					{
						cancellationToken.ThrowIfCancellationRequested ();
						var size = fieldBuilder.EncodeToBinaryTransportRepresentation (bytes, HeaderFieldBuilder.MaxLineLengthRecommended);
						await destination.WriteAsync (bytes.AsMemory (0, size), cancellationToken).ConfigureAwait (false);
						totalSize += size;
					}
				}
				finally
				{
					ArrayPool<byte>.Shared.Return (bytes);
				}

				return totalSize;
			}
		}

		// Метод получения очередной части тела поля заголовка, возвращает 0 если частей больше нет
		// тело разбивается на части так, чтобы они были пригодны для фолдинга
		protected abstract int GetNextPart (Span<byte> buf, out bool isLast);

		// вставляет пробелы и переводы строки по мере фолдинга по указанной длине
		private static int FoldPartByLength (Span<byte> part, int partLength, int maxLineLength, ref int lineLen)
		{
			var needWhiteSpace = (part[0] != (byte)' ') && (part[0] != (byte)'\t');
			if (needWhiteSpace)
			{
				part.Slice (0, partLength).CopyTo (part.Slice (1));
				part[0] = (byte)' ';
				partLength++;
			}

			lineLen += partLength;
			if (lineLen > maxLineLength)
			{
				// если накопленная строка с добавлением новой части превысит maxLineLength, то перед новой частью добавляем перевод строки
				lineLen = partLength + 1; // плюс пробел
				part.Slice (0, partLength).CopyTo (part.Slice (2));
				part[0] = (byte)'\r';
				part[1] = (byte)'\n';
				partLength += 2;
			}

			return partLength;
		}
	}
}
