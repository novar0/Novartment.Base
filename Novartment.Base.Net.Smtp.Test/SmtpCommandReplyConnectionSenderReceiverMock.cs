using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Net.Smtp;

namespace Novartment.Base.Smtp.Test
{
	internal class SmtpCommandReplyConnectionSenderReceiverMock :
		ISmtpCommandTransport
	{
		public bool TlsEstablished => false;

		public X509Certificate RemoteCertificate => null;

		internal Queue<SmtpCommand> ReceivedCommands { get; } = new Queue<SmtpCommand> ();

		internal Queue<SmtpCommand> SendedCommands { get; } = new Queue<SmtpCommand> ();

		internal Queue<string> SendedDataBlocks { get; } = new Queue<string> ();

		internal Queue<SmtpReply> ReceivedReplies { get; } = new Queue<SmtpReply> ();

		internal Queue<SmtpReply> SendedReplies { get; } = new Queue<SmtpReply> ();

		public Task SendCommandAsync (SmtpCommand command, CancellationToken cancellationToken = default)
		{
			this.SendedCommands.Enqueue (command);
			return Task.CompletedTask;
		}

		public Task SendReplyAsync (SmtpReply reply, bool canBeGrouped, CancellationToken cancellationToken = default)
		{
			// тут вставлять паузу для тестирования экстренной остановки сервера
			// await Task.Delay (3000, cancellationToken).ConfigureAwait (false);
			this.SendedReplies.Enqueue (reply);
			return Task.CompletedTask;
		}

		public async Task SendBinaryAsync (IBufferedSource source, CancellationToken cancellationToken = default)
		{
			using (var strm = new MemoryStream ())
			{
				try
				{
					while (true)
					{
						await source.LoadAsync ().ConfigureAwait (false);
						if (source.Count <= 0)
						{
							break;
						}

						strm.Write (source.BufferMemory.Span.Slice (source.Offset, source.Count));
						source.Skip (source.Count);
					}
				}
				finally
				{
					strm.TryGetBuffer (out ArraySegment<byte> buf);
					var str = Encoding.ASCII.GetString (buf.Array, buf.Offset, (int)strm.Length);
					this.SendedDataBlocks.Enqueue (str);
				}
			}
		}

		public Task<SmtpReply> ReceiveReplyAsync (CancellationToken cancellationToken = default)
		{
			return Task.FromResult (this.ReceivedReplies.Dequeue ());
		}

		public Task<SmtpCommand> ReceiveCommandAsync (SmtpCommand.ExpectedInputType expectedInputType, CancellationToken cancellationToken = default)
		{
			return Task.FromResult (this.ReceivedCommands.Dequeue ());
		}

		public Task StartTlsClientAsync (X509CertificateCollection clientCertificates)
		{
			throw new NotImplementedException ();
		}

		public Task StartTlsServerAsync (X509Certificate serverCertificate, bool clientCertificateRequired)
		{
			throw new NotImplementedException ();
		}
	}
}
