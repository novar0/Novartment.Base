using System;
using Novartment.Base.Text;
using Xunit;

namespace Novartment.Base.Test
{
	public class StringExtensionsTests
	{
		[Fact]
		[Trait ("Category", "Text.StringExtensions")]
		public void AppendSeparator ()
		{
			var src = Array.Empty<string> ();
			var result = StringExtensions.AppendSeparator (src, ' ');
			Assert.Equal (0, result.Count);

			src = new string[] { "value" };
			result = StringExtensions.AppendSeparator (src, ' ');
			Assert.Equal (1, result.Count);
			Assert.Equal ("value", result[0]);

			src = new string[] { "value1", "value2" };
			result = StringExtensions.AppendSeparator (src, ';');
			Assert.Equal (2, result.Count);
			Assert.Equal ("value1;", result[0]);
			Assert.Equal ("value2", result[1]);
		}

		[Fact]
		[Trait ("Category", "Text.StringExtensions")]
		public void GetCodePoint ()
		{
			int pos = 0;
			Assert.Throws<ArgumentOutOfRangeException> (() => StringExtensions.GetCodePoint (default (ReadOnlySpan<char>), ref pos));

			var template = "👍A👎ж\t ";

			pos = -1;
			Assert.Throws<ArgumentOutOfRangeException> (() => StringExtensions.GetCodePoint (template, ref pos));
			pos = template.Length;
			Assert.Throws<ArgumentOutOfRangeException> (() => StringExtensions.GetCodePoint (template, ref pos));

			pos = 0;
			Assert.Equal (0x1F44D, StringExtensions.GetCodePoint (template, ref pos)); // 👍
			Assert.Equal (2, pos);
			Assert.Equal (0x0041, StringExtensions.GetCodePoint (template, ref pos)); // A
			Assert.Equal (3, pos);
			Assert.Equal (0x1F44E, StringExtensions.GetCodePoint (template, ref pos)); // 👎
			Assert.Equal (5, pos);
			Assert.Equal (0x0436, StringExtensions.GetCodePoint (template, ref pos)); // ж
			Assert.Equal (6, pos);
			Assert.Equal (0x0009, StringExtensions.GetCodePoint (template, ref pos)); // \t
			Assert.Equal (7, pos);
			Assert.Equal (0x0020, StringExtensions.GetCodePoint (template, ref pos)); // space
			Assert.Equal (template.Length, pos);
			Assert.Throws<ArgumentOutOfRangeException> (() => StringExtensions.GetCodePoint (template, ref pos));
		}
	}
}
