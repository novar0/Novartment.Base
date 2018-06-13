using System;
using Novartment.Base.Text;
using Novartment.Base.Text.CharSpanExtensions;
using Xunit;

namespace Novartment.Base.Test
{
	public class UnicodeCodePointReaderTests
	{
		private readonly string _template = "👍a👎🔧{0🔨жби🔑}-\t";

		[Fact]
		[Trait ("Category", "Text.CodePointReader")]
		public void EnsureCodePoint ()
		{
			Assert.Throws<FormatException> (() => UnicodeCodePointReader.EnsureCodePoint (default (ReadOnlySpan<char>), (int)'='));
			Assert.Throws<FormatException> (() => UnicodeCodePointReader.EnsureCodePoint (_template.AsSpan (), (int)'='));

			var t0 = _template.AsSpan ().Slice (0, 8);
			var str1 = UnicodeCodePointReader.EnsureCodePoint (t0, 0x1F44D);
			var str2 = UnicodeCodePointReader.EnsureCodePoint (str1, (int)'a');
			var str3 = UnicodeCodePointReader.EnsureCodePoint (str2, 0x1F44E);
			var str4 = UnicodeCodePointReader.EnsureCodePoint (str3, 0x1F527);
			var str5 = UnicodeCodePointReader.EnsureCodePoint (str4, (int)'{');
			Assert.Equal (0, str5.Length); // end of string
		}

		[Fact]
		[Trait ("Category", "Text.CodePointReader")]
		public void GetAndSkipCodePoint ()
		{
			var emptyStr = default (ReadOnlySpan<char>);
			Assert.Equal (-1, UnicodeCodePointReader.GetFirstCodePoint (emptyStr));
			Assert.Equal (-1, UnicodeCodePointReader.GetSecondCodePoint (emptyStr));

			var t0 = _template.AsSpan ().Slice (0, 8);
			Assert.Equal (0x1F44D, UnicodeCodePointReader.GetFirstCodePoint (t0)); // 👍
			Assert.Equal ((int)'a', UnicodeCodePointReader.GetSecondCodePoint (t0));
			var t1 = UnicodeCodePointReader.SkipCodePoint (t0);
			Assert.Equal ((int)'a', UnicodeCodePointReader.GetFirstCodePoint (t1)); // a
			Assert.Equal (0x1F44E, UnicodeCodePointReader.GetSecondCodePoint (t1)); // 👎
			var t2 = UnicodeCodePointReader.SkipCodePoint (t1);
			Assert.Equal (0x1F44E, UnicodeCodePointReader.GetFirstCodePoint (t2)); // 👎
			Assert.Equal (0x1F527, UnicodeCodePointReader.GetSecondCodePoint (t2)); // 🔧
			var t3 = UnicodeCodePointReader.SkipCodePoint (t2);
			Assert.Equal (0x1F527, UnicodeCodePointReader.GetFirstCodePoint (t3)); // 🔧
			Assert.Equal ((int)'{', UnicodeCodePointReader.GetSecondCodePoint (t3)); // {
			var t4 = UnicodeCodePointReader.SkipCodePoint (t3);
			Assert.Equal ((int)'{', UnicodeCodePointReader.GetFirstCodePoint (t4)); // {
			Assert.Equal (-1, UnicodeCodePointReader.GetSecondCodePoint (t4)); // end of string
			var t5 = UnicodeCodePointReader.SkipCodePoint (t4);
			Assert.Equal (-1, UnicodeCodePointReader.GetFirstCodePoint (t5)); // end of string
			Assert.Equal (-1, UnicodeCodePointReader.GetSecondCodePoint (t5)); // end of string
			var t6 = UnicodeCodePointReader.SkipCodePoint (t5);
			Assert.Equal (-1, UnicodeCodePointReader.GetFirstCodePoint (t6)); // end of string
			Assert.Equal (-1, UnicodeCodePointReader.GetSecondCodePoint (t6)); // end of string
		}
	}
}
