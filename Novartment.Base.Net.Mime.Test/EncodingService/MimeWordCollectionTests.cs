using System.Text;
using Xunit;

namespace Novartment.Base.Net.Mime.Test
{
	public class MimeWordCollectionTests
	{
		[Fact]
		[Trait ("Category", "Mime")]
		public void ParseWords ()
		{
			var words = new MimeWordCollection (999);
			var templBytes = Encoding.UTF8.GetBytes (string.Empty);
			words.ParseWords (templBytes, false, 999);
			Assert.Equal (0, words.Count);

			templBytes = Encoding.UTF8.GetBytes ("a");
			words.ParseWords (templBytes, false, 999);
			Assert.Equal (1, words.Count);
			Assert.Equal (0, words.WordPositions[0]);
			Assert.Equal (false, words.WordEncodeNeeds[0]);

			templBytes = Encoding.UTF8.GetBytes (" a  ");
			words.ParseWords (templBytes, false, 999);
			Assert.Equal (1, words.Count);
			Assert.Equal (0, words.WordPositions[0]);
			Assert.Equal (false, words.WordEncodeNeeds[0]);

			templBytes = Encoding.UTF8.GetBytes ("a \x3");
			words.ParseWords (templBytes, false, 999);
			Assert.Equal (2, words.Count);
			Assert.Equal (0, words.WordPositions[0]);
			Assert.Equal (false, words.WordEncodeNeeds[0]);
			Assert.Equal (1, words.WordPositions[1]);
			Assert.Equal (true, words.WordEncodeNeeds[1]);

			templBytes = Encoding.UTF8.GetBytes (" \t ab\t\tcde\t \tf \t");
			words.ParseWords (templBytes, false, 999);
			Assert.Equal (3, words.Count);
			Assert.Equal (0, words.WordPositions[0]);
			Assert.Equal (false, words.WordEncodeNeeds[0]);
			Assert.Equal (5, words.WordPositions[1]);
			Assert.Equal (false, words.WordEncodeNeeds[1]);
			Assert.Equal (10, words.WordPositions[2]);
			Assert.Equal (false, words.WordEncodeNeeds[2]);
			words.ParseWords (templBytes, true, 999);
			Assert.Equal (2, words.Count);
			Assert.Equal (0, words.WordPositions[0]);
			Assert.Equal (false, words.WordEncodeNeeds[0]);
			Assert.Equal (11, words.WordPositions[1]);
			Assert.Equal (false, words.WordEncodeNeeds[1]);
		}
	}
}
