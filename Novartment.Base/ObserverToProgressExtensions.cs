using System;
using System.Diagnostics.Contracts;

namespace Novartment.Base
{
	/// <summary>
	/// Методы расширения для ковертирования друг в друга подписчиков на события типов IObserver/IProgress.
	/// </summary>
	public static class ObserverToProgressExtensions
	{
		/// <summary>
		/// Создаёт объёкт-наблюдатель, реализующий IObserver&lt;&gt;
		/// на основе указанного провайдера прогресса, реализующего IProgress&lt;&gt;.
		/// </summary>
		/// <typeparam name="T">Тип объекта, передаваемого в виде параметра прогресса.</typeparam>
		/// <param name="progressProvider">Провайдера прогресса, реализующий IProgress&lt;&gt;.</param>
		/// <returns>
		/// Объёкт-наблюдатель, реализующий IObserver&lt;&gt;
		/// на основе указанного провайдера прогресса, реализующего IProgress&lt;&gt;.
		/// </returns>
		public static IObserver<T> AsObserver<T> (this IProgress<T> progressProvider)
		{
			if (progressProvider == null)
			{
				throw new ArgumentNullException (nameof (progressProvider));
			}
			Contract.EndContractBlock ();

			return new ProgressToObserverTranslator<T> (progressProvider);
		}

		/// <summary>
		/// Создаёт провайдер прогресса, реализующий IProgress&lt;&gt;
		/// на основе указанного объёкта-наблюдателя, реализующего IObserver&lt;&gt;.
		/// </summary>
		/// <typeparam name="T">Тип объекта, передаваемого в виде параметра прогресса.</typeparam>
		/// <param name="observer">Объёкт-наблюдатель, реализующий IObserver&lt;&gt;.</param>
		/// <returns>
		/// Провайдер прогресса, реализующий IProgress&lt;&gt;
		/// на основе указанного объёкта-наблюдателя, реализующего IObserver&lt;&gt;.
		/// </returns>
		public static IProgress<T> AsProgressProvider<T> (this IObserver<T> observer)
		{
			if (observer == null)
			{
				throw new ArgumentNullException (nameof (observer));
			}
			Contract.EndContractBlock ();

			return new ObserverToProgressTranslator<T> (observer);
		}

		#region class ProgressToObserverTranslator<T>

		internal class ProgressToObserverTranslator<T> :
			IObserver<T>
		{
			private readonly IProgress<T> _progressProvider;

			internal ProgressToObserverTranslator (IProgress<T> progressProvider)
			{
				_progressProvider = progressProvider;
			}

			void IObserver<T>.OnNext (T value)
			{
				_progressProvider.Report (value);
			}

			void IObserver<T>.OnCompleted () { }

			void IObserver<T>.OnError (Exception error) { }
		}

		#endregion

		#region class ObserverToProgressTranslator<T>

		internal class ObserverToProgressTranslator<T> :
			IProgress<T>
		{
			private readonly IObserver<T> _observer;

			internal ObserverToProgressTranslator (IObserver<T> observer)
			{
				_observer = observer;
			}

			void IProgress<T>.Report (T value)
			{
				_observer.OnNext (value);
			}
		}

		#endregion
	}
}
