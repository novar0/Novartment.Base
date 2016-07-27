using System;
using System.Text;
using System.Security.Cryptography;
using System.Diagnostics.Contracts;

namespace Novartment.Base
{
	/// <summary>
	/// Провайдер для вычисления хэш-функции по алгоритму MurmurHash3 в варианте 32 бита.
	/// </summary>
	/// <remarks>
	/// Алгоритм Murmur3 характеризуется хорошим распределением, мощным лавинным эффектом,
	/// высокой скоростью и сравнительно высокой устойчивостью к коллизиям.
	/// </remarks>
	[CLSCompliant (false)]
	public class MurmurHash3 : HashAlgorithm
	{
		private readonly uint _seed;
		private uint _hash;
		private uint _length;

		/// <summary>
		/// Инициализирует новый экземпляр MurmurHash3 используя модификатор (соль) по умолчанию.
		/// </summary>
		public MurmurHash3 ()
			: this (144)
		{
		}

		/// <summary>
		/// Инициализирует новый экземпляр MurmurHash3 используя указанный модификатор (соль).
		/// </summary>
		/// <param name="seed">Модификатор (соль) для вычисления хэшей.
		/// Укажите любое случайное значение, недоступное для сторонних компонентов.
		/// В зависимости от него может измениться характер распределение хэшей для ваших данных.</param>
		public MurmurHash3 (uint seed)
		{
			_seed = seed;
			Initialize ();
		}

		/// <summary>
		/// Получает размер вычисленного хэш-кода в битах.
		/// </summary>
		public override int HashSize => 32;

		/// <summary>
		/// Инициализирует хэш-алгоритм, подготавливая его к следующему вычислению хэш-кода.
		/// </summary>
		public override void Initialize ()
		{
			_hash = _seed;
			_length = 0;
		}

		#region static methods GetHashCode

		/// <summary>
		/// Вычисляет хэш-функцию для указанного source.
		/// </summary>
		/// <param name="source">Данные для вычисления хэш-функции.</param>
		/// <returns>Хэш-функция для указанного source.</returns>
		public static uint GetHashCode (byte[] source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}
			Contract.EndContractBlock ();

			var hash = HashCore (source, 0, source.Length, 144);
			return HashFinal (hash, (uint)source.Length);
		}

		/// <summary>
		/// Вычисляет хэш-функцию для указанного source.
		/// </summary>
		/// <param name="source">Данные для вычисления хэш-функции.</param>
		/// <returns>Хэш-функция для указанного source.</returns>
		public static uint GetHashCode (char source)
		{
			var bytes = BitConverter.GetBytes (source);
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse (bytes);
			}

			var hash = HashCore (bytes, 0, bytes.Length, 144);
			return HashFinal (hash, (uint)bytes.Length);
		}

		/// <summary>
		/// Вычисляет хэш-функцию для указанного source.
		/// </summary>
		/// <param name="source">Данные для вычисления хэш-функции.</param>
		/// <returns>Хэш-функция для указанного source.</returns>
		public static uint GetHashCode (string source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}
			Contract.EndContractBlock ();

			var bytes = Encoding.UTF8.GetBytes (source);

			var hash = HashCore (bytes, 0, bytes.Length, 144);
			return HashFinal (hash, (uint)bytes.Length);
		}

		/// <summary>
		/// Вычисляет хэш-функцию для указанного source.
		/// </summary>
		/// <param name="source">Данные для вычисления хэш-функции.</param>
		/// <returns>Хэш-функция для указанного source.</returns>
		public static uint GetHashCode (float source)
		{
			var bytes = BitConverter.GetBytes (source);
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse (bytes);
			}

			var hash = HashCore (bytes, 0, bytes.Length, 144);
			return HashFinal (hash, (uint)bytes.Length);
		}

		/// <summary>
		/// Вычисляет хэш-функцию для указанного source.
		/// </summary>
		/// <param name="source">Данные для вычисления хэш-функции.</param>
		/// <returns>Хэш-функция для указанного source.</returns>
		public static uint GetHashCode (double source)
		{
			var bytes = BitConverter.GetBytes (source);
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse (bytes);
			}

			var hash = HashCore (bytes, 0, bytes.Length, 144);
			return HashFinal (hash, (uint)bytes.Length);
		}

		/// <summary>
		/// Вычисляет хэш-функцию для указанного source.
		/// </summary>
		/// <param name="source">Данные для вычисления хэш-функции.</param>
		/// <returns>Хэш-функция для указанного source.</returns>
		public static uint GetHashCode (short source)
		{
			var bytes = BitConverter.GetBytes (source);
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse (bytes);
			}

			var hash = HashCore (bytes, 0, bytes.Length, 144);
			return HashFinal (hash, (uint)bytes.Length);
		}

		/// <summary>
		/// Вычисляет хэш-функцию для указанного source.
		/// </summary>
		/// <param name="source">Данные для вычисления хэш-функции.</param>
		/// <returns>Хэш-функция для указанного source.</returns>
		public static uint GetHashCode (ushort source)
		{
			var bytes = BitConverter.GetBytes (source);
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse (bytes);
			}

			var hash = HashCore (bytes, 0, bytes.Length, 144);
			return HashFinal (hash, (uint)bytes.Length);
		}

		/// <summary>
		/// Вычисляет хэш-функцию для указанного source.
		/// </summary>
		/// <param name="source">Данные для вычисления хэш-функции.</param>
		/// <returns>Хэш-функция для указанного source.</returns>
		public static uint GetHashCode (int source)
		{
			var bytes = BitConverter.GetBytes (source);
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse (bytes);
			}

			var hash = HashCore (bytes, 0, bytes.Length, 144);
			return HashFinal (hash, (uint)bytes.Length);
		}

		/// <summary>
		/// Вычисляет хэш-функцию для указанного source.
		/// </summary>
		/// <param name="source">Данные для вычисления хэш-функции.</param>
		/// <returns>Хэш-функция для указанного source.</returns>
		public static uint GetHashCode (uint source)
		{
			var bytes = BitConverter.GetBytes (source);
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse (bytes);
			}

			var hash = HashCore (bytes, 0, bytes.Length, 144);
			return HashFinal (hash, (uint)bytes.Length);
		}

		/// <summary>
		/// Вычисляет хэш-функцию для указанного source.
		/// </summary>
		/// <param name="source">Данные для вычисления хэш-функции.</param>
		/// <returns>Хэш-функция для указанного source.</returns>
		public static uint GetHashCode (long source)
		{
			var bytes = BitConverter.GetBytes (source);
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse (bytes);
			}

			var hash = HashCore (bytes, 0, bytes.Length, 144);
			return HashFinal (hash, (uint)bytes.Length);
		}

		/// <summary>
		/// Вычисляет хэш-функцию для указанного source.
		/// </summary>
		/// <param name="source">Данные для вычисления хэш-функции.</param>
		/// <returns>Хэш-функция для указанного source.</returns>
		public static uint GetHashCode (ulong source)
		{
			var bytes = BitConverter.GetBytes (source);
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse (bytes);
			}

			var hash = HashCore (bytes, 0, bytes.Length, 144);
			return HashFinal (hash, (uint)bytes.Length);
		}

		/// <summary>
		/// Вычисляет хэш-функцию для указанного source.
		/// </summary>
		/// <param name="source">Данные для вычисления хэш-функции.</param>
		/// <returns>Хэш-функция для указанного source.</returns>
		public static uint GetHashCode (DateTime source)
		{
			var bytes1 = BitConverter.GetBytes (source.Ticks);
			var bytes2 = BitConverter.GetBytes ((int)source.Kind);
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse (bytes1);
				Array.Reverse (bytes2);
			}

			var hash = HashCore (bytes1, 0, bytes1.Length, 144);
			hash = HashCore (bytes2, 0, bytes2.Length, hash);
			return HashFinal (hash, (uint)bytes1.Length + (uint)bytes2.Length);
		}

		/// <summary>
		/// Вычисляет хэш-функцию для указанного source.
		/// </summary>
		/// <param name="source">Данные для вычисления хэш-функции.</param>
		/// <returns>Хэш-функция для указанного source.</returns>
		public static uint GetHashCode (decimal source)
		{
			var bits = Decimal.GetBits (source);
			uint hash = 144;
			uint length = 0;
			foreach (var value in bits)
			{
				var bytes = BitConverter.GetBytes (value);
				if (!BitConverter.IsLittleEndian)
				{
					Array.Reverse (bytes);
				}
				hash = HashCore (bytes, 0, bytes.Length, hash);
				length += (uint)bytes.Length;
			}

			return HashFinal (hash, length);
		}

		#endregion

		/// <summary>
		/// Передаёт данные, записанные в объект, на вход хэш-алгоритма для вычисления хэша.
		/// </summary>
		/// <param name="array">Входные данные, для которых вычисляется хэш-код. </param>
		/// <param name="index">Смещение в массиве байтов, начиная с которого следует использовать данные. </param>
		/// <param name="count">Число байтов в массиве для использования в качестве данных. </param>
		protected override void HashCore (byte[] array, int index, int count)
		{
			if (array == null)
			{
				throw new ArgumentNullException (nameof (array));
			}
			if ((index < 0) || (index > array.Length) || ((index == array.Length) && (index > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (index));
			}
			if ((count < 0) || (count > array.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}
			Contract.EndContractBlock ();

			_hash = HashCore (array, index, count, _hash);
			_length += (uint)count;
		}

		/// <summary>
		/// Завершает вычисление хэша после обработки последних данных криптографическим потоковым объектом.
		/// </summary>
		/// <returns>Вычисляемый хэш-код.</returns>
		protected override byte[] HashFinal ()
		{
			var bytes = BitConverter.GetBytes (HashFinal (_hash, _length));
			if (!BitConverter.IsLittleEndian)
			{
				Array.Reverse (bytes);
			}
			return bytes;
		}

		private static uint HashCore (byte[] array, int ibStart, int cbSize, uint hash)
		{
			const uint c1 = 0xcc9e2d51;
			const uint c2 = 0x1b873593;
			uint k1;

			while (cbSize > 0)
			{
				switch (cbSize)
				{
					default: // >= 4
						k1 = (uint)(array[ibStart] | array[ibStart + 1] << 8 | array[ibStart + 2] << 16 | array[ibStart + 3] << 24);
						k1 *= c1;
						k1 = (k1 << 15) | (k1 >> 17);
						k1 *= c2;
						hash ^= k1;
						hash = (hash << 13) | (hash >> 19);
						hash = hash * 5 + 0xe6546b64;
						ibStart += 4;
						cbSize -= 4;
						break;
					case 3:
						k1 = (uint)(array[ibStart] | array[ibStart + 1] << 8 | array[ibStart + 2] << 16);
						k1 *= c1;
						k1 = (k1 << 15) | (k1 >> 17);
						k1 *= c2;
						hash ^= k1;
						return hash;
					case 2:
						k1 = (uint)(array[ibStart] | array[ibStart + 1] << 8);
						k1 *= c1;
						k1 = (k1 << 15) | (k1 >> 17);
						k1 *= c2;
						hash ^= k1;
						return hash;
					case 1:
						k1 = array[ibStart];
						k1 *= c1;
						k1 = (k1 << 15) | (k1 >> 17);
						k1 *= c2;
						hash ^= k1;
						return hash;
				}
			}
			return hash;
		}

		private static uint HashFinal (uint hash, uint length)
		{
			hash ^= length;
			hash ^= hash >> 16;
			hash *= 0x85ebca6b;
			hash ^= hash >> 13;
			hash *= 0xc2b2ae35;
			hash ^= hash >> 16;
			return hash;
		}
	}
}
