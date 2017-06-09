using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.IO
{
	/// <summary>
	/// Методы расширения ICryptoTransform для работы с файлами.
	/// </summary>
	public static class HashFileExtensions
	{
		/// <summary>
		/// Вычисляет хэш-функцию для файла, используя указанный алгоритм хэширования
		/// с поддержкой уведомления о прогрессе.
		/// </summary>
		/// <param name="hashAlgorithm">Алгоритм хэширования.</param>
		/// <param name="fileName">Имя файла для вычисления.</param>
		/// <param name="progress">Объект, получающий уведомления о прогрессе операции.
		/// Укажите null если отслеживать прогресс не требуется.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая асинхронную операцию хэширования,
		/// результатом которой будет вычисленный хэш.</returns>
		public static Task<byte[]> HashFileAsync (
			this IncrementalHash hashAlgorithm,
			string fileName,
			IProgress<FileStreamStatus> progress,
			CancellationToken cancellationToken)
		{
			if (hashAlgorithm == null)
			{
				throw new ArgumentNullException (nameof (hashAlgorithm));
			}

			if (fileName == null)
			{
				throw new ArgumentNullException (nameof (fileName));
			}

			Contract.EndContractBlock ();

			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<byte[]> (cancellationToken);
			}

			FileStream readStream = null;
			long length = 0L;
			try
			{
				readStream = new FileStream (fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
				length = readStream.Length;
				progress?.Report (new FileStreamStatus (0L, length, null));
				return HashFileImplementation ();
			}
			catch
			{
				readStream?.Dispose ();
				throw;
			}

			async Task<byte[]> HashFileImplementation ()
			{
				// TODO: сделать конфигурируемым размер буфера
				var buffer = new byte[0x14000];
				long position = 0L;
				try
				{
					while (true)
					{
						int bytesRead = await readStream.ReadAsync (buffer, 0, buffer.Length, cancellationToken).ConfigureAwait (false);
						if (bytesRead == 0)
						{
							return hashAlgorithm.GetHashAndReset ();
						}

						hashAlgorithm.AppendData (buffer, 0, bytesRead);
						position += bytesRead;
						progress?.Report (new FileStreamStatus (position, length, null));
					}
				}
				finally
				{
					readStream?.Dispose ();
				}
			}
		}

		/// <summary>
		/// Копирует существующий файл в новый файл
		/// с вычислением указанной хэш-функции по содержимому.
		/// Опционально посылает уведомления о прогрессе в указанный получатель уведомлений.
		/// </summary>
		/// <param name="hashAlgorithm">Алгоритм хэш-функции.</param>
		/// <param name="sourceFileName">Имя исходного файла для копирования.</param>
		/// <param name="destinationFileName">Имя нового файла, в который будет выполняться копирование.</param>
		/// <param name="progress">Объект, получающий уведомления о прогрессе операции.
		/// Укажите null если отслеживать прогресс не требуется.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая асинхронную операцию копирования,
		/// результатом которой будет вычисленный хэш.</returns>
		public static Task<byte[]> CopyFileWithHashingAsync (
			this IncrementalHash hashAlgorithm,
			string sourceFileName,
			string destinationFileName,
			IProgress<FileStreamStatus> progress,
			CancellationToken cancellationToken)
		{
			if (hashAlgorithm == null)
			{
				throw new ArgumentNullException (nameof (hashAlgorithm));
			}

			if (sourceFileName == null)
			{
				throw new ArgumentNullException (nameof (sourceFileName));
			}

			if (destinationFileName == null)
			{
				throw new ArgumentNullException (nameof (destinationFileName));
			}

			Contract.EndContractBlock ();

			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<byte[]> (cancellationToken);
			}

			FileStream readStream = null;
			FileStream writeStream = null;
			long length = 0L;
			try
			{
				readStream = new FileStream (sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
				length = readStream.Length;
				progress?.Report (new FileStreamStatus (0L, length, null));
				writeStream = new FileStream (destinationFileName, FileMode.Create, FileAccess.Write, FileShare.None);
				return CopyFileWithHashingImplementation ();
			}
			catch
			{
				writeStream?.Dispose ();
				readStream?.Dispose ();
				throw;
			}

			async Task<byte[]> CopyFileWithHashingImplementation ()
			{
				// TODO: сделать конфигурируемым размер буфера
				var buffer = new byte[0x14000];
				long position = 0L;
				try
				{
					while (true)
					{
						int bytesRead = await readStream.ReadAsync (buffer, 0, buffer.Length, cancellationToken).ConfigureAwait (false);
						if (bytesRead == 0)
						{
							return hashAlgorithm.GetHashAndReset ();
						}

						hashAlgorithm.AppendData (buffer, 0, bytesRead);
						await writeStream.WriteAsync (buffer, 0, bytesRead, cancellationToken).ConfigureAwait (false);
						position += bytesRead;
						progress?.Report (new FileStreamStatus (position, length, null));
					}
				}
				finally
				{
					writeStream?.Dispose ();
					readStream?.Dispose ();
				}
			}
		}
	}
}
