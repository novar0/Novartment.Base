using System.Windows.Input;
using System.Windows.Controls;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>Класс, добавляющий к System.Windows.Controls.TextBox обновление свойства Text по нажатию клавиши Enter.</summary>
	public class ByEnterUpdateTextBox : TextBox
	{
		/// <summary>Вызывается, когда происходит событие <see cref="E:System.Windows.UIElement.KeyDown" />.</summary>
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
