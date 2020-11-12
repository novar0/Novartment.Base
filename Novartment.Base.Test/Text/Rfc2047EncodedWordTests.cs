using System;
using System.Text;
using Novartment.Base.Text;
using Xunit;

namespace Novartment.Base.Test
{
	public sealed class Rfc2047EncodedWordTests
	{
		public Rfc2047EncodedWordTests ()
		{
			Encoding.RegisterProvider (CodePagesEncodingProvider.Instance);
		}

		[Theory]
		[Trait ("Category", "Text.Rfc2047EncodedWord")]
		[InlineData ("=?koi8-r?B?68/O09TBztTJziD0xczJ3svP?=", "Константин Теличко")] // b-encoding 8-битно
		[InlineData ("=?utf-8*ru-ru?B?0YLQtdC80LAg0YHQvtC+0LHRidC10L3QuNGPINGC0LXQutGB0YIg0YHQvtC+0LHRidC10L3QuNGP?=", "тема сообщения текст сообщения")] // b-encoding длина кратно 3
		[InlineData ("=?utf-8?B?INGD0YHQuNC70LXQvdC90YvRhQ==?=", " усиленных")] // b-encoding длина кратно 3 + 1
		[InlineData ("=?utf-8?B?INC80LXRgNCw0YU=?=", " мерах")] // b-encoding длина кратно 3 + 2
		[InlineData ("=?us-ascii?q?some_text?=", "some text")] // q-encoding ascii
		[InlineData ("=?us-ascii*ru-ru?q?some_text?=", "some text")] // q-encoding ascii ru
		[InlineData ("=?windows-1251*ru-ru?Q?=C8=E4=E5=FF_=F1=EE=F1=F2=EE=E8=F2_=E2_=F2=EE=EC?=", "Идея состоит в том")] // q-encoding windows-1251
		[Trait ("Category", "Text.Rfc2047EncodedWord")]
		public void Parse (string encoded, string decoded)
		{
			Assert.Equal (decoded, Rfc2047EncodedWord.Parse (encoded));

			var buf = new char[1024];
			var len = Rfc2047EncodedWord.Parse (encoded.AsSpan (), buf.AsSpan ());
			Assert.Equal (decoded, new string (buf, 0, len));
		}

		[Theory]
		[Trait ("Category", "Text.Rfc2047EncodedWord")]
		[InlineData ("=?a?B??")]
		[InlineData ("=?uk-ascii?q?text?=")]
		[InlineData ("=?us-ascii?t?text?=")]
		[InlineData ("=?us-ascii?q?text?")]
		[InlineData ("=?utf-8?B?INC80LXRgN=w0YU=?=")]
		[InlineData ("=?utf-8?B?INC80LXRgNCw0Y=?=")]
		[InlineData ("=?utf-8?B?INC8.LXRgNCw0YU=?=")]
		[InlineData ("=?utf-8?B?INC8:LXRgNCw0YU=?=")]
		[InlineData ("=?utf-8?B?INC8@LXRgNCw0YU=?=")]
		[InlineData ("=?utf-8?B?INC8[LXRgNCw0YU=?=")]
		[InlineData ("=?utf-8?B?INC8`LXRgNCw0YU=?=")]
		[InlineData ("=?utf-8?B?INC8|LXRgNCw0YU=?=")]
		[InlineData ("=?utf-8*ru-ru?B?0YLQtdC80LAg0YHQvtC+0LHRidC10L3QuNGPINGC0LXQutGB0YIg0YHQvtC+0LHRidC10L3QuNG?=")]
		public void ParseException (string encoded)
		{
			Assert.Throws<FormatException> (() => Rfc2047EncodedWord.Parse (encoded));

			var buf = new char[1024];
			Assert.Throws<FormatException> (() => Rfc2047EncodedWord.Parse (encoded.AsSpan (), buf.AsSpan ()));
		}
	}
}
