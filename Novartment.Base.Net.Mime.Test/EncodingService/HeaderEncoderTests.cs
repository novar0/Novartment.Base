using System;
using System.Text;
using Novartment.Base.Collections;
using Xunit;

namespace Novartment.Base.Net.Mime.Test
{
	internal class ArrayListMock : System.Collections.Generic.List<string>,
		IAdjustableCollection<string>
	{
	}

	public class HeaderEncoderTests
	{
		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void EncodeUnstructured ()
		{
			// более подробные тесты в HeaderValueEncoderTests
			var values = new ArrayListMock ();
			HeaderEncoder.EncodeUnstructured (values, string.Empty);
			Assert.Equal (0, values.Count);

			// разбиение на части ограниченные макс.разрешенной длиной
			var template = "An 'encoded-word' may, appear: in a message; values or \"body part\" values according слово to the снова rules valuerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerules again";
			values = new ArrayListMock ();
			HeaderEncoder.EncodeUnstructured (values, template);
			Assert.Equal (17, values.Count);
			Assert.Equal ("An", values[0]);
			Assert.Equal (" 'encoded-word'", values[1]);
			Assert.Equal (" may,", values[2]);
			Assert.Equal (" appear:", values[3]);
			Assert.Equal (" in", values[4]);
			Assert.Equal (" a", values[5]);
			Assert.Equal (" message;", values[6]);
			Assert.Equal (" values", values[7]);
			Assert.Equal (" or", values[8]);
			Assert.Equal (" \"body", values[9]);
			Assert.Equal (" part\"", values[10]);
			Assert.Equal (" values", values[11]);
			Assert.Equal (" according", values[12]);
			Assert.Equal (" =?utf-8?B?0YHQu9C+0LLQviB0byB0aGUg0YHQvdC+0LLQsA==?=", values[13]);
			Assert.Equal (" rules", values[14]);
			Assert.Equal (" valuerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerules", values[15]);
			Assert.Equal (" again", values[16]);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void EncodePhrase ()
		{
			// более подробные тесты в HeaderValueEncoderTests

			// разбиение на части ограниченные макс.разрешенной длиной
			var values = new ArrayListMock ();
			HeaderEncoder.EncodePhrase (values, "An 'encoded-word' may, appear: in a message; values or \"body part\" values according слово to the снова rules valuerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerules again");
			Assert.Equal (9, values.Count);
			Assert.Equal ("An", values[0]);
			Assert.Equal (" 'encoded-word'", values[1]);
			Assert.Equal (" \"may, appear: in a message; values or \\\"body part\\\"\"", values[2]);
			Assert.Equal (" values", values[3]);
			Assert.Equal (" according", values[4]);
			Assert.Equal (" =?utf-8?B?0YHQu9C+0LLQviB0byB0aGUg0YHQvdC+0LLQsA==?=", values[5]);
			Assert.Equal (" rules", values[6]);
			Assert.Equal (" valuerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerules", values[7]);
			Assert.Equal (" again", values[8]);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void EncodeMailbox ()
		{
			var values = new ArrayListMock ();
			HeaderEncoder.EncodeMailbox (values, new Mailbox (new AddrSpec ("someone", "server.com"), null));
			Assert.Equal (1, values.Count);
			Assert.Equal ("<someone@server.com>", values[0]);

			values = new ArrayListMock ();
			HeaderEncoder.EncodeMailbox (values, new Mailbox (new AddrSpec ("someone", "server.com"), "Dear"));
			Assert.Equal (2, values.Count);
			Assert.Equal ("Dear", values[0]);
			Assert.Equal ("<someone@server.com>", values[1]);

			values = new ArrayListMock ();
			HeaderEncoder.EncodeMailbox (values, new Mailbox (
				new AddrSpec ("really-long-address(for.one.line)", "some literal domain"),
				"Henry Abdula Rabi Ж  (the Third King of the Mooon)"));
			Assert.Equal (6, values.Count);
			Assert.Equal ("Henry", values[0]);
			Assert.Equal (" Abdula", values[1]);
			Assert.Equal (" Rabi", values[2]);
			Assert.Equal (" =?utf-8?B?0JY=?=", values[3]);
			Assert.Equal (" \" (the Third King of the Mooon)\"", values[4]);
			Assert.Equal ("<\"really-long-address(for.one.line)\"@[some literal domain]>", values[5]);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void SaveHeader ()
		{
			var src = Array.Empty<HeaderFieldBuilder> ();
			var bytes = new BinaryDestinationMock (8192);
			HeaderEncoder.SaveHeaderAsync (src, bytes).Wait ();
			Assert.Equal (0, bytes.Count);

			src = new HeaderFieldBuilder[]
			{
				HeaderFieldBuilder.CreateExactValue (HeaderFieldName.ContentType, "text/plain"),
				HeaderFieldBuilder.CreateExactValue (HeaderFieldName.ConversionWithLoss, null),
				HeaderFieldBuilder.CreateUnstructured (HeaderFieldName.Received, "by server10.espc2.mechel.com (8.8.8/1.37) id CAA22933; Tue, 15 May 2012 02:49:22 +0100"),
				HeaderFieldBuilder.CreateExactValue (HeaderFieldName.ContentMD5, ":Q2hlY2sgSW50ZWdyaXR5IQ=="),
			};
			src[0].AddParameter ("format", "flowed");
			src[0].AddParameter ("charset", "koi8-r");
			src[0].AddParameter ("reply-type", "original");

			bytes = new BinaryDestinationMock (8192);
			HeaderEncoder.SaveHeaderAsync (src, bytes).Wait ();

			var template = "Content-Type: text/plain; format=flowed; charset=koi8-r; reply-type=original\r\n" +
				"Conversion-With-Loss:\r\n" +
				"Received: by server10.espc2.mechel.com (8.8.8/1.37) id CAA22933; Tue, 15 May\r\n 2012 02:49:22 +0100\r\n" +
				"Content-MD5: :Q2hlY2sgSW50ZWdyaXR5IQ==\r\n";
			Assert.Equal (template.Length, bytes.Count);
			Assert.Equal (template, Encoding.ASCII.GetString (bytes.Buffer.Slice (0, bytes.Count)));
		}
	}
}
