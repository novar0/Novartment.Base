using System;
using System.Text;
using Xunit;

namespace Novartment.Base.Net.Mime.Test
{
	public class MailboxTests
	{
		public MailboxTests ()
		{
			Encoding.RegisterProvider (CodePagesEncodingProvider.Instance);
		}

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

			mailbox = Mailbox.Parse ("no.name@mailinator.com");
			Assert.Null (mailbox.Name);
			Assert.Equal ("no.name", mailbox.Address.LocalPart);
			Assert.Equal ("mailinator.com", mailbox.Address.Domain);

			mailbox = Mailbox.Parse ("\"Recipient A.B. \\\"First\\\"\" <sp1@[some strange domain]>");
			Assert.Equal ("Recipient A.B. \"First\"", mailbox.Name);
			Assert.Equal ("sp1", mailbox.Address.LocalPart);
			Assert.Equal ("some strange domain", mailbox.Address.Domain);

			mailbox = Mailbox.Parse ("=?windows-1251?Q?new_=F1=EE=E2=F1=E5=EC_one_222?= <\"namewith,comma\"@mailinator.com>");
			Assert.Equal ("new совсем one 222", mailbox.Name);
			Assert.Equal ("namewith,comma", mailbox.Address.LocalPart);
			Assert.Equal ("mailinator.com", mailbox.Address.Domain);

			mailbox = Mailbox.Parse ("=?windows-1251?Q?=C8=E4=E5=FF_=F1=EE=F1=F2=EE=E8=F2_=E2_=F2=EE=EC=2C_=F7?=\t=?windows-1251?Q?=F2=EE=E1=FB_=EF=E8=F1=E0=F2=FC_=F2=E5=F1=F2=FB_=E4=EB=FF_?=\t=?windows-1251?Q?=EA=E0=E6=E4=EE=E9_=ED=E5=F2=F0=E8=E2=E8=E0=EB=FC=ED=EE=E9?=\t=?windows-1251?Q?_=F4=F3=ED=EA=F6=E8=E8_=E8=EB=E8_=EC=E5=F2=EE=E4=E0?= <sp3@mailinator.com>");
			Assert.Equal ("Идея состоит в том, чтобы писать тесты для каждой нетривиальной функции или метода", mailbox.Name);
			Assert.Equal ("sp3", mailbox.Address.LocalPart);
			Assert.Equal ("mailinator.com", mailbox.Address.Domain);
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
