using System;
using System.Diagnostics.Contracts;
using System.Threading;

namespace Novartment.Base
{
	/// <summary>
	/// Класс, отслеживающий оригинальность создаваемых экземпляров с одинаковым именем.
	/// Только экземпляр созданный в отсутствии других экземпляров с тем же именем,
	/// будет иметь свойство оригинальности.
	/// </summary>
	public sealed class SystemWideSingleton :
		IDisposable
	{
		private readonly Mutex _mutex;
		private bool _isInstanceOriginal;

		/// <summary>
		/// Инициализирует новый экземпляр SystemWideSingleton с указанным именем.
		/// </summary>
		/// <param name="uniqueName">Уникальное имя, отличающее создаваемые экземпляры.</param>
		public SystemWideSingleton(string uniqueName)
		{
			if (uniqueName == null)
			{
				throw new ArgumentNullException(nameof(uniqueName));
			}

			if (uniqueName.Length < 8)
			{
				throw new ArgumentOutOfRangeException(nameof(uniqueName), "Specified name is not enough unique.");
			}

			Contract.EndContractBlock();

			_mutex = new Mutex(false, uniqueName, out _isInstanceOriginal);
		}

		/// <summary>
		/// Получает признак оригинальности объекта.
		/// True означает что объект оригинален, False - что объект является копией.
		/// </summary>
		public bool IsOriginal => _isInstanceOriginal;

		/// <summary>
		/// Освобождает объект, давая возможность вновь создаваемым объектам становиться уникальными.
		/// </summary>
		public void Dispose ()
		{
			if (this.IsOriginal)
			{
				_isInstanceOriginal = false;
				_mutex.Dispose ();
			}
		}
	}
}
