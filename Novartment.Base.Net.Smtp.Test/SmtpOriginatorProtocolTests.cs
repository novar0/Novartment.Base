using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Net;
using Novartment.Base.Net.Smtp;
using Xunit;

namespace Novartment.Base.Smtp.Test
{
	public class SmtpOriginatorProtocolTests
	{
		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void Workflow ()
		{
			var received =
				"220 smtp35.i.mail.ru ESMTP ready\r\n" +
				"250 OK\r\n" +
				"221 Service closing transmission channel\r\n";
			var src = new ArrayBufferedSource (Encoding.ASCII.GetBytes (received));
			var connection = new TcpConnectionMock (
				new IPEndPoint (IPAddress.Loopback, 2555),
				new IPEndPoint (IPAddress.Loopback, 25),
				src);
			var originator = new SmtpTransactionOriginatorMock ();
			var protocol = new SmtpOriginatorProtocol (originator.OriginateTransactionsAsync, SmtpClientSecurityParameters.AllowNoSecurity, null);
			Assert.Equal (0, originator.CallCount);
			protocol.StartAsync (connection, CancellationToken.None).Wait ();
			var sended = connection.OutData.Queue.ToArray ();
			Assert.Equal (0, src.Count);
			Assert.Equal (2, sended.Length);
			Assert.StartsWith ("EHLO ", sended[0], StringComparison.OrdinalIgnoreCase);
			Assert.StartsWith ("QUIT", sended[1], StringComparison.OrdinalIgnoreCase);

			received =
				"220 smtp35.i.mail.ru ESMTP ready\r\n" +
				"500 Syntax error, command unrecognized\r\n" +
				"250 OK\r\n" +
				"221 Service closing transmission channel\r\n";
			src = new ArrayBufferedSource (Encoding.ASCII.GetBytes (received));
			connection = new TcpConnectionMock (
				new IPEndPoint (IPAddress.Loopback, 2555),
				new IPEndPoint (IPAddress.Loopback, 25),
				src);
			protocol.StartAsync (connection, CancellationToken.None).Wait ();
			sended = connection.OutData.Queue.ToArray ();
			Assert.Equal (0, src.Count);
			Assert.Equal (3, sended.Length);
			Assert.StartsWith ("EHLO ", sended[0], StringComparison.OrdinalIgnoreCase);
			Assert.StartsWith ("HELO ", sended[1], StringComparison.OrdinalIgnoreCase);
			Assert.StartsWith ("QUIT", sended[2], StringComparison.OrdinalIgnoreCase);
		}

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void UnexpectedStop ()
		{
			// сервер сразу отвечает что недоступен
			var received = "421 mail.ru Service not available, closing transmission channel\r\n";
			var src = new ArrayBufferedSource (Encoding.ASCII.GetBytes (received));
			var connection = new TcpConnectionMock (
				new IPEndPoint (IPAddress.Loopback, 2555),
				new IPEndPoint (IPAddress.Loopback, 25),
				src);
			var originator = new SmtpTransactionOriginatorMock ();
			var protocol = new SmtpOriginatorProtocol (originator.OriginateTransactionsAsync, SmtpClientSecurityParameters.AllowNoSecurity, null);
			Assert.ThrowsAsync<InvalidOperationException> (() => protocol.StartAsync (connection, CancellationToken.None));
			Assert.Equal (0, src.Count);

			// сервер приветствует, но на EHLO отвечает что недоступен
			received =
				"220 smtp35.i.mail.ru ESMTP ready\r\n" +
				"421 mail.ru Service not available, closing transmission channel\r\n";
			src = new ArrayBufferedSource (Encoding.ASCII.GetBytes (received));
			connection = new TcpConnectionMock (
				new IPEndPoint (IPAddress.Loopback, 2555),
				new IPEndPoint (IPAddress.Loopback, 25),
				src);
			Assert.ThrowsAsync<InvalidOperationException> (() => protocol.StartAsync (connection, CancellationToken.None));
			Assert.Equal (0, src.Count);
			var sended = connection.OutData.Queue.ToArray ();
			Assert.Single (sended);
			Assert.StartsWith ("EHLO ", sended[0], StringComparison.OrdinalIgnoreCase);

			// сервер приветствует и обрывает соединение
			received =
				"220 smtp35.i.mail.ru ESMTP ready\r\n";
			src = new ArrayBufferedSource (Encoding.ASCII.GetBytes (received));
			connection = new TcpConnectionMock (
				new IPEndPoint (IPAddress.Loopback, 2555),
				new IPEndPoint (IPAddress.Loopback, 25),
				src);
			Assert.ThrowsAsync<InvalidOperationException> (() => protocol.StartAsync (connection, CancellationToken.None));
			Assert.Equal (0, src.Count);
			sended = connection.OutData.Queue.ToArray ();
			Assert.Equal (2, sended.Length);
			Assert.StartsWith ("EHLO ", sended[0], StringComparison.OrdinalIgnoreCase);
			Assert.StartsWith ("QUIT", sended[1], StringComparison.OrdinalIgnoreCase);
		}

		internal class SmtpTransactionOriginatorMock
		{
			internal int CallCount { get; private set; } = 0;

#pragma warning disable CA1801 // Review unused parameters
			public Task OriginateTransactionsAsync (TransactionHandlerFactory transactionFactory, CancellationToken cancellationToken)
#pragma warning restore CA1801 // Review unused parameters
			{
				if (transactionFactory == null)
				{
					throw new ArgumentNullException (nameof (transactionFactory));
				}

				this.CallCount++;
				return Task.CompletedTask;
			}
		}
	}
}
