using System;
using System.Collections.Generic;
using System.Text;
using Novartment.Base.BinaryStreaming;
using Xunit;
using static System.Linq.Enumerable;

namespace Novartment.Base.Net.Mime.Test
{
	public class EntityTests
	{
		public EntityTests ()
		{
			Encoding.RegisterProvider (CodePagesEncodingProvider.Instance);
		}

		[Fact]
		[Trait ("Category", "Mime")]
		public void LoadDefaultValues ()
		{
			var textTemplate1 = "Subject: testing1\r\nDate: Tue, 15 May 2012 14:16:26 +0600\r\nFrom: one@server.com\r\nTo: y@server.org\r\n\r\nHello!\r\n";
			var textTemplate2 = "Subject: testing2\r\nDate: Tue, 15 May 2013 16:16:26 +0600\r\nFrom: two@server.com\r\nTo: z@server.org\r\n\r\nEhllo!\r\n";

			// для multipart/mixed вложенные части, для которых не указан Content-Type, должны получаться text/plain
			var defaultTextTypeTemplate = new string[]
			{
				"Content-type: multipart/mixed; boundary=nextpart",
				string.Empty,
				"--nextpart",
				string.Empty,
				string.Empty,
				textTemplate1,
				"--nextpart",
				string.Empty,
				string.Empty,
				textTemplate2,
				"--nextpart--",
			};

			var srcText = string.Join ("\r\n", defaultTextTypeTemplate);
			var src = new MemoryBufferedSource (Encoding.ASCII.GetBytes (srcText));
			var entity = new Entity ();
			entity.LoadAsync (src, EntityBodyFactory.Create, Entity.DefaultType, Entity.DefaultSubtype).Wait ();

			Assert.Equal (ContentMediaType.Multipart, entity.MediaType);
			Assert.IsAssignableFrom<ICompositeEntityBody> (entity.Body);
			var rootBody = (ICompositeEntityBody)entity.Body;
			Assert.Equal (2, rootBody.Parts.Count);

			var part1 = rootBody.Parts[0];
			Assert.Equal (ContentMediaType.Unspecified, part1.MediaType);
			Assert.Null (part1.MediaSubtype);
			Assert.Equal (ContentTransferEncoding.SevenBit, part1.RequiredTransferEncoding);
			Assert.IsType<TextEntityBody> (part1.Body);
			var body1 = (TextEntityBody)part1.Body;
			var text1 = body1.GetText ();
			Assert.Equal (textTemplate1, text1);

			var part2 = rootBody.Parts[1];
			Assert.Equal (ContentMediaType.Unspecified, part2.MediaType);
			Assert.Null (part2.MediaSubtype);
			Assert.Equal (ContentTransferEncoding.SevenBit, part2.RequiredTransferEncoding);
			Assert.IsType<TextEntityBody> (part2.Body);
			var body2 = (TextEntityBody)part2.Body;
			var text2 = body2.GetText ();
			Assert.Equal (textTemplate2, text2);

			// для multipart/digest вложенные части, для которых не указан Content-Type, должны получаться message/rfc822
			var defaultMessagetTypeTemplate = new string[]
			{
				"Content-type: multipart/digest; boundary=nextpart",
				string.Empty,
				"--nextpart",
				string.Empty,
				string.Empty,
				textTemplate1,
				"--nextpart",
				string.Empty,
				string.Empty,
				textTemplate2,
				"--nextpart--",
			};

			srcText = string.Join ("\r\n", defaultMessagetTypeTemplate);
			src = new MemoryBufferedSource (Encoding.ASCII.GetBytes (srcText));
			entity = new Entity ();
			entity.LoadAsync (src, EntityBodyFactory.Create, Entity.DefaultType, Entity.DefaultSubtype).Wait ();

			Assert.Equal (ContentMediaType.Multipart, entity.MediaType);
			Assert.IsAssignableFrom<ICompositeEntityBody> (entity.Body);
			rootBody = (ICompositeEntityBody)entity.Body;
			Assert.Equal (2, rootBody.Parts.Count);

			part1 = rootBody.Parts[0];
			Assert.Equal (ContentMediaType.Unspecified, part1.MediaType);
			Assert.Null (part1.MediaSubtype);
			Assert.Equal (ContentTransferEncoding.SevenBit, part1.RequiredTransferEncoding);
			Assert.IsType<MessageEntityBody> (part1.Body);
			var msgBody1 = (MessageEntityBody)part1.Body;
			Assert.Equal ("testing1", msgBody1.Message.Subject);

			part2 = rootBody.Parts[1];
			Assert.Equal (ContentMediaType.Unspecified, part2.MediaType);
			Assert.Null (part2.MediaSubtype);
			Assert.Equal (ContentTransferEncoding.SevenBit, part2.RequiredTransferEncoding);
			Assert.IsType<MessageEntityBody> (part2.Body);
			var msgBody2 = (MessageEntityBody)part2.Body;
			Assert.Equal ("testing2", msgBody2.Message.Subject);
		}

		[Fact]
		[Trait ("Category", "Mime")]
		public void Load ()
		{
			string template1 =
				"Content-Type: text/plain;\tformat=flowed;\tcharset=\"us-ascii\";\treply-type=original\r\n" +
				"Content-Disposition: attachment;\tfilename*0*=windows-1251''This%20document%20specifies%20an;\tfilename*1=\" Internet standards track protocol for the \";\tfilename*2*=%F4%F3%ED%EA%F6%E8%E8;\tfilename*3=\" and requests discussion and suggestions.txt\";\tmodification-date=\"Thu, 24 Nov 2011 09:48:27 +0700\";\tcreation-date=\"Tue, 10 Jul 2012 10:01:06 +0600\";\tread-date=\"Wed, 11 Jul 2012 10:40:13 +0600\";\tsize=\"318\"\r\n" +
				"Content-Transfer-Encoding: 8bit\r\n" +
				"Content-MD5: Q2hlY2sgSW50ZWdyaXR5IQ==\r\n" +
				"Content-ID: <201205150149.CAA11933@server10.shop3.company.com>\r\n" +
				"X-Priority: 3\r\n" +
				"X-MimeOLE: Produced By Microsoft MimeOLE V6.00.3790.4913\r\n" +
				"Accept-Language: ru-ru,ru;q=0.8,en-us;q=0.5,en;q=0.3\r\n" +
				"Content-Description: =?utf-8?B?0YLQtdC80LAg0YHQvtC+0LHRidC10L3QuNGPINGC0LXQutGB0YIg0YHQvtC+0LHRidC10L3QuNGP?=\t=?utf-8?B?INC/0LDRgNGB0LjQvCDRgdC+0L7QsdGJ0LXQvdC40LUg0YLQtdC80LAg0YHQvtC+0LHRidC10L0=?=\t=?utf-8?B?0LjRjyDRgtC10LrRgdGCINGB0L7QvtCx0YnQtdC90LjRjyDQv9Cw0YDRgdC40Lwg0YHQvtC+0LE=?=\t=?utf-8?B?0YnQtdC90LjQtQ==?=\r\n" +
				"Content-Duration: 12387\r\n" +
				"Content-Base: http://www.ietf.cnri.reston.va.us/images/\r\n" +
				"Content-Location: foo1.bar1\r\n" +
				"Content-Quality: 0.3 (poor)\r\n" +
				"Content-features: (& (color=Binary)\t(image-file-structure=TIFF-limited)\t(dpi=400)\t(dpi-xyratio=1)\t(paper-size=A4)\t(image-coding=MMR)\t(MRC-mode=0)\t(ua-media=stationery) )\r\n" +
				"Content-alternative: (& (color=Binary)\t(image-file-structure=TIFF-minimal)\t(dpi=200)\t(dpi-xyratio=1)\t(paper-size=A4)\t(image-coding=MH)\t(MRC-mode=0)\t(ua-media=stationery) )\r\n" +
				"Content-alternative: (& (color=Binary)\t(image-file-structure=TIFF-minimal)\t(dpi=100)\t(dpi-xyratio=1)\t(paper-size=A5)\t(image-coding=MH)\t(MRC-mode=0)\t(ua-media=stationery) )\r\n" +
				"Content-Language: i-mingo, de, el, en (This is a dictionary), fr, it\r\n\r\n" +
				"some text";

			var entity = new Entity ();
			entity.LoadAsync (
				new MemoryBufferedSource (Encoding.ASCII.GetBytes (template1)),
				parameters => new TextEntityBody (Encoding.ASCII, parameters.TransferEncoding),
				Entity.DefaultType,
				Entity.DefaultSubtype).Wait ();

			Assert.Equal (ContentMediaType.Text, entity.MediaType);
			Assert.Equal ("plain", entity.MediaSubtype);
			Assert.Equal (ContentDispositionType.Attachment, entity.DispositionType);
			Assert.Equal ("This document specifies an Internet standards track protocol for the функции and requests discussion and suggestions.txt", entity.FileName);
			Assert.Equal (318, entity.Size);
			Assert.Equal (ContentTransferEncoding.EightBit, entity.RequiredTransferEncoding);
			Assert.Equal (634775112660000000L, entity.CreationDate.Value.Ticks);
			Assert.Equal (634577249070000000L, entity.ModificationDate.Value.Ticks);
			Assert.Equal (634776000130000000L, entity.ReadDate.Value.Ticks);
			Assert.Equal (16, entity.MD5.Length);
			var md5 = new byte[16];
			entity.MD5.CopyTo (md5.AsMemory ());
			Assert.Equal (0x43, md5[0]);
			Assert.Equal (0x68, md5[1]);
			Assert.Equal (0x65, md5[2]);
			Assert.Equal (0x63, md5[3]);
			Assert.Equal (0x6b, md5[4]);
			Assert.Equal (0x20, md5[5]);
			Assert.Equal (0x49, md5[6]);
			Assert.Equal (0x6e, md5[7]);
			Assert.Equal (0x74, md5[8]);
			Assert.Equal (0x65, md5[9]);
			Assert.Equal (0x67, md5[10]);
			Assert.Equal (0x72, md5[11]);
			Assert.Equal (0x69, md5[12]);
			Assert.Equal (0x74, md5[13]);
			Assert.Equal (0x79, md5[14]);
			Assert.Equal (0x21, md5[15]);
			Assert.Equal ("201205150149.CAA11933", entity.Id.LocalPart);
			Assert.Equal ("server10.shop3.company.com", entity.Id.Domain);
			Assert.Equal ("тема сообщения текст сообщения парсим сообщение тема сообщения текст сообщения парсим сообщение", entity.Description);
			Assert.Equal (new TimeSpan (123870000000L), entity.Duration);
			Assert.Equal ("http://www.ietf.cnri.reston.va.us/images/", entity.Base);
			Assert.Equal ("(& (color=Binary)\t(image-file-structure=TIFF-limited)\t(dpi=400)\t(dpi-xyratio=1)\t(paper-size=A4)\t(image-coding=MMR)\t(MRC-mode=0)\t(ua-media=stationery) )", entity.Features);
			Assert.Equal (2, entity.Alternatives.Count);
			Assert.Equal ("(& (color=Binary)\t(image-file-structure=TIFF-minimal)\t(dpi=200)\t(dpi-xyratio=1)\t(paper-size=A4)\t(image-coding=MH)\t(MRC-mode=0)\t(ua-media=stationery) )", entity.Alternatives[0]);
			Assert.Equal ("(& (color=Binary)\t(image-file-structure=TIFF-minimal)\t(dpi=100)\t(dpi-xyratio=1)\t(paper-size=A5)\t(image-coding=MH)\t(MRC-mode=0)\t(ua-media=stationery) )", entity.Alternatives[1]);
			Assert.Equal (6, entity.Languages.Count);
			Assert.Equal ("i-mingo", entity.Languages[0]);
			Assert.Equal ("de", entity.Languages[1]);
			Assert.Equal ("el", entity.Languages[2]);
			Assert.Equal ("en", entity.Languages[3]);
			Assert.Equal ("fr", entity.Languages[4]);
			Assert.Equal ("it", entity.Languages[5]);
			Assert.Equal (4, entity.ExtraFields.Count);
			var extField = (ExtensionHeaderField)entity.ExtraFields[0];
			Assert.Equal ("X-Priority", extField.ExtensionName);
			Assert.Equal (" 3", Encoding.ASCII.GetString (entity.ExtraFields[0].Body.Span));
			extField = (ExtensionHeaderField)entity.ExtraFields[1];
			Assert.Equal ("X-MimeOLE", extField.ExtensionName);
			Assert.Equal (" Produced By Microsoft MimeOLE V6.00.3790.4913", Encoding.ASCII.GetString (entity.ExtraFields[1].Body.Span));
			Assert.Equal (HeaderFieldName.AcceptLanguage, entity.ExtraFields[2].Name);
			Assert.Equal (" ru-ru,ru;q=0.8,en-us;q=0.5,en;q=0.3", Encoding.ASCII.GetString (entity.ExtraFields[2].Body.Span));
			extField = (ExtensionHeaderField)entity.ExtraFields[3];
			Assert.Equal ("Content-Quality", extField.ExtensionName);
			Assert.Equal (" 0.3 (poor)", Encoding.ASCII.GetString (entity.ExtraFields[3].Body.Span));

			Assert.IsType<TextEntityBody> (entity.Body);
			Assert.Equal ("some text", ((TextEntityBody)entity.Body).GetText ());
			Assert.Equal ("us-ascii", ((TextEntityBody)entity.Body).Encoding.WebName);
		}

		[Fact]
		[Trait ("Category", "Mime")]
		public void Save ()
		{
			var body = new TextEntityBody (Encoding.GetEncoding ("koi8-r"), ContentTransferEncoding.EightBit);
			body.SetDataAsync (new MemoryBufferedSource (new byte[] { 48, 49, 50 })).Wait ();
			var entity = new Entity (body, ContentMediaType.Text, "plain")
			{
				DispositionType = ContentDispositionType.Attachment,
				FileName = "This document specifies an Internet standards track protocol for the функции and requests discussion and suggestions.txt",
				Size = 318,
				CreationDate = new DateTimeOffset (new DateTime (634775112660000000L), TimeSpan.FromHours (6)),
				ModificationDate = new DateTimeOffset (new DateTime (634577249070000000L), TimeSpan.FromHours (6)),
				ReadDate = new DateTimeOffset (new DateTime (634776000130000000L), TimeSpan.FromHours (6)),
				MD5 = new byte[] { 0x43, 0x68, 0x65, 0x63, 0x6b, 0x20, 0x49, 0x6e, 0x74, 0x65, 0x67, 0x72, 0x69, 0x74, 0x79, 0x21 },
				Id = new AddrSpec ("201205150149.CAA11933", "server10.shop3.company.com"),
				Description = "Mongolian написаны in the Cyrillic script as used in Mongolia",
				Duration = new TimeSpan (0, 0, 12387),
				Base = "http://www.ietf.cnri.reston.va.us/images/",
				Location = "foo1.bar1",
				Features = "(& (color=Binary) (image-file-structure=TIFF-limited) (dpi=400) (dpi-xyratio=1) (paper-size=A4) (image-coding=MMR) (MRC-mode=0) (ua-media=stationery) )",
			};
			entity.Alternatives.Add ("(& (color=Binary) (image-file-structure=TIFF-minimal) (dpi=200) (dpi-xyratio=1) (paper-size=A4) (image-coding=MH) (MRC-mode=0) (ua-media=stationery) )");
			entity.Alternatives.Add ("(& (color=Binary) (image-file-structure=TIFF-minimal) (dpi=100) (dpi-xyratio=1) (paper-size=A5) (image-coding=MH) (MRC-mode=0) (ua-media=stationery) )");
			entity.Languages.Add ("i-mingo");
			entity.Languages.Add ("fr");

			var bytes = new BinaryDestinationMock (8192);
			entity.SaveAsync (bytes).Wait ();
			var rawData = Encoding.ASCII.GetString (bytes.Buffer.Slice (0, bytes.Count));

			var elements = SplitToElements (rawData);
			Assert.Equal (14, elements.Count);
			var headers = elements.Take (13).OrderBy (item => item, StringComparer.OrdinalIgnoreCase).ToArray ();
			var bodyText = elements[13];
			Assert.Equal ("\r\n\r\n012\r\n", bodyText);

			Assert.Equal (
				"Content-alternative: (& (color=Binary) (image-file-structure=TIFF-minimal)\r\n" +
				" (dpi=100) (dpi-xyratio=1) (paper-size=A5) (image-coding=MH) (MRC-mode=0)\r\n" +
				" (ua-media=stationery) )",
				headers[0]);
			Assert.Equal (
				"Content-alternative: (& (color=Binary) (image-file-structure=TIFF-minimal)\r\n" +
				" (dpi=200) (dpi-xyratio=1) (paper-size=A4) (image-coding=MH) (MRC-mode=0)\r\n" +
				" (ua-media=stationery) )",
				headers[1]);
			Assert.Equal ("Content-Base: http://www.ietf.cnri.reston.va.us/images/", headers[2]);
			Assert.Equal (
				"Content-Description: Mongolian =?utf-8?B?0L3QsNC/0LjRgdCw0L3Riw==?= in the\r\n" +
				" Cyrillic script as used in Mongolia",
				headers[3]);
			Assert.Equal (
				"Content-Disposition: attachment;\r\n" +
				" filename*0*=utf-8''This%20document%20specifies%20an%20Internet%20standards;\r\n" +
				" filename*1*=%20track%20protocol%20for%20the%20%D1%84%D1%83%D0%BD%D0%BA%D1%86;\r\n" +
				" filename*2*=%D0%B8%D0%B8%20and%20requests%20discussion%20and%20suggestions.t;\r\n" +
				" filename*3*=xt; modification-date=\"24 Nov 2011 09:48:27 +0600\";\r\n" +
				" creation-date=\"10 Jul 2012 10:01:06 +0600\";\r\n" +
				" read-date=\"11 Jul 2012 10:40:13 +0600\"; size=318",
				headers[4]);
			Assert.Equal ("Content-Duration: 12387", headers[5]);
			Assert.Equal (
				"Content-features: (& (color=Binary) (image-file-structure=TIFF-limited)\r\n" +
				" (dpi=400) (dpi-xyratio=1) (paper-size=A4) (image-coding=MMR) (MRC-mode=0)\r\n" +
				" (ua-media=stationery) )",
				headers[6]);
			Assert.Equal ("Content-ID: <201205150149.CAA11933@server10.shop3.company.com>", headers[7]);
			Assert.Equal ("Content-Language: i-mingo, fr", headers[8]);
			Assert.Equal ("Content-Location: foo1.bar1", headers[9]);
			Assert.Equal ("Content-MD5: Q2hlY2sgSW50ZWdyaXR5IQ==", headers[10]);
			Assert.Equal ("Content-Transfer-Encoding: 8bit", headers[11]);
			Assert.Equal ("Content-Type: text/plain; charset=koi8-r", headers[12]);
		}

		// разбираем сохраненное сообщение на заголовки и тело
		private static IReadOnlyList<string> SplitToElements (string source)
		{
			var result = new List<string> ();
			int start = 0;
			int pos = 0;
			while (pos < source.Length - 1)
			{
				if (source[pos] == '\r' && source[pos + 1] == '\n')
				{
					if ((pos < (source.Length - 2)) && source[pos + 2] == '\r' && source[pos + 3] == '\n')
					{
						if (start < pos)
						{
							if (start > 0)
							{
								start += 2;
							}

							result.Add (source[start..pos]);
						}

						break;
					}

					if ((pos < (source.Length - 1)) && source[pos + 2] != ' ' && source[pos + 3] != '\t')
					{
						if (start > 0)
						{
							start += 2;
						}

						result.Add (source[start..pos]);
						start = pos;
					}
				}

				pos++;
			}

			result.Add (source[pos..]);
			return result;
		}
	}
}
