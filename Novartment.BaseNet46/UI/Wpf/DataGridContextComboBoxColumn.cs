using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Дублирует System.Windows.Controls.DataGridComboBoxColumn,
	/// плюс содержит свойство DataGridDataContext,
	/// возвращающее контекст данных родительского DataGrid.
	/// </summary>
	public class DataGridContextComboBoxColumn : DataGridComboBoxColumn,
		INotifyPropertyChanged
	{
		/// <summary>Происходит после изменения свойства.</summary>
		public event PropertyChangedEventHandler PropertyChanged;

		// Свойство DataGridOwner.DataContext устанавливается не сразу,
		// поэтому наше свойство DataGridDataContext будет равно null при раннем использовании.
		// Для решения проблемы уведомляем об его изменении при изменении DisplayIndex,
		// что грубо соответствует моменту,
		// когда уже можно пользоваться свойством DataGridOwner.DataContext.

		/// <summary>
		/// Получает контекст данных родительского DataGrid.
		/// </summary>
		public object DataGridDataContext => this.DataGridOwner?.DataContext;

		/// <summary>
		/// Инициирует событие DependencyPropertyChanged с указанными аргументами.
		/// </summary>
		/// <param name="e">Аргументы события DependencyPropertyChanged.</param>
		protected override void OnPropertyChanged (DependencyPropertyChangedEventArgs e)
		{
			if (e.Property == DisplayIndexProperty)
			{
				OnPropertyChanged ("DataGridDataContext");
			}

			base.OnPropertyChanged (e);
		}

		/// <summary>
		/// Инициирует событие PropertyChanged с указанным именем свойства.
		/// </summary>
		/// <param name="name">Имя свойства.</param>
		protected virtual void OnPropertyChanged (string name)
		{
			this.PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (name));
		}
	}
}
