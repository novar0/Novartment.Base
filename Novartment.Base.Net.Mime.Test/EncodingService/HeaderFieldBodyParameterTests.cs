using System.Text;
using Xunit;

namespace Novartment.Base.Net.Mime.Test
{
	public sealed class HeaderFieldBodyParameterTests
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

			template = "   (Plain text) ;\tformat=flowed   (Plain text)";
			pos = 0;
			par = HeaderFieldBodyParameter.Parse (template, buf, ref pos);
			Assert.NotNull (par);
			Assert.Equal ("format", par.Name);
			Assert.Equal ("flowed", par.Value);
			par = HeaderFieldBodyParameter.Parse (template, buf, ref pos);
			Assert.Null (par);
			par = HeaderFieldBodyParameter.Parse (template, buf, ref pos);
			Assert.Null (par);

			template = ";\tformat=flowed;\tcharset=\"us-ascii\";\treply-type=original (Plain text)";
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

			template = ";\tfilename*0*=windows-1251''This%20document%20specifies%20an;\tfilename*1=\" Internet standards track protocol for the \";\tfilename*2*=%F4%F3%ED%EA%F6%E8%E8;\tfilename*3=\" and requests discussion and suggestions.txt\";\tmodification-date=\"Thu, 24 Nov 2011 09:48:27 +0700\";\tcreation-date=\"Tue, 10 Jul 2012 10:01:06 +0600\";\tread-date=\"Wed, 11 Jul 2012 10:40:13 +0600\";\tsize=\"318\" (Plain text)";
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

			// тест особой ситуации ('encoded-word' внутри 'quoted-string' запрещено правилами) для совместимости с IBM Notes
			template = "; filename=\"=?KOI8-R?B?79DF0sHUydfO2cog0sHQz9LUIDIwMjAueGxzeA==?=\"";
			pos = 0;
			par = HeaderFieldBodyParameter.Parse (template, buf, ref pos);
			Assert.NotNull (par);
			Assert.Equal ("filename", par.Name);
			Assert.Equal ("Оперативный рапорт 2020.xlsx", par.Value);
			par = HeaderFieldBodyParameter.Parse (template, buf, ref pos);
			Assert.Null (par);
			par = HeaderFieldBodyParameter.Parse (template, buf, ref pos);
			Assert.Null (par);

			template = "; filename=\"=?UTF-8?B?0KLQoC0xNTEg0L7RgiAyOS4wNy4yMDIwINCe?= =?UTF-8?B?INC/0YDQuNC80LXQvdC10L3QuNC4INGD0YLQtdC/0LvRj9GO0YnQtdC5?= =?UTF-8?B?INGB0LzQtdGB0Lgg0L3QsCDQvtGB0L3QvtCy0LUg0YjQsNC80L7RgtCwICg=?= =?UTF-8?B?0KjQmtCSKSDQstC30LDQvNC10L0g0LvRjtC90LrQtdGA0LjRgtCw?= =?UTF-8?B?INC/0YDQuCDRgNCw0LfQu9C40LLQutC1INC/0L4g0LfQsNC60LDQt9Cw0Lw=?= =?UTF-8?B?INCf0JDQniDCq9Cj0YDQsNC70LrRg9C3wrsg0Lgg0L/RgNC+0LrQsNGC0LAu?= =?UTF-8?B?c3Fs?=\"";
			pos = 0;
			par = HeaderFieldBodyParameter.Parse (template, buf, ref pos);
			Assert.NotNull (par);
			Assert.Equal ("filename", par.Name);
			Assert.Equal ("ТР-151 от 29.07.2020 О применении утепляющей смеси на основе шамота (ШКВ) взамен люнкерита при разливке по заказам ПАО «Уралкуз» и проката.sql", par.Value);
			par = HeaderFieldBodyParameter.Parse (template, buf, ref pos);
			Assert.Null (par);
			par = HeaderFieldBodyParameter.Parse (template, buf, ref pos);
			Assert.Null (par);
		}
	}
}
