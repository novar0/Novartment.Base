using System;
using System.Globalization;

namespace Novartment.Base
{
	/// <summary>
	/// Методы для выполнения действия с перечислениями.
	/// </summary>
	public static class EnumComparer
	{
		/// <summary>
		/// Проверяет что от старого к новому значению перечисления были изменены все указанные значения-флаги.
		/// </summary>
		/// <param name="bits">Проверяемые значения-флаги.</param>
		/// <param name="oldValue">Старое значение.</param>
		/// <param name="newValue">Новое значение.</param>
		/// <returns>True если от старого к новому значению перечисления были изменены все указанные значения-флаги,
		/// иначе False.</returns>
		public static bool AllBitsChanged (Enum bits, Enum oldValue, Enum newValue)
		{
			var flagI = Convert.ToUInt64 (bits, CultureInfo.InvariantCulture);
			var oldValueI = Convert.ToUInt64 (oldValue, CultureInfo.InvariantCulture);
			var newValueI = Convert.ToUInt64 (newValue, CultureInfo.InvariantCulture);
			return ((oldValueI ^ newValueI) & flagI) == flagI;
		}

		/// <summary>
		/// Проверяет что от старого к новому значению перечисления были установлены все указанные значения-флаги.
		/// </summary>
		/// <param name="bits">Проверяемые значения-флаги.</param>
		/// <param name="oldValue">Старое значение.</param>
		/// <param name="newValue">Новое значение.</param>
		/// <returns>True если от старого к новому значению перечисления были установлены все указанные значения-флаги,
		/// иначе False.</returns>
		public static bool AllBitsSet (Enum bits, Enum oldValue, Enum newValue)
		{
			var flagI = Convert.ToUInt64 (bits, CultureInfo.InvariantCulture);
			var oldValueI = Convert.ToUInt64 (oldValue, CultureInfo.InvariantCulture);
			var newValueI = Convert.ToUInt64 (newValue, CultureInfo.InvariantCulture);
			return ((oldValueI & flagI) == 0) && ((newValueI & flagI) == flagI);
		}

		/// <summary>
		/// Проверяет что от старого к новому значению перечисления были сброшены все указанные значения-флаги.
		/// </summary>
		/// <param name="bits">Проверяемые значения-флаги.</param>
		/// <param name="oldValue">Старое значение.</param>
		/// <param name="newValue">Новое значение.</param>
		/// <returns>True если от старого к новому значению перечисления были сброшены все указанные значения-флаги,
		/// иначе False.</returns>
		public static bool AllBitsUnset (Enum bits, Enum oldValue, Enum newValue)
		{
			var flagI = Convert.ToUInt64 (bits, CultureInfo.InvariantCulture);
			var oldValueI = Convert.ToUInt64 (oldValue, CultureInfo.InvariantCulture);
			var newValueI = Convert.ToUInt64 (newValue, CultureInfo.InvariantCulture);
			return ((oldValueI & flagI) == flagI) && ((newValueI & flagI) == 0);
		}

		/// <summary>
		/// Проверяет что от старого к новому значению перечисления был изменён хотя бы один из указанных значений-флагов.
		/// </summary>
		/// <param name="bits">Проверяемые значения-флаги.</param>
		/// <param name="oldValue">Старое значение.</param>
		/// <param name="newValue">Новое значение.</param>
		/// <returns>True если от старого к новому значению перечисления был изменён хотя бы один из указанных значений-флагов,
		/// иначе False.</returns>
		public static bool AnyBitsChanged (Enum bits, Enum oldValue, Enum newValue)
		{
			var flagI = Convert.ToUInt64 (bits, CultureInfo.InvariantCulture);
			var oldValueI = Convert.ToUInt64 (oldValue, CultureInfo.InvariantCulture);
			var newValueI = Convert.ToUInt64 (newValue, CultureInfo.InvariantCulture);
			return ((oldValueI ^ newValueI) & flagI) != 0;
		}

		/// <summary>
		/// Проверяет что от старого к новому значению перечисления был установлен хотя бы один из указанных значений-флагов.
		/// </summary>
		/// <param name="bits">Проверяемые значения-флаги.</param>
		/// <param name="oldValue">Старое значение.</param>
		/// <param name="newValue">Новое значение.</param>
		/// <returns>True если от старого к новому значению перечисления был установлен хотя бы один из указанных значений-флагов,
		/// иначе False.</returns>
		public static bool AnyBitsSet (Enum bits, Enum oldValue, Enum newValue)
		{
			var flagI = Convert.ToUInt64 (bits, CultureInfo.InvariantCulture);
			var oldValueI = Convert.ToUInt64 (oldValue, CultureInfo.InvariantCulture);
			var newValueI = Convert.ToUInt64 (newValue, CultureInfo.InvariantCulture);
			for (ulong mask = 1; mask > 0; mask <<= 1)
			{
				if (((flagI & mask) != 0) &&
					((oldValueI & mask) == 0) &&
					((newValueI & mask) != 0))
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Проверяет что от старого к новому значению перечисления был сброшен хотя бы один из указанных значений-флагов.
		/// </summary>
		/// <param name="bits">Проверяемые значения-флаги.</param>
		/// <param name="oldValue">Старое значение.</param>
		/// <param name="newValue">Новое значение.</param>
		/// <returns>True если от старого к новому значению перечисления был сброшен хотя бы один из указанных значений-флагов,
		/// иначе False.</returns>
		public static bool AnyBitsUnset (Enum bits, Enum oldValue, Enum newValue)
		{
			var flagI = Convert.ToUInt64 (bits, CultureInfo.InvariantCulture);
			var oldValueI = Convert.ToUInt64 (oldValue, CultureInfo.InvariantCulture);
			var newValueI = Convert.ToUInt64 (newValue, CultureInfo.InvariantCulture);
			for (ulong mask = 1; mask > 0; mask <<= 1)
			{
				if (((flagI & mask) != 0) &&
					((oldValueI & mask) != 0) &&
					((newValueI & mask) == 0))
				{
					return true;
				}
			}

			return false;
		}
	}
}
