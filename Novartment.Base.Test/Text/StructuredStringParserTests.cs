using System;
using Novartment.Base.Text;
using Xunit;

namespace Novartment.Base.Test
{
	public class StructuredStringParserTests
	{
		[Fact]
		[Trait ("Category", "Text.StructuredStringParser")]
		public void Parse ()
		{
			ReadOnlySpan<char> src = default;
			var parserPos = 0;
			var parser = new StructuredStringParser (AsciiCharClasses.WhiteSpace, AsciiCharClasses.Atom, false, null);
			var token = parser.Parse (src, ref parserPos);
			Assert.False (token.IsValid);

			src = "(a1)  bb2\t(c3 (d4)) eee5\t\t(ff6)";
			parserPos = 0;
			parser = new StructuredStringParser (AsciiCharClasses.WhiteSpace, AsciiCharClasses.Token, false, StructuredStringParser.StructuredHeaderFieldBodyFormats);
			token = parser.Parse (src, ref parserPos);
			Assert.True (token.IsRoundBracketedValue (src));
			Assert.Equal (0, token.Position); // a1
			Assert.Equal (4, token.Length);
			token = parser.Parse (src, ref parserPos);
			Assert.Equal (StructuredStringTokenType.Value, token.TokenType);
			Assert.Equal (6, token.Position); // bb2
			Assert.Equal (3, token.Length);
			token = parser.Parse (src, ref parserPos);
			Assert.True (token.IsRoundBracketedValue (src));
			Assert.Equal (10, token.Position); // (c3 (d4))
			Assert.Equal (9, token.Length);
			token = parser.Parse (src, ref parserPos);
			Assert.Equal (StructuredStringTokenType.Value, token.TokenType);
			Assert.Equal (20, token.Position); // eee5
			Assert.Equal (4, token.Length);
			token = parser.Parse (src, ref parserPos);
			Assert.True (token.IsRoundBracketedValue (src));
			Assert.Equal (26, token.Position); // ff6
			Assert.Equal (5, token.Length);
			token = parser.Parse (src, ref parserPos);
			Assert.False (token.IsValid);

			// квотированные символы распознаются только в комментариях и кавычках
			src = " a1\\ 1 (bb2\\)2) \"ccc3\\\"3\\ \",";
			parserPos = 0;
			token = parser.Parse (src, ref parserPos);
			Assert.Equal (StructuredStringTokenType.Value, token.TokenType);
			Assert.Equal (1, token.Position); // 'a1'
			Assert.Equal (2, token.Length);
			token = parser.Parse (src, ref parserPos);
			Assert.Equal (StructuredStringTokenType.Separator, token.TokenType);
			Assert.Equal (3, token.Position); // '\'
			Assert.Equal (1, token.Length);
			token = parser.Parse (src, ref parserPos);
			Assert.Equal (StructuredStringTokenType.Value, token.TokenType);
			Assert.Equal (5, token.Position); // '1'
			Assert.Equal (1, token.Length);
			token = parser.Parse (src, ref parserPos);
			Assert.True (token.IsRoundBracketedValue (src));
			Assert.Equal (7, token.Position); // 'bb2\)2'
			Assert.Equal (8, token.Length);
			token = parser.Parse (src, ref parserPos);
			Assert.True (token.IsDoubleQuotedValue (src));
			Assert.Equal (16, token.Position); // 'ccc3\"3\ '
			Assert.Equal (11, token.Length);
			token = parser.Parse (src, ref parserPos);
			Assert.Equal (StructuredStringTokenType.Separator, token.TokenType);
			Assert.Equal (27, token.Position); // ','
			Assert.Equal (1, token.Length);
			token = parser.Parse (src, ref parserPos);
			Assert.False (token.IsValid);

			// встречаются все типы
			src = "abc,de;fgh \"ijkl\" (mnop)   <rst>\t[uvw]";
			parserPos = 0;
			parser = new StructuredStringParser (AsciiCharClasses.WhiteSpace, AsciiCharClasses.Atom, false, StructuredStringParser.StructuredHeaderFieldBodyFormats);
			token = parser.Parse (src, ref parserPos);
			Assert.Equal (StructuredStringTokenType.Value, token.TokenType);
			Assert.Equal (0, token.Position); // abc
			Assert.Equal (3, token.Length);
			token = parser.Parse (src, ref parserPos);
			Assert.Equal (StructuredStringTokenType.Separator, token.TokenType);
			Assert.Equal (3, token.Position); // ,
			Assert.Equal (1, token.Length);
			token = parser.Parse (src, ref parserPos);
			Assert.Equal (StructuredStringTokenType.Value, token.TokenType);
			Assert.Equal (4, token.Position); // de
			Assert.Equal (2, token.Length);
			token = parser.Parse (src, ref parserPos);
			Assert.Equal (StructuredStringTokenType.Separator, token.TokenType);
			Assert.Equal (6, token.Position); // ;
			Assert.Equal (1, token.Length);
			token = parser.Parse (src, ref parserPos);
			Assert.Equal (StructuredStringTokenType.Value, token.TokenType);
			Assert.Equal (7, token.Position); // fgh
			Assert.Equal (3, token.Length);
			token = parser.Parse (src, ref parserPos);
			Assert.True (token.IsDoubleQuotedValue (src));
			Assert.Equal (11, token.Position); // ijkl
			Assert.Equal (6, token.Length);
			token = parser.Parse (src, ref parserPos);
			Assert.True (token.IsRoundBracketedValue (src));
			Assert.Equal (18, token.Position); // mnop
			Assert.Equal (6, token.Length);
			token = parser.Parse (src, ref parserPos);
			Assert.True (token.IsAngleBracketedValue (src));
			Assert.Equal (27, token.Position); // rst
			Assert.Equal (5, token.Length);
			token = parser.Parse (src, ref parserPos);
			Assert.True (token.IsSquareBracketedValue (src));
			Assert.Equal (33, token.Position); // uvw
			Assert.Equal (5, token.Length);
			token = parser.Parse (src, ref parserPos);
			Assert.False (token.IsValid);
			token = parser.Parse (src, ref parserPos);
			Assert.False (token.IsValid);
		}
	}
}
