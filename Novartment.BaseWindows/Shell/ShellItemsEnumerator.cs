using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Threading;
using Novartment.Base.UnsafeWin32;

namespace Novartment.Base.Shell
{
	/// <summary>
	/// Перечислитель элементов на основе перечислителя IEnumShellItems.
	/// </summary>
	internal class ShellItemsEnumerator :
		IEnumerator<IShellItem>
	{
		private IEnumShellItems _nativeEnumShellItems;
		private IShellItem _current = null;

		/// <summary>
		/// Инициализирует новый экземпляр ShellItemsEnumerator на основе указанного IEnumShellItems.
		/// </summary>
		/// <param name="nativeEnumShellItems">Перечислитель IEnumShellItems.</param>
		internal ShellItemsEnumerator (IEnumShellItems nativeEnumShellItems)
		{
			if (nativeEnumShellItems == null)
			{
				throw new ArgumentNullException (nameof (nativeEnumShellItems));
			}

			Contract.EndContractBlock ();

			if (nativeEnumShellItems == null)
			{
				throw new ArgumentNullException (nameof (nativeEnumShellItems));
			}

			_nativeEnumShellItems = nativeEnumShellItems;
		}

		/// <summary>
		/// Получает текущий элемент перечислителя.
		/// </summary>
		public IShellItem Current => _current;

		object IEnumerator.Current => _current;

#pragma warning disable CA1063 // Implement IDisposable Correctly
		/// <summary>
		/// Освобождает занятые объектом ресурсы.
		/// </summary>
		public void Dispose ()
#pragma warning restore CA1063 // Implement IDisposable Correctly
		{
			var oldValue = Interlocked.Exchange (ref _nativeEnumShellItems, null);
			if (oldValue != null)
			{
				Marshal.FinalReleaseComObject (oldValue);
			}
		}

		/// <summary>
		/// Перемещает перечислитель к следующему элементу строки.
		/// </summary>
		/// <returns>true, если перечислитель был успешно перемещен к следующему элементу;
		/// false, если перечислитель достиг конца.</returns>
		public bool MoveNext ()
		{
			uint itemsRequested = 1;
			var hr = _nativeEnumShellItems.Next (itemsRequested, out IShellItem shellItem, out uint numItemsReturned);

			if ((numItemsReturned < itemsRequested) || (hr != 0))
			{
				return false;
			}

			_current = shellItem;

			return true;
		}

		/// <summary>
		/// Возвращает перечислитель в исходное положение.
		/// </summary>
		public void Reset ()
		{
			var hr = _nativeEnumShellItems.Reset ();
			if (hr != 0)
			{
				throw new InvalidOperationException ("IEnumShellItems.Reset() failed.", Marshal.GetExceptionForHR (hr));
			}

			_current = null;
		}
	}
}
