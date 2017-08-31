using System;

namespace Novartment.Base
{
#pragma warning disable CA1032 // Implement standard exception constructors
	/// <summary>Ошибка выполнения процесса.</summary>
	public class ProcessFailedException : Exception
#pragma warning restore CA1032 // Implement standard exception constructors
	{
		/// <summary>Инициализирует новый экземпляр ProcessFailedException с указанным кодом ошибки.</summary>
		/// <param name="exitCode">Код ошибки, с которой завершился процесс.</param>
		public ProcessFailedException (int exitCode)
			: this (FormattableString.Invariant ($"Process exit with error {exitCode}."), exitCode)
		{
		}

		/// <summary>Инициализирует новый экземпляр ProcessFailedException с указанным сообщением и кодом ошибки.</summary>
		/// <param name="message">Сообщение ошибки, с которой завершился процесс.</param>
		/// <param name="exitCode">Код ошибки, с которой завершился процесс.</param>
		public ProcessFailedException (string message, int exitCode)
			: base (message)
		{
			this.ExitCode = exitCode;
		}

		/// <summary>Инициализирует новый экземпляр ProcessFailedException с указанным сообщением и кодом ошибки.</summary>
		/// <param name="message">Сообщение ошибки, с которой завершился процесс.</param>
		/// <param name="exitCode">Код ошибки, с которой завершился процесс.</param>
		/// <param name="innerException">Исключение, приведшее к создаваемому исключению, или null-ссылка если не указано.</param>
		public ProcessFailedException (string message, int exitCode, Exception innerException)
			: base (message, innerException)
		{
			this.ExitCode = exitCode;
		}

		/// <summary>Получает код ошибки, с которой завершился процесс.</summary>
		public int ExitCode { get; }
	}
}
