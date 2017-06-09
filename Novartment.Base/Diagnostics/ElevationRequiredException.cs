using System;

namespace Novartment.Base
{
	/// <summary>Ошибка, представляющая недостаток привилегий.</summary>
	public class ElevationRequiredException : Exception
	{
		/// <summary>Инициализирует новый экземпляр класса ElevationRequiredException.</summary>
		public ElevationRequiredException ()
			: base ("Requested action requires elevated privilegies.")
		{
		}

		/// <summary>Инициализирует новый экземпляр класса ElevationRequiredException с указанием сообщения.</summary>
		/// <param name="message">Сообщение, описывающее ошибку.</param>
		public ElevationRequiredException (string message)
			: base (message)
		{
		}

		/// <summary>Инициализирует новый экземпляр класса ElevationRequiredException с указанием сообщения.</summary>
		/// <param name="message">Сообщение, описывающее ошибку.</param>
		/// <param name="innerException">Исключение, приведшее к создаваемому исключению, или null-ссылка если не указано.</param>
		public ElevationRequiredException (string message, Exception innerException)
			: base (message, innerException)
		{
		}
	}
}
