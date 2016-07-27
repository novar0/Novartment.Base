using System;
using System.ComponentModel;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Novartment.Base
{
	/// <summary>
	/// Базовый класс для моделей при использовании шаблона Model-View-*.
	/// </summary>
	public abstract class ModelBase :
		INotifyPropertyChanged,
		IDisposable
	{
		/// <summary>
		/// Происходит когда меняется значение одного из свойств модели.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Инициирует событие PropertyChanged с указанными аргументами.
		/// </summary>
		/// <param name="propertyChangedEventArgs">Аргументы события PropertyChanged.</param>
		[SuppressMessage ("Microsoft.Design",
			"CA1030:UseEventsWhereAppropriate",
			Justification = "Already event")]
		protected virtual void RaisePropertyChanged (PropertyChangedEventArgs propertyChangedEventArgs)
		{
			if (propertyChangedEventArgs == null)
			{
				throw new ArgumentNullException (nameof (propertyChangedEventArgs));
			}
			Contract.EndContractBlock ();

			Contract.Assert (
				this.GetType ().GetRuntimeProperty (propertyChangedEventArgs.PropertyName) != null,
				"Property [" + propertyChangedEventArgs.PropertyName + "] not found");
			this.PropertyChanged?.Invoke (this, propertyChangedEventArgs);
		}

		/// <summary>
		/// Инициирует событие PropertyChanged с указанным именем свойства.
		/// </summary>
		/// <param name="propertyName">Имя свойства для события PropertyChanged.</param>
		[SuppressMessage ("Microsoft.Design", "CA1030:UseEventsWhereAppropriate",
			Justification = "This cannot be an event")]
		protected void RaisePropertyChanged (string propertyName)
		{
			if (propertyName == null)
			{
				throw new ArgumentNullException (nameof (propertyName));
			}
			Contract.EndContractBlock ();

			RaisePropertyChanged (new PropertyChangedEventArgs (propertyName));
		}

		/// <summary>
		/// Освобождает ресурсы, занимаемые моделью.
		/// </summary>
		[SuppressMessage ("Microsoft.Usage",
			"CA1816:CallGCSuppressFinalizeCorrectly",
			Justification = "There is no meaning to introduce a finalizer in derived type."),
		SuppressMessage (
			"Microsoft.Design",
			"CA1063:ImplementIDisposableCorrectly",
			Justification = "Implemented correctly.")]
		public virtual void Dispose ()
		{
			PropertyChanged = null;
		}
	}
}
