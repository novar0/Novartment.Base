using System;
using System.Diagnostics.CodeAnalysis;

namespace Novartment.Base
{
	/// <summary>Ошибка выполнения процесса.</summary>
	[SuppressMessage (
		"Microsoft.Design",
		"CA1032:ImplementStandardExceptionConstructors",
		Justification = "Constructor with custom message or inner exception not allowed.")]
	public class ProcessFailedException : Exception
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

		/// <summary>Получает код ошибки, с которой завершился процесс.</summary>
		public int ExitCode { get; }
	}
}
