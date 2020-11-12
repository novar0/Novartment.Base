using System;
using System.Threading;

namespace Novartment.Base
{
	/// <summary>
	/// Потоко-безопасный класс-обёртка для хранения объектов, подлежащих освобождению.
	/// При освобождении также освобождается хранимый объект.
	/// При смене значения, старое значение освобождается.
	/// </summary>
	public sealed class ReusableDisposable :
		IDisposable,
		IValueHolder<IDisposable>
	{
		private IDisposable _holder;

		/// <summary>Инициализирует новый экземпляр класса ReusableDisposable.</summary>
		public ReusableDisposable ()
		{
			_holder = null;
		}

		/// <summary>Инициализирует новый экземпляр класса ReusableDisposable,
		/// содержащий указанное значение.</summary>
		/// <param name="initialValue">Начальное значение для хранения.</param>
		public ReusableDisposable (IDisposable initialValue)
		{
			_holder = initialValue;
		}

		/// <summary>Получает или устанавливает значение-начинку.</summary>
		/// <returns>Значение-начинка.</returns>
		public IDisposable Value
		{
			get => _holder;
			set
			{
				Interlocked.Exchange (ref _holder, value)?.Dispose ();
			}
		}

		/// <summary>Производит освобождение (вызов Dispose()) хранимого объекта.</summary>
		public void Dispose ()
		{
			this.Value = null;
		}
	}
}
