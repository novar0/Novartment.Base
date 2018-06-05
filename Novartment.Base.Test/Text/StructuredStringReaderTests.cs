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
			Assert.Equal (-1, parser.NextCodePoint);
			Assert.Equal (-1, parser.NextNextCodePoint);
			Assert.True (parser.IsExhausted);

			var template = " 👍a👎🔧{0🔨жби🔑}-\t";
			parser = new StructuredStringReader (template, 1, template.Length - 2);
			Assert.Equal (1, parser.Position);
			Assert.Equal (0x1F44D, parser.NextCodePoint); // 👍
			Assert.Equal ((int)'a', parser.NextNextCodePoint);
			Assert.False (parser.IsExhausted);
			Assert.Equal (0x1F44D, parser.SkipCodePoint ()); // 👍
			Assert.Equal ((int)'a', parser.SkipCodePoint ());
			Assert.Equal (0x1F44E, parser.NextCodePoint); // 👎
			Assert.Equal (0x1F527, parser.NextNextCodePoint); // 🔧
			Assert.Equal (0x1F44E, parser.SkipCodePoint ()); // 👎
			Assert.Equal (0x1F527, parser.SkipCodePoint ()); // 🔧

			var delimiter = DelimitedElement.CreateBracketed ((int)'(', (int)'}', false);
			Assert.Throws<FormatException> (() => parser.EnsureDelimitedElement (delimiter));
			delimiter = DelimitedElement.CreatePrefixedFixedLength ((int)'=', 20);
			Assert.Throws<FormatException> (() => parser.EnsureDelimitedElement (delimiter));
			delimiter = DelimitedElement.CreatePrefixedFixedLength ((int)'{', 20);
			Assert.Throws<FormatException> (() => parser.EnsureDelimitedElement (delimiter));

			delimiter = DelimitedElement.CreateBracketed ((int)'{', (int)'}', false);
			Assert.Equal (18, parser.EnsureDelimitedElement (delimiter));
			Assert.Equal ((int)'-', parser.NextCodePoint);
			Assert.Equal (-1, parser.NextNextCodePoint);
			Assert.False (parser.IsExhausted);
			Assert.Throws<FormatException> (() => parser.EnsureCodePoint ((int)'='));
			parser.EnsureCodePoint ((int)'-');
			Assert.Equal (-1, parser.NextCodePoint);
			Assert.Equal (-1, parser.NextNextCodePoint);
			Assert.True (parser.IsExhausted);
		}
	}
}
