using System.Text;
using Novartment.Base.Text;
using Xunit;

namespace Novartment.Base.Net.Mime.Test
{
	public class StructuredValueElementTests
	{
		[Fact]
		[Trait ("Category", "Mime")]
		public void Decode ()
		{
			Assert.Equal (
				"=?aa?bb?cc?",
				new StructuredValueElement (StructuredValueElementType.Value, Encoding.ASCII.GetBytes ("=?aa?bb?cc?")).Decode ());
			Assert.Equal (
				";",
				new StructuredValueElement ((byte)';').Decode ());
			Assert.Equal (
				"=?aa?bb?cc?",
				new StructuredValueElement (StructuredValueElementType.QuotedValue, Encoding.ASCII.GetBytes ("=?aa?bb?cc?")).Decode ());
			Assert.Equal (
				"=?aa?bb?cc?",
				new StructuredValueElement (StructuredValueElementType.SquareBracketedValue, Encoding.ASCII.GetBytes ("=?aa?bb?cc?")).Decode ());
			Assert.Equal (
				"some   \"one\"",
				new StructuredValueElement (StructuredValueElementType.SquareBracketedValue, Encoding.ASCII.GetBytes ("some   \\\"one\\\"")).Decode ());
			Assert.Equal (
				"some   \"one\"",
				new StructuredValueElement (StructuredValueElementType.QuotedValue, Encoding.ASCII.GetBytes ("some   \\\"one\\\"")).Decode ());
			Assert.Equal (
				"тема сообщения текст сообщения",
				new StructuredValueElement (StructuredValueElementType.Value, Encoding.ASCII.GetBytes ("=?utf-8*ru-ru?B?0YLQtdC80LAg0YHQvtC+0LHRidC10L3QuNGPINGC0LXQutGB0YIg0YHQvtC+0LHRidC10L3QuNGP?=")).Decode ());
			Assert.Equal (
				"тема сообщения текст сообщения",
				new StructuredValueElement (StructuredValueElementType.QuotedValue, Encoding.ASCII.GetBytes ("=?utf-8*ru-ru?B?0YLQtdC80LAg0YHQvtC+0LHRidC10L3QuNGPINGC0LXQutGB0YIg0YHQvtC+0LHRidC10L3QuNGP?=")).Decode ());
		}

		[Fact]
		[Trait ("Category", "Mime")]
		public void Parse_Collection ()
		{
			var elements = StructuredValueElementCollection.Parse (
				Encoding.ASCII.GetBytes ("abc,de;fgh \"ijkl\" (mnop)   <rst>\t[uvw]"),
				AsciiCharClasses.Atom,
				false,
				StructuredValueElementType.Unspecified);
			Assert.Equal (9, elements.Count);
			Assert.Equal (StructuredValueElementType.Value, elements[0].ElementType);
			Assert.Equal ("abc", Encoding.ASCII.GetString (elements[0].Value.Span));
			Assert.Equal (StructuredValueElementType.Separator, elements[1].ElementType);
			Assert.Equal (",", Encoding.ASCII.GetString (elements[1].Value.Span));
			Assert.Equal (StructuredValueElementType.Value, elements[2].ElementType);
			Assert.Equal ("de", Encoding.ASCII.GetString (elements[2].Value.Span));
			Assert.Equal (StructuredValueElementType.Separator, elements[3].ElementType);
			Assert.Equal (";", Encoding.ASCII.GetString (elements[3].Value.Span));
			Assert.Equal (StructuredValueElementType.Value, elements[4].ElementType);
			Assert.Equal ("fgh", Encoding.ASCII.GetString (elements[4].Value.Span));
			Assert.Equal (StructuredValueElementType.QuotedValue, elements[5].ElementType);
			Assert.Equal ("ijkl", Encoding.ASCII.GetString (elements[5].Value.Span));
			Assert.Equal (StructuredValueElementType.RoundBracketedValue, elements[6].ElementType);
			Assert.Equal ("mnop", Encoding.ASCII.GetString (elements[6].Value.Span));
			Assert.Equal (StructuredValueElementType.AngleBracketedValue, elements[7].ElementType);
			Assert.Equal ("rst", Encoding.ASCII.GetString (elements[7].Value.Span));
			Assert.Equal (StructuredValueElementType.SquareBracketedValue, elements[8].ElementType);
			Assert.Equal ("uvw", Encoding.ASCII.GetString (elements[8].Value.Span));
		}

		[Fact]
		[Trait ("Category", "Mime")]
		public void Decode_Collection ()
		{
			var elements = new StructuredValueElement[]
			{
				new StructuredValueElement (StructuredValueElementType.Value, Encoding.ASCII.GetBytes ("abc")),
				new StructuredValueElement ((byte)','),
				new StructuredValueElement (StructuredValueElementType.Value, Encoding.ASCII.GetBytes ("=?utf-8?B?INGD0YHQuNC70LXQvdC90YvRhQ==?=")),
				new StructuredValueElement (StructuredValueElementType.Value, Encoding.ASCII.GetBytes ("=?us-ascii?q?some_text?=")),
				new StructuredValueElement ((byte)';'),
				new StructuredValueElement (StructuredValueElementType.QuotedValue, Encoding.ASCII.GetBytes ("i\\\\jkl")),
			};
			var result = StructuredValueElementCollection.Decode (elements, elements.Length);
			Assert.Equal ("abc ,  усиленныхsome text ; i\\jkl", result);
		}
	}
}
