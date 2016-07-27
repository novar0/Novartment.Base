using System;
using System.Collections.Generic;

using Xunit;

namespace Novartment.Base.Net.Mime.Test
{
	public class HeaderFieldBuilderTests
	{
		[Fact, Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateExactValue ()
		{
			// invalid characters
			var headerField = HeaderFieldBuilder.CreateExactValue (HeaderFieldName.Supersedes, "short\r\n.   \tvalue  ЮУЖД").ToHeaderField (int.MaxValue);
			Assert.Equal (HeaderFieldName.Supersedes, headerField.Name);
			Assert.Equal ("short\r\n.   \tvalue  ЮУЖД", headerField.Value);

			// one parameter
			var headerFieldBuilder = HeaderFieldBuilder.CreateExactValue (HeaderFieldName.Supersedes, "short.value");
			headerFieldBuilder.AddParameter ("charset", "koi8-r");
			headerField = headerFieldBuilder.ToHeaderField (int.MaxValue);
			Assert.Equal (HeaderFieldName.Supersedes, headerField.Name);
			Assert.Equal ("short.value; charset=koi8-r", headerField.Value);

			// many parameters
			headerFieldBuilder = HeaderFieldBuilder.CreateExactValue (HeaderFieldName.Supersedes, "short.value");
			headerFieldBuilder.AddParameter ("name1", "value1");
			headerFieldBuilder.AddParameter ("charset", "koi8-r");
			headerFieldBuilder.AddParameter ("name2", "value2");
			headerField = headerFieldBuilder.ToHeaderField (int.MaxValue);
			Assert.Equal (HeaderFieldName.Supersedes, headerField.Name);
			Assert.Equal ("short.value; name1=value1; charset=koi8-r; name2=value2", headerField.Value);

			// failed folding
			headerField = HeaderFieldBuilder.CreateExactValue (HeaderFieldName.OriginalEncodedInformationTypes, "really.very.long.value.20000000000000000")
				.ToHeaderField (70);
			Assert.Equal (HeaderFieldName.OriginalEncodedInformationTypes, headerField.Name);
			Assert.Equal ("\r\n really.very.long.value.20000000000000000", headerField.Value);

			// successful folding
			headerField = HeaderFieldBuilder.CreateExactValue (HeaderFieldName.OriginalEncodedInformationTypes, "really.very.long.value.20000000000000000")
				.ToHeaderField (70);
			Assert.Equal (HeaderFieldName.OriginalEncodedInformationTypes, headerField.Name);
			Assert.Equal ("\r\n really.very.long.value.20000000000000000", headerField.Value);

			//
			headerFieldBuilder = HeaderFieldBuilder.CreateExactValue (HeaderFieldName.Supersedes, "value");
			headerFieldBuilder.AddParameter ("charset", "koi8-r");
			headerFieldBuilder.AddParameter ("boundary", "NextPart_fc40ea2198d04f01a1f390890c44e614");
			headerFieldBuilder.AddParameter ("filename", "This document specifies an Internet standards track protocol for the функции and requests discussion and suggestions.txt");
			headerField = headerFieldBuilder.ToHeaderField (78);
			Assert.Equal (HeaderFieldName.Supersedes, headerField.Name);
			Assert.Equal (
				"value; charset=koi8-r;\r\n" +
				" boundary=NextPart_fc40ea2198d04f01a1f390890c44e614;\r\n" +
				" filename*0*=utf-8''This%20document%20specifies%20an%20Internet%20standards;\r\n" +
				" filename*1=\" track protocol for the \";\r\n" +
				" filename*2*=%D1%84%D1%83%D0%BD%D0%BA%D1%86%D0%B8%D0%B8;\r\n" +
				" filename*3=\" and requests discussion and suggestions.txt\"",
				headerField.Value);
		}

		[Fact, Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateAtomAndUnstructured ()
		{
			var values = HeaderFieldBuilder.CreateAtomAndUnstructured (HeaderFieldName.Supersedes, "type", "value").ToHeaderField (int.MaxValue);
			Assert.Equal ("type; value", values.Value);

			values = HeaderFieldBuilder.CreateAtomAndUnstructured (HeaderFieldName.Supersedes, "dns", "2000 Адресат Один").ToHeaderField (int.MaxValue);
			Assert.Equal ("dns; 2000 =?utf-8?B?0JDQtNGA0LXRgdCw0YIg0J7QtNC40L0=?=", values.Value);
		}

		[Fact, Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateUnstructuredPair ()
		{
			var values = HeaderFieldBuilder.CreateUnstructuredPair (HeaderFieldName.Supersedes, "value", null).ToHeaderField (int.MaxValue);
			Assert.Equal ("value", values.Value);

			values = HeaderFieldBuilder.CreateUnstructuredPair (HeaderFieldName.Supersedes, "Lena's Personal <Joke> List", "слово to the снова").ToHeaderField (int.MaxValue);
			Assert.Equal ("Lena's Personal <Joke> List; =?utf-8?B?0YHQu9C+0LLQviB0byB0aGUg0YHQvdC+0LLQsA==?=", values.Value);
		}

		[Fact, Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateTokensAndDate ()
		{
			var dt = new DateTimeOffset (634726649620000000L, TimeSpan.FromHours (3));
			var values = HeaderFieldBuilder.CreateTokensAndDate (HeaderFieldName.Supersedes, null, dt).ToHeaderField (int.MaxValue);
			Assert.Equal ("; 15 May 2012 07:49:22 +0300", values.Value);

			dt = new DateTimeOffset (634726649620000000L, TimeSpan.FromHours (1));
			values = HeaderFieldBuilder.CreateTokensAndDate (HeaderFieldName.Supersedes, "CAA22933", dt).ToHeaderField (int.MaxValue);
			Assert.Equal ("CAA22933; 15 May 2012 07:49:22 +0100", values.Value);

			dt = new DateTimeOffset (634726649620000000L, TimeSpan.FromHours (-6));
			values = HeaderFieldBuilder.CreateTokensAndDate (HeaderFieldName.Supersedes, " CMK-SLNS06.chmk.mechelgroup.ru   CAA22933\t", dt).ToHeaderField (int.MaxValue);
			Assert.Equal ("CMK-SLNS06.chmk.mechelgroup.ru CAA22933; 15 May 2012 07:49:22 -0600", values.Value);

			dt = new DateTimeOffset (634726649620000000L, TimeSpan.FromHours (10));
			values = HeaderFieldBuilder.CreateTokensAndDate (HeaderFieldName.Supersedes, "by server10.espc2.mechel.com id CAA22933", dt).ToHeaderField (int.MaxValue);
			Assert.Equal ("by server10.espc2.mechel.com id CAA22933; 15 May 2012 07:49:22 +1000", values.Value);

			dt = new DateTimeOffset (634726649670000000L, TimeSpan.FromHours (0));
			values = HeaderFieldBuilder.CreateTokensAndDate (HeaderFieldName.Supersedes, "by CMK-SLNS06.chmk.mechelgroup.ru from server10.espc2.mechel.com ([10.2.21.210])\r\n\twith ESMTP id 2012051507492777-49847", dt).ToHeaderField (int.MaxValue);
			Assert.Equal ("by CMK-SLNS06.chmk.mechelgroup.ru from server10.espc2.mechel.com ([10.2.21.210]) with ESMTP id 2012051507492777-49847; 15 May 2012 07:49:27 +0000", values.Value);
		}

		[Fact, Trait ("Category", "Mime.HeaderEncoder")]
		public void CreatePhraseAndId ()
		{
			var values = HeaderFieldBuilder.CreatePhraseAndId (HeaderFieldName.Supersedes, "lenas-jokes.da39efc25c530ad145d41b86f7420c3b.021999.localhost", null).ToHeaderField (int.MaxValue);
			Assert.Equal ("<lenas-jokes.da39efc25c530ad145d41b86f7420c3b.021999.localhost>", values.Value);

			values = HeaderFieldBuilder.CreatePhraseAndId (HeaderFieldName.Supersedes, "lenas-jokes.da39efc25c530ad145d41b86f7420c3b.021999.localhost", "Lena's Personal <Joke> List").ToHeaderField (int.MaxValue);
			Assert.Equal ("Lena's Personal \"<Joke>\" List <lenas-jokes.da39efc25c530ad145d41b86f7420c3b.021999.localhost>", values.Value);
		}

		[Fact, Trait ("Category", "Mime.HeaderEncoder")]
		public void CreatePhraseList ()
		{
			var values = HeaderFieldBuilder.CreatePhraseList (HeaderFieldName.Supersedes, new string[0]).ToHeaderField (int.MaxValue);
			Assert.Equal ("", values.Value);

			values = HeaderFieldBuilder.CreatePhraseList (HeaderFieldName.Supersedes, new string[] { "keyword" }).ToHeaderField (int.MaxValue);
			Assert.Equal ("keyword", values.Value);

			values = HeaderFieldBuilder.CreatePhraseList (HeaderFieldName.Supersedes, new string[] { "keyword", "KEY WORD", "Richard H. Nixon", "ключслово" }).ToHeaderField (int.MaxValue);
			Assert.Equal ("keyword, KEY WORD, Richard \"H.\" Nixon, =?utf-8?B?0LrQu9GO0YfRgdC70L7QstC+?=", values.Value);

		}

		[Fact, Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateMailboxList ()
		{
			var mailboxes = new List<Mailbox> ();
			var values = HeaderFieldBuilder.CreateMailboxList (HeaderFieldName.Supersedes, mailboxes).ToHeaderField (int.MaxValue);
			Assert.Equal ("", values.Value);

			mailboxes.Add (new Mailbox ("one@mail.ru", "one man"));

			values = HeaderFieldBuilder.CreateMailboxList (HeaderFieldName.Supersedes, mailboxes).ToHeaderField (int.MaxValue);
			Assert.Equal ("one man <one@mail.ru>", values.Value);

			mailboxes.Add (new Mailbox ("two@gmail.ru", "man 2"));
			mailboxes.Add (new Mailbox ("three@hotmail.com"));
			values = HeaderFieldBuilder.CreateMailboxList (HeaderFieldName.Supersedes, mailboxes).ToHeaderField (int.MaxValue);
			Assert.Equal ("one man <one@mail.ru>, man 2 <two@gmail.ru>, <three@hotmail.com>", values.Value);

			mailboxes.Clear ();
			mailboxes.Add (new Mailbox ("sp1@mailinator.com", "Адресат Один"));
			mailboxes.Add (new Mailbox ("sp2@mailinator.com", "Адресат Два"));
			mailboxes.Add (new Mailbox ("sp3@mailinator.com", "Адресат Три"));
			values = HeaderFieldBuilder.CreateMailboxList (HeaderFieldName.Supersedes, mailboxes).ToHeaderField (int.MaxValue);
			Assert.Equal ("=?utf-8?B?0JDQtNGA0LXRgdCw0YIg0J7QtNC40L0=?= <sp1@mailinator.com>, =?utf-8?B?0JDQtNGA0LXRgdCw0YIg0JTQstCw?= <sp2@mailinator.com>, =?utf-8?B?0JDQtNGA0LXRgdCw0YIg0KLRgNC4?= <sp3@mailinator.com>", values.Value);
		}

		[Fact, Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateAngleBracketedList ()
		{
			var values = HeaderFieldBuilder.CreateAngleBracketedList (HeaderFieldName.Supersedes, new string[0]).ToHeaderField (int.MaxValue);
			Assert.Equal ("", values.Value);

			values = HeaderFieldBuilder.CreateAngleBracketedList (HeaderFieldName.Supersedes, new string[] { "mailto:list@host.com?subject=help" }).ToHeaderField (int.MaxValue);
			Assert.Equal ("<mailto:list@host.com?subject=help>", values.Value);

			values = HeaderFieldBuilder.CreateAngleBracketedList (HeaderFieldName.Supersedes, new string[] {
				"mailto:list@host.com?subject=help",
				"ftp://ftp.host.com/list.txt",
				"magnet:?xt=urn:tree:tiger:Z4URQ35KGEQW3YZZTIM7YXS3OLKLHFJ3M43DPHQ&xl=8539516502&dn=12.mkv",
				"some currently unknown command"
			}).ToHeaderField (int.MaxValue);
			Assert.Equal ("<mailto:list@host.com?subject=help>, <ftp://ftp.host.com/list.txt>, <magnet:?xt=urn:tree:tiger:Z4URQ35KGEQW3YZZTIM7YXS3OLKLHFJ3M43DPHQ&xl=8539516502&dn=12.mkv>, <some currently unknown command>", values.Value);
		}

		[Fact, Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateDispositionNotificationParameterList ()
		{
			var values = HeaderFieldBuilder.CreateDispositionNotificationParameterList (HeaderFieldName.Supersedes, new DispositionNotificationParameter[] {
				new DispositionNotificationParameter ("signed-receipt", DispositionNotificationParameterImportance.Optional, "pkcs7-signature"),
			}).ToHeaderField (int.MaxValue);
			Assert.Equal ("signed-receipt=optional,pkcs7-signature", values.Value);

			values = HeaderFieldBuilder.CreateDispositionNotificationParameterList (HeaderFieldName.Supersedes, new DispositionNotificationParameter[] {
				new DispositionNotificationParameter ("signed-receipt", DispositionNotificationParameterImportance.Optional, "pkcs7-signature"),
				new DispositionNotificationParameter ("signed-receipt-micalg", DispositionNotificationParameterImportance.Required, "sha1").AddValue ("md5")
			}).ToHeaderField (int.MaxValue);
			Assert.Equal ("signed-receipt=optional,pkcs7-signature; signed-receipt-micalg=required,sha1,md5", values.Value);
		}

		[Fact, Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateDisposition ()
		{
			var bulder = HeaderFieldBuilder.CreateDisposition (HeaderFieldName.Supersedes, "value1", "value2", "value3", new string[0]).ToHeaderField (int.MaxValue);
			Assert.Equal ("value1/value2; value3", bulder.Value);

			bulder = HeaderFieldBuilder.CreateDisposition (HeaderFieldName.Supersedes, "manual-action", "MDN-sent-manually", "displayed", new string[] { "value1", "value2", "value3" }).ToHeaderField (int.MaxValue);
			Assert.Equal ("manual-action/MDN-sent-manually; displayed/value1,value2,value3", bulder.Value);
		}

	}
}
