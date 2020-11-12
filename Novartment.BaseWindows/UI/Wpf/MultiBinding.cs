using System.Globalization;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Дублирует System.Windows.Data.MultiBinding,
	/// но устанавливает текущую культуру для конвертеров.
	/// </summary>
	public sealed class MultiBinding : System.Windows.Data.MultiBinding
	{
		/// <summary>
		/// <summary>Инициализирует новый экземпляр класса MultiBinding.</summary>
		/// </summary>
		public MultiBinding ()
			: base ()
		{
			this.ConverterCulture = CultureInfo.CurrentCulture;
		}
	}
}
