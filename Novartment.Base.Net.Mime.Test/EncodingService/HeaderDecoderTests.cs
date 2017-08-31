using System;
using System.Text;
using System.Threading;
using Novartment.Base.BinaryStreaming;
using Xunit;

namespace Novartment.Base.Net.Mime.Test
{
	public class HeaderDecoderTests
	{
		public HeaderDecoderTests ()
		{
			Encoding.RegisterProvider (CodePagesEncodingProvider.Instance);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderDecoder")]
		public void DecodeAtom ()
		{
			Assert.Equal ("Header-Test.2012", HeaderDecoder.DecodeAtom ("Header-Test.2012"));
			Assert.Equal ("no.name", HeaderDecoder.DecodeAtom (" (dd klk 2012) (222 333) no.name (eee 2002 w) "));
			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeAtom ("a b"));
			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeAtom ("<ab>"));
			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeAtom ("a=b"));
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderDecoder")]
		public void DecodeUnstructured ()
		{
			// проверяем что не корректно декодируется пустое значение
			Assert.Equal (string.Empty, HeaderDecoder.DecodeUnstructured (string.Empty));

			// проверяем что не корректно декодируется значение только из пробельных символов
			Assert.Equal ("\t  \t", HeaderDecoder.DecodeUnstructured ("\t  \t"));
			Assert.Equal ("Simple", HeaderDecoder.DecodeUnstructured ("Simple"));

			// проверяем что не теряются пробельные символы
			Assert.Equal ("\t Simple  \" no\\t \\ quoted \"  Text\" ", HeaderDecoder.DecodeUnstructured ("\t Simple  \" no\\t \\ quoted \"  Text\" "));

			// проверяем что НЕ распознаются треугольные кавычки
			Assert.Equal ("\t Simple  < not addr >  Text ", HeaderDecoder.DecodeUnstructured ("\t Simple  < not addr >  Text "));

			// проверяем что НЕ распознаются круглые кавычки
			Assert.Equal ("\t Simple  (not\tcomment)  ;Text ", HeaderDecoder.DecodeUnstructured ("\t Simple  (not\tcomment)  ;Text "));

			// проверяем что распознаются кодированные слова отделённые пробелом
			Assert.Equal ("aa 123;abc", HeaderDecoder.DecodeUnstructured ("aa =?utf-8?Q?123;abc?="));

			// проверяем что НЕ распознаются кодированные слова НЕ отделённые пробелом
			Assert.Equal ("aa=?utf-8?Q?123;abc?=", HeaderDecoder.DecodeUnstructured ("aa=?utf-8?Q?123;abc?="));
			Assert.Equal ("\t Simple  (not\tcomment)  Text ", HeaderDecoder.DecodeUnstructured ("=?koi8-r?Q?=09_Simple__(not=09comment)__Text_?="));

			// проверяем что НЕ соседние кодированные слова склеиваются корректно (с сохранением пробелов)
			Assert.Equal ("123;abc aa\t\t456;def", HeaderDecoder.DecodeUnstructured ("=?utf-8?Q?123;abc?= aa\t\t=?utf-8?Q?456;def?="));

			// проверяем что соседние (разделённые пробелами) кодированные слова склеиваются корректно (без пробелов)
			Assert.Equal ("Идея состоит в томКонстантин Теличко", HeaderDecoder.DecodeUnstructured ("=?windows-1251?Q?=C8=E4=E5=FF_=F1=EE=F1=F2=EE=E8=F2_=E2_=F2=EE=EC?=\t  =?koi8-r?B?68/O09TBztTJziD0xczJ3svP?="));
			Assert.Equal (
				"Simple=?windows-1251?Q?=C8=E4=E5=FF_=F1=EE=F1=F2=EE=E8=F2_=E2_=F2=EE=EC?=\t  Константин Теличко",
				HeaderDecoder.DecodeUnstructured ("Simple=?windows-1251?Q?=C8=E4=E5=FF_=F1=EE=F1=F2=EE=E8=F2_=E2_=F2=EE=EC?=\t  =?koi8-r?B?68/O09TBztTJziD0xczJ3svP?="));
			Assert.Equal (
				"Simple Идея состоит в томКонстантин Теличко",
				HeaderDecoder.DecodeUnstructured ("Simple =?windows-1251?Q?=C8=E4=E5=FF_=F1=EE=F1=F2=EE=E8=F2_=E2_=F2=EE=EC?=\t  =?koi8-r?B?68/O09TBztTJziD0xczJ3svP?="));
			Assert.Equal ("\tПри 2012г усиленных мерах безопасности", HeaderDecoder.DecodeUnstructured (
				"\t=?utf-8?B?0J/RgNC4?= =?utf-8?Q?_2012=D0=B3?=" +
				"\t=?utf-8?B?INGD0YHQuNC70LXQvdC90YvRhQ==?= =?utf-8?B?INC80LXRgNCw0YU=?=" +
				"\t=?utf-8?B?INCx0LXQt9C+0L/QsNGB0L3QvtGB0YLQuA==?="));
			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeUnstructured ("Simp\u00d7le"));
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderDecoder")]
		public void DecodePhrase ()
		{
			Assert.Equal ("source", HeaderDecoder.DecodePhrase ("source"));
			Assert.Equal ("source one", HeaderDecoder.DecodePhrase ("source one"));
			Assert.Equal ("source one", HeaderDecoder.DecodePhrase ("source  one"));
			Assert.Equal ("source one", HeaderDecoder.DecodePhrase (" source \t one  "));
			Assert.Equal ("source source.net Один", HeaderDecoder.DecodePhrase ("source (dd klk 2012) source.net =?utf-8?B?0J7QtNC40L0=?="));
			Assert.Equal (
				"Join your peers at Eloqua Experience 2013 при best practices and road-tested дисплеем с разрешением порядка",
				HeaderDecoder.DecodePhrase (
					"Join your peers at Eloqua Experience 2013 =?utf-8?B?0L/RgNC4?= best practices " +
					"and road-tested (strategies (that) get results) " +
					"=?utf-8?B?0LTQuNGB0L/Qu9C10LXQvA==?= =?utf-8?B?INGB?= " +
					"=?utf-8?B?INGA0LDQt9GA0LXRiNC10L3QuNC10Lw=?= =?utf-8?B?INC/0L7RgNGP0LTQutCw?="));
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderDecoder")]
		public void DecodeAtomList ()
		{
			var arr = HeaderDecoder.DecodeAtomList ("ru-ru");
			Assert.Equal (1, arr.Count);
			Assert.Equal ("ru-ru", arr[0]);

			arr = HeaderDecoder.DecodeAtomList (" ru-ru,\t   (not\tcomment)  Text,\t1-2-3-4 ");
			Assert.Equal (3, arr.Count);
			Assert.Equal ("ru-ru", arr[0]);
			Assert.Equal ("Text", arr[1]);
			Assert.Equal ("1-2-3-4", arr[2]);

			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeAtomList ("atom,a b"));
			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeAtomList ("atom,<ab>"));
			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeAtomList ("atom,a=b"));
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderDecoder")]
		public void DecodeNotificationFieldValue ()
		{
			var value = HeaderDecoder.DecodeNotificationFieldValue ("\trfc822 (mail-addres) ; louisl@larry.slip.umd.edu");
			Assert.Equal (NotificationFieldValueKind.Mailbox, value.Kind);
			Assert.Equal ("louisl@larry.slip.umd.edu", value.Value);

			value = HeaderDecoder.DecodeNotificationFieldValue (" smtp ; =?utf-8?B?0LrRgtC+LdGC0L4=?=  2012 ");
			Assert.Equal (NotificationFieldValueKind.Status, value.Kind);
			Assert.Equal ("кто-то  2012", value.Value);

			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeNotificationFieldValue ("atom;"));
			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeNotificationFieldValue (";"));
			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeNotificationFieldValue ("type=id;"));
			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeNotificationFieldValue ("esmtp;value"));
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderDecoder")]
		public void DecodeUnstructuredPair ()
		{
			var data = HeaderDecoder.DecodeUnstructuredPair ("\t Simple  < not addr >  Text ");
			Assert.Equal ("\t Simple  < not addr >  Text ", data.Value1);
			Assert.Null (data.Value2);

			data = HeaderDecoder.DecodeUnstructuredPair ("\t Simple  < not addr >  Text ; \t=?utf-8?B?0J/RgNC4?= =?utf-8?Q?_2012=D0=B3?=\t ");
			Assert.Equal ("\t Simple  < not addr >  Text ", data.Value1);
			Assert.Equal (" \tПри 2012г\t ", data.Value2);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderDecoder")]
		public void DecodeUnstructuredAndDate ()
		{
			var data = HeaderDecoder.DecodeUnstructuredAndDate ("\t Simple  < not addr >  Text ; Tue, 15 May 2012 02:49:22 +0100 (comment 2 here) ");
			Assert.Equal ("\t Simple  < not addr >  Text ", data.Text);
			Assert.Equal (new DateTimeOffset (2012, 5, 15, 2, 49, 22, new TimeSpan (1, 0, 0)), data.Time);

			data = HeaderDecoder.DecodeUnstructuredAndDate ("Simple Text ; Tue, 15 May 2012 02:49:22 +0100");
			Assert.Equal ("Simple Text ", data.Text);
			Assert.Equal (new DateTimeOffset (2012, 5, 15, 2, 49, 22, new TimeSpan (1, 0, 0)), data.Time);

			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeUnstructuredAndDate ("Tue, 15 May 2012 02:49:22 + 0100"));
		}

		[Fact]
		[Trait ("Category", "Mime")]
		public void DecodePhraseAndId ()
		{
			var data = HeaderDecoder.DecodePhraseAndId (" ( comment here )    <some.id@someserver.com> ");
			Assert.Null (data.Value1);
			Assert.Equal ("some.id@someserver.com", data.Value2);

			data = HeaderDecoder.DecodePhraseAndId ("\t Simple  (not\tcomment)  Text     <some.id@someserver.com>");
			Assert.Equal ("Simple Text", data.Value1);
			Assert.Equal ("some.id@someserver.com", data.Value2);

			data = HeaderDecoder.DecodePhraseAndId ("\t=?utf-8?B?0J/RgNC4?= =?utf-8?Q?_2012=D0=B3?=\t<some.id@someserver.com> ");
			Assert.Equal ("При 2012г", data.Value1);
			Assert.Equal ("some.id@someserver.com", data.Value2);

			Assert.Throws<FormatException> (() => HeaderDecoder.DecodePhraseAndId (string.Empty));
			Assert.Throws<FormatException> (() => HeaderDecoder.DecodePhraseAndId ("Some Text postmaser@someserver.com"));
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderDecoder")]
		public void DecodePhraseList ()
		{
			var arr = HeaderDecoder.DecodePhraseList ("ru-ru");
			Assert.Equal (1, arr.Count);
			Assert.Equal ("ru-ru", arr[0]);

			arr = HeaderDecoder.DecodePhraseList (" ru-ru,\t Simple  (not\tcomment)  Text,\t=?utf-8?B?0J/RgNC4?= =?utf-8?Q?_2012=D0=B3?= ");
			Assert.Equal (3, arr.Count);
			Assert.Equal ("ru-ru", arr[0]);
			Assert.Equal ("Simple Text", arr[1]);
			Assert.Equal ("При 2012г", arr[2]);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderDecoder")]
		public void DecodeAddrSpecList ()
		{
			var addrs = HeaderDecoder.DecodeAddrSpecList ("(return path here) < (none) > ", true);
			Assert.Equal (0, addrs.Count);
			addrs = HeaderDecoder.DecodeAddrSpecList ("\troot@server10.espc2.mechel.com", true);
			Assert.Equal (1, addrs.Count);
			Assert.Equal ("root", addrs[0].LocalPart);
			Assert.Equal ("server10.espc2.mechel.com", addrs[0].Domain);
			addrs = HeaderDecoder.DecodeAddrSpecList ("\t <\"sp1 <d>\"@[some...strange domain]>  ");
			Assert.Equal (1, addrs.Count);
			Assert.Equal ("sp1 <d>", addrs[0].LocalPart);
			Assert.Equal ("some...strange domain", addrs[0].Domain);
			addrs = HeaderDecoder.DecodeAddrSpecList ("\t<root@server10.espc2.mechel.com>\t <\"sp1 <d>\"@[some...strange domain]> < 2V1WYw6Z100000137@itc-serv01.chmk.mechelgroup.ru> ");
			Assert.Equal (3, addrs.Count);
			Assert.Equal ("root", addrs[0].LocalPart);
			Assert.Equal ("server10.espc2.mechel.com", addrs[0].Domain);
			Assert.Equal ("sp1 <d>", addrs[1].LocalPart);
			Assert.Equal ("some...strange domain", addrs[1].Domain);
			Assert.Equal ("2V1WYw6Z100000137", addrs[2].LocalPart);
			Assert.Equal ("itc-serv01.chmk.mechelgroup.ru", addrs[2].Domain);

			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeAddrSpecList ("<postmaster@server.com> report@gov.ru"));
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderDecoder")]
		public void DecodeMailboxList ()
		{
			var arr = HeaderDecoder.DecodeMailboxList ("no.name@mailinator.com,\t\"Recipient A.B. \\\"First\\\"\" <sp1@[some strange domain]>,\t=?windows-1251?Q?new_=F1=EE=E2=F1=E5=EC_one_222?= <\"namewith,comma\"@mailinator.com>,\t=?windows-1251?Q?=C8=E4=E5=FF_=F1=EE=F1=F2=EE=E8=F2_=E2_=F2=EE=EC=2C_=F7?=\t=?windows-1251?Q?=F2=EE=E1=FB_=EF=E8=F1=E0=F2=FC_=F2=E5=F1=F2=FB_=E4=EB=FF_?=\t=?windows-1251?Q?=EA=E0=E6=E4=EE=E9_=ED=E5=F2=F0=E8=E2=E8=E0=EB=FC=ED=EE=E9?=\t=?windows-1251?Q?_=F4=F3=ED=EA=F6=E8=E8_=E8=EB=E8_=EC=E5=F2=EE=E4=E0?= <sp3@mailinator.com>");
			Assert.Equal (4, arr.Count);
			Assert.Null (arr[0].Name);
			Assert.Equal ("no.name", arr[0].Address.LocalPart);
			Assert.Equal ("mailinator.com", arr[0].Address.Domain);
			Assert.Equal ("Recipient A.B. \"First\"", arr[1].Name);
			Assert.Equal ("sp1", arr[1].Address.LocalPart);
			Assert.Equal ("some strange domain", arr[1].Address.Domain);
			Assert.Equal ("new совсем one 222", arr[2].Name);
			Assert.Equal ("namewith,comma", arr[2].Address.LocalPart);
			Assert.Equal ("mailinator.com", arr[2].Address.Domain);
			Assert.Equal ("Идея состоит в том, чтобы писать тесты для каждой нетривиальной функции или метода", arr[3].Name);
			Assert.Equal ("sp3", arr[3].Address.LocalPart);
			Assert.Equal ("mailinator.com", arr[3].Address.Domain);

			arr = HeaderDecoder.DecodeMailboxList ("=?utf-8?Q?2012_=D0=B3?= <one@mail.ru>, \"man 2\" <two@gmail.ru>, three@hotmail.com, \"Price of Persia\" <prince@persia.com>, \"King of Scotland\" <king.scotland@server.net>");
			Assert.Equal (5, arr.Count);
			Assert.Equal ("2012 г", arr[0].Name);
			Assert.Equal ("one", arr[0].Address.LocalPart);
			Assert.Equal ("mail.ru", arr[0].Address.Domain);
			Assert.Equal ("man 2", arr[1].Name);
			Assert.Equal ("two", arr[1].Address.LocalPart);
			Assert.Equal ("gmail.ru", arr[1].Address.Domain);
			Assert.Null (arr[2].Name);
			Assert.Equal ("three", arr[2].Address.LocalPart);
			Assert.Equal ("hotmail.com", arr[2].Address.Domain);
			Assert.Equal ("Price of Persia", arr[3].Name);
			Assert.Equal ("prince", arr[3].Address.LocalPart);
			Assert.Equal ("persia.com", arr[3].Address.Domain);
			Assert.Equal ("King of Scotland", arr[4].Name);
			Assert.Equal ("king.scotland", arr[4].Address.LocalPart);
			Assert.Equal ("server.net", arr[4].Address.Domain);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderDecoder")]
		public void DecodeQualityValueParameterList ()
		{
			var arr = HeaderDecoder.DecodeQualityValueParameterList ("ru-ru");
			Assert.Equal (1, arr.Count);
			Assert.Equal (1.0M, arr[0].Importance);
			Assert.Equal ("ru-ru", arr[0].Value);

			arr = HeaderDecoder.DecodeQualityValueParameterList ("ru-ru,ru,en-us");
			Assert.Equal (3, arr.Count);
			Assert.Equal ("ru-ru", arr[0].Value);
			Assert.Equal ("ru", arr[1].Value);
			Assert.Equal ("en-us", arr[2].Value);

			arr = HeaderDecoder.DecodeQualityValueParameterList ("ru-ru,ru;q=0.8,en-us;q=0.5,en;q=0.3");
			Assert.Equal (4, arr.Count);
			Assert.Equal (1.0M, arr[0].Importance);
			Assert.Equal ("ru-ru", arr[0].Value);
			Assert.Equal (0.8M, arr[1].Importance);
			Assert.Equal ("ru", arr[1].Value);
			Assert.Equal (0.5M, arr[2].Importance);
			Assert.Equal ("en-us", arr[2].Value);
			Assert.Equal (0.3M, arr[3].Importance);
			Assert.Equal ("en", arr[3].Value);

			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeQualityValueParameterList ("<ru>;q=0.8,en-us;q=0.5"));
			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeQualityValueParameterList ("ru:q=0.8,en-us;q=0.5"));
			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeQualityValueParameterList ("ru;i=0.8,en-us;q=0.5"));
			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeQualityValueParameterList ("ru;q-0.8,en-us;q=0.5"));
			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeQualityValueParameterList ("ru;q=<0.8>,en-us;q=0.5"));
			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeQualityValueParameterList ("ru;q=0.a8,en-us;q=0.5"));
			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeQualityValueParameterList ("ru;q=-0.8,en-us;q=0.5"));
			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeQualityValueParameterList ("ru;q=1.8,en-us;q=0.5"));
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderDecoder")]
		public void DecodeAngleBracketedlList ()
		{
			var arr = HeaderDecoder.DecodeAngleBracketedlList ("<ru-ru>");
			Assert.Equal (1, arr.Count);
			Assert.Equal ("ru-ru", arr[0]);

			arr = HeaderDecoder.DecodeAngleBracketedlList (" (no one) <ru-ru>,\t ( comment here )    <some.id@someserver.com> ");
			Assert.Equal (2, arr.Count);
			Assert.Equal ("ru-ru", arr[0]);
			Assert.Equal ("some.id@someserver.com", arr[1]);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderDecoder")]
		public void DecodeVersion ()
		{
			var data = HeaderDecoder.DecodeVersion ("1.0");
			Assert.Equal (1, data.Major);
			Assert.Equal (0, data.Minor);

			data = HeaderDecoder.DecodeVersion (" ( comment here )  2 . ( new) 5 ");
			Assert.Equal (2, data.Major);
			Assert.Equal (5, data.Minor);

			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeVersion ("1.0 a"));
			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeVersion ("1;0"));
			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeVersion ("1.<0>"));
			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeVersion ("1a.0"));
			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeVersion ("1.b0"));
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderDecoder")]
		public void DecodeAtomAndParameterList ()
		{
			var data = HeaderDecoder.DecodeAtomAndParameterList ("text/plain");
			Assert.Equal ("text/plain", data.Text);
			var arr = data.Parameters;
			Assert.Equal (0, arr.Count);

			data = HeaderDecoder.DecodeAtomAndParameterList (" text/plain\t;\t   format = flowed (obsolette) ;\t   charset= \"koi8-r\"  ; reply-type = original  ");
			Assert.Equal ("text/plain", data.Text);
			arr = data.Parameters;
			Assert.Equal (3, arr.Count);
			Assert.Equal ("format", arr[0].Name);
			Assert.Equal ("flowed", arr[0].Value);
			Assert.Equal ("charset", arr[1].Name);
			Assert.Equal ("koi8-r", arr[1].Value);
			Assert.Equal ("reply-type", arr[2].Name);
			Assert.Equal ("original", arr[2].Value);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderDecoder")]
		public void DecodeDispositionAction ()
		{
			var data = HeaderDecoder.DecodeDispositionAction ("manual-action/MDN-sent-manually;displayed");
			Assert.Equal ("manual-action", data.Value1);
			Assert.Equal ("MDN-sent-manually", data.Value2);
			Assert.Equal ("displayed", data.Value3);
			Assert.Equal (0, data.List.Count);

			data = HeaderDecoder.DecodeDispositionAction ("\t automatic-action /  MDN-sent-automatically  ; deleted / some1, some2 , some3 ");
			Assert.Equal ("automatic-action", data.Value1);
			Assert.Equal ("MDN-sent-automatically", data.Value2);
			Assert.Equal ("deleted", data.Value3);
			Assert.Equal (3, data.List.Count);
			Assert.Equal ("some1", data.List[0]);
			Assert.Equal ("some2", data.List[1]);
			Assert.Equal ("some3", data.List[2]);

			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeDispositionAction (
				"manual-action/MDN-sent-manually"));
			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeDispositionAction (
				"manual-action;MDN-sent-manually;displayed"));
			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeDispositionAction (
				"manual-action/MDN-sent-manually/displayed"));
			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeDispositionAction (
				"manual-action/<MDN-sent-manually>;displayed"));
			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeDispositionAction (
				"manual-action/MDN-sent-manually;<displayed>"));
			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeDispositionAction (
				"<manual-action>/MDN-sent-manually;displayed"));
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderDecoder")]
		public void DecodeDispositionNotificationParameterList ()
		{
			var arr = HeaderDecoder.DecodeDispositionNotificationParameterList ("signed-receipt-protocol=optional,pkcs7-signature");
			Assert.Equal (1, arr.Count);
			Assert.Equal ("signed-receipt-protocol", arr[0].Name);
			Assert.Equal (DispositionNotificationParameterImportance.Optional, arr[0].Importance);
			Assert.Equal (1, arr[0].Values.Count);
			Assert.Equal ("pkcs7-signature", arr[0].Values[0]);

			arr = HeaderDecoder.DecodeDispositionNotificationParameterList ("  signed-receipt-protocol = optional ,  pkcs7-signature (obsolette)  ; \t   signed-receipt-micalg = required , sha1   , ( second ) md5  ");
			Assert.Equal (2, arr.Count);
			Assert.Equal ("signed-receipt-protocol", arr[0].Name);
			Assert.Equal (DispositionNotificationParameterImportance.Optional, arr[0].Importance);
			Assert.Equal (1, arr[0].Values.Count);
			Assert.Equal ("pkcs7-signature", arr[0].Values[0]);
			Assert.Equal ("signed-receipt-micalg", arr[1].Name);
			Assert.Equal (DispositionNotificationParameterImportance.Required, arr[1].Importance);
			Assert.Equal (2, arr[1].Values.Count);
			Assert.Equal ("sha1", arr[1].Values[0]);
			Assert.Equal ("md5", arr[1].Values[1]);

			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeDispositionNotificationParameterList (
				"signed-receipt-protocol=optional,"));
			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeDispositionNotificationParameterList (
				"signed-receipt-protocol,optional,pkcs7-signature"));
			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeDispositionNotificationParameterList (
				"signed-receipt-protocol=optional=pkcs7-signature"));
			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeDispositionNotificationParameterList (
				"<signed-receipt-protocol>=optional,pkcs7-signature"));
			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeDispositionNotificationParameterList (
				"signed-receipt-protocol=[optional],pkcs7-signature"));
			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeDispositionNotificationParameterList (
				"signed-receipt-protocol=optional,<pkcs7-signature>"));
			Assert.Throws<FormatException> (() => HeaderDecoder.DecodeDispositionNotificationParameterList (
				"signed-receipt-protocol=ignore,pkcs7-signature"));
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderDecoder")]
		public void LoadHeaderFields ()
		{
			var src = new ArrayBufferedSource (Encoding.ASCII.GetBytes (string.Empty));
			var fields = HeaderDecoder.LoadHeaderFieldsAsync (src, CancellationToken.None).Result;
			Assert.Equal (0, fields.Count);

			src = new ArrayBufferedSource (Encoding.ASCII.GetBytes (
				"Content-Type: text/plain;\r\n\tformat=flowed;\r\n\tcharset=\"koi8-r\";\r\n\treply-type=original\r\n" +
				"Prevent-NonDelivery-Report:\r\n" +
				"InvalidField\r\n" +
				"Received:\r\n\tby server10.espc2.mechel.com (8.8.8/1.37)\r\n\tid CAA22933; Tue, 15 May 2012 02:49:22 +0100   \r\n" +
				"Autoforwarded::Q2hlY2sgSW50ZWdyaXR5IQ=="));
			fields = HeaderDecoder.LoadHeaderFieldsAsync (src, CancellationToken.None).Result;
			Assert.Equal (4, fields.Count);
			Assert.Equal (HeaderFieldName.ContentType, fields[0].Name);
			Assert.Equal ("text/plain;\tformat=flowed;\tcharset=\"koi8-r\";\treply-type=original", fields[0].Value);
			Assert.Equal (HeaderFieldName.PreventNonDeliveryReport, fields[1].Name);
			Assert.Equal (string.Empty, fields[1].Value);
			Assert.Equal (HeaderFieldName.Received, fields[2].Name);
			Assert.Equal ("by server10.espc2.mechel.com (8.8.8/1.37)\tid CAA22933; Tue, 15 May 2012 02:49:22 +0100", fields[2].Value);
			Assert.Equal (HeaderFieldName.AutoForwarded, fields[3].Name);
			Assert.Equal (":Q2hlY2sgSW50ZWdyaXR5IQ==", fields[3].Value);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderDecoder")]
		public void ParseFoldedField ()
		{
			var src = new ArrayBufferedSource (Encoding.ASCII.GetBytes ("x-Type:"));
			var field = HeaderDecoder.ParseFoldedFieldAsync (src, CancellationToken.None).Result;
			Assert.IsType<ExtensionHeaderField> (field);
			Assert.Equal (HeaderFieldName.Extension, field.Name);
			Assert.Equal (string.Empty, field.Value);
			Assert.Equal ("x-Type", ((ExtensionHeaderField)field).ExtensionName);

			src = new ArrayBufferedSource (Encoding.ASCII.GetBytes (
				"content-Type:  \r\n   text/plain;\r\n\t\t\t\tformat=flowed;\r\n\t\tcharset=\"koi8-r\";\r\n\treply-type=original  \t \t"));
			field = HeaderDecoder.ParseFoldedFieldAsync (src, CancellationToken.None).Result;
			Assert.Equal (HeaderFieldName.ContentType, field.Name);
			Assert.Equal ("text/plain;\t\t\t\tformat=flowed;\t\tcharset=\"koi8-r\";\treply-type=original", field.Value);

			src = new ArrayBufferedSource (Encoding.ASCII.GetBytes ("Subject=about"));
			Assert.ThrowsAsync<FormatException> (() => HeaderDecoder.ParseFoldedFieldAsync (src, CancellationToken.None));
		}
	}
}
