using System.Text;
using Xunit;

namespace Novartment.Base.Net.Mime.Test
{
	public class HeaderFieldBodyParameterTests
	{
		public HeaderFieldBodyParameterTests ()
		{
			Encoding.RegisterProvider (CodePagesEncodingProvider.Instance);
		}

		[Fact]
		[Trait ("Category", "Mime")]
		public void Parse ()
		{
			var buf = new char[1000];
			var template = " \t";
			var pos = 0;
			var par = HeaderFieldBodyParameter.Parse (template, buf, ref pos);
			Assert.Null (par);

			template = ";\tformat=flowed  ";
			pos = 0;
			par = HeaderFieldBodyParameter.Parse (template, buf, ref pos);
			Assert.NotNull (par);
			Assert.Equal ("format", par.Name);
			Assert.Equal ("flowed", par.Value);
			par = HeaderFieldBodyParameter.Parse (template, buf, ref pos);
			Assert.Null (par);
			par = HeaderFieldBodyParameter.Parse (template, buf, ref pos);
			Assert.Null (par);

			template = ";\tformat=flowed;\tcharset=\"us-ascii\";\treply-type=original";
			pos = 0;
			par = HeaderFieldBodyParameter.Parse (template, buf, ref pos);
			Assert.NotNull (par);
			Assert.Equal ("format", par.Name);
			Assert.Equal ("flowed", par.Value);
			par = HeaderFieldBodyParameter.Parse (template, buf, ref pos);
			Assert.NotNull (par);
			Assert.Equal ("charset", par.Name);
			Assert.Equal ("us-ascii", par.Value);
			par = HeaderFieldBodyParameter.Parse (template, buf, ref pos);
			Assert.NotNull (par);
			Assert.Equal ("reply-type", par.Name);
			Assert.Equal ("original", par.Value);
			par = HeaderFieldBodyParameter.Parse (template, buf, ref pos);
			Assert.Null (par);
			par = HeaderFieldBodyParameter.Parse (template, buf, ref pos);
			Assert.Null (par);

			template = ";\tfilename*0*=windows-1251''This%20document%20specifies%20an;\tfilename*1=\" Internet standards track protocol for the \";\tfilename*2*=%F4%F3%ED%EA%F6%E8%E8;\tfilename*3=\" and requests discussion and suggestions.txt\";\tmodification-date=\"Thu, 24 Nov 2011 09:48:27 +0700\";\tcreation-date=\"Tue, 10 Jul 2012 10:01:06 +0600\";\tread-date=\"Wed, 11 Jul 2012 10:40:13 +0600\";\tsize=\"318\"";
			pos = 0;
			par = HeaderFieldBodyParameter.Parse (template, buf, ref pos);
			Assert.NotNull (par);
			Assert.Equal ("filename", par.Name);
			Assert.Equal ("This document specifies an Internet standards track protocol for the функции and requests discussion and suggestions.txt", par.Value);
			par = HeaderFieldBodyParameter.Parse (template, buf, ref pos);
			Assert.NotNull (par);
			Assert.Equal ("modification-date", par.Name);
			Assert.Equal ("Thu, 24 Nov 2011 09:48:27 +0700", par.Value);
			par = HeaderFieldBodyParameter.Parse (template, buf, ref pos);
			Assert.NotNull (par);
			Assert.Equal ("creation-date", par.Name);
			Assert.Equal ("Tue, 10 Jul 2012 10:01:06 +0600", par.Value);
			par = HeaderFieldBodyParameter.Parse (template, buf, ref pos);
			Assert.NotNull (par);
			Assert.Equal ("read-date", par.Name);
			Assert.Equal ("Wed, 11 Jul 2012 10:40:13 +0600", par.Value);
			par = HeaderFieldBodyParameter.Parse (template, buf, ref pos);
			Assert.NotNull (par);
			Assert.Equal ("size", par.Name);
			Assert.Equal ("318", par.Value);
			par = HeaderFieldBodyParameter.Parse (template, buf, ref pos);
			Assert.Null (par);
			par = HeaderFieldBodyParameter.Parse (template, buf, ref pos);
			Assert.Null (par);
		}
	}
}
