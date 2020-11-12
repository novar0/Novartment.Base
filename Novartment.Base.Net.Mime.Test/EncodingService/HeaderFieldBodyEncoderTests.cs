using System.Text;
using Xunit;

namespace Novartment.Base.Net.Mime.Test
{
	public sealed class HeaderFieldBodyEncoderTests
	{
		[Fact]
		[Trait ("Category", "Mime")]
		public void EncodeNextElementUnstructured ()
		{
			var buf = new byte[1000];
			var src = Encoding.UTF8.GetBytes (string.Empty);
			var position = 0;
			var prevSequenceIsWordEncoded = false;
			var size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);

			src = Encoding.UTF8.GetBytes ("a");
			position = 0;
			prevSequenceIsWordEncoded = false;
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("a", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);

			src = Encoding.UTF8.GetBytes (" a  ");
			position = 0;
			prevSequenceIsWordEncoded = false;
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" a  ", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);

			src = Encoding.UTF8.GetBytes ("a \x3");
			position = 0;
			prevSequenceIsWordEncoded = false;
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("a", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" =?utf-8?B?Aw==?=", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);

			// похоже на комментарии и на строки в кавычках (не должны распознаваться)
			src = Encoding.UTF8.GetBytes ("\t (a 1)  \"[no\\\\ne]\" bb\tccc  ");
			position = 0;
			prevSequenceIsWordEncoded = false;
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("\t (a", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" 1)", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("  \"[no\\\\ne]\"", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" bb", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("\tccc  ", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);

			// похоже на кодированные слова (должны быть закодированы)
			src = Encoding.UTF8.GetBytes ("aaa   =?utf-8?B?0JY=?= bbb");
			position = 0;
			prevSequenceIsWordEncoded = false;
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("aaa", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("   =?utf-8?B?PT91dGYtOD9CPzBKWT0/PQ==?=", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" bbb", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);

			src = Encoding.UTF8.GetBytes (" =?utf-64?q?one?= \t=?utf-8?B?0JY=?= ");
			position = 0;
			prevSequenceIsWordEncoded = false;
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" =?utf-8?B?PT91dGYtNjQ/cT9vbmU/PSAJPT91dGYtOD9CPzBKWT0/PSA=?=", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);

			// не-ASCII символы (должны быть закодированы)
			src = Encoding.UTF8.GetBytes ("1:  §2 \t ДВА   ©1999...2001");
			position = 0;
			prevSequenceIsWordEncoded = false;
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("1:", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("  =?utf-8?B?wqcyIAkg0JTQktCQICAgwqkxOTk5Li4uMjAwMQ==?=", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);

			// разбиение на части ограниченные макс.разрешенной длиной
			src = Encoding.UTF8.GetBytes ("An 'encoded-word' may, appear: in a message; values or \"body part\" values according слово to the снова rules valuerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerules again");
			position = 0;
			prevSequenceIsWordEncoded = false;
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("An", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" 'encoded-word'", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" may,", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" appear:", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" in", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" a", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" message;", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" values", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" or", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" \"body", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" part\"", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" values", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" according", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" =?utf-8?B?0YHQu9C+0LLQviB0byB0aGUg0YHQvdC+0LLQsA==?=", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" rules", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" valuerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerules", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" again", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);
		}

		[Fact]
		[Trait ("Category", "Mime")]
		public void EncodeNextElementPhrase ()
		{
			var buf = new byte[1000];
			var src = Encoding.UTF8.GetBytes (" \t ab\t\tcde\t \tf \t");
			var position = 0;
			var prevSequenceIsWordEncoded = false;
			var size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("\" \t ab\t\tcde\t \tf \t\"", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);

			var s1 = new string ('a', 20);
			var s2 = new string ('b', 40);
			var s3 = new string ('c', 60);
			var s4 = new string ('d', 80);
			src = Encoding.UTF8.GetBytes ($" {s1}  {s2}   {s3} {s4}");
			position = 0;
			prevSequenceIsWordEncoded = false;
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ($"\" {s1}  {s2}\"", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ($" \"  {s3}\"", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ($" {s4}", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);

			// пробельные символы в начале, конце и середине
			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("abc");
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("abc", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);

			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("abc def");
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("abc", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" def", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);

			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("abc\tdef");
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("\"abc\tdef\"", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);

			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes (" abc def");
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("\" abc\"", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" def", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);

			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("abc def ");
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("abc", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" \"def \"", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);

			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("abc  def");
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("abc", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" \" def\"", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);

			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes (" abc def ");
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("\" abc def \"", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);

			// максимум что можно представить в виде atom
			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("!#$%&'*+-/=?^_`{|}~abyzABYZ0189");
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("!#$%&'*+-/=?^_`{|}~abyzABYZ0189", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);

			// максимум что можно представить в виде quotable
			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("!\"#$%&'()*+,-./0123456789:;<=>?@ABYZ[\\]^_`abyz{|}~");
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("\"!\\\"#$%&'()*+,-./0123456789:;<=>?@ABYZ[\\\\]^_`abyz{|}~\"", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);

			// требуется кодирование
			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("ABYZ©9810abyz!*+-/");
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("=?utf-8?B?QUJZWsKpOTgxMGFieXohKistLw==?=", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);

			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("«§2©2123}»");
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("=?utf-8?B?wqvCpzLCqTIxMjN9wrs=?=", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);

			// похоже на кодированное слово (отдельно)
			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("=?utf-8?B?0JY=?=");
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("=?utf-8?B?PT91dGYtOD9CPzBKWT0/PQ==?=", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);

			// похоже на кодированное слово (среди непохожих)
			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("abc =?utf-8?B?0JY=?= def");
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("abc", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" =?utf-8?B?PT91dGYtOD9CPzBKWT0/PQ==?=", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" def", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);

			// похоже на кодированное слово (выделено табами)
			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("abc\t=?utf-8?B?0JY=?=\tdef");
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("=?utf-8?B?YWJjCT0/dXRmLTg/Qj8wSlk9Pz0JZGVm?=", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);

			// не похоже на кодированное слово (не выделено пробельными символами)
			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("aaa_=?utf-8?B?0JY=?=_bbb");
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("aaa_=?utf-8?B?0JY=?=_bbb", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);

			// объединение нескольких слов
			// a a - объединять не нужно, оставляем свободу комбинирования
			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("abyz!#$%&'*+- ABYZ0189/=?^_`{|}~");
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("abyz!#$%&'*+-", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" ABYZ0189/=?^_`{|}~", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);

			// q q - объединять можно чтобы съэкономить две кавычки. нужно ли?
			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("@ABYZ 0123456789;");
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("\"@ABYZ 0123456789;\"", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);

			// e e - объединять нужно чтобы прилично съэкономить на обрамлении encoded-word
			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("ABYZ©9810 abyz§!*+-/");
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("=?utf-8?B?QUJZWsKpOTgxMCBhYnl6wqchKistLw==?=", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);

			// q a q - объединять можно чтобы сэкономить две кавычки. нужно ли?
			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("@ABYZ value 0123456789;");
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("\"@ABYZ value 0123456789;\"", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);

			// q e q - объединять невозможно
			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("@ABYZ §2000 0123456789;");
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("\"@ABYZ\"", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" =?utf-8?B?wqcyMDAw?=", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" \"0123456789;\"", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);

			// e a e - объединять нужно чтобы прилично съэкономить на обрамлении encoded-word
			src = Encoding.UTF8.GetBytes ("ABYZ©9810 value abyz§!*+-/");
			position = 0;
			prevSequenceIsWordEncoded = false;
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("=?utf-8?B?QUJZWsKpOTgxMCB2YWx1ZSBhYnl6wqchKistLw==?=", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);

			// e q e - объединять можно чтобы съэкономить на обрамлении encoded-word, но зависит от размера q посередине. нужно ли?
			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("ABYZ©9810 @ABYZ abyz§!*+-/");
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("=?utf-8?B?QUJZWsKpOTgxMCBAQUJZWiBhYnl6wqchKistLw==?=", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);

			// разбиение на части ограниченные макс.разрешенной длиной
			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("An 'encoded-word' may, appear: in a message; values or \"body part\" values according слово to the снова rules valuerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerules again");
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("An", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" 'encoded-word'", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" \"may, appear: in a message; values or \\\"body part\\\"\"", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" values", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" according", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" =?utf-8?B?0YHQu9C+0LLQviB0byB0aGUg0YHQvdC+0LLQsA==?=", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" rules", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" valuerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerules", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" again", Encoding.ASCII.GetString (buf, 0, size));
			size = HeaderFieldBodyEncoder.EncodeNextElement (src, buf, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (0, size);
		}
	}
}
