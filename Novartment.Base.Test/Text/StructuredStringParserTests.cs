using System;
using Novartment.Base.Text;
using Xunit;

namespace Novartment.Base.Test
{
	public sealed class StructuredStringParserTests
	{
		internal sealed class TokenFormatQuotedString : StructuredStringTokenDelimitedFormat
		{
			internal TokenFormatQuotedString () : base ('\"', '\"', StructuredStringIngoreTokenType.EscapedChar, false) { }
		}

		internal sealed class TokenFormatComment : StructuredStringTokenDelimitedFormat
		{
			internal TokenFormatComment () : base ('(', ')', StructuredStringIngoreTokenType.EscapedChar, true) { }
		}

		internal sealed class TokenFormatLiteral : StructuredStringTokenDelimitedFormat
		{
			internal TokenFormatLiteral () : base ('[', ']', StructuredStringIngoreTokenType.EscapedChar, false) { }
		}

		internal sealed class TokenFormatId : StructuredStringTokenDelimitedFormat
		{
			internal TokenFormatId () : base ('<', '>', StructuredStringIngoreTokenType.QuotedValue, false) { }
		}

		[Fact]
		[Trait ("Category", "Text.StructuredStringToken")]
		public void Parse ()
		{
			var allFormats = new StructuredStringTokenDelimitedFormat[] { new TokenFormatQuotedString (), new TokenFormatComment (), new TokenFormatLiteral (), new TokenFormatId () };
			ReadOnlySpan<char> src = default;
			var parserPos = 0;
			var format = new StructuredStringFormat (AsciiCharClasses.WhiteSpace, AsciiCharClasses.Atom, char.MaxValue, null);
			var token = format.ParseToken (src, ref parserPos);
			Assert.Null (token.Format);

			src = "(a1)  bb2\t(c3 (d4)) eee5\t\t(ff6)";
			parserPos = 0;
			format = new StructuredStringFormat (AsciiCharClasses.WhiteSpace, AsciiCharClasses.Token, char.MaxValue, allFormats);
			token = format.ParseToken (src, ref parserPos);
			Assert.IsType<TokenFormatComment> (token.Format);
			Assert.Equal (0, token.Position); // a1
			Assert.Equal (4, token.Length);
			token = format.ParseToken (src, ref parserPos);
			Assert.IsType<StructuredStringTokenValueFormat> (token.Format);
			Assert.Equal (6, token.Position); // bb2
			Assert.Equal (3, token.Length);
			token = format.ParseToken (src, ref parserPos);
			Assert.IsType<TokenFormatComment> (token.Format);
			Assert.Equal (10, token.Position); // (c3 (d4))
			Assert.Equal (9, token.Length);
			token = format.ParseToken (src, ref parserPos);
			Assert.IsType<StructuredStringTokenValueFormat> (token.Format);
			Assert.Equal (20, token.Position); // eee5
			Assert.Equal (4, token.Length);
			token = format.ParseToken (src, ref parserPos);
			Assert.IsType<TokenFormatComment> (token.Format);
			Assert.Equal (26, token.Position); // ff6
			Assert.Equal (5, token.Length);
			token = format.ParseToken (src, ref parserPos);
			Assert.Null (token.Format);

			// квотированные символы распознаются только в комментариях и кавычках
			src = " a1\\ 1 (bb2\\)2) \"ccc3\\\"3\\ \",";
			parserPos = 0;
			token = format.ParseToken (src, ref parserPos);
			Assert.IsType<StructuredStringTokenValueFormat> (token.Format);
			Assert.Equal (1, token.Position); // 'a1'
			Assert.Equal (2, token.Length);
			token = format.ParseToken (src, ref parserPos);
			Assert.IsType<StructuredStringTokenSeparatorFormat> (token.Format);
			Assert.Equal (3, token.Position); // '\'
			Assert.Equal (1, token.Length);
			token = format.ParseToken (src, ref parserPos);
			Assert.IsType<StructuredStringTokenValueFormat> (token.Format);
			Assert.Equal (5, token.Position); // '1'
			Assert.Equal (1, token.Length);
			token = format.ParseToken (src, ref parserPos);
			Assert.IsType<TokenFormatComment> (token.Format);
			Assert.Equal (7, token.Position); // 'bb2\)2'
			Assert.Equal (8, token.Length);
			token = format.ParseToken (src, ref parserPos);
			Assert.IsType<TokenFormatQuotedString> (token.Format);
			Assert.Equal (16, token.Position); // 'ccc3\"3\ '
			Assert.Equal (11, token.Length);
			token = format.ParseToken (src, ref parserPos);
			Assert.IsType<StructuredStringTokenSeparatorFormat> (token.Format);
			Assert.Equal (27, token.Position); // ','
			Assert.Equal (1, token.Length);
			token = format.ParseToken (src, ref parserPos);
			Assert.Null (token.Format);

			// встречаются все типы
			src = "abc,de;fgh \"ijkl\" (mnop)   <rst>\t[uvw]";
			parserPos = 0;
			format = new StructuredStringFormat (AsciiCharClasses.WhiteSpace, AsciiCharClasses.Atom, char.MaxValue, allFormats);
			token = format.ParseToken (src, ref parserPos);
			Assert.IsType<StructuredStringTokenValueFormat> (token.Format);
			Assert.Equal (0, token.Position); // abc
			Assert.Equal (3, token.Length);
			token = format.ParseToken (src, ref parserPos);
			Assert.IsType<StructuredStringTokenSeparatorFormat> (token.Format);
			Assert.Equal (3, token.Position); // ,
			Assert.Equal (1, token.Length);
			token = format.ParseToken (src, ref parserPos);
			Assert.IsType<StructuredStringTokenValueFormat> (token.Format);
			Assert.Equal (4, token.Position); // de
			Assert.Equal (2, token.Length);
			token = format.ParseToken (src, ref parserPos);
			Assert.IsType<StructuredStringTokenSeparatorFormat> (token.Format);
			Assert.Equal (6, token.Position); // ;
			Assert.Equal (1, token.Length);
			token = format.ParseToken (src, ref parserPos);
			Assert.IsType<StructuredStringTokenValueFormat> (token.Format);
			Assert.Equal (7, token.Position); // fgh
			Assert.Equal (3, token.Length);
			token = format.ParseToken (src, ref parserPos);
			Assert.IsType<TokenFormatQuotedString> (token.Format);
			Assert.Equal (11, token.Position); // ijkl
			Assert.Equal (6, token.Length);
			token = format.ParseToken (src, ref parserPos);
			Assert.IsType<TokenFormatComment> (token.Format);
			Assert.Equal (18, token.Position); // mnop
			Assert.Equal (6, token.Length);
			token = format.ParseToken (src, ref parserPos);
			Assert.IsType<TokenFormatId> (token.Format);
			Assert.Equal (27, token.Position); // rst
			Assert.Equal (5, token.Length);
			token = format.ParseToken (src, ref parserPos);
			Assert.IsType<TokenFormatLiteral> (token.Format);
			Assert.Equal (33, token.Position); // uvw
			Assert.Equal (5, token.Length);
			token = format.ParseToken (src, ref parserPos);
			Assert.Null (token.Format);
			token = format.ParseToken (src, ref parserPos);
			Assert.Null (token.Format);
		}
	}
}
