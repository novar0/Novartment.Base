using System;
using System.Diagnostics.CodeAnalysis;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>Ошибка нехватки данных.</summary>
	[SuppressMessage (
		"Microsoft.Design",
		"CA1032:ImplementStandardExceptionConstructors",
		Justification = "Constructor with inner exception not allowed.")]
	public class NotEnoughDataException : Exception
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

		/// <summary>Получает количество недостающих данных.</summary>
		public long Shortage { get; }
	}
}
