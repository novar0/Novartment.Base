using System;
using System.Security.Cryptography;
using System.Diagnostics.Contracts;

namespace Novartment.Base
{
	/// <summary>
	/// Провайдер для вычисления хэш-функции по алгоритму MurmurHash3 в варианте 128 бит.
	/// </summary>
	/// <remarks>
	/// Алгоритм MurmurHash3 характеризуется хорошим распределением, мощным лавинным эффектом,
	/// высокой скоростью и сравнительно высокой устойчивостью к коллизиям.
	/// </remarks>
	[CLSCompliant (false)]
	public class MurmurHash3Variant128 : HashAlgorithm
	{
		private const ulong C1 = 0x87c37b91114253d5L;
		private const ulong C2 = 0x4cf5ad432745937fL;

		private readonly uint _seed;
		private ulong _hash1;
		private ulong _hash2;
		private ulong _length;

		/// <summary>
		/// Инициализирует новый экземпляр MurmurHash3Variant128 используя модификатор (соль) по умолчанию.
		/// </summary>
		public MurmurHash3Variant128 ()
			: this (144)
		{
		}

		/// <summary>
		/// Инициализирует новый экземпляр MurmurHash3Variant128 используя указанный модификатор (соль).
		/// </summary>
		/// <param name="seed">Модификатор (соль) для вычисления хэшей.
		/// Укажите любое случайное значение, недоступное для сторонних компонентов.
		/// В зависимости от него может измениться характер распределение хэшей для ваших данных.</param>
		public MurmurHash3Variant128 (uint seed)
		{
			_seed = seed;
			Initialize ();
		}

		/// <summary>
		/// Получает размер вычисленного хэш-кода в битах.
		/// </summary>
		public override int HashSize => 128;

		/// <summary>
		/// Инициализирует хэш-алгоритм, подготавливая его к следующему вычислению хэш-кода.
		/// </summary>
		public override void Initialize ()
		{
			_hash1 = _seed;
			_length = 0L;
		}

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
			if ((index < 0) || (index > array.Length) || ((index == array.Length) && (count > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (index));
			}
			if ((count < 0) || (count > array.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (count));
			}
			Contract.EndContractBlock ();

			_length += (ulong)count;

			// read 128 bits, 16 bytes, 2 longs in eacy cycle
			while (count >= 16)
			{
				var k1 = BitConverter.ToUInt64 (array, index);
				index += 8;

				var k2 = BitConverter.ToUInt64 (array, index);
				index += 8;

				count -= 16;

				_hash1 ^= MixKey1 (k1);

				_hash1 = (_hash1 << 27) | (_hash1 >> 37);
				_hash1 += _hash2;
				_hash1 = _hash1 * 5 + 0x52dce729;

				_hash2 ^= MixKey2 (k2);

				_hash2 = (_hash2 << 31) | (_hash2 >> 33);
				_hash2 += _hash1;
				_hash2 = _hash2 * 5 + 0x38495ab5;
			}

			// if the input MOD 16 != 0
			if (count > 0)
			{
				ulong k1 = 0;
				ulong k2 = 0;

				// little endian (x86) processing
				switch (count)
				{
					case 15:
						k2 ^= (ulong)array[index + 14] << 48; // fall through
						goto case 14;
					case 14:
						k2 ^= (ulong)array[index + 13] << 40; // fall through
						goto case 13;
					case 13:
						k2 ^= (ulong)array[index + 12] << 32; // fall through
						goto case 12;
					case 12:
						k2 ^= (ulong)array[index + 11] << 24; // fall through
						goto case 11;
					case 11:
						k2 ^= (ulong)array[index + 10] << 16; // fall through
						goto case 10;
					case 10:
						k2 ^= (ulong)array[index + 9] << 8; // fall through
						goto case 9;
					case 9:
						k2 ^= array[index + 8]; // fall through
						goto case 8;
					case 8:
						k1 ^= BitConverter.ToUInt64 (array, index);
						break;
					case 7:
						k1 ^= (ulong)array[index + 6] << 48; // fall through
						goto case 6;
					case 6:
						k1 ^= (ulong)array[index + 5] << 40; // fall through
						goto case 5;
					case 5:
						k1 ^= (ulong)array[index + 4] << 32; // fall through
						goto case 4;
					case 4:
						k1 ^= (ulong)array[index + 3] << 24; // fall through
						goto case 3;
					case 3:
						k1 ^= (ulong)array[index + 2] << 16; // fall through
						goto case 2;
					case 2:
						k1 ^= (ulong)array[index + 1] << 8; // fall through
						goto case 1;
					case 1:
						k1 ^= array[index];
						break;
				}

				_hash1 ^= MixKey1 (k1);
				_hash2 ^= MixKey2 (k2);
			}
		}

		/// <summary>
		/// Завершает вычисление хэша после обработки последних данных криптографическим потоковым объектом.
		/// </summary>
		/// <returns>Вычисляемый хэш-код.</returns>
		protected override byte[] HashFinal ()
		{
			_hash1 ^= _length;
			_hash2 ^= _length;

			_hash1 += _hash2;
			_hash2 += _hash1;

			_hash1 = MixFinal (_hash1);
			_hash2 = MixFinal (_hash2);

			_hash1 += _hash2;
			_hash2 += _hash1;

			var hash = new byte[16];

			Array.Copy (BitConverter.GetBytes (_hash1), 0, hash, 0, 8);
			Array.Copy (BitConverter.GetBytes (_hash2), 0, hash, 8, 8);

			return hash;
		}

		private static ulong MixKey1 (ulong k1)
		{
			k1 *= C1;
			k1 = (k1 << 31) | (k1 >> 33);
			k1 *= C2;
			return k1;
		}

		private static ulong MixKey2 (ulong k2)
		{
			k2 *= C2;
			k2 = (k2 << 31) | (k2 >> 33);
			k2 *= C1;
			return k2;
		}

		private static ulong MixFinal (ulong k)
		{
			// avalanche bits
			k ^= k >> 33;
			k *= 0xff51afd7ed558ccdL;
			k ^= k >> 33;
			k *= 0xc4ceb9fe1a85ec53L;
			k ^= k >> 33;
			return k;
		}
	}
}
