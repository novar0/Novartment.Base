using System;
using System.Threading;

namespace Novartment.Base
{
	/// <summary>
	/// Предоставляет хранилище для данных, локальных для потока.
	/// </summary>
	/// <typeparam name="T">Тип значения-начинки.</typeparam>
	public sealed class ThreadLocalValueHolder<T> : ThreadLocal<T>,
		ILazyValueHolder<T>
	{
		/// <summary>
		/// Инициализирует экземпляр ThreadLocal.
		/// </summary>
		public ThreadLocalValueHolder ()
			: base ()
		{
		}

		/// <summary>
		/// Инициализирует экземпляр ThreadLocal с заданной функцией valueFactory.
		/// </summary>
		/// <param name="valueFactory">Объект System.Func&lt;TResult&gt;,
		/// вызываемый для получения неактивно инициализированного значения
		/// при совершении попытки получить Value без предварительной инициализации.</param>
		public ThreadLocalValueHolder (Func<T> valueFactory)
			: base (valueFactory)
		{
		}

		/// <summary>
		/// Инициализирует экземпляр ThreadLocal.
		/// </summary>
		/// <param name="trackAllValues">Следует ли отслеживать все значения, заданные в экземпляре,
		/// и представлять их с помощью свойства Values.</param>
		public ThreadLocalValueHolder (bool trackAllValues)
			: base (trackAllValues)
		{
		}

		/// <summary>
		/// Инициализирует экземпляр ThreadLocal с заданной функцией valueFactory.
		/// </summary>
		/// <param name="valueFactory">Объект System.Func&lt;TResult&gt;,
		/// вызываемый для получения неактивно инициализированного значения
		/// при совершении попытки получить Value без предварительной инициализации.</param>
		/// <param name="trackAllValues">Следует ли отслеживать все значения, заданные в экземпляре,
		/// и представлять их с помощью свойства Values.</param>
		public ThreadLocalValueHolder (Func<T> valueFactory, bool trackAllValues)
			: base (valueFactory, trackAllValues)
		{
		}
	}
}
