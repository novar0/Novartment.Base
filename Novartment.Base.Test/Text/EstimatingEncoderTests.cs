using System;
using System.Text;
using Novartment.Base.Text;
using Xunit;

namespace Novartment.Base.Test
{
	public sealed class EstimatingEncoderTests
	{
		private static readonly string Template1 = "token#numer_one2001";
		private static readonly string Template2 = " two\tthree";
		private static readonly string Template3 = " \\^ between SLASHES ^\\ \"quoted\"";
		private static readonly string Template4 = "кириллица";
		private static readonly string Template10 = "Идея stop2020 состоит в том";
		private static readonly string Template10ResultEncoded = "=?windows-1251?Q?=C8=E4=E5=FF_stop2020_=F1=EE=F1=F2=EE=E8=F2_=E2_=F2=EE=EC?=";
		private static readonly string Template20 = "тема сообщения текст сообщения";
		private static readonly string Template20ResultEncoded = "=?utf-8?B?0YLQtdC80LAg0YHQvtC+0LHRidC10L3QuNGPINGC0LXQutGB0YIg0YHQvtC+0LHRidC10L3QuNGP?=";

		public EstimatingEncoderTests ()
		{
			Encoding.RegisterProvider (CodePagesEncodingProvider.Instance);
		}

		[Fact]
		[Trait ("Category", "Text.EstimatingEncoder")]
		public void QuotedString_EstimateEncode ()
		{
			var encoding = Encoding.UTF8;
			var buf = encoding.GetBytes (Template1 + Template2 + Template3 + Template4);
			var encoder = new QuotedStringEstimatingEncoder ();
			var encodedBuf = new byte[999];

			// неподходящие элементы в начале
			var tuple = encoder.Estimate (
				buf.AsSpan (Template1.Length + Template2.Length + Template3.Length, buf.Length - (Template1.Length + Template2.Length + Template3.Length)),
				99999,
				0,
				false);
			Assert.Equal (0, tuple.BytesProduced);
			Assert.Equal (0, tuple.BytesConsumed);
			tuple = encoder.Encode (
					buf.AsSpan (Template1.Length + Template2.Length + Template3.Length, buf.Length - (Template1.Length + Template2.Length + Template3.Length)),
					encodedBuf,
					0,
					false);
			Assert.Equal (0, tuple.BytesProduced);
			Assert.Equal (0, tuple.BytesConsumed);

			// полная строка, неподходящие начинаются с середины
			var tmpl = "\"" + (Template1 + Template2 + Template3).Replace (@"\", @"\\", StringComparison.Ordinal).Replace ("\"", "\\\"", StringComparison.Ordinal) + "\"";
			tuple = encoder.Estimate (buf, 99999, 0, false);
			Assert.Equal (tmpl.Length, tuple.BytesProduced);
			Assert.Equal (Template1.Length + Template2.Length + Template3.Length, tuple.BytesConsumed);
			tuple = encoder.Encode (buf, encodedBuf, 0, false);
			Assert.Equal (tmpl, Encoding.UTF8.GetString (encodedBuf, 0, tuple.BytesProduced));
			Assert.Equal (Template1.Length + Template2.Length + Template3.Length, tuple.BytesConsumed);

			// ограничение по размеру входа
			tuple = encoder.Estimate (buf.AsSpan (0, 7), 99999, 0, false);
			Assert.Equal (9, tuple.BytesProduced);
			Assert.Equal (7, tuple.BytesConsumed);
			tuple = encoder.Encode (buf.AsSpan (0, 7), encodedBuf, 0, false);
			Assert.Equal ("\"" + Template1.Substring (0, 7) + "\"", Encoding.UTF8.GetString (encodedBuf, 0, tuple.BytesProduced));
			Assert.Equal (7, tuple.BytesConsumed);

			// ограничение по размеру выхода
			tuple = encoder.Estimate (buf.AsSpan (Template1.Length + Template2.Length), 4, 0, false);
			Assert.Equal (3, tuple.BytesProduced);
			Assert.Equal (1, tuple.BytesConsumed);
			tuple = encoder.Encode (buf.AsSpan (Template1.Length + Template2.Length), encodedBuf.AsSpan (0, 4), 0, false);
			Assert.Equal ("\"" + Template3[0] + "\"", Encoding.UTF8.GetString (encodedBuf, 0, tuple.BytesProduced));
			Assert.Equal (1, tuple.BytesConsumed);
		}

		[Fact]
		[Trait ("Category", "Text.EstimatingEncoder")]
		public void EncodedWordQ_EstimateEncode ()
		{
			var encoding = Encoding.GetEncoding ("windows-1251");
			var buf = encoding.GetBytes (Template10);
			var encoder = new EncodedWordQEstimatingEncoder (encoding, AsciiCharClasses.QEncodingAllowedInUnstructured);
			var encodedBuf = new byte[999];

			// полная строка
			var tuple = encoder.Estimate (buf, 99999, 0, false);
			Assert.Equal (Template10ResultEncoded.Length, tuple.BytesProduced);
			Assert.Equal (buf.Length, tuple.BytesConsumed);
			tuple = encoder.Encode (buf, encodedBuf, 0, false);
			Assert.Equal (Template10ResultEncoded, Encoding.UTF8.GetString (encodedBuf, 0, tuple.BytesProduced));
			Assert.Equal (buf.Length, tuple.BytesConsumed);

			// ограничение по размеру входа
			tuple = encoder.Estimate (buf.AsSpan (0, 3), 99999, 0, false);
			Assert.Equal (28, tuple.BytesProduced);
			Assert.Equal (3, tuple.BytesConsumed);
			tuple = encoder.Encode (buf.AsSpan (0, 3), encodedBuf, 0, false);
			Assert.Equal (Template10ResultEncoded.Substring (0, 26) + "?=", Encoding.UTF8.GetString (encodedBuf, 0, tuple.BytesProduced));
			Assert.Equal (3, tuple.BytesConsumed);

			// ограничение по размеру выхода
			tuple = encoder.Estimate (buf, 24, 0, false);
			Assert.Equal (22, tuple.BytesProduced);
			Assert.Equal (1, tuple.BytesConsumed);
			tuple = encoder.Encode (buf, encodedBuf.AsSpan (0, 24), 0, false);
			Assert.Equal (Template10ResultEncoded.Substring (0, 20) + "?=", Encoding.UTF8.GetString (encodedBuf, 0, tuple.BytesProduced));
			Assert.Equal (1, tuple.BytesConsumed);
			tuple = encoder.Estimate (buf, 25, 0, false);
			Assert.Equal (25, tuple.BytesProduced);
			Assert.Equal (2, tuple.BytesConsumed);
			tuple = encoder.Encode (buf, encodedBuf.AsSpan (0, 25), 0, false);
			Assert.Equal (Template10ResultEncoded.Substring (0, 23) + "?=", Encoding.UTF8.GetString (encodedBuf, 0, tuple.BytesProduced));
			Assert.Equal (2, tuple.BytesConsumed);
		}

		[Fact]
		[Trait ("Category", "Text.EstimatingEncoder")]
		public void EncodedWordB_EstimateEncode ()
		{
			var encoding = Encoding.UTF8;
			var buf = encoding.GetBytes (Template20);
			var encoder = new EncodedWordBEstimatingEncoder (encoding);
			var encodedBuf = new byte[999];

			// полная строка
			var tuple = encoder.Estimate (buf, 99999, 0, false);
			Assert.Equal (Template20ResultEncoded.Length, tuple.BytesProduced);
			Assert.Equal (buf.Length, tuple.BytesConsumed);
			tuple = encoder.Encode (buf, encodedBuf, 0, false);
			Assert.Equal (Template20ResultEncoded, Encoding.UTF8.GetString (encodedBuf, 0, tuple.BytesProduced));
			Assert.Equal (buf.Length, tuple.BytesConsumed);

			// ограничение по размеру входа
			tuple = encoder.Estimate (buf.AsSpan (0, 3), 99999, 0, false);
			Assert.Equal (16, tuple.BytesProduced);
			Assert.Equal (3, tuple.BytesConsumed);
			tuple = encoder.Encode (buf.AsSpan (0, 3), encodedBuf, 0, false);
			Assert.Equal (Template20ResultEncoded.Substring (0, 14) + "?=", Encoding.UTF8.GetString (encodedBuf, 0, tuple.BytesProduced));
			Assert.Equal (3, tuple.BytesConsumed);

			// ограничение по размеру выхода
			tuple = encoder.Estimate (buf, 19, 0, false);
			Assert.Equal (16, tuple.BytesProduced);
			Assert.Equal (3, tuple.BytesConsumed);
			tuple = encoder.Encode (buf, encodedBuf.AsSpan (0, 19), 0, false);
			Assert.Equal (Template20ResultEncoded.Substring (0, 14) + "?=", Encoding.UTF8.GetString (encodedBuf, 0, tuple.BytesProduced));
			Assert.Equal (3, tuple.BytesConsumed);
			tuple = encoder.Estimate (buf, 20, 0, false);
			Assert.Equal (20, tuple.BytesProduced);
			Assert.Equal (6, tuple.BytesConsumed);
			tuple = encoder.Encode (buf, encodedBuf.AsSpan (0, 20), 0, false);
			Assert.Equal (Template20ResultEncoded.Substring (0, 18) + "?=", Encoding.UTF8.GetString (encodedBuf, 0, tuple.BytesProduced));
			Assert.Equal (6, tuple.BytesConsumed);
		}
	}
}
