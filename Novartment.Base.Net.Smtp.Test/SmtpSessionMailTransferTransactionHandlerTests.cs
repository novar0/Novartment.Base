using System;
using System.Collections.Generic;
using System.Text;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Net;
using Novartment.Base.Net.Smtp;
using Xunit;

namespace Novartment.Base.Smtp.Test
{
	public class SmtpSessionMailTransferTransactionHandlerTests
	{
		private static readonly AddrSpec Mailbox0 = new AddrSpec ("someone", "server.org");
		private static readonly AddrSpec Mailbox1 = new AddrSpec ("postmaster", "github.org");
		private static readonly AddrSpec Mailbox2 = new AddrSpec ("admin", "sender.org");
		private static readonly string MailBody = "Hello!!\r\n\r\nPlese reply me.\r\n";
		private static readonly string InvalidMailBodyPart1 = "Hello!!";
		private static readonly string InvalidMailBodyPart2 = "\r\n.\r\nPlese reply me.\r\n";

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void StartAsync ()
		{
			var sender = new SmtpCommandReplyConnectionSenderReceiverMock ();
			var transaction = new SmtpSessionMailTransferTransactionHandler (
				new SmtpOriginatorProtocolSession (sender, "test.localhost"),
				ContentTransferEncoding.SevenBit,
				null);

			// стартуем с неподходящим серверу return-path
			sender.ReceivedReplies.Enqueue (SmtpReply.MailboxNotAllowed);
			Assert.ThrowsAsync<UnacceptableSmtpMailboxException> (() => transaction.StartAsync (Mailbox0));
			Assert.Empty (sender.ReceivedReplies);
			var cmd = sender.SendedCommands.Dequeue ();
			Assert.Empty (sender.SendedCommands);
			Assert.Equal (SmtpCommandType.MailFrom, cmd.CommandType);
			var mailFromCmd = (SmtpMailFromCommand)cmd;
			Assert.Equal (Mailbox0, mailFromCmd.ReturnPath);

			// стартуем успешно
			sender.ReceivedReplies.Enqueue (SmtpReply.OK);
			transaction.StartAsync (Mailbox0).Wait ();
			Assert.Empty (sender.ReceivedReplies);
			cmd = sender.SendedCommands.Dequeue ();
			Assert.Empty (sender.SendedCommands);
			Assert.Equal (SmtpCommandType.MailFrom, cmd.CommandType);
			mailFromCmd = (SmtpMailFromCommand)cmd;
			Assert.Equal (Mailbox0, mailFromCmd.ReturnPath);

			// стартуем уже законченную
			transaction.Dispose ();
			Assert.ThrowsAsync<InvalidOperationException> (() => transaction.StartAsync (Mailbox0));
			Assert.Empty (sender.SendedCommands);
		}

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void TryAddRecipientAsync ()
		{
			var sender = new SmtpCommandReplyConnectionSenderReceiverMock ();
			var transaction = new SmtpSessionMailTransferTransactionHandler (
				new SmtpOriginatorProtocolSession (sender, "test.localhost"),
				ContentTransferEncoding.SevenBit,
				null);

			// добавляем получаетелй в не начатую
			Assert.ThrowsAsync<InvalidOperationException> (() => transaction.TryAddRecipientAsync (Mailbox1));
			Assert.Empty (sender.SendedCommands);

			// начинаем для дальнейших тестов
			sender.ReceivedReplies.Enqueue (SmtpReply.OK);
			transaction.StartAsync (Mailbox0).Wait ();
			sender.SendedCommands.Clear ();

			// добавляем получателя успешно
			sender.ReceivedReplies.Enqueue (SmtpReply.OK);
			Assert.Equal (RecipientAcceptanceState.Success, transaction.TryAddRecipientAsync (Mailbox1).Result);
			Assert.Empty (sender.ReceivedReplies);
			var cmd = sender.SendedCommands.Dequeue ();
			Assert.Empty (sender.SendedCommands);
			Assert.Equal (SmtpCommandType.RcptTo, cmd.CommandType);
			var rcptToCmd = (SmtpRcptToCommand)cmd;
			Assert.Equal (Mailbox1, rcptToCmd.Recipient);

			// добавляем получателя безуспешно
			sender.ReceivedReplies.Enqueue (SmtpReply.MailboxUnavailable);
			Assert.Equal (RecipientAcceptanceState.FailureMailboxUnavailable, transaction.TryAddRecipientAsync (Mailbox2).Result);
			Assert.Empty (sender.ReceivedReplies);
			cmd = sender.SendedCommands.Dequeue ();
			Assert.Empty (sender.SendedCommands);
			Assert.Equal (SmtpCommandType.RcptTo, cmd.CommandType);
			rcptToCmd = (SmtpRcptToCommand)cmd;
			Assert.Equal (Mailbox2, rcptToCmd.Recipient);

			transaction.Dispose ();
		}

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void TransferDataAndFinishAsync ()
		{
			var extensionsSupported = new HashSet<string> ();

			var sender = new SmtpCommandReplyConnectionSenderReceiverMock ();

			// передаём данные в не начатую
			var transaction = new SmtpSessionMailTransferTransactionHandler (
				new SmtpOriginatorProtocolSession (sender, "test.localhost"),
				ContentTransferEncoding.SevenBit,
				null);
			var src = new MemoryBufferedSource (Encoding.ASCII.GetBytes (MailBody));
			Assert.ThrowsAsync<InvalidOperationException> (() => transaction.TransferDataAndFinishAsync (src,  -1));
			Assert.Empty (sender.SendedCommands);
			transaction.Dispose ();

			// передаём данные не указав получателей
			transaction = new SmtpSessionMailTransferTransactionHandler (
				new SmtpOriginatorProtocolSession (sender, "test.localhost"),
				ContentTransferEncoding.SevenBit,
				null);
			sender.ReceivedReplies.Enqueue (SmtpReply.OK);
			transaction.StartAsync (Mailbox0).GetAwaiter ().GetResult ();
			src = new MemoryBufferedSource (Encoding.ASCII.GetBytes (MailBody));
			Assert.ThrowsAsync<InvalidOperationException> (() => transaction.TransferDataAndFinishAsync (src, -1));
			transaction.Dispose ();

			// передаем данные без поддержки CHUNKING
			transaction = PrepareSessionForDataTransfer (sender, extensionsSupported);
			sender.ReceivedReplies.Enqueue (SmtpReply.DataStart);
			sender.ReceivedReplies.Enqueue (SmtpReply.OK);
			src = new MemoryBufferedSource (Encoding.ASCII.GetBytes (MailBody));
			Assert.Empty (sender.SendedDataBlocks);
			transaction.TransferDataAndFinishAsync (src, -1).Wait ();
			Assert.Empty (sender.ReceivedReplies);
			var cmd = sender.SendedCommands.Dequeue ();
			Assert.Empty (sender.SendedCommands);
			Assert.Equal (SmtpCommandType.Data, cmd.CommandType);
			var block1 = sender.SendedDataBlocks.Dequeue ();
			var block2 = sender.SendedDataBlocks.Dequeue ();
			Assert.Empty (sender.SendedDataBlocks);
			Assert.Equal (MailBody, block1);
			Assert.Equal ("\r\n.\r\n", block2);
			transaction.Dispose ();

			// передаем данные без поддержки CHUNKING, недопустимый маркер конца внутри данных, должно вызвать UnrecoverableProtocolException
			transaction = PrepareSessionForDataTransfer (sender, extensionsSupported);
			sender.ReceivedReplies.Enqueue (SmtpReply.DataStart);
			src = new MemoryBufferedSource (Encoding.ASCII.GetBytes (InvalidMailBodyPart1 + InvalidMailBodyPart2));
			Assert.ThrowsAsync<UnrecoverableProtocolException> (() => transaction.TransferDataAndFinishAsync (src,  -1));
			Assert.Empty (sender.ReceivedReplies);
			cmd = sender.SendedCommands.Dequeue ();
			Assert.Empty (sender.SendedCommands);
			Assert.Equal (SmtpCommandType.Data, cmd.CommandType);
			block1 = sender.SendedDataBlocks.Dequeue ();
			Assert.Empty (sender.SendedDataBlocks);
			Assert.Equal (InvalidMailBodyPart1, block1);
			transaction.Dispose ();

			// передаем данные с поддержкой CHUNKING
			extensionsSupported.Add ("CHUNKING");
			transaction = PrepareSessionForDataTransfer (sender, extensionsSupported);
			sender.ReceivedReplies.Enqueue (SmtpReply.OK);
			src = new MemoryBufferedSource (Encoding.ASCII.GetBytes (MailBody));
			transaction.TransferDataAndFinishAsync (src, MailBody.Length).Wait ();
			Assert.Empty (sender.ReceivedReplies);
			cmd = sender.SendedCommands.Dequeue ();
			Assert.Empty (sender.SendedCommands);
			Assert.Equal (SmtpCommandType.Bdat, cmd.CommandType);
			block1 = sender.SendedDataBlocks.Dequeue ();
			Assert.Empty (sender.SendedDataBlocks);
			Assert.Equal (MailBody, block1);
			transaction.Dispose ();

			// передаем данные с поддержкой CHUNKING источник предоставляет данных меньше чем указано, должно вызвать UnrecoverableProtocolException
			transaction = PrepareSessionForDataTransfer (sender, extensionsSupported);
			src = new MemoryBufferedSource (Encoding.ASCII.GetBytes (MailBody));
			Assert.ThrowsAsync<UnrecoverableProtocolException> (() => transaction.TransferDataAndFinishAsync (src, MailBody.Length + 5));
			cmd = sender.SendedCommands.Dequeue ();
			Assert.Empty (sender.SendedCommands);
			Assert.Equal (SmtpCommandType.Bdat, cmd.CommandType);
			block1 = sender.SendedDataBlocks.Dequeue ();
			Assert.Empty (sender.SendedDataBlocks);
			Assert.Equal (MailBody, block1);
			transaction.Dispose ();
		}

		private SmtpSessionMailTransferTransactionHandler PrepareSessionForDataTransfer (
			SmtpCommandReplyConnectionSenderReceiverMock sender,
			HashSet<string> serverSupportedExtensions)
		{
			var session = new SmtpOriginatorProtocolSession (sender, "test.localhost");
			sender.ReceivedReplies.Enqueue (SmtpReply.CreateServiceReady ("test", new Version (0, 0)));
			sender.ReceivedReplies.Enqueue (SmtpReply.CreateHelloResponse ("test.localhost", serverSupportedExtensions));
			session.ReceiveGreetingAndStartAsync ().Wait ();

			// создаём начатую для дальнейших тестов
			var transaction = new SmtpSessionMailTransferTransactionHandler (
				session,
				ContentTransferEncoding.SevenBit,
				null);
			sender.ReceivedReplies.Enqueue (SmtpReply.OK);
			transaction.StartAsync (Mailbox0).Wait ();
			sender.ReceivedReplies.Enqueue (SmtpReply.OK);
			transaction.TryAddRecipientAsync (Mailbox1).Wait ();
			sender.SendedCommands.Clear ();
			return transaction;
		}
	}
}
