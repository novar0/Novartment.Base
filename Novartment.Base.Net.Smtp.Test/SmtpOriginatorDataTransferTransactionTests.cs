using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Xunit;
using Novartment.Base.Net;
using Novartment.Base.Net.Smtp;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Smtp.Test
{

	public class SmtpOriginatorDataTransferTransactionTests
	{
		private readonly static AddrSpec mailbox = new AddrSpec ("someone", "server.org");
		private readonly static AddrSpec mailbox1 = new AddrSpec ("postmaster", "github.org");
		private readonly static AddrSpec mailbox2 = new AddrSpec ("admin", "sender.org");
		private readonly static string mailBody = "Hello!!\r\n\r\nPlese reply me.\r\n";
		private readonly static string invalidMailBodyPart1 = "Hello!!";
		private readonly static string invalidMailBodyPart2 = "\r\n.\r\nPlese reply me.\r\n";

		[Fact, Trait ("Category", "Net.Smtp")]
		public void StartAsync ()
		{
			var sender = new SmtpCommandReplyConnectionSenderReceiverMock ();
			var transaction = new SmtpOriginatorDataTransferTransaction (
				new SmtpOriginatorProtocolSession (sender, "test.localhost"),
				ContentTransferEncoding.SevenBit,
				null);

			// стартуем с неподходящим серверу return-path
			sender.ReceivedReplies.Enqueue (SmtpReply.MailboxNotAllowed);
			Assert.ThrowsAsync<UnacceptableSmtpMailboxException> (async () => await transaction.StartAsync (mailbox, CancellationToken.None));
			Assert.Equal (0, sender.ReceivedReplies.Count);
			var cmd = sender.SendedCommands.Dequeue ();
			Assert.Equal (0, sender.SendedCommands.Count);
			Assert.Equal (SmtpCommandType.MailFrom, cmd.CommandType);
			var mailFromCmd = (SmtpMailFromCommand)cmd;
			Assert.Equal (mailbox, mailFromCmd.ReturnPath);

			// стартуем успешно
			sender.ReceivedReplies.Enqueue (SmtpReply.OK);
			transaction.StartAsync (mailbox, CancellationToken.None).Wait ();
			Assert.Equal (0, sender.ReceivedReplies.Count);
			cmd = sender.SendedCommands.Dequeue ();
			Assert.Equal (0, sender.SendedCommands.Count);
			Assert.Equal (SmtpCommandType.MailFrom, cmd.CommandType);
			mailFromCmd = (SmtpMailFromCommand)cmd;
			Assert.Equal (mailbox, mailFromCmd.ReturnPath);

			// стартуем уже законченную
			transaction.Dispose ();
			Assert.ThrowsAsync<InvalidOperationException> (async () => await transaction.StartAsync (mailbox, CancellationToken.None));
			Assert.Equal (0, sender.SendedCommands.Count);
		}

		[Fact, Trait ("Category", "Net.Smtp")]
		public void TryAddRecipientAsync ()
		{
			var sender = new SmtpCommandReplyConnectionSenderReceiverMock ();
			var transaction = new SmtpOriginatorDataTransferTransaction (
				new SmtpOriginatorProtocolSession (sender, "test.localhost"),
				ContentTransferEncoding.SevenBit,
				null);

			// добавляем получаетелй в не начатую
			Assert.ThrowsAsync<InvalidOperationException> (async () => await transaction.TryAddRecipientAsync (mailbox1, CancellationToken.None));
			Assert.Equal (0, sender.SendedCommands.Count);

			// начинаем для дальнейших тестов
			sender.ReceivedReplies.Enqueue (SmtpReply.OK);
			transaction.StartAsync (mailbox, CancellationToken.None).Wait ();
			sender.SendedCommands.Clear ();

			// добавляем получателя успешно
			sender.ReceivedReplies.Enqueue (SmtpReply.OK);
			Assert.Equal (RecipientAcceptanceState.Success, transaction.TryAddRecipientAsync (mailbox1, CancellationToken.None).Result);
			Assert.Equal (0, sender.ReceivedReplies.Count);
			var cmd = sender.SendedCommands.Dequeue ();
			Assert.Equal (0, sender.SendedCommands.Count);
			Assert.Equal (SmtpCommandType.RcptTo, cmd.CommandType);
			var rcptToCmd = (SmtpRcptToCommand)cmd;
			Assert.Equal (mailbox1, rcptToCmd.Recipient);

			// добавляем получателя безуспешно
			sender.ReceivedReplies.Enqueue (SmtpReply.MailboxUnavailable);
			Assert.Equal (RecipientAcceptanceState.FailureMailboxUnavailable, transaction.TryAddRecipientAsync (mailbox2, CancellationToken.None).Result);
			Assert.Equal (0, sender.ReceivedReplies.Count);
			cmd = sender.SendedCommands.Dequeue ();
			Assert.Equal (0, sender.SendedCommands.Count);
			Assert.Equal (SmtpCommandType.RcptTo, cmd.CommandType);
			rcptToCmd = (SmtpRcptToCommand)cmd;
			Assert.Equal (mailbox2, rcptToCmd.Recipient);
		}

		private SmtpOriginatorDataTransferTransaction PrepareSessionForDataTransfer (
			SmtpCommandReplyConnectionSenderReceiverMock sender,
			HashSet<string> serverSupportedExtensions)
		{
			var session = new SmtpOriginatorProtocolSession (sender, "test.localhost");
			sender.ReceivedReplies.Enqueue (SmtpReply.CreateServiceReady ("test", new Version (0,0)));
			sender.ReceivedReplies.Enqueue (SmtpReply.CreateHelloResponse ("test.localhost", serverSupportedExtensions));
			session.ReceiveGreetingAndStartAsync (CancellationToken.None).Wait ();
			// создаём начатую для дальнейших тестов
			var transaction = new SmtpOriginatorDataTransferTransaction (
				session,
				ContentTransferEncoding.SevenBit,
				null);
			sender.ReceivedReplies.Enqueue (SmtpReply.OK);
			transaction.StartAsync (mailbox, CancellationToken.None).Wait ();
			sender.ReceivedReplies.Enqueue (SmtpReply.OK);
			transaction.TryAddRecipientAsync (mailbox1, CancellationToken.None).Wait ();
			sender.SendedCommands.Clear ();
			return transaction;
		}

		[Fact, Trait ("Category", "Net.Smtp")]
		public void TransferDataAndFinishAsync ()
		{
			var extensionsSupported = new HashSet<string> ();

			var sender = new SmtpCommandReplyConnectionSenderReceiverMock ();
			var transaction = new SmtpOriginatorDataTransferTransaction (
				new SmtpOriginatorProtocolSession (sender, "test.localhost"),
				ContentTransferEncoding.SevenBit,
				null);

			// передаём данные в не начатую
			var src = new ArrayBufferedSource (Encoding.ASCII.GetBytes (mailBody));
			Assert.ThrowsAsync<InvalidOperationException> (async () => await transaction.TransferDataAndFinishAsync (src, -1, CancellationToken.None));
			Assert.Equal (0, sender.SendedCommands.Count);

			// передаём данные не указав получателей
			transaction = PrepareSessionForDataTransfer (sender, extensionsSupported);
			transaction = new SmtpOriginatorDataTransferTransaction (
				new SmtpOriginatorProtocolSession (sender, "test.localhost"),
				ContentTransferEncoding.SevenBit,
				null);
			src = new ArrayBufferedSource (Encoding.ASCII.GetBytes (mailBody));
			Assert.ThrowsAsync<InvalidOperationException> (async () => await transaction.TransferDataAndFinishAsync (src, -1, CancellationToken.None));
			Assert.Equal (0, sender.SendedCommands.Count);

			// передаем данные без поддержки CHUNKING
			transaction = PrepareSessionForDataTransfer (sender, extensionsSupported);
			sender.ReceivedReplies.Enqueue (SmtpReply.DataStart);
			sender.ReceivedReplies.Enqueue (SmtpReply.OK);
			src = new ArrayBufferedSource (Encoding.ASCII.GetBytes (mailBody));
			Assert.Equal (0, sender.SendedDataBlocks.Count);
			transaction.TransferDataAndFinishAsync (src, -1, CancellationToken.None).Wait ();
			Assert.Equal (0, sender.ReceivedReplies.Count);
			var cmd = sender.SendedCommands.Dequeue ();
			Assert.Equal (0, sender.SendedCommands.Count);
			Assert.Equal (SmtpCommandType.Data, cmd.CommandType);
			var block1 = sender.SendedDataBlocks.Dequeue ();
			var block2 = sender.SendedDataBlocks.Dequeue ();
			Assert.Equal (0, sender.SendedDataBlocks.Count);
			Assert.Equal (mailBody, block1);
			Assert.Equal ("\r\n.\r\n", block2);

			// передаем данные без поддержки CHUNKING, недопустимый маркер конца внутри данных, должно вызвать UnrecoverableProtocolException
			transaction = PrepareSessionForDataTransfer (sender, extensionsSupported);
			sender.ReceivedReplies.Enqueue (SmtpReply.DataStart);
			src = new ArrayBufferedSource (Encoding.ASCII.GetBytes (invalidMailBodyPart1 + invalidMailBodyPart2));
			Assert.ThrowsAsync<UnrecoverableProtocolException> (async () => await transaction.TransferDataAndFinishAsync (src, -1, CancellationToken.None));
			Assert.Equal (0, sender.ReceivedReplies.Count);
			cmd = sender.SendedCommands.Dequeue ();
			Assert.Equal (0, sender.SendedCommands.Count);
			Assert.Equal (SmtpCommandType.Data, cmd.CommandType);
			block1 = sender.SendedDataBlocks.Dequeue ();
			Assert.Equal (0, sender.SendedDataBlocks.Count);
			Assert.Equal (invalidMailBodyPart1, block1);

			// передаем данные с поддержкой CHUNKING
			extensionsSupported.Add ("CHUNKING");
			transaction = PrepareSessionForDataTransfer (sender, extensionsSupported);
			sender.ReceivedReplies.Enqueue (SmtpReply.OK);
			src = new ArrayBufferedSource (Encoding.ASCII.GetBytes (mailBody));
			transaction.TransferDataAndFinishAsync (src, mailBody.Length, CancellationToken.None).Wait ();
			Assert.Equal (0, sender.ReceivedReplies.Count);
			cmd = sender.SendedCommands.Dequeue ();
			Assert.Equal (0, sender.SendedCommands.Count);
			Assert.Equal (SmtpCommandType.Bdat, cmd.CommandType);
			block1 = sender.SendedDataBlocks.Dequeue ();
			Assert.Equal (0, sender.SendedDataBlocks.Count);
			Assert.Equal (mailBody, block1);

			// передаем данные с поддержкой CHUNKING источник предоставляет данных меньше чем указано, должно вызвать UnrecoverableProtocolException
			transaction = PrepareSessionForDataTransfer (sender, extensionsSupported);
			src = new ArrayBufferedSource (Encoding.ASCII.GetBytes (mailBody));
			Assert.ThrowsAsync<UnrecoverableProtocolException> (async () => await transaction.TransferDataAndFinishAsync (src, mailBody.Length + 5, CancellationToken.None));
			cmd = sender.SendedCommands.Dequeue ();
			Assert.Equal (0, sender.SendedCommands.Count);
			Assert.Equal (SmtpCommandType.Bdat, cmd.CommandType);
			block1 = sender.SendedDataBlocks.Dequeue ();
			Assert.Equal (0, sender.SendedDataBlocks.Count);
			Assert.Equal (mailBody, block1);
		}
	}
}
