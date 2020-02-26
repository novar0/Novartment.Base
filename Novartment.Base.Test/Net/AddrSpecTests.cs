using System;
using Xunit;
using Novartment.Base.Net;

namespace Novartment.Base.Test
{
	public class AddrSpecTests
	{
		[Fact]
		[Trait ("Category", "Net")]
		public void Creation_Exception ()
		{
			Assert.Throws<ArgumentOutOfRangeException> (() => new AddrSpec ("юж", "someserver.ru"));
			Assert.Throws<ArgumentOutOfRangeException> (() => new AddrSpec ("postmaster", "жжж.ru"));
			Assert.Throws<ArgumentOutOfRangeException> (() => new AddrSpec ("someone", "[someserver].ru"));
		}

		[Fact]
		[Trait ("Category", "Net")]
		public void Parse ()
		{
			var addr = AddrSpec.Parse ("someone@someserver.ru");
			Assert.Equal ("someone", addr.LocalPart);
			Assert.Equal ("someserver.ru", addr.Domain);

			addr = AddrSpec.Parse ("\"real(addr)\"@\t(comment here)\t[ some literal domain  ]");
			Assert.Equal ("real(addr)", addr.LocalPart);
			Assert.Equal (" some literal domain  ", addr.Domain);

			addr = AddrSpec.Parse ("<some>");
			Assert.Equal ("some", addr.LocalPart);
			Assert.Equal ("localhost", addr.Domain);

			addr = AddrSpec.Parse ("\"sp1 <d>\"@[some...strange domain]");
			Assert.Equal ("sp1 <d>", addr.LocalPart);
			Assert.Equal ("some...strange domain", addr.Domain);
		}

		[Fact]
		[Trait ("Category", "Net")]
		public void ParseException ()
		{
			Assert.Throws<FormatException> (() => AddrSpec.Parse ("someone@someserver.ru a"));
			Assert.Throws<FormatException> (() => AddrSpec.Parse ("(someone)@someserver.ru"));
			Assert.Throws<FormatException> (() => AddrSpec.Parse ("someone:someserver.ru"));
			Assert.Throws<FormatException> (() => AddrSpec.Parse ("someone@(someserver.ru)"));
		}

		[Fact]
		[Trait ("Category", "Net")]
		public void ToString_ ()
		{
			var values = new AddrSpec ("someone", "someserver.ru").ToString ();
			Assert.Equal ("someone@someserver.ru", values);

			values = new AddrSpec ("real(addr)", "someserver.ru").ToString ();
			Assert.Equal ("\"real(addr)\"@someserver.ru", values);

			values = new AddrSpec ("someone", "some literal domain").ToString ();
			Assert.Equal ("someone@[some literal domain]", values);

			values = new AddrSpec ("real(addr)", "some literal domain").ToString ();
			Assert.Equal ("\"real(addr)\"@[some literal domain]", values);
		}

		[Fact]
		[Trait ("Category", "Net")]
		public void ToStringSpan ()
		{
			var buf = new char[100];
			var size = new AddrSpec ("someone", "someserver.ru").ToString (buf);
			Assert.Equal ("someone@someserver.ru", new string (buf.AsSpan (0, size)));

			size = new AddrSpec ("real(addr)", "someserver.ru").ToString (buf);
			Assert.Equal ("\"real(addr)\"@someserver.ru", new string (buf.AsSpan (0, size)));

			size = new AddrSpec ("someone", "some literal domain").ToString (buf);
			Assert.Equal ("someone@[some literal domain]", new string (buf.AsSpan (0, size)));

			size = new AddrSpec ("real(addr)", "some literal domain").ToString (buf);
			Assert.Equal ("\"real(addr)\"@[some literal domain]", new string (buf.AsSpan (0, size)));
		}

		[Fact]
		[Trait ("Category", "Net")]
		public void Equals_ ()
		{
			Assert.True (new AddrSpec ("someone", "someserver.ru").Equals (new AddrSpec ("someone", "someserver.ru")));
			Assert.True (new AddrSpec ("someone", "someserver.ru").Equals (new AddrSpec ("someone", "someSERVER.ru")));

			Assert.False (new AddrSpec ("someone", "someserver.ru").Equals (new AddrSpec ("someONE", "someSERVER.ru")));
			Assert.False (new AddrSpec ("someone", "someserver.ru").Equals (new AddrSpec ("someone2", "someserver.ru")));
			Assert.False (new AddrSpec ("someone", "someserver.ru").Equals (new AddrSpec ("someone", "someserver2.ru")));
		}
	}
}
