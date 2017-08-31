using System;

namespace Novartment.Base
{
	/// <summary>
	/// Ошибка работы с шифрованием из-за несоответствия учетной записи.
	/// </summary>
	public class CryptWrongCredentialsException : Exception
	{
		/// <summary>Инициализирует новый экземпляр класса CryptWrongCredentialsException.</summary>
		public CryptWrongCredentialsException ()
			: base ("Data can not be decrypted using user's data.")
		{
		}

		/// <summary>Инициализирует новый экземпляр класса CryptWrongCredentialsException с указанием сообщения.</summary>
		/// <param name="innerException">Исключение, приведшее к создаваемому исключению, или null-ссылка если не указано.</param>
		public CryptWrongCredentialsException (Exception innerException)
			: base ("Data can not be decrypted using user's data.", innerException)
		{
		}

		/// <summary>Инициализирует новый экземпляр класса CryptWrongCredentialsException.</summary>
		/// <param name="message">Сообщение ошибки.</param>
		public CryptWrongCredentialsException (string message)
			: base (message)
		{
		}

		/// <summary>Инициализирует новый экземпляр класса CryptWrongCredentialsException.</summary>
		/// <param name="message">Сообщение ошибки.</param>
		/// <param name="innerException">Исключение, приведшее к создаваемому исключению, или null-ссылка если не указано.</param>
		public CryptWrongCredentialsException (string message, Exception innerException)
			: base (message, innerException)
		{
		}
	}
}
