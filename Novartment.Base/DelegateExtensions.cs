using System;

namespace Novartment.Base
{
	/// <summary>
	/// Методы преобразования делегатов,
	/// облегчающие использование существующих методов для передачи в виде параметров типа делегат.
	/// </summary>
	/// <remarks>
	/// Позволяют избегать вставки лямбда-выражений и создания дополнительных анонимных классов.
	/// </remarks>
	public static class DelegateExtensions
	{
#pragma warning disable SA1618 // Generic type parameters must be documented
#pragma warning disable SA1615 // Element return value must be documented
#pragma warning disable SA1611 // Element parameters must be documented
		/// <summary>
		/// Меняет тип параметра указанного делегата на базовый, от которого он наследован.
		/// Создает Action&lt;TBase&gt; из Action&lt;TInherited&gt;,
		/// позволяя передать метод, требующий параметр наследованного типа туда,
		/// где требуется метод с параметром базового типа.
		/// </summary>
		public static void ParameterAsBaseType<TBase, TInherited> (this Action<TInherited> action, TBase parameter)
			where TInherited : TBase
		{
			action.Invoke ((TInherited)parameter);
		}

		/// <summary>
		/// Меняет тип параметра указанного делегата на базовый, от которого он наследован.
		/// Создает Func&lt;TBase, TResult&gt; из Func&lt;TInherited, TResult&gt;,
		/// позволяя передать метод, требующий параметр наследованного типа туда,
		/// где требуется метод с параметром базового типа.
		/// </summary>
		public static TResult ParameterAsBaseType<TBase, TInherited, TResult> (this Func<TInherited, TResult> action, TBase parameter)
			where TInherited : TBase
		{
			return action.Invoke ((TInherited)parameter);
		}

		/// <summary>
		/// Меняет тип параметра указанного делегата на Object.
		/// Создает Action&lt;Object&gt; из Action&lt;T&gt;,
		/// позволяя передать метод, требующий параметр конкретного типа туда,
		/// где требуется метод с параметром общего типа Object.
		/// </summary>
		public static void ParameterAsObject<T> (this Action<T> action, object parameter)
		{
			action.Invoke ((T)parameter);
		}

		/// <summary>
		/// Меняет тип первого параметра указанного делегата на Object.
		/// Создает Action&lt;Object, T2&gt; из Action&lt;T1, T2&gt;,
		/// позволяя передать метод, требующий параметр конкретного типа туда,
		/// где требуется метод с параметром общего типа Object.
		/// </summary>
		public static void ParameterAsObject<T1, T2> (this Action<T1, T2> action, object parameter1, T2 parameter2)
		{
			action.Invoke ((T1)parameter1, parameter2);
		}

		/// <summary>
		/// Меняет тип первого параметра указанного делегата на Object.
		/// Создает Action&lt;object, T2, T3&gt; из Action&lt;T1, T2, T3&gt;,
		/// позволяя передать метод, требующий параметр конкретного типа туда,
		/// где требуется метод с параметром общего типа Object.
		/// </summary>
		public static void ParameterAsObject<T1, T2, T3> (this Action<T1, T2, T3> action, object parameter1, T2 parameter2, T3 parameter3)
		{
			action.Invoke ((T1)parameter1, parameter2, parameter3);
		}

		/// <summary>
		/// Меняет тип параметра указанного делегата на Object.
		/// Создает Func&lt;Object, TResult&gt; из Func&lt;T, TResult&gt;,
		/// позволяя передать метод, требующий параметр конкретного типа туда,
		/// где требуется метод с параметром общего типа Object.
		/// </summary>
		public static TResult ParameterAsObject<T, TResult> (this Func<T, TResult> function, object parameter)
		{
			return function.Invoke ((T)parameter);
		}

		/// <summary>
		/// Меняет тип первого параметра указанного делегата на Object.
		/// Создает Func&lt;Object, T2, TResult&gt; из Func&lt;T1, T2, TResult&gt;,
		/// позволяя передать метод, требующий параметр конкретного типа туда,
		/// где требуется метод с параметром общего типа Object.
		/// </summary>
		public static TResult ParameterAsObject<T1, T2, TResult> (this Func<T1, T2, TResult> function, object parameter1, T2 parameter2)
		{
			return function.Invoke ((T1)parameter1, parameter2);
		}

		/// <summary>
		/// Меняет тип первого параметра указанного делегата на Object.
		/// Создает Func&lt;Object, T2, T3, TResult&gt; из Func&lt;T1, T2, T3, TResult&gt;,
		/// позволяя передать метод, требующий параметр конкретного типа туда,
		/// где требуется метод с параметром общего типа Object.
		/// </summary>
		public static TResult ParameterAsObject<T1, T2, T3, TResult> (this Func<T1, T2, T3, TResult> function, object parameter1, T2 parameter2, T3 parameter3)
		{
			return function.Invoke ((T1)parameter1, parameter2, parameter3);
		}

		/// <summary>
		/// Декаррирует указанный делегат, снабжая его дополнительным параметром указанного типа.
		/// Создает Action&lt;T&gt; из Action,
		/// позволяя передать метод, не требующий параметров туда, где требуется метод с параметром.
		/// </summary>
		public static void AddParameter<T> (
			this Action action,
#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable CA1801 // Remove unused parameter
			T parameter)
#pragma warning restore CA1801 // Remove unused parameter
#pragma warning restore IDE0060 // Remove unused parameter
		{
			action.Invoke ();
		}

		/// <summary>
		/// Декаррирует указанный делегат, снабжая его дополнительным параметром указанного типа.
		/// Создает Func&lt;T, TResult&gt; из Func&lt;TResult&gt;,
		/// позволяя передать метод, не требующий параметров туда, где требуется метод с параметром.
		/// </summary>
		public static TResult AddParameter<T, TResult> (
			this Func<TResult> function,
#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable CA1801 // Remove unused parameter
			T parameter)
#pragma warning restore CA1801 // Remove unused parameter
#pragma warning restore IDE0060 // Remove unused parameter
		{
			return function.Invoke ();
		}
#pragma warning restore SA1611 // Element parameters must be documented
#pragma warning restore SA1615 // Element return value must be documented
#pragma warning restore SA1618 // Generic type parameters must be documented
	}
}
