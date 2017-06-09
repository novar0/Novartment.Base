namespace Novartment.Base.UI
{
	/// <summary>
	/// Модель для представлений в виде диалога.
	/// </summary>
	/// <typeparam name="TResult">Тип объекта, являющегося результатом диалога.</typeparam>
	public interface IDialogViewModel<out TResult>
	{
		/// <summary>
		/// Результат диалога.
		/// </summary>
		TResult Result { get; }
	}
}
