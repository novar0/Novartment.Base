namespace Novartment.Base.Text
{
	/// <summary>
	/// Тип элемента структурированного значения.
	/// </summary>
	public enum StructuredStringTokenType : byte
	{
		/// <summary>Тип не указан.</summary>
		Unspecified = 0,

		/// <summary>Знак, разделяющий другие элементы.</summary>
		Separator = 1,

		/// <summary>Значение, целиком состоящее из символов опредёлнного класса.</summary>
		Value = 2,

		/// <summary>Значение, ограниченное определёнными символами (кавычками, скобками и т.д.).</summary>
		DelimitedValue = 3,
	}
}
