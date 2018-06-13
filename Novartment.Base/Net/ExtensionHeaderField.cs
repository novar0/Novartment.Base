using System;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Net
{
	/// <summary>
	/// Поле заголовка, для использования которого требуется расширение (нестандартное поле).
	/// </summary>
	public class ExtensionHeaderField : HeaderField
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса ExtensionHeaderField с указанным именем и значением.
		/// </summary>
		/// <param name="extensionName">Имя поля заголовка.</param>
		/// <param name="body">Тело поля заголовка в кодированном виде, используемом для передачи по сетевым протоколам.</param>
		public ExtensionHeaderField (string extensionName, ReadOnlySpan<byte> body)
			: base (HeaderFieldName.Extension, body)
		{
			if (extensionName == null)
			{
				throw new ArgumentNullException (nameof (extensionName));
			}

			Contract.EndContractBlock ();

			this.ExtensionName = extensionName;
		}

		/// <summary>
		/// Получает имя поля заголовка.
		/// </summary>
		public string ExtensionName { get; }
	}
}
