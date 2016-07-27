using Novartment.Base.Text;
using Xunit;

namespace Novartment.Base.Net.Mime.Test
{
	public class StructuredValueElementTests
	{
		[Fact, Trait ("Category", "Mime")]
		public void Decode ()
		{
			Assert.Equal ("=?aa?bb?cc?",
				new StructuredValueElement (StructuredValueElementType.Value, "=?aa?bb?cc?").Decode ());
			Assert.Equal (";",
				new StructuredValueElement (';').Decode ());
			Assert.Equal ("=?aa?bb?cc?",
				new StructuredValueElement (StructuredValueElementType.QuotedValue, "=?aa?bb?cc?").Decode ());
			Assert.Equal ("=?aa?bb?cc?",
				new StructuredValueElement (StructuredValueElementType.SquareBracketedValue, "=?aa?bb?cc?").Decode ());
			Assert.Equal ("some   \"one\"",
				new StructuredValueElement (StructuredValueElementType.SquareBracketedValue, "some   \\\"one\\\"").Decode ());
			Assert.Equal ("some   \"one\"",
				new StructuredValueElement (StructuredValueElementType.QuotedValue, "some   \\\"one\\\"").Decode ());
			Assert.Equal ("тема сообщения текст сообщения",
				new StructuredValueElement (StructuredValueElementType.Value, "=?utf-8*ru-ru?B?0YLQtdC80LAg0YHQvtC+0LHRidC10L3QuNGPINGC0LXQutGB0YIg0YHQvtC+0LHRidC10L3QuNGP?=").Decode ());
			Assert.Equal ("тема сообщения текст сообщения",
				new StructuredValueElement (StructuredValueElementType.QuotedValue, "=?utf-8*ru-ru?B?0YLQtdC80LAg0YHQvtC+0LHRidC10L3QuNGPINGC0LXQutGB0YIg0YHQvtC+0LHRidC10L3QuNGP?=").Decode ());
		}

		[Fact, Trait ("Category", "Mime")]
		public void Parse_Collection ()
		{
			var elements = StructuredValueElementCollection.Parse (
				"abc,de;fgh \"ijkl\" (mnop)   <rst>\t[uvw]",
				AsciiCharClasses.Atom,
				false,
				StructuredValueElementType.Unspecified);
			Assert.Equal (9, elements.Count);
			Assert.Equal (StructuredValueElementType.Value, elements[0].ElementType);
			Assert.Equal ("abc", elements[0].Value);
			Assert.Equal (StructuredValueElementType.Separator, elements[1].ElementType);
			Assert.Equal (",", elements[1].Value);
			Assert.Equal (StructuredValueElementType.Value, elements[2].ElementType);
			Assert.Equal ("de", elements[2].Value);
			Assert.Equal (StructuredValueElementType.Separator, elements[3].ElementType);
			Assert.Equal (";", elements[3].Value);
			Assert.Equal (StructuredValueElementType.Value, elements[4].ElementType);
			Assert.Equal ("fgh", elements[4].Value);
			Assert.Equal (StructuredValueElementType.QuotedValue, elements[5].ElementType);
			Assert.Equal ("ijkl", elements[5].Value);
			Assert.Equal (StructuredValueElementType.RoundBracketedValue, elements[6].ElementType);
			Assert.Equal ("mnop", elements[6].Value);
			Assert.Equal (StructuredValueElementType.AngleBracketedValue, elements[7].ElementType);
			Assert.Equal ("rst", elements[7].Value);
			Assert.Equal (StructuredValueElementType.SquareBracketedValue, elements[8].ElementType);
			Assert.Equal ("uvw", elements[8].Value);
		}

		[Fact, Trait ("Category", "Mime")]
		public void Decode_Collection ()
		{
			var elements = new StructuredValueElement[]
			{
				new StructuredValueElement (StructuredValueElementType.Value, "abc"),
				new StructuredValueElement (','),
				new StructuredValueElement (StructuredValueElementType.Value, "=?utf-8?B?INGD0YHQuNC70LXQvdC90YvRhQ==?="),
				new StructuredValueElement (StructuredValueElementType.Value, "=?us-ascii?q?some_text?="),
				new StructuredValueElement (';'),
				new StructuredValueElement (StructuredValueElementType.QuotedValue, "i\\\\jkl")
			};
			var result = StructuredValueElementCollection.Decode (elements, elements.Length);
			Assert.Equal ("abc ,  усиленныхsome text ; i\\jkl", result);
		}
	}
}
