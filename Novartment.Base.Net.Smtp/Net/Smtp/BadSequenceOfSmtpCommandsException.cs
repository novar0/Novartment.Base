using System;
using System.Diagnostics.CodeAnalysis;

namespace Novartment.Base.Net.Smtp
{
	/// <summary>
	/// Неверная последовательность команд SMTP.
	/// </summary>
	[SuppressMessage (
		"Microsoft.Usage",
		"CA2237:MarkISerializableTypesWithSerializable",
		Justification = "In portable projects this class would not be ISerializable")]
	[SuppressMessage (
		"Microsoft.Design",
		"CA1032:ImplementStandardExceptionConstructors",
		Justification = "Constructor with custom message not allowed.")]
	public class BadSequenceOfSmtpCommandsException : InvalidOperationException
	{
		/// <summary>Инициализирует новый экземпляр класса BadSequenceOfSmtpCommandsException.</summary>
		public BadSequenceOfSmtpCommandsException ()
			: base ("Bad sequence of SMTP commands.")
		{
		}
	}
}
