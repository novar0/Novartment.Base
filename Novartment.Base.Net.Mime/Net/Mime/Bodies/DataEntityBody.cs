using System;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Тело MIME-сущности с простым (discrete) содержимым,
	/// хранящимся в виде массива байтов.
	/// </summary>
	public class DataEntityBody :
		IDiscreteEntityBody
	{
		private const int DefaultEncodeBufferSize = 32000;
		private ReadOnlyMemory<byte> _encodedData;

		/// <summary>
		/// Инициализирует новый экземпляр класса DataEntityBody
		/// использующий указанную кодировку передачи содержимого.
		/// </summary>
		/// <param name="transferEncoding">Кодировка передачи содержимого.</param>
		public DataEntityBody (ContentTransferEncoding transferEncoding)
		{
			if (transferEncoding == ContentTransferEncoding.Unspecified)
			{
				throw new ArgumentOutOfRangeException (nameof (transferEncoding));
			}

			this.TransferEncoding = transferEncoding;
		}

		/// <summary>Получает кодировку передачи содержимого.
		/// Соответствует полю заголовка "Content-Transfer-Encoding" определённому в RFC 2045 часть 6.</summary>
		public ContentTransferEncoding TransferEncoding { get; }

		/// <summary>
		/// Очищает тело сущности.
		/// </summary>
		public void Clear ()
		{
			_encodedData = default;
		}

		/// <summary>
		/// Загружает тело сущности из указанного источника данных.
		/// </summary>
		/// <param name="source">Источник данных, содержащий тело сущности.</param>
		/// <param name="subBodyFactory">Фабрика, позволяющая создавать тело вложенных сущностей с указанными параметрами.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая операцию.</returns>
		public async Task LoadAsync (
			IBufferedSource source,
			Func<EssentialContentProperties, IEntityBody> subBodyFactory,
			CancellationToken cancellationToken = default)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			var result = await source.ReadAllBytesAsync (cancellationToken).ConfigureAwait (false);
			_encodedData = result;
		}

		/// <summary>
		/// Saves this entity in the specified binary data destination.
		/// </summary>
		/// <param name="destination">The binary data destination, in which this entity will be saved.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>A task that represents the operation.</returns>
		public async Task SaveAsync (IBinaryDestination destination, CancellationToken cancellationToken = default)
		{
			if (destination == null)
			{
				throw new ArgumentNullException (nameof (destination));
			}

			var isEmptyBody = _encodedData.Length < 1;
			if (!isEmptyBody)
			{
				await destination.WriteAsync (_encodedData, cancellationToken).ConfigureAwait (false);
			}

			// RFC 5321 part 4.1.1.4:
			// ... if the message body were passed to the originating SMTP-sender with a final "line" that did not end in <CRLF>;
			// in that case, the originating SMTP system MUST either reject the message as invalid or
			// add <CRLF> in order to have the receiving SMTP server recognize the "end of data" condition.
			if (isEmptyBody ||
				(_encodedData.Length < 2) ||
				(_encodedData.Span[_encodedData.Length - 2] != HeaderDecoder.CarriageReturnLinefeed[0]) ||
				(_encodedData.Span[_encodedData.Length - 1] != HeaderDecoder.CarriageReturnLinefeed[1]))
			{
				await destination.WriteAsync (HeaderDecoder.CarriageReturnLinefeed, cancellationToken).ConfigureAwait (false);
			}
		}

		/// <summary>
		/// Возвращает декодированное тело сущности в виде источника данных.
		/// </summary>
		/// <returns>Декодированное тело сущности в виде источника данных.</returns>
		/// <remarks>
		/// Метод является неоптимальным, потому что оборачивает в асинхронный вид синхронные действия.
		/// По возможности используйте синхронный вариант этого метода.
		/// </remarks>
		public IBufferedSource GetDataSource ()
		{
			if (_encodedData.Length < 1)
			{
				throw new InvalidOperationException ("Entity body does not have any data.");
			}

			IBufferedSource src = new MemoryBufferedSource (_encodedData);
			IBufferedSource dst;
			switch (this.TransferEncoding)
			{
				case ContentTransferEncoding.QuotedPrintable:
					var transform = new FromQuotedPrintableConverter ();
					var buf1 = CreateCryptoOutputBuffer (transform, _encodedData.Length);
					dst = new CryptoTransformingBufferedSource (src, transform, buf1);
					break;
				case ContentTransferEncoding.Base64:
					var transform2 = new FromBase64Converter ();
					var buf2 = CreateCryptoOutputBuffer (transform2, _encodedData.Length);
					dst = new CryptoTransformingBufferedSource (src, transform2, buf2);
					break;
				case ContentTransferEncoding.EightBit:
				case ContentTransferEncoding.SevenBit:
				case ContentTransferEncoding.Binary:
					dst = src;
					break;
				default:
					throw new NotSupportedException (FormattableString.Invariant ($"Content-Transfer-Encoding '{this.TransferEncoding}' not supported."));
			}

			return dst;
		}

		/// <summary>
		/// Устанавливает тело сущности считывая данные из указанного источника.
		/// </summary>
		/// <param name="data">Источник данных, содержимое которого станет телом сущности.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, результатом которой является количество байтов,
		/// которое заняли данные в теле сущности в закодированном виде.</returns>
		public async Task<int> SetDataAsync (IBufferedSource data, CancellationToken cancellationToken = default)
		{
			if (data == null)
			{
				throw new ArgumentNullException (nameof (data));
			}

			switch (this.TransferEncoding)
			{
				case ContentTransferEncoding.QuotedPrintable:
					var transform = new ToQuotedPrintableWithLineBreaksConverter (this is TextEntityBody);
					data = new CryptoTransformingBufferedSource (
						data,
						transform,
						CreateCryptoOutputBuffer (transform, data.IsExhausted ? data.Count : 0));
					break;
				case ContentTransferEncoding.Base64:
					var transform2 = new ToBase64WithLineBreaksConverter ();
					data = new CryptoTransformingBufferedSource (
						data,
						transform2,
						CreateCryptoOutputBuffer (transform2, data.IsExhausted ? data.Count : 0));
					break;
				case ContentTransferEncoding.EightBit:
				case ContentTransferEncoding.SevenBit:
				case ContentTransferEncoding.Binary:
					break;
				default:
					throw new NotSupportedException (
						FormattableString.Invariant ($"Content-Transfer-Encoding '{this.TransferEncoding}' not supported."));
			}

			var result = await BufferedSourceExtensions.ReadAllBytesAsync (data, cancellationToken).ConfigureAwait (false);

			_encodedData = result;
			return result.Length;
		}

		/// <summary>
		/// Создаёт байтовый буфер для результатов указанного криптографического преобразования
		/// руководствуясь указанной подсказкой о размере входных данных.
		/// </summary>
		/// <param name="cryptoTransform">Криптографическое преобразование, для результатов которого создаётся буфер.</param>
		/// <param name="inputSizeHint">Подсказка о размере входных данных, может быть ноль если о размере ничего не известно.</param>
		/// <returns>Созданный байтовый буфер.</returns>
		private static byte[] CreateCryptoOutputBuffer (ISpanCryptoTransform cryptoTransform, int inputSizeHint)
		{
			if (cryptoTransform == null)
			{
				throw new ArgumentNullException (nameof (cryptoTransform));
			}

			int blocks;
			if (inputSizeHint < 1)
			{
				blocks = DefaultEncodeBufferSize / cryptoTransform.OutputBlockSize;
			}
			else
			{
				blocks = inputSizeHint / cryptoTransform.InputBlockSize;
				if ((inputSizeHint % cryptoTransform.InputBlockSize) > 0)
				{
					blocks++;
				}
			}

			if (blocks < 1)
			{
				blocks = 1;
			}

			return new byte[blocks * cryptoTransform.OutputBlockSize];
		}
	}
}
