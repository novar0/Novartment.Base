using System.Text;
using Xunit;

namespace Novartment.Base.Net.Mime.Test
{
	public class HeaderFieldBodyEncoderTests
	{
		[Fact]
		[Trait ("Category", "Mime")]
		public void GetNextElementUnstructured ()
		{
			var src = Encoding.UTF8.GetBytes (string.Empty);
			var position = 0;
			var prevSequenceIsWordEncoded = false;
			var element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);

			src = Encoding.UTF8.GetBytes ("a");
			position = 0;
			prevSequenceIsWordEncoded = false;
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("a", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);

			src = Encoding.UTF8.GetBytes (" a  ");
			position = 0;
			prevSequenceIsWordEncoded = false;
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" a  ", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);

			src = Encoding.UTF8.GetBytes ("a \x3");
			position = 0;
			prevSequenceIsWordEncoded = false;
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("a", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" =?utf-8?B?Aw==?=", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);

			// похоже на комментарии и на строки в кавычках (не должны распознаваться)
			src = Encoding.UTF8.GetBytes ("\t (a 1)  \"[no\\\\ne]\" bb\tccc  ");
			position = 0;
			prevSequenceIsWordEncoded = false;
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("\t (a", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" 1)", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("  \"[no\\\\ne]\"", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" bb", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("\tccc  ", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);

			// похоже на кодированные слова (должны быть закодированы)
			src = Encoding.UTF8.GetBytes ("aaa   =?utf-8?B?0JY=?= bbb");
			position = 0;
			prevSequenceIsWordEncoded = false;
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("aaa", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("   =?utf-8?B?PT91dGYtOD9CPzBKWT0/PQ==?=", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" bbb", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);

			src = Encoding.UTF8.GetBytes (" =?utf-64?q?one?= \t=?utf-8?B?0JY=?= ");
			position = 0;
			prevSequenceIsWordEncoded = false;
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" =?utf-8?B?PT91dGYtNjQ/cT9vbmU/PSAJPT91dGYtOD9CPzBKWT0/PSA=?=", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);

			// не-ASCII символы (должны быть закодированы)
			src = Encoding.UTF8.GetBytes ("1:  §2 \t ДВА   ©1999...2001");
			position = 0;
			prevSequenceIsWordEncoded = false;
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("1:", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("  =?utf-8?B?wqcyIAkg0JTQktCQICAgwqkxOTk5Li4uMjAwMQ==?=", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);

			// разбиение на части ограниченные макс.разрешенной длиной
			src = Encoding.UTF8.GetBytes ("An 'encoded-word' may, appear: in a message; values or \"body part\" values according слово to the снова rules valuerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerules again");
			position = 0;
			prevSequenceIsWordEncoded = false;
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("An", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" 'encoded-word'", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" may,", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" appear:", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" in", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" a", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" message;", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" values", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" or", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" \"body", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" part\"", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" values", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" according", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" =?utf-8?B?0YHQu9C+0LLQviB0byB0aGUg0YHQvdC+0LLQsA==?=", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" rules", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" valuerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerules", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" again", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);
		}

		[Fact]
		[Trait ("Category", "Mime")]
		public void GetNextElementPhrase ()
		{
			var src = Encoding.UTF8.GetBytes (" \t ab\t\tcde\t \tf \t");
			var position = 0;
			var prevSequenceIsWordEncoded = false;
			var element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("\" \t ab\t\tcde\t \tf \t\"", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);

			var s1 = new string ('a', 20);
			var s2 = new string ('b', 40);
			var s3 = new string ('c', 60);
			var s4 = new string ('d', 80);
			src = Encoding.UTF8.GetBytes ($" {s1}  {s2}   {s3} {s4}");
			position = 0;
			prevSequenceIsWordEncoded = false;
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ($"\" {s1}  {s2}\"", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ($" \"  {s3}\"", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ($" {s4}", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);

			// пробельные символы в начале, конце и середине
			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("abc");
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("abc", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);

			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("abc def");
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("abc", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" def", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);

			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("abc\tdef");
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("\"abc\tdef\"", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);

			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes (" abc def");
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("\" abc\"", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" def", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);

			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("abc def ");
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("abc", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" \"def \"", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);

			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("abc  def");
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("abc", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" \" def\"", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);

			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes (" abc def ");
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("\" abc def \"", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);

			// максимум что можно представить в виде atom
			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("!#$%&'*+-/=?^_`{|}~abyzABYZ0189");
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("!#$%&'*+-/=?^_`{|}~abyzABYZ0189", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);

			// максимум что можно представить в виде quotable
			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("!\"#$%&'()*+,-./0123456789:;<=>?@ABYZ[\\]^_`abyz{|}~");
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("\"!\\\"#$%&'()*+,-./0123456789:;<=>?@ABYZ[\\\\]^_`abyz{|}~\"", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);

			// требуется кодирование
			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("ABYZ©9810abyz!*+-/");
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("=?utf-8?B?QUJZWsKpOTgxMGFieXohKistLw==?=", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);

			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("«§2©2123}»");
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("=?utf-8?B?wqvCpzLCqTIxMjN9wrs=?=", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);

			// похоже на кодированное слово (отдельно)
			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("=?utf-8?B?0JY=?=");
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("=?utf-8?B?PT91dGYtOD9CPzBKWT0/PQ==?=", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);

			// похоже на кодированное слово (среди непохожих)
			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("abc =?utf-8?B?0JY=?= def");
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("abc", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" =?utf-8?B?PT91dGYtOD9CPzBKWT0/PQ==?=", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" def", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);

			// похоже на кодированное слово (выделено табами)
			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("abc\t=?utf-8?B?0JY=?=\tdef");
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("=?utf-8?B?YWJjCT0/dXRmLTg/Qj8wSlk9Pz0JZGVm?=", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);

			// не похоже на кодированное слово (не выделено пробельными символами)
			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("aaa_=?utf-8?B?0JY=?=_bbb");
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("aaa_=?utf-8?B?0JY=?=_bbb", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);

			// объединение нескольких слов
			// a a - объединять не нужно, оставляем свободу комбинирования
			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("abyz!#$%&'*+- ABYZ0189/=?^_`{|}~");
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("abyz!#$%&'*+-", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" ABYZ0189/=?^_`{|}~", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);

			// q q - объединять можно чтобы съэкономить две кавычки. нужно ли?
			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("@ABYZ 0123456789;");
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("\"@ABYZ 0123456789;\"", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);

			// e e - объединять нужно чтобы прилично съэкономить на обрамлении encoded-word
			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("ABYZ©9810 abyz§!*+-/");
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("=?utf-8?B?QUJZWsKpOTgxMCBhYnl6wqchKistLw==?=", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);

			// q a q - объединять можно чтобы сэкономить две кавычки. нужно ли?
			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("@ABYZ value 0123456789;");
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("\"@ABYZ value 0123456789;\"", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);

			// q e q - объединять невозможно
			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("@ABYZ §2000 0123456789;");
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("\"@ABYZ\"", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" =?utf-8?B?wqcyMDAw?=", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" \"0123456789;\"", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);

			// e a e - объединять нужно чтобы прилично съэкономить на обрамлении encoded-word
			src = Encoding.UTF8.GetBytes ("ABYZ©9810 value abyz§!*+-/");
			position = 0;
			prevSequenceIsWordEncoded = false;
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("=?utf-8?B?QUJZWsKpOTgxMCB2YWx1ZSBhYnl6wqchKistLw==?=", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);

			// e q e - объединять можно чтобы съэкономить на обрамлении encoded-word, но зависит от размера q посередине. нужно ли?
			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("ABYZ©9810 @ABYZ abyz§!*+-/");
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("=?utf-8?B?QUJZWsKpOTgxMCBAQUJZWiBhYnl6wqchKistLw==?=", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);

			// разбиение на части ограниченные макс.разрешенной длиной
			position = 0;
			prevSequenceIsWordEncoded = false;
			src = Encoding.UTF8.GetBytes ("An 'encoded-word' may, appear: in a message; values or \"body part\" values according слово to the снова rules valuerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerules again");
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal ("An", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" 'encoded-word'", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" \"may, appear: in a message; values or \\\"body part\\\"\"", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" values", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" according", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" =?utf-8?B?0YHQu9C+0LLQviB0byB0aGUg0YHQvdC+0LLQsA==?=", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" rules", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" valuerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerules", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Equal (" again", element);
			element = HeaderFieldBodyEncoder.EncodeNextElement (src, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
			Assert.Null (element);
		}
	}
}
