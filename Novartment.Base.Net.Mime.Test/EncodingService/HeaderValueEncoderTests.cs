using Xunit;

namespace Novartment.Base.Net.Mime.Test
{
	public class HeaderValueEncoderTests
	{
		[Fact]
		[Trait ("Category", "Mime")]
		public void Parse ()
		{
			var words = HeaderValueEncoder.Parse (string.Empty, TextSemantics.Unstructured);
			Assert.Null (words.GetNextElement ());

			words = HeaderValueEncoder.Parse ("a", TextSemantics.Unstructured);
			Assert.Equal ("a", words.GetNextElement ());
			Assert.Null (words.GetNextElement ());

			words = HeaderValueEncoder.Parse (" a  ", TextSemantics.Unstructured);
			Assert.Equal (" a  ", words.GetNextElement ());
			Assert.Null (words.GetNextElement ());

			words = HeaderValueEncoder.Parse ("a \x3", TextSemantics.Unstructured);
			Assert.Equal ("a", words.GetNextElement ());
			Assert.Equal (" =?utf-8?B?Aw==?=", words.GetNextElement ());
			Assert.Null (words.GetNextElement ());

			words = HeaderValueEncoder.Parse (" \t ab\t\tcde\t \tf \t", TextSemantics.Unstructured);
			Assert.Equal (" \t ab", words.GetNextElement ());
			Assert.Equal ("\t\tcde", words.GetNextElement ());
			Assert.Equal ("\t \tf \t", words.GetNextElement ());
			Assert.Null (words.GetNextElement ());

			words = HeaderValueEncoder.Parse (" \t ab\t\tcde\t \tf \t", TextSemantics.Phrase);
			Assert.Equal ("\" \t ab\t\tcde\t \tf \t\"", words.GetNextElement ());
			Assert.Null (words.GetNextElement ());

			words = HeaderValueEncoder.Parse ("1:  §2 \t ДВА   ©1999...2001", TextSemantics.Unstructured);
			Assert.Equal ("1:", words.GetNextElement ());
			Assert.Equal ("  =?utf-8?B?wqcyIAkg0JTQktCQICAgwqkxOTk5Li4uMjAwMQ==?=", words.GetNextElement ());
			Assert.Null (words.GetNextElement ());

			var s1 = new string ('a', 20);
			var s2 = new string ('b', 40);
			var s3 = new string ('c', 60);
			var s4 = new string ('d', 80);
			words = HeaderValueEncoder.Parse ($" {s1}  {s2}   {s3} {s4}", TextSemantics.Phrase);
			Assert.Equal ($"\" {s1}  {s2}\"", words.GetNextElement ());
			Assert.Equal ($" \"  {s3}\"", words.GetNextElement ());
			Assert.Equal ($" {s4}", words.GetNextElement ());
			Assert.Null (words.GetNextElement ());

			words = HeaderValueEncoder.Parse ("a \x3", TextSemantics.Unstructured);
			Assert.Equal ("a", words.GetNextElement ());
			Assert.Equal (" =?utf-8?B?Aw==?=", words.GetNextElement ());
			Assert.Null (words.GetNextElement ());
		}
	}
}
