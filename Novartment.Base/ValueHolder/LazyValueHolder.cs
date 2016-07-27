using System;
using System.Threading;

namespace Novartment.Base
{
	/// <summary>
	/// Обеспечивает поддержку отложенной инициализации.
	/// </summary>
	/// <typeparam name="T">Тип значения-начинки.</typeparam>
	public sealed class LazyValueHolder<T> : Lazy<T>,
		ILazyValueHolder<T>
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса LazyValueHolder&lt;T&gt;.
		/// При отложенной инициализации используется конструктор целевого типа по умолчанию.
		/// </summary>
		public LazyValueHolder () : base () { }

		/// <summary>
		/// Инициализирует новый экземпляр класса LazyValueHolder&lt;T&gt;.
		/// При отложенной инициализации используются конструктор целевого типа по умолчанию и заданный режим инициализации.
		/// </summary>
		/// <param name="isThreadSafe">Значение true, если необходимо разрешить параллельное использование этого экземпляра несколькими потоками;
		/// значение false, если необходимо разрешить одновременное использование этого экземпляра только одним потоком.</param>
		public LazyValueHolder (bool isThreadSafe) : base (isThreadSafe) { }

		/// <summary>
		/// Инициализирует новый экземпляр класса LazyValueHolder&lt;T&gt;.
		/// При отложенной инициализации используется заданная функция инициализации.
		/// </summary>
		/// <param name="valueFactory">Делегат, вызываемый для получения отложенно инициализированного значения, когда оно требуется.</param>
		public LazyValueHolder (Func<T> valueFactory) : base (valueFactory) { }

		/// <summary>
		/// Инициализирует новый экземпляр класса LazyValueHolder&lt;T&gt;,
		/// который использует конструктор T по умолчанию и заданный потокобезопасный режим.
		/// </summary>
		/// <param name="mode">Одно из значений перечисления, определяющее режим потокобезопасности.</param>
		public LazyValueHolder (LazyThreadSafetyMode mode) : base (mode) { }

		/// <summary>
		/// Инициализирует новый экземпляр класса LazyValueHolder&lt;T&gt;.
		/// При отложенной инициализации используются заданные функция и режим инициализации.
		/// </summary>
		/// <param name="valueFactory">Делегат, вызываемый для получения отложенно инициализированного значения, когда оно требуется.</param>
		/// <param name="isThreadSafe">Значение true, если необходимо разрешить параллельное использование этого экземпляра несколькими потоками.</param>
		public LazyValueHolder (Func<T> valueFactory, bool isThreadSafe) : base (valueFactory, isThreadSafe) { }

		/// <summary>
		/// Инициализирует новый экземпляр класса LazyValueHolder&lt;T&gt;,
		/// который использует заданную функцию инициализации и потокобезопасный режим.
		/// </summary>
		/// <param name="valueFactory">Делегат, вызываемый для получения отложенно инициализированного значения, когда оно требуется.</param>
		/// <param name="mode">Одно из значений перечисления, определяющее режим потокобезопасности.</param>
		public LazyValueHolder (Func<T> valueFactory, LazyThreadSafetyMode mode) : base (valueFactory, mode) { }
	}
}
