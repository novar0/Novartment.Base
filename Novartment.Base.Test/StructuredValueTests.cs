using System;
using System.Text;
using Novartment.Base.Text;
using Xunit;

namespace Novartment.Base.Test
{
	public class StructuredValueTests
	{
		[Fact]
		[Trait ("Category", "Mime")]
		public void Parse ()
		{
			ReadOnlySpan<byte> src = default (ReadOnlySpan<byte>);
			var parserPos = 0;
			var element = StructuredValueParser.GetNextElement (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredValueElementType.Unspecified);
			Assert.False (element.IsValid);

			src = Encoding.ASCII.GetBytes ("abc,de;fgh \"ijkl\" (mnop)   <rst>\t[uvw]");
			parserPos = 0;
			element = StructuredValueParser.GetNextElement (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredValueElementType.Unspecified);
			Assert.Equal (StructuredValueElementType.Value, element.ElementType);
			Assert.Equal (0, element.StartPosition); // abc
			Assert.Equal (3, element.Length);
			element = StructuredValueParser.GetNextElement (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredValueElementType.Unspecified);
			Assert.Equal (StructuredValueElementType.Separator, element.ElementType);
			Assert.Equal (3, element.StartPosition); // ,
			Assert.Equal (1, element.Length);
			element = StructuredValueParser.GetNextElement (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredValueElementType.Unspecified);
			Assert.Equal (StructuredValueElementType.Value, element.ElementType);
			Assert.Equal (4, element.StartPosition); // de
			Assert.Equal (2, element.Length);
			element = StructuredValueParser.GetNextElement (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredValueElementType.Unspecified);
			Assert.Equal (StructuredValueElementType.Separator, element.ElementType);
			Assert.Equal (6, element.StartPosition); // ;
			Assert.Equal (1, element.Length);
			element = StructuredValueParser.GetNextElement (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredValueElementType.Unspecified);
			Assert.Equal (StructuredValueElementType.Value, element.ElementType);
			Assert.Equal (7, element.StartPosition); // fgh
			Assert.Equal (3, element.Length);
			element = StructuredValueParser.GetNextElement (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredValueElementType.Unspecified);
			Assert.Equal (StructuredValueElementType.QuotedValue, element.ElementType);
			Assert.Equal (12, element.StartPosition); // ijkl
			Assert.Equal (4, element.Length);
			element = StructuredValueParser.GetNextElement (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredValueElementType.Unspecified);
			Assert.Equal (StructuredValueElementType.RoundBracketedValue, element.ElementType);
			Assert.Equal (19, element.StartPosition); // mnop
			Assert.Equal (4, element.Length);
			element = StructuredValueParser.GetNextElement (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredValueElementType.Unspecified);
			Assert.Equal (StructuredValueElementType.AngleBracketedValue, element.ElementType);
			Assert.Equal (28, element.StartPosition); // rst
			Assert.Equal (3, element.Length);
			element = StructuredValueParser.GetNextElement (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredValueElementType.Unspecified);
			Assert.Equal (StructuredValueElementType.SquareBracketedValue, element.ElementType);
			Assert.Equal (34, element.StartPosition); // uvw
			Assert.Equal (3, element.Length);
			element = StructuredValueParser.GetNextElement (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredValueElementType.Unspecified);
			Assert.False (element.IsValid);
			element = StructuredValueParser.GetNextElement (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredValueElementType.Unspecified);
			Assert.False (element.IsValid);
		}

		[Fact]
		[Trait ("Category", "Mime")]
		public void DecodeElement ()
		{
			var src = Encoding.ASCII.GetBytes ("=?aa?bb?cc?").AsSpan ();
			var item = new StructuredValueElement (StructuredValueElementType.Value, 0, src.Length);
			Assert.Equal ("=?aa?bb?cc?", item.DecodeElement (src));
			src = Encoding.ASCII.GetBytes ("=?aa?bb?cc?");
			item = new StructuredValueElement (StructuredValueElementType.QuotedValue, 0, src.Length);
			Assert.Equal ("=?aa?bb?cc?", item.DecodeElement (src));
			src = Encoding.ASCII.GetBytes ("=?aa?bb?cc?");
			item = new StructuredValueElement (StructuredValueElementType.SquareBracketedValue, 0, src.Length);
			Assert.Equal ("=?aa?bb?cc?", item.DecodeElement (src));
			src = Encoding.ASCII.GetBytes ("some   \\\"one\\\"");
			item = new StructuredValueElement (StructuredValueElementType.SquareBracketedValue, 0, src.Length);
			Assert.Equal ("some   \"one\"", item.DecodeElement (src));
			src = Encoding.ASCII.GetBytes ("some   \\\"one\\\"");
			item = new StructuredValueElement (StructuredValueElementType.QuotedValue, 0, src.Length);
			Assert.Equal ("some   \"one\"", item.DecodeElement (src));
			src = Encoding.ASCII.GetBytes ("=?utf-8*ru-ru?B?0YLQtdC80LAg0YHQvtC+0LHRidC10L3QuNGPINGC0LXQutGB0YIg0YHQvtC+0LHRidC10L3QuNGP?=");
			item = new StructuredValueElement (StructuredValueElementType.Value, 0, src.Length);
			Assert.Equal ("тема сообщения текст сообщения", item.DecodeElement (src));
			src = Encoding.ASCII.GetBytes ("=?utf-8*ru-ru?B?0YLQtdC80LAg0YHQvtC+0LHRidC10L3QuNGPINGC0LXQutGB0YIg0YHQvtC+0LHRidC10L3QuNGP?=");
			item = new StructuredValueElement (StructuredValueElementType.QuotedValue, 0, src.Length);
			Assert.Equal ("тема сообщения текст сообщения", item.DecodeElement (src));
		}
	}
}
