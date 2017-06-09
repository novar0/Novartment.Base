using System.Data;

namespace Novartment.Base.Data
{
	/// <summary>
	/// Значение и его тип для обмена с БД.
	/// </summary>
	public struct DbValue
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса DbValue с указанным значением и типом.
		/// </summary>
		/// <param name="value">Значение.</param>
		/// <param name="type">Тип значения.</param>
		public DbValue (object value, DbType type)
		{
			this.Value = value;
			this.Type = type;
		}

		/// <summary>
		/// Значение.
		/// </summary>
		public object Value { get; }

		/// <summary>
		/// Тип значения.
		/// </summary>
		public DbType Type { get; }

		/// <summary>
		/// Деконструирует данные.
		/// </summary>
		/// <param name="value">Получает значение.</param>
		/// <param name="type">Получает тип значения.</param>
		public void Deconstruct (out object value, out DbType type)
		{
			value = this.Value;
			type = this.Type;
		}
	}
}
