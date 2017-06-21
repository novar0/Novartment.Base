using System;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Markup;
using Novartment.Base.Collections.Immutable;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Defines a base class for markup extensions which are managed by a central.
	/// <see cref="MarkupExtensionManager"/>.
	/// This allows the associated markup targets to be updated via the manager.
	/// </summary>
	/// <remarks>
	/// The MarkupExtensionTrackedTargets holds a weak reference to the target object to allow it to update the target.
	/// A weak reference is used to avoid a circular dependency which would prevent the target being garbage collected.
	/// </remarks>
	public abstract class TargetsTrackingExtensionBase : MarkupExtension
	{
		/// <summary>List of weak reference to the target DependencyObjects to allow them to be garbage collected</summary>
		private SingleLinkedListNode<WeakReference> _targetObjects = null;

		/// <summary>The target property</summary>
		private object _targetProperty;

		/// <summary>
		/// Initializes a new instance of MarkupExtensionTrackedTargets using specified manager.
		/// </summary>
		/// <param name="manager">Manager for use.</param>
		protected TargetsTrackingExtensionBase (MarkupExtensionManager manager)
		{
			if (manager == null)
			{
				throw new ArgumentNullException (nameof (manager));
			}

			Contract.EndContractBlock ();

			manager.RegisterExtension (this);
		}

		/// <summary>
		/// Is an associated target still alive ie not garbage collected.
		/// </summary>
		public bool IsTargetAlive
		{
			get
			{
				// for normal elements the _targetObjects.Count will always be 1.
				// for templates the Count may be zero if this method is called in the middle of window elaboration
				// after the template has been instantiated but before the elements that use it have been.
				// In this case return true so that we don't unhook the extension prematurely.
				if (_targetObjects == null)
				{
					return true;
				}

				// otherwise just check whether the referenced target(s) are alive
				var node = _targetObjects;
				while (node != null)
				{
					var reference = node.Value;
					if (reference.IsAlive)
					{
						return true;
					}

					node = node.Next;
				}

				return false;
			}
		}

		/// <summary>
		/// Return the target objects the extension is associated with.
		/// </summary>
		/// <remarks>
		/// For normal elements their will be a single target.
		/// For templates their may be zero or more targets.
		/// </remarks>
		protected SingleLinkedListNode<WeakReference> TargetObjects => _targetObjects;

		/// <summary>
		/// Return the Target Property the extension is associated with.
		/// </summary>
		/// <remarks>
		/// Can either be a <see cref="DependencyProperty"/> or <see cref="PropertyInfo"/>.
		/// </remarks>
		protected object TargetProperty => _targetProperty;

		/// <summary>
		/// Return the type of the Target Property.
		/// </summary>
		protected Type TargetPropertyType
		{
			get
			{
				Type result = null;
				if (_targetProperty is DependencyProperty)
				{
					result = (_targetProperty as DependencyProperty).PropertyType;
				}
				else
				{
					if (_targetProperty is PropertyInfo)
					{
						result = (_targetProperty as PropertyInfo).PropertyType;
					}
					else
					{
						if (_targetProperty != null)
						{
							result = _targetProperty.GetType ();
						}
					}
				}

				return result;
			}
		}

		/// <summary>
		/// Return the value for this instance of the Markup Extension.
		/// </summary>
		/// <param name="serviceProvider">The service provider.</param>
		/// <returns>The value of the element.</returns>
		public override object ProvideValue (IServiceProvider serviceProvider)
		{
			RegisterTarget (serviceProvider);
			object result = this;

			// when used in a template the _targetProperty may be null - in this case return this
			if (_targetProperty != null)
			{
				result = GetValue ();
			}

			return result;
		}

		/// <summary>
		/// Update the associated targets.
		/// </summary>
		public void UpdateTargets ()
		{
			var node = _targetObjects;
			while (node != null)
			{
				var reference = node.Value;
				if (reference.IsAlive)
				{
					UpdateTarget (reference.Target);
				}

				node = node.Next;
			}
		}

		/// <summary>
		/// Is the given object the target for the extension.
		/// </summary>
		/// <param name="target">The target to check.</param>
		/// <returns>True if the object is one of the targets for this extension.</returns>
		public bool IsTarget (object target)
		{
			var node = _targetObjects;
			while (node != null)
			{
				var reference = node.Value;
				if (reference.IsAlive && (reference.Target == target))
				{
					return true;
				}

				node = node.Next;
			}

			return false;
		}

		/// <summary>
		/// Return the value associated with the key from the resource manager.
		/// </summary>
		/// <returns>The value from the resources if possible otherwise the default value.</returns>
		[SuppressMessage (
			"Microsoft.Design",
			"CA1024:UsePropertiesWhereAppropriate",
			Justification = "This method supposed to do some work in derived classes and can not be property.")]
		protected abstract object GetValue ();

		/// <summary>
		/// Called by <see cref="ProvideValue(IServiceProvider)"/> to register the target and object using the extension.
		/// </summary>
		/// <param name="serviceProvider">The service provider.</param>
		protected virtual void RegisterTarget (IServiceProvider serviceProvider)
		{
			if (serviceProvider == null)
			{
				throw new ArgumentNullException (nameof (serviceProvider));
			}

			Contract.EndContractBlock ();

			var provideValueTarget = serviceProvider.GetService (typeof (IProvideValueTarget)) as IProvideValueTarget;
			var target = provideValueTarget.TargetObject;

			// Check if the target is a SharedDp which indicates the target is a template.
			// In this case we don't register the target and ProvideValue returns this
			// allowing the extension to be evaluated for each instance of the template
			var fullName = target?.GetType ().FullName;
			if (fullName != "System.Windows.SharedDp")
			{
				_targetProperty = provideValueTarget.TargetProperty;
				var spinWait = default (SpinWait);
				var newRef = new WeakReference (target);
				while (true)
				{
					var state1 = _targetObjects;
					var newState = state1.AddItem (newRef);

					// заменяем состояние если оно не изменилось с момента вызова
					var state2 = Interlocked.CompareExchange (ref _targetObjects, newState, state1);
					if (state1 == state2)
					{
						return;
					}

					// состояние изменилось за время вызова, поэтому повторим попытку после паузы
					spinWait.SpinOnce ();
				}
			}
		}

		/// <summary>
		/// Called by <see cref="UpdateTargets"/> to update each target referenced by the extension.
		/// </summary>
		/// <param name="target">The target to update.</param>
		protected virtual void UpdateTarget (object target)
		{
			if (_targetProperty is DependencyProperty)
			{
				(target as DependencyObject)?.SetValue (_targetProperty as DependencyProperty, GetValue ());
			}
			else
			{
				(_targetProperty as PropertyInfo)?.SetValue (target, GetValue (), null);
			}
		}
	}
}
