using System;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using Xunit;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net.Mime.Test
{
	public class CompositeEntityBodyTests
	{
		private string[] BodySample1 = new string[] {
			"",
			"",
			"--9B095B5ADSN=_01CD2A60C2F9F3000000298Ditc?serv01.chmk.",
			"Content-Type: text/plain; charset=us-ascii",
			"",
			"This is an automatically generated Delivery Status Notification.",
			"",
			"--9B095B5ADSN=_01CD2A60C2F9F3000000298Ditc?serv01.chmk.",
			"Content-Type: message/delivery-status",
			"",
			"Reporting-MTA: dns;itc-serv01.chmk.mechelgroup.ru",
			"Arrival-Date: Sun, 13 May 2012 12:48:25 +0600",
			"",
			"Final-Recipient: rfc822;quality@itc-serv01.chmk.mechelgroup.ru",
			"Action: failed",
			"Status: 5.0.0",
			"",
			"--9B095B5ADSN=_01CD2A60C2F9F3000000298Ditc?serv01.chmk.",
			"Content-Type: message/rfc822",
			"",
			"Date: Sun, 13 May 2012 13:47:55 +0100",
			"From: =?koi8-r?B?68/O09TBztTJziD0xczJ3svP?= <espc6@mailinator.com>",
			"Subject: raport from espc6",
			"",
			"Using QNX",
			"",
			"--9B095B5ADSN=_01CD2A60C2F9F3000000298Ditc?serv01.chmk.",
			"",
			"",
			"=30=31=32=33=34",
			"",
			"--9B095B5ADSN=_01CD2A60C2F9F3000000298Ditc?serv01.chmk.--",
			"some ending text" };
		private string[] BodySample2 = new string[] {
			"",
			"--NextPart=_2a1f80a0bc26469baafab5bbfb1dbdff",
			"Content-Type: application/octet-stream",
			"Content-Transfer-Encoding: base64",
			"",
			"FBUW",
			"",
			"--NextPart=_2a1f80a0bc26469baafab5bbfb1dbdff",
			"Content-Type: multipart/alternative;",
			" boundary=\"NextPart=_cfa821c7ffa04e00aed5619fd21ba49b\"",
			"Content-Transfer-Encoding: binary",
			"",
			"",
			"--NextPart=_cfa821c7ffa04e00aed5619fd21ba49b",
			"Content-Type: text/plain; charset=utf-8",
			"Content-Transfer-Encoding: binary",
			"",
			"text1",
			"",
			"--NextPart=_cfa821c7ffa04e00aed5619fd21ba49b",
			"Content-Type: text/html; charset=utf-8",
			"Content-Transfer-Encoding: binary",
			"",
			"text2",
			"",
			"--NextPart=_cfa821c7ffa04e00aed5619fd21ba49b--",
			"",
			"--NextPart=_2a1f80a0bc26469baafab5bbfb1dbdff",
			"Content-Type: message/rfc822",
			"Content-Transfer-Encoding: 7bit",
			"",
			"Date: 1 Jan 2001 00:00:00", // тут будет текущая дата
			"From: <some@server>",
			"MIME-Version: 1.0",
			"Content-Type: text/xml; charset=us-ascii",
			"Content-Transfer-Encoding: quoted-printable",
			"",
			"012",
			"",
			"--NextPart=_2a1f80a0bc26469baafab5bbfb1dbdff",
			"Content-Type: image/png",
			"Content-Transfer-Encoding: base64",
			"",
			"QUJD",
			"",
			"--NextPart=_2a1f80a0bc26469baafab5bbfb1dbdff--" };

		[Fact, Trait ("Category", "Mime")]
		public void Load ()
		{
			var body = new CompositeEntityBody ("9B095B5ADSN=_01CD2A60C2F9F3000000298Ditc?serv01.chmk.");
			var source = new ArrayBufferedSource (Encoding.ASCII.GetBytes (string.Join ("\r\n", BodySample1)));
			body.LoadAsync (
				source,
				parameters => new DataEntityBody (parameters.TransferEncoding),
				CancellationToken.None).Wait ();
			Assert.Equal (4, body.Parts.Count);

			Assert.Equal (ContentMediaType.Text, body.Parts[0].Type);
			Assert.Equal ("plain", body.Parts[0].Subtype);

			Assert.Equal (ContentMediaType.Message, body.Parts[1].Type);
			Assert.Equal ("delivery-status", body.Parts[1].Subtype);

			Assert.Equal (ContentMediaType.Message, body.Parts[2].Type);
			Assert.Equal ("rfc822", body.Parts[2].Subtype);

			// часть для которой не указан медиатип, должна быть типом по-умолчанию
			Assert.Equal (ContentMediaType.Unspecified, body.Parts[3].Type);
			Assert.Null (body.Parts[3].Subtype);
		}

		[Fact, Trait ("Category", "Mime")]
		public void CreatePartsAndSave ()
		{
			var compositeEntityBody = new CompositeEntityBody ("NextPart=_2a1f80a0bc26469baafab5bbfb1dbdff");

			var pkcs7Body = new DataEntityBody (ContentTransferEncoding.Base64);
			pkcs7Body.SetDataAsync (new ArrayBufferedSource (new byte[] { 20, 21, 22 }), CancellationToken.None).Wait ();
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

			var newBody = new MessageEntityBody ();
			newBody.Message = nestedMessage;
			var newEntity = new Entity (newBody, ContentMediaType.Message, MessageMediaSubtypeNames.Rfc822);
			compositeEntityBody.Parts.Add (newEntity);

			var singlePartBody = new DataEntityBody (ContentTransferEncoding.Base64);
			singlePartBody.SetDataAsync (new ArrayBufferedSource (new byte[] { 65, 66, 67 }), CancellationToken.None).Wait ();
			var singlePartEntity = new Entity (singlePartBody, ContentMediaType.Image, "png");
			compositeEntityBody.Parts.Add (singlePartEntity);

			var bytes = new BinaryDestinationMock (8192);
			compositeEntityBody.SaveAsync (bytes, CancellationToken.None).Wait ();
			var text = Encoding.UTF8.GetString (bytes.Buffer, 0, bytes.Count);
			var lines = text.Split (new string[] { "\r\n" }, StringSplitOptions.None);

			for (var idx = 0; idx < BodySample2.Length; idx++)
			{
				if (!lines[idx].StartsWith ("Date: ")) // дату проверить не можем, она генерируется автоматически
				{
					Assert.Equal (BodySample2[idx], lines[idx]);
				}
			}
		}

		[Fact, Trait ("Category", "Mime")]
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
