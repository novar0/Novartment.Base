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
			var src = string.Join (' ', strs);
			var elements = new StructuredHeaderFieldLexicalToken[]
			{
				new StructuredHeaderFieldLexicalToken (StructuredHeaderFieldLexicalTokenType.Value, 0, strs[0].Length),
				new StructuredHeaderFieldLexicalToken (StructuredHeaderFieldLexicalTokenType.Separator, strs[0].Length + 1, strs[1].Length),
				new StructuredHeaderFieldLexicalToken (StructuredHeaderFieldLexicalTokenType.Value, strs[0].Length + 1 + strs[1].Length + 1, strs[2].Length),
				new StructuredHeaderFieldLexicalToken (StructuredHeaderFieldLexicalTokenType.Value, strs[0].Length + 1 + strs[1].Length + 1 + strs[2].Length + 1, strs[3].Length),
				new StructuredHeaderFieldLexicalToken (StructuredHeaderFieldLexicalTokenType.Separator, strs[0].Length + 1 + strs[1].Length + 1 + strs[2].Length + 1 + strs[3].Length + 1, strs[4].Length),
				new StructuredHeaderFieldLexicalToken (StructuredHeaderFieldLexicalTokenType.QuotedValue, strs[0].Length + 1 + strs[1].Length + 1 + strs[2].Length + 1 + strs[3].Length + 1 + strs[4].Length + 1, strs[5].Length),
			};

			var decoder = new StructuredHeaderFieldDecoder ();
			foreach (var element in elements)
			{
				decoder.AddElement (src, element);
			}

			Assert.Equal ("abc ,  усиленныхsome text ; i\\jkl", decoder.GetResult ());
		}
	}
}
