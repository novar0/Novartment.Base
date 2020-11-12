using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Novartment.Base.Net.Mime.Test
{
	#region класс-обёртки чтобы получить доступ к protected-методам PrepareToEncode() и EncodeNextPart()

	internal sealed class ExposedHeaderFieldBuilderUnstructured : HeaderFieldBuilderUnstructuredValue
	{
		internal ExposedHeaderFieldBuilderUnstructured (HeaderFieldName name, string text) : base (name, text) { }
		internal void PrepareToEncodeExposed (byte[] oneLineBuffer) => base.PrepareToEncode (oneLineBuffer);
		internal int GetNextPartExposed (Span<byte> buf, out bool isLast) => base.EncodeNextPart (buf, out isLast);
	}

	internal sealed class ExposedHeaderFieldBuilderPhrase : HeaderFieldBuilderStructuredValue
	{
		internal ExposedHeaderFieldBuilderPhrase (HeaderFieldName name, string text) : base (name, text) { }
		internal void PrepareToEncodeExposed (byte[] oneLineBuffer) => base.PrepareToEncode (oneLineBuffer);
		internal int GetNextPartExposed (Span<byte> buf, out bool isLast) => base.EncodeNextPart (buf, out isLast);
	}

	internal sealed class ExposedHeaderFieldBuilderMailbox : HeaderFieldBuilderMailbox
	{
		internal ExposedHeaderFieldBuilderMailbox (HeaderFieldName name, Mailbox mailbox) : base (name, mailbox) { }
		internal void PrepareToEncodeExposed (byte[] oneLineBuffer) => base.PrepareToEncode (oneLineBuffer);
		internal int GetNextPartExposed (Span<byte> buf, out bool isLast) => base.EncodeNextPart (buf, out isLast);
	}


	internal sealed class ExposedHeaderFieldBuilderLanguageList : HeaderFieldBuilderLanguageCollection
	{
		internal ExposedHeaderFieldBuilderLanguageList (HeaderFieldName name, IReadOnlyList<string> languages) : base (name, languages) { }
		internal void PrepareToEncodeExposed (byte[] oneLineBuffer) => base.PrepareToEncode (oneLineBuffer);
		internal int GetNextPartExposed (Span<byte> buf, out bool isLast) => base.EncodeNextPart (buf, out isLast);
	}

	internal sealed class ExposedHeaderFieldBuilderAddrSpecList : HeaderFieldBuilderAddrSpecCollection
	{
		internal ExposedHeaderFieldBuilderAddrSpecList (HeaderFieldName name, IReadOnlyList<AddrSpec> addrSpecs) : base (name, addrSpecs) { }
		internal void PrepareToEncodeExposed (byte[] oneLineBuffer) => base.PrepareToEncode (oneLineBuffer);
		internal int GetNextPartExposed (Span<byte> buf, out bool isLast) => base.EncodeNextPart (buf, out isLast);
	}

	internal sealed class ExposedHeaderFieldBuilderAtomAndUnstructured : HeaderFieldBuilderAtomAndUnstructuredValue
	{
		internal ExposedHeaderFieldBuilderAtomAndUnstructured (HeaderFieldName name, string type, string value) : base (name, type, value) { }
		internal void PrepareToEncodeExposed (byte[] oneLineBuffer) => base.PrepareToEncode (oneLineBuffer);
		internal int GetNextPartExposed (Span<byte> buf, out bool isLast) => base.EncodeNextPart (buf, out isLast);
	}

	internal sealed class ExposedHeaderFieldBuilderUnstructuredPair : HeaderFieldBuilderUnstructuredValuePair
	{
		internal ExposedHeaderFieldBuilderUnstructuredPair (HeaderFieldName name, string value1, string value2) : base (name, value1, value2) { }
		internal void PrepareToEncodeExposed (byte[] oneLineBuffer) => base.PrepareToEncode (oneLineBuffer);
		internal int GetNextPartExposed (Span<byte> buf, out bool isLast) => base.EncodeNextPart (buf, out isLast);
	}

	internal sealed class ExposedHeaderFieldBuilderTokensAndDate : HeaderFieldBuilderTokensAndDate
	{
		internal ExposedHeaderFieldBuilderTokensAndDate (HeaderFieldName name, string value, DateTimeOffset dateTimeOffset) : base (name, value, dateTimeOffset) { }
		internal void PrepareToEncodeExposed (byte[] oneLineBuffer) => base.PrepareToEncode (oneLineBuffer);
		internal int GetNextPartExposed (Span<byte> buf, out bool isLast) => base.EncodeNextPart (buf, out isLast);
	}

	internal sealed class ExposedHeaderFieldBuilderPhraseAndId : HeaderFieldBuilderStructuredValueAndId
	{
		internal ExposedHeaderFieldBuilderPhraseAndId (HeaderFieldName name, string id, string phrase) : base (name, id, phrase) { }
		internal void PrepareToEncodeExposed (byte[] oneLineBuffer) => base.PrepareToEncode (oneLineBuffer);
		internal int GetNextPartExposed (Span<byte> buf, out bool isLast) => base.EncodeNextPart (buf, out isLast);
	}

	internal sealed class ExposedHeaderFieldBuilderPhraseList : HeaderFieldBuilderStructuredValueCollection
	{
		internal ExposedHeaderFieldBuilderPhraseList (HeaderFieldName name, IReadOnlyList<string> values) : base (name, values) { }
		internal void PrepareToEncodeExposed (byte[] oneLineBuffer) => base.PrepareToEncode (oneLineBuffer);
		internal int GetNextPartExposed (Span<byte> buf, out bool isLast) => base.EncodeNextPart (buf, out isLast);
	}

	internal sealed class ExposedHeaderFieldBuilderMailboxList : HeaderFieldBuilderMailboxCollection
	{
		internal ExposedHeaderFieldBuilderMailboxList (HeaderFieldName name, IReadOnlyList<Mailbox> mailboxes) : base (name, mailboxes) { }
		internal void PrepareToEncodeExposed (byte[] oneLineBuffer) => base.PrepareToEncode (oneLineBuffer);
		internal int GetNextPartExposed (Span<byte> buf, out bool isLast) => base.EncodeNextPart (buf, out isLast);
	}

	internal sealed class ExposedHeaderFieldBuilderAngleBracketedList : HeaderFieldBuilderAngleBracketedList
	{
		internal ExposedHeaderFieldBuilderAngleBracketedList (HeaderFieldName name, IReadOnlyList<string> urls) : base (name, urls) { }
		internal void PrepareToEncodeExposed (byte[] oneLineBuffer) => base.PrepareToEncode (oneLineBuffer);
		internal int GetNextPartExposed (Span<byte> buf, out bool isLast) => base.EncodeNextPart (buf, out isLast);
	}

	internal sealed class ExposedHeaderFieldBuilderDispositionNotificationParameterList : HeaderFieldBuilderDispositionNotificationParameterCollection
	{
		internal ExposedHeaderFieldBuilderDispositionNotificationParameterList (HeaderFieldName name, IReadOnlyList<DispositionNotificationParameter> parameters) : base (name, parameters) { }
		internal void PrepareToEncodeExposed (byte[] oneLineBuffer) => base.PrepareToEncode (oneLineBuffer);
		internal int GetNextPartExposed (Span<byte> buf, out bool isLast) => base.EncodeNextPart (buf, out isLast);
	}

	internal sealed class ExposedHeaderFieldBuilderDisposition : HeaderFieldBuilderDisposition
	{
		internal ExposedHeaderFieldBuilderDisposition (HeaderFieldName name, string actionMode, string sendingMode, string type, IReadOnlyList<string> modifiers) : base (name, actionMode, sendingMode, type, modifiers) { }
		internal void PrepareToEncodeExposed (byte[] oneLineBuffer) => base.PrepareToEncode (oneLineBuffer);
		internal int GetNextPartExposed (Span<byte> buf, out bool isLast) => base.EncodeNextPart (buf, out isLast);
	}

	#endregion

	public sealed class HeaderFieldBuilderTests
	{
		// добавить тестирование HeaderFieldBuilderExactValue

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateUnstructured ()
		{
			var buf = new byte[100];
			var lineBuf = new byte[1000];
			var builder = new ExposedHeaderFieldBuilderUnstructured (HeaderFieldName.Supersedes, string.Empty);
			builder.PrepareToEncodeExposed (lineBuf);
			var size = builder.GetNextPartExposed (buf, out bool isLast);
			Assert.True (isLast);
			Assert.Equal (0, size);

			builder = new ExposedHeaderFieldBuilderUnstructured (HeaderFieldName.Supersedes, "An 'encoded-word' may, appear: in a message; values or \"body part\" values according слово to the снова rules valuerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerules again");
			builder.PrepareToEncodeExposed (lineBuf);
			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("An", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" 'encoded-word'", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" may,", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" appear:", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" in", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" a", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" message;", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" values", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" or", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" \"body", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" part\"", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" values", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" according", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" =?utf-8?B?0YHQu9C+0LLQviB0byB0aGUg0YHQvdC+0LLQsA==?=", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" rules", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" valuerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerules", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal (" again", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal (0, size);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreatePhrase ()
		{
			var buf = new byte[100];
			var lineBuf = new byte[1000];
			var builder = new ExposedHeaderFieldBuilderPhrase (HeaderFieldName.Supersedes, "An 'encoded-word' may, appear: in a message; values or \"body part\" values according слово to the снова rules valuerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerules again");
			builder.PrepareToEncodeExposed (lineBuf);

			var size = builder.GetNextPartExposed (buf, out bool isLast);
			Assert.False (isLast);
			Assert.Equal ("An", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" 'encoded-word'", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" \"may, appear: in a message; values or \\\"body part\\\"\"", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" values", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" according", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" =?utf-8?B?0YHQu9C+0LLQviB0byB0aGUg0YHQvdC+0LLQsA==?=", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" rules", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" valuerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerulesvaluerules", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal (" again", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal (0, size);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateMailbox ()
		{
			var buf = new byte[100];
			var lineBuf = new byte[1000];
			var builder = new ExposedHeaderFieldBuilderMailbox (HeaderFieldName.Supersedes, new Mailbox (new AddrSpec ("someone", "server.com"), null));
			builder.PrepareToEncodeExposed (lineBuf);

			var size = builder.GetNextPartExposed (buf, out bool isLast);
			Assert.True (isLast);
			Assert.Equal ("<someone@server.com>", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal (0, size);

			builder = new ExposedHeaderFieldBuilderMailbox (HeaderFieldName.Supersedes, new Mailbox (new AddrSpec ("someone", "server.com"), "Dear"));
			builder.PrepareToEncodeExposed (lineBuf);

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("Dear", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal ("<someone@server.com>", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal (0, size);

			builder = new ExposedHeaderFieldBuilderMailbox (HeaderFieldName.Supersedes, new Mailbox (
				new AddrSpec ("really-long-address(for.one.line)", "some literal domain"),
				"Henry Abdula Rabi Ж  (the Third King of the Mooon)"));
			builder.PrepareToEncodeExposed (lineBuf);

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("Henry", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" Abdula", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" Rabi", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" =?utf-8?B?0JY=?=", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" \" (the Third King of the Mooon)\"", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal ("<\"really-long-address(for.one.line)\"@[some literal domain]>", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal (0, size);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateLanguageList ()
		{
			var buf = new byte[100];
			var lineBuf = new byte[1000];
			var builder = new ExposedHeaderFieldBuilderLanguageList (HeaderFieldName.Supersedes, new string[] { "one", "two2", "three-en" });
			builder.PrepareToEncodeExposed (lineBuf);

			var size = builder.GetNextPartExposed (buf, out bool isLast);
			Assert.False (isLast);
			Assert.Equal ("one,", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("two2,", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal ("three-en", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal (0, size);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateAddrSpecList ()
		{
			var buf = new byte[100];
			var lineBuf = new byte[1000];

			var builder = new ExposedHeaderFieldBuilderAddrSpecList (HeaderFieldName.Supersedes, new AddrSpec[] { AddrSpec.Parse ("someone@someserver.ru"), AddrSpec.Parse ("\"real(addr)\"@someserver.ru"), AddrSpec.Parse ("\"real(addr)\"@[some literal domain]") });
			builder.PrepareToEncodeExposed (lineBuf);

			var size = builder.GetNextPartExposed (buf, out bool isLast);
			Assert.False (isLast);
			Assert.Equal ("<someone@someserver.ru>", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("<\"real(addr)\"@someserver.ru>", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal ("<\"real(addr)\"@[some literal domain]>", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal (0, size);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateAtomAndUnstructured ()
		{
			var buf = new byte[100];
			var lineBuf = new byte[1000];
			var builder = new ExposedHeaderFieldBuilderAtomAndUnstructured (HeaderFieldName.Supersedes, "type", "value");
			builder.PrepareToEncodeExposed (lineBuf);

			var size = builder.GetNextPartExposed (buf, out bool isLast);
			Assert.False (isLast);
			Assert.Equal ("type;", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal ("value", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal (0, size);

			builder = new ExposedHeaderFieldBuilderAtomAndUnstructured (HeaderFieldName.Supersedes, "dns", "2000 Адресат Один");
			builder.PrepareToEncodeExposed (lineBuf);

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("dns;", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("2000", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal (" =?utf-8?B?0JDQtNGA0LXRgdCw0YIg0J7QtNC40L0=?=", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal (0, size);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateUnstructuredPair ()
		{
			var buf = new byte[100];
			var lineBuf = new byte[1000];

			var builder = new ExposedHeaderFieldBuilderUnstructuredPair (HeaderFieldName.Supersedes, "value", null);
			builder.PrepareToEncodeExposed (lineBuf);

			var size = builder.GetNextPartExposed (buf, out bool isLast);
			Assert.True (isLast);
			Assert.Equal ("value", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal (0, size);

			builder = new ExposedHeaderFieldBuilderUnstructuredPair (HeaderFieldName.Supersedes, "Lena's Personal <Joke> List", "слово to the снова");
			builder.PrepareToEncodeExposed (lineBuf);

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("Lena's", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" Personal", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" <Joke>", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" List;", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal ("=?utf-8?B?0YHQu9C+0LLQviB0byB0aGUg0YHQvdC+0LLQsA==?=", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal (0, size);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateTokensAndDate ()
		{
			var buf = new byte[100];
			var lineBuf = new byte[1000];

			var dt = new DateTimeOffset (634726649620000000L, TimeSpan.FromHours (3));
			var builder = new ExposedHeaderFieldBuilderTokensAndDate (HeaderFieldName.Supersedes, null, dt);
			builder.PrepareToEncodeExposed (lineBuf);

			var size = builder.GetNextPartExposed (buf, out bool isLast);
			Assert.False (isLast);
			Assert.Equal (";", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal ("15 May 2012 07:49:22 +0300", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal (0, size);

			dt = new DateTimeOffset (634726649620000000L, TimeSpan.FromHours (1));
			builder = new ExposedHeaderFieldBuilderTokensAndDate (HeaderFieldName.Supersedes, "CAA22933", dt);
			builder.PrepareToEncodeExposed (lineBuf);

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("CAA22933;", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal ("15 May 2012 07:49:22 +0100", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal (0, size);

			dt = new DateTimeOffset (634726649620000000L, TimeSpan.FromHours (-6));
			builder = new ExposedHeaderFieldBuilderTokensAndDate (HeaderFieldName.Supersedes, " CMK-SLNS06.chmk.mechelgroup.ru   CAA22933\t", dt);
			builder.PrepareToEncodeExposed (lineBuf);

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("CMK-SLNS06.chmk.mechelgroup.ru", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("CAA22933;", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal ("15 May 2012 07:49:22 -0600", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal (0, size);

			dt = new DateTimeOffset (634726649620000000L, TimeSpan.FromHours (10));
			builder = new ExposedHeaderFieldBuilderTokensAndDate (HeaderFieldName.Supersedes, "by server10.espc2.mechel.com id CAA22933", dt);
			builder.PrepareToEncodeExposed (lineBuf);

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("by", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("server10.espc2.mechel.com", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("id", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("CAA22933;", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal ("15 May 2012 07:49:22 +1000", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal (0, size);

			dt = new DateTimeOffset (634726649670000000L, TimeSpan.FromHours (0));
			builder = new ExposedHeaderFieldBuilderTokensAndDate (HeaderFieldName.Supersedes, "by CMK-SLNS06.chmk.mechelgroup.ru from server10.espc2.mechel.com ([10.2.21.210])\r\n\twith ESMTP id 2012051507492777-49847", dt);
			builder.PrepareToEncodeExposed (lineBuf);

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("by", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("CMK-SLNS06.chmk.mechelgroup.ru", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("from", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("server10.espc2.mechel.com", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("([10.2.21.210])", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("with", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("ESMTP", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("id", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("2012051507492777-49847;", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal ("15 May 2012 07:49:27 +0000", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal (0, size);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreatePhraseAndId ()
		{
			var buf = new byte[100];
			var lineBuf = new byte[1000];

			var builder = new ExposedHeaderFieldBuilderPhraseAndId (HeaderFieldName.Supersedes, "lenas-jokes.da39efc25c530ad145d41b86f7420c3b.021999.localhost", null);
			builder.PrepareToEncodeExposed (lineBuf);

			var size = builder.GetNextPartExposed (buf, out bool isLast);
			Assert.True (isLast);
			Assert.Equal ("<lenas-jokes.da39efc25c530ad145d41b86f7420c3b.021999.localhost>", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal (0, size);

			builder = new ExposedHeaderFieldBuilderPhraseAndId (HeaderFieldName.Supersedes, "lenas-jokes.da39efc25c530ad145d41b86f7420c3b.021999.localhost", "Lena's Personal <Joke> List");
			builder.PrepareToEncodeExposed (lineBuf);

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("Lena's", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" Personal", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" \"<Joke>\"", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" List", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal ("<lenas-jokes.da39efc25c530ad145d41b86f7420c3b.021999.localhost>", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal (0, size);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreatePhraseList ()
		{
			var buf = new byte[100];
			var lineBuf = new byte[1000];

			var builder = new ExposedHeaderFieldBuilderPhraseList (HeaderFieldName.Supersedes, Array.Empty<string> ());
			builder.PrepareToEncodeExposed (lineBuf);
			var size = builder.GetNextPartExposed (buf, out bool isLast);
			Assert.True (isLast);
			Assert.Equal (0, size);

			builder = new ExposedHeaderFieldBuilderPhraseList (HeaderFieldName.Supersedes, new string[] { "keyword" });
			builder.PrepareToEncodeExposed (lineBuf);

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal ("keyword", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal (0, size);

			builder = new ExposedHeaderFieldBuilderPhraseList (HeaderFieldName.Supersedes, new string[] { "keyword", "KEY WORD", "Richard H. Nixon", "ключслово" });
			builder.PrepareToEncodeExposed (lineBuf);

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("keyword,", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("KEY", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" WORD,", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("Richard", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" \"H.\"", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" Nixon,", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal ("=?utf-8?B?0LrQu9GO0YfRgdC70L7QstC+?=", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal (0, size);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateMailboxList ()
		{
			var buf = new byte[100];
			var lineBuf = new byte[1000];

			var mailboxes = new List<Mailbox> ();
			var builder = new ExposedHeaderFieldBuilderMailboxList (HeaderFieldName.Supersedes, mailboxes);
			builder.PrepareToEncodeExposed (lineBuf);
			var size = builder.GetNextPartExposed (buf, out bool isLast);
			Assert.True (isLast);
			Assert.Equal (0, size);

			mailboxes.Add (new Mailbox ("one@mail.ru", "one man"));
			builder = new ExposedHeaderFieldBuilderMailboxList (HeaderFieldName.Supersedes, mailboxes);
			builder.PrepareToEncodeExposed (lineBuf);

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("one", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" man", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal ("<one@mail.ru>", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal (0, size);

			mailboxes.Add (new Mailbox ("two@gmail.ru", "man 2"));
			mailboxes.Add (new Mailbox ("three@hotmail.com"));
			builder = new ExposedHeaderFieldBuilderMailboxList (HeaderFieldName.Supersedes, mailboxes);
			builder.PrepareToEncodeExposed (lineBuf);

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("one", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" man", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("<one@mail.ru>,", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("man", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal (" 2", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("<two@gmail.ru>,", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal ("<three@hotmail.com>", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal (0, size);

			mailboxes.Clear ();
			mailboxes.Add (new Mailbox ("sp1@mailinator.com", "Адресат Один"));
			mailboxes.Add (new Mailbox ("sp2@mailinator.com", "Адресат Два"));
			mailboxes.Add (new Mailbox ("sp3@mailinator.com", "Адресат Три"));
			builder = new ExposedHeaderFieldBuilderMailboxList (HeaderFieldName.Supersedes, mailboxes);
			builder.PrepareToEncodeExposed (lineBuf);

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("=?utf-8?B?0JDQtNGA0LXRgdCw0YIg0J7QtNC40L0=?=", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("<sp1@mailinator.com>,", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("=?utf-8?B?0JDQtNGA0LXRgdCw0YIg0JTQstCw?=", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("<sp2@mailinator.com>,", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("=?utf-8?B?0JDQtNGA0LXRgdCw0YIg0KLRgNC4?=", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal ("<sp3@mailinator.com>", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal (0, size);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateAngleBracketedList ()
		{
			var buf = new byte[100];
			var lineBuf = new byte[1000];

			var builder = new ExposedHeaderFieldBuilderAngleBracketedList (HeaderFieldName.Supersedes, Array.Empty<string> ());
			builder.PrepareToEncodeExposed (lineBuf);
			var size = builder.GetNextPartExposed (buf, out bool isLast);
			Assert.True (isLast);
			Assert.Equal (0, size);

			builder = new ExposedHeaderFieldBuilderAngleBracketedList (HeaderFieldName.Supersedes, new string[] { "mailto:list@host.com?subject=help" });
			builder.PrepareToEncodeExposed (lineBuf);

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal ("<mailto:list@host.com?subject=help>", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal (0, size);

			var data = new string[]
			{
				"mailto:list@host.com?subject=help",
				"ftp://ftp.host.com/list.txt",
				"magnet:?xt=urn:tree:tiger:Z4URQ35KGEQW3YZZTIM7YXS3OLKLHFJ3M43DPHQ&xl=8539516502&dn=12.mkv",
				"some currently unknown command",
			};
			builder = new ExposedHeaderFieldBuilderAngleBracketedList (HeaderFieldName.Supersedes, data);
			builder.PrepareToEncodeExposed (lineBuf);

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("<mailto:list@host.com?subject=help>,", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("<ftp://ftp.host.com/list.txt>,", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("<magnet:?xt=urn:tree:tiger:Z4URQ35KGEQW3YZZTIM7YXS3OLKLHFJ3M43DPHQ&xl=8539516502&dn=12.mkv>,", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal ("<some currently unknown command>", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal (0, size);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateDispositionNotificationParameterList ()
		{
			var buf = new byte[100];
			var lineBuf = new byte[1000];

			var parameters = new DispositionNotificationParameter[]
			{
				new DispositionNotificationParameter ("signed-receipt", DispositionNotificationParameterImportance.Optional, "pkcs7-signature"),
			};
			var builder = new ExposedHeaderFieldBuilderDispositionNotificationParameterList (HeaderFieldName.Supersedes, parameters);
			builder.PrepareToEncodeExposed (lineBuf);

			var size = builder.GetNextPartExposed (buf, out bool isLast);
			Assert.True (isLast);
			Assert.Equal ("signed-receipt=optional,pkcs7-signature", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal (0, size);

			parameters = new DispositionNotificationParameter[]
			{
				new DispositionNotificationParameter ("signed-receipt", DispositionNotificationParameterImportance.Optional, "pkcs7-signature"),
				new DispositionNotificationParameter ("signed-receipt-micalg", DispositionNotificationParameterImportance.Required, "sha1").AddValue ("md5"),
			};
			builder = new ExposedHeaderFieldBuilderDispositionNotificationParameterList (HeaderFieldName.Supersedes, parameters);
			builder.PrepareToEncodeExposed (lineBuf);

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("signed-receipt=optional,pkcs7-signature;", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal ("signed-receipt-micalg=required,sha1,md5", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal (0, size);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void CreateDisposition ()
		{
			var buf = new byte[100];
			var lineBuf = new byte[1000];

			var builder = new ExposedHeaderFieldBuilderDisposition (HeaderFieldName.Supersedes, "value1", "value2", "value3", Array.Empty<string> ());
			builder.PrepareToEncodeExposed (lineBuf);

			var size = builder.GetNextPartExposed (buf, out bool isLast);
			Assert.False (isLast);
			Assert.Equal ("value1/value2;", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal ("value3", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal (0, size);

			builder = new ExposedHeaderFieldBuilderDisposition (HeaderFieldName.Supersedes, "manual-action", "MDN-sent-manually", "displayed", new string[] { "value1", "value2", "value3" });
			builder.PrepareToEncodeExposed (lineBuf);

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.False (isLast);
			Assert.Equal ("manual-action/MDN-sent-manually;", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal ("displayed/value1,value2,value3", Encoding.ASCII.GetString (buf, 0, size));

			size = builder.GetNextPartExposed (buf, out isLast);
			Assert.True (isLast);
			Assert.Equal (0, size);
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void EncodeToBinaryTransportRepresentation ()
		{
			var buf = new byte[1000];
			var lineBuf = new byte[1000];

			// один параметр
			var builder = new HeaderFieldBuilderExactValue (HeaderFieldName.Supersedes, "short.value");
			builder.AddParameter ("charset", "koi8-r");
			var size = builder.EncodeToBinaryTransportRepresentation (buf, lineBuf);
			Assert.Equal ("Supersedes: short.value; charset=koi8-r\r\n", Encoding.ASCII.GetString (buf, 0, size));

			builder = new HeaderFieldBuilderExactValue (HeaderFieldName.Supersedes, "short.value");
			builder.AddParameter ("charset", "koi8 r");
			size = builder.EncodeToBinaryTransportRepresentation (buf, lineBuf);
			Assert.Equal ("Supersedes: short.value; charset=\"koi8 r\"\r\n", Encoding.ASCII.GetString (buf, 0, size));

			builder = new HeaderFieldBuilderExactValue (HeaderFieldName.Supersedes, "short.value");
			builder.AddParameter ("charset", "функции");
			size = builder.EncodeToBinaryTransportRepresentation (buf, lineBuf);
			Assert.Equal ("Supersedes: short.value;\r\n charset*0*=utf-8''%D1%84%D1%83%D0%BD%D0%BA%D1%86%D0%B8%D0%B8\r\n", Encoding.ASCII.GetString (buf, 0, size));

			// несколько параметров
			builder = new HeaderFieldBuilderExactValue (HeaderFieldName.Supersedes, "short.value");
			builder.AddParameter ("name1", "value1");
			builder.AddParameter ("charset", "koi8-r");
			builder.AddParameter ("name2", "value2");
			size = builder.EncodeToBinaryTransportRepresentation (buf, lineBuf);
			Assert.Equal ("Supersedes: short.value; name1=value1; charset=koi8-r; name2=value2\r\n", Encoding.ASCII.GetString (buf, 0, size));

			// оптимальное кодирования длинного значения параметра
			builder = new HeaderFieldBuilderExactValue (HeaderFieldName.Supersedes, "value");
			builder.AddParameter ("filename", "This document specifies an Internet standards track protocol for the функции and requests discussion and suggestions.txt");
			size = builder.EncodeToBinaryTransportRepresentation (buf, lineBuf);
			Assert.Equal (
				"Supersedes: value;\r\n" +
				" filename*0*=utf-8''This%20document%20specifies%20an%20Internet%20standards;\r\n" +
				" filename*1*=%20track%20protocol%20for%20the%20%D1%84%D1%83%D0%BD%D0%BA%D1%86;\r\n" +
				" filename*2*=%D0%B8%D0%B8%20and%20requests%20discussion%20and%20suggestions.t;\r\n" +
				" filename*3*=xt\r\n",
				Encoding.ASCII.GetString (buf, 0, size));

			builder = new HeaderFieldBuilderExactValue (HeaderFieldName.Supersedes, "value");
			builder.AddParameter ("filename", "ТР-151 от 29.07.2020 О применении утепляющей смеси на основе шамота (ШКВ) взамен люнкерита при разливке по заказам ПАО «Уралкуз» и прокат.pdf");
			size = builder.EncodeToBinaryTransportRepresentation (buf, lineBuf);
			Assert.Equal (
				"Supersedes: value;\r\n" +
				" filename*0*=utf-8''%D0%A2%D0%A0-151%20%D0%BE%D1%82%2029.07.2020%20%D0%9E%20;\r\n" +
				" filename*1*=%D0%BF%D1%80%D0%B8%D0%BC%D0%B5%D0%BD%D0%B5%D0%BD%D0%B8%D0%B8%20;\r\n" +
				" filename*2*=%D1%83%D1%82%D0%B5%D0%BF%D0%BB%D1%8F%D1%8E%D1%89%D0%B5%D0%B9%20;\r\n" +
				" filename*3*=%D1%81%D0%BC%D0%B5%D1%81%D0%B8%20%D0%BD%D0%B0%20%D0%BE%D1%81;\r\n" +
				" filename*4*=%D0%BD%D0%BE%D0%B2%D0%B5%20%D1%88%D0%B0%D0%BC%D0%BE%D1%82%D0%B0;\r\n" +
				" filename*5*=%20%28%D0%A8%D0%9A%D0%92%29%20%D0%B2%D0%B7%D0%B0%D0%BC%D0%B5;\r\n" +
				" filename*6*=%D0%BD%20%D0%BB%D1%8E%D0%BD%D0%BA%D0%B5%D1%80%D0%B8%D1%82%D0%B0;\r\n" +
				" filename*7*=%20%D0%BF%D1%80%D0%B8%20%D1%80%D0%B0%D0%B7%D0%BB%D0%B8%D0%B2;\r\n" +
				" filename*8*=%D0%BA%D0%B5%20%D0%BF%D0%BE%20%D0%B7%D0%B0%D0%BA%D0%B0%D0%B7;\r\n" +
				" filename*9*=%D0%B0%D0%BC%20%D0%9F%D0%90%D0%9E%20%C2%AB%D0%A3%D1%80%D0%B0;\r\n" +
				" filename*10*=%D0%BB%D0%BA%D1%83%D0%B7%C2%BB%20%D0%B8%20%D0%BF%D1%80%D0%BE;\r\n" +
				" filename*11*=%D0%BA%D0%B0%D1%82.pdf\r\n",
				Encoding.ASCII.GetString (buf, 0, size));

			// всё вместе
			builder = new HeaderFieldBuilderExactValue (HeaderFieldName.ContentDisposition, "attachment");
			builder.AddParameter ("filename", "This document specifies an Internet standards track protocol for the функции and requests discussion and suggestions.txt");
			builder.AddParameter ("modification-date", "24 Nov 2011 09:48:27 +0600");
			builder.AddParameter ("creation-date", "10 Jul 2012 10:01:06 +0600");
			builder.AddParameter ("read-date", "11 Jul 2012 10:40:13 +0600");
			builder.AddParameter ("size", "318");
			size = builder.EncodeToBinaryTransportRepresentation (buf, lineBuf);
			Assert.Equal (
				"Content-Disposition: attachment;\r\n" +
				" filename*0*=utf-8''This%20document%20specifies%20an%20Internet%20standards;\r\n" +
				" filename*1*=%20track%20protocol%20for%20the%20%D1%84%D1%83%D0%BD%D0%BA%D1%86;\r\n" +
				" filename*2*=%D0%B8%D0%B8%20and%20requests%20discussion%20and%20suggestions.t;\r\n" +
				" filename*3*=xt; modification-date=\"24 Nov 2011 09:48:27 +0600\";\r\n" +
				" creation-date=\"10 Jul 2012 10:01:06 +0600\";\r\n" +
				" read-date=\"11 Jul 2012 10:40:13 +0600\"; size=318\r\n",
				Encoding.ASCII.GetString (buf, 0, size));
		}

		[Fact]
		[Trait ("Category", "Mime.HeaderEncoder")]
		public void SaveHeader ()
		{
			var src = Array.Empty<HeaderFieldBuilder> ();
			var bytes = new BinaryDestinationMock (8192);
			HeaderFieldBuilder.SaveHeaderAsync (src, bytes).Wait ();
			Assert.Equal (0, bytes.Count);

			src = new HeaderFieldBuilder[]
			{
				new HeaderFieldBuilderExactValue (HeaderFieldName.ContentType, "text/plain"),
				new HeaderFieldBuilderExactValue (HeaderFieldName.ConversionWithLoss, null),
				new HeaderFieldBuilderUnstructuredValue (HeaderFieldName.Received, "by server10.espc2.mechel.com (8.8.8/1.37) id CAA22933; Tue, 15 May 2012 02:49:22 +0100"),
				new HeaderFieldBuilderExactValue (HeaderFieldName.ContentMD5, ":Q2hlY2sgSW50ZWdyaXR5IQ=="),
			};
			src[0].AddParameter ("format", "flowed");
			src[0].AddParameter ("charset", "koi8-r");
			src[0].AddParameter ("reply-type", "original");

			bytes = new BinaryDestinationMock (8192);
			HeaderFieldBuilder.SaveHeaderAsync (src, bytes).Wait ();

			var template = "Content-Type: text/plain; format=flowed; charset=koi8-r; reply-type=original\r\n" +
				"Conversion-With-Loss:\r\n" +
				"Received: by server10.espc2.mechel.com (8.8.8/1.37) id CAA22933; Tue, 15 May\r\n 2012 02:49:22 +0100\r\n" +
				"Content-MD5: :Q2hlY2sgSW50ZWdyaXR5IQ==\r\n";
			Assert.Equal (template, Encoding.ASCII.GetString (bytes.Buffer.Slice (0, bytes.Count)));
		}
	}
}
