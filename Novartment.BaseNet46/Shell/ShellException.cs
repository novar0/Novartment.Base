using System;
using System.Diagnostics.CodeAnalysis;

namespace Novartment.Base.Shell
{
	/// <summary>
	/// Исключение при работе оболочки.
	/// </summary>
	[SuppressMessage (
		"Microsoft.Usage",
		"CA2237:MarkISerializableTypesWithSerializable",
		Justification = "In PCL Exception is not ISerializable.")]
	[SuppressMessage (
		"Microsoft.Design",
		"CA1032:ImplementStandardExceptionConstructors",
		Justification = "Constructors with SerializationInfo are not PCL-compatible.")]
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
