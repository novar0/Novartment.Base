using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Novartment.Base
{
	/// <summary>
	/// Базовый класс для реализации таймера.
	/// </summary>
	/// <typeparam name="TState">Тип объекта, передаваемый в делегат срабатывания таймера.</typeparam>
	public abstract class BaseTimer<TState> :
		ITimer
	{
		private readonly Action<TState> _callback;
		private readonly TState _state;

		/// <summary>
		/// Инициализирует новый экземпляр BaseTimer, вызывающий при срабатывании указанный делегат с указанным параметром.
		/// </summary>
		/// <param name="callback">Делегат, вызываемый при срабатывании таймера.</param>
		/// <param name="state">Объект, передавамый в делегат при срабатывании таймера.</param>
		protected BaseTimer (Action<TState> callback, TState state)
		{
			if (callback == null)
			{
				throw new ArgumentNullException (nameof (callback));
			}

			Contract.EndContractBlock ();

			_callback = callback;
			_state = state;
		}

		/// <summary>
		/// Получает или устанавливает интервал срабатывания таймера.
		/// </summary>
		public abstract TimeSpan Interval { get; set; }

		/// <summary>
		/// Получает состояние таймера. True если таймер запущен, иначе false.
		/// </summary>
		public abstract bool Enabled { get; }

		/// <summary>
		/// Запускает таймер.
		/// </summary>
		public abstract void Start ();

		/// <summary>
		/// Останавливает таймер.
		/// </summary>
		public abstract void Stop ();

		/// <summary>
		/// Освобождает занимаемые объектом ресурсы.
		/// </summary>
		[SuppressMessage (
			"Microsoft.Design",
			"CA1063:ImplementIDisposableCorrectly",
			Justification = "Implemented correctly.")]
		public abstract void Dispose ();

		/// <summary>
		/// Вызывает делегат срабатывания таймера.
		/// Можно использовать для делегата типа TimerCallback.
		/// </summary>
		/// <param name="notUsed">Не используется.</param>
		protected void DoCallback (object notUsed)
		{
			_callback.Invoke (_state);
		}

		/// <summary>
		/// Вызывает делегат срабатывания таймера.
		/// Можно использовать для делегата типа EventHandler.
		/// </summary>
		/// <param name="notUsed1">notUsed1 не используется.</param>
		/// <param name="notUsed2">notUsed2 не используется.</param>
		protected void DoCallback (object notUsed1, EventArgs notUsed2)
		{
			_callback.Invoke (_state);
		}

		/// <summary>
		/// Вызывает делегат срабатывания таймераа.
		/// Можно использовать для делегата типа Action.
		/// </summary>
		protected void DoCallback ()
		{
			_callback.Invoke (_state);
		}
	}
}
