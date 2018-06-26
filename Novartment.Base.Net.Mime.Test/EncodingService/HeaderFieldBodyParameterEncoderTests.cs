using Xunit;

namespace Novartment.Base.Net.Mime.Test
{
	public class HeaderFieldBodyParameterEncoderTests
	{
		[Fact]
		[Trait ("Category", "Mime")]
		public void Parse ()
		{
			/*
			var segments = HeaderFieldBodyParameterEncoder.Parse ("charset", "koi8-r");
			Assert.Equal ("charset=koi8-r", segments.GetNextSegment ());
			Assert.Null (segments.GetNextSegment ());

			segments = HeaderFieldBodyParameterEncoder.Parse ("charset", "koi8 r");
			Assert.Equal ("charset=\"koi8 r\"", segments.GetNextSegment ());
			Assert.Null (segments.GetNextSegment ());

			segments = HeaderFieldBodyParameterEncoder.Parse ("charset", "функции");
			Assert.Equal ("charset*0*=utf-8''%D1%84%D1%83%D0%BD%D0%BA%D1%86%D0%B8%D0%B8", segments.GetNextSegment ());
			Assert.Null (segments.GetNextSegment ());

			segments = HeaderFieldBodyParameterEncoder.Parse ("filename", "This document specifies an Internet standards track protocol for the функции and requests discussion and suggestions.txt");
			Assert.Equal ("filename*0*=utf-8''This%20document%20specifies%20an%20Internet%20standards;", segments.GetNextSegment ());
			Assert.Equal ("filename*1=\" track protocol for the \";", segments.GetNextSegment ());
			Assert.Equal ("filename*2*=%D1%84%D1%83%D0%BD%D0%BA%D1%86%D0%B8%D0%B8;", segments.GetNextSegment ());
			Assert.Equal ("filename*3=\" and requests discussion and suggestions.txt\"", segments.GetNextSegment ());
			Assert.Null (segments.GetNextSegment ());*/
		}
	}
}
