using System;
using System.Windows;
using System.Windows.Input;
using BclDragDropAction = Novartment.Base.UI.DragDropAction;
using BclDragDropEffect = Novartment.Base.UI.DragDropEffects;
using BclDragDropKeyStates = Novartment.Base.UI.DragDropKeyStates;
using WindowsDragAction = System.Windows.DragAction;
using WindowsDragDropEffects = System.Windows.DragDropEffects;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Методы расширения для UIElement.
	/// </summary>
	public static class UIElementExtensions
	{
		/// <summary>
		/// Регистрирует указанный элемент интерфейса пользователя как источник перетаскивания и
		/// устанавливает для него указанный обработчик событий.
		/// </summary>
		/// <param name="element">Элемент интерфейса пользователя, перетаскивание с которого будет обслуживать обработчик источника перетаскивания.</param>
		/// <param name="handler">Обработчик источника перетаскивания, который будет обрабатывать перетаскивание с элемента интерфейса пользователя.</param>
		/// <returns>Объект, освобождение которого будет отменять регистрацию источника перетаскивания.</returns>
		/// <remarks>
		/// Используются встроенные в WPF функции источника перетаскивания: функция DragDrop.DoDragDrop и события QueryContinueDrag и GiveFeedback.
		/// которые вызываются на основе анализа WPF событий PreviewMouseLeftButtonDown и PreviewMouseMove.
		/// Данные перетаскивания представляются в виде WpfDataContainer.
		/// </remarks>
		public static IDisposable RegisterAsDragDropSource (this UIElement element, IDragDropSource handler)
		{
			if (element == null)
			{
				throw new ArgumentNullException (nameof (element));
			}

			if (handler == null)
			{
				throw new ArgumentNullException (nameof (handler));
			}

			return new DragDropSourceUIElementHandler (element, handler);
		}

		/// <summary>
		/// Регистрирует указанный элемент интерфейса пользователя как цель перетаскивания и
		/// устанавливает для него указанный обработчик событий.
		/// </summary>
		/// <param name="element">Элемент интерфейса пользователя, перетаскивание на который будет обслуживать обработчик цели перетаскивания.</param>
		/// <param name="handler">Обработчик цели перетаскивания, который будет обрабатывать перетаскивание на элемент интерфейса пользователя.</param>
		/// <returns>Объект, освобождение которого будет отменять регистрацию цели перетаскивания.</returns>
		/// <remarks>
		/// Используются встроенные WPF функции цели перетаскивания: свойство AllowDrop и события DragEnter, DragLeave, DragOver и Drop.
		/// Данные перетаскивания представляются в виде WpfDataContainer.
		/// </remarks>
		public static IDisposable RegisterAsDragDropTarget (this UIElement element, IDragDropTarget handler)
		{
			if (element == null)
			{
				throw new ArgumentNullException (nameof (element));
			}

			if (handler == null)
			{
				throw new ArgumentNullException (nameof (handler));
			}

			return new DragDropTargetUIElementHandler (element, handler);
		}

		internal sealed class DragDropTargetUIElementHandler :
			IDisposable
		{
			private readonly UIElement _element;
			private IDragDropTarget _handler;

			internal DragDropTargetUIElementHandler (UIElement element, IDragDropTarget handler)
			{
				_element = element;
				_handler = handler;
				element.DragLeave += this.DragLeave;
				element.DragEnter += this.DragEnter;
				element.DragOver += this.DragOver;
				element.Drop += this.Drop;
				element.AllowDrop = true;
			}

			/// <summary>
			/// Освобождает занятые объектом ресурсы.
			/// </summary>
			public void Dispose ()
			{
				_element.AllowDrop = false;
				_element.Drop -= this.Drop;
				_element.DragOver -= this.DragOver;
				_element.DragEnter -= this.DragEnter;
				_element.DragLeave -= this.DragLeave;
				_handler = null;
			}

			private void DragEnter (object sender, DragEventArgs e)
			{
				var pos = e.GetPosition (_element);
				e.Effects = (WindowsDragDropEffects)_handler?.DragEnter (
					new WpfDataContainer ((DataObject)e.Data),
					(BclDragDropKeyStates)e.KeyStates,
					pos.X,
					pos.Y,
					(BclDragDropEffect)e.AllowedEffects);
			}

			private void DragOver (object sender, DragEventArgs e)
			{
				var pos = e.GetPosition (_element);
				e.Effects = (WindowsDragDropEffects)_handler?.DragOver (
					(BclDragDropKeyStates)e.KeyStates,
					pos.X,
					pos.Y,
					(BclDragDropEffect)e.AllowedEffects);
			}

			private void DragLeave (object sender, DragEventArgs e)
			{
				_handler?.DragLeave ();
			}

			private void Drop (object sender, DragEventArgs e)
			{
				var pos = e.GetPosition (_element);
				e.Effects = (WindowsDragDropEffects)_handler?.Drop (
					new WpfDataContainer ((DataObject)e.Data),
					(BclDragDropKeyStates)e.KeyStates,
					pos.X,
					pos.Y,
					(BclDragDropEffect)e.AllowedEffects);
			}
		}

		internal sealed class DragDropSourceUIElementHandler :
			IDisposable
		{
			private readonly UIElement _element;
			private MouseButton _startButton = MouseButton.XButton2; // неиспользуемое значение
			private Point _startPoint;
			private IDragDropSource _handler;

			public DragDropSourceUIElementHandler (UIElement element, IDragDropSource handler)
			{
				_element = element;
				_handler = handler;
				element.PreviewMouseDown += this.OnPreviewMouseButtonDown;
				element.PreviewMouseMove += this.OnPreviewMouseMove;
				element.QueryContinueDrag += this.OnQueryContinueDrag;
				element.GiveFeedback += this.OnGiveFeedback;
			}

			/// <summary>
			/// Освобождает занятые объектом ресурсы.
			/// </summary>
			public void Dispose ()
			{
				_element.GiveFeedback -= this.OnGiveFeedback;
				_element.QueryContinueDrag -= this.OnQueryContinueDrag;
				_element.PreviewMouseDown -= this.OnPreviewMouseButtonDown;
				_element.PreviewMouseMove -= this.OnPreviewMouseMove;
				_handler = null;
			}

			private void OnPreviewMouseButtonDown (object sender, MouseButtonEventArgs e)
			{
				_startButton = e.ChangedButton;
				_startPoint = e.GetPosition (_element);
			}

			private void OnPreviewMouseMove (object sender, MouseEventArgs e)
			{
				var currentPoint = e.GetPosition (_element);
				var diff = _startPoint - currentPoint;

				bool buttonStillPressed = false;
				var button = DragControl.None;
				switch (_startButton)
				{
					case MouseButton.Left:
						buttonStillPressed = e.LeftButton == MouseButtonState.Pressed;
						button = DragControl.MouseButtonLeft;
						break;
					case MouseButton.Middle:
						buttonStillPressed = e.MiddleButton == MouseButtonState.Pressed;
						button = DragControl.MouseButtonMiddle;
						break;
					case MouseButton.Right:
						buttonStillPressed = e.RightButton == MouseButtonState.Pressed;
						button = DragControl.MouseButtonRight;
						break;
				}

				if (buttonStillPressed &&
					(Math.Abs (diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
					Math.Abs (diff.Y) > SystemParameters.MinimumVerticalDragDistance))
				{
					var handler = _handler;
					if (handler != null)
					{
						var dragData = handler.DragStart (currentPoint.X, currentPoint.Y, button);
						if (dragData.AllowedEffects != BclDragDropEffect.None)
						{
							var dataContainerWpf = dragData.DragObject as WpfDataContainer;
							var dataObject = dataContainerWpf?.InternalDataObject ?? new DataObject (dragData.DragObject);
							var result = DragDrop.DoDragDrop (
								_element,
								dataObject,
								(WindowsDragDropEffects)dragData.AllowedEffects);
							handler.DragEnd ((BclDragDropEffect)result);
						}
					}

					_startButton = MouseButton.XButton2; // неиспользуемое значение
				}
			}

			private void OnGiveFeedback (object sender, GiveFeedbackEventArgs e)
			{
				var handler = _handler;
				e.UseDefaultCursors = (handler == null) || handler.GiveFeedback ((BclDragDropEffect)e.Effects);
			}

			private void OnQueryContinueDrag (object sender, QueryContinueDragEventArgs e)
			{
				var handler = _handler;
				var action = (handler != null) ?
					handler.QueryContinueDrag (e.EscapePressed, (BclDragDropKeyStates)e.KeyStates) :
					BclDragDropAction.Cancel;
				e.Action = action switch
				{
					BclDragDropAction.Cancel => WindowsDragAction.Cancel,
					BclDragDropAction.Drop => WindowsDragAction.Drop,
					_ => WindowsDragAction.Continue,
				};
			}
		}
	}
}
