using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace Novartment.Base.IO
{
	/// <summary>
	/// Методы расширения ICryptoTransform для работы с файлами.
	/// </summary>
	public static class CryptoTransformFileExtensions
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
		/// <returns>Задача, представляющая асинхронную операцию хэширования.</returns>
		public static Task HashFileAsync (
			this ICryptoTransform hashAlgorithm,
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
				return Task.FromCanceled (cancellationToken);
			}

			ObservableStream readStream = null;
			CryptoStream cryptoStream = null;
			IDisposable subscription = null;
			try
			{
				readStream = new ObservableStream (new FileStream (fileName, FileMode.Open, FileAccess.Read, FileShare.Read));
				cryptoStream = new CryptoStream (readStream, hashAlgorithm, CryptoStreamMode.Read);
				if (progress != null)
				{
					subscription = readStream.Subscribe (progress.AsObserver ());
				}
				// TODO: сделать конфигурируемым размер буфера
				var task = cryptoStream.CopyToAsync (Stream.Null, 0x14000, cancellationToken);
				return HashFileAsyncFinalizer (task, readStream, cryptoStream, subscription);
			}
			catch
			{
				subscription?.Dispose ();
				cryptoStream?.Dispose ();
				readStream?.Dispose ();
				throw;
			}
		}

		private static async Task HashFileAsyncFinalizer (Task task,
			IDisposable disposable1,
			IDisposable disposable2,
			IDisposable disposable3)
		{
			try
			{
				// TODO: сделать конфигурируемым размер буфера
				await task.ConfigureAwait (false);
			}
			finally
			{
				disposable3?.Dispose ();
				disposable2?.Dispose ();
				disposable1?.Dispose ();
			}
		}

		/// <summary>
		/// Копирует существующий файл в новый файл
		/// с применением крипто-трансформации к содержимому и с
		/// поддержкой уведомления о прогрессе.
		/// </summary>
		/// <param name="cryptoTransform">Алгоритм крипто-транформации.</param>
		/// <param name="sourceFileName">Имя исходного файла для копирования.</param>
		/// <param name="destinationFileName">Имя нового файла, в который будет выполняться копирование.</param>
		/// <param name="progress">Объект, получающий уведомления о прогрессе операции.
		/// Укажите null если отслеживать прогресс не требуется.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая асинхронную операцию копирования.</returns>
		public static Task TransformFileAsync (
			this ICryptoTransform cryptoTransform,
			string sourceFileName,
			string destinationFileName,
			IProgress<FileStreamStatus> progress,
			CancellationToken cancellationToken)
		{
			if (cryptoTransform == null)
			{
				throw new ArgumentNullException (nameof (cryptoTransform));
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
				return Task.FromCanceled (cancellationToken);
			}

			ObservableStream readStream = null;
			FileStream writeStream = null;
			CryptoStream cryptoStream = null;
			IDisposable subscription = null;
			try
			{
				readStream = new ObservableStream (new FileStream (sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read));
				writeStream = new FileStream (destinationFileName, FileMode.Create, FileAccess.Write, FileShare.None);
				cryptoStream = new CryptoStream (readStream, cryptoTransform, CryptoStreamMode.Read);
				if (progress != null)
				{
					subscription = readStream.Subscribe (progress.AsObserver ());
				}
				// TODO: сделать конфигурируемым размер буфера
				var task = cryptoStream.CopyToAsync (writeStream, 0x14000, cancellationToken);
				return TransformFileAsyncFinalizer (task, readStream, writeStream, cryptoStream, subscription);
			}
			catch
			{
				subscription?.Dispose ();
				cryptoStream?.Dispose ();
				writeStream?.Dispose ();
				readStream?.Dispose ();
				throw;
			}
		}

		private static async Task TransformFileAsyncFinalizer (Task task,
			IDisposable disposable1,
			IDisposable disposable2,
			IDisposable disposable3,
			IDisposable disposable4)
		{
			try
			{
				// TODO: сделать конфигурируемым размер буфера
				await task.ConfigureAwait (false);
			}
			finally
			{
				disposable4?.Dispose ();
				disposable3?.Dispose ();
				disposable2?.Dispose ();
				disposable1?.Dispose ();
			}
		}
	}
}
