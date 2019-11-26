using System.Data;

namespace Novartment.Base.Data
{
#pragma warning disable CA1815 // Override equals and operator equals on value types
	/// <summary>
	/// Значение и его тип для обмена с БД.
	/// </summary>
	public readonly struct DbValue
#pragma warning restore CA1815 // Override equals and operator equals on value types
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса DbValue с указанным значением и типом.
		/// </summary>
		/// <param name="value">Значение.</param>
		/// <param name="dbType">Тип значения.</param>
		public DbValue (object value, DbType dbType)
		{
			this.Value = value;
			this.DbType = dbType;
		}

		/// <summary>
		/// Значение.
		/// </summary>
		public readonly object Value { get; }

		/// <summary>
		/// Тип значения.
		/// </summary>
		public readonly DbType DbType { get; }

		/// <summary>
		/// Деконструирует данные.
		/// </summary>
		/// <param name="value">Получает значение.</param>
		/// <param name="dbType">Получает тип значения.</param>
		public readonly void Deconstruct (out object value, out DbType dbType)
		{
			value = this.Value;
			dbType = this.DbType;
		}
	}
}
