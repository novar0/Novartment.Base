using System;

namespace Novartment.Base.Shell
{
	/// <summary>
	/// Исключение при работе оболочки.
	/// </summary>
	public class ShellException : Exception
	{
		/// <summary>
		/// Инициализирует новый экземпляр ShellException.
		/// </summary>
		public ShellException ()
			: base ()
		{
		}

		/// <summary>
		/// Инициализирует новый экземпляр ShellException с указанным сообщением.
		/// </summary>
		/// <param name="message">Сообщение исключения.</param>
		public ShellException (string message)
			: base (message)
		{
		}

		/// <summary>
		/// Инициализирует новый экземпляр ShellException с указанным сообщением и внутренним исключением.
		/// </summary>
		/// <param name="message">Сообщение исключения.</param>
		/// <param name="innerException">Внутреннее исключение.</param>
		public ShellException (string message, Exception innerException)
			: base (message, innerException)
		{
		}
	}
}
