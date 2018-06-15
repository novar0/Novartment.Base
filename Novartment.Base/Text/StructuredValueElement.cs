namespace Novartment.Base.Text
{
	/// <summary>
	/// Элемент структурированного значения.
	/// </summary>
	public struct StructuredValueElement
	{
		private static readonly StructuredValueElement _invalid = new StructuredValueElement (StructuredValueElementType.Unspecified, -1, -1);

		/// <summary>
		/// Инициализирует новый экземпляр класса StructuredValueElement на основе указанного типа и кодированного значения.
		/// </summary>
		/// <param name="type">Тип тип, определяющий способ кодирования элемента.</param>
		/// <param name="startPosition">Позиция в source.</param>
		/// <param name="length">Количество элементов в source.</param>
		public StructuredValueElement (StructuredValueElementType type, int startPosition, int length)
		{
			this.ElementType = type;
			this.StartPosition = startPosition;
			this.Length = length;
		}

		/// <summary>
		/// Получает особый элемент-метку, который не может быть получен в результате обычного разбора.
		/// </summary>
		public static StructuredValueElement Invalid => _invalid;

		/// <summary>
		/// Получает тип, определяющий способ кодирования элемента.
		/// </summary>
		public StructuredValueElementType ElementType { get; }

		/// <summary>
		/// Получает начальную позицию элемента.
		/// </summary>
		public int StartPosition { get; }

		/// <summary>
		/// Получает количество байт элемента.
		/// </summary>
		public int Length { get; }

		/// <summary>
		/// Получает признак валидности элемента.
		/// </summary>
		public bool IsValid => (this.ElementType != _invalid.ElementType) || (this.StartPosition != _invalid.StartPosition) || (this.Length != _invalid.Length);
	}
}
