using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using static System.Linq.Enumerable;

namespace Novartment.Base.UI.Wpf
{
	/*
	Use:
	===
	<Window ResxExtension.ResxName="WpfApp.MainWindow">
	convert to
	<TextBlock Text="{Resx MyText}"/>
	or
	<TextBlock Text="{Resx ResxName=MyApp.TestWindow, Key=MyText}"/>

	Use for non-text:
	================
	<TextBlock Margin="{Resx Key=MyMargin, DefaultValue='18,0,0,71'}"/>

	Formatting:
	==========
	<Binding StringFormat="Selected Item: {0}" ElementName="_fileListBox" Path="SelectedItem"/>
	convert to
	<Resx Key="MyFormatString" BindingElementName="_fileListBox" BindingPath="SelectedItem"/>

	<Resx Key="MyMultiFormatString">
		<Resx BindingElementName="_fileListBox" BindingPath="Name"/>
		<Resx BindingElementName="_fileListBox" BindingPath="SelectedItem"/>
	</Resx>


	Changing the Culture Dynamically at Runtime:
	===========================================
	set the Thread.CurrentThread.CurrentUICulture
	ResxExtension.UpdateAllTargets ()


	Hiding RESX Files
	================
	<EmbeddedResource Include="TestWindow.resx" />
	convert to
	<EmbeddedResource Include="TestWindow.resx">
	  <DependentUpon>TestWindow.xaml</DependentUpon>
	  <SubType>Designer</SubType>
	</EmbeddedResource>
	*/

	/// <summary>
	/// A markup extension to allow resources for WPF Windows and controls to be retrieved
	/// from an embedded resource (resx) file associated with the window or control.
	/// </summary>
	[SuppressMessage (
		"Microsoft.Naming",
		"CA1704:IdentifiersShouldBeSpelledCorrectly",
		MessageId = "Resx",
		Justification = "'RESX' is the standard term for resource files.")]
	[MarkupExtensionReturnType (typeof (object))]
	[ContentProperty ("Children")]
	public class ResxExtension : TargetsTrackingExtensionBase
	{
		/// <summary>
		/// The ResxName attached property.
		/// </summary>
		[SuppressMessage (
			"Microsoft.Naming",
			"CA1704:IdentifiersShouldBeSpelledCorrectly",
			MessageId = "Resx",
			Justification = "'RESX' is the standard term for resource files.")]
		public static readonly DependencyProperty DefaultResxNameProperty =
			DependencyProperty.RegisterAttached (
			"DefaultResxName",
			typeof (string),
			typeof (ResxExtension),
			new FrameworkPropertyMetadata (null, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits, OnDefaultResxNamePropertyChanged));

		/// <summary>Cached resource managers.</summary>
		private static readonly Dictionary<string, WeakReference> _resourceManagers = new Dictionary<string, WeakReference> ();

		/// <summary>The manager for resx extensions.</summary>
		private static readonly MarkupExtensionManager _markupManager = new MarkupExtensionManager (40);

		/// <summary>The binding (if any) used to store the binding properties for the extension.</summary>
		private readonly ILazyValueHolder<Binding> _binding = new LazyValueHolder<Binding> ();

		/// <summary>The child ResxExtensions (if any) when using MultiBinding expressions.</summary>
		private readonly Collection<ResxExtension> _children = new Collection<ResxExtension> ();

		/// <summary>The explicitly set embedded Resx Name (if any).</summary>
		private string _resxName;

		/// <summary>The default resx name (based on the attached property).</summary>
		private string _defaultResxName;

		/// <summary>The key used to retrieve the resource.</summary>
		private string _key;

		/// <summary>The default value for the property.</summary>
		private string _defaultValue;

		/// <summary>
		/// The resource manager to use for this extension. Holding a strong reference to the
		/// Resource Manager keeps it in the cache while ever there are ResxExtensions that are using it.
		/// </summary>
		private ResourceManager _resourceManager;

		/// <summary>
		/// Create a new instance of the markup extension.
		/// </summary>
		public ResxExtension ()
			: base (_markupManager)
		{
		}

		/// <summary>
		/// Create a new instance of the markup extension.
		/// </summary>
		/// <param name="key">The key used to get the value from the resources.</param>
		public ResxExtension (string key)
			: base (_markupManager)
		{
			_key = key;
		}

		/// <summary>
		/// Return the MarkupManager for this extension.
		/// </summary>
		public static MarkupExtensionManager MarkupManager => _markupManager;

		/// <summary>
		/// The fully qualified name of the embedded resx (without .resources) to get the resource from.
		/// </summary>
		[SuppressMessage (
			"Microsoft.Naming",
			"CA1704:IdentifiersShouldBeSpelledCorrectly",
			MessageId = "Resx",
			Justification = "'RESX' is the standard term for resource files.")]
		public string ResxName
		{
			get
			{
				// if the ResxName property is not set explicitly then check the attached property
				var result = _resxName;
				var isNullOrEmpty = string.IsNullOrEmpty (result);
				if (isNullOrEmpty)
				{
					if (_defaultResxName == null)
					{
						var node = this.TargetObjects;
						while (node != null)
						{
							var targetRef = node.Value;
							if (targetRef.IsAlive)
							{
								if (targetRef.Target is DependencyObject dependencyObject)
								{
									_defaultResxName = dependencyObject.GetValue (DefaultResxNameProperty) as string;
									break;
								}
							}

							node = node.Next;
						}
					}

					result = _defaultResxName;
				}

				return result;
			}

			set
			{
				_resxName = value;
			}
		}

		/// <summary>
		/// The name of the resource key.
		/// </summary>
		public string Key
		{
			get => _key;
			set { _key = value; }
		}

		/// <summary>
		/// The default value to use if the resource can't be found.
		/// </summary>
		/// <remarks>
		/// This particularly useful for properties which require non-null values
		/// because it allows the page to be displayed even if the resource can't be loaded.
		/// </remarks>
		public string DefaultValue
		{
			get => _defaultValue;
			set { _defaultValue = value; }
		}

		/// <summary>
		/// The child Resx elements (if any).
		/// </summary>
		/// <remarks>
		/// You can nest Resx elements in this case the parent Resx element
		/// value is used as a format string to format the values from child Resx
		/// elements similar to a <see cref="MultiBinding"/> eg If a Resx has two
		/// child elements then you.
		/// </remarks>
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public Collection<ResxExtension> Children => _children;

		/// <summary>
		/// Return the associated binding for the extension
		/// </summary>
		public Binding Binding => _binding.Value;

		/// <summary>
		/// Use the Resx value to format bound data.  See <see cref="System.Windows.Data.Binding.ElementName"/>.
		/// </summary>
		[DefaultValue (null)]
		public string BindingElementName
		{
			get => _binding.Value.ElementName;

			set
			{
				_binding.Value.ElementName = value;
			}
		}

		/// <summary>
		/// Use the Resx value to format bound data.  See <see cref="System.Windows.Data.Binding.Path"/>.
		/// </summary>
		[DefaultValue (null)]
		public PropertyPath BindingPath
		{
			get => _binding.Value.Path;

			set
			{
				_binding.Value.Path = value;
			}
		}

		/// <summary>
		/// Use the Resx value to format bound data.  See <see cref="System.Windows.Data.Binding.RelativeSource"/>.
		/// </summary>
		[DefaultValue (null)]
		public RelativeSource BindingRelativeSource
		{
			get => _binding.Value.RelativeSource;

			set
			{
				_binding.Value.RelativeSource = value;
			}
		}

		/// <summary>
		/// Use the Resx value to format bound data.  See <see cref="System.Windows.Data.Binding.Source"/>.
		/// </summary>
		[DefaultValue (null)]
		public object BindingSource
		{
			get => _binding.Value.Source;

			set
			{
				_binding.Value.Source = value;
			}
		}

		/// <summary>
		/// Use the Resx value to format bound data.  See <see cref="System.Windows.Data.Binding.XPath"/>.
		/// </summary>
		[DefaultValue (null)]
		public string BindingXPath
		{
			get => _binding.Value.XPath;

			set
			{
				_binding.Value.XPath = value;
			}
		}

		/// <summary>
		/// Use the Resx value to format bound data.  See <see cref="System.Windows.Data.Binding.Converter"/>.
		/// </summary>
		[DefaultValue (null)]
		public IValueConverter BindingConverter
		{
			get => _binding.Value.Converter;

			set
			{
				_binding.Value.Converter = value;
			}
		}

		/// <summary>
		/// Use the Resx value to format bound data.  See <see cref="System.Windows.Data.BindingBase.BindingGroupName"/>.
		/// </summary>
		[DefaultValue (null)]
		public string BindingGroupName
		{
			get => _binding.Value.BindingGroupName;

			set
			{
				_binding.Value.BindingGroupName = value;
			}
		}

		/// <summary>
		/// Use the Resx value to format bound data.  See <see cref="System.Windows.Data.Binding.ConverterCulture"/>.
		/// </summary>
		[DefaultValue (null)]
		public CultureInfo BindingConverterCulture
		{
			get => _binding.Value.ConverterCulture;

			set
			{
				_binding.Value.ConverterCulture = value;
			}
		}

		/// <summary>
		/// Use the Resx value to format bound data.  See <see cref="System.Windows.Data.Binding.ConverterParameter"/>.
		/// </summary>
		[DefaultValue (null)]
		public object BindingConverterParameter
		{
			get => _binding.Value.ConverterParameter;

			set
			{
				_binding.Value.ConverterParameter = value;
			}
		}

		/// <summary>
		/// Use the Resx value to format bound data.  See <see cref="System.Windows.Data.Binding.BindsDirectlyToSource"/>.
		/// </summary>
		[DefaultValue (false)]
		public bool BindsDirectlyToSource
		{
			get => _binding.Value.BindsDirectlyToSource;

			set
			{
				_binding.Value.BindsDirectlyToSource = value;
			}
		}

		/// <summary>
		/// Use the Resx value to format bound data.  See <see cref="System.Windows.Data.Binding.Mode"/>.
		/// </summary>
		[DefaultValue (BindingMode.Default)]
		public BindingMode BindingMode
		{
			get => _binding.Value.Mode;

			set
			{
				_binding.Value.Mode = value;
			}
		}

		/// <summary>
		/// Use the Resx value to format bound data.  See <see cref="System.Windows.Data.Binding.NotifyOnSourceUpdated"/>.
		/// </summary>
		[DefaultValue (false)]
		public bool BindingNotifyOnSourceUpdated
		{
			get => _binding.Value.NotifyOnSourceUpdated;

			set
			{
				_binding.Value.NotifyOnSourceUpdated = value;
			}
		}

		/// <summary>
		/// Use the Resx value to format bound data.  See <see cref="System.Windows.Data.Binding.NotifyOnTargetUpdated"/>.
		/// </summary>
		[DefaultValue (false)]
		public bool BindingNotifyOnTargetUpdated
		{
			get => _binding.Value.NotifyOnTargetUpdated;

			set
			{
				_binding.Value.NotifyOnTargetUpdated = value;
			}
		}

		/// <summary>
		/// Use the Resx value to format bound data.  See <see cref="System.Windows.Data.Binding.NotifyOnValidationError"/>.
		/// </summary>
		[DefaultValue (false)]
		public bool BindingNotifyOnValidationError
		{
			get => _binding.Value.NotifyOnValidationError;

			set
			{
				_binding.Value.NotifyOnValidationError = value;
			}
		}

		/// <summary>
		/// Use the Resx value to format bound data.  See <see cref="System.Windows.Data.Binding.AsyncState"/>.
		/// </summary>
		[DefaultValue (null)]
		public object BindingAsyncState
		{
			get => _binding.Value.AsyncState;

			set
			{
				_binding.Value.AsyncState = value;
			}
		}

		/// <summary>
		/// Use the Resx value to format bound data.  See <see cref="System.Windows.Data.Binding.IsAsync"/>.
		/// </summary>
		[DefaultValue (false)]
		public bool BindingIsAsync
		{
			get => _binding.Value.IsAsync;

			set
			{
				_binding.Value.IsAsync = value;
			}
		}

		/// <summary>
		/// Use the Resx value to format bound data.  See <see cref="System.Windows.Data.BindingBase.FallbackValue"/>.
		/// </summary>
		[DefaultValue (null)]
		public object BindingFallbackValue
		{
			get => _binding.Value.FallbackValue;

			set
			{
				_binding.Value.FallbackValue = value;
			}
		}

		/// <summary>
		/// Use the Resx value to format bound data.  See <see cref="System.Windows.Data.BindingBase.TargetNullValue"/>.
		/// </summary>
		[DefaultValue (null)]
		public object BindingTargetNullValue
		{
			get => _binding.Value.TargetNullValue;

			set
			{
				_binding.Value.TargetNullValue = value;
			}
		}

		/// <summary>
		/// Use the Resx value to format bound data.  See <see cref="System.Windows.Data.Binding.ValidatesOnDataErrors"/>.
		/// </summary>
		[DefaultValue (false)]
		public bool BindingValidatesOnDataErrors
		{
			get => _binding.Value.ValidatesOnDataErrors;

			set
			{
				_binding.Value.ValidatesOnDataErrors = value;
			}
		}

		/// <summary>
		/// Use the Resx value to format bound data.  See <see cref="System.Windows.Data.Binding.ValidatesOnExceptions"/>.
		/// </summary>
		[DefaultValue (false)]
		public bool BindingValidatesOnExceptions
		{
			get => _binding.Value.ValidatesOnExceptions;

			set
			{
				_binding.Value.ValidatesOnExceptions = value;
			}
		}

		/// <summary>
		/// Use the Resx value to format bound data.  See <see cref="System.Windows.Data.Binding.UpdateSourceTrigger"/>.
		/// </summary>
		[DefaultValue (UpdateSourceTrigger.Default)]
		public UpdateSourceTrigger BindingUpdateSourceTrigger
		{
			get => _binding.Value.UpdateSourceTrigger;

			set
			{
				_binding.Value.UpdateSourceTrigger = value;
			}
		}

		/// <summary>
		/// Use the Resx value to format bound data.  See <see cref="System.Windows.Data.Binding.ValidationRules"/>.
		/// </summary>
		[DefaultValue (false)]
		public Collection<ValidationRule> BindingValidationRules => _binding.Value.ValidationRules;

		/// <summary>
		/// Is this ResxExtension being used as a multi-binding parent.
		/// </summary>
		private bool IsMultiBindingParent => _children.Count > 0;

		/// <summary>
		/// Is this ResxExtension being used inside another Resx Extension for multi-binding.
		/// </summary>
		private bool IsMultiBindingChild => TargetPropertyType == typeof (Collection<ResxExtension>);

		/// <summary>
		/// Use the Markup Manager to update all targets.
		/// </summary>
		public static void UpdateAllTargets ()
		{
			_markupManager.UpdateAllTargets ();
		}

		/// <summary>
		/// Update the ResxExtension target with the given key.
		/// </summary>
		/// <param name="key">Resource key for update.</param>
		public static void UpdateTarget (string key)
		{
			var node = _markupManager.ActiveExtensions;
			while (node != null)
			{
				if (node.Value is ResxExtension ext && (ext.Key == key))
				{
					ext.UpdateTargets ();
				}

				node = node.Next;
			}
		}

		/// <summary>
		/// Get the DefaultResxName attached property for the given target.
		/// </summary>
		/// <param name="target">The Target object.</param>
		/// <returns>The name of the Resx.</returns>
		[SuppressMessage (
			"Microsoft.Naming",
			"CA1704:IdentifiersShouldBeSpelledCorrectly",
			MessageId = "Resx",
			Justification = "'RESX' is the standard term for resource files.")]
		[AttachedPropertyBrowsableForChildren (IncludeDescendants = true)]
		public static string GetDefaultResxName (DependencyObject target)
		{
			if (target == null)
			{
				throw new ArgumentNullException (nameof (target));
			}

			Contract.EndContractBlock ();

			return (string)target.GetValue (DefaultResxNameProperty);
		}

		/// <summary>
		/// Set the DefaultResxName attached property for the given target.
		/// </summary>
		/// <param name="target">The Target object.</param>
		/// <param name="value">The name of the Resx.</param>
		[SuppressMessage (
			"Microsoft.Naming",
			"CA1704:IdentifiersShouldBeSpelledCorrectly",
			MessageId = "Resx",
			Justification = "'RESX' is the standard term for resource files.")]
		public static void SetDefaultResxName (DependencyObject target, string value)
		{
			if (target == null)
			{
				throw new ArgumentNullException (nameof (target));
			}

			Contract.EndContractBlock ();

			target.SetValue (DefaultResxNameProperty, value);
		}

		/// <summary>
		/// Return the value for this instance of the Markup Extension.
		/// </summary>
		/// <param name="serviceProvider">The service provider.</param>
		/// <returns>The value of the element.</returns>
		public override object ProvideValue (IServiceProvider serviceProvider)
		{
			object result;

			// register the target and property so we can update them
			RegisterTarget (serviceProvider);

			var isValidBinding = !string.IsNullOrEmpty (this.Key) || IsBindingExpression ();
			if (!isValidBinding)
			{
				throw new InvalidOperationException ("Required property Key or Binding not set.");
			}

			// if the extension is used in a template or as a child of another
			// resx extension (for multi-binding) then return this
			if (TargetProperty == null || this.IsMultiBindingChild)
			{
				result = this;
			}
			else
			{
				// if this extension has child Resx elements then invoke AFTER this method has returned
				// to setup the MultiBinding on the target element.
				if (this.IsMultiBindingParent)
				{
					var binding = CreateMultiBinding ();
					result = binding.ProvideValue (serviceProvider);
				}
				else
				{
					var isBindingExpression = IsBindingExpression ();
					if (isBindingExpression)
					{
						// if this is a simple binding then return the binding
						var binding = CreateBinding ();
						result = binding.ProvideValue (serviceProvider);
					}
					else
					{
						// otherwise return the value from the resources
						result = GetValue ();
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Return the value for the markup extension.
		/// </summary>
		/// <returns>The value from the resources if possible otherwise the default value.</returns>
		protected override object GetValue ()
		{
			var isKeyEmpty = string.IsNullOrEmpty (_key);
			if (isKeyEmpty)
			{
				return null;
			}

			object result = null;
			var isResxNameEmpty = string.IsNullOrEmpty (this.ResxName);
			if (!isResxNameEmpty)
			{
				if (_resourceManager == null)
				{
					_resourceManager = GetResourceManager (this.ResxName);
				}

				if (_resourceManager != null)
				{
					result = _resourceManager.GetObject (_key/*, CultureManager.UICulture*/);
				}

				if (!this.IsMultiBindingChild)
				{
					result = ConvertValue (result);
				}
			}

			return result ?? GetDefaultValue (this.Key);
		}

		/// <summary>
		/// Update the given target when the culture changes.
		/// </summary>
		/// <param name="target">The target to update.</param>
		protected override void UpdateTarget (object target)
		{
			// binding of child extensions is done by the parent
			if (this.IsMultiBindingChild)
			{
				return;
			}

			var el = target as FrameworkElement;
			if (this.IsMultiBindingParent)
			{
				if (el != null)
				{
					var multiBinding = CreateMultiBinding ();
					el.SetBinding (TargetProperty as DependencyProperty, multiBinding);
				}
			}
			else
			{
				var isBindingExpression = IsBindingExpression ();
				if (isBindingExpression)
				{
					if (el != null)
					{
						var binding = CreateBinding ();
						el.SetBinding (TargetProperty as DependencyProperty, binding);
					}
				}
				else
				{
					base.UpdateTarget (target);
				}
			}
		}

		/// <summary>
		/// Check if the assembly contains an embedded resx of the given name.
		/// </summary>
		/// <param name="assembly">The assembly to check.</param>
		/// <param name="resxName">The name of the resource we are looking for.</param>
		/// <returns>True if the assembly contains the resource.</returns>
		private static bool HasEmbeddedResx (Assembly assembly, string resxName)
		{
			if (assembly.IsDynamic)
			{
				return false;
			}

			try
			{
				var resources = assembly.GetManifestResourceNames ();
				var searchName = resxName + ".resources";
				var isResourcesHasResource = resources.Any (resource => searchName.Equals (resource, StringComparison.OrdinalIgnoreCase));
				if (isResourcesHasResource)
				{
					return true;
				}
			}
			catch (NotSupportedException)
			{
				// GetManifestResourceNames may throw an exception
				// for some assemblies - just ignore these assemblies.
			}

			return false;
		}

		/// <summary>
		/// Handle a change to the attached DefaultResxName property.
		/// </summary>
		/// <param name="element">the dependency object (a WPF element).</param>
		/// <param name="args">the dependency property changed event arguments.</param>
		/// <remarks>In design mode update the extension with the correct ResxName.</remarks>
		private static void OnDefaultResxNamePropertyChanged (DependencyObject element, DependencyPropertyChangedEventArgs args)
		{
			var isInDesignMode = DesignerProperties.GetIsInDesignMode (element);
			if (isInDesignMode)
			{
				var node = _markupManager.ActiveExtensions;
				while (node != null)
				{
					var ext = node.Value as ResxExtension;
					var isNodeTargetOfElement = (ext != null) && ext.IsTarget (element);
					if (isNodeTargetOfElement)
					{
						// force the resource manager to be reloaded when the attached resx name changes
						ext._resourceManager = null;
						ext._defaultResxName = args.NewValue as string;
						ext.UpdateTarget (element);
					}

					node = node.Next;
				}
			}
		}

		/// <summary>
		/// Create a binding for this Resx Extension.
		/// </summary>
		/// <returns>A binding for this Resx Extension.</returns>
		private Binding CreateBinding ()
		{
			var binding = new Binding ();
			var isBindingExpression = IsBindingExpression ();
			if (isBindingExpression)
			{
				// copy all the properties of the binding to the new binding
				if (_binding.Value.ElementName != null)
				{
					binding.ElementName = _binding.Value.ElementName;
				}

				if (_binding.Value.RelativeSource != null)
				{
					binding.RelativeSource = _binding.Value.RelativeSource;
				}

				if (_binding.Value.Source != null)
				{
					binding.Source = _binding.Value.Source;
				}

				binding.AsyncState = _binding.Value.AsyncState;
				binding.BindingGroupName = _binding.Value.BindingGroupName;
				binding.BindsDirectlyToSource = _binding.Value.BindsDirectlyToSource;
				binding.Converter = _binding.Value.Converter;
				binding.ConverterCulture = _binding.Value.ConverterCulture;
				binding.ConverterParameter = _binding.Value.ConverterParameter;
				binding.FallbackValue = _binding.Value.FallbackValue;
				binding.IsAsync = _binding.Value.IsAsync;
				binding.Mode = _binding.Value.Mode;
				binding.NotifyOnSourceUpdated = _binding.Value.NotifyOnSourceUpdated;
				binding.NotifyOnTargetUpdated = _binding.Value.NotifyOnTargetUpdated;
				binding.NotifyOnValidationError = _binding.Value.NotifyOnValidationError;
				binding.Path = _binding.Value.Path;
				binding.TargetNullValue = _binding.Value.TargetNullValue;
				binding.UpdateSourceTrigger = _binding.Value.UpdateSourceTrigger;
				binding.ValidatesOnDataErrors = _binding.Value.ValidatesOnDataErrors;
				binding.ValidatesOnExceptions = _binding.Value.ValidatesOnExceptions;
				foreach (var rule in _binding.Value.ValidationRules)
				{
					binding.ValidationRules.Add (rule);
				}

				binding.XPath = _binding.Value.XPath;
				binding.StringFormat = GetValue () as string;
			}
			else
			{
				binding.Source = GetValue ();
			}

			return binding;
		}

		/// <summary>
		/// Create new MultiBinding that binds to the child Resx Extensioins.
		/// </summary>
		/// <returns>Created MultiBinding.</returns>
		private MultiBinding CreateMultiBinding ()
		{
			var result = new MultiBinding ();
			foreach (var child in _children)
			{
				// ensure the child has a resx name
				if (child.ResxName == null)
				{
					child.ResxName = ResxName;
				}

				result.Bindings.Add (child.CreateBinding ());
			}

			result.StringFormat = GetValue () as string;
			return result;
		}

		// Have any of the binding properties been set.
		private bool IsBindingExpression ()
		{
			if (!_binding.IsValueCreated)
			{
				return false;
			}

			return _binding.Value.Source != null || _binding.Value.RelativeSource != null ||
					_binding.Value.ElementName != null || _binding.Value.XPath != null ||
					_binding.Value.Path != null;
		}

		/// <summary>
		/// Find the assembly that contains the type.
		/// </summary>
		/// <returns>The assembly if loaded (otherwise null).</returns>
		private Assembly FindResourceAssembly ()
		{
			var assembly = Assembly.GetEntryAssembly ();

			// check the entry assembly first - this will short circuit a lot of searching
			var isResourceFound = (assembly != null) && HasEmbeddedResx (assembly, this.ResxName);
			if (isResourceFound)
			{
				return assembly;
			}

			var assemblies = AppDomain.CurrentDomain.GetAssemblies ();
			foreach (var searchAssembly in assemblies)
			{
				// skip system assemblies
				var name = searchAssembly.FullName;
				var isSystemAssembly = name.StartsWith ("Microsoft.", StringComparison.Ordinal) ||
					name.StartsWith ("System.", StringComparison.Ordinal) ||
					name.StartsWith ("System,", StringComparison.Ordinal) ||
					name.StartsWith ("mscorlib,", StringComparison.Ordinal) ||
					name.StartsWith ("PresentationFramework,", StringComparison.Ordinal) ||
					name.StartsWith ("WindowsBase,", StringComparison.Ordinal);
				if (!isSystemAssembly)
				{
					var isAssemblyHasEmbeddedResx = HasEmbeddedResx (searchAssembly, ResxName);
					if (isAssemblyHasEmbeddedResx)
					{
						return searchAssembly;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Get the resource manager for this type.
		/// </summary>
		/// <param name="resxName">The name of the embedded resx.</param>
		/// <returns>The resource manager.</returns>
		/// <remarks>Caches resource managers to improve performance.</remarks>
		private ResourceManager GetResourceManager (string resxName)
		{
			ResourceManager result = null;
			if (resxName == null)
			{
				return null;
			}

			var isValueGetted = _resourceManagers.TryGetValue (resxName, out WeakReference reference);
			if (isValueGetted)
			{
				result = reference.Target as ResourceManager;

				// if the resource manager has been garbage collected then remove the cache
				// entry (it will be readded)
				if (result == null)
				{
					_resourceManagers.Remove (resxName);
				}
			}

			if (result == null)
			{
				var assembly = FindResourceAssembly ();
				if (assembly != null)
				{
					result = new ResourceManager (resxName, assembly);
				}

				_resourceManagers.Add (resxName, new WeakReference (result));
			}

			return result;
		}

		/// <summary>
		/// Convert a resource object to the type required by the WPF element.
		/// </summary>
		/// <param name="value">The resource value to convert.</param>
		/// <returns>The WPF element value.</returns>
		private object ConvertValue (object value)
		{
			var result = value;

			// allow for resources to either contain simple strings or typed data
			var targetType = TargetPropertyType;
			if (targetType != null)
			{
				if (value is string strValue && (targetType != typeof (string)) && (targetType != typeof (object)))
				{
					var tc = TypeDescriptor.GetConverter (targetType);
					result = tc.ConvertFromInvariantString (strValue);
				}
			}

			return result;
		}

		/// <summary>
		/// Return the default value for the property.
		/// </summary>
		/// <param name="key">Property for retrieving default value.</param>
		/// <returns>Default value for specified property.</returns>
		private object GetDefaultValue (string key)
		{
			object result = _defaultValue;
			var targetType = this.TargetPropertyType;
			if (_defaultValue == null)
			{
				if ((targetType == typeof (string)) || (targetType == typeof (object)) || this.IsMultiBindingChild)
				{
					result = "#" + key;
				}
			}
			else
			{
				if (targetType != null)
				{
					// convert the default value if necessary to the required type
					if ((targetType != typeof (string)) && (targetType != typeof (object)))
					{
						try
						{
							var tc = TypeDescriptor.GetConverter (targetType);
							result = tc.ConvertFromInvariantString (_defaultValue);
						}
						catch (NotSupportedException)
						{
						}
					}
				}
			}

			return result;
		}
	}
}
