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
			var token = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.False (token.IsValid);

			// комментарии берём
			src = "(a1)  bb2\t(c3 (d4)) eee5\t\t(ff6)";
			parserPos = 0;
			token = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Token, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.RoundBracketedValue, token.TokenType);
			Assert.Equal (1, token.Position); // a1
			Assert.Equal (2, token.Length);
			token = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Token, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.Value, token.TokenType);
			Assert.Equal (6, token.Position); // bb2
			Assert.Equal (3, token.Length);
			token = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Token, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.RoundBracketedValue, token.TokenType);
			Assert.Equal (11, token.Position); // (c3 (d4))
			Assert.Equal (7, token.Length);
			token = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Token, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.Value, token.TokenType);
			Assert.Equal (20, token.Position); // eee5
			Assert.Equal (4, token.Length);
			token = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Token, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.RoundBracketedValue, token.TokenType);
			Assert.Equal (27, token.Position); // ff6
			Assert.Equal (3, token.Length);
			token = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Token, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.False (token.IsValid);

			// комментарии пропускаем
			parserPos = 0;
			token = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Token, false, StructuredHeaderFieldLexicalTokenType.RoundBracketedValue);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.Value, token.TokenType);
			Assert.Equal (6, token.Position); // bb2
			Assert.Equal (3, token.Length);
			token = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Token, false, StructuredHeaderFieldLexicalTokenType.RoundBracketedValue);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.Value, token.TokenType);
			Assert.Equal (20, token.Position); // eee5
			Assert.Equal (4, token.Length);
			token = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Token, false, StructuredHeaderFieldLexicalTokenType.RoundBracketedValue);
			Assert.False (token.IsValid);

			// квотированные символы распознаются только в комментариях и кавычках
			src = " a1\\ 1 (bb2\\)2) \"ccc3\\\"3\\ \",";
			parserPos = 0;
			token = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Token, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.Value, token.TokenType);
			Assert.Equal (1, token.Position); // 'a1'
			Assert.Equal (2, token.Length);
			token = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Token, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.Separator, token.TokenType);
			Assert.Equal (3, token.Position); // '\'
			Assert.Equal (1, token.Length);
			token = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Token, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.Value, token.TokenType);
			Assert.Equal (5, token.Position); // '1'
			Assert.Equal (1, token.Length);
			token = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Token, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.RoundBracketedValue, token.TokenType);
			Assert.Equal (8, token.Position); // 'bb2\)2'
			Assert.Equal (6, token.Length);
			token = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Token, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.QuotedValue, token.TokenType);
			Assert.Equal (17, token.Position); // 'ccc3\"3\ '
			Assert.Equal (9, token.Length);
			token = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Token, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.Separator, token.TokenType);
			Assert.Equal (27, token.Position); // ','
			Assert.Equal (1, token.Length);
			token = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Token, false, StructuredHeaderFieldLexicalTokenType.RoundBracketedValue);
			Assert.False (token.IsValid);

			// встречаются все типы
			src = "abc,de;fgh \"ijkl\" (mnop)   <rst>\t[uvw]";
			parserPos = 0;
			token = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.Value, token.TokenType);
			Assert.Equal (0, token.Position); // abc
			Assert.Equal (3, token.Length);
			token = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.Separator, token.TokenType);
			Assert.Equal (3, token.Position); // ,
			Assert.Equal (1, token.Length);
			token = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.Value, token.TokenType);
			Assert.Equal (4, token.Position); // de
			Assert.Equal (2, token.Length);
			token = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.Separator, token.TokenType);
			Assert.Equal (6, token.Position); // ;
			Assert.Equal (1, token.Length);
			token = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.Value, token.TokenType);
			Assert.Equal (7, token.Position); // fgh
			Assert.Equal (3, token.Length);
			token = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.QuotedValue, token.TokenType);
			Assert.Equal (12, token.Position); // ijkl
			Assert.Equal (4, token.Length);
			token = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.RoundBracketedValue, token.TokenType);
			Assert.Equal (19, token.Position); // mnop
			Assert.Equal (4, token.Length);
			token = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.AngleBracketedValue, token.TokenType);
			Assert.Equal (28, token.Position); // rst
			Assert.Equal (3, token.Length);
			token = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.Equal (StructuredHeaderFieldLexicalTokenType.SquareBracketedValue, token.TokenType);
			Assert.Equal (34, token.Position); // uvw
			Assert.Equal (3, token.Length);
			token = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.False (token.IsValid);
			token = StructuredHeaderFieldLexicalToken.Parse (src, ref parserPos, AsciiCharClasses.Atom, false, StructuredHeaderFieldLexicalTokenType.Unspecified);
			Assert.False (token.IsValid);
		}

		[Fact]
		[Trait ("Category", "Mime")]
		public void Decode ()
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
