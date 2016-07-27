using System;
using static System.Linq.Enumerable;
using System.Text;
using Xunit;

namespace Novartment.Base.Test
{
	public class MurmurHash3Tests
	{
		[Fact, Trait ("Category", "Cryptography.MurmurHash3")]
		public void ConstantValues ()
		{
			var provider = new MurmurHash3 (0);
			var providerAlt = new MurmurHash3 (321);

			// differend seeds produce different hashes
			Assert.NotEqual (provider.ComputeHash (new byte[0]), providerAlt.ComputeHash (new byte[0]));

			// common cases
			Assert.NotEqual (
				BitConverter.ToUInt32 (provider.ComputeHash (BitConverter.GetBytes (0)), 0),
				BitConverter.ToUInt32 (provider.ComputeHash (BitConverter.GetBytes (1)), 0));
			Assert.NotEqual (
				BitConverter.ToUInt32 (provider.ComputeHash (BitConverter.GetBytes (0)), 0),
				BitConverter.ToUInt32 (provider.ComputeHash (BitConverter.GetBytes (int.MinValue)), 0));
			Assert.NotEqual (
				BitConverter.ToUInt32 (provider.ComputeHash (BitConverter.GetBytes (0)), 0),
				BitConverter.ToUInt32 (provider.ComputeHash (BitConverter.GetBytes (int.MaxValue)), 0));
			Assert.NotEqual (
				BitConverter.ToUInt32 (provider.ComputeHash (BitConverter.GetBytes (int.MinValue)), 0),
				BitConverter.ToUInt32 (provider.ComputeHash (BitConverter.GetBytes (int.MaxValue)), 0));
			Assert.NotEqual (
				BitConverter.ToUInt32 (provider.ComputeHash (Encoding.UTF8.GetBytes ("")), 0),
				BitConverter.ToUInt32 (provider.ComputeHash (Encoding.UTF8.GetBytes ("0")), 0));
			Assert.NotEqual (
				MurmurHash3.GetHashCode ('0'),
				MurmurHash3.GetHashCode ('1'));
			Assert.NotEqual (
				MurmurHash3.GetHashCode ('\x0000'),
				MurmurHash3.GetHashCode ('\x0001'));

			// different lengths produce different hashes
			Assert.NotEqual (
				MurmurHash3.GetHashCode (new byte[] { 0 }),
				MurmurHash3.GetHashCode (new byte[] { 0, 0 }));

			// concrete values
			Assert.Equal ((uint)0x1b20e026,
				BitConverter.ToUInt32 (provider.ComputeHash (BitConverter.GetBytes (1717859169)), 0));
			Assert.Equal ((uint)0x14570c6f,
				BitConverter.ToUInt32 (provider.ComputeHash (Encoding.UTF8.GetBytes ("asd")), 0));
			Assert.Equal ((uint)0xa46b5209,
				BitConverter.ToUInt32 (provider.ComputeHash (Encoding.UTF8.GetBytes ("asdfqwer")), 0));
			Assert.Equal ((uint)0xa3cfe04b,
				BitConverter.ToUInt32 (provider.ComputeHash (Encoding.UTF8.GetBytes ("asdfqwerty")), 0));
		}

		[Fact, Trait ("Category", "Cryptography.MurmurHash3")]
		public void CollisionsForSequentialNumbers ()
		{
			// collisions for large set of sequential numbers
			var rnd = new Random ();
			int startingNumber = rnd.Next (int.MinValue, int.MaxValue - 1000000);
			var hashes = Range (startingNumber, 1000000)
				.Select (item => MurmurHash3.GetHashCode (item))
				.ToArray ();
			var uniques = hashes.GroupBy (item => item);
			Assert.Equal (hashes.Length, uniques.Count ());
		}

		[Fact, Trait ("Category", "Cryptography.MurmurHash3")]
		public void DistributionForSmallSet ()
		{
			// distribution for small set of values
			var rnd = new Random ();
			for (int i = 0; i < 1000; i++)
			{
				var hashes = Range (5000000, 20)
					.Select (item => MurmurHash3.GetHashCode (rnd.NextDouble ()))
					.ToArray ();
				Array.Sort (hashes);
				var hashes2 = Repeat (uint.MinValue, 1).Concat (hashes).Concat (Repeat (uint.MaxValue, 1)).ToArray ();
				var distances = new uint[hashes2.Length - 1];
				for (int j = 1; j < hashes2.Length; j++)
				{
					distances[j-1] = hashes2[j] - hashes2[j - 1];
				}
				var min = distances.Min ();
				var max = distances.Max ();
				Assert.InRange (min, 21U, uint.MaxValue); // может иногда нарушаться, повторите тест
				Assert.InRange (max, 0U, (uint.MaxValue / 2U)); // может иногда нарушаться, повторите тест
			}
		}

		[Fact, Trait ("Category", "Cryptography.MurmurHash3")]
		public void CollisionsForRandomChunks ()
		{
			// collisions for random chunks
			var rnd = new Random ();
			byte[] buf;
			var hashes = Range (0, 100000)
				.Select (item => { buf = new byte[rnd.Next (4, 50)]; rnd.NextBytes (buf); return MurmurHash3.GetHashCode (buf); })
				.ToArray ();
			var uniques = hashes.GroupBy (item => item);
			var collisions = hashes.Length - uniques.Count ();
			var maxCollision = uniques.Select ((key, value) => key.Count ()).Max ();
			Assert.InRange (collisions, 0, 4); // может иногда нарушаться, повторите тест
			Assert.InRange (maxCollision, 0, 2); // может иногда нарушаться, повторите тест
		}
	}
}
