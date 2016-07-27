using System;

namespace Novartment.Base
{
	/// <summary>
	/// Данные о событии.
	/// </summary>
	/// <typeparam name="T">Тип значения-начинки, содержащей данные о событии.</typeparam>
	public class DataEventArgs<T> : EventArgs,
		IValueHolder<T>
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса DataEventArgs&lt;T&gt; содержащий указанные данные.
		/// </summary>
		/// <param name="data">Данные о событии.</param>
		public DataEventArgs (T data)
			: base ()
		{
			this.Value = data;
		}

		/// <summary>
		/// Получает объект, содержащий данные о событии.
		/// </summary>
		public T Value { get; }
	}
}