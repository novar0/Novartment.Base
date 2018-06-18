using System.Text;
using Novartment.Base.Text;
using Xunit;

namespace Novartment.Base.Net.Mime.Test
{
	public class StructuredValuePhraseDecoderTests
	{
		[Fact]
		[Trait ("Category", "Mime")]
		public void DecodePhrase ()
		{
			var strs = new string[]
			{
				"abc",
				",",
				"=?utf-8?B?INGD0YHQuNC70LXQvdC90YvRhQ==?=",
				"=?us-ascii?q?some_text?=",
				";",
				"i\\\\jkl",
			};
			var src = Encoding.ASCII.GetBytes (string.Join (' ', strs));
			var elements = new StructuredValueElement[]
			{
				new StructuredValueElement (StructuredValueElementType.Value, 0, strs[0].Length),
				new StructuredValueElement (StructuredValueElementType.Separator, strs[0].Length + 1, strs[1].Length),
				new StructuredValueElement (StructuredValueElementType.Value, strs[0].Length + 1 + strs[1].Length + 1, strs[2].Length),
				new StructuredValueElement (StructuredValueElementType.Value, strs[0].Length + 1 + strs[1].Length + 1 + strs[2].Length + 1, strs[3].Length),
				new StructuredValueElement (StructuredValueElementType.Separator, strs[0].Length + 1 + strs[1].Length + 1 + strs[2].Length + 1 + strs[3].Length + 1, strs[4].Length),
				new StructuredValueElement (StructuredValueElementType.QuotedValue, strs[0].Length + 1 + strs[1].Length + 1 + strs[2].Length + 1 + strs[3].Length + 1 + strs[4].Length + 1, strs[5].Length),
			};

			var decoder = new StructuredValuePhraseDecoder ();
			foreach (var element in elements)
			{
				decoder.AddElement (src, element);
			}

			Assert.Equal ("abc ,  усиленныхsome text ; i\\jkl", decoder.GetResult ());
		}
	}
}
