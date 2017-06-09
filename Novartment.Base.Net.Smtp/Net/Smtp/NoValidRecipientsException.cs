using System;
using System.Diagnostics.CodeAnalysis;

namespace Novartment.Base.Net.Smtp
{
	/// <summary>
	/// Отсутствуют подтверждённые получатели.
	/// </summary>
	[SuppressMessage (
		"Microsoft.Usage",
		"CA2237:MarkISerializableTypesWithSerializable",
		Justification = "In portable projects this class would not be ISerializable")]
	[SuppressMessage (
		"Microsoft.Design",
		"CA1032:ImplementStandardExceptionConstructors",
		Justification = "Constructor with custom message not allowed.")]
	public class NoValidRecipientsException : InvalidOperationException
	{
		/// <summary>Инициализирует новый экземпляр класса NoValidRecipientsException.</summary>
		public NoValidRecipientsException ()
			: base ("No valid recipients.")
		{
		}
	}
}
