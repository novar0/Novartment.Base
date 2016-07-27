using System;

namespace Novartment.Base
{
	/// <summary>
	/// Простое число.
	/// </summary>
	public static class PrimeNumber
	{
		private static readonly int[] _Primes = {
			3, 7, 11, 0x11, 0x17, 0x1d, 0x25, 0x2f, 0x3b, 0x47, 0x59, 0x6b, 0x83, 0xa3, 0xc5, 0xef,
			0x125, 0x161, 0x1af, 0x209, 0x277, 0x2f9, 0x397, 0x44f, 0x52f, 0x63d, 0x78b, 0x91d, 0xaf1, 0xd2b, 0xfd1, 0x12fd,
			0x16cf, 0x1b65, 0x20e3, 0x2777, 0x2f6f, 0x38ff, 0x446f, 0x521f, 0x628d, 0x7655, 0x8e01, 0xaa6b, 0xcc89, 0xf583, 0x126a7, 0x1619b,
			0x1a857, 0x1fd3b, 0x26315, 0x2dd67, 0x3701b, 0x42023, 0x4f361, 0x5f0ed, 0x72125, 0x88e31, 0xa443b, 0xc51eb, 0xec8c1, 0x11bdbf, 0x154a3f, 0x198c4f,
			0x1ea867, 0x24ca19, 0x2c25c1, 0x34fa1b, 0x3f928f, 0x4c4987, 0x5b8b6f, 0x6dda89,
			8639249, 10367101, 12440513, 14928619 };

		/// <summary>
		/// Получает простое число равное или большее чем указанное.
		/// </summary>
		/// <param name="min">Минимальное необходимое значение простого числа.</param>
		/// <returns>Простое число равное или большее чем указано.</returns>
		public static int GetGreaterThanOrEqual (int min)
		{
			for (var i = 0; i < _Primes.Length; i++)
			{
				var num2 = _Primes[i];
				if (num2 >= min)
				{
					return num2;
				}
			}
			for (var j = min | 1; j < int.MaxValue; j += 2)
			{
				if (IsValid (j))
				{
					return j;
				}
			}
			return min;
		}

		/// <summary>
		/// Проверяет, является ли указанное число простым.
		/// </summary>
		/// <param name="candidate">Число для проверки.</param>
		/// <returns>True если указанное число - простое, иначе False.</returns>
		public static bool IsValid (int candidate)
		{
			if ((candidate & 1) == 0)
			{
				return (candidate == 2);
			}

			var num = (int)Math.Sqrt (candidate);
			for (var i = 3; i <= num; i += 2)
			{
				if ((candidate % i) == 0)
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Получает минимальное простое число больше двух.
		/// </summary>
		public static int MinValue => _Primes[0];
	}
}
