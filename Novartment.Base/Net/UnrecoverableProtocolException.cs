using System;
using System.Diagnostics.CodeAnalysis;

namespace Novartment.Base.Net
{
	/// <summary>
	/// Непоправимое нарушение протокола.
	/// </summary>
	[SuppressMessage (
		"Microsoft.Design",
		"CA1032:ImplementStandardExceptionConstructors",
		Justification = "Constructor with custom message or inner exception not allowed.")]
	[SuppressMessage (
		"Microsoft.Usage",
		"CA2237:MarkISerializableTypesWithSerializable",
		Justification = "In portable projects this class would not be ISerializable")]
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
