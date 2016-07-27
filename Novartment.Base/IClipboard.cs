using System.Diagnostics.CodeAnalysis;

namespace Novartment.Base
{
	/// <summary>
	/// Буфер обмена данными между приложениями.
	/// </summary>
	public interface IClipboard
	{
		/// <summary>
		/// Закрепляет данные в буфере обмена так, чтобы они оставались доступными даже после закрытия приложения.
		/// </summary>
		void Flush ();

		/// <summary>
		/// Сравнивает указанные данные с содержимым буфера обмена.
		/// </summary>
		/// <param name="data">Объект данных для проверки на совпадение с содержимым буфера обмена.</param>
		/// <returns>True если указанный объект данных совпадает с тем, который находится в буфере обмена, иначе False.</returns>
		bool IsCurrent (IDataContainer data);

		/// <summary>
		/// Получает объект с данными, представляющий содержимое буфера обмена.
		/// </summary>
		/// <returns>
		/// Объект дающий доступ к содержимому буфера обмена,
		/// или null если в буфере обмена нет данных.
		/// </returns>
		[SuppressMessage (
			"Microsoft.Design",
			"CA1024:UsePropertiesWhereAppropriate",
			Justification = "Too expensive to be property.")]
		IDataContainer GetData ();

		/// <summary>
		/// Помещает указанные данные в буфер обмена.
		/// </summary>
		/// <param name="dataObject">Объект с данными для помещения в буфер обмена.</param>
		void SetData (IDataContainer dataObject);
	}
}
