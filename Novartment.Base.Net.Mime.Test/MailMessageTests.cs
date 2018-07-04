using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Novartment.Base.BinaryStreaming;
using Xunit;
using static System.Linq.Enumerable;

namespace Novartment.Base.Net.Mime.Test
{
	public class MailMessageTests
	{
		public MailMessageTests ()
		{
			Encoding.RegisterProvider (CodePagesEncodingProvider.Instance);
		}

		[Fact]
		[Trait ("Category", "Mime")]
		public void Load_AllHeaderFields ()
		{
			string template1 =
				"Received: from ChelMKMail.mailinator.com ([10.2.6.173]) by itc-serv01.chmk.mechelgroup.ru with Microsoft SMTPSVC(6.0.3790.4675);\tTue, 15 May 2012 07:49:27 +0600\r\n" +
				"Resent-Message-ID: <111aaabbb@server.com>\r\n" +
				"Resent-From: <gateway@mechel.com>\r\n" +
				"Resent-Date: Tue, 15 May 2012 03:49:22 +0100\r\n" +
				"Received: from server10.espc2.mechel.com ([10.2.21.210])\tby CMK-SLNS06.chmk.mechelgroup.ru (Lotus Domino Release 8.5.2FP4)\twith ESMTP id 2012051507492777-49847 ;\tTue, 15 May 2012 07:49:27 +0600\r\n" +
				"Received: by server10.espc2.mechel.com (8.8.8/1.37)\tid CAA22933; Tue, 15 May 2012 02:49:22 +0100\r\n" +
				"Date: Tue, 15 May 2012 02:49:22 +0100\r\n" +
				"Comments: The presence of header in a message is merely a request for an MDN.\r\n" +
				"Message-Id: <201205150149.CAA22933@server10.espc2.mechel.com>\r\n" +
				"From: <asutp_espc2@server10.espc2.mechel.com>\r\n" +
				"To: manager@itc-serv01.chmk.mechelgroup.ru\r\n" +
				"CC: no.name@mailinator.com,\t\"Recipient A.B. \\\"First\\\"\" <sp1@[some strange domain]>,\t=?windows-1251?Q?new_=F1=EE=E2=F1=E5=EC_one_222?= <\"namewith,comma\"@mailinator.com>,\t=?windows-1251?Q?=C8=E4=E5=FF_=F1=EE=F1=F2=EE=E8=F2_=E2_=F2=EE=EC=2C_=F7?=\t=?windows-1251?Q?=F2=EE=E1=FB_=EF=E8=F1=E0=F2=FC_=F2=E5=F1=F2=FB_=E4=EB=FF_?=\t=?windows-1251?Q?=EA=E0=E6=E4=EE=E9_=ED=E5=F2=F0=E8=E2=E8=E0=EB=FC=ED=EE=E9?=\t=?windows-1251?Q?_=F4=F3=ED=EA=F6=E8=E8_=E8=EB=E8_=EC=E5=F2=EE=E4=E0?= <sp3@mailinator.com>\r\n" +
				"Bcc: \"one man\" <one@mail.ru>, \"man 2\" <two@gmail.ru>, three@hotmail.com, \"Price of Persia\" <prince@persia.com>, \"King of Scotland\" <king.scotland@server.net>\r\n" +
				"Subject: =?utf-8?B?0YLQtdC80LAg0YHQvtC+0LHRidC10L3QuNGPINGC0LXQutGB0YIg0YHQvtC+0LHRidC10L3QuNGP?=\t=?utf-8?B?INC/0LDRgNGB0LjQvCDRgdC+0L7QsdGJ0LXQvdC40LUg0YLQtdC80LAg0YHQvtC+0LHRidC10L0=?=\t=?utf-8?B?0LjRjyDRgtC10LrRgdGCINGB0L7QvtCx0YnQtdC90LjRjyDQv9Cw0YDRgdC40Lwg0YHQvtC+0LE=?=\t=?utf-8?B?0YnQtdC90LjQtQ==?=\r\n" +
				"Disposition-Notification-To: \"some one\" <addr1@server1.com>, \"some2\" <addr2@server2.com> (not known), \"Ivan Sidorov\" <addr.ru@server3.com>\r\n" +
				"Disposition-Notification-Options: signed-receipt-protocol=optional,pkcs7-signature;\tsigned-receipt-micalg=required,sha1,md5\r\n" +
				"Return-Path: (some (nested (subnested 2) one) comments) (another\\) comment) <\"root person\"@ [server10/espc2/mechel third]>\r\n" +
				"Comments: MUA does not understand the meaning of the parameter\r\n" +
				"Comments: addresses may be rewritten while the message is in transit\r\n" +
				"Content-Base: http://www.ietf.cnri.reston.va.us/images/\r\n" +
				"Content-Location: foo1.bar1\r\n" +
				"Content-Quality: 0.3 (poor)\r\n" +
				"X-Priority: 3\r\n" +
				"X-MimeOLE: Produced By Microsoft MimeOLE V6.00.3790.4913\r\n" +
				"Accept-Language: ru-ru,ru;q=0.8,en-us;q=0.5,en;q=0.3\r\n" +
				"List-Help: <ftp://ftp.host.com/list.txt> (FTP),\t<mailto:list@host.com?subject=help> (List Instructions)\r\n" +
				"List-Unsubscribe: (Use this command to get off the list)\t<mailto:list-manager@host.com?body=unsubscribe%20list>,\t<mailto:list-request@host.com?subject=unsubscribe>\r\n" +
				"List-Post: NO (posting not allowed on this list)\r\n" +
				"List-Owner: <mailto:listmom@host.com> (Contact Person for Help)\r\n" +
				"List-Subscribe: <some currently unknown command>,\t<magnet:?xt=urn:tree:tiger:Z4URQ35KGEQW3YZZTIM7YXS3OLKLHFJ3M43DPHQ&xl=8539516502&dn=12.mkv>\r\n" +
				"List-Archive: <http://www.host.com/list/archive/> (Web Archive)\r\n" +
				"List-Id: \"Lena's Personal <Joke> List\"\t<lenas-jokes.da39efc25c530ad145d41b86f7420c3b.021999.localhost>\r\n" +
				"Mime-Version: (\"important \\(info\\)\" here) 3.(produced by MetaSend Vx.x)2\r\n\r\n";

			var msg = new MailMessage ();
			msg.LoadAsync (new MemoryBufferedSource (Encoding.ASCII.GetBytes (template1)), EntityBodyFactory.Create).Wait ();

			// Trace
			Assert.Equal (3, msg.Trace.Count);

			Assert.Equal (new DateTimeOffset (2012, 5, 15, 2, 49, 22, new TimeSpan (1, 0, 0)), msg.Trace[0].ReceivedTime);
			Assert.Equal ("by server10.espc2.mechel.com (8.8.8/1.37)\tid CAA22933", msg.Trace[0].ReceivedParameters);

			Assert.Equal (new DateTimeOffset (2012, 5, 15, 2, 49, 27, new TimeSpan (1, 0, 0)), msg.Trace[1].ReceivedTime);
			Assert.Equal ("from server10.espc2.mechel.com ([10.2.21.210])\tby CMK-SLNS06.chmk.mechelgroup.ru (Lotus Domino Release 8.5.2FP4)\twith ESMTP id 2012051507492777-49847", msg.Trace[1].ReceivedParameters);

			Assert.Equal (new DateTimeOffset (2012, 5, 15, 2, 49, 27, new TimeSpan (1, 0, 0)), msg.Trace[2].ReceivedTime);
			Assert.Equal ("from ChelMKMail.mailinator.com ([10.2.6.173]) by itc-serv01.chmk.mechelgroup.ru with Microsoft SMTPSVC(6.0.3790.4675)", msg.Trace[2].ReceivedParameters);
			Assert.Equal (1, msg.Trace[2].ResentFrom.Count);
			Assert.Equal ("gateway", msg.Trace[2].ResentFrom[0].Address.LocalPart);
			Assert.Equal ("mechel.com", msg.Trace[2].ResentFrom[0].Address.Domain);
			Assert.Equal ("111aaabbb", msg.Trace[2].ResentMessageId.LocalPart);
			Assert.Equal ("server.com", msg.Trace[2].ResentMessageId.Domain);
			Assert.Equal (new DateTimeOffset (2012, 5, 15, 3, 49, 22, new TimeSpan (1, 0, 0)), msg.Trace[2].ResentDate);

			// common
			Assert.Equal (1, msg.From.Count);
			Assert.Equal ("asutp_espc2", msg.From[0].Address.LocalPart);
			Assert.Equal ("server10.espc2.mechel.com", msg.From[0].Address.Domain);
			Assert.Equal ("201205150149.CAA22933", msg.MessageId.LocalPart);
			Assert.Equal ("server10.espc2.mechel.com", msg.MessageId.Domain);
			Assert.Null (msg.Sender);
			Assert.Equal (0, msg.ReplyTo.Count);
			Assert.Equal ("тема сообщения текст сообщения парсим сообщение тема сообщения текст сообщения парсим сообщение", msg.Subject);
			Assert.Equal (new DateTimeOffset (2012, 5, 15, 2, 49, 22, new TimeSpan (1, 0, 0)), msg.OriginationDate);
			Assert.Equal ("The presence of header in a message is merely a request for an MDN.\r\nMUA does not understand the meaning of the parameter\r\naddresses may be rewritten while the message is in transit", msg.Comments);
			Assert.Equal (new Version (3, 2), msg.MimeVersion);
			Assert.Equal (3, msg.ExtraFields.Count);
			var extField = (ExtensionHeaderField)msg.ExtraFields[0];
			Assert.Equal ("Content-Quality", extField.ExtensionName);
			Assert.Equal (" 0.3 (poor)", Encoding.ASCII.GetString (msg.ExtraFields[0].Body.Span));
			extField = (ExtensionHeaderField)msg.ExtraFields[1];
			Assert.Equal ("X-Priority", extField.ExtensionName);
			Assert.Equal (" 3", Encoding.ASCII.GetString (msg.ExtraFields[1].Body.Span));
			extField = (ExtensionHeaderField)msg.ExtraFields[2];
			Assert.Equal ("X-MimeOLE", extField.ExtensionName);
			Assert.Equal (" Produced By Microsoft MimeOLE V6.00.3790.4913", Encoding.ASCII.GetString (msg.ExtraFields[2].Body.Span));

			// to
			Assert.Equal (1, msg.RecipientTo.Count);
			Assert.IsType<Mailbox> (msg.RecipientTo[0]);
			Assert.Equal ("manager", msg.RecipientTo[0].Address.LocalPart);
			Assert.Equal ("itc-serv01.chmk.mechelgroup.ru", msg.RecipientTo[0].Address.Domain);

			// cc
			Assert.Equal (4, msg.RecipientCC.Count);
			var arr = msg.RecipientCC;
			Assert.Null (arr[0].Name);
			Assert.IsType<Mailbox> (arr[0]);
			Assert.Equal ("no.name", arr[0].Address.LocalPart);
			Assert.Equal ("mailinator.com", arr[0].Address.Domain);
			Assert.Equal ("Recipient A.B. \"First\"", arr[1].Name);
			Assert.IsType<Mailbox> (arr[1]);
			Assert.Equal ("sp1", arr[1].Address.LocalPart);
			Assert.Equal ("some strange domain", arr[1].Address.Domain);
			Assert.Equal ("new совсем one 222", arr[2].Name);
			Assert.IsType<Mailbox> (arr[2]);
			Assert.Equal ("namewith,comma", arr[2].Address.LocalPart);
			Assert.Equal ("mailinator.com", arr[2].Address.Domain);
			Assert.Equal ("Идея состоит в том, чтобы писать тесты для каждой нетривиальной функции или метода", arr[3].Name);
			Assert.IsType<Mailbox> (arr[3]);
			Assert.Equal ("sp3", arr[3].Address.LocalPart);
			Assert.Equal ("mailinator.com", arr[3].Address.Domain);

			// DispositionNotificationTo
			Assert.Equal (3, msg.DispositionNotificationTo.Count);
			Assert.Equal ("some one", msg.DispositionNotificationTo[0].Name);
			Assert.Equal ("addr1", msg.DispositionNotificationTo[0].Address.LocalPart);
			Assert.Equal ("server1.com", msg.DispositionNotificationTo[0].Address.Domain);
			Assert.Equal ("some2", msg.DispositionNotificationTo[1].Name);
			Assert.Equal ("addr2", msg.DispositionNotificationTo[1].Address.LocalPart);
			Assert.Equal ("server2.com", msg.DispositionNotificationTo[1].Address.Domain);
			Assert.Equal ("Ivan Sidorov", msg.DispositionNotificationTo[2].Name);
			Assert.Equal ("addr.ru", msg.DispositionNotificationTo[2].Address.LocalPart);
			Assert.Equal ("server3.com", msg.DispositionNotificationTo[2].Address.Domain);

			// DispositionNotificationOptions
			Assert.Equal (2, msg.DispositionNotificationOptions.Count);
			Assert.Equal ("signed-receipt-protocol", msg.DispositionNotificationOptions[0].Name);
			Assert.Equal (DispositionNotificationParameterImportance.Optional, msg.DispositionNotificationOptions[0].Importance);
			Assert.Equal ("pkcs7-signature", msg.DispositionNotificationOptions[0].Values[0]);
			Assert.Equal ("signed-receipt-micalg", msg.DispositionNotificationOptions[1].Name);
			Assert.Equal (DispositionNotificationParameterImportance.Required, msg.DispositionNotificationOptions[1].Importance);
			Assert.Equal (2, msg.DispositionNotificationOptions[1].Values.Count);
			Assert.Equal ("sha1", msg.DispositionNotificationOptions[1].Values[0]);
			Assert.Equal ("md5", msg.DispositionNotificationOptions[1].Values[1]);

			// AcceptLanguages
			Assert.Equal (4, msg.AcceptLanguages.Count);
			Assert.Equal ("ru-ru", msg.AcceptLanguages[0].Value);
			Assert.Equal (1.0m, msg.AcceptLanguages[0].Importance);
			Assert.Equal ("ru", msg.AcceptLanguages[1].Value);
			Assert.Equal (0.8m, msg.AcceptLanguages[1].Importance);
			Assert.Equal ("en-us", msg.AcceptLanguages[2].Value);
			Assert.Equal (0.5m, msg.AcceptLanguages[2].Importance);
			Assert.Equal ("en", msg.AcceptLanguages[3].Value);
			Assert.Equal (0.3m, msg.AcceptLanguages[3].Importance);

			// List*
			Assert.NotNull (msg.MailingList);
			Assert.Equal ("lenas-jokes.da39efc25c530ad145d41b86f7420c3b.021999.localhost", msg.MailingList.Id);
			Assert.Equal ("Lena's Personal <Joke> List", msg.MailingList.Description);
			Assert.Equal (2, msg.MailingList.HelpCommands.Count);
			Assert.Equal ("ftp://ftp.host.com/list.txt", msg.MailingList.HelpCommands[0]);
			Assert.Equal ("mailto:list@host.com?subject=help", msg.MailingList.HelpCommands[1]);
			Assert.Equal (2, msg.MailingList.UnsubscribeCommands.Count);
			Assert.Equal ("mailto:list-manager@host.com?body=unsubscribe%20list", msg.MailingList.UnsubscribeCommands[0]);
			Assert.Equal ("mailto:list-request@host.com?subject=unsubscribe", msg.MailingList.UnsubscribeCommands[1]);
			Assert.Equal (0, msg.MailingList.PostCommands.Count);
			Assert.Equal (1, msg.MailingList.OwnerCommands.Count);
			Assert.Equal ("mailto:listmom@host.com", msg.MailingList.OwnerCommands[0]);
			Assert.Equal (2, msg.MailingList.SubscribeCommands.Count);
			Assert.Equal ("some currently unknown command", msg.MailingList.SubscribeCommands[0]);
			Assert.Equal ("magnet:?xt=urn:tree:tiger:Z4URQ35KGEQW3YZZTIM7YXS3OLKLHFJ3M43DPHQ&xl=8539516502&dn=12.mkv", msg.MailingList.SubscribeCommands[1]);
			Assert.Equal (1, msg.MailingList.ArchiveCommands.Count);
			Assert.Equal ("http://www.host.com/list/archive/", msg.MailingList.ArchiveCommands[0]);

			Assert.Equal ("root person", msg.ReturnPath.LocalPart);
			Assert.Equal ("server10/espc2/mechel third", msg.ReturnPath.Domain);
		}

		[Fact]
		[Trait ("Category", "Mime")]
		public void Load_Simple ()
		{
			MailMessage msg;
			using (var fs = new FileStream (@"test2.eml", FileMode.Open, FileAccess.Read))
			{
				msg = new MailMessage ();
				msg.LoadAsync (fs.AsBufferedSource (new byte[1024]), EntityBodyFactory.Create).Wait ();
				Assert.Equal (fs.Length, fs.Position); // Поток данных сообщения прочитан не полностью
			}

			Assert.Equal (4, msg.RecipientCC.Count);
			Assert.Null (msg.RecipientCC[0].Name);
			Assert.Equal ("no.name", msg.RecipientCC[0].Address.LocalPart);
			Assert.Equal ("mailinator.com", msg.RecipientCC[0].Address.Domain);
			Assert.Equal ("Recipient A.B. \"First\"", msg.RecipientCC[1].Name);
			Assert.Equal ("sp1", msg.RecipientCC[1].Address.LocalPart);
			Assert.Equal ("some strange domain", msg.RecipientCC[1].Address.Domain);
			Assert.Equal ("new совсем one 222", msg.RecipientCC[2].Name);
			Assert.Equal ("namewith,comma", msg.RecipientCC[2].Address.LocalPart);
			Assert.Equal ("mailinator.com", msg.RecipientCC[2].Address.Domain);
			Assert.Equal ("Идея состоит в том, чтобы писать тесты для каждой нетривиальной функции или метода", msg.RecipientCC[3].Name);
			Assert.Equal ("sp3", msg.RecipientCC[3].Address.LocalPart);
			Assert.Equal ("mailinator.com", msg.RecipientCC[3].Address.Domain);

			Assert.Equal (1, msg.RecipientTo.Count);
			Assert.Equal ("sp", msg.RecipientTo[0].Address.LocalPart);
			Assert.Equal ("mailinator.com", msg.RecipientTo[0].Address.Domain);

			Assert.Equal (4, msg.ExtraFields.Count);
			var extField = (ExtensionHeaderField)msg.ExtraFields[0];
			Assert.Equal ("X-Priority", extField.ExtensionName);
			Assert.Equal (" 3", Encoding.ASCII.GetString (msg.ExtraFields[0].Body.Span));
			extField = (ExtensionHeaderField)msg.ExtraFields[1];
			Assert.Equal ("X-MSMail-Priority", extField.ExtensionName);
			Assert.Equal (" Normal", Encoding.ASCII.GetString (msg.ExtraFields[1].Body.Span));
			extField = (ExtensionHeaderField)msg.ExtraFields[2];
			Assert.Equal ("X-Unsent", extField.ExtensionName);
			Assert.Equal (" 1", Encoding.ASCII.GetString (msg.ExtraFields[2].Body.Span));
			extField = (ExtensionHeaderField)msg.ExtraFields[3];
			Assert.Equal ("X-MimeOLE", extField.ExtensionName);
			Assert.Equal (" Produced By Microsoft MimeOLE V6.00.3790.4913", Encoding.ASCII.GetString (msg.ExtraFields[3].Body.Span));

			// body
			Assert.IsType<TextEntityBody> (msg.Body);
			Assert.Equal (ContentMediaType.Text, msg.MediaType);
			Assert.Equal ("plain", msg.MediaSubtype);
			Assert.Equal (ContentTransferEncoding.EightBit, msg.TransferEncoding);
			Assert.Equal ("текст сообщения \r\n", ((TextEntityBody)msg.Body).GetText ());
			Assert.Equal ("koi8-r", ((TextEntityBody)msg.Body).Encoding.WebName);
		}

		[Fact]
		[Trait ("Category", "Mime")]
		public void Load_WithAttachment ()
		{
			MailMessage msg;
			using (var fs = new FileStream (@"test1.eml", FileMode.Open, FileAccess.Read))
			{
				msg = new MailMessage ();
				msg.LoadAsync (fs.AsBufferedSource (new byte[1024]), EntityBodyFactory.Create).Wait ();

				// Assert.Equal (fs.Length, fs.Position, "Поток данных сообщения прочитан не полностью");
			}

			Assert.Equal (1, msg.From.Count);
			Assert.Equal ("asutp_espc2", msg.From[0].Address.LocalPart);
			Assert.Equal ("server10.espc2.mechel.com", msg.From[0].Address.Domain);
			Assert.Equal ("201205150149.CAA22933", msg.MessageId.LocalPart);
			Assert.Equal ("server10.espc2.mechel.com", msg.MessageId.Domain);
			Assert.Equal ("rate of teeming", msg.Subject);
			Assert.Equal (new DateTimeOffset (2012, 5, 15, 2, 49, 22, new TimeSpan (1, 0, 0)), msg.OriginationDate);
			Assert.Equal (1, msg.RecipientTo.Count);
			Assert.Equal ("manager", msg.RecipientTo[0].Address.LocalPart);
			Assert.Equal ("itc-serv01.chmk.mechelgroup.ru", msg.RecipientTo[0].Address.Domain);
			Assert.Equal (5, msg.ExtraFields.Count);
			var extField = (ExtensionHeaderField)msg.ExtraFields[0];
			Assert.Equal ("X-Priority", extField.ExtensionName);
			Assert.Equal (" 3 (Normal)", Encoding.ASCII.GetString (msg.ExtraFields[0].Body.Span));
			extField = (ExtensionHeaderField)msg.ExtraFields[1];
			Assert.Equal ("X-MSMail-Priority", extField.ExtensionName);
			Assert.Equal (" Normal", Encoding.ASCII.GetString (msg.ExtraFields[1].Body.Span));
			extField = (ExtensionHeaderField)msg.ExtraFields[2];
			Assert.Equal ("X-Mailer", extField.ExtensionName);
			Assert.Equal (" Microsoft Outlook Express 5.00.2615.200", Encoding.ASCII.GetString (msg.ExtraFields[2].Body.Span));
			extField = (ExtensionHeaderField)msg.ExtraFields[3];
			Assert.Equal ("X-MIMETrack", extField.ExtensionName);
			Assert.Equal (
				" Itemize by SMTP Server on ChelMKMail2/SRV/MechelSG(Release 8.5.2FP4|November\r\n 17, 2011) at 15.05.2012 07:49:28,\r\n\tSerialize by Router on ChelMKGate1/SRV/MechelSG(Release 8.5.2FP4|November\r\n 17, 2011) at 15.05.2012 07:49:27",
				Encoding.ASCII.GetString (msg.ExtraFields[3].Body.Span));
			extField = (ExtensionHeaderField)msg.ExtraFields[4];
			Assert.Equal ("X-OriginalArrivalTime", extField.ExtensionName);
			Assert.Equal (" 15 May 2012 01:49:27.0976 (UTC) FILETIME=[F6915A80:01CD323C]", Encoding.ASCII.GetString (msg.ExtraFields[4].Body.Span));

			// body
			Assert.Equal (ContentMediaType.Multipart, msg.MediaType);
			Assert.Equal ("mixed", msg.MediaSubtype);
			Assert.IsType<CompositeEntityBody> (msg.Body);
			Assert.Equal ("sp-1234", ((CompositeEntityBody)msg.Body).Boundary);
			var rootBody = (ICompositeEntityBody)msg.Body;
			Assert.Equal (1, rootBody.Parts.Count);

			// attachment
			var att = rootBody.Parts[0];
			Assert.IsAssignableFrom<IDiscreteEntityBody> (att.Body);
			Assert.Equal (ContentMediaType.Application, att.MediaType);
			Assert.Equal ("text", att.MediaSubtype);
			Assert.Equal (ContentDispositionType.Attachment, att.DispositionType);
			Assert.Equal ("This document specifies an Internet standards track protocol for the функции and requests discussion and suggestions.txt", att.FileName);
			Assert.Equal ("base64", att.TransferEncoding.GetName ());
			byte[] hash;
			var data = ((IDiscreteEntityBody)att.Body).GetDataSource ().ReadAllBytesAsync ().Result;
			Assert.Equal (70289, data.Length);
#pragma warning disable CA5351 // Do not use insecure cryptographic algorithm MD5.
			using (var prov = MD5.Create ())
#pragma warning restore CA5351 // Do not use insecure cryptographic algorithm MD5.
			{
				hash = prov.ComputeHash (data.ToArray ());
			}

			Assert.Equal<byte> (
				new byte[] { 0x63, 0x4B, 0xD3, 0x11, 0x80, 0xE6, 0xE5, 0xB8, 0xE3, 0xFA, 0xD5, 0xF3, 0xBD, 0x10, 0x4D, 0x50 },
				hash);
		}

		[Fact]
		[Trait ("Category", "Mime")]
		public void Load_DeliveryReportMail ()
		{
			MailMessage msg;
			using (var fs = new FileStream (@"test3.eml", FileMode.Open, FileAccess.Read))
			{
				msg = new MailMessage ();
				msg.LoadAsync (fs.AsBufferedSource (new byte[1024]), EntityBodyFactory.Create).Wait ();
				Assert.Equal (fs.Length, fs.Position); // Поток данных сообщения прочитан не полностью
			}

			Assert.Equal ("postmaster", msg.From[0].Address.LocalPart);
			Assert.Equal ("itc-serv01.chmk.mechelgroup.ru", msg.From[0].Address.Domain);
			Assert.Equal ("2V1WYw6Z100000137", msg.MessageId.LocalPart);
			Assert.Equal ("itc-serv01.chmk.mechelgroup.ru", msg.MessageId.Domain);
			Assert.Equal ("Delivery Status Notification (Failure)", msg.Subject);
			Assert.Equal (new DateTimeOffset (2012, 5, 13, 12, 48, 25, new TimeSpan (6, 0, 0)), msg.OriginationDate);
			Assert.Equal (new Version (1, 0), msg.MimeVersion);
			Assert.Equal (1, msg.ExtraFields.Count);
			var extField = (ExtensionHeaderField)msg.ExtraFields[0];
			Assert.Equal ("X-DSNContext", extField.ExtensionName);
			Assert.Equal (" 7ce717b1 - 1160 - 00000002 - 00000000", Encoding.ASCII.GetString (msg.ExtraFields[0].Body.Span));

			// structure
			Assert.IsAssignableFrom<ICompositeEntityBody> (msg.Body);
			var rootBody = (ICompositeEntityBody)msg.Body;
			Assert.Equal (3, rootBody.Parts.Count);

			// root body
			Assert.Equal (ContentMediaType.Multipart, msg.MediaType);
			Assert.Equal ("report", msg.MediaSubtype);
			Assert.Equal ("9B095B5ADSN=_01CD2A60C2F9F3000000298Ditc?serv01.chmk.", ((CompositeEntityBody)msg.Body).Boundary);

			// text body
			var part1 = rootBody.Parts[0];
			Assert.IsType<TextEntityBody> (part1.Body);
			Assert.Equal (ContentMediaType.Text, part1.MediaType);
			Assert.Equal ("plain", part1.MediaSubtype);

			// Assert.Equal ("unicode - 1 - 1-utf-7", part1.Charset);
			Assert.Equal (
				"This is an automatically generated Delivery Status Notification.\r\n\r\nDelivery to the following recipients failed.\r\n\r\n       quality@itc-serv01.chmk.mechelgroup.ru\r\n\r\n\r\n\r\n",
				((TextEntityBody)part1.Body).GetText ());

			// delivery status body
			var part2 = rootBody.Parts[1];
			Assert.IsType<DeliveryStatusEntityBody> (part2.Body);
			Assert.Equal (ContentMediaType.Message, part2.MediaType);
			Assert.Equal ("delivery-status", part2.MediaSubtype);

			// message body
			var part3 = rootBody.Parts[2];
			Assert.IsType<MessageEntityBody> (part3.Body);
			var nestedMsg = ((MessageEntityBody)part3.Body).Message;
			Assert.IsType<TextEntityBody> (nestedMsg.Body);
			Assert.Equal (ContentMediaType.Unspecified, nestedMsg.MediaType);
			Assert.Null (nestedMsg.MediaSubtype);
			Assert.Equal ("201205131247.NAA05952", nestedMsg.MessageId.LocalPart);
			Assert.Equal ("asu15.espc6.mechel.com", nestedMsg.MessageId.Domain);
			Assert.Null (nestedMsg.Sender);
			Assert.NotNull (nestedMsg.ReplyTo);
			Assert.Equal (0, nestedMsg.ReplyTo.Count);
			Assert.Equal ("raport from espc6", nestedMsg.Subject);
			Assert.Equal (new DateTimeOffset (2012, 5, 13, 13, 47, 55, new TimeSpan (1, 0, 0)), nestedMsg.OriginationDate);
			Assert.Equal ("espc6", nestedMsg.From[0].Address.LocalPart);
			Assert.Equal ("mailinator.com", nestedMsg.From[0].Address.Domain);
			Assert.Equal ("Константин Теличко", nestedMsg.From[0].Name);
			Assert.Equal ("quality", nestedMsg.RecipientTo[0].Address.LocalPart);
			Assert.Equal ("itc-serv01.chmk.mechelgroup.ru", nestedMsg.RecipientTo[0].Address.Domain);
			Assert.Equal (2, nestedMsg.ExtraFields.Count);
			extField = (ExtensionHeaderField)nestedMsg.ExtraFields[0];
			Assert.Equal ("X-MIMETrack", extField.ExtensionName);
			Assert.Equal (
				" Itemize by SMTP Server on ChelMKMail2/SRV/MechelSG(Release 8.5.2FP4|November\r\n 17, 2011) at 13.05.2012 12:48:25,\r\n\tSerialize by Router on ChelMKGate1/SRV/MechelSG(Release 8.5.2FP4|November\r\n 17, 2011) at 13.05.2012 12:48:25,\r\n\tSerialize complete at 13.05.2012 12:48:25",
				Encoding.ASCII.GetString (nestedMsg.ExtraFields[0].Body.Span));
			extField = (ExtensionHeaderField)nestedMsg.ExtraFields[1];
			Assert.Equal ("X-OriginalArrivalTime", extField.ExtensionName);
			Assert.Equal (" 13 May 2012 06:48:25.0831 (UTC) FILETIME=[65894F70:01CD30D4]", Encoding.ASCII.GetString (nestedMsg.ExtraFields[1].Body.Span));
			Assert.Equal (
				"Using QNX\r\n--------------------\r\nHome  script=//15/home/dbserver/script/itc/.send_enpl\r\nSending File=raport_espc6h.xml\r\nCurrent Date=Sun May 13 12:47:44 2012\r\n--------------------\r\n\r\nbegin 644 raport_espc6h.xml\r\nM/#]X;6P@=F5R<VEO;CTB,2XP(B!E;F-O9&EN9STB8W X-C8B(#\\^/&1A=&%B\r\nM87-E('AM;&YS/2)U<FDZ.FUE8VAE;\"YR87!O<G0N1&%T85-C:&5M82(^/&5S\r\nM<&,V7VAE870^/&AE871?;G5M8F5R/C$Y,34W,SPO:&5A=%]N=6UB97(^/&5S\r\nM<&,V7V-H96UI<W1R>3X\\=&EM93XR,#$R+3 U+3$S5#$R.C0W.C T/\"]T:6UE\r\nM/CQP<F]B95]N=6UB97(^-C4\\+W!R;V)E7VYU;6)E<CX\\+V5S<&,V7V-H96UI\r\n><W1R>3X\\+V5S<&,V7VAE870^/\"]D871A8F%S93X*\r\n \r\nend\r\n\r\n",
				((TextEntityBody)nestedMsg.Body).GetText ());
		}

		[Fact]
		[Trait ("Category", "Mime")]
		public void Save_AllHeaderFields ()
		{
			var msg = MailMessage.CreateSimpleText ("html", Encoding.ASCII, ContentTransferEncoding.EightBit);

			msg.From.Add ("asutp_espc2@server10.espc2.mechel.com");
			msg.MessageId = new AddrSpec ("201205150149.CAA22933", "server10.espc2.mechel.com");
			msg.InReplyTo.Add (new AddrSpec ("201105150149.AAA13933", "raport.asus.com"));
			msg.References.Add (new AddrSpec ("201010100149.ACD43953", "raport.asus.com"));
			msg.References.Add (new AddrSpec ("201210100149.ACD43953", "raport.oracle.com"));
			msg.Subject = "тема сообщения текст сообщения парсим сообщение тема сообщения текст сообщения парсим сообщение";
			msg.OriginationDate = new DateTimeOffset (new DateTime (634726649620000000L), TimeSpan.FromHours (6));
			msg.Sender = new Mailbox ("mail-master@server.com", "General Autority");
			msg.ReplyTo.Add ("mail-master@server.com");
			msg.Comments = "addresses may be rewritten while the message is in transit";
			msg.RecipientTo.Add ("manager@itc-serv01.chmk.mechelgroup.ru");
			msg.RecipientCC.Add ("one@mail.ru", "one man");
			msg.RecipientCC.Add ("two@gmail.ru", "man 2");
			msg.RecipientCC.Add ("three@hotmail.com");
			msg.RecipientBcc.Add ("lion.king@africa.com", "Ling King");
			msg.DispositionNotificationTo.Add ("no.name@itc-serv01.chmk.mechelgroup.ru", "Recipient A.B. First");
			msg.DispositionNotificationOptions.Add (new DispositionNotificationParameter ("signed-receipt", DispositionNotificationParameterImportance.Optional, "pkcs7-signature"));
			msg.DispositionNotificationOptions.Add (new DispositionNotificationParameter ("signed-receipt-micalg", DispositionNotificationParameterImportance.Required, "sha1").AddValue ("md5"));
			msg.Keywords.Add ("computer");
			msg.Keywords.Add ("science");
			msg.Keywords.Add ("soft ware");
			msg.Keywords.Add ("program");
			msg.AcceptLanguages.Add (new QualityValueParameter ("en-us", 0.7m));
			msg.AcceptLanguages.Add (new QualityValueParameter ("en", 0.5m));
			msg.AcceptLanguages.AddOrderedByQuality ("ru-ru");
			msg.AcceptLanguages.AddOrderedByQuality ("ru");
			msg.MailingList = new MailingList ()
			{
				Id = "lenas-jokes.da39efc25c530ad145d41b86f7420c3b.021999.localhost",
				Description = "Lena's Personal <Joke> List",
			};
			msg.MailingList.HelpCommands.Add ("ftp://ftp.host.com/list.txt");
			msg.MailingList.HelpCommands.Add ("mailto:list@host.com?subject=help");
			msg.MailingList.UnsubscribeCommands.Add ("mailto:list-manager@host.com?body=unsubscribe%20list");
			msg.MailingList.UnsubscribeCommands.Add ("mailto:list-request@host.com?subject=unsubscribe");
			msg.MailingList.OwnerCommands.Add ("mailto:listmom@host.com");
			msg.MailingList.SubscribeCommands.Add ("some currently unknown command");
			msg.MailingList.SubscribeCommands.Add ("magnet:?xt=urn:tree:tiger:Z4URQ35KGEQW3YZZTIM7YXS3OLKLHFJ3M43DPHQ&xl=8539516502&dn=12.mkv");
			msg.MailingList.ArchiveCommands.Add ("http://www.host.com/list/archive/");
			((IDiscreteEntityBody)msg.Body).SetDataAsync (new MemoryBufferedSource (new byte[] { 48, 49, 50 })).Wait ();

			var bytes = new BinaryDestinationMock (8192);
			msg.SaveAsync (bytes).Wait ();
			var rawData = Encoding.ASCII.GetString (bytes.Buffer.Slice (0, bytes.Count));

			var elements = SplitToElements (rawData);
			var headers = elements
				.Take (elements.Count - 1)
				.Where (element => !element.StartsWith ("content", StringComparison.OrdinalIgnoreCase))
				.OrderBy (item => item, StringComparer.OrdinalIgnoreCase)
				.ToArray ();
			Assert.Equal (22, headers.Length);
			var body = elements[elements.Count - 1];
			Assert.Equal ("\r\n\r\n012\r\n", body); // в конце добавился CRLF как требует стандарт SMTP

			Assert.Equal ("Accept-Language: en-us, en, ru-ru, ru", headers[0]);

			// "Bcc: Ling King <lion.king@africa.com>" не будет, для соблюдения приватности
			Assert.Equal ("CC: one man <one@mail.ru>, man 2 <two@gmail.ru>, <three@hotmail.com>", headers[1]);
			Assert.Equal ("Comments: addresses may be rewritten while the message is in transit", headers[2]);
			Assert.Equal ("Date: 15 May 2012 07:49:22 +0600", headers[3]);
			Assert.Equal (
				"Disposition-Notification-Options: signed-receipt=optional,pkcs7-signature;\r\n" +
				" signed-receipt-micalg=required,sha1,md5",
				headers[4]);
			Assert.Equal (
				"Disposition-Notification-To: Recipient \"A.B.\" First\r\n" +
				" <no.name@itc-serv01.chmk.mechelgroup.ru>",
				headers[5]);
			Assert.Equal ("From: <asutp_espc2@server10.espc2.mechel.com>", headers[6]);
			Assert.Equal ("In-Reply-To: <201105150149.AAA13933@raport.asus.com>", headers[7]);
			Assert.Equal ("Keywords: computer, science, soft ware, program", headers[8]);
			Assert.Equal ("List-Archive: <http://www.host.com/list/archive/>", headers[9]);
			Assert.Equal ("List-Help: <ftp://ftp.host.com/list.txt>, <mailto:list@host.com?subject=help>", headers[10]);
			Assert.Equal (
				"List-ID: Lena's Personal \"<Joke>\" List\r\n" +
				" <lenas-jokes.da39efc25c530ad145d41b86f7420c3b.021999.localhost>",
				headers[11]);
			Assert.Equal ("List-Owner: <mailto:listmom@host.com>", headers[12]);
			Assert.Equal (
				"List-Subscribe: <some currently unknown command>,\r\n" +
				" <magnet:?xt=urn:tree:tiger:Z4URQ35KGEQW3YZZTIM7YXS3OLKLHFJ3M43DPHQ&xl=8539516502&dn=12.mkv>",
				headers[13]);
			Assert.Equal (
				"List-Unsubscribe: <mailto:list-manager@host.com?body=unsubscribe%20list>,\r\n" +
				" <mailto:list-request@host.com?subject=unsubscribe>",
				headers[14]);
			Assert.Equal ("Message-ID: <201205150149.CAA22933@server10.espc2.mechel.com>", headers[15]);
			Assert.Equal ("MIME-Version: 1.0", headers[16]);
			Assert.Equal (
				"References: <201010100149.ACD43953@raport.asus.com>\r\n" +
				" <201210100149.ACD43953@raport.oracle.com>",
				headers[17]);
			Assert.Equal ("Reply-To: <mail-master@server.com>", headers[18]);
			Assert.Equal ("Sender: General Autority <mail-master@server.com>", headers[19]);
			Assert.Equal (
				"Subject: =?utf-8?B?0YLQtdC80LAg0YHQvtC+0LHRidC10L3QuNGPINGC0LXQutGB0YI=?=\r\n" +
				" =?utf-8?B?INGB0L7QvtCx0YnQtdC90LjRjyDQv9Cw0YDRgdC40Lw=?=\r\n" +
				" =?utf-8?B?INGB0L7QvtCx0YnQtdC90LjQtSDRgtC10LzQsA==?=\r\n" +
				" =?utf-8?B?INGB0L7QvtCx0YnQtdC90LjRjyDRgtC10LrRgdGC?=\r\n" +
				" =?utf-8?B?INGB0L7QvtCx0YnQtdC90LjRjyDQv9Cw0YDRgdC40Lw=?=\r\n" +
				" =?utf-8?B?INGB0L7QvtCx0YnQtdC90LjQtQ==?=",
				headers[20]);
			Assert.Equal ("To: <manager@itc-serv01.chmk.mechelgroup.ru>", headers[21]);
		}

		[Fact]
		[Trait ("Category", "Mime")]
		public void Save_WithAttachment ()
		{
			var msg = MailMessage.CreateComposite (MultipartMediaSubtypeNames.Mixed);

			msg.GenerateId ();
			msg.RecipientTo.Add (new Mailbox ("sp1@mailinator.com", "Адресат Один"));
			msg.RecipientTo.Add (new Mailbox ("sp2@mailinator.com", "Адресат Два"));
			msg.RecipientTo.Add (new Mailbox ("sp3@mailinator.com", "Адресат Три"));
			msg.From.Add (new Mailbox ("noone@mailinator.com", "Сергей Пономарёв"));
			msg.Subject = "тема сообщения";
			msg.AddTextPart ("текст сообщения", Encoding.UTF8, TextMediaSubtypeNames.Plain, ContentTransferEncoding.Base64);
			msg.AddAttachmentAsync (new FileInfo (@"test4.ico").OpenRead ().AsBufferedSource (new byte[1024]), "test4.ico").Wait ();

			var bytes = new BinaryDestinationMock (8192);
			msg.SaveAsync (bytes).Wait ();
			var rawData = Encoding.ASCII.GetString (bytes.Buffer.Slice (0, bytes.Count));

			Assert.IsType<CompositeEntityBody> (msg.Body);
			var sampleBoundary = ((CompositeEntityBody)msg.Body).Boundary;
			var elements = SplitToElements (rawData);
			Assert.Equal (9, elements.Count);
			var headers = elements.Take (8).OrderBy (item => item, StringComparer.OrdinalIgnoreCase).ToArray ();
			Assert.Equal ("Content-Transfer-Encoding: 7bit", headers[0]);
			Assert.Equal ("Content-Type: multipart/mixed;\r\n boundary=\"" + sampleBoundary + "\"", headers[1]);
			Assert.StartsWith ("Date: ", headers[2], StringComparison.OrdinalIgnoreCase);
			Assert.Equal ("From: =?utf-8?B?0KHQtdGA0LPQtdC5INCf0L7QvdC+0LzQsNGA0ZHQsg==?=\r\n <noone@mailinator.com>", headers[3]);

			// домен текущего компьютера может быть длинным, поэтому возможен перенос на новую строку
			var messageIdStr = "<" + msg.MessageId.LocalPart + "@" + msg.MessageId.Domain + ">";
			if (("Message-ID: " + messageIdStr).Length <= 78)
			{
				Assert.Equal ("Message-ID: " + messageIdStr, headers[4]);
			}
			else
			{
				Assert.Equal ("Message-ID:\r\n " + messageIdStr, headers[4]);
			}

			Assert.Equal ("MIME-Version: 1.0", headers[5]);
			Assert.Equal ("Subject: =?utf-8?B?0YLQtdC80LAg0YHQvtC+0LHRidC10L3QuNGP?=", headers[6]);
			Assert.Equal ("To: =?utf-8?B?0JDQtNGA0LXRgdCw0YIg0J7QtNC40L0=?= <sp1@mailinator.com>,\r\n =?utf-8?B?0JDQtNGA0LXRgdCw0YIg0JTQstCw?= <sp2@mailinator.com>,\r\n =?utf-8?B?0JDQtNGA0LXRgdCw0YIg0KLRgNC4?= <sp3@mailinator.com>", headers[7]);

			var parts = SplitToParts (elements[8], "\r\n--" + sampleBoundary);
			Assert.Equal (4, parts.Count);
			Assert.Equal ("\r\n\r\n", parts[0]);
			Assert.Equal ('-', parts[3][0]);
			Assert.Equal ('-', parts[3][1]);

			var subElements1 = SplitToElements (parts[1].Substring (2));
			Assert.Equal (3, subElements1.Count);
			var subHeaders1 = subElements1.Take (2).OrderBy (item => item, StringComparer.OrdinalIgnoreCase).ToArray ();
			var subBody1 = subElements1[2];
			Assert.Equal ("Content-Transfer-Encoding: base64", subHeaders1[0]);
			Assert.Equal ("Content-Type: text/plain; charset=utf-8", subHeaders1[1]);
			Assert.Equal ("\r\n\r\n0YLQtdC60YHRgiDRgdC+0L7QsdGJ0LXQvdC40Y8=\r\n", subBody1);

			var subElements2 = SplitToElements (parts[2].Substring (2));
			Assert.Equal (4, subElements2.Count);
			var subHeaders2 = subElements2.Take (3).OrderBy (item => item, StringComparer.OrdinalIgnoreCase).ToArray ();
			var subBody2 = subElements2[3];
			Assert.StartsWith ("Content-Disposition: attachment; filename=test4.ico; size=318", subHeaders2[0], StringComparison.OrdinalIgnoreCase);
			Assert.Equal ("Content-Transfer-Encoding: base64", subHeaders2[1]);
			Assert.Equal ("Content-Type: application/octet-stream", subHeaders2[2]);
			Assert.Equal (
				"\r\n\r\nAAABAAEAEBAQAAEABAAoAQAAFgAAACgAAAAQAAAAIAAAAAEABAAAAAAAAAAAAAAAAAAAAAAAEAAA\r\n" +
				"AAAAAAAEAgQAhIOEAMjHyABIR0gA6ejpAGlqaQCpqKkAKCgoAPz9/AAZGBkAmJiYANjZ2ABXWFcA\r\n" +
				"ent6ALm6uQA8OjwAiIiIiIiIiIiIiI4oiL6IiIiIgzuIV4iIiIhndo53KIiIiB/WvXoYiIiIfEZf\r\n" +
				"WBSIiIEGi/foqoiIgzuL84i9iIjpGIoMiEHoiMkos3FojmiLlUipYliEWIF+iDe0GoRa7D6GPbjc\r\n" +
				"u1yIiIiIiIiIiIiIiIiIiIiIiIiIiIiIiIgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA\r\n" +
				"AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA\r\n",
				subBody2);
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

							result.Add (source.Substring (start, pos - start));
						}

						break;
					}

					if ((pos < (source.Length - 1)) && source[pos + 2] != ' ' && source[pos + 3] != '\t')
					{
						if (start > 0)
						{
							start += 2;
						}

						result.Add (source.Substring (start, pos - start));
						start = pos;
					}
				}

				pos++;
			}

			result.Add (source.Substring (pos));
			return result;
		}

		// разбираем сохраненное сообщение на заголовки и тело
		private static IReadOnlyList<string> SplitToParts (string source, string delimiter)
		{
			var result = new List<string> ();
			int pos = 0;
			do
			{
				int idx = source.IndexOf (delimiter, pos, StringComparison.Ordinal);
				if (idx < 0)
				{
					break;
				}

				result.Add (source.Substring (pos, idx - pos));
				pos = idx + delimiter.Length;
			}
			while (true);
			result.Add (source.Substring (pos));
			return result;
		}
	}
}
