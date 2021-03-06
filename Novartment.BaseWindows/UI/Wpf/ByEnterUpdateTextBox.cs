using System.Windows.Controls;
using System.Windows.Input;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>Класс, добавляющий к System.Windows.Controls.TextBox обновление свойства Text по нажатию клавиши Enter.</summary>
	public sealed class ByEnterUpdateTextBox : TextBox
	{
		/// <summary>Вызывается, когда происходит событие <see cref="System.Windows.UIElement.KeyDown" />.</summary>
		/// <param name="e">Данные события.</param>
		protected override void OnPreviewKeyDown (KeyEventArgs e)
		{
			if (e?.Key == Key.Enter)
			{
				GetBindingExpression (TextProperty)?.UpdateSource ();
			}

			base.OnPreviewKeyDown (e);
		}
	}
}
