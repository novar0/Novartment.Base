using System;

namespace Novartment.Base.BinaryStreaming
{
#pragma warning disable CA1032 // Implement standard exception constructors
	/// <summary>Ошибка нехватки данных.</summary>
	public class NotEnoughDataException : Exception
#pragma warning restore CA1032 // Implement standard exception constructors
	{
		/// <summary>Инициализирует новый экземпляр NotEnoughDataException с указанным количеством недостающих данных.</summary>
		/// <param name="shortage">Количество недостающих данных, приведшее к ошибке.</param>
		public NotEnoughDataException (long shortage)
			: this (FormattableString.Invariant ($"Source can not provide requested size of data. Shortage = {shortage}."), shortage)
		{
		}

		/// <summary>Инициализирует новый экземпляр NotEnoughDataException с указанным сообщением и количеством недостающих данных.</summary>
		/// <param name="message">Сообщение ошибки.</param>
		/// <param name="shortage">Количество недостающих данных, приведшее к ошибке.</param>
		public NotEnoughDataException (string message, long shortage)
			: base (message)
		{
			this.Shortage = shortage;
		}

		/// <summary>Инициализирует новый экземпляр NotEnoughDataException с указанным сообщением и количеством недостающих данных.</summary>
		/// <param name="message">Сообщение ошибки.</param>
		/// <param name="shortage">Количество недостающих данных, приведшее к ошибке.</param>
		/// <param name="innerException">Исключение, приведшее к создаваемому исключению, или null-ссылка если не указано.</param>
		public NotEnoughDataException (string message, long shortage, Exception innerException)
			: base (message, innerException)
		{
			this.Shortage = shortage;
		}

		/// <summary>Получает количество недостающих данных.</summary>
		public long Shortage { get; }
	}
}
