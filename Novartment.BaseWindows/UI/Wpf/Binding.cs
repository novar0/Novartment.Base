using System.Globalization;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Дублирует System.Windows.Data.Binding,
	/// но устанавливает текущую культуру для конвертеров.
	/// </summary>
	public sealed class Binding : System.Windows.Data.Binding
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
}
