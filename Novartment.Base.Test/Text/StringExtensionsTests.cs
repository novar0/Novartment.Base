using System;
using Novartment.Base.Text;
using Xunit;

namespace Novartment.Base.Test
{
	public class StringExtensionsTests
	{
		[Fact]
		[Trait ("Category", "Text.StringExtensions")]
		public void Replace ()
		{
			Assert.Equal (
				"у нее было КРАСИВОЕ лицо",
				"у нее было КРАСИВОЕ лицо".Replace ("красивое", "страшное", StringComparison.Ordinal));
			Assert.Equal (
				"у нее было страшное лицо",
				"у нее было КРАСИВОЕ лицо".Replace ("красивое", "страшное", StringComparison.OrdinalIgnoreCase));
			Assert.Equal (
				"12345\r\n123*^& 123*^&789 *^&",
				"12345\r\n123456 123456789 456".Replace ("456", "*^&", StringComparison.OrdinalIgnoreCase));
		}

		[Fact]
		[Trait ("Category", "Text.StringExtensions")]
		public void AppendSeparator ()
		{
			var src = new string[0];
			var result = src.AppendSeparator (' ');
			Assert.Equal (0, result.Count);

			src = new string[] { "value" };
			result = src.AppendSeparator (' ');
			Assert.Equal (1, result.Count);
			Assert.Equal ("value", result[0]);

			src = new string[] { "value1", "value2" };
			result = src.AppendSeparator (';');
			Assert.Equal (2, result.Count);
			Assert.Equal ("value1;", result[0]);
			Assert.Equal ("value2", result[1]);
		}
	}
}
