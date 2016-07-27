
namespace Novartment.Base.Text
{
	/// <summary>
	/// Тип элемента структурированного значения.
	/// </summary>
	public enum StructuredValueElementType
	{
		/// <summary>Тип не указан.</summary>
		Unspecified = 0,

		/// <summary>Знак, разделяющий другие элементы.</summary>
		Separator = 1,

		/// <summary>Атом.</summary>
		Value = 2,

		/// <summary>Значение в двойных кавычках.</summary>
		QuotedValue = 3,

		/// <summary>Значение в круглых скобках.</summary>
		RoundBracketedValue = 4,

		/// <summary>Значение в треугольных скобках.</summary>
		AngleBracketedValue = 5,

		/// <summary>Значение в квадратных скобках.</summary>
		SquareBracketedValue = 6
	}
}
