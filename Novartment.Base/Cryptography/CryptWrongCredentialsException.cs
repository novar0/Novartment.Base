using System;
using System.Diagnostics.CodeAnalysis;

namespace Novartment.Base
{
	/// <summary>
	/// Ошибка работы с шифрованием из-за несоответствия учетной записи.
	/// </summary>
	[SuppressMessage (
		"Microsoft.Design",
		"CA1032:ImplementStandardExceptionConstructors",
		Justification = "Constructor with custom message not allowed.")]
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
	}
}
