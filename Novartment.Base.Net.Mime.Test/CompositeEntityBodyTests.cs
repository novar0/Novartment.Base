using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Novartment.Base.BinaryStreaming;
using Xunit;

namespace Novartment.Base.Net.Mime.Test
{
	public sealed class CompositeEntityBodyTests
	{
		private readonly string[] _bodySample1 = new string[]
		{
			string.Empty,
			string.Empty,
			"--9B095B5ADSN=_01CD2A60C2F9F3000000298Ditc?serv01.chmk.",
			"Content-Type: text/plain; charset=us-ascii",
			string.Empty,
			"This is an automatically generated Delivery Status Notification.",
			string.Empty,
			"--9B095B5ADSN=_01CD2A60C2F9F3000000298Ditc?serv01.chmk.",
			"Content-Type: message/delivery-status",
			string.Empty,
			"Reporting-MTA: dns;itc-serv01.chmk.mechelgroup.ru",
			"Arrival-Date: Sun, 13 May 2012 12:48:25 +0600",
			string.Empty,
			"Final-Recipient: rfc822;quality@itc-serv01.chmk.mechelgroup.ru",
			"Action: failed",
			"Status: 5.0.0",
			string.Empty,
			"--9B095B5ADSN=_01CD2A60C2F9F3000000298Ditc?serv01.chmk.",
			"Content-Type: message/rfc822",
			string.Empty,
			"Date: Sun, 13 May 2012 13:47:55 +0100",
			"From: =?koi8-r?B?68/O09TBztTJziD0xczJ3svP?= <espc6@mailinator.com>",
			"Subject: raport from espc6",
			string.Empty,
			"Using QNX",
			string.Empty,
			"--9B095B5ADSN=_01CD2A60C2F9F3000000298Ditc?serv01.chmk.",
			string.Empty,
			string.Empty,
			"=30=31=32=33=34",
			string.Empty,
			"--9B095B5ADSN=_01CD2A60C2F9F3000000298Ditc?serv01.chmk.--",
			"some ending text",
		};

		private readonly string[] _bodySample2 = new string[]
		{
			string.Empty,
			"--NextPart=_2a1f80a0bc26469baafab5bbfb1dbdff",
			"Content-Type: application/octet-stream",
			"Content-Transfer-Encoding: base64",
			string.Empty,
			"FBUW",
			string.Empty,
			"--NextPart=_2a1f80a0bc26469baafab5bbfb1dbdff",
			"Content-Type: multipart/alternative;",
			" boundary=\"NextPart=_cfa821c7ffa04e00aed5619fd21ba49b\"",
			"Content-Transfer-Encoding: binary",
			string.Empty,
			string.Empty,
			"--NextPart=_cfa821c7ffa04e00aed5619fd21ba49b",
			"Content-Type: text/plain; charset=utf-8",
			"Content-Transfer-Encoding: binary",
			string.Empty,
			"text1",
			string.Empty,
			"--NextPart=_cfa821c7ffa04e00aed5619fd21ba49b",
			"Content-Type: text/html; charset=utf-8",
			"Content-Transfer-Encoding: binary",
			string.Empty,
			"text2",
			string.Empty,
			"--NextPart=_cfa821c7ffa04e00aed5619fd21ba49b--",
			string.Empty,
			"--NextPart=_2a1f80a0bc26469baafab5bbfb1dbdff",
			"Content-Type: message/rfc822",
			"Content-Transfer-Encoding: 7bit",
			string.Empty,
			"Date: 1 Jan 2001 00:00:00", // тут будет текущая дата
			"From: <some@server>",
			"MIME-Version: 1.0",
			"Content-Type: text/xml; charset=us-ascii",
			"Content-Transfer-Encoding: quoted-printable",
			string.Empty,
			"012",
			string.Empty,
			"--NextPart=_2a1f80a0bc26469baafab5bbfb1dbdff",
			"Content-Type: image/png",
			"Content-Transfer-Encoding: base64",
			string.Empty,
			"QUJD",
			string.Empty,
			"--NextPart=_2a1f80a0bc26469baafab5bbfb1dbdff--",
		};

		[Fact]
		[Trait ("Category", "Mime")]
		public void Load ()
		{
			var body = new CompositeEntityBody ("9B095B5ADSN=_01CD2A60C2F9F3000000298Ditc?serv01.chmk.");
			var source = new MemoryBufferedSource (Encoding.ASCII.GetBytes (string.Join ("\r\n", _bodySample1)));
			body.LoadAsync (source, parameters => new DataEntityBody (parameters.TransferEncoding)).Wait ();
			Assert.Equal (4, body.Parts.Count);

			Assert.Equal (ContentMediaType.Text, body.Parts[0].MediaType);
			Assert.Equal ("plain", body.Parts[0].MediaSubtype);

			Assert.Equal (ContentMediaType.Message, body.Parts[1].MediaType);
			Assert.Equal ("delivery-status", body.Parts[1].MediaSubtype);

			Assert.Equal (ContentMediaType.Message, body.Parts[2].MediaType);
			Assert.Equal ("rfc822", body.Parts[2].MediaSubtype);

			// часть для которой не указан медиатип, должна быть типом по-умолчанию
			Assert.Equal (ContentMediaType.Unspecified, body.Parts[3].MediaType);
			Assert.Null (body.Parts[3].MediaSubtype);
		}

		[Fact]
		[Trait ("Category", "Mime")]
		public void CreatePartsAndSave ()
		{
			var compositeEntityBody = new CompositeEntityBody ("NextPart=_2a1f80a0bc26469baafab5bbfb1dbdff");

			var pkcs7Body = new DataEntityBody (ContentTransferEncoding.Base64);
			pkcs7Body.SetDataAsync (new MemoryBufferedSource (new byte[] { 20, 21, 22 })).Wait ();
			var pkcs7Entity = new Entity (pkcs7Body, ContentMediaType.Application, ApplicationMediaSubtypeNames.OctetStream);
			compositeEntityBody.Parts.Add (pkcs7Entity);

			var compositeBody = new CompositeEntityBody ("NextPart=_cfa821c7ffa04e00aed5619fd21ba49b");
			var compositeEntity = new Entity (compositeBody, ContentMediaType.Multipart, "alternative");
			compositeEntityBody.Parts.Add (compositeEntity);

			var textBody = new TextEntityBody (Encoding.UTF8, ContentTransferEncoding.Binary);
			textBody.SetText ("text1\r\n");
			var textEntity = new Entity (textBody, ContentMediaType.Text, "plain");
			compositeBody.Parts.Add (textEntity);

			textBody = new TextEntityBody (Encoding.UTF8, ContentTransferEncoding.Binary);
			textBody.SetText ("text2\r\n");
			textEntity = new Entity (textBody, ContentMediaType.Text, "html");
			compositeBody.Parts.Add (textEntity);

			var nestedMessage = MailMessage.CreateSimpleText ("xml", Encoding.ASCII, ContentTransferEncoding.QuotedPrintable);
			nestedMessage.From.Add ("some@server");
			((TextEntityBody)nestedMessage.Body).SetText ("012");

			var newBody = new MessageEntityBody ()
			{
				Message = nestedMessage,
			};
			var newEntity = new Entity (newBody, ContentMediaType.Message, MessageMediaSubtypeNames.Rfc822);
			compositeEntityBody.Parts.Add (newEntity);

			var singlePartBody = new DataEntityBody (ContentTransferEncoding.Base64);
			singlePartBody.SetDataAsync (new MemoryBufferedSource (new byte[] { 65, 66, 67 })).Wait ();
			var singlePartEntity = new Entity (singlePartBody, ContentMediaType.Image, "png");
			compositeEntityBody.Parts.Add (singlePartEntity);

			var bytes = new BinaryDestinationMock (8192);
			compositeEntityBody.SaveAsync (bytes).Wait ();
			var text = Encoding.UTF8.GetString (bytes.Buffer.Slice (0, bytes.Count));
			var lines = text.Split (new string[] { "\r\n" }, StringSplitOptions.None);

			for (var idx = 0; idx < _bodySample2.Length; idx++)
			{
				if (!lines[idx].StartsWith ("Date: ", StringComparison.Ordinal))
				{
					// дату проверить не можем, она генерируется автоматически
					Assert.Equal (_bodySample2[idx], lines[idx]);
				}
			}
		}

		[Fact]
		[Trait ("Category", "Mime")]
		public void GenerateBoundary ()
		{
			var allowed = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789'()+_,-./:=?";
			var set = new HashSet<string> ();
			for (var i = 0; i < 1000; i++)
			{
				var entity = new CompositeEntityBody (null);
				Assert.True (entity.Boundary.Length >= 8);
				foreach (var ch in entity.Boundary)
				{
					Assert.InRange (allowed.IndexOf (ch), 0, int.MaxValue);
				}

				Assert.True (set.Add (entity.Boundary));
			}
		}
	}
}
