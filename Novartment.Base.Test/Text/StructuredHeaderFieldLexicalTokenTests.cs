using System;
using System.Text;
using Novartment.Base.Text;
using Xunit;

namespace Novartment.Base.Test
{
	public class StructuredHeaderFieldLexicalTokenTests
	{
		[Fact]
		[Trait ("Category", "Mime")]
		public void Parse ()
		{
			ReadOnlySpan<char> src = default;
			var parserPos = 0;
			var element = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.False (element.IsValid);

			// комментарии берём
			src = "(a1)  bb2\t(c3 (d4)) eee5\t\t(ff6)";
			parserPos = 0;
			element = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Token, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.RoundBracketedValue, element.TokenType);
			Assert.Equal (1, element.Position); // a1
			Assert.Equal (2, element.Length);
			element = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Token, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.Value, element.TokenType);
			Assert.Equal (6, element.Position); // bb2
			Assert.Equal (3, element.Length);
			element = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Token, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.RoundBracketedValue, element.TokenType);
			Assert.Equal (11, element.Position); // (c3 (d4))
			Assert.Equal (7, element.Length);
			element = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Token, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.Value, element.TokenType);
			Assert.Equal (20, element.Position); // eee5
			Assert.Equal (4, element.Length);
			element = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Token, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.RoundBracketedValue, element.TokenType);
			Assert.Equal (27, element.Position); // ff6
			Assert.Equal (3, element.Length);
			element = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Token, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.False (element.IsValid);

			// комментарии пропускаем
			parserPos = 0;
			element = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Token, false, StructuredHeaderFieldLexicalTokenType.RoundBracketedValue);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.Value, element.TokenType);
			Assert.Equal (6, element.Position); // bb2
			Assert.Equal (3, element.Length);
			element = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Token, false, StructuredHeaderFieldLexicalTokenType.RoundBracketedValue);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.Value, element.TokenType);
			Assert.Equal (20, element.Position); // eee5
			Assert.Equal (4, element.Length);
			element = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Token, false, StructuredHeaderFieldLexicalTokenType.RoundBracketedValue);
			Assert.False (element.IsValid);

			// квотированные символы распознаются только в комментариях и кавычках
			src = " a1\\ 1 (bb2\\)2) \"ccc3\\\"3\\ \",";
			parserPos = 0;
			element = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Token, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.Value, element.TokenType);
			Assert.Equal (1, element.Position); // 'a1'
			Assert.Equal (2, element.Length);
			element = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Token, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.Separator, element.TokenType);
			Assert.Equal (3, element.Position); // '\'
			Assert.Equal (1, element.Length);
			element = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Token, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.Value, element.TokenType);
			Assert.Equal (5, element.Position); // '1'
			Assert.Equal (1, element.Length);
			element = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Token, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.RoundBracketedValue, element.TokenType);
			Assert.Equal (8, element.Position); // 'bb2\)2'
			Assert.Equal (6, element.Length);
			element = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Token, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.QuotedValue, element.TokenType);
			Assert.Equal (17, element.Position); // 'ccc3\"3\ '
			Assert.Equal (9, element.Length);
			element = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Token, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.Separator, element.TokenType);
			Assert.Equal (27, element.Position); // ','
			Assert.Equal (1, element.Length);
			element = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Token, false, StructuredHeaderFieldLexicalTokenType.RoundBracketedValue);
			Assert.False (element.IsValid);

			// встречаются все типы
			src = "abc,de;fgh \"ijkl\" (mnop)   <rst>\t[uvw]";
			parserPos = 0;
			element = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.Value, element.TokenType);
			Assert.Equal (0, element.Position); // abc
			Assert.Equal (3, element.Length);
			element = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.Separator, element.TokenType);
			Assert.Equal (3, element.Position); // ,
			Assert.Equal (1, element.Length);
			element = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.Value, element.TokenType);
			Assert.Equal (4, element.Position); // de
			Assert.Equal (2, element.Length);
			element = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.Separator, element.TokenType);
			Assert.Equal (6, element.Position); // ;
			Assert.Equal (1, element.Length);
			element = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.Value, element.TokenType);
			Assert.Equal (7, element.Position); // fgh
			Assert.Equal (3, element.Length);
			element = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.QuotedValue, element.TokenType);
			Assert.Equal (12, element.Position); // ijkl
			Assert.Equal (4, element.Length);
			element = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.RoundBracketedValue, element.TokenType);
			Assert.Equal (19, element.Position); // mnop
			Assert.Equal (4, element.Length);
			element = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.AngleBracketedValue, element.TokenType);
			Assert.Equal (28, element.Position); // rst
			Assert.Equal (3, element.Length);
			element = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.SquareBracketedValue, element.TokenType);
			Assert.Equal (34, element.Position); // uvw
			Assert.Equal (3, element.Length);
			element = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.False (element.IsValid);
			element = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.False (element.IsValid);
		}

		[Fact]
		[Trait ("Category", "Mime")]
		public void DecodeElement ()
		{
			var src = "=?aa?bb?cc?".AsSpan ();
			var item = new StructuredHeaderFieldLexicalToken (StructuredHeaderFieldLexicalTokenType.Value, 0, src.Length);
			Assert.Equal ("=?aa?bb?cc?", item.Decode (src));

			src = "=?aa?bb?cc?";
			item = new StructuredHeaderFieldLexicalToken (StructuredHeaderFieldLexicalTokenType.QuotedValue, 0, src.Length);
			Assert.Equal ("=?aa?bb?cc?", item.Decode (src));

			src = "=?aa?bb?cc?";
			item = new StructuredHeaderFieldLexicalToken (StructuredHeaderFieldLexicalTokenType.SquareBracketedValue, 0, src.Length);
			Assert.Equal ("=?aa?bb?cc?", item.Decode (src));

			src = "some   \\\"one\\\"";
			item = new StructuredHeaderFieldLexicalToken (StructuredHeaderFieldLexicalTokenType.SquareBracketedValue, 0, src.Length);
			Assert.Equal ("some   \"one\"", item.Decode (src));

			src = "some   \\\"one\\\"";
			item = new StructuredHeaderFieldLexicalToken (StructuredHeaderFieldLexicalTokenType.QuotedValue, 0, src.Length);
			Assert.Equal ("some   \"one\"", item.Decode (src));

			src = "=?utf-8*ru-ru?B?0YLQtdC80LAg0YHQvtC+0LHRidC10L3QuNGPINGC0LXQutGB0YIg0YHQvtC+0LHRidC10L3QuNGP?=";
			item = new StructuredHeaderFieldLexicalToken (StructuredHeaderFieldLexicalTokenType.Value, 0, src.Length);
			Assert.Equal ("тема сообщения текст сообщения", item.Decode (src));

			src = "=?utf-8*ru-ru?B?0YLQtdC80LAg0YHQvtC+0LHRidC10L3QuNGPINGC0LXQutGB0YIg0YHQvtC+0LHRidC10L3QuNGP?=";
			item = new StructuredHeaderFieldLexicalToken (StructuredHeaderFieldLexicalTokenType.Value, 0, src.Length);
			Assert.Equal ("тема сообщения текст сообщения", item.Decode (src));
		}

		[Fact]
		[Trait ("Category", "Mime")]
		public void DecodeElementSpan ()
		{
			var buf = new char[500];

			var src = "=?aa?bb?cc?".AsSpan ();
			var item = new StructuredHeaderFieldLexicalToken (StructuredHeaderFieldLexicalTokenType.Value, 0, src.Length);
			var size = item.Decode (src, buf);
			Assert.Equal ("=?aa?bb?cc?", new string (buf, 0, size));

			src = "=?aa?bb?cc?";
			item = new StructuredHeaderFieldLexicalToken (StructuredHeaderFieldLexicalTokenType.QuotedValue, 0, src.Length);
			size = item.Decode (src, buf);
			Assert.Equal ("=?aa?bb?cc?", new string (buf, 0, size));

			src = "=?aa?bb?cc?";
			item = new StructuredHeaderFieldLexicalToken (StructuredHeaderFieldLexicalTokenType.SquareBracketedValue, 0, src.Length);
			size = item.Decode (src, buf);
			Assert.Equal ("=?aa?bb?cc?", new string (buf, 0, size));

			src = "some   \\\"one\\\"";
			item = new StructuredHeaderFieldLexicalToken (StructuredHeaderFieldLexicalTokenType.SquareBracketedValue, 0, src.Length);
			size = item.Decode (src, buf);
			Assert.Equal ("some   \"one\"", new string (buf, 0, size));

			src = "some   \\\"one\\\"";
			item = new StructuredHeaderFieldLexicalToken (StructuredHeaderFieldLexicalTokenType.QuotedValue, 0, src.Length);
			size = item.Decode (src, buf);
			Assert.Equal ("some   \"one\"", new string (buf, 0, size));

			src = "=?utf-8*ru-ru?B?0YLQtdC80LAg0YHQvtC+0LHRidC10L3QuNGPINGC0LXQutGB0YIg0YHQvtC+0LHRidC10L3QuNGP?=";
			item = new StructuredHeaderFieldLexicalToken (StructuredHeaderFieldLexicalTokenType.Value, 0, src.Length);
			size = item.Decode (src, buf);
			Assert.Equal ("тема сообщения текст сообщения", new string (buf, 0, size));

			src = "=?utf-8*ru-ru?B?0YLQtdC80LAg0YHQvtC+0LHRidC10L3QuNGPINGC0LXQutGB0YIg0YHQvtC+0LHRidC10L3QuNGP?=";
			item = new StructuredHeaderFieldLexicalToken (StructuredHeaderFieldLexicalTokenType.Value, 0, src.Length);
			size = item.Decode (src, buf);
			Assert.Equal ("тема сообщения текст сообщения", new string (buf, 0, size));
		}
	}
}
