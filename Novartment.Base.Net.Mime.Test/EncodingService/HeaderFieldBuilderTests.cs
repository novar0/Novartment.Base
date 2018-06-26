using System;
using System.Text;
using System.Collections.Generic;
using Xunit;

namespace Novartment.Base.Net.Mime.Test
{
	public class HeaderFieldBuilderTests
	{
		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateExactValue ()
		{
			// invalid characters
			var builder = HeaderFieldBuilder.CreateExactValue (HeaderFieldName.Supersedes, "short\r\n.   \tvalue  =?utf-8?B?0JDQtNGA0LXRgdCw0YIg0J7QtNC40L0=?=");
			Assert.Equal (HeaderFieldName.Supersedes, builder.Name);
			Assert.Equal (1, builder.ValueParts.Count);
			Assert.Equal ("short\r\n.   \tvalue  =?utf-8?B?0JDQtNGA0LXRgdCw0YIg0J7QtNC40L0=?=", builder.ValueParts[0]);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateAtomAndUnstructured ()
		{
			var builder = HeaderFieldBuilder.CreateAtomAndUnstructured (HeaderFieldName.Supersedes, "type", "value");
			Assert.Equal (2, builder.ValueParts.Count);
			Assert.Equal ("type;", builder.ValueParts[0]);
			Assert.Equal ("value", builder.ValueParts[1]);

			builder = HeaderFieldBuilder.CreateAtomAndUnstructured (HeaderFieldName.Supersedes, "dns", "2000 Адресат Один");
			Assert.Equal (3, builder.ValueParts.Count);
			Assert.Equal ("dns;", builder.ValueParts[0]);
			Assert.Equal ("2000", builder.ValueParts[1]);
			Assert.Equal (" =?utf-8?B?0JDQtNGA0LXRgdCw0YIg0J7QtNC40L0=?=", builder.ValueParts[2]);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateUnstructuredPair ()
		{
			var builder = HeaderFieldBuilder.CreateUnstructuredPair (HeaderFieldName.Supersedes, "value", null);
			Assert.Equal (1, builder.ValueParts.Count);
			Assert.Equal ("value", builder.ValueParts[0]);

			builder = HeaderFieldBuilder.CreateUnstructuredPair (HeaderFieldName.Supersedes, "Lena's Personal <Joke> List", "слово to the снова");
			Assert.Equal (5, builder.ValueParts.Count);
			Assert.Equal ("Lena's", builder.ValueParts[0]);
			Assert.Equal (" Personal", builder.ValueParts[1]);
			Assert.Equal (" <Joke>", builder.ValueParts[2]);
			Assert.Equal (" List;", builder.ValueParts[3]);
			Assert.Equal ("=?utf-8?B?0YHQu9C+0LLQviB0byB0aGUg0YHQvdC+0LLQsA==?=", builder.ValueParts[4]);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateTokensAndDate ()
		{
			var dt = new DateTimeOffset (634726649620000000L, TimeSpan.FromHours (3));
			var builder = HeaderFieldBuilder.CreateTokensAndDate (HeaderFieldName.Supersedes, null, dt);
			Assert.Equal (2, builder.ValueParts.Count);
			Assert.Equal (";", builder.ValueParts[0]);
			Assert.Equal ("15 May 2012 07:49:22 +0300", builder.ValueParts[1]);

			dt = new DateTimeOffset (634726649620000000L, TimeSpan.FromHours (1));
			builder = HeaderFieldBuilder.CreateTokensAndDate (HeaderFieldName.Supersedes, "CAA22933", dt);
			Assert.Equal (2, builder.ValueParts.Count);
			Assert.Equal ("CAA22933;", builder.ValueParts[0]);
			Assert.Equal ("15 May 2012 07:49:22 +0100", builder.ValueParts[1]);

			dt = new DateTimeOffset (634726649620000000L, TimeSpan.FromHours (-6));
			builder = HeaderFieldBuilder.CreateTokensAndDate (HeaderFieldName.Supersedes, " CMK-SLNS06.chmk.mechelgroup.ru   CAA22933\t", dt);
			Assert.Equal (3, builder.ValueParts.Count);
			Assert.Equal ("CMK-SLNS06.chmk.mechelgroup.ru", builder.ValueParts[0]);
			Assert.Equal ("CAA22933;", builder.ValueParts[1]);
			Assert.Equal ("15 May 2012 07:49:22 -0600", builder.ValueParts[2]);

			dt = new DateTimeOffset (634726649620000000L, TimeSpan.FromHours (10));
			builder = HeaderFieldBuilder.CreateTokensAndDate (HeaderFieldName.Supersedes, "by server10.espc2.mechel.com id CAA22933", dt);
			Assert.Equal (5, builder.ValueParts.Count);
			Assert.Equal ("by", builder.ValueParts[0]);
			Assert.Equal ("server10.espc2.mechel.com", builder.ValueParts[1]);
			Assert.Equal ("id", builder.ValueParts[2]);
			Assert.Equal ("CAA22933;", builder.ValueParts[3]);
			Assert.Equal ("15 May 2012 07:49:22 +1000", builder.ValueParts[4]);

			dt = new DateTimeOffset (634726649670000000L, TimeSpan.FromHours (0));
			builder = HeaderFieldBuilder.CreateTokensAndDate (HeaderFieldName.Supersedes, "by CMK-SLNS06.chmk.mechelgroup.ru from server10.espc2.mechel.com ([10.2.21.210])\r\n\twith ESMTP id 2012051507492777-49847", dt);
			Assert.Equal (10, builder.ValueParts.Count);
			Assert.Equal ("by", builder.ValueParts[0]);
			Assert.Equal ("CMK-SLNS06.chmk.mechelgroup.ru", builder.ValueParts[1]);
			Assert.Equal ("from", builder.ValueParts[2]);
			Assert.Equal ("server10.espc2.mechel.com", builder.ValueParts[3]);
			Assert.Equal ("([10.2.21.210])", builder.ValueParts[4]);
			Assert.Equal ("with", builder.ValueParts[5]);
			Assert.Equal ("ESMTP", builder.ValueParts[6]);
			Assert.Equal ("id", builder.ValueParts[7]);
			Assert.Equal ("2012051507492777-49847;", builder.ValueParts[8]);
			Assert.Equal ("15 May 2012 07:49:27 +0000", builder.ValueParts[9]);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreatePhraseAndId ()
		{
			var builder = HeaderFieldBuilder.CreatePhraseAndId (HeaderFieldName.Supersedes, "lenas-jokes.da39efc25c530ad145d41b86f7420c3b.021999.localhost", null);
			Assert.Equal (1, builder.ValueParts.Count);
			Assert.Equal ("<lenas-jokes.da39efc25c530ad145d41b86f7420c3b.021999.localhost>", builder.ValueParts[0]);

			builder = HeaderFieldBuilder.CreatePhraseAndId (HeaderFieldName.Supersedes, "lenas-jokes.da39efc25c530ad145d41b86f7420c3b.021999.localhost", "Lena's Personal <Joke> List");
			Assert.Equal (5, builder.ValueParts.Count);
			Assert.Equal ("Lena's", builder.ValueParts[0]);
			Assert.Equal (" Personal", builder.ValueParts[1]);
			Assert.Equal (" \"<Joke>\"", builder.ValueParts[2]);
			Assert.Equal (" List", builder.ValueParts[3]);
			Assert.Equal ("<lenas-jokes.da39efc25c530ad145d41b86f7420c3b.021999.localhost>", builder.ValueParts[4]);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreatePhraseList ()
		{
			var builder = HeaderFieldBuilder.CreatePhraseList (HeaderFieldName.Supersedes, Array.Empty<string> ());
			Assert.Equal (0, builder.ValueParts.Count);

			builder = HeaderFieldBuilder.CreatePhraseList (HeaderFieldName.Supersedes, new string[] { "keyword" });
			Assert.Equal (1, builder.ValueParts.Count);
			Assert.Equal ("keyword", builder.ValueParts[0]);

			builder = HeaderFieldBuilder.CreatePhraseList (HeaderFieldName.Supersedes, new string[] { "keyword", "KEY WORD", "Richard H. Nixon", "ключслово" });
			Assert.Equal (7, builder.ValueParts.Count);
			Assert.Equal ("keyword,", builder.ValueParts[0]);
			Assert.Equal ("KEY", builder.ValueParts[1]);
			Assert.Equal (" WORD,", builder.ValueParts[2]);
			Assert.Equal ("Richard", builder.ValueParts[3]);
			Assert.Equal (" \"H.\"", builder.ValueParts[4]);
			Assert.Equal (" Nixon,", builder.ValueParts[5]);
			Assert.Equal ("=?utf-8?B?0LrQu9GO0YfRgdC70L7QstC+?=", builder.ValueParts[6]);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateMailboxList ()
		{
			var mailboxes = new List<Mailbox> ();
			var builder = HeaderFieldBuilder.CreateMailboxList (HeaderFieldName.Supersedes, mailboxes);
			Assert.Equal (0, builder.ValueParts.Count);

			mailboxes.Add (new Mailbox ("one@mail.ru", "one man"));

			builder = HeaderFieldBuilder.CreateMailboxList (HeaderFieldName.Supersedes, mailboxes);
			Assert.Equal (3, builder.ValueParts.Count);
			Assert.Equal ("one", builder.ValueParts[0]);
			Assert.Equal (" man", builder.ValueParts[1]);
			Assert.Equal ("<one@mail.ru>", builder.ValueParts[2]);

			mailboxes.Add (new Mailbox ("two@gmail.ru", "man 2"));
			mailboxes.Add (new Mailbox ("three@hotmail.com"));
			builder = HeaderFieldBuilder.CreateMailboxList (HeaderFieldName.Supersedes, mailboxes);
			Assert.Equal (7, builder.ValueParts.Count);
			Assert.Equal ("one", builder.ValueParts[0]);
			Assert.Equal (" man", builder.ValueParts[1]);
			Assert.Equal ("<one@mail.ru>,", builder.ValueParts[2]);
			Assert.Equal ("man", builder.ValueParts[3]);
			Assert.Equal (" 2", builder.ValueParts[4]);
			Assert.Equal ("<two@gmail.ru>,", builder.ValueParts[5]);
			Assert.Equal ("<three@hotmail.com>", builder.ValueParts[6]);

			mailboxes.Clear ();
			mailboxes.Add (new Mailbox ("sp1@mailinator.com", "Адресат Один"));
			mailboxes.Add (new Mailbox ("sp2@mailinator.com", "Адресат Два"));
			mailboxes.Add (new Mailbox ("sp3@mailinator.com", "Адресат Три"));
			builder = HeaderFieldBuilder.CreateMailboxList (HeaderFieldName.Supersedes, mailboxes);
			Assert.Equal (6, builder.ValueParts.Count);
			Assert.Equal ("=?utf-8?B?0JDQtNGA0LXRgdCw0YIg0J7QtNC40L0=?=", builder.ValueParts[0]);
			Assert.Equal ("<sp1@mailinator.com>,", builder.ValueParts[1]);
			Assert.Equal ("=?utf-8?B?0JDQtNGA0LXRgdCw0YIg0JTQstCw?=", builder.ValueParts[2]);
			Assert.Equal ("<sp2@mailinator.com>,", builder.ValueParts[3]);
			Assert.Equal ("=?utf-8?B?0JDQtNGA0LXRgdCw0YIg0KLRgNC4?=", builder.ValueParts[4]);
			Assert.Equal ("<sp3@mailinator.com>", builder.ValueParts[5]);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateAngleBracketedList ()
		{
			var builder = HeaderFieldBuilder.CreateAngleBracketedList (HeaderFieldName.Supersedes, Array.Empty<string> ());
			Assert.Equal (0, builder.ValueParts.Count);
	
			builder = HeaderFieldBuilder.CreateAngleBracketedList (HeaderFieldName.Supersedes, new string[] { "mailto:list@host.com?subject=help" });
			Assert.Equal (1, builder.ValueParts.Count);
			Assert.Equal ("<mailto:list@host.com?subject=help>", builder.ValueParts[0]);

			var data = new string[]
			{
				"mailto:list@host.com?subject=help",
				"ftp://ftp.host.com/list.txt",
				"magnet:?xt=urn:tree:tiger:Z4URQ35KGEQW3YZZTIM7YXS3OLKLHFJ3M43DPHQ&xl=8539516502&dn=12.mkv",
				"some currently unknown command",
			};
			builder = HeaderFieldBuilder.CreateAngleBracketedList (HeaderFieldName.Supersedes, data);
			Assert.Equal (4, builder.ValueParts.Count);
			Assert.Equal ("<mailto:list@host.com?subject=help>,", builder.ValueParts[0]);
			Assert.Equal ("<ftp://ftp.host.com/list.txt>,", builder.ValueParts[1]);
			Assert.Equal ("<magnet:?xt=urn:tree:tiger:Z4URQ35KGEQW3YZZTIM7YXS3OLKLHFJ3M43DPHQ&xl=8539516502&dn=12.mkv>,", builder.ValueParts[2]);
			Assert.Equal ("<some currently unknown command>", builder.ValueParts[3]);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateDispositionNotificationParameterList ()
		{
			var parameters = new DispositionNotificationParameter[]
			{
				new DispositionNotificationParameter ("signed-receipt", DispositionNotificationParameterImportance.Optional, "pkcs7-signature"),
			};
			var builder = HeaderFieldBuilder.CreateDispositionNotificationParameterList (HeaderFieldName.Supersedes, parameters);
			Assert.Equal (1, builder.ValueParts.Count);
			Assert.Equal ("signed-receipt=optional,pkcs7-signature", builder.ValueParts[0]);

			parameters = new DispositionNotificationParameter[]
			{
				new DispositionNotificationParameter ("signed-receipt", DispositionNotificationParameterImportance.Optional, "pkcs7-signature"),
				new DispositionNotificationParameter ("signed-receipt-micalg", DispositionNotificationParameterImportance.Required, "sha1").AddValue ("md5"),
			};
			builder = HeaderFieldBuilder.CreateDispositionNotificationParameterList (HeaderFieldName.Supersedes, parameters);
			Assert.Equal (2, builder.ValueParts.Count);
			Assert.Equal ("signed-receipt=optional,pkcs7-signature;", builder.ValueParts[0]);
			Assert.Equal ("signed-receipt-micalg=required,sha1,md5", builder.ValueParts[1]);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateDisposition ()
		{
			var builder = HeaderFieldBuilder.CreateDisposition (HeaderFieldName.Supersedes, "value1", "value2", "value3", Array.Empty<string> ());
			Assert.Equal (2, builder.ValueParts.Count);
			Assert.Equal ("value1/value2;", builder.ValueParts[0]);
			Assert.Equal ("value3", builder.ValueParts[1]);

			builder = HeaderFieldBuilder.CreateDisposition (HeaderFieldName.Supersedes, "manual-action", "MDN-sent-manually", "displayed", new string[] { "value1", "value2", "value3" });
			Assert.Equal (2, builder.ValueParts.Count);
			Assert.Equal ("manual-action/MDN-sent-manually;", builder.ValueParts[0]);
			Assert.Equal ("displayed/value1,value2,value3", builder.ValueParts[1]);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void EncodeToBinaryTransportRepresentation ()
		{
			var buf = new byte[1000];

			// one parameter
			var builder = HeaderFieldBuilder.CreateExactValue (HeaderFieldName.Supersedes, "short.value");
			builder.AddParameter ("charset", "koi8-r");
			var size = builder.EncodeToBinaryTransportRepresentation (buf, 78);
			Assert.Equal ("Supersedes: short.value; charset=koi8-r\r\n", Encoding.ASCII.GetString (buf, 0, size));

			builder = HeaderFieldBuilder.CreateExactValue (HeaderFieldName.Supersedes, "short.value");
			builder.AddParameter ("charset", "koi8 r");
			size = builder.EncodeToBinaryTransportRepresentation (buf, 78);
			Assert.Equal ("Supersedes: short.value; charset=\"koi8 r\"\r\n", Encoding.ASCII.GetString (buf, 0, size));

			builder = HeaderFieldBuilder.CreateExactValue (HeaderFieldName.Supersedes, "short.value");
			builder.AddParameter ("charset", "функции");
			size = builder.EncodeToBinaryTransportRepresentation (buf, 78);
			Assert.Equal ("Supersedes: short.value;\r\n charset*0*=utf-8''%D1%84%D1%83%D0%BD%D0%BA%D1%86%D0%B8%D0%B8\r\n", Encoding.ASCII.GetString (buf, 0, size));

			// many parameters
			builder = HeaderFieldBuilder.CreateExactValue (HeaderFieldName.Supersedes, "short.value");
			builder.AddParameter ("name1", "value1");
			builder.AddParameter ("charset", "koi8-r");
			builder.AddParameter ("name2", "value2");
			size = builder.EncodeToBinaryTransportRepresentation (buf, 78);
			Assert.Equal ("Supersedes: short.value; name1=value1; charset=koi8-r; name2=value2\r\n", Encoding.ASCII.GetString (buf, 0, size));

			// оптимальное кодирования длинного значения параметра
			builder = HeaderFieldBuilder.CreateExactValue (HeaderFieldName.Supersedes, "value");
			builder.AddParameter ("filename", "This document specifies an Internet standards track protocol for the функции and requests discussion and suggestions.txt");
			size = builder.EncodeToBinaryTransportRepresentation (buf, 78);
			Assert.Equal (
				"Supersedes: value;\r\n filename*0*=utf-8''This%20document%20specifies%20an%20Internet%20standards;\r\n" +
				" filename*1=\" track protocol for the \";\r\n" +
				" filename*2*=%D1%84%D1%83%D0%BD%D0%BA%D1%86%D0%B8%D0%B8;\r\n" +
				" filename*3=\" and requests discussion and suggestions.txt\"\r\n",
				Encoding.ASCII.GetString (buf, 0, size));

			builder = HeaderFieldBuilder.CreateExactValue (HeaderFieldName.ContentDisposition, "attachment");
			builder.AddParameter ("filename", "This document specifies an Internet standards track protocol for the функции and requests discussion and suggestions.txt");
			builder.AddParameter ("modification-date", "24 Nov 2011 09:48:27 +0600");
			builder.AddParameter ("creation-date", "10 Jul 2012 10:01:06 +0600");
			builder.AddParameter ("read-date", "11 Jul 2012 10:40:13 +0600");
			builder.AddParameter ("size", "318");
			size = builder.EncodeToBinaryTransportRepresentation (buf, 78);
			Assert.Equal (
				"Content-Disposition: attachment;\r\n" +
				" filename*0*=utf-8''This%20document%20specifies%20an%20Internet%20standards;\r\n" +
				" filename*1=\" track protocol for the \";\r\n" +
				" filename*2*=%D1%84%D1%83%D0%BD%D0%BA%D1%86%D0%B8%D0%B8;\r\n" +
				" filename*3=\" and requests discussion and suggestions.txt\";\r\n" +
				" modification-date=\"24 Nov 2011 09:48:27 +0600\";\r\n" +
				" creation-date=\"10 Jul 2012 10:01:06 +0600\";\r\n" +
				" read-date=\"11 Jul 2012 10:40:13 +0600\"; size=318\r\n",
				Encoding.ASCII.GetString (buf, 0, size));
		}
	}
}
