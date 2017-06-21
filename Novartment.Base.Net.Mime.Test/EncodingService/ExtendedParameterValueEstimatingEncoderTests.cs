using System.Text;
using Xunit;

namespace Novartment.Base.Net.Mime.Test
{
	public class ExtendedParameterValueEstimatingEncoderTests
	{
		private static readonly string Template1 = "token#numer_one2001";
		private static readonly string Template2 = " two\tthree";
		private static readonly string Template3 = " \\^ between SLASHES ^\\ \"quoted\"";
		private static readonly string Template4 = "кириллица";
		private static readonly string TemplateResultEncodedSection0Prolog = "utf-8''";
		private static readonly string TemplateResultEncoded = "token#numer_one2001%20two%09three%20%5C^%20between%20SLASHES%20^%5C%20%22quoted%22%D0%BA%D0%B8%D1%80%D0%B8%D0%BB%D0%BB%D0%B8%D1%86%D0%B0";

		[Fact]
		[Trait ("Category", "Mime")]
		public void EstimateEncode ()
		{
			var encoding = Encoding.UTF8;
			var buf = encoding.GetBytes (Template1 + Template2 + Template3 + Template4);
			var encoder = new ExtendedParameterValueEstimatingEncoder (encoding);
			var encodedBuf = new byte[999];

			// полная строка, неподходящие начинаются с середины, первый кусок
			var tuple = encoder.Estimate (buf, 0, buf.Length, encodedBuf.Length, 0, false);
			Assert.Equal (TemplateResultEncodedSection0Prolog.Length + TemplateResultEncoded.Length, tuple.BytesProduced);
			Assert.Equal (buf.Length, tuple.BytesConsumed);
			tuple = encoder.Encode (buf, 0, buf.Length, encodedBuf, 0, encodedBuf.Length, 0, false);
			Assert.Equal (TemplateResultEncodedSection0Prolog + TemplateResultEncoded, Encoding.ASCII.GetString (encodedBuf, 0, tuple.BytesProduced));
			Assert.Equal (buf.Length, tuple.BytesConsumed);

			// полная строка, неподходящие начинаются с середины, последующие куски
			tuple = encoder.Estimate (buf, 0, buf.Length, encodedBuf.Length, 1, false);
			Assert.Equal (TemplateResultEncoded.Length, tuple.BytesProduced);
			Assert.Equal (buf.Length, tuple.BytesConsumed);
			tuple = encoder.Encode (buf, 0, buf.Length, encodedBuf, 0, encodedBuf.Length, 1, false);
			Assert.Equal (TemplateResultEncoded, Encoding.ASCII.GetString (encodedBuf, 0, tuple.BytesProduced));
			Assert.Equal (buf.Length, tuple.BytesConsumed);

			// ограничение по размеру входа
			tuple = encoder.Estimate (buf, 0, 7, encodedBuf.Length, 0, false);
			Assert.Equal (14, tuple.BytesProduced);
			Assert.Equal (7, tuple.BytesConsumed);
			tuple = encoder.Encode (buf, 0, 7, encodedBuf, 0, encodedBuf.Length, 0, false);
			Assert.Equal ((TemplateResultEncodedSection0Prolog + TemplateResultEncoded).Substring (0, 14), Encoding.ASCII.GetString (encodedBuf, 0, tuple.BytesProduced));
			Assert.Equal (7, tuple.BytesConsumed);

			// ограничение по размеру выхода
			tuple = encoder.Estimate (buf, Template1.Length + Template2.Length, buf.Length, 11, 0, false);
			Assert.Equal (10, tuple.BytesProduced);
			Assert.Equal (1, tuple.BytesConsumed);
			tuple = encoder.Encode (buf, Template1.Length + Template2.Length, buf.Length, encodedBuf, 0, 11, 0, false);
			Assert.Equal ("utf-8''%20", Encoding.ASCII.GetString (encodedBuf, 0, tuple.BytesProduced));
			Assert.Equal (1, tuple.BytesConsumed);
		}
	}
}
