using System;
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
			var src = Encoding.ASCII.GetBytes ("=?aa?bb?cc?").AsSpan ();
			var item = new StructuredValueElement (StructuredValueElementType.Value, 0, src.Length);
			Assert.Equal (
				"=?aa?bb?cc?",
				StructuredValueElementCollection.DecodeElement (src.Slice (item.StartPosition, item.Length), item.ElementType));
			src = Encoding.ASCII.GetBytes ("=?aa?bb?cc?");
			item = new StructuredValueElement (StructuredValueElementType.QuotedValue, 0, src.Length);
			Assert.Equal (
				"=?aa?bb?cc?",
				StructuredValueElementCollection.DecodeElement (src.Slice (item.StartPosition, item.Length), item.ElementType));
			src = Encoding.ASCII.GetBytes ("=?aa?bb?cc?");
			item = new StructuredValueElement (StructuredValueElementType.SquareBracketedValue, 0, src.Length);
			Assert.Equal (
				"=?aa?bb?cc?",
				StructuredValueElementCollection.DecodeElement (src.Slice (item.StartPosition, item.Length), item.ElementType));
			src = Encoding.ASCII.GetBytes ("some   \\\"one\\\"");
			item = new StructuredValueElement (StructuredValueElementType.SquareBracketedValue, 0, src.Length);
			Assert.Equal (
				"some   \"one\"",
				StructuredValueElementCollection.DecodeElement (src.Slice (item.StartPosition, item.Length), item.ElementType));
			src = Encoding.ASCII.GetBytes ("some   \\\"one\\\"");
			item = new StructuredValueElement (StructuredValueElementType.QuotedValue, 0, src.Length);
			Assert.Equal (
				"some   \"one\"",
				StructuredValueElementCollection.DecodeElement (src.Slice (item.StartPosition, item.Length), item.ElementType));
			src = Encoding.ASCII.GetBytes ("=?utf-8*ru-ru?B?0YLQtdC80LAg0YHQvtC+0LHRidC10L3QuNGPINGC0LXQutGB0YIg0YHQvtC+0LHRidC10L3QuNGP?=");
			item = new StructuredValueElement (StructuredValueElementType.Value, 0, src.Length);
			Assert.Equal (
				"тема сообщения текст сообщения",
				StructuredValueElementCollection.DecodeElement (src.Slice (item.StartPosition, item.Length), item.ElementType));
			src = Encoding.ASCII.GetBytes ("=?utf-8*ru-ru?B?0YLQtdC80LAg0YHQvtC+0LHRidC10L3QuNGPINGC0LXQutGB0YIg0YHQvtC+0LHRidC10L3QuNGP?=");
			item = new StructuredValueElement (StructuredValueElementType.QuotedValue, 0, src.Length);
			Assert.Equal (
				"тема сообщения текст сообщения",
				StructuredValueElementCollection.DecodeElement (src.Slice (item.StartPosition, item.Length), item.ElementType));
		}

		[Fact]
		[Trait ("Category", "Mime")]
		public void Parse_Collection ()
		{
			var src = Encoding.ASCII.GetBytes ("abc,de;fgh \"ijkl\" (mnop)   <rst>\t[uvw]");
			var elements = StructuredValueElementCollection.Parse (
				src,
				AsciiCharClasses.Atom,
				false,
				StructuredValueElementType.Unspecified);
			Assert.Equal (9, elements.Count);
			Assert.Equal (StructuredValueElementType.Value, elements[0].ElementType);
			Assert.Equal (0, elements[0].StartPosition); // abc
			Assert.Equal (3, elements[0].Length);
			Assert.Equal (StructuredValueElementType.Separator, elements[1].ElementType);
			Assert.Equal (3, elements[1].StartPosition); // ,
			Assert.Equal (1, elements[1].Length);
			Assert.Equal (StructuredValueElementType.Value, elements[2].ElementType);
			Assert.Equal (4, elements[2].StartPosition); // de
			Assert.Equal (2, elements[2].Length);
			Assert.Equal (StructuredValueElementType.Separator, elements[3].ElementType);
			Assert.Equal (6, elements[3].StartPosition); // ;
			Assert.Equal (1, elements[3].Length);
			Assert.Equal (StructuredValueElementType.Value, elements[4].ElementType);
			Assert.Equal (7, elements[4].StartPosition); // fgh
			Assert.Equal (3, elements[4].Length);
			Assert.Equal (StructuredValueElementType.QuotedValue, elements[5].ElementType);
			Assert.Equal (12, elements[5].StartPosition); // ijkl
			Assert.Equal (4, elements[5].Length);
			Assert.Equal (StructuredValueElementType.RoundBracketedValue, elements[6].ElementType);
			Assert.Equal (19, elements[6].StartPosition); // mnop
			Assert.Equal (4, elements[6].Length);
			Assert.Equal (StructuredValueElementType.AngleBracketedValue, elements[7].ElementType);
			Assert.Equal (28, elements[7].StartPosition); // rst
			Assert.Equal (3, elements[7].Length);
			Assert.Equal (StructuredValueElementType.SquareBracketedValue, elements[8].ElementType);
			Assert.Equal (34, elements[8].StartPosition); // uvw
			Assert.Equal (3, elements[8].Length);
		}

		[Fact]
		[Trait ("Category", "Mime")]
		public void Decode_Collection ()
		{
			var strs = new string[]
			{
				"abc",
				",",
				"=?utf-8?B?INGD0YHQuNC70LXQvdC90YvRhQ==?=",
				"=?us-ascii?q?some_text?=",
				";",
				"i\\\\jkl",
			};
			var src = Encoding.ASCII.GetBytes (string.Join (' ', strs));
			var elements = new StructuredValueElement[]
			{
				new StructuredValueElement (StructuredValueElementType.Value, 0, strs[0].Length),
				new StructuredValueElement (StructuredValueElementType.Separator, strs[0].Length + 1, strs[1].Length),
				new StructuredValueElement (StructuredValueElementType.Value, strs[0].Length + 1 + strs[1].Length + 1, strs[2].Length),
				new StructuredValueElement (StructuredValueElementType.Value, strs[0].Length + 1 + strs[1].Length + 1 + strs[2].Length + 1, strs[3].Length),
				new StructuredValueElement (StructuredValueElementType.Separator, strs[0].Length + 1 + strs[1].Length + 1 + strs[2].Length + 1 + strs[3].Length + 1, strs[4].Length),
				new StructuredValueElement (StructuredValueElementType.QuotedValue, strs[0].Length + 1 + strs[1].Length + 1 + strs[2].Length + 1 + strs[3].Length + 1 + strs[4].Length + 1, strs[5].Length),
			};
			var result = StructuredValueElementCollection.Decode (elements, src, elements.Length);
			Assert.Equal ("abc ,  усиленныхsome text ; i\\jkl", result);
		}
	}
}
