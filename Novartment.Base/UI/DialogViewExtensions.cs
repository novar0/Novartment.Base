using System;
using System.Threading;

namespace Novartment.Base.UI
{
	/// <summary>
	/// Методы расширения для IDialogView.
	/// </summary>
	public static class DialogViewExtensions
	{
		/// <summary>
		/// Активирует представление-диалог с возможностью принудительной отмены с помощью указанного CancellationToken.
		/// </summary>
		/// <typeparam name="TResult">Тип объекта, являющегося результатом диалога.</typeparam>
		/// <param name="view">Представление-диалог которое надо активировать.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>True если пользователь подтвердил данные, False если отменил, null если результат неизвестен.</returns>
		public static bool? ShowDialog<TResult> (this IDialogView<TResult> view, CancellationToken cancellationToken = default)
		{
			if (view == null)
			{
				throw new ArgumentNullException (nameof (view));
			}

			if (cancellationToken.IsCancellationRequested)
			{
				return false;
			}

			bool? dialogResult;
			if (!cancellationToken.CanBeCanceled)
			{
				dialogResult = view.ShowDialog ();
			}
			else
			{
				using (cancellationToken.Register (obj => { ((IDialogView<TResult>)obj).DialogResult = false; }, view, true))
				{
					dialogResult = view.ShowDialog ();
				}
			}

			return dialogResult;
		}

		/// <summary>
		/// Активирует представление-диалог, вызывая указанное действие в случае подтверждения диалога пользователем,
		/// с возможностью принудительной отмены с помощью указанного CancellationToken.
		/// </summary>
		/// <typeparam name="TResult">Тип объекта, являющегося результатом диалога.</typeparam>
		/// <param name="view">Представление-диалог которое надо активировать.</param>
		/// <param name="successAction">Действие, которое будет выполнено в случае успешного подтверждения диалога пользователем.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>True если пользователь подтвердил данные, False если отменил, null если результат неизвестен.</returns>
		public static bool? ShowDialog<TResult> (this IDialogView<TResult> view, Action<TResult> successAction, CancellationToken cancellationToken = default)
		{
			if (view == null)
			{
				throw new ArgumentNullException (nameof (view));
			}

			if (successAction == null)
			{
				throw new ArgumentNullException (nameof (successAction));
			}

			var dialogResult = ShowDialog (view, cancellationToken);
			if (dialogResult == true)
			{
				successAction.Invoke (view.ViewModel.Result);
			}

			return dialogResult;
		}
	}
}
