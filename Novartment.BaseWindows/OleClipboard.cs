using System;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using System.Threading;
using Novartment.Base.UnsafeWin32;

namespace Novartment.Base
{
	/// <summary>
	/// Буфер обмена данными между приложениями на основе Win32 функций OleClipboard*.
	/// </summary>
	/// <remarks>
	/// Все методы делают несколько попыток выполнить операцию с паузой между попытками.
	/// Это необходимо в большинстве случаев ввиду
	/// полной асинхронности субъектов, использующих буфер обмена и
	/// его недоступности для других пока он используется.
	/// </remarks>
	public sealed class OleClipboard :
		IClipboard
	{
		/// <summary>Количество попыток выполнения запрошенных операций.</summary>
		private const int RetryCount = 10;

		/// <summary>Пауза между попытками выполнения запрошенных операций (миллисекунды).</summary>
		private const int RetryPeriodMs = 100;

		private readonly Func<IDataContainer, IDataObject> _toComDataObjectConverter;
		private readonly Func<IDataObject, IDataContainer> _fromComDataObjectConverter;

		/// <summary>
		/// Инициализирует новый экземпляр OleClipboard,
		/// использующий указанные конвертеры контейнера данных в/из внутреннего формата.
		/// </summary>
		/// <param name="toComDataObjectConverter">Конвертер контейнера данных во внутренний формат.</param>
		/// <param name="fromComDataObjectConverter">Конвертер контейнера данных из внутреннего формата.</param>
		public OleClipboard (
			Func<IDataContainer, IDataObject> toComDataObjectConverter,
			Func<IDataObject, IDataContainer> fromComDataObjectConverter)
		{
			if (toComDataObjectConverter == null)
			{
				throw new ArgumentNullException (nameof (toComDataObjectConverter));
			}

			if (fromComDataObjectConverter == null)
			{
				throw new ArgumentNullException (nameof (fromComDataObjectConverter));
			}

			Contract.EndContractBlock ();

			_toComDataObjectConverter = toComDataObjectConverter;
			_fromComDataObjectConverter = fromComDataObjectConverter;
		}

		/// <summary>
		/// Закрепляет данные в буфере обмена так, чтобы они оставались доступными даже после закрытия приложения.
		/// </summary>
		[SecurityCritical]
		public void Flush ()
		{
			var currentThreadApartmentState = Thread.CurrentThread.GetApartmentState ();
			if (currentThreadApartmentState != ApartmentState.STA)
			{
				throw new ThreadStateException ("For OLE functions thread must be STA.");
			}

			var num = RetryCount;
			while (true)
			{
				var hr = NativeMethods.Ole32.OleFlushClipboard ();
				if (hr >= 0)
				{
					break;
				}

				if (--num == 0)
				{
					Marshal.ThrowExceptionForHR (hr);
				}

				Thread.Sleep (RetryPeriodMs);
			}
		}

		/// <summary>
		/// Сравнивает указанные данные с содержимым буфера обмена.
		/// </summary>
		/// <param name="data">Объект данных для проверки на совпадение с содержимым буфера обмена.</param>
		/// <returns>True если указанный объект данных совпадает с тем, который находится в буфере обмена, иначе False.</returns>
		[SecurityCritical]
		public bool IsCurrent (IDataContainer data)
		{
			if (data == null)
			{
				throw new ArgumentNullException (nameof (data));
			}

			Contract.EndContractBlock ();

			var currentThreadApartmentState = Thread.CurrentThread.GetApartmentState ();
			if (currentThreadApartmentState != ApartmentState.STA)
			{
				throw new ThreadStateException ("For OLE functions thread must be STA.");
			}

			var dataObject = _toComDataObjectConverter.Invoke (data);
			var num = RetryCount;
			int hr;
			while (true)
			{
				hr = NativeMethods.Ole32.OleIsCurrentClipboard (dataObject);
				if ((hr >= 0) || --num == 0)
				{
					break;
				}

				Thread.Sleep (RetryPeriodMs);
			}

			Marshal.ThrowExceptionForHR (hr);
			return hr == 0;
		}

		/// <summary>
		/// Получает объект с данными, представляющий содержимое буфера обмена.
		/// </summary>
		/// <returns>
		/// Объект дающий доступ к содержимому буфера обмена,
		/// или null если в буфере обмена нет данных.
		/// </returns>
		[SecurityCritical]
		public IDataContainer GetData ()
		{
			var currentThreadApartmentState = Thread.CurrentThread.GetApartmentState ();
			if (currentThreadApartmentState != ApartmentState.STA)
			{
				throw new ThreadStateException ("For OLE functions thread must be STA.");
			}

			var num = RetryCount;
			IDataObject result;
			while (true)
			{
				result = null;
				var hr = NativeMethods.Ole32.OleGetClipboard (ref result);
				if (hr >= 0)
				{
					break;
				}

				if (--num == 0)
				{
					Marshal.ThrowExceptionForHR (hr);
				}

				Thread.Sleep (RetryPeriodMs);
			}

			return _fromComDataObjectConverter.Invoke (result);
		}

		/// <summary>
		/// Помещает указанные данные в буфер обмена.
		/// </summary>
		/// <param name="dataObject">Объект с данными для помещения в буфер обмена.</param>
		[SecurityCritical]
		public void SetData (IDataContainer dataObject)
		{
			if (dataObject == null)
			{
				throw new ArgumentNullException (nameof (dataObject));
			}

			Contract.EndContractBlock ();

			var currentThreadApartmentState = Thread.CurrentThread.GetApartmentState ();
			if (currentThreadApartmentState != ApartmentState.STA)
			{
				throw new ThreadStateException ("For OLE functions thread must be STA.");
			}

			var data = _toComDataObjectConverter.Invoke (dataObject);
			var num = RetryCount;
			while (true)
			{
				var hr = NativeMethods.Ole32.OleSetClipboard (data);
				if (hr >= 0)
				{
					break;
				}

				if (--num == 0)
				{
					Marshal.ThrowExceptionForHR (hr);
				}

				Thread.Sleep (RetryPeriodMs);
			}
		}
	}
}
