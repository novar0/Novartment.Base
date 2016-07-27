using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Дублирует System.Windows.Controls.DataGridCheckBoxColumn,
	/// плюс содержит свойство DataGridDataContext,
	/// возвращающее контекст данных родительского DataGrid.
	/// </summary>
	public class DataGridContextCheckBoxColumn : DataGridCheckBoxColumn,
		INotifyPropertyChanged
	{
		// Свойство DataGridOwner.DataContext устанавливается не сразу,
		// поэтому наше свойство DataGridDataContext будет равно null при раннем использовании.
		// Для решения проблемы уведомляем об его изменении при изменении DisplayIndex,
		// что грубо соответствует моменту,
		// когда уже можно пользоваться свойством DataGridOwner.DataContext.

		/// <summary>
		/// Получает контекст данных родительского DataGrid.
		/// </summary>
		public object DataGridDataContext
		{
			get
			{
				return (this.DataGridOwner != null) ? this.DataGridOwner.DataContext : null;
			}
		}

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

		/// <summary>Происходит после изменения свойства.</summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Инициирует событие PropertyChanged с указанным именем свойства.
		/// </summary>
		/// <param name="name">Имя свойства.</param>
		protected virtual void OnPropertyChanged (string name)
		{
			this.PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (name));
		}
	}

	/// <summary>
	/// Дублирует System.Windows.Controls.DataGridHyperlinkColumn,
	/// плюс содержит свойство DataGridDataContext,
	/// возвращающее контекст данных родительского DataGrid.
	/// </summary>
	public class DataGridContextHyperlinkColumn : DataGridHyperlinkColumn,
		INotifyPropertyChanged
	{
		// Свойство DataGridOwner.DataContext устанавливается не сразу,
		// поэтому наше свойство DataGridDataContext будет равно null при раннем использовании.
		// Для решения проблемы уведомляем об его изменении при изменении DisplayIndex,
		// что грубо соответствует моменту,
		// когда уже можно пользоваться свойством DataGridOwner.DataContext.

		/// <summary>
		/// Получает контекст данных родительского DataGrid.
		/// </summary>
		public object DataGridDataContext
		{
			get
			{
				return this.DataGridOwner?.DataContext;
			}
		}

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

		/// <summary>Происходит после изменения свойства.</summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Инициирует событие PropertyChanged с указанным именем свойства.
		/// </summary>
		/// <param name="name">Имя свойства.</param>
		protected virtual void OnPropertyChanged (string name)
		{
			this.PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (name));
		}
	}

	/// <summary>
	/// Дублирует System.Windows.Controls.DataGridTextColumn,
	/// плюс содержит свойство DataGridDataContext,
	/// возвращающее контекст данных родительского DataGrid.
	/// </summary>
	public class DataGridContextTextColumn : DataGridTextColumn,
		INotifyPropertyChanged
	{
		// Свойство DataGridOwner.DataContext устанавливается не сразу,
		// поэтому наше свойство DataGridDataContext будет равно null при раннем использовании.
		// Для решения проблемы уведомляем об его изменении при изменении DisplayIndex,
		// что грубо соответствует моменту,
		// когда уже можно пользоваться свойством DataGridOwner.DataContext.

		/// <summary>
		/// Получает контекст данных родительского DataGrid.
		/// </summary>
		public object DataGridDataContext
		{
			get
			{
				return this.DataGridOwner?.DataContext;
			}
		}

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

		/// <summary>Происходит после изменения свойства.</summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Инициирует событие PropertyChanged с указанным именем свойства.
		/// </summary>
		/// <param name="name">Имя свойства.</param>
		protected virtual void OnPropertyChanged (string name)
		{
			this.PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (name));
		}
	}

	/// <summary>
	/// Дублирует System.Windows.Controls.DataGridComboBoxColumn,
	/// плюс содержит свойство DataGridDataContext,
	/// возвращающее контекст данных родительского DataGrid.
	/// </summary>
	public class DataGridContextComboBoxColumn : DataGridComboBoxColumn,
		INotifyPropertyChanged
	{
		// Свойство DataGridOwner.DataContext устанавливается не сразу,
		// поэтому наше свойство DataGridDataContext будет равно null при раннем использовании.
		// Для решения проблемы уведомляем об его изменении при изменении DisplayIndex,
		// что грубо соответствует моменту,
		// когда уже можно пользоваться свойством DataGridOwner.DataContext.

		/// <summary>
		/// Получает контекст данных родительского DataGrid.
		/// </summary>
		public object DataGridDataContext
		{
			get
			{
				return this.DataGridOwner?.DataContext;
			}
		}

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

		/// <summary>Происходит после изменения свойства.</summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Инициирует событие PropertyChanged с указанным именем свойства.
		/// </summary>
		/// <param name="name">Имя свойства.</param>
		protected virtual void OnPropertyChanged (string name)
		{
			this.PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (name));
		}
	}

	/// <summary>
	/// Дублирует System.Windows.Controls.DataGridTemplateColumn,
	/// плюс содержит свойство DataGridDataContext,
	/// возвращающее контекст данных родительского DataGrid.
	/// </summary>
	public class DataGridContextTemplateColumn : DataGridTemplateColumn,
		INotifyPropertyChanged
	{
		// Свойство DataGridOwner.DataContext устанавливается не сразу,
		// поэтому наше свойство DataGridDataContext будет равно null при раннем использовании.
		// Для решения проблемы уведомляем об его изменении при изменении DisplayIndex,
		// что грубо соответствует моменту,
		// когда уже можно пользоваться свойством DataGridOwner.DataContext.

		/// <summary>
		/// Получает контекст данных родительского DataGrid.
		/// </summary>
		public object DataGridDataContext
		{
			get
			{
				return this.DataGridOwner?.DataContext;
			}
		}

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

		/// <summary>Происходит после изменения свойства.</summary>
		public event PropertyChangedEventHandler PropertyChanged;

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
