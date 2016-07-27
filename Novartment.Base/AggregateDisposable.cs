using System;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics.Contracts;
using static System.Linq.Enumerable;

namespace Novartment.Base
{
	/// <summary>Класс для хранения коллекции IDisposable-объектов.</summary>
	public sealed class AggregateDisposable :
		IDisposable
	{
		private IReadOnlyCollection<IDisposable> _disposables;

		/// <summary>Инициализирует новый экземпляр класса.</summary>
		/// <param name="disposable1">Объект, который будет освобождён при освобождении этого экземпляра.</param>
		/// <param name="disposable2">Объект, который будет освобождён при освобождении этого экземпляра.</param>
		public AggregateDisposable (IDisposable disposable1, IDisposable disposable2)
		{
			_disposables = new[] { disposable1, disposable2 };
		}

		/// <summary>Инициализирует новый экземпляр класса.</summary>
		/// <param name="disposable1">Объект, который будет освобождён при освобождении этого экземпляра.</param>
		/// <param name="disposable2">Объект, который будет освобождён при освобождении этого экземпляра.</param>
		/// <param name="disposable3">Объект, который будет освобождён при освобождении этого экземпляра.</param>
		public AggregateDisposable (IDisposable disposable1, IDisposable disposable2, IDisposable disposable3)
		{
			_disposables = new[] { disposable1, disposable2, disposable3 };
		}

		/// <summary>Инициализирует новый экземпляр класса.</summary>
		/// <param name="disposable1">Объект, который будет освобождён при освобождении этого экземпляра.</param>
		/// <param name="disposable2">Объект, который будет освобождён при освобождении этого экземпляра.</param>
		/// <param name="disposable3">Объект, который будет освобождён при освобождении этого экземпляра.</param>
		/// <param name="disposable4">Объект, который будет освобождён при освобождении этого экземпляра.</param>
		public AggregateDisposable (IDisposable disposable1, IDisposable disposable2, IDisposable disposable3, IDisposable disposable4)
		{
			_disposables = new[] { disposable1, disposable2, disposable3, disposable4 };
		}

		/// <summary>Инициализирует новый экземпляр класса.</summary>
		/// <param name="disposables">Последовательность объектов, которые будут освобождёны при освобождении этого экземпляра.</param>
		public AggregateDisposable (IReadOnlyCollection<IDisposable> disposables)
		{
			if (disposables == null)
			{
				throw new ArgumentNullException ("disposables");
			}
			Contract.EndContractBlock ();

			_disposables = disposables;
		}

		/// <summary>Производит освобождение (вызов Dispose()) для всех хранимых объектов.</summary>
		public void Dispose ()
		{
			var list = Interlocked.Exchange (ref _disposables, null);
			if (list != null)
			{
				foreach (var item in list)
				{
					item?.Dispose ();
				}
			}
		}
	}
}
