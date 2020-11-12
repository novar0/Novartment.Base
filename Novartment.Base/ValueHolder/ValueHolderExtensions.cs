using System;
using System.Diagnostics.Contracts;

namespace Novartment.Base
{
	/// <summary>
	/// Предоставляет набор статических методов для выполнения действий с объектами,
	/// реализующими интерфейс IValueHolder&lt;T&gt;.
	/// </summary>
	public static class ValueHolderExtensions
	{
		/// <summary>
		/// Освобождает объект и его начинку путём вызова их метода IDisposable.Dispose ().
		/// </summary>
		/// <typeparam name="T">Тип объекта-начинки.</typeparam>
		/// <param name="holder">Объект, который необходимо освободить вместе с начинкой.</param>
		/// <remarks>Не инициализирует объекты с отложенной инициализацией.
		/// Не вызывает исключений если объект не реализует интерфейс IDisposable.</remarks>
		public static void DisposeWithValue<T> (this IValueHolder<T> holder)
			where T : IDisposable
		{
			if (holder == null)
			{
				throw new ArgumentNullException (nameof (holder));
			}

			Contract.EndContractBlock ();

			if ((holder is not ILazyValueHolder<T> lazy) || lazy.IsValueCreated)
			{
				holder.Value?.Dispose();
			}

			(holder as IDisposable)?.Dispose ();
		}
	}
}
