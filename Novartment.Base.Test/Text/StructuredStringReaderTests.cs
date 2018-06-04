using System;
using Novartment.Base.Text;
using Xunit;

namespace Novartment.Base.Test
{
	public class StructuredStringReaderTests
	{
		[Fact]
		[Trait ("Category", "Text.StructuredStringReader")]
		public void Parse ()
		{
			var parser = new StructuredStringReader (string.Empty);
			Assert.Equal (0, parser.Position);
			Assert.Equal (-1, parser.NextChar);
			Assert.Equal (-1, parser.NextNextChar);
			Assert.True (parser.IsExhausted);

			var template = " 👍a👎🔧{0🔨жби🔑}-\t";
			parser = new StructuredStringReader (template, 1, template.Length - 2);
			Assert.Equal (1, parser.Position);
			Assert.Equal (0x1F44D, parser.NextChar); // 👍
			Assert.Equal ((int)'a', parser.NextNextChar);
			Assert.False (parser.IsExhausted);
			Assert.Equal (0x1F44D, parser.SkipChar ()); // 👍
			Assert.Equal ((int)'a', parser.SkipChar ());
			Assert.Equal (0x1F44E, parser.NextChar); // 👎
			Assert.Equal (0x1F527, parser.NextNextChar); // 🔧
			Assert.Equal (0x1F44E, parser.SkipChar ()); // 👎
			Assert.Equal (0x1F527, parser.SkipChar ()); // 🔧

			var delimiter = DelimitedElement.CreateBracketed ((int)'(', (int)'}', false);
			Assert.Throws<FormatException> (() => parser.EnsureDelimitedElement (delimiter));
			delimiter = DelimitedElement.CreatePrefixedFixedLength ((int)'=', 20);
			Assert.Throws<FormatException> (() => parser.EnsureDelimitedElement (delimiter));
			delimiter = DelimitedElement.CreatePrefixedFixedLength ((int)'{', 20);
			Assert.Throws<FormatException> (() => parser.EnsureDelimitedElement (delimiter));

			delimiter = DelimitedElement.CreateBracketed ((int)'{', (int)'}', false);
			Assert.Equal (18, parser.EnsureDelimitedElement (delimiter));
			Assert.Equal ((int)'-', parser.NextChar);
			Assert.Equal (-1, parser.NextNextChar);
			Assert.False (parser.IsExhausted);
			Assert.Throws<FormatException> (() => parser.EnsureChar ((int)'='));
			Assert.Equal (19, parser.EnsureChar ((int)'-'));
			Assert.Equal (-1, parser.NextChar);
			Assert.Equal (-1, parser.NextNextChar);
			Assert.True (parser.IsExhausted);
		}
	}
}
