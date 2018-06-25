using System;
using System.Text;
using System.Threading;
using Xunit;
using Novartment.Base.Collections;

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
			var values = new ArrayListMock ();
			HeaderEncoder.EncodeUnstructured (values, string.Empty);
			Assert.Equal (0, values.Count);

			values = new ArrayListMock ();
			HeaderEncoder.EncodeUnstructured (values, "value");
			Assert.Equal (1, values.Count);
			Assert.Equal ("value", values[0]);
			values = new ArrayListMock ();
			HeaderEncoder.EncodeUnstructured (values, "value1  (value2) \t value3");
			Assert.Equal (3, values.Count);
			Assert.Equal ("value1", values[0]);
			Assert.Equal ("  (value2)", values[1]);
			Assert.Equal (" \t value3", values[2]);

			// похоже на комментарии и на строки в кавычках (не должны распознаваться)
			values = new ArrayListMock ();
			HeaderEncoder.EncodeUnstructured (values, "\t (a 1)  \"[no\\\\ne]\" bb\tccc  ");
			Assert.Equal (5, values.Count);
			Assert.Equal ("\t (a", values[0]);
			Assert.Equal (" 1)", values[1]);
			Assert.Equal ("  \"[no\\\\ne]\"", values[2]);
			Assert.Equal (" bb", values[3]);
			Assert.Equal ("\tccc  ", values[4]);

			// похоже на кодированные слова (должны быть закодированы)
			var template = "aaa   =?utf-8?B?0JY=?= bbb";
			values = new ArrayListMock ();
			HeaderEncoder.EncodeUnstructured (values, template);
			Assert.Equal (3, values.Count);
			Assert.Equal ("aaa", values[0]);
			Assert.Equal ("   =?utf-8?B?PT91dGYtOD9CPzBKWT0/PQ==?=", values[1]);
			Assert.Equal (" bbb", values[2]);
			template = " =?utf-64?q?one?= \t=?utf-8?B?0JY=?= ";
			values = new ArrayListMock ();
			HeaderEncoder.EncodeUnstructured (values, template);
			Assert.Equal (1, values.Count);
			Assert.Equal (" =?utf-8?B?PT91dGYtNjQ/cT9vbmU/PSAJPT91dGYtOD9CPzBKWT0/PSA=?=", values[0]);

			// не-ASCII символы (должны быть закодированы)
			template = "1:  §2 \t ДВА   ©1999...2001";
			values = new ArrayListMock ();
			HeaderEncoder.EncodeUnstructured (values, template);
			Assert.Equal (2, values.Count);
			Assert.Equal ("1:", values[0]);
			Assert.Equal ("  =?utf-8?B?wqcyIAkg0JTQktCQICAgwqkxOTk5Li4uMjAwMQ==?=", values[1]);

			// разбиение на части ограниченные макс.разрешенной длиной
			template = "An 'encoded-word' may, appear: in a message; values or \"body part\" values according слово to the снова rules valuerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerules again";
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
			// пробельные символы в начале, конце и середине
			var values = new ArrayListMock ();
			HeaderEncoder.EncodePhrase (values, "abc");
			Assert.Equal (1, values.Count);
			Assert.Equal ("abc", values[0]);
			values = new ArrayListMock ();
			HeaderEncoder.EncodePhrase (values, "abc def");
			Assert.Equal (2, values.Count);
			Assert.Equal ("abc", values[0]);
			Assert.Equal (" def", values[1]);
			values = new ArrayListMock ();
			HeaderEncoder.EncodePhrase (values, "abc\tdef");
			Assert.Equal (1, values.Count);
			Assert.Equal ("\"abc\tdef\"", values[0]);
			values = new ArrayListMock ();
			HeaderEncoder.EncodePhrase (values, " abc def");
			Assert.Equal (2, values.Count);
			Assert.Equal ("\" abc\"", values[0]);
			Assert.Equal (" def", values[1]);
			values = new ArrayListMock ();
			HeaderEncoder.EncodePhrase (values, "abc def ");
			Assert.Equal (2, values.Count);
			Assert.Equal ("abc", values[0]);
			Assert.Equal (" \"def \"", values[1]);
			values = new ArrayListMock ();
			HeaderEncoder.EncodePhrase (values, "abc  def");
			Assert.Equal (2, values.Count);
			Assert.Equal ("abc", values[0]);
			Assert.Equal (" \" def\"", values[1]);
			values = new ArrayListMock ();
			HeaderEncoder.EncodePhrase (values, " abc def ");
			Assert.Equal (1, values.Count);
			Assert.Equal ("\" abc def \"", values[0]);

			// максимум что можно представить в виде atom
			values = new ArrayListMock ();
			HeaderEncoder.EncodePhrase (values, "!#$%&'*+-/=?^_`{|}~abyzABYZ0189");
			Assert.Equal (1, values.Count);
			Assert.Equal ("!#$%&'*+-/=?^_`{|}~abyzABYZ0189", values[0]);

			// максимум что можно представить в виде quotable
			values = new ArrayListMock ();
			HeaderEncoder.EncodePhrase (values, "!\"#$%&'()*+,-./0123456789:;<=>?@ABYZ[\\]^_`abyz{|}~");
			Assert.Equal (1, values.Count);
			Assert.Equal ("\"!\\\"#$%&'()*+,-./0123456789:;<=>?@ABYZ[\\\\]^_`abyz{|}~\"", values[0]);

			// требуется кодирование
			values = new ArrayListMock ();
			HeaderEncoder.EncodePhrase (values, "ABYZ©9810abyz!*+-/");
			Assert.Equal (1, values.Count);
			Assert.Equal ("=?utf-8?B?QUJZWsKpOTgxMGFieXohKistLw==?=", values[0]);
			values = new ArrayListMock ();
			HeaderEncoder.EncodePhrase (values, "«§2©2123}»");
			Assert.Equal (1, values.Count);
			Assert.Equal ("=?utf-8?B?wqvCpzLCqTIxMjN9wrs=?=", values[0]);

			// похоже на кодированное слово (отдельно)
			values = new ArrayListMock ();
			HeaderEncoder.EncodePhrase (values, "=?utf-8?B?0JY=?=");
			Assert.Equal (1, values.Count);
			Assert.Equal ("=?utf-8?B?PT91dGYtOD9CPzBKWT0/PQ==?=", values[0]);

			// похоже на кодированное слово (среди непохожих)
			values = new ArrayListMock ();
			HeaderEncoder.EncodePhrase (values, "abc =?utf-8?B?0JY=?= def");
			Assert.Equal (3, values.Count);
			Assert.Equal ("abc", values[0]);
			Assert.Equal (" =?utf-8?B?PT91dGYtOD9CPzBKWT0/PQ==?=", values[1]);
			Assert.Equal (" def", values[2]);

			// похоже на кодированное слово (выделено табами)
			values = new ArrayListMock ();
			HeaderEncoder.EncodePhrase (values, "abc\t=?utf-8?B?0JY=?=\tdef");
			Assert.Equal (1, values.Count);
			Assert.Equal ("=?utf-8?B?YWJjCT0/dXRmLTg/Qj8wSlk9Pz0JZGVm?=", values[0]);

			// не похоже на кодированное слово (не выделено пробельными символами)
			values = new ArrayListMock ();
			HeaderEncoder.EncodePhrase (values, "aaa_=?utf-8?B?0JY=?=_bbb");
			Assert.Equal (1, values.Count);
			Assert.Equal ("aaa_=?utf-8?B?0JY=?=_bbb", values[0]);

			// объединение нескольких слов
			// a a - объединять не нужно, оставляем свободу комбинирования
			values = new ArrayListMock ();
			HeaderEncoder.EncodePhrase (values, "abyz!#$%&'*+- ABYZ0189/=?^_`{|}~");
			Assert.Equal (2, values.Count);
			Assert.Equal ("abyz!#$%&'*+-", values[0]);
			Assert.Equal (" ABYZ0189/=?^_`{|}~", values[1]);

			// q q - объединять можно чтобы съэкономить две кавычки. нужно ли?
			values = new ArrayListMock ();
			HeaderEncoder.EncodePhrase (values, "@ABYZ 0123456789;");
			Assert.Equal (1, values.Count);
			Assert.Equal ("\"@ABYZ 0123456789;\"", values[0]);

			// e e - объединять нужно чтобы прилично съэкономить на обрамлении encoded-word
			values = new ArrayListMock ();
			HeaderEncoder.EncodePhrase (values, "ABYZ©9810 abyz§!*+-/");
			Assert.Equal (1, values.Count);
			Assert.Equal ("=?utf-8?B?QUJZWsKpOTgxMCBhYnl6wqchKistLw==?=", values[0]);

			// q a q - объединять можно чтобы съэкономить две кавычки. нужно ли?
			values = new ArrayListMock ();
			HeaderEncoder.EncodePhrase (values, "@ABYZ value 0123456789;");
			Assert.Equal (1, values.Count);
			Assert.Equal ("\"@ABYZ value 0123456789;\"", values[0]);

			// q e q - объединять невозможно
			values = new ArrayListMock ();
			HeaderEncoder.EncodePhrase (values, "@ABYZ §2000 0123456789;");
			Assert.Equal (3, values.Count);
			Assert.Equal ("\"@ABYZ\"", values[0]);
			Assert.Equal (" =?utf-8?B?wqcyMDAw?=", values[1]);
			Assert.Equal (" \"0123456789;\"", values[2]);

			// e a e - объединять нужно чтобы прилично съэкономить на обрамлении encoded-word
			values = new ArrayListMock ();
			HeaderEncoder.EncodePhrase (values, "ABYZ©9810 value abyz§!*+-/");
			Assert.Equal (1, values.Count);
			Assert.Equal ("=?utf-8?B?QUJZWsKpOTgxMCB2YWx1ZSBhYnl6wqchKistLw==?=", values[0]);

			// e q e - объединять можно чтобы съэкономить на обрамлении encoded-word, но зависит от размера q посередине. нужно ли?
			values = new ArrayListMock ();
			HeaderEncoder.EncodePhrase (values, "ABYZ©9810 @ABYZ abyz§!*+-/");
			Assert.Equal (1, values.Count);
			Assert.Equal ("=?utf-8?B?QUJZWsKpOTgxMCBAQUJZWiBhYnl6wqchKistLw==?=", values[0]);

			// разбиение на части ограниченные макс.разрешенной длиной
			values = new ArrayListMock ();
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
