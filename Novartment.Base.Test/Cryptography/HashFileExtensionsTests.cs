using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Novartment.Base.IO;
using Xunit;

namespace Novartment.Base.Test
{
	public class HashFileExtensionsTests
	{
		private static readonly string _textTemplate = "I feel lucky to live in the days of continuously connected devices.\r\n";
		private static readonly byte[] _md5OfTextTemplate = new byte[]
		{
			0xdf, 0xa3, 0x6d, 0x8e, 0xd1, 0x32, 0x21, 0x64, 0x58, 0x68, 0xf1, 0x4e, 0x40, 0xed, 0x1a, 0xdb,
		};

		[Fact]
		[Trait ("Category", "HashFileExtensions")]
		public void HashFileAsync ()
		{
			var fn1 = Path.Combine (Path.GetTempPath (), "ttt1.txt");
			var fi1 = new FileInfo (fn1);
			var buf = Encoding.ASCII.GetBytes (_textTemplate);
			using (var writer = fi1.OpenWrite ())
			{
				writer.Write (buf, 0, buf.Length);
			}

			using (var hashProvider = IncrementalHash.CreateHash (HashAlgorithmName.MD5))
			{
				var hash = IncrementalHashExtensions.HashFileAsync (hashProvider, fn1, null).Result;
				Assert.Equal (_md5OfTextTemplate, hash);
			}

			File.Delete (fn1);
		}

		[Fact]
		[Trait ("Category", "HashFileExtensions")]
		public void CopyFileWithHashingAsync ()
		{
			var fn1 = Path.Combine (Path.GetTempPath (), "ttt1.txt");
			var fn2 = Path.Combine (Path.GetTempPath (), "ttt2.txt");

			var fi1 = new FileInfo (fn1);
			var buf = Encoding.ASCII.GetBytes (_textTemplate);
			using (var writer = fi1.OpenWrite ())
			{
				writer.Write (buf, 0, buf.Length);
			}

			using (var hashProvider = IncrementalHash.CreateHash (HashAlgorithmName.MD5))
			{
				var hash = IncrementalHashExtensions.CopyFileWithHashingAsync (hashProvider, fn1, fn2, null).Result;
				Assert.Equal (_md5OfTextTemplate, hash);
			}

			var fi2 = new FileInfo (fn2);
			using (var reader = fi2.OpenText ())
			{
				Assert.Equal (_textTemplate, reader.ReadToEnd ());
			}

			File.Delete (fn1);
			File.Delete (fn2);
		}
	}
}
