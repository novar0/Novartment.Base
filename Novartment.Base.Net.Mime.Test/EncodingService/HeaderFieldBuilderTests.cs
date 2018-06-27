using System;
using System.Text;
using System.Collections.Generic;
using Xunit;

namespace Novartment.Base.Net.Mime.Test
{
	public class HeaderFieldBuilderTests
	{
		// нет смысла тестировать тривиальные HeaderFieldBuilderExactValue, HeaderFieldBuilderUnstructured, HeaderFieldBuilderPhrase, HeaderFieldBuilderMailbox

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateLanguageList ()
		{
			var builder = new HeaderFieldBuilderLanguageList (HeaderFieldName.Supersedes, new string[] { "one", "two2", "three-en" });
			var parts = builder.GetParts ();
			Assert.Equal (HeaderFieldName.Supersedes, builder.Name);
			Assert.Equal (3, parts.Count);
			Assert.Equal ("one,", parts[0]);
			Assert.Equal ("two2,", parts[1]);
			Assert.Equal ("three-en", parts[2]);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateAddrSpecList ()
		{
			var builder = new HeaderFieldBuilderAddrSpecList (HeaderFieldName.Supersedes, new AddrSpec[] { AddrSpec.Parse ("someone@someserver.ru"), AddrSpec.Parse ("\"real(addr)\"@someserver.ru"), AddrSpec.Parse ("\"real(addr)\"@[some literal domain]") });
			var parts = builder.GetParts ();
			Assert.Equal (HeaderFieldName.Supersedes, builder.Name);
			Assert.Equal (3, parts.Count);
			Assert.Equal ("<someone@someserver.ru>", parts[0]);
			Assert.Equal ("<\"real(addr)\"@someserver.ru>", parts[1]);
			Assert.Equal ("<\"real(addr)\"@[some literal domain]>", parts[2]);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateAtomAndUnstructured ()
		{
			var builder = new HeaderFieldBuilderAtomAndUnstructured (HeaderFieldName.Supersedes, "type", "value");
			var parts = builder.GetParts ();
			Assert.Equal (2, parts.Count);
			Assert.Equal ("type;", parts[0]);
			Assert.Equal ("value", parts[1]);

			builder = new HeaderFieldBuilderAtomAndUnstructured (HeaderFieldName.Supersedes, "dns", "2000 Адресат Один");
			parts = builder.GetParts ();
			Assert.Equal (3, parts.Count);
			Assert.Equal ("dns;", parts[0]);
			Assert.Equal ("2000", parts[1]);
			Assert.Equal (" =?utf-8?B?0JDQtNGA0LXRgdCw0YIg0J7QtNC40L0=?=", parts[2]);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateUnstructuredPair ()
		{
			var builder = new HeaderFieldBuilderUnstructuredPair (HeaderFieldName.Supersedes, "value", null);
			var parts = builder.GetParts ();
			Assert.Equal (1, parts.Count);
			Assert.Equal ("value", parts[0]);

			builder = new HeaderFieldBuilderUnstructuredPair (HeaderFieldName.Supersedes, "Lena's Personal <Joke> List", "слово to the снова");
			parts = builder.GetParts ();
			Assert.Equal (5, parts.Count);
			Assert.Equal ("Lena's", parts[0]);
			Assert.Equal (" Personal", parts[1]);
			Assert.Equal (" <Joke>", parts[2]);
			Assert.Equal (" List;", parts[3]);
			Assert.Equal ("=?utf-8?B?0YHQu9C+0LLQviB0byB0aGUg0YHQvdC+0LLQsA==?=", parts[4]);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateTokensAndDate ()
		{
			var dt = new DateTimeOffset (634726649620000000L, TimeSpan.FromHours (3));
			var builder = new HeaderFieldBuilderTokensAndDate (HeaderFieldName.Supersedes, null, dt);
			var parts = builder.GetParts ();
			Assert.Equal (2, parts.Count);
			Assert.Equal (";", parts[0]);
			Assert.Equal ("15 May 2012 07:49:22 +0300", parts[1]);

			dt = new DateTimeOffset (634726649620000000L, TimeSpan.FromHours (1));
			builder = new HeaderFieldBuilderTokensAndDate (HeaderFieldName.Supersedes, "CAA22933", dt);
			parts = builder.GetParts ();
			Assert.Equal (2, parts.Count);
			Assert.Equal ("CAA22933;", parts[0]);
			Assert.Equal ("15 May 2012 07:49:22 +0100", parts[1]);

			dt = new DateTimeOffset (634726649620000000L, TimeSpan.FromHours (-6));
			builder = new HeaderFieldBuilderTokensAndDate (HeaderFieldName.Supersedes, " CMK-SLNS06.chmk.mechelgroup.ru   CAA22933\t", dt);
			parts = builder.GetParts ();
			Assert.Equal (3, parts.Count);
			Assert.Equal ("CMK-SLNS06.chmk.mechelgroup.ru", parts[0]);
			Assert.Equal ("CAA22933;", parts[1]);
			Assert.Equal ("15 May 2012 07:49:22 -0600", parts[2]);

			dt = new DateTimeOffset (634726649620000000L, TimeSpan.FromHours (10));
			builder = new HeaderFieldBuilderTokensAndDate (HeaderFieldName.Supersedes, "by server10.espc2.mechel.com id CAA22933", dt);
			parts = builder.GetParts ();
			Assert.Equal (5, parts.Count);
			Assert.Equal ("by", parts[0]);
			Assert.Equal ("server10.espc2.mechel.com", parts[1]);
			Assert.Equal ("id", parts[2]);
			Assert.Equal ("CAA22933;", parts[3]);
			Assert.Equal ("15 May 2012 07:49:22 +1000", parts[4]);

			dt = new DateTimeOffset (634726649670000000L, TimeSpan.FromHours (0));
			builder = new HeaderFieldBuilderTokensAndDate (HeaderFieldName.Supersedes, "by CMK-SLNS06.chmk.mechelgroup.ru from server10.espc2.mechel.com ([10.2.21.210])\r\n\twith ESMTP id 2012051507492777-49847", dt);
			parts = builder.GetParts ();
			Assert.Equal (10, parts.Count);
			Assert.Equal ("by", parts[0]);
			Assert.Equal ("CMK-SLNS06.chmk.mechelgroup.ru", parts[1]);
			Assert.Equal ("from", parts[2]);
			Assert.Equal ("server10.espc2.mechel.com", parts[3]);
			Assert.Equal ("([10.2.21.210])", parts[4]);
			Assert.Equal ("with", parts[5]);
			Assert.Equal ("ESMTP", parts[6]);
			Assert.Equal ("id", parts[7]);
			Assert.Equal ("2012051507492777-49847;", parts[8]);
			Assert.Equal ("15 May 2012 07:49:27 +0000", parts[9]);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreatePhraseAndId ()
		{
			var builder = new HeaderFieldBuilderPhraseAndId (HeaderFieldName.Supersedes, "lenas-jokes.da39efc25c530ad145d41b86f7420c3b.021999.localhost", null);
			var parts = builder.GetParts ();
			Assert.Equal (1, parts.Count);
			Assert.Equal ("<lenas-jokes.da39efc25c530ad145d41b86f7420c3b.021999.localhost>", parts[0]);

			builder = new HeaderFieldBuilderPhraseAndId (HeaderFieldName.Supersedes, "lenas-jokes.da39efc25c530ad145d41b86f7420c3b.021999.localhost", "Lena's Personal <Joke> List");
			parts = builder.GetParts ();
			Assert.Equal (5, parts.Count);
			Assert.Equal ("Lena's", parts[0]);
			Assert.Equal (" Personal", parts[1]);
			Assert.Equal (" \"<Joke>\"", parts[2]);
			Assert.Equal (" List", parts[3]);
			Assert.Equal ("<lenas-jokes.da39efc25c530ad145d41b86f7420c3b.021999.localhost>", parts[4]);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreatePhraseList ()
		{
			var builder = new HeaderFieldBuilderPhraseList (HeaderFieldName.Supersedes, Array.Empty<string> ());
			var parts = builder.GetParts ();
			Assert.Equal (0, parts.Count);

			builder = new HeaderFieldBuilderPhraseList (HeaderFieldName.Supersedes, new string[] { "keyword" });
			parts = builder.GetParts ();
			Assert.Equal (1, parts.Count);
			Assert.Equal ("keyword", parts[0]);

			builder = new HeaderFieldBuilderPhraseList (HeaderFieldName.Supersedes, new string[] { "keyword", "KEY WORD", "Richard H. Nixon", "ключслово" });
			parts = builder.GetParts ();
			Assert.Equal (7, parts.Count);
			Assert.Equal ("keyword,", parts[0]);
			Assert.Equal ("KEY", parts[1]);
			Assert.Equal (" WORD,", parts[2]);
			Assert.Equal ("Richard", parts[3]);
			Assert.Equal (" \"H.\"", parts[4]);
			Assert.Equal (" Nixon,", parts[5]);
			Assert.Equal ("=?utf-8?B?0LrQu9GO0YfRgdC70L7QstC+?=", parts[6]);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateMailboxList ()
		{
			var mailboxes = new List<Mailbox> ();
			var builder = new HeaderFieldBuilderMailboxList (HeaderFieldName.Supersedes, mailboxes);
			var parts = builder.GetParts ();
			Assert.Equal (0, parts.Count);

			mailboxes.Add (new Mailbox ("one@mail.ru", "one man"));

			builder = new HeaderFieldBuilderMailboxList (HeaderFieldName.Supersedes, mailboxes);
			parts = builder.GetParts ();
			Assert.Equal (3, parts.Count);
			Assert.Equal ("one", parts[0]);
			Assert.Equal (" man", parts[1]);
			Assert.Equal ("<one@mail.ru>", parts[2]);

			mailboxes.Add (new Mailbox ("two@gmail.ru", "man 2"));
			mailboxes.Add (new Mailbox ("three@hotmail.com"));
			builder = new HeaderFieldBuilderMailboxList (HeaderFieldName.Supersedes, mailboxes);
			parts = builder.GetParts ();
			Assert.Equal (7, parts.Count);
			Assert.Equal ("one", parts[0]);
			Assert.Equal (" man", parts[1]);
			Assert.Equal ("<one@mail.ru>,", parts[2]);
			Assert.Equal ("man", parts[3]);
			Assert.Equal (" 2", parts[4]);
			Assert.Equal ("<two@gmail.ru>,", parts[5]);
			Assert.Equal ("<three@hotmail.com>", parts[6]);

			mailboxes.Clear ();
			mailboxes.Add (new Mailbox ("sp1@mailinator.com", "Адресат Один"));
			mailboxes.Add (new Mailbox ("sp2@mailinator.com", "Адресат Два"));
			mailboxes.Add (new Mailbox ("sp3@mailinator.com", "Адресат Три"));
			builder = new HeaderFieldBuilderMailboxList (HeaderFieldName.Supersedes, mailboxes);
			parts = builder.GetParts ();
			Assert.Equal (6, parts.Count);
			Assert.Equal ("=?utf-8?B?0JDQtNGA0LXRgdCw0YIg0J7QtNC40L0=?=", parts[0]);
			Assert.Equal ("<sp1@mailinator.com>,", parts[1]);
			Assert.Equal ("=?utf-8?B?0JDQtNGA0LXRgdCw0YIg0JTQstCw?=", parts[2]);
			Assert.Equal ("<sp2@mailinator.com>,", parts[3]);
			Assert.Equal ("=?utf-8?B?0JDQtNGA0LXRgdCw0YIg0KLRgNC4?=", parts[4]);
			Assert.Equal ("<sp3@mailinator.com>", parts[5]);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateAngleBracketedList ()
		{
			var builder = new HeaderFieldBuilderAngleBracketedList (HeaderFieldName.Supersedes, Array.Empty<string> ());
			var parts = builder.GetParts ();
			Assert.Equal (0, parts.Count);
	
			builder = new HeaderFieldBuilderAngleBracketedList (HeaderFieldName.Supersedes, new string[] { "mailto:list@host.com?subject=help" });
			parts = builder.GetParts ();
			Assert.Equal (1, parts.Count);
			Assert.Equal ("<mailto:list@host.com?subject=help>", parts[0]);

			var data = new string[]
			{
				"mailto:list@host.com?subject=help",
				"ftp://ftp.host.com/list.txt",
				"magnet:?xt=urn:tree:tiger:Z4URQ35KGEQW3YZZTIM7YXS3OLKLHFJ3M43DPHQ&xl=8539516502&dn=12.mkv",
				"some currently unknown command",
			};
			builder = new HeaderFieldBuilderAngleBracketedList (HeaderFieldName.Supersedes, data);
			parts = builder.GetParts ();
			Assert.Equal (4, parts.Count);
			Assert.Equal ("<mailto:list@host.com?subject=help>,", parts[0]);
			Assert.Equal ("<ftp://ftp.host.com/list.txt>,", parts[1]);
			Assert.Equal ("<magnet:?xt=urn:tree:tiger:Z4URQ35KGEQW3YZZTIM7YXS3OLKLHFJ3M43DPHQ&xl=8539516502&dn=12.mkv>,", parts[2]);
			Assert.Equal ("<some currently unknown command>", parts[3]);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateDispositionNotificationParameterList ()
		{
			var parameters = new DispositionNotificationParameter[]
			{
				new DispositionNotificationParameter ("signed-receipt", DispositionNotificationParameterImportance.Optional, "pkcs7-signature"),
			};
			var builder = new HeaderFieldBuilderDispositionNotificationParameterList (HeaderFieldName.Supersedes, parameters);
			var parts = builder.GetParts ();
			Assert.Equal (1, parts.Count);
			Assert.Equal ("signed-receipt=optional,pkcs7-signature", parts[0]);

			parameters = new DispositionNotificationParameter[]
			{
				new DispositionNotificationParameter ("signed-receipt", DispositionNotificationParameterImportance.Optional, "pkcs7-signature"),
				new DispositionNotificationParameter ("signed-receipt-micalg", DispositionNotificationParameterImportance.Required, "sha1").AddValue ("md5"),
			};
			builder = new HeaderFieldBuilderDispositionNotificationParameterList (HeaderFieldName.Supersedes, parameters);
			parts = builder.GetParts ();
			Assert.Equal (2, parts.Count);
			Assert.Equal ("signed-receipt=optional,pkcs7-signature;", parts[0]);
			Assert.Equal ("signed-receipt-micalg=required,sha1,md5", parts[1]);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateDisposition ()
		{
			var builder = new HeaderFieldBuilderDisposition (HeaderFieldName.Supersedes, "value1", "value2", "value3", Array.Empty<string> ());
			var parts = builder.GetParts ();
			Assert.Equal (2, parts.Count);
			Assert.Equal ("value1/value2;", parts[0]);
			Assert.Equal ("value3", parts[1]);

			builder = new HeaderFieldBuilderDisposition (HeaderFieldName.Supersedes, "manual-action", "MDN-sent-manually", "displayed", new string[] { "value1", "value2", "value3" });
			parts = builder.GetParts ();
			Assert.Equal (2, parts.Count);
			Assert.Equal ("manual-action/MDN-sent-manually;", parts[0]);
			Assert.Equal ("displayed/value1,value2,value3", parts[1]);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void EncodeToBinaryTransportRepresentation ()
		{
			var buf = new byte[1000];

			// один параметр
			var builder = new HeaderFieldBuilderExactValue (HeaderFieldName.Supersedes, "short.value");
			builder.AddParameter ("charset", "koi8-r");
			var size = builder.EncodeToBinaryTransportRepresentation (buf, 78);
			Assert.Equal ("Supersedes: short.value; charset=koi8-r\r\n", Encoding.ASCII.GetString (buf, 0, size));

			builder = new HeaderFieldBuilderExactValue (HeaderFieldName.Supersedes, "short.value");
			builder.AddParameter ("charset", "koi8 r");
			size = builder.EncodeToBinaryTransportRepresentation (buf, 78);
			Assert.Equal ("Supersedes: short.value; charset=\"koi8 r\"\r\n", Encoding.ASCII.GetString (buf, 0, size));

			builder = new HeaderFieldBuilderExactValue (HeaderFieldName.Supersedes, "short.value");
			builder.AddParameter ("charset", "функции");
			size = builder.EncodeToBinaryTransportRepresentation (buf, 78);
			Assert.Equal ("Supersedes: short.value;\r\n charset*0*=utf-8''%D1%84%D1%83%D0%BD%D0%BA%D1%86%D0%B8%D0%B8\r\n", Encoding.ASCII.GetString (buf, 0, size));

			// несколько параметров
			builder = new HeaderFieldBuilderExactValue (HeaderFieldName.Supersedes, "short.value");
			builder.AddParameter ("name1", "value1");
			builder.AddParameter ("charset", "koi8-r");
			builder.AddParameter ("name2", "value2");
			size = builder.EncodeToBinaryTransportRepresentation (buf, 78);
			Assert.Equal ("Supersedes: short.value; name1=value1; charset=koi8-r; name2=value2\r\n", Encoding.ASCII.GetString (buf, 0, size));

			// оптимальное кодирования длинного значения параметра
			builder = new HeaderFieldBuilderExactValue (HeaderFieldName.Supersedes, "value");
			builder.AddParameter ("filename", "This document specifies an Internet standards track protocol for the функции and requests discussion and suggestions.txt");
			size = builder.EncodeToBinaryTransportRepresentation (buf, 78);
			Assert.Equal (
				"Supersedes: value;\r\n" +
				" filename*0*=utf-8''This%20document%20specifies%20an%20Internet%20standards;\r\n" +
				" filename*1*=%20track%20protocol%20for%20the%20%D1%84%D1%83%D0%BD%D0%BA%D1%86;\r\n" +
				" filename*2*=%D0%B8%D0%B8%20and%20requests%20discussion%20and%20suggestions.t;\r\n" +
				" filename*3=xt\r\n",
				Encoding.ASCII.GetString (buf, 0, size));

			// всё вместе
			builder = new HeaderFieldBuilderExactValue (HeaderFieldName.ContentDisposition, "attachment");
			builder.AddParameter ("filename", "This document specifies an Internet standards track protocol for the функции and requests discussion and suggestions.txt");
			builder.AddParameter ("modification-date", "24 Nov 2011 09:48:27 +0600");
			builder.AddParameter ("creation-date", "10 Jul 2012 10:01:06 +0600");
			builder.AddParameter ("read-date", "11 Jul 2012 10:40:13 +0600");
			builder.AddParameter ("size", "318");
			size = builder.EncodeToBinaryTransportRepresentation (buf, 78);
			Assert.Equal (
				"Content-Disposition: attachment;\r\n" +
				" filename*0*=utf-8''This%20document%20specifies%20an%20Internet%20standards;\r\n" +
				" filename*1*=%20track%20protocol%20for%20the%20%D1%84%D1%83%D0%BD%D0%BA%D1%86;\r\n" +
				" filename*2*=%D0%B8%D0%B8%20and%20requests%20discussion%20and%20suggestions.t;\r\n" +
				" filename*3=xt; modification-date=\"24 Nov 2011 09:48:27 +0600\";\r\n" +
				" creation-date=\"10 Jul 2012 10:01:06 +0600\";\r\n" +
				" read-date=\"11 Jul 2012 10:40:13 +0600\"; size=318\r\n",
				Encoding.ASCII.GetString (buf, 0, size));
		}
	}
}
