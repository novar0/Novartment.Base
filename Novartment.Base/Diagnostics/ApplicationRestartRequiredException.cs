using System;

namespace Novartment.Base
{
	/// <summary>Ошибка, представляющая невозможность работы из-за требующегося перезапуска приложения.</summary>
	public class ApplicationRestartRequiredException : Exception
	{
		/// <summary>Инициализирует новый экземпляр класса ApplicationRestartRequiredException.</summary>
		public ApplicationRestartRequiredException () : base ("Application must be restarted.") { }

		/// <summary>Инициализирует новый экземпляр класса ApplicationRestartRequiredException с указанием сообщения.</summary>
		/// <param name="message">Сообщение, описывающее ошибку.</param>
		public ApplicationRestartRequiredException (string message) : base (message) { }

		/// <summary>Инициализирует новый экземпляр класса ApplicationRestartRequiredException с указанием сообщения.</summary>
		/// <param name="message">Сообщение, описывающее ошибку.</param>
		/// <param name="innerException">Исключение, приведшее к создаваемому исключению, или null-ссылка если не указано.</param>
		public ApplicationRestartRequiredException (string message, Exception innerException) : base (message, innerException) { }
	}
}
