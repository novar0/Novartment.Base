namespace Novartment.Base
{
	/// <summary>
	/// Объект, который может быть помечен.
	/// </summary>
	public interface IMarkHolder
	{
		/// <summary>
		/// Получает наличие отметки объекта.
		/// </summary>
		bool IsMarked { get; set; }
	}
}
