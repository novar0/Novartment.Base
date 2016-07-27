using System;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Novartment.Base
{
	/// <summary>
	/// Класс, отслеживающий оригинальность создаваемых экземпляров с одинаковым именем.
	/// Только экземпляр созданный в отсутствии других экземпляров с тем же именем,
	/// будет иметь свойство оригинальности.
	/// </summary>
	[SuppressMessage (
		"Microsoft.Design",
		"CA1063:ImplementIDisposableCorrectly",
		Justification = "Implemented correctly.")]
	public class SystemWideSingleton :
		IDisposable
	{
		private readonly Mutex _mutex;
		private bool _isInstanceOriginal;

		/// <summary>
		/// Получает признак оригинальности объекта.
		/// True означает что объект оригинален, False - что объект является копией.
		/// </summary>
		public bool IsOriginal => _isInstanceOriginal;

		/// <summary>
		/// Инициализирует новый экземпляр SystemWideSingleton с указанным именем.
		/// </summary>
		/// <param name="uniqueName">Уникальное имя, отличающее создаваемые экземпляры.</param>
		public SystemWideSingleton (string uniqueName)
		{
			if (uniqueName == null)
			{
				throw new ArgumentNullException (nameof (uniqueName));
			}
			if (uniqueName.Length < 8)
			{
				throw new ArgumentOutOfRangeException (nameof (uniqueName), "Specified name is not enough unique.");
			}
			Contract.EndContractBlock ();

			_mutex = new Mutex (false, uniqueName, out _isInstanceOriginal);
		}

		/// <summary>
		/// Освобождает объект, давая возможность вновь создаваемым объектам становиться уникальными.
		/// </summary>
		[SuppressMessage ("Microsoft.Usage",
			"CA1816:CallGCSuppressFinalizeCorrectly",
			Justification = "There is no meaning to introduce a finalizer in derived type."),
		SuppressMessage (
			"Microsoft.Design",
			"CA1063:ImplementIDisposableCorrectly",
			Justification = "Implemented correctly.")]
		public void Dispose ()
		{
			if (IsOriginal)
			{
				_isInstanceOriginal = false;
				_mutex.Dispose ();
			}
		}
	}
}
