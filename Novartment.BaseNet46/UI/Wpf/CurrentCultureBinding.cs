using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Дублирует System.Windows.Data.Binding,
	/// но устанавливает текущую культуру для конвертеров.
	/// </summary>
	public class Binding : System.Windows.Data.Binding
	{
		/// <summary>
		/// <summary>Инициализирует новый экземпляр класса Binding.</summary>
		/// </summary>
		public Binding ()
			: base ()
		{
			this.ConverterCulture = CultureInfo.CurrentCulture;
		}

		/// <summary>Инициализация нового экземпляра  класса Binding с начальным путём.</summary>
		/// <param name="path">Начальный путь для привязки.</param>
		public Binding (string path)
			: base (path)
		{
			this.ConverterCulture = CultureInfo.CurrentCulture;
		}
	}

	/// <summary>
	/// Дублирует System.Windows.Data.MultiBinding,
	/// но устанавливает текущую культуру для конвертеров.
	/// </summary>
	[SuppressMessage ("Microsoft.Naming",
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
