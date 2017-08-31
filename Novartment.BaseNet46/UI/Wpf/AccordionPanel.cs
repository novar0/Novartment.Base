using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Windows;
using System.Windows.Controls;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Панель, состоящая из экспандеров, один из которых всегда развёрнут.
	/// </summary>
	public class AccordionPanel : Panel
	{
		/// <summary>
		/// Получает или устанавливает открытый экспандер.
		/// </summary>
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Visible)]
		[Browsable (true)]
		[Description ("Specifies expanded item.")]
		[Category ("Layout")]
		public static readonly DependencyProperty ExpandedItemProperty = DependencyProperty.Register (
			"ExpandedItem",
			typeof (UIElement),
			typeof (AccordionPanel),
			new FrameworkPropertyMetadata (null, FrameworkPropertyMetadataOptions.AffectsArrange, ExpandedItemPropertyChangedCallback, CoerceExpandedItem));

		/// <summary>
		/// Получает или устанавливает номер открытого экспандера.
		/// </summary>
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Visible)]
		[Browsable (true)]
		[Description ("Specifies index of expanded item.")]
		[Category ("Layout")]
		public static readonly DependencyProperty ExpandedIndexProperty = DependencyProperty.Register (
			"ExpandedIndex",
			typeof (int),
			typeof (AccordionPanel),
			new FrameworkPropertyMetadata (0, FrameworkPropertyMetadataOptions.AffectsArrange, ExpandedIndexPropertyChangedCallback, CoerceExpandedIndex));

		/// <summary>Получает или устанавливает значение, представляющее ориентацию элемента.</summary>
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Visible)]
		[Browsable (true)]
		[Description ("Specifies index of expanded item.")]
		[Category ("Layout")]
		public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register (
			"Orientation",
			typeof (Orientation),
			typeof (AccordionPanel),
			new FrameworkPropertyMetadata (Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsParentMeasure));

		/// <summary>
		/// Получает признак того, что экспандер открыт.
		/// </summary>
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Visible)]
		[Browsable (true)]
		[Description ("Specifies expanded state of item.")]
		[Category ("Layout")]
		public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.RegisterAttached (
			"IsExpanded",
			typeof (bool),
			typeof (AccordionPanel),
			new FrameworkPropertyMetadata (false, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure, IsExpandedPropertyChangedCallback, CoerceIsExpanded));

		private int _lastWantedIndex = -1;

		private UIElement _lastWantedItem = null;

		/// <summary>
		/// Инициализирует новый экземпляр AccordionPanel.
		/// </summary>
		public AccordionPanel ()
			: base ()
		{
			this.Loaded += EnsureSomethingSelected;
			this.Unloaded -= EnsureSomethingSelected;
		}

		/// <summary>
		/// Получает или устанавливает открытый экспандер.
		/// </summary>
		public UIElement ExpandedItem
		{
			get => (UIElement)GetValue (ExpandedItemProperty);

			set
			{
				SetValue (ExpandedItemProperty, value);
			}
		}

		/// <summary>
		/// Получает или устанавливает номер открытого экспандера.
		/// </summary>
		public int ExpandedIndex
		{
			get => (int)GetValue (ExpandedIndexProperty);
			set { SetValue (ExpandedIndexProperty, value); }
		}

		/// <summary>Получает или устанавливает значение, представляющее ориентацию элемента.</summary>
		public Orientation Orientation
		{
			get => (Orientation)GetValue (OrientationProperty);

			set
			{
				SetValue (OrientationProperty, value);
			}
		}

		/// <summary>Получает значение, которое указывает, организовывает ли этот объект своих потомков в единой размерности.</summary>
		protected override bool HasLogicalOrientation => true;

		/// <summary>Получает значение, представляющее ориентацию элемента.</summary>
		protected override Orientation LogicalOrientation => this.Orientation;

		/// <summary>
		/// Получает признак того, что экспандер открыт.
		/// </summary>
		/// <param name="expander">Экспандер.</param>
		/// <returns>True если экспандер открыт, иначе False.</returns>
		public static bool GetIsExpanded (DependencyObject expander)
		{
			if (expander == null)
			{
				throw new ArgumentNullException (nameof (expander));
			}

			Contract.EndContractBlock ();

			return (bool)expander.GetValue (IsExpandedProperty);
		}

		/// <summary>
		/// Устанавливает признак того, что экспандер открыт.
		/// </summary>
		/// <param name="expander">Экспандер.</param>
		/// <param name="value">Признак того, что экспандер открыт.</param>
		public static void SetIsExpanded (DependencyObject expander, bool value)
		{
			if (expander == null)
			{
				throw new ArgumentNullException (nameof (expander));
			}

			Contract.EndContractBlock ();

			expander.SetValue (IsExpandedProperty, value);
		}

		/// <summary>Создает новый объект <see cref="UIElementCollection" />.</summary>
		/// <param name="logicalParent">Логический родительский элемент создаваемой коллекции.</param>
		/// <returns>Упорядоченная коллекция элементов, которые имеют указанный логический родительский элемент.</returns>
		protected override UIElementCollection CreateUIElementCollection (FrameworkElement logicalParent)
		{
			var collection = new ObservableUIElementCollection (this, logicalParent);
			collection.CollectionChanged += UIElementCollectionChanged;
			return collection;
		}

		/// <summary>
		/// Проверяет чтобы был выбран хотя бы один из содержащихся экспандеров.
		/// </summary>
		protected virtual void EnsureSomethingSelected ()
		{
			// если распахнутый элемент не содерижтся в коллекции то его надо переназначить
			var needSetNewExpandedItem = !this.InternalChildren.Contains (this.ExpandedItem);
			if (needSetNewExpandedItem)
			{
				if ((_lastWantedItem == null) && (_lastWantedIndex >= 0) && (_lastWantedIndex < this.InternalChildren.Count))
				{
					_lastWantedItem = this.InternalChildren[_lastWantedIndex];
				}

				if (_lastWantedItem != null)
				{
					this.ExpandedItem = _lastWantedItem;
				}
				else
				{
					if (InternalChildren.Count > 0)
					{
						this.ExpandedItem = InternalChildren[0];
					}
				}
			}
		}

		/// <summary>Измеряет дочерние элементы объекта, ожидая их упорядочивания во время передачи метода ArrangeOverride().</summary>
		/// <param name="availableSize">Верхний предел, который не должен быть превышен.</param>
		/// <returns>Объект, который представляет желаемый размер элемента.</returns>
		protected override Size MeasureOverride (Size availableSize)
		{
			if (this.InternalChildren.Count < 1)
			{
				return availableSize;
			}

			var constraint = availableSize;
			var panelSize = default (Size);
			var isVertical = this.Orientation == Orientation.Vertical;

			// даём пределы и меряем все элементы кроме распахнутого в надежде что они не попросят слишком много
			for (var i = 0; i < InternalChildren.Count; i++)
			{
				if (i == this.ExpandedIndex)
				{
					continue;
				}

				var element = this.InternalChildren[i];

				// Size testingSize = new Size (double.PositiveInfinity, double.PositiveInfinity); element.Measure (testingSize);
				element.Measure (constraint);
				if (isVertical)
				{
					panelSize.Height += element.DesiredSize.Height;
					var isPositiveInfinity = double.IsPositiveInfinity (constraint.Height);
					if (!isPositiveInfinity)
					{
						constraint.Height = Math.Max (0, constraint.Height - element.DesiredSize.Height);
					}

					panelSize.Width = Math.Max (element.DesiredSize.Width, panelSize.Width);
				}
				else
				{
					panelSize.Width += element.DesiredSize.Width;
					var isPositiveInfinity = double.IsPositiveInfinity (constraint.Width);
					if (!isPositiveInfinity)
					{
						constraint.Width = Math.Max (0, constraint.Width - element.DesiredSize.Width);
					}

					panelSize.Height = Math.Max (element.DesiredSize.Height, panelSize.Height);
				}
			}

			// распахнутый элемент меряем в последнюю очередь (на случай если он захочет всё доступное место)
			var selectedElement = this.InternalChildren[this.ExpandedIndex];
			selectedElement.Measure (constraint);
			return panelSize;
		}

		/// <summary>Размещает дочерние элементы и определяет размер.</summary>
		/// <param name="finalSize">Итоговая область в родительском элементе, которую этот элемент должен использовать для собственного размещения и размещения своих дочерних элементов.</param>
		/// <returns>Реальный используемый размер.</returns>
		protected override Size ArrangeOverride (Size finalSize)
		{
			if (this.InternalChildren.Count < 1)
			{
				return default (Size);
			}

			var isVertical = this.Orientation == Orientation.Vertical;
			var currentPos = default (Point);
			double unexpandedTotalSize = 0;

			// считаем суммарную высоту/ширину нераспахнутых элементов (оставшееся место потом отдадим распахнутому)
			for (var i = 0; i < this.InternalChildren.Count; i++)
			{
				var element = this.InternalChildren[i];
				if (i != this.ExpandedIndex)
				{
					unexpandedTotalSize += isVertical ? element.DesiredSize.Height : element.DesiredSize.Width;
				}
			}

			var elementHeight = finalSize.Height;
			var elementWidth = finalSize.Width;
			for (var i = 0; i < this.InternalChildren.Count; i++)
			{
				var element = this.InternalChildren[i];
				if (isVertical)
				{
					elementHeight = (i != this.ExpandedIndex) ? element.DesiredSize.Height : Math.Max (0, finalSize.Height - unexpandedTotalSize);
				}
				else
				{
					elementWidth = (i != this.ExpandedIndex) ? element.DesiredSize.Width : Math.Max (0, finalSize.Width - unexpandedTotalSize);
				}

				var bounds = new Rect (currentPos, new Size (elementWidth, elementHeight));

				// System.Diagnostics.Debug.WriteLine (string.Format ("{0}: {1}x{2} ({3}x{4})", ((FrameworkElement)element).Name, (int)bounds.Width, (int)bounds.Height, (int)element.DesiredSize.Width, (int)element.DesiredSize.Height));
				element.Arrange (bounds);
				if (isVertical)
				{
					currentPos.Y += elementHeight;
				}
				else
				{
					currentPos.X += elementWidth;
				}
			}

			return finalSize;
		}

		private static object CoerceExpandedIndex (DependencyObject panel, object value)
		{
			var accordeon = (AccordionPanel)panel;
			var index = (int)value;
			accordeon._lastWantedIndex = index;
			if (index < 0 || index >= accordeon.InternalChildren.Count)
			{
				index = 0;
			}

			return index;
		}

		private static void ExpandedIndexPropertyChangedCallback (DependencyObject panel, DependencyPropertyChangedEventArgs e)
		{
			var accordeon = (AccordionPanel)panel;
			var index = (int)e.NewValue;
			var item = accordeon.InternalChildren[index];
			if (accordeon.ExpandedItem != item)
			{
				accordeon.ExpandedItem = item;
			}
		}

		private static object CoerceExpandedItem (DependencyObject panel, object value)
		{
			var accordeon = (AccordionPanel)panel;
			var item = value as UIElement;
			accordeon._lastWantedItem = item;
			return ((accordeon.InternalChildren.Count < 1) || !accordeon.InternalChildren.Contains (item)) ?
				null : item;
		}

		private static void ExpandedItemPropertyChangedCallback (DependencyObject panel, DependencyPropertyChangedEventArgs e)
		{
			var accordeon = (AccordionPanel)panel;
			var item = (UIElement)e.NewValue;
			var idx = accordeon.InternalChildren.IndexOf (item);
			if (accordeon.ExpandedIndex != idx)
			{
				accordeon.ExpandedIndex = idx;
			}

			accordeon.UpdateChildrenExpandedProperty (idx);
		}

#pragma warning disable CA1801 // Review unused parameters
		private static object CoerceIsExpanded (DependencyObject notUsed, object value)
#pragma warning restore CA1801 // Review unused parameters
		{
			// var accordeon = (AccordionPanel)obj;
			return value;
		}

		private static void IsExpandedPropertyChangedCallback (DependencyObject expander, DependencyPropertyChangedEventArgs e)
		{
			var newValue = (bool)e.NewValue;
			if (!newValue)
			{
				return;
			}

			if (expander is FrameworkElement elem)
			{
				if (elem.Parent is AccordionPanel accordeon)
				{
					if (accordeon.ExpandedItem != elem)
					{
						accordeon.ExpandedItem = elem;
					}
				}
			}
		}

		private void UpdateChildrenExpandedProperty (int idx)
		{
			for (var i = 0; i < this.InternalChildren.Count; i++)
			{
				var item = this.InternalChildren[i];
				var oldValue = (bool)item.GetValue (IsExpandedProperty);
				var newValue = i == idx;
				if (oldValue != newValue)
				{
					item.SetValue (IsExpandedProperty, newValue);
				}
			}
		}

		private void UIElementCollectionChanged (object sender, NotifyCollectionChangedEventArgs e)
		{
			// вызывать только после первичного наполенния коллекции
			if (IsLoaded)
			{
				EnsureSomethingSelected ();
			}
		}

		private void EnsureSomethingSelected (object sender, RoutedEventArgs e)
		{
			EnsureSomethingSelected ();
		}
	}
}
