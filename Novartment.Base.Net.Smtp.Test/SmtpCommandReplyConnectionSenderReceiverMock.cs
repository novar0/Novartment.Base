using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using Novartment.Base.Net.Smtp;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Smtp.Test
{
	internal class SmtpCommandReplyConnectionSenderReceiverMock : ISmtpCommandTransport
	{
		internal Queue<SmtpCommand> ReceivedCommands = new Queue<SmtpCommand> ();
		internal Queue<SmtpCommand> SendedCommands = new Queue<SmtpCommand> ();
		internal Queue<string> SendedDataBlocks = new Queue<string> ();
		internal Queue<SmtpReply> ReceivedReplies = new Queue<SmtpReply> ();
		internal Queue<SmtpReply> SendedReplies = new Queue<SmtpReply> ();

		public bool TlsEstablished => false;

		public X509Certificate RemoteCertificate => null;

		public Task SendCommandAsync (SmtpCommand command, CancellationToken cancellationToken)
		{
			this.SendedCommands.Enqueue (command);
			return Task.CompletedTask;
		}

		public Task SendReplyAsync (SmtpReply reply, bool canBeGrouped, CancellationToken cancellationToken)
		{
			this.SendedReplies.Enqueue (reply);
			return Task.CompletedTask;
		}

		public async Task SendBinaryAsync (IBufferedSource source, CancellationToken cancellationToken)
		{
			MemoryStream strm;
			using (strm = new MemoryStream ())
			{
				try
				{
					while (true)
					{
						await source.FillBufferAsync (CancellationToken.None).ConfigureAwait (false);
						if (source.Count <= 0)
						{
							break;
						}
						strm.Write (source.Buffer, source.Offset, source.Count);
						source.SkipBuffer (source.Count);
					}
				}
				finally
				{
					ArraySegment<byte> buf;
					strm.TryGetBuffer (out buf);
					var str = Encoding.ASCII.GetString (buf.Array, buf.Offset, (int)strm.Length);
					this.SendedDataBlocks.Enqueue (str);
				}
			}
		}

		public Task<SmtpReply> ReceiveReplyAsync (CancellationToken cancellationToken)
		{
			return Task.FromResult (this.ReceivedReplies.Dequeue ());
		}

		public Task<SmtpCommand> ReceiveCommandAsync (SmtpCommand.ExpectedInputType expectedInputType, CancellationToken cancellationToken)
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
