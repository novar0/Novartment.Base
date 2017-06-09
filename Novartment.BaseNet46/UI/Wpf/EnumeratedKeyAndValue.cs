using System;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Значение перечисления, связанное со строковым представлением.
	/// </summary>
	public class EnumeratedKeyAndValue
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса EnumeratedKeyAndValue с указанным значением и строковым представлением.
		/// </summary>
		/// <param name="key">Значение перечисления.</param>
		/// <param name="value">Строковое представление значения перечисления.</param>
		public EnumeratedKeyAndValue (Enum key, string value)
		{
			this.Key = key;
			this.Value = value;
		}

		/// <summary>
		/// Получает значение перечисления.
		/// </summary>
		public Enum Key { get; }

		/// <summary>
		/// Получает строковое представление значения перечисления.
		/// </summary>
		public string Value { get; }
	}
}
