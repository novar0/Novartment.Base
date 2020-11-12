using System;
using System.Data.Common;
using System.Threading;

namespace Novartment.Base.Data.SqlWrapper
{
	/// <summary>Класс для хранения любого реализующего IDisposable значения плюс связанная БД-команда.
	/// При освобождении будут освобождены и значение и связанная команда.
	/// </summary>
	/// <typeparam name="T">Тип значения-начинки.</typeparam>
	public sealed class DisposableValueLinkedWithDbCommand<T> :
		IDisposable,
		IValueHolder<T>
		where T : class, IDisposable
	{
		private T _value;
		private DbCommand _dbCommand;

		/// <summary>Инициализирует новый экземпляр класса DisposableValueLinkedWithDbCommand.</summary>
		/// <param name="value">Значение, которое будет освобождено при освобождении этого экземпляра.</param>
		/// <param name="dbCommand">Связанная БД-команда, которая будет освобождёна при освобождении этого экземпляра.</param>
		public DisposableValueLinkedWithDbCommand(T value, DbCommand dbCommand)
		{
			_value = value;
			_dbCommand = dbCommand;
		}

		/// <summary>Получает значение-начинку.</summary>
		/// <returns>Значение-начинка, указанное при создании объекта.</returns>
		public T Value => _value;

		/// <summary>Производит освобождение всех хранимых объектов.</summary>
		public void Dispose ()
		{
			Interlocked.Exchange (ref _value, null)?.Dispose ();
			Interlocked.Exchange (ref _dbCommand, null)?.Dispose ();
		}
	}
}
