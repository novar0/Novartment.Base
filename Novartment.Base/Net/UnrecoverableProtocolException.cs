using System;

namespace Novartment.Base.Net
{
	/// <summary>
	/// Непоправимое нарушение протокола.
	/// </summary>
	public class UnrecoverableProtocolException : InvalidOperationException
	{
		/// <summary>Инициализирует новый экземпляр класса UnrecoverableProtocolException.</summary>
		public UnrecoverableProtocolException ()
			: base ("Unrecoverable protocol exception.")
		{
		}

		/// <summary>Инициализирует новый экземпляр класса UnrecoverableProtocolException.</summary>
		/// <param name="message">Сообщение.</param>
		public UnrecoverableProtocolException (string message)
			: base (message)
		{
		}

		/// <summary>Инициализирует новый экземпляр класса UnrecoverableProtocolException.</summary>
		/// <param name="message">Сообщение.</param>
		/// <param name="innerException">Исключение - причина.</param>
		public UnrecoverableProtocolException (string message, Exception innerException)
			: base (message, innerException)
		{
		}
	}
}
