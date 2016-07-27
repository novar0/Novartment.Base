namespace Novartment.Base
{
	/// <summary>
	/// Поддержка категоризации для группировки объектов.
	/// </summary>
	public interface ICategory
	{
		/// <summary>
		/// Получает категорию объекта.
		/// </summary>
		int Category { get; }
	}
}
