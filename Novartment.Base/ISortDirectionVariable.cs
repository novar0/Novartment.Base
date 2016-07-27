using System.ComponentModel;

namespace Novartment.Base
{
	/// <summary>
	/// Поддержка изменения направления сортировки.
	/// </summary>
	public interface ISortDirectionVariable
	{
		/// <summary>
		/// Получает или устанавливает признак обратного направления сортировки.
		/// </summary>
		bool DescendingOrder { get; set; }
	}
}
