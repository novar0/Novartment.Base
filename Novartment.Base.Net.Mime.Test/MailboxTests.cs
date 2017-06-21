using System;
using Xunit;

namespace Novartment.Base.Net.Mime.Test
{
	public class MailboxTests
	{
		[Fact]
		[Trait ("Category", "Mime")]
		public void Parse ()
		{
			var mailbox = Mailbox.Parse ("postmaster@server.com");
			Assert.Null (mailbox.Name);
			Assert.Equal ("postmaster", mailbox.Address.LocalPart);
			Assert.Equal ("server.com", mailbox.Address.Domain);

			mailbox = Mailbox.Parse ("<postmaster@server.com>");
			Assert.Null (mailbox.Name);
			Assert.Equal ("postmaster", mailbox.Address.LocalPart);
			Assert.Equal ("server.com", mailbox.Address.Domain);

			mailbox = Mailbox.Parse (" (some comments)  Bill Clinton   <\"real(addr)\"@[some literal domain]> (yet another comment)");
			Assert.Equal ("Bill Clinton", mailbox.Name);
			Assert.Equal ("real(addr)", mailbox.Address.LocalPart);
			Assert.Equal ("some literal domain", mailbox.Address.Domain);
		}

		[Fact]
		[Trait ("Category", "Mime")]
		public void Parse_Exception ()
		{
			Assert.Throws<FormatException> (() => Mailbox.Parse ("@"));
			Assert.Throws<FormatException> (() => Mailbox.Parse ("postmaster"));
			Assert.Throws<FormatException> (() => Mailbox.Parse ("postmaster@"));
			Assert.Throws<FormatException> (() => Mailbox.Parse ("@server.com"));
			Assert.Throws<FormatException> (() => Mailbox.Parse ("(postmaster@server.com)"));
			Assert.Throws<FormatException> (() => Mailbox.Parse ("<postmaster@server.com> Me"));
			Assert.Throws<FormatException> (() => Mailbox.Parse ("postmaster server.com"));
			Assert.Throws<FormatException> (() => Mailbox.Parse ("Bill Clinton postmaster@server.com"));
		}

		[Fact]
		[Trait ("Category", "Mime")]
		public void ToString_ ()
		{
			var mailbox = new Mailbox ("some-one@server.com");
			Assert.Equal ("<some-one@server.com>", mailbox.ToString ());

			var addr = new AddrSpec ("real(addr)", "some literal domain");
			mailbox = new Mailbox (addr, "Bill Clinton");
			Assert.Equal ("Bill Clinton <\"real(addr)\"@[some literal domain]>", mailbox.ToString ());
		}

		[Fact]
		[Trait ("Category", "Mime")]
		public void Equals_ ()
		{
			Assert.Equal (
				new Mailbox (new AddrSpec ("someone", "someserver.ru"), "Bill Clinton"),
				new Mailbox (new AddrSpec ("someone", "someserver.ru"), "Bill Clinton"));
			Assert.Equal (
				new Mailbox (new AddrSpec ("someone", "someserver.ru"), "Bill Clinton"),
				new Mailbox (new AddrSpec ("someone", "someSERVER.ru"), "Bill Clinton"));

			Assert.NotEqual (
				new Mailbox (new AddrSpec ("someone", "someserver.ru"), "Bill Clinton"),
				new Mailbox (new AddrSpec ("someone", "someserver.ru"), "William Clinton"));
			Assert.NotEqual (
				new Mailbox (new AddrSpec ("someone", "someserver.ru"), "Bill Clinton"),
				new Mailbox (new AddrSpec ("someONE", "someSERVER.ru"), "Bill Clinton"));
			Assert.NotEqual (
				new Mailbox (new AddrSpec ("someone", "someserver.ru"), "Bill Clinton"),
				new Mailbox (new AddrSpec ("someone2", "someserver.ru"), "Bill Clinton"));
			Assert.NotEqual (
				new Mailbox (new AddrSpec ("someone", "someserver.ru"), "Bill Clinton"),
				new Mailbox (new AddrSpec ("someone", "someserver2.ru"), "Bill Clinton"));
		}
	}
}
