using System;
using System.Collections.Generic;
using System.Threading;

namespace Novartment.Base
{
	/// <summary>Класс для хранения коллекции IDisposable-объектов.</summary>
	public sealed class AggregateDisposable :
		IDisposable
	{
		private IReadOnlyCollection<IDisposable> _disposables;

		/// <summary>Инициализирует новый экземпляр класса.</summary>
		/// <param name="disposable1">Объект 1, который будет освобождён при освобождении этого экземпляра.</param>
		/// <param name="disposable2">Объект 2, который будет освобождён при освобождении этого экземпляра.</param>
		public AggregateDisposable (IDisposable disposable1, IDisposable disposable2)
		{
			_disposables = new[] { disposable1, disposable2 };
		}

		/// <summary>Инициализирует новый экземпляр класса.</summary>
		/// <param name="disposable1">Объект 1, который будет освобождён при освобождении этого экземпляра.</param>
		/// <param name="disposable2">Объект 2, который будет освобождён при освобождении этого экземпляра.</param>
		/// <param name="disposable3">Объект 3, который будет освобождён при освобождении этого экземпляра.</param>
		public AggregateDisposable (IDisposable disposable1, IDisposable disposable2, IDisposable disposable3)
		{
			_disposables = new[] { disposable1, disposable2, disposable3 };
		}

		/// <summary>Инициализирует новый экземпляр класса.</summary>
		/// <param name="disposable1">Объект 1, который будет освобождён при освобождении этого экземпляра.</param>
		/// <param name="disposable2">Объект 2, который будет освобождён при освобождении этого экземпляра.</param>
		/// <param name="disposable3">Объект 3, который будет освобождён при освобождении этого экземпляра.</param>
		/// <param name="disposable4">Объект 4, который будет освобождён при освобождении этого экземпляра.</param>
		public AggregateDisposable (IDisposable disposable1, IDisposable disposable2, IDisposable disposable3, IDisposable disposable4)
		{
			_disposables = new[] { disposable1, disposable2, disposable3, disposable4 };
		}

		/// <summary>Инициализирует новый экземпляр класса.</summary>
		/// <param name="disposables">Последовательность объектов, которые будут освобождёны при освобождении этого экземпляра.</param>
		public AggregateDisposable (IReadOnlyCollection<IDisposable> disposables)
		{
			_disposables = disposables ?? throw new ArgumentNullException (nameof (disposables));
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
