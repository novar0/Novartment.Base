﻿using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Implements a "split button" for WPF.
	/// </summary>
	[TemplatePart (Name = SplitElementName, Type = typeof (UIElement))]
	public class SplitButton : Button
	{
		private const string SplitElementName = "SplitElement";
		private UIElement _splitElement;
		private ContextMenu _contextMenu;
		private Point _contextMenuInitialOffset;
		private readonly ObservableCollection<object> _buttonMenuItemsSource = new ObservableCollection<object> ();

		/// <summary>
		/// Gets the collection of items for the split button's menu.
		/// </summary>
		public Collection<object> ButtonMenuItemsSource => _buttonMenuItemsSource;

		/// <summary>
		/// Gets or sets a value indicating whetherthe mouse is over the split element.
		/// </summary>
		protected bool IsMouseOverSplitElement { get; private set; }

		/// <summary>
		/// Initializes a new instance of the SplitButton class.
		/// </summary>
		public SplitButton ()
		{
			DefaultStyleKey = typeof (SplitButton);
		}

		/// <summary>
		/// Called when the template is changed.
		/// </summary>
		public override void OnApplyTemplate ()
		{
			// Unhook existing handlers
			if (null != _splitElement)
			{
				_splitElement.MouseEnter -= SplitElement_MouseEnter;
				_splitElement.MouseLeave -= SplitElement_MouseLeave;
				_splitElement = null;
			}
			if (null != _contextMenu)
			{
				_contextMenu.Opened -= ContextMenu_Opened;
				_contextMenu.Closed -= ContextMenu_Closed;
				_contextMenu = null;
			}

			// Apply new template
			base.OnApplyTemplate ();

			// Hook new event handlers
			_splitElement = GetTemplateChild (SplitElementName) as UIElement;
			if (null != _splitElement)
			{
				_splitElement.MouseEnter += SplitElement_MouseEnter;
				_splitElement.MouseLeave += SplitElement_MouseLeave;

				_contextMenu = ContextMenuService.GetContextMenu (_splitElement);
				if (null != _contextMenu)
				{

					_contextMenu.Opened += ContextMenu_Opened;
					_contextMenu.Closed += ContextMenu_Closed;
				}
			}
		}

		/// <summary>
		/// Called when the Button is clicked.
		/// </summary>
		protected override void OnClick ()
		{
			if (IsMouseOverSplitElement)
			{
				OpenButtonMenu ();
			}
			else
			{
				base.OnClick ();
			}
		}

		/// <summary>
		/// Called when a key is pressed.
		/// </summary>
		protected override void OnKeyDown (KeyEventArgs e)
		{
			if (null == e)
			{
				throw new ArgumentNullException (nameof (e));
			}

			if ((Key.Down == e.Key) || (Key.Up == e.Key))
			{
				// WPF requires this to happen via BeginInvoke
				Dispatcher.BeginInvoke ((Action)OpenButtonMenu);
			}
			else
			{
				base.OnKeyDown (e);
			}
		}

		/// <summary>
		/// Opens the button menu.
		/// </summary>
		protected void OpenButtonMenu ()
		{
			if ((0 < _buttonMenuItemsSource.Count) && (null != _contextMenu))
			{
				_contextMenu.HorizontalOffset = 0;
				_contextMenu.VerticalOffset = 0;
				_contextMenu.IsOpen = true;
			}
		}

		/// <summary>
		/// Called when the mouse goes over the split element.
		/// </summary>
		/// <param name="sender">Event source.</param>
		/// <param name="e">Event arguments.</param>
		private void SplitElement_MouseEnter (object sender, MouseEventArgs e)
		{
			IsMouseOverSplitElement = true;
		}

		/// <summary>
		/// Called when the mouse goes off the split element.
		/// </summary>
		/// <param name="sender">Event source.</param>
		/// <param name="e">Event arguments.</param>
		private void SplitElement_MouseLeave (object sender, MouseEventArgs e)
		{
			IsMouseOverSplitElement = false;
		}

		/// <summary>
		/// Called when the ContextMenu is opened.
		/// </summary>
		/// <param name="sender">Event source.</param>
		/// <param name="e">Event arguments.</param>
		private void ContextMenu_Opened (object sender, RoutedEventArgs e)
		{
			// Offset the ContextMenu correctly
			_contextMenuInitialOffset = TranslatePoint (new Point (0, ActualHeight), _contextMenu);
			UpdateContextMenuOffsets ();

			// Hook LayoutUpdated to handle application resize and zoom changes
			LayoutUpdated += SplitButton_LayoutUpdated;
		}

		/// <summary>
		/// Called when the ContextMenu is closed.
		/// </summary>
		/// <param name="sender">Event source.</param>
		/// <param name="e">Event arguments.</param>
		private void ContextMenu_Closed (object sender, RoutedEventArgs e)
		{
			// No longer need to handle LayoutUpdated
			LayoutUpdated -= SplitButton_LayoutUpdated;

			// Restore focus to the Button
			Focus ();
		}

		/// <summary>
		/// Called when the ContextMenu is open and layout is updated.
		/// </summary>
		/// <param name="sender">Event source.</param>
		/// <param name="e">Event arguments.</param>
		private void SplitButton_LayoutUpdated (object sender, EventArgs e)
		{
			UpdateContextMenuOffsets ();
		}

		/// <summary>
		/// Updates the ContextMenu's Horizontal/VerticalOffset properties to keep it under the SplitButton.
		/// </summary>
		private void UpdateContextMenuOffsets ()
		{
			// Calculate desired offset to put the ContextMenu below and left-aligned to the Button
			var currentOffset = new Point ();
			var desiredOffset = _contextMenuInitialOffset;
			_contextMenu.HorizontalOffset = desiredOffset.X - currentOffset.X;
			_contextMenu.VerticalOffset = desiredOffset.Y - currentOffset.Y;
			// Adjust for RTL
			if (FlowDirection.RightToLeft == FlowDirection)
			{
				_contextMenu.HorizontalOffset *= -1;
			}
		}

	}
}
