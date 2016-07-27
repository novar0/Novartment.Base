using System;
using System.Text;
using Novartment.Base.Text;
using Xunit;

namespace Novartment.Base.Test
{
	public class Rfc2047EncodedWordTests
	{
		public Rfc2047EncodedWordTests ()
		{
			Encoding.RegisterProvider (CodePagesEncodingProvider.Instance);
		}

		[Fact, Trait ("Category", "Text.Rfc2047EncodedWord")]
		public void Parse_QEncoding ()
		{
			Assert.Equal (
				"some text",
				Rfc2047EncodedWord.Parse ("=?us-ascii?q?some_text?="));
			Assert.Equal (
				"some text",
				Rfc2047EncodedWord.Parse ("=?us-ascii*ru-ru?q?some_text?="));
			Assert.Equal (
				"Идея состоит в том",
				Rfc2047EncodedWord.Parse ("=?windows-1251*ru-ru?Q?=C8=E4=E5=FF_=F1=EE=F1=F2=EE=E8=F2_=E2_=F2=EE=EC?="));
		}

		[Fact, Trait ("Category", "Text.Rfc2047EncodedWord")]
		public void Parse_BEncoding ()
		{
			Assert.Equal (
				"Константин Теличко",
				Rfc2047EncodedWord.Parse ("=?koi8-r?B?68/O09TBztTJziD0xczJ3svP?=")); // 8-битно
			Assert.Equal (
				"тема сообщения текст сообщения",
				Rfc2047EncodedWord.Parse ("=?utf-8*ru-ru?B?0YLQtdC80LAg0YHQvtC+0LHRidC10L3QuNGPINGC0LXQutGB0YIg0YHQvtC+0LHRidC10L3QuNGP?=")); // длина кратно 3
			Assert.Equal (
				" усиленных",
				Rfc2047EncodedWord.Parse ("=?utf-8?B?INGD0YHQuNC70LXQvdC90YvRhQ==?=")); // длина кратно 3 + 1
			Assert.Equal (
				" мерах",
				Rfc2047EncodedWord.Parse ("=?utf-8?B?INC80LXRgNCw0YU=?=")); // длина кратно 3 + 2
		}

		[Fact, Trait ("Category", "Text.Rfc2047EncodedWord")]
		public void ParseException ()
		{
			Assert.Throws<FormatException> (() => Rfc2047EncodedWord.Parse ("=?a?B??"));
			Assert.Throws<FormatException> (() => Rfc2047EncodedWord.Parse ("=?uk-ascii?q?text?="));
			Assert.Throws<FormatException> (() => Rfc2047EncodedWord.Parse ("=?us-ascii?t?text?="));
			Assert.Throws<FormatException> (() => Rfc2047EncodedWord.Parse ("=?us-ascii?q?text?"));
			Assert.Throws<FormatException> (() => Rfc2047EncodedWord.Parse ("=?utf-8?B?INC80LXRgN=w0YU=?="));
			Assert.Throws<FormatException> (() => Rfc2047EncodedWord.Parse ("=?utf-8?B?INC80LXRgNCw0Y=?="));
			Assert.Throws<FormatException> (() => Rfc2047EncodedWord.Parse ("=?utf-8?B?INC8.LXRgNCw0YU=?="));
			Assert.Throws<FormatException> (() => Rfc2047EncodedWord.Parse ("=?utf-8?B?INC8:LXRgNCw0YU=?="));
			Assert.Throws<FormatException> (() => Rfc2047EncodedWord.Parse ("=?utf-8?B?INC8@LXRgNCw0YU=?="));
			Assert.Throws<FormatException> (() => Rfc2047EncodedWord.Parse ("=?utf-8?B?INC8[LXRgNCw0YU=?="));
			Assert.Throws<FormatException> (() => Rfc2047EncodedWord.Parse ("=?utf-8?B?INC8`LXRgNCw0YU=?="));
			Assert.Throws<FormatException> (() => Rfc2047EncodedWord.Parse ("=?utf-8?B?INC8|LXRgNCw0YU=?="));
			Assert.Throws<FormatException> (() => Rfc2047EncodedWord.Parse ("=?utf-8*ru-ru?B?0YLQtdC80LAg0YHQvtC+0LHRidC10L3QuNGPINGC0LXQutGB0YIg0YHQvtC+0LHRidC10L3QuNG?="));
		}

		[Fact, Trait ("Category", "Text.Rfc2047EncodedWord")]
		public void IsValid ()
		{
			// legal
			Assert.True (Rfc2047EncodedWord.IsValid ("=?utf-8?q?some_text?="));
			Assert.True (Rfc2047EncodedWord.IsValid ("=?utf-8*ru-ru?B?0YLQtdC80LAg0YHQvtC+0LHRidC10L3QuNGPINGC0LXQutGB0YIg0YHQvtC+0LHRidC10L3QuNGP?="));
			Assert.True (Rfc2047EncodedWord.IsValid ("=?windows-1251?Q?new_=F1=EE=E2=F1=E5=EC_one_222?="));
			Assert.True (Rfc2047EncodedWord.IsValid ("=?us-ascii?new-method?%20%30%40?="));
			// illegal
			Assert.False (Rfc2047EncodedWord.IsValid ("sdfs"));
			Assert.False (Rfc2047EncodedWord.IsValid ("=?sdfs"));
			Assert.False (Rfc2047EncodedWord.IsValid ("=?"));
			Assert.False (Rfc2047EncodedWord.IsValid ("=?utf-8?"));
			Assert.False (Rfc2047EncodedWord.IsValid ("=?utf-8?q?"));
			Assert.False (Rfc2047EncodedWord.IsValid ("=?utf-8?q?some_text?"));
			Assert.False (Rfc2047EncodedWord.IsValid ("=?utf-8?q?абвгд?="));
			Assert.False (Rfc2047EncodedWord.IsValid (" =?utf-8?q?some_text?="));
			Assert.False (Rfc2047EncodedWord.IsValid ("=?utf-8?q?some_text?= "));
			Assert.False (Rfc2047EncodedWord.IsValid ("=?utf-8?q?some_text ?="));
			//Assert.False (Rfc2047EncodedWord.IsValid ("=?utf-8?q?some?text?="));
			//Assert.False (Rfc2047EncodedWord.IsValid ("=?utf-8?some_text?="));
			//Assert.False (Rfc2047EncodedWord.IsValid ("=?utf/8?q?some_text?="));
		}
	}
}
