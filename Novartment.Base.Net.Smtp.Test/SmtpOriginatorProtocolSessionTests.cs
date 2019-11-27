using System;
using Novartment.Base.Net.Smtp;
using Xunit;

namespace Novartment.Base.Smtp.Test
{
	public class SmtpOriginatorProtocolSessionTests
	{
		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void Workflow ()
		{
			var sender = new SmtpCommandReplyConnectionSenderReceiverMock ();
			var session = new SmtpOriginatorProtocolSession (sender, "test.localhost");
			sender.ReceivedReplies.Enqueue (SmtpReply.CreateServiceReady ("test", new Version (0, 0)));
			sender.ReceivedReplies.Enqueue (SmtpReply.CreateHelloResponse ("test.localhost", new string[] { "PIPELINING", "STARTTLS", "AUTH PLAIN", "8BITMIME" }));
			session.ReceiveGreetingAndStartAsync ().Wait ();
			Assert.Equal (4, session.ServerSupportedExtensions.Count);
			Assert.True (session.ServerSupportedExtensions.Contains ("PIPELINING"));
			Assert.True (session.ServerSupportedExtensions.Contains ("STARTTLS"));
			Assert.True (session.ServerSupportedExtensions.Contains ("AUTH PLAIN"));
			Assert.True (session.ServerSupportedExtensions.Contains ("8BITMIME"));
			Assert.True (session.IsServerSupportsAuthPlain ());
			//session.AuthenticateAsync (new NetworkCredential ("user", "password"), default (CancellationToken)).Wait ();
			//session.FinishAsync ();
			//session.IsServerSupportsAuthPlain ();
			//session.ProcessCommandAsync ();
			//session.RestartWithTlsAsync ();

			session.Dispose ();
		}
	}
}
