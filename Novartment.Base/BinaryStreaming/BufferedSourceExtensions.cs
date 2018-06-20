﻿using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// Методы расширения для IBufferedSource.
	/// </summary>
	public static class BufferedSourceExtensions
	{
		/// <summary>
		/// Проверяет что указанный источник исчерпан и не содержит данных.
		/// </summary>
		/// <param name="source">Источник данных.</param>
		/// <returns>True если источник исчерпан и не содержит данных.</returns>
		public static bool IsEmpty (this IBufferedSource source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			return (source.Count < 1) && source.IsExhausted;
		}

		/// <summary>
		/// Пытается асинхронно пропустить указанное количество данных источника, включая доступные в буфере данные.
		/// При выполнении могут измениться свойства Offset, Count и IsExhausted.
		/// </summary>
		/// <param name="source">Источник данных.</param>
		/// <param name="size">Количество байтов данных для пропуска, включая доступные в буфере данные.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>
		/// Задача, результатом которой является количество пропущеных байтов данных, включая доступные в буфере данные.
		/// Может быть меньше, чем было указано, если источник исчерпался.
		/// После завершения задачи, независимо от её результата, источник будет предоставлять данные, идущие сразу за пропущенными.
		/// </returns>
		public static Task<long> TrySkipAsync (this IBufferedSource source, long size, CancellationToken cancellationToken)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if (size < 0L)
			{
				throw new ArgumentOutOfRangeException (nameof (size));
			}

			Contract.EndContractBlock ();

			if (source is IFastSkipBufferedSource fastSkipSource)
			{
				return fastSkipSource.TryFastSkipAsync(size, cancellationToken);
			}

			// источник не поддерживает быстрый пропуск,
			// поэтому будем пропускать путём последовательного считывания и пропуска буфера
			var available = source.Count;
			if (size <= (long)available)
			{
				// достаточно доступных данных буфера
				source.SkipBuffer ((int)size);
				return Task.FromResult (size);
			}

			if (source.IsExhausted)
			{
				// источник исчерпан
				source.SkipBuffer (available);
				return Task.FromResult ((long)available);
			}

			return TrySkipAsyncStateMachine();

			async Task<long> TrySkipAsyncStateMachine()
			{
				long skipped = 0L;
				do
				{
					// пропускаем всё что в буфере
					available = source.Count;
					source.SkipBuffer(available);
					size -= (long)available;
					skipped += (long)available;

					// заполняем буфер
					await source.FillBufferAsync(cancellationToken).ConfigureAwait(false);
				}
				while (!source.IsExhausted && (size > (long)source.Count));

				// пропускаем частично буфер
				var reminder = Math.Min(size, (long)source.Count);
				source.SkipBuffer((int)reminder);
				skipped += reminder;

				return skipped;
			}
		}

		/// <summary>
		/// Пытается асинхронно пропустить все оставшиеся данные источника, включая доступные в буфере данные.
		/// При выполнении могут измениться свойства Offset, Count и IsExhausted.
		/// </summary>
		/// <param name="source">Источник данных.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>
		/// Задача, результатом которой является количество пропущеных байтов данных.
		/// </returns>
		public static Task<long> SkipToEndAsync (this IBufferedSource source, CancellationToken cancellationToken)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			if (source is IFastSkipBufferedSource fastSkipSource)
			{
				return fastSkipSource.TryFastSkipAsync(long.MaxValue, cancellationToken);
			}

			var available = source.Count;
			source.SkipBuffer (available);
			if (source.IsExhausted)
			{
				// источник исчерпан
				return Task.FromResult ((long)available);
			}

			// источник не поддерживает быстрый пропуск,
			// поэтому будем пропускать путём последовательного считывания и пропуска буфера
			return SkipToEndStateMachine ();

			async Task<long> SkipToEndStateMachine ()
			{
				var skipped = (long)available;
				do
				{
					// заполняем буфер
					await source.FillBufferAsync (cancellationToken).ConfigureAwait (false);

					// пропускаем всё что в буфере
					available = source.Count;
					source.SkipBuffer (available);
					skipped += (long)available;
				}
				while (!source.IsExhausted);

				return skipped;
			}
		}

		/// <summary>
		/// Считывает указанное количество байтов в указанный массив по указанному смещению из указанного источника данных.
		/// </summary>
		/// <param name="source">Источник данных.</param>
		/// <param name="buffer">Массив байтов - приёмник данных.
		/// После возврата из метода, буфер в указанном диапазоне будет содержать данные
		/// считанные из источника.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, результатом которой является количество байтов, помещённых в буфер.
		/// Может быть меньше, чем запрошено, если источник исчерпан.</returns>
		public static Task<int> ReadAsync (
			this IBufferedSource source,
			Memory<byte> buffer,
#pragma warning disable CA1801 // Review unused parameters
			CancellationToken cancellationToken)
#pragma warning restore CA1801 // Review unused parameters
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			if (buffer.Length < 1)
			{
				Task.FromResult (0);
			}

			int totalSize = 0;
			if (source.Count > 0)
			{
				// копируем то, что уже есть в буфере
				totalSize = Math.Min (buffer.Length, source.Count);
				source.BufferMemory.Slice (source.Offset, totalSize).CopyTo (buffer);
				source.SkipBuffer (totalSize);
				buffer = buffer.Slice (totalSize);
			}

			if ((buffer.Length < 1) || source.IsExhausted)
			{
				return Task.FromResult (totalSize);
			}

			return ReadAsyncStateMachine ();

			async Task<int> ReadAsyncStateMachine ()
			{
				do
				{
					await source.FillBufferAsync (cancellationToken).ConfigureAwait (false);

					if (source.Count < 1)
					{
						break; // end of stream
					}
					else
					{
						var size = Math.Min (source.Count, buffer.Length);
						source.BufferMemory.Slice (source.Offset, size).CopyTo (buffer);
						source.SkipBuffer (size);
						totalSize += size;

						if (buffer.Length <= size)
						{
							break;
						}

						buffer = buffer.Slice (size);
					}
				}
				while (!source.IsExhausted);

				return totalSize;
			}
		}

#pragma warning disable CA1801 // Review unused parameters
		/// <summary>
		/// Асинхронно ищет первое нахождение указанного байта в буфере указанного источника данных,
		/// запрашивая заполнение буфера по необходимости.
		/// </summary>
		/// <param name="source">Источник данных.</param>
		/// <param name="value">Байт-образец, который будет найден в источнике данных.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Позиция первого нахождения указанного байта в буфере указанного источника данных,
		/// либо -1 если указанный байт в буфере не найден.</returns>
		public static Task<int> IndexOfAsync (this IBufferedSource source, byte value, CancellationToken cancellationToken)
#pragma warning restore CA1801 // Review unused parameters
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			// проверяем то, что уже есть в буфере
			var offset = source.Offset;
			var end = offset + source.Count;
			var sourceBuf = source.BufferMemory.Span;
			while (offset < end)
			{
				if (sourceBuf[offset] == value)
				{
					return Task.FromResult (offset);
				}

				offset++;
			}

			if (source.IsExhausted || // источник исчерпался
				((source.Offset < 1) && (offset >= source.BufferMemory.Length)))
			{
				// буфер полон, запрашивать данные больше нет смысла
				return Task.FromResult (-1);
			}

			// продолжение поиска с предварительным вызовом заполнения буфера
			return IndexOfAsyncStateMachine ();

			async Task<int> IndexOfAsyncStateMachine ()
			{
				while (!source.IsExhausted)
				{
					cancellationToken.ThrowIfCancellationRequested ();

					// запоминаем сколько мы уже просмотрели от начала, потому что начало сдвинется при запросе новых данных
					// запрашиваем дополнительные данные (старые данные останутся в буфере)
					var sizeScanned = offset - source.Offset;
					await source.FillBufferAsync (cancellationToken).ConfigureAwait (false);
					offset = source.Offset + sizeScanned;
					end = source.Offset + source.Count;
					if (offset >= end)
					{
						// запрос не добавил новых данных, что означает что в буфер больше ничего не влезет
						break;
					}

					while (offset < end)
					{
						if (source.BufferMemory.Span[offset] == value)
						{
							return offset;
						}

						offset++;
					}
				}

				// источник исчерпался или нет места в буфере, запрашивать данные больше нет смысла
				return -1;
			}
		}

		public static Task<int> CopyToBufferUntilMarkerAsync (this IBufferedSource source, byte marker, Memory<byte> buffer, CancellationToken cancellationToken)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			int outCount;

			// проверяем то, что уже есть в буфере
			var offset = source.Offset;
			var end = offset + source.Count;
			var sourceBuffer = source.BufferMemory.Span;
			while (offset < end)
			{
				outCount = offset - source.Offset;
				var b = sourceBuffer[offset];
				if (b == marker)
				{
					if (outCount > 0)
					{
						source.BufferMemory.Slice (source.Offset, outCount).CopyTo (buffer);
						source.SkipBuffer (outCount);
					}

					return Task.FromResult (outCount);
				}

				if (outCount >= buffer.Length)
				{
					throw new InvalidOperationException ("Too small destination.");
				}

				offset++;
			}

			outCount = source.Count;
			if (outCount > 0)
			{
				source.BufferMemory.Slice (source.Offset, outCount).CopyTo (buffer);
				source.SkipBuffer (outCount);
			}

			if (source.IsExhausted)
			{
				return Task.FromResult (outCount);
			}

			buffer = buffer.Slice (outCount);

			// продолжение поиска с предварительным вызовом заполнения буфера
			return CopyToBufferUntilMarkerStateMachine ();

			async Task<int> CopyToBufferUntilMarkerStateMachine ()
			{
				var totalOutCount = outCount;
				while (!source.IsExhausted)
				{
					cancellationToken.ThrowIfCancellationRequested ();

					await source.FillBufferAsync (cancellationToken).ConfigureAwait (false);
					offset = source.Offset;
					end = source.Offset + source.Count;

					while (offset < end)
					{
						outCount = offset - source.Offset;
						var b = source.BufferMemory.Span[offset];
						if (b == marker)
						{
							if (outCount > 0)
							{
								source.BufferMemory.Slice (source.Offset, outCount).CopyTo (buffer);
								source.SkipBuffer (outCount);
							}

							return totalOutCount + outCount;
						}

						if ((totalOutCount + outCount) >= buffer.Length)
						{
							throw new InvalidOperationException ("Too small destination.");
						}

						offset++;
					}

					outCount = source.Count;
					if (outCount > 0)
					{
						source.BufferMemory.Slice (source.Offset, outCount).CopyTo (buffer);
						source.SkipBuffer (outCount);
						buffer = buffer.Slice (outCount);
						totalOutCount += outCount;
					}
				}

				// источник исчерпался или нет места в буфере, запрашивать данные больше нет смысла
				return totalOutCount;
			}
		}

		public static Task<int> CopyToBufferUntilMarkerAsync (this IBufferedSource source, byte marker1, byte marker2, Memory<byte> buffer, CancellationToken cancellationToken)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			// нужен байт, который точно не равен маркеру
			var notMarker = 0;
			while ((notMarker == marker1) || (notMarker == marker2))
			{
				notMarker++;
			}

			int outCount;

			// проверяем то, что уже есть в буфере
			var offset = source.Offset;
			var end = offset + source.Count;
			var sourceBuf = source.BufferMemory.Span;
			while (offset < end)
			{
				outCount = offset - source.Offset;
				var b1 = sourceBuf[offset];
				if (b1 == marker1)
				{
					if ((offset == (end - 1)) && !source.IsExhausted)
					{
						// если буфер кончается маркером1 то его пока не рассматривать
						break;
					}
					else
					{
						var b2 = ((offset + 1) < end) ? sourceBuf[offset + 1] : notMarker;
						if (b2 == marker2)
						{
							if (outCount > 0)
							{
								source.BufferMemory.Slice (source.Offset, outCount).CopyTo (buffer);
								source.SkipBuffer (outCount);
							}

							return Task.FromResult (outCount);
						}
					}
				}

				if (outCount >= buffer.Length)
				{
					throw new InvalidOperationException ("Too small destination.");
				}

				offset++;
			}

			outCount = offset - source.Offset;
			if (outCount > 0)
			{
				source.BufferMemory.Slice (source.Offset, outCount).CopyTo (buffer);
				source.SkipBuffer (outCount);
			}

			if (source.IsExhausted)
			{
				return Task.FromResult (outCount);
			}

			buffer = buffer.Slice (outCount);

			// продолжение поиска с предварительным вызовом заполнения буфера
			return CopyToBufferUntilMarkerStateMachine ();

			async Task<int> CopyToBufferUntilMarkerStateMachine ()
			{
				var totalOutCount = outCount;
				while (!source.IsExhausted)
				{
					cancellationToken.ThrowIfCancellationRequested ();

					await source.FillBufferAsync (cancellationToken).ConfigureAwait (false);
					offset = source.Offset;
					end = source.Offset + source.Count;

					while (offset < end)
					{
						outCount = offset - source.Offset;
						var b1 = source.BufferMemory.Span[offset];
						if (b1 == marker1)
						{
							if ((offset == (end - 1)) && !source.IsExhausted)
							{
								// если буфер кончается маркером1 то его пока не рассматривать
								break;
							}
							else
							{
								var b2 = ((offset + 1) < end) ? source.BufferMemory.Span[offset + 1] : notMarker;
								if (b2 == marker2)
								{
									if (outCount > 0)
									{
										source.BufferMemory.Slice (source.Offset, outCount).CopyTo (buffer);
										source.SkipBuffer (outCount);
									}

									return totalOutCount + outCount;
								}
							}
						}

						if ((totalOutCount + outCount) >= buffer.Length)
						{
							throw new InvalidOperationException ("Too small destination.");
						}

						offset++;
					}

					outCount = offset - source.Offset;
					if (outCount > 0)
					{
						source.BufferMemory.Slice (source.Offset, outCount).CopyTo (buffer);
						source.SkipBuffer (outCount);
						buffer = buffer.Slice (outCount);
						totalOutCount += outCount;
					}
				}

				// источник исчерпался или нет места в буфере, запрашивать данные больше нет смысла
				return totalOutCount;
			}
		}

#pragma warning disable CA1801 // Review unused parameters
		/// <summary>
		/// Считывает все данные, оставшиеся в источнике и возвращает их в виде массива байтов.
		/// </summary>
		/// <param name="source">Источник данных.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, результатом которой является массив байтов, считанный из источника.</returns>
		/// <remarks>Возвращаемый массив является копией и не связан массивом-буфером источника.</remarks>
		public static Task<ReadOnlyMemory<byte>> ReadAllBytesAsync (this IBufferedSource source, CancellationToken cancellationToken)
#pragma warning restore CA1801 // Review unused parameters
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			if (source.IsExhausted)
			{
				var sizeExhausted = source.Count;
				var copy = new byte[sizeExhausted];
				if (sizeExhausted > 0)
				{
					source.BufferMemory.Slice (source.Offset, sizeExhausted).CopyTo (copy);
					source.SkipBuffer (sizeExhausted);
				}

				return Task.FromResult<ReadOnlyMemory<byte>> (copy);
			}

			return ReadAllBytesAsyncStateMachine ();

			async Task<ReadOnlyMemory<byte>> ReadAllBytesAsyncStateMachine ()
			{
				await source.FillBufferAsync (cancellationToken).ConfigureAwait (false);
				var size = source.Count;
				int totalSize = 0;
				var memStream = new ArrayBinaryDestination (size);
				while (size > 0)
				{
					cancellationToken.ThrowIfCancellationRequested ();
					if (((long)totalSize + (long)size) > (long)int.MaxValue)
					{
						throw new InvalidOperationException (FormattableString.Invariant (
							$"Too big size of data ({(long)totalSize + (long)size}). Supported maximum is {int.MaxValue}."));
					}

					memStream.Write (source.BufferMemory.Span.Slice (source.Offset, size));
					totalSize += size;
					source.SkipBuffer (size);
					await source.FillBufferAsync (cancellationToken).ConfigureAwait (false);
					size = source.Count;
				}

				return memStream.Buffer;
			}
		}

		/// <summary>
		/// Считывает все данные, оставшиеся в источнике и возвращает их в виде строки.
		/// </summary>
		/// <param name="source">Источник данных.</param>
		/// <param name="encoding">Кодировка, используемая для конвертации байтов в строку.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, результатом которой является строка, считанная из источника.</returns>
		public static Task<string> ReadAllTextAsync (this IBufferedSource source, Encoding encoding, CancellationToken cancellationToken)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if (encoding == null)
			{
				throw new ArgumentNullException (nameof (encoding));
			}

			Contract.EndContractBlock ();

			// нельзя декодировать частями, потому что неизвестно сколько байт занимают отдельные символы
			var task = ReadAllBytesAsync (source, cancellationToken);
			return ReadAllTextAsyncFinalizer ();

			async Task<string> ReadAllTextAsyncFinalizer ()
			{
				var buf = await task.ConfigureAwait (false);
#if NETCOREAPP2_1
				return encoding.GetString (buf.Span);
#else
				var tempBuf = new byte[buf.Length];
				buf.CopyTo (tempBuf);
				return encoding.GetString (tempBuf);
#endif
			}
		}

		/// <summary>
		/// Сохраняет содержимое указанного источника данных в указанный получатель двоичных данных.
		/// </summary>
		/// <param name="source">Источник данных, содержимое которого будет сохранено в указанный получатель двоичных данных.</param>
		/// <param name="destination">Получатель двоичных данных, в который будет сохранено содержимое указанного источника данных.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, результатом которой является количество байтов, записанный в указанный получатель двоичных данных.</returns>
		public static Task<long> WriteToAsync (
			this IBufferedSource source,
			IBinaryDestination destination,
#pragma warning disable CA1801 // Review unused parameters
			CancellationToken cancellationToken)
#pragma warning restore CA1801 // Review unused parameters
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if (destination == null)
			{
				throw new ArgumentNullException (nameof (destination));
			}

			Contract.EndContractBlock ();

			return WriteToAsyncStateMachine ();

			async Task<long> WriteToAsyncStateMachine ()
			{
				long resultSize = 0;
				while (true)
				{
					await source.FillBufferAsync (cancellationToken).ConfigureAwait (false);
					var available = source.Count;
					if (available <= 0)
					{
						break;
					}

					cancellationToken.ThrowIfCancellationRequested ();
					await destination.WriteAsync (source.BufferMemory.Slice (source.Offset, available), cancellationToken).ConfigureAwait (false);
					resultSize += available;
					source.SkipBuffer (available);
				}

				return resultSize;
			}
		}
	}
}
