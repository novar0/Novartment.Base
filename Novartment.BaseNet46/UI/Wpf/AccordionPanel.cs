using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Diagnostics.Contracts;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Панель, состоящая из экспандеров, один из которых всегда развёрнут.
	/// </summary>
	public class AccordionPanel : Panel
	{
		/// <summary>
		/// Инициализирует новый экземпляр AccordionPanel.
		/// </summary>
		public AccordionPanel ()
			: base ()
		{
			this.Loaded += EnsureSomethingSelected;
			this.Unloaded -= EnsureSomethingSelected;
		}

		#region ExpandedIndex property

		private int _lastWantedIndex = -1;

		/// <summary>
		/// Получает или устанавливает номер открытого экспандера.
		/// </summary>
		public int ExpandedIndex
		{
			get { return (int)GetValue (ExpandedIndexProperty); }
			set { SetValue (ExpandedIndexProperty, value); }
		}

		/// <summary>
		/// Получает или устанавливает номер открытого экспандера.
		/// </summary>
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Visible)]
		[BrowsableAttribute (true)]
		[DescriptionAttribute ("Specifies index of expanded item.")]
		[CategoryAttribute ("Layout")]
		public static readonly DependencyProperty ExpandedIndexProperty = DependencyProperty.Register (
			"ExpandedIndex",
			typeof (int),
			typeof (AccordionPanel),
			new FrameworkPropertyMetadata (0, FrameworkPropertyMetadataOptions.AffectsArrange, ExpandedIndexPropertyChangedCallback, CoerceExpandedIndex));

		private static object CoerceExpandedIndex (DependencyObject obj, object value)
		{
			var accordeon = (AccordionPanel)obj;
			var index = (int)value;
			accordeon._lastWantedIndex = index;
			if (index < 0 || index >= accordeon.InternalChildren.Count)
			{
				index = 0;
			}
			return index;
		}

		private static void ExpandedIndexPropertyChangedCallback (DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			var accordeon = (AccordionPanel)obj;
			var index = (int)e.NewValue;
			var item = accordeon.InternalChildren[index];
			if (accordeon.ExpandedItem != item)
			{
				accordeon.ExpandedItem = item;
			}
		}

		#endregion

		#region ExpandedItem property

		private UIElement _lastWantedItem = null;

		/// <summary>
		/// Получает или устанавливает открытый экспандер.
		/// </summary>
		public UIElement ExpandedItem
		{
			get
			{
				return (UIElement)GetValue (ExpandedItemProperty);
			}
			set
			{
				SetValue (ExpandedItemProperty, value);
			}
		}

		/// <summary>
		/// Получает или устанавливает открытый экспандер.
		/// </summary>
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Visible)]
		[BrowsableAttribute (true)]
		[DescriptionAttribute ("Specifies expanded item.")]
		[CategoryAttribute ("Layout")]
		public static readonly DependencyProperty ExpandedItemProperty = DependencyProperty.Register (
			"ExpandedItem",
			typeof (UIElement),
			typeof (AccordionPanel),
			new FrameworkPropertyMetadata (null, FrameworkPropertyMetadataOptions.AffectsArrange, ExpandedItemPropertyChangedCallback, CoerceExpandedItem));

		private static object CoerceExpandedItem (DependencyObject obj, object value)
		{
			var accordeon = (AccordionPanel)obj;
			var item = value as UIElement;
			accordeon._lastWantedItem = item;
			return ((accordeon.InternalChildren.Count < 1) || !accordeon.InternalChildren.Contains (item)) ?
				null : item;
		}

		private static void ExpandedItemPropertyChangedCallback (DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			var accordeon = (AccordionPanel)obj;
			var item = (UIElement)e.NewValue;
			var idx = accordeon.InternalChildren.IndexOf (item);
			if (accordeon.ExpandedIndex != idx)
			{
				accordeon.ExpandedIndex = idx;
			}
			accordeon.UpdateChildrenExpandedProperty (idx);
		}
		#endregion

		#region Orientation property

		/// <summary>Получает или устанавливает значение, представляющее ориентацию элемента.</summary>
		public Orientation Orientation
		{
			get
			{
				return (Orientation)GetValue (OrientationProperty);
			}
			set
			{
				SetValue (OrientationProperty, value);
			}
		}

		/// <summary>Получает или устанавливает значение, представляющее ориентацию элемента.</summary>
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Visible)]
		[BrowsableAttribute (true)]
		[DescriptionAttribute ("Specifies index of expanded item.")]
		[CategoryAttribute ("Layout")]
		public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register (
			"Orientation",
			typeof (Orientation),
			typeof (AccordionPanel),
			new FrameworkPropertyMetadata (Orientation.Vertical, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentArrange | FrameworkPropertyMetadataOptions.AffectsParentMeasure));

		#endregion

		#region IsExpanded attached property

		/// <summary>
		/// Получает признак того, что экспандер открыт.
		/// </summary>
		/// <param name="obj">Экспандер.</param>
		/// <returns>True если экспандер открыт, иначе False.</returns>
		public static bool GetIsExpanded (DependencyObject obj)
		{
			if (obj == null)
			{
				throw new ArgumentNullException (nameof (obj));
			}
			Contract.EndContractBlock ();

			return (bool)obj.GetValue (IsExpandedProperty);
		}

		/// <summary>
		/// Устанавливает признак того, что экспандер открыт.
		/// </summary>
		/// <param name="obj">Экспандер.</param>
		/// <param name="value">Признак того, что экспандер открыт.</param>
		public static void SetIsExpanded (DependencyObject obj, bool value)
		{
			if (obj == null)
			{
				throw new ArgumentNullException (nameof (obj));
			}
			Contract.EndContractBlock ();

			obj.SetValue (IsExpandedProperty, value);
		}

		/// <summary>
		/// Получает признак того, что экспандер открыт.
		/// </summary>
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Visible)]
		[BrowsableAttribute (true)]
		[DescriptionAttribute ("Specifies expanded state of item.")]
		[CategoryAttribute ("Layout")]
		public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.RegisterAttached (
			"IsExpanded",
			typeof (bool),
			typeof (AccordionPanel),
			new FrameworkPropertyMetadata (false, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsMeasure, IsExpandedPropertyChangedCallback, CoerceIsExpanded));

		private static object CoerceIsExpanded (DependencyObject obj, object value)
		{
			//var accordeon = (AccordionPanel)obj;
			return value;
		}

		private static void IsExpandedPropertyChangedCallback (DependencyObject obj, DependencyPropertyChangedEventArgs e)
		{
			var newValue = (bool)e.NewValue;
			if (!newValue)
			{
				return;
			}
			var elem = obj as FrameworkElement;
			if (elem != null)
			{
				var accordeon = elem.Parent as AccordionPanel;
				if (accordeon != null)
				{
					if (accordeon.ExpandedItem != elem)
					{
						accordeon.ExpandedItem = elem;
					}
				}
			}
		}

		#endregion

		/// <summary>Получает значение, которое указывает, организовывает ли этот объект своих потомков в единой размерности.</summary>
		protected override bool HasLogicalOrientation => true;

		/// <summary>Получает значение, представляющее ориентацию элемента.</summary>
		protected override Orientation LogicalOrientation => this.Orientation;

		private void UpdateChildrenExpandedProperty (int idx)
		{
			for (var i = 0; i < this.InternalChildren.Count; i++)
			{
				var item = this.InternalChildren[i];
				var oldValue = (bool)item.GetValue (IsExpandedProperty);
				var newValue = (i == idx);
				if (oldValue != newValue)
				{
					item.SetValue (IsExpandedProperty, newValue);
				}
			}
		}

		/// <summary>Создает новый объект <see cref="T:System.Windows.Controls.UIElementCollection" />.</summary>
		/// <param name="logicalParent">Логический родительский элемент создаваемой коллекции.</param>
		/// <returns>Упорядоченная коллекция элементов, которые имеют указанный логический родительский элемент.</returns>
		protected override UIElementCollection CreateUIElementCollection (FrameworkElement logicalParent)
		{
			var collection = new ObservableUIElementCollection (this, logicalParent);
			collection.CollectionChanged += UIElementCollectionChanged;
			return collection;
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

		#region Measure & Arrange overrides

		/// <summary>Измеряет дочерние элементы объекта, ожидая их упорядочивания во время передачи метода ArrangeOverride().</summary>
		/// <param name="availableSize">Верхний предел, который не должен быть превышен.</param>
		/// <returns>Объект, который представляет желаемый размер элемента.</returns>
		protected override Size MeasureOverride (Size availableSize)
		{
			if (base.InternalChildren.Count < 1)
			{
				return availableSize;
			}
			var constraint = availableSize;
			var panelSize = new Size ();
			var isVertical = this.Orientation == Orientation.Vertical;
			// даём пределы и меряем все элементы кроме распахнутого в надежде что они не попросят слишком много
			for (var i = 0; i < InternalChildren.Count; i++)
			{
				if (i == this.ExpandedIndex)
				{
					continue;
				}
				var element = this.InternalChildren[i];
				//Size testingSize = new Size (double.PositiveInfinity, double.PositiveInfinity); element.Measure (testingSize);
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
			if (base.InternalChildren.Count < 1)
			{
				return new Size ();
			}

			var isVertical = this.Orientation == Orientation.Vertical;
			var currentPos = new Point ();
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

				//System.Diagnostics.Debug.WriteLine (string.Format ("{0}: {1}x{2} ({3}x{4})", ((FrameworkElement)element).Name, (int)bounds.Width, (int)bounds.Height, (int)element.DesiredSize.Width, (int)element.DesiredSize.Height));

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
		#endregion
	}
}
