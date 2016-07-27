using System.IO;
using System.Text;
using System.Security.Cryptography;
using System.Threading;
using Novartment.Base.IO;
using Xunit;

namespace Novartment.Base.Net45.Test
{

	public class CryptoTransformFileExtensionsTests
	{
		private static readonly string _textTemplate = "I feel lucky to live in the days of continuously connected devices.\r\n";
		private static readonly string _base64Template = "SSBmZWVsIGx1Y2t5IHRvIGxpdmUgaW4gdGhlIGRheXMgb2YgY29udGludW91c2x5IGNvbm5lY3RlZCBkZXZpY2VzLg0K";
		private static readonly byte[] _md5template = new byte[] { 0xdf,0xa3,0x6d,0x8e,0xd1,0x32,0x21,0x64,0x58,0x68,0xf1,0x4e,0x40,0xed,0x1a,0xdb };

		[Fact, Trait ("Category", "CryptoTransformFileExtensions")]
		public void HashFileAsync ()
		{
			var fn1 = Path.Combine (Path.GetTempPath (), "ttt1.txt");
			var fi1 = new FileInfo (fn1);
			var buf = Encoding.ASCII.GetBytes (_textTemplate);
			using (var writer = fi1.OpenWrite ())
			{
				writer.Write (buf, 0, buf.Length);
			}
			using (var hashProvider = MD5.Create ())
			{
				CryptoTransformFileExtensions.HashFileAsync (hashProvider, fn1, null, CancellationToken.None).Wait ();
				Assert.Equal (_md5template, hashProvider.Hash);
			}
			File.Delete (fn1);
		}

		[Fact, Trait ("Category", "CryptoTransformExtensions")]
		public void TransformFileAsync ()
		{
			var fn1 = Path.Combine (Path.GetTempPath (), "ttt1.txt");
			var fn2 = Path.Combine (Path.GetTempPath (), "ttt2.txt");

			var fi1 = new FileInfo (fn1);
			var buf = Encoding.ASCII.GetBytes (_textTemplate);
			using (var writer = fi1.OpenWrite ())
			{
				writer.Write (buf, 0, buf.Length);
			}
			CryptoTransformFileExtensions.TransformFileAsync (new ToBase64Transform (), fn1, fn2, null, CancellationToken.None).Wait ();

			var fi2 = new FileInfo (fn2);
			using (var reader = fi2.OpenText ())
			{
				Assert.Equal (_base64Template, reader.ReadToEnd ());
			}
			File.Delete (fn1);
			File.Delete (fn2);
		}
	}
}
