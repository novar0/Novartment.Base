namespace Novartment.Base.UI
{
	/// <summary>
	/// Представление в виде диалога с результатом 'ОК' либо 'Отмена'.
	/// </summary>
	/// <typeparam name="TResult">Тип объекта, являющегося результатом диалога.</typeparam>
	public interface IDialogView<out TResult>
	{
		/// <summary>
		/// Получает модель, на основе которой работает представление.
		/// </summary>
		IDialogViewModel<TResult> ViewModel { get; }

		/// <summary>
		/// Получает или устанавливает результат диалога.
		/// True если пользователь подтвердил данные, False если отменил, null пока результат ещё неизвестен.
		/// </summary>
		bool? DialogResult { get; set; }

		/// <summary>
		/// Закрывает представление.
		/// </summary>
		void Close ();

		/// <summary>
		/// Активирует представление.
		/// </summary>
		/// <returns>True если пользователь подтвердил данные, False если отменил, null если результат неизвестен.</returns>
		bool? ShowDialog ();
	}
}
