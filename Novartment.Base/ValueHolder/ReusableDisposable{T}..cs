using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Novartment.Base
{
	/// <summary>
	/// Потоко-безопасный класс-обёртка для хранения объектов, подлежащих освобождению.
	/// При освобождении также освобождается хранимый объект.
	/// При смене значения, старое значение освобождается.
	/// </summary>
	/// <typeparam name="T">Тип значения-начинки, должен быть ссылочным и реализовать System.IDisposable.</typeparam>
	[SuppressMessage (
		"Microsoft.Design",
		"CA1063:ImplementIDisposableCorrectly",
		Justification = "Implemented correctly.")]
	public class ReusableDisposable<T> :
		IDisposable,
		IValueHolder<T>
		where T : class, IDisposable
	{
		private T _holder;

		/// <summary>Инициализирует новый экземпляр класса ReusableDisposable&lt;T&gt;.</summary>
		public ReusableDisposable ()
		{
			_holder = null;
		}

		/// <summary>Инициализирует новый экземпляр класса ReusableDisposable&lt;T&gt;,
		/// содержащий указанное значение.</summary>
		/// <param name="initialValue">Начальное значение для хранения.</param>
		public ReusableDisposable (T initialValue)
		{
			_holder = initialValue;
		}

		/// <summary>Получает или устанавливает значение-начинку.</summary>
		/// <returns>Значение-начинка.</returns>
		public T Value
		{
			get => _holder;
			set
			{
				Interlocked.Exchange (ref _holder, value)?.Dispose ();
			}
		}

		/// <summary>Производит освобождение (вызов Dispose()) хранимого объекта.</summary>
		[SuppressMessage (
			"Microsoft.Usage",
			"CA1816:CallGCSuppressFinalizeCorrectly",
			Justification = "There is no meaning to introduce a finalizer in derived type.")]
		[SuppressMessage (
			"Microsoft.Design",
			"CA1063:ImplementIDisposableCorrectly",
			Justification = "Implemented correctly.")]
		public void Dispose ()
		{
			Value = null;
		}
	}
}
