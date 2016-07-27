using System;
using System.Threading;
using System.Diagnostics.Contracts;

namespace Novartment.Base
{
	/// <summary>
	/// Производитель освобождаемых объектов, которые при освобождении вызывают делегат.
	/// </summary>
	public static class DisposeAction
	{
		private readonly static IDisposable _empty = new DisposeActionHolder (null);

		/// <summary>
		/// Получает освобождаемый объект-заглушку, который ничего не делает при освобождении.
		/// </summary>
		public static IDisposable Empty => _empty;

		/// <summary>
		/// Создаёт освобождаемый объект, который при освобождении вызывает указанный делегат.
		/// </summary>
		/// <param name="disposeAction">Делегат, который будет вызван при освобождении объекта.</param>
		/// <returns>Созданный освобождаемый объект, который при освобождении вызывает указанный делегат.</returns>
		/// <remarks>
		/// Для конкурентного доступа синхронизация не требуется:
		/// при освобождении объекта указанный делегат будет вызван гарантированно только один раз.
		/// </remarks>
		public static IDisposable Create (Action disposeAction)
		{
			if (disposeAction == null)
			{
				throw new ArgumentNullException (nameof (disposeAction));
			}
			Contract.EndContractBlock ();

			return new DisposeActionHolder (disposeAction);
		}

		/// <summary>
		/// Создаёт освобождаемый объект, который при освобождении вызывает указанный делегат с указанным параметром.
		/// </summary>
		/// <param name="disposeAction">Делегат, который будет вызван при освобождении объекта.</param>
		/// <param name="state">Объект-состояние, который будет параметром при вызове делегата при освобождении объекта.</param>
		/// <returns>Созданный освобождаемый объект, который при освобождении вызывает указанный делегат.</returns>
		/// <remarks>
		/// Для конкурентного доступа синхронизация не требуется:
		/// при освобождении объекта указанный делегат будет вызван гарантированно только один раз.
		/// </remarks>
		public static IDisposable Create<TState> (Action<TState> disposeAction, TState state)
		{
			if (disposeAction == null)
			{
				throw new ArgumentNullException (nameof (disposeAction));
			}
			Contract.EndContractBlock ();

			return new DisposeActionHolderWithState<TState> (disposeAction, state);
		}

		private sealed class DisposeActionHolder :
			IDisposable
		{
			private Action _disposeAction;

			internal DisposeActionHolder (Action disposeAction)
			{
				_disposeAction = disposeAction;
			}

			public void Dispose ()
			{
				Interlocked.Exchange (ref _disposeAction, null)?.Invoke ();
			}
		}

		private sealed class DisposeActionHolderWithState<TState> :
			IDisposable
		{
			private Action<TState> _disposeAction;
			private TState _state;

			internal DisposeActionHolderWithState (Action<TState> disposeAction, TState state)
			{
				_disposeAction = disposeAction;
				_state = state;
			}

			public void Dispose ()
			{
				var value = Interlocked.Exchange (ref _disposeAction, null);
				if (value != null)
				{
					value.Invoke (_state);
					_state = default (TState);
				}
			}
		}
	}
}
