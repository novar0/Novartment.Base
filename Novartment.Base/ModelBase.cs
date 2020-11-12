using System;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Reflection;

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
		/// Освобождает ресурсы, занимаемые моделью.
		/// </summary>
		public virtual void Dispose ()
		{
			PropertyChanged = null;
			GC.SuppressFinalize (this);
		}

		/// <summary>
		/// Инициирует событие PropertyChanged с указанными аргументами.
		/// </summary>
		/// <param name="propertyChangedEventArgs">Аргументы события PropertyChanged.</param>
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
		protected void RaisePropertyChanged (string propertyName)
		{
			if (propertyName == null)
			{
				throw new ArgumentNullException (nameof (propertyName));
			}

			Contract.EndContractBlock ();

			RaisePropertyChanged (new PropertyChangedEventArgs (propertyName));
		}
	}
}
