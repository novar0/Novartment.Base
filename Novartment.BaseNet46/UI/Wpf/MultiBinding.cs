using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Дублирует System.Windows.Data.MultiBinding,
	/// но устанавливает текущую культуру для конвертеров.
	/// </summary>
	[SuppressMessage (
		"Microsoft.Naming",
		"CA1704:IdentifiersShouldBeSpelledCorrectly",
		MessageId = "Multi",
		Justification = "Name inherited from library class.")]
	public class MultiBinding : System.Windows.Data.MultiBinding
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
