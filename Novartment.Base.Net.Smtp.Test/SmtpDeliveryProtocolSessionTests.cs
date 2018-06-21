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
	public class SmtpDeliveryProtocolSessionTests
	{
		private TransactionBehavior _transactionBehavior = TransactionBehavior.Normal;

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void InvalidSequenceOfCommands ()
		{
			var createdTransactionsCount = 0;
			var sender = new SmtpCommandReplyConnectionSenderReceiverMock ();
			var session = new SmtpDeliveryProtocolSession (
				sender,
				new IPHostEndPoint (IPAddress.Loopback, 25),
				srcAttribs =>
				{
					createdTransactionsCount++;
					return new SmtDataTransferTransactionMock (null, null, TransactionBehavior.Normal);
				},
				"test.localhost",
				SmtpServerSecurityParameters.NoSecurity,
				null);

			sender.ReceivedCommands.Enqueue (new SmtpMailFromCommand (new AddrSpec ("source", "client.com"), ContentTransferEncoding.SevenBit, null));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			Assert.Equal (0, createdTransactionsCount);
			var reply = sender.SendedReplies.Dequeue ();
			Assert.Equal (503, reply.Code);
			Assert.Empty (sender.SendedReplies);

			sender.ReceivedCommands.Enqueue (new SmtpRcptToCommand (new AddrSpec ("source", "client.com")));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			Assert.Equal (0, createdTransactionsCount);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Equal (503, reply.Code);
			Assert.Empty (sender.SendedReplies);

			sender.ReceivedCommands.Enqueue (SmtpCommand.CachedCmdData);
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			Assert.Equal (0, createdTransactionsCount);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Equal (503, reply.Code);
			Assert.Empty (sender.SendedReplies);

			sender.ReceivedCommands.Enqueue (new SmtpHeloCommand ("my-client-ID-007"));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			Assert.Equal (0, createdTransactionsCount);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Equal (250, reply.Code);
			Assert.Empty (sender.SendedReplies);
			sender.ReceivedCommands.Enqueue (new SmtpRcptToCommand (new AddrSpec ("source", "client.com")));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			Assert.Equal (0, createdTransactionsCount);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Equal (503, reply.Code);
			Assert.Empty (sender.SendedReplies);

			sender.ReceivedCommands.Enqueue (SmtpCommand.CachedCmdData);
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			Assert.Equal (0, createdTransactionsCount);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Equal (503, reply.Code);
			Assert.Empty (sender.SendedReplies);

			sender.ReceivedCommands.Enqueue (new SmtpMailFromCommand (new AddrSpec ("source", "client.com"), ContentTransferEncoding.SevenBit, null));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			Assert.Equal (1, createdTransactionsCount);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Equal (250, reply.Code);
			Assert.Empty (sender.SendedReplies);
			sender.ReceivedCommands.Enqueue (SmtpCommand.CachedCmdData);
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			Assert.Equal (1, createdTransactionsCount);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Equal (554, reply.Code);
			Assert.Empty (sender.SendedReplies);
		}

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void ValidateTransaction ()
		{
			var validReversePath = new AddrSpec ("source", "client.com");
			var invalidReversePath = new AddrSpec ("postmaster", "server.com");
			var invalidRecipient = new AddrSpec ("abuse", "mail.ru");

			var createdTransactionsCount = 0;
			var sender = new SmtpCommandReplyConnectionSenderReceiverMock ();
			var session = new SmtpDeliveryProtocolSession (
				sender,
				new IPHostEndPoint (IPAddress.Loopback, 25),
				srcAttribs =>
				{
					createdTransactionsCount++;
					return new SmtDataTransferTransactionMock (invalidReversePath, invalidRecipient, TransactionBehavior.Normal);
				},
				"test.localhost",
				SmtpServerSecurityParameters.NoSecurity,
				null);

			sender.ReceivedCommands.Enqueue (new SmtpHeloCommand ("my-client-ID-007"));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			Assert.Equal (0, createdTransactionsCount);
			var reply = sender.SendedReplies.Dequeue ();
			Assert.Equal (250, reply.Code);
			Assert.Empty (sender.SendedReplies);

			// неподходящий адрес возврата
			sender.ReceivedCommands.Enqueue (new SmtpMailFromCommand (invalidReversePath, ContentTransferEncoding.SevenBit, null));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			Assert.Equal (1, createdTransactionsCount);
			Assert.Null (session.CurrentTransaction);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Equal (553, reply.Code);
			Assert.Empty (sender.SendedReplies);

			// неподходящий получатель
			sender.ReceivedCommands.Enqueue (new SmtpMailFromCommand (validReversePath, ContentTransferEncoding.SevenBit, null));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			Assert.Equal (2, createdTransactionsCount);
			var trctn = (SmtDataTransferTransactionMock)session.CurrentTransaction;
			Assert.Equal (validReversePath, trctn.ReversePath);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Equal (250, reply.Code);
			Assert.Empty (sender.SendedReplies);

			sender.ReceivedCommands.Enqueue (new SmtpRcptToCommand (invalidRecipient));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			Assert.Equal (2, createdTransactionsCount);
			Assert.Equal (trctn, session.CurrentTransaction);
			Assert.Empty (trctn.Recipients);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Equal (550, reply.Code);
			Assert.Empty (sender.SendedReplies);

			sender.ReceivedCommands.Enqueue (SmtpCommand.CachedCmdData);
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			Assert.Equal (2, createdTransactionsCount);
			Assert.Null (session.CurrentTransaction);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Equal (554, reply.Code);
			Assert.Empty (sender.SendedReplies);
		}

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void MultipleTransactions ()
		{
			var createdTransactionsCount = 0;
			var sender = new SmtpCommandReplyConnectionSenderReceiverMock ();
			var session = new SmtpDeliveryProtocolSession (
				sender,
				new IPHostEndPoint (IPAddress.Loopback, 25),
				srcAttribs =>
				{
					createdTransactionsCount++;
					return new SmtDataTransferTransactionMock (null, null, TransactionBehavior.Normal);
				},
				"test.localhost",
				SmtpServerSecurityParameters.NoSecurity,
				null);

			sender.ReceivedCommands.Enqueue (new SmtpHeloCommand ("my-client-ID-007"));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			Assert.Equal (0, createdTransactionsCount);
			var reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);

			// first
			var mailbox = new AddrSpec ("error", "hacker.net");
			sender.ReceivedCommands.Enqueue (new SmtpMailFromCommand (mailbox, ContentTransferEncoding.SevenBit, null));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			Assert.Equal (1, createdTransactionsCount);
			var trctn = (SmtDataTransferTransactionMock)session.CurrentTransaction;
			Assert.Equal (mailbox, trctn.ReversePath);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);
			Assert.False (trctn.Completed);
			Assert.False (trctn.Disposed);

			sender.ReceivedCommands.Enqueue (SmtpCommand.CachedCmdNoop);
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			Assert.Equal (1, createdTransactionsCount);
			Assert.Equal (trctn, session.CurrentTransaction);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);

			sender.ReceivedCommands.Enqueue (new SmtpRcptToCommand (mailbox));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			Assert.Equal (1, createdTransactionsCount);
			Assert.Equal (trctn, session.CurrentTransaction);
			Assert.Equal (mailbox, trctn.Recipients[0]);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);

			sender.ReceivedCommands.Enqueue (SmtpCommand.CachedCmdData);
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			Assert.Equal (1, createdTransactionsCount);
			Assert.Equal (trctn, session.CurrentTransaction);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (354, reply.Code);

			var mailBody = "Hello dear!\r\nTell me please how you feel about last meeting.";
			var src = new ArrayBufferedSource (Encoding.ASCII.GetBytes (mailBody + "\r\n.\r\n"));
			sender.ReceivedCommands.Enqueue (new SmtpActualDataCommand (src, true));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);
			Assert.Equal (1, createdTransactionsCount);
			Assert.Null (session.CurrentTransaction);
			Assert.True (trctn.Completed);
			Assert.True (trctn.Disposed);
			Assert.Equal (mailBody, trctn.ReadedData);
			Assert.Equal (mailBody.Length + 5, src.Offset);

			// second
			mailbox = new AddrSpec ("source", "client.com");
			sender.ReceivedCommands.Enqueue (new SmtpMailFromCommand (mailbox, ContentTransferEncoding.SevenBit, null));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);
			Assert.Equal (2, createdTransactionsCount);
			Assert.NotNull (session.CurrentTransaction);
			trctn = (SmtDataTransferTransactionMock)session.CurrentTransaction;
			Assert.Equal (mailbox, trctn.ReversePath);

			mailbox = new AddrSpec ("support", "www.ru");
			sender.ReceivedCommands.Enqueue (new SmtpRcptToCommand (mailbox));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);
			Assert.Equal (2, createdTransactionsCount);
			Assert.Single (trctn.Recipients);
			Assert.Equal (mailbox, trctn.Recipients[0]);

			sender.ReceivedCommands.Enqueue (SmtpCommand.CachedCmdNoop);
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);
			Assert.Equal (2, createdTransactionsCount);

			sender.ReceivedCommands.Enqueue (SmtpCommand.CachedCmdNoop);
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);
			Assert.Equal (2, createdTransactionsCount);

			mailbox = new AddrSpec ("abuse", "mail.ru");
			sender.ReceivedCommands.Enqueue (new SmtpRcptToCommand (mailbox));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);
			Assert.Equal (2, createdTransactionsCount);
			Assert.Equal (2, trctn.Recipients.Count);
			Assert.Equal (mailbox, trctn.Recipients[1]);

			mailbox = new AddrSpec ("admin", "www.gov.ru");
			sender.ReceivedCommands.Enqueue (new SmtpRcptToCommand (mailbox));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);
			Assert.Equal (3, trctn.Recipients.Count);
			Assert.Equal (mailbox, trctn.Recipients[2]);
			Assert.False (trctn.Completed);
			Assert.False (trctn.Disposed);

			mailBody = "33 sample\r\n\r\n\r\ndata 33";
			src = new ArrayBufferedSource (Encoding.ASCII.GetBytes (mailBody + "\r\n.\r\n"));
			sender.ReceivedCommands.Enqueue (new SmtpActualDataCommand (src, true));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);
			Assert.Equal (2, createdTransactionsCount);
			Assert.Null (session.CurrentTransaction);
			Assert.True (trctn.Completed);
			Assert.True (trctn.Disposed);
			Assert.Equal (mailBody, trctn.ReadedData);
			Assert.Equal (mailBody.Length + 5, src.Offset);

			// third
			sender.ReceivedCommands.Enqueue (new SmtpHeloCommand ("anonymouse"));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);
			Assert.Equal (2, createdTransactionsCount);
			Assert.Null (session.CurrentTransaction);

			mailbox = new AddrSpec ("error", "disaster.com");
			sender.ReceivedCommands.Enqueue (new SmtpMailFromCommand (mailbox, ContentTransferEncoding.SevenBit, null));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);
			Assert.Equal (3, createdTransactionsCount);
			Assert.NotNull (session.CurrentTransaction);
			trctn = (SmtDataTransferTransactionMock)session.CurrentTransaction;
			Assert.Equal (mailbox, trctn.ReversePath);

			mailbox = new AddrSpec ("spam", "www.ru");
			sender.ReceivedCommands.Enqueue (new SmtpRcptToCommand (mailbox));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);
			Assert.Equal (3, createdTransactionsCount);
			Assert.Single (trctn.Recipients);
			Assert.Equal (mailbox, trctn.Recipients[0]);
			Assert.False (trctn.Completed);
			Assert.False (trctn.Disposed);
		}

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void Reset ()
		{
			var createdTransactionsCount = 0;
			var sender = new SmtpCommandReplyConnectionSenderReceiverMock ();
			var session = new SmtpDeliveryProtocolSession (
				sender,
				new IPHostEndPoint (IPAddress.Loopback, 25),
				srcAttribs =>
				{
					createdTransactionsCount++;
					return new SmtDataTransferTransactionMock (null, null, TransactionBehavior.Normal);
				},
				"test.localhost",
				SmtpServerSecurityParameters.NoSecurity,
				null);

			sender.ReceivedCommands.Enqueue (new SmtpHeloCommand ("my-client-ID-007"));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			var reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);

			sender.ReceivedCommands.Enqueue (SmtpCommand.CachedCmdRset);
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);
			Assert.Equal (0, createdTransactionsCount);
			Assert.Null (session.CurrentTransaction);

			// first
			var mailbox = new AddrSpec ("error", "hacker.net");
			sender.ReceivedCommands.Enqueue (new SmtpMailFromCommand (mailbox, ContentTransferEncoding.SevenBit, null));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);
			Assert.Equal (1, createdTransactionsCount);
			Assert.NotNull (session.CurrentTransaction);
			var trctn = (SmtDataTransferTransactionMock)session.CurrentTransaction;
			Assert.Equal (mailbox, trctn.ReversePath);
			Assert.False (trctn.Completed);
			Assert.False (trctn.Disposed);

			sender.ReceivedCommands.Enqueue (SmtpCommand.CachedCmdRset);
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);
			Assert.Equal (1, createdTransactionsCount);
			Assert.Null (session.CurrentTransaction);
			Assert.False (trctn.Completed);
			Assert.True (trctn.Disposed);

			// second
			mailbox = new AddrSpec ("source", "client.com");
			sender.ReceivedCommands.Enqueue (new SmtpMailFromCommand (mailbox, ContentTransferEncoding.SevenBit, null));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);
			Assert.Equal (2, createdTransactionsCount);
			Assert.NotNull (session.CurrentTransaction);
			trctn = (SmtDataTransferTransactionMock)session.CurrentTransaction;
			Assert.Equal (mailbox, trctn.ReversePath);

			mailbox = new AddrSpec ("support", "www.ru");
			sender.ReceivedCommands.Enqueue (new SmtpRcptToCommand (mailbox));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);
			Assert.Equal (2, createdTransactionsCount);
			Assert.Single (trctn.Recipients);
			Assert.Equal (mailbox, trctn.Recipients[0]);

			mailbox = new AddrSpec ("abuse", "mail.ru");
			sender.ReceivedCommands.Enqueue (new SmtpRcptToCommand (mailbox));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);
			Assert.Equal (2, createdTransactionsCount);
			Assert.Equal (2, trctn.Recipients.Count);
			Assert.Equal (mailbox, trctn.Recipients[1]);

			sender.ReceivedCommands.Enqueue (SmtpCommand.CachedCmdRset);
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);
			Assert.Equal (2, createdTransactionsCount);
			Assert.Null (session.CurrentTransaction);
			Assert.False (trctn.Completed);
			Assert.True (trctn.Disposed);

			sender.ReceivedCommands.Enqueue (SmtpCommand.CachedCmdRset);
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);
			Assert.Equal (2, createdTransactionsCount);
			Assert.Null (session.CurrentTransaction);
		}

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void ExceptionInTransaction ()
		{
			var createdTransactionsCount = 0;
			SmtDataTransferTransactionMock lastTransaction = null;
			var sender = new SmtpCommandReplyConnectionSenderReceiverMock ();
			var session = new SmtpDeliveryProtocolSession (
				sender,
				new IPHostEndPoint (IPAddress.Loopback, 25),
				srcAttribs =>
				{
					createdTransactionsCount++;
					lastTransaction = new SmtDataTransferTransactionMock (null, null, _transactionBehavior);
					return lastTransaction;
				},
				"test.localhost",
				SmtpServerSecurityParameters.NoSecurity,
				null);

			sender.ReceivedCommands.Enqueue (new SmtpHeloCommand ("my-client-ID-007"));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			var reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);

			// исключение в IMailDataTransferTransaction.TryStartAsync()
			// сбрасывает начатую транзакцию
			_transactionBehavior = TransactionBehavior.FailStarting;

			sender.ReceivedCommands.Enqueue (new SmtpMailFromCommand (null, ContentTransferEncoding.SevenBit, null));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (451, reply.Code);
			Assert.Equal (1, createdTransactionsCount);
			Assert.Null (session.CurrentTransaction);
			Assert.NotNull (lastTransaction);
			Assert.False (lastTransaction.Completed);
			Assert.True (lastTransaction.Disposed);

			// исключение в IMailDataTransferTransaction.TryAddRecipientAsync()
			// не сбрасывает начатую транзакцию
			_transactionBehavior = TransactionBehavior.FailAddingRecipient;
			var mailbox = new AddrSpec ("error", "hacker.net");
			sender.ReceivedCommands.Enqueue (new SmtpMailFromCommand (mailbox, ContentTransferEncoding.SevenBit, null));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);
			Assert.Equal (2, createdTransactionsCount);
			Assert.NotNull (session.CurrentTransaction);
			var trctn = (SmtDataTransferTransactionMock)session.CurrentTransaction;
			Assert.False (trctn.Completed);
			Assert.False (trctn.Disposed);

			sender.ReceivedCommands.Enqueue (new SmtpRcptToCommand (null));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (451, reply.Code);
			Assert.Equal (2, createdTransactionsCount);
			Assert.NotNull (session.CurrentTransaction);
			Assert.False (trctn.Completed);
			Assert.False (trctn.Disposed);

			sender.ReceivedCommands.Enqueue (SmtpCommand.CachedCmdRset);
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);
			Assert.Equal (2, createdTransactionsCount);
			Assert.Null (session.CurrentTransaction);
			Assert.False (trctn.Completed);
			Assert.True (trctn.Disposed);

			// исключение в IMailDataTransferTransaction.TransferDataAsync()
			// сбрасывает начатую транзакцию
			_transactionBehavior = TransactionBehavior.FailProcessData;
			mailbox = new AddrSpec ("error", "hacker.net");
			sender.ReceivedCommands.Enqueue (new SmtpMailFromCommand (mailbox, ContentTransferEncoding.SevenBit, null));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);
			Assert.Equal (3, createdTransactionsCount);
			Assert.NotNull (session.CurrentTransaction);
			trctn = (SmtDataTransferTransactionMock)session.CurrentTransaction;
			Assert.Equal (mailbox, trctn.ReversePath);
			Assert.False (trctn.Completed);
			Assert.False (trctn.Disposed);

			mailbox = new AddrSpec ("support", "www.ru");
			sender.ReceivedCommands.Enqueue (new SmtpRcptToCommand (mailbox));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);
			Assert.Equal (3, createdTransactionsCount);
			Assert.Single (trctn.Recipients);
			Assert.Equal (mailbox, trctn.Recipients[0]);

			sender.ReceivedCommands.Enqueue (SmtpCommand.CachedCmdData);
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (354, reply.Code);
			Assert.Equal (3, createdTransactionsCount);

			var mailBody = "Hello\r\n.\r\n";
			var src = new ArrayBufferedSource (Encoding.ASCII.GetBytes (mailBody));
			sender.ReceivedCommands.Enqueue (new SmtpActualDataCommand (src, true));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (451, reply.Code);
			Assert.Equal (3, createdTransactionsCount);
			Assert.Null (session.CurrentTransaction);
			Assert.False (trctn.Completed);
			Assert.True (trctn.Disposed);
		}

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void MergingBdatChunks ()
		{
			var createdTransactionsCount = 0;
			var sender = new SmtpCommandReplyConnectionSenderReceiverMock ();
			var session = new SmtpDeliveryProtocolSession (
				sender,
				new IPHostEndPoint (IPAddress.Loopback, 25),
				srcAttribs =>
				{
					createdTransactionsCount++;
					return new SmtDataTransferTransactionMock (null, null, TransactionBehavior.Normal);
				},
				"test.localhost",
				SmtpServerSecurityParameters.NoSecurity,
				null);

			var trctn = SetUpTransaction (session, sender);

			var mailBodyChunk1 = "Hello dear!\r\n";
			var mailBodyChunk2 = "Tell me please how you feel about last meeting.";
			Memory<byte> bytes = Encoding.ASCII.GetBytes (mailBodyChunk1 + mailBodyChunk2);
			var src1 = new ArrayBufferedSource (bytes.Slice (0, mailBodyChunk1.Length));
			var src2 = new ArrayBufferedSource (bytes.Slice (mailBodyChunk1.Length));

			sender.ReceivedCommands.Enqueue (new SmtpBdatCommand (src1, mailBodyChunk1.Length, false));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			var reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);
			Assert.Equal (1, createdTransactionsCount);
			Assert.False (trctn.Completed);
			Assert.False (trctn.Disposed);
			Assert.Equal (mailBodyChunk1.Length, src1.Offset);

			sender.ReceivedCommands.Enqueue (new SmtpBdatCommand (src2, mailBodyChunk2.Length, true));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);
			Assert.Equal (1, createdTransactionsCount);
			Assert.True (trctn.Completed);
			Assert.True (trctn.Disposed);
			Assert.Equal (mailBodyChunk2.Length, src2.Offset);
			Assert.Equal (mailBodyChunk1 + mailBodyChunk2, trctn.ReadedData);
		}

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void UnexpectedEndOfData ()
		{
			var createdTransactionsCount = 0;
			var sender = new SmtpCommandReplyConnectionSenderReceiverMock ();
			var session = new SmtpDeliveryProtocolSession (
				sender,
				new IPHostEndPoint (IPAddress.Loopback, 25),
				srcAttribs =>
				{
					createdTransactionsCount++;
					return new SmtDataTransferTransactionMock (null, null, TransactionBehavior.Normal);
				},
				"test.localhost",
				SmtpServerSecurityParameters.NoSecurity,
				null);

			// сессия с передачей данных командой DATA
			var trctn = SetUpTransaction (session, sender);

			sender.ReceivedCommands.Enqueue (SmtpCommand.CachedCmdData);
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			var reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (354, reply.Code);
			Assert.Equal (1, createdTransactionsCount);
			var mailBody = "Hello";
			var mailBodyBytes = Encoding.ASCII.GetBytes (mailBody);

			// источник не сможет пропустить разделитель, что означает неожиданный обрыв данных
			var src = new ArrayBufferedSource (mailBodyBytes);
			sender.ReceivedCommands.Enqueue (new SmtpActualDataCommand (src, true));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (451, reply.Code);
			Assert.Equal (1, createdTransactionsCount);
			Assert.Null (session.CurrentTransaction);
			Assert.False (trctn.Completed);
			Assert.True (trctn.Disposed);

			// сессия с передачей данных командой BDAT
			trctn = SetUpTransaction (session, sender);

			// укажем размер больше чем у источника, что означает неожиданный обрыв данных
			var src2 = new SizeLimitedBufferedSource (new ArrayBufferedSource (mailBodyBytes), mailBodyBytes.Length + 555);
			sender.ReceivedCommands.Enqueue (new SmtpBdatCommand (src2, mailBodyBytes.Length + 555, false));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (451, reply.Code);
			Assert.Null (session.CurrentTransaction);
			Assert.Equal (2, createdTransactionsCount);
			Assert.False (trctn.Completed);
			Assert.True (trctn.Disposed);
		}

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void UnrecoverableInvalidSyntax ()
		{
			var createdTransactionsCount = 0;
			var sender = new SmtpCommandReplyConnectionSenderReceiverMock ();
			var session = new SmtpDeliveryProtocolSession (
				sender,
				new IPHostEndPoint (IPAddress.Loopback, 25),
				srcAttribs =>
				{
					createdTransactionsCount++;
					return new SmtDataTransferTransactionMock (null, null, TransactionBehavior.Normal);
				},
				"test.localhost",
				SmtpServerSecurityParameters.NoSecurity,
				null);

			// сессия с передачей данных командой DATA
			var trctn = SetUpTransaction (session, sender);

			var mailBody = "Hello";
			var src = new ArrayBufferedSource (Encoding.ASCII.GetBytes (mailBody));
			sender.ReceivedCommands.Enqueue (new SmtpInvalidSyntaxCommand (SmtpCommandType.Bdat, "some unrecoverable error"));
			Assert.ThrowsAsync<UnrecoverableProtocolException> (() => session.ReceiveCommandSendReplyAsync (CancellationToken.None));
			Assert.Empty (sender.SendedReplies);
			Assert.True (trctn.Disposed);
		}

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void MailFromCancellation ()
		{
			var createdTransactionsCount = 0;
			SmtDataTransferTransactionMock currentTransaction = null;
			var sender = new SmtpCommandReplyConnectionSenderReceiverMock ();
			var session = new SmtpDeliveryProtocolSession (
				sender,
				new IPHostEndPoint (IPAddress.Loopback, 25),
				srcAttribs =>
				{
					createdTransactionsCount++;
					currentTransaction = new SmtDataTransferTransactionMock (null, null, TransactionBehavior.SlowStarting);
					return currentTransaction;
				},
				"test.localhost",
				SmtpServerSecurityParameters.NoSecurity,
				null);

			sender.ReceivedCommands.Enqueue (new SmtpHeloCommand ("my-client-ID-007"));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			var reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);

			var mailbox = new AddrSpec ("error", "hacker.net");
			var cts = new CancellationTokenSource ();
			sender.ReceivedCommands.Enqueue (new SmtpMailFromCommand (mailbox, ContentTransferEncoding.SevenBit, null));
			var task = session.ReceiveCommandSendReplyAsync (cts.Token);
			Assert.NotNull (currentTransaction);
			Assert.True (currentTransaction.SlowOperationInProgressEvent.WaitOne ());
			cts.Cancel ();
			Assert.ThrowsAsync<OperationCanceledException> (() => task).Wait ();
			Assert.Equal (1, createdTransactionsCount);
			Assert.Empty (sender.SendedReplies);
			Assert.False (currentTransaction.Completed);
			Assert.True (currentTransaction.Disposed);
			Assert.Null (session.CurrentTransaction);
		}

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void RcptToCancellation ()
		{
			var createdTransactionsCount = 0;
			SmtDataTransferTransactionMock currentTransaction = null;
			var sender = new SmtpCommandReplyConnectionSenderReceiverMock ();
			var session = new SmtpDeliveryProtocolSession (
				sender,
				new IPHostEndPoint (IPAddress.Loopback, 25),
				srcAttribs =>
				{
					createdTransactionsCount++;
					currentTransaction = new SmtDataTransferTransactionMock (null, null, TransactionBehavior.SlowAddingRecipient);
					return currentTransaction;
				},
				"test.localhost",
				SmtpServerSecurityParameters.NoSecurity,
				null);

			sender.ReceivedCommands.Enqueue (new SmtpHeloCommand ("my-client-ID-007"));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			var reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);

			var mailbox = new AddrSpec ("error", "hacker.net");
			sender.ReceivedCommands.Enqueue (new SmtpMailFromCommand (mailbox, ContentTransferEncoding.SevenBit, null));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);
			Assert.NotNull (session.CurrentTransaction);
			var trctn = (SmtDataTransferTransactionMock)session.CurrentTransaction;
			Assert.Equal (mailbox, trctn.ReversePath);
			Assert.False (trctn.Completed);
			Assert.False (trctn.Disposed);

			mailbox = new AddrSpec ("support", "www.ru");
			sender.ReceivedCommands.Enqueue (new SmtpRcptToCommand (mailbox));
			var cts = new CancellationTokenSource ();
			var task = session.ReceiveCommandSendReplyAsync (cts.Token);
			Assert.NotNull (session.CurrentTransaction);
			Assert.True (trctn.SlowOperationInProgressEvent.WaitOne ());
			cts.Cancel ();
			Assert.ThrowsAsync<TaskCanceledException> (() => task);
			Assert.Equal (1, createdTransactionsCount);
			Assert.False (trctn.Completed);
			Assert.False (trctn.Disposed);
			Assert.NotNull (session.CurrentTransaction);
		}

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void ActualDataCancellation ()
		{
			var createdTransactionsCount = 0;
			SmtDataTransferTransactionMock currentTransaction = null;
			var sender = new SmtpCommandReplyConnectionSenderReceiverMock ();
			var session = new SmtpDeliveryProtocolSession (
				sender,
				new IPHostEndPoint (IPAddress.Loopback, 25),
				srcAttribs =>
				{
					createdTransactionsCount++;
					currentTransaction = new SmtDataTransferTransactionMock (null, null, TransactionBehavior.SlowProcessData);
					return currentTransaction;
				},
				"test.localhost",
				SmtpServerSecurityParameters.NoSecurity,
				null);

			var trctn = SetUpTransaction (session, sender);

			var mailBody = "Hello";
			var src = new ArrayBufferedSource (Encoding.ASCII.GetBytes (mailBody));
			sender.ReceivedCommands.Enqueue (new SmtpActualDataCommand (src, true));
			var cts = new CancellationTokenSource ();
			var task = session.ReceiveCommandSendReplyAsync (cts.Token);
			Assert.True (trctn.SlowOperationInProgressEvent.WaitOne ());
			cts.Cancel ();
			Assert.ThrowsAsync<OperationCanceledException> (() => task).Wait ();
			Assert.Equal (1, createdTransactionsCount);
			Assert.False (trctn.Completed);
			Assert.True (trctn.Disposed);
			Assert.Null (session.CurrentTransaction);
			Assert.Empty (sender.SendedReplies);
		}

		[Fact]
		[Trait ("Category", "Net.Smtp")]
		public void BdatCancellation ()
		{
			var createdTransactionsCount = 0;
			SmtDataTransferTransactionMock currentTransaction = null;
			var sender = new SmtpCommandReplyConnectionSenderReceiverMock ();
			var session = new SmtpDeliveryProtocolSession (
				sender,
				new IPHostEndPoint (IPAddress.Loopback, 25),
				srcAttribs =>
				{
					createdTransactionsCount++;
					currentTransaction = new SmtDataTransferTransactionMock (null, null, TransactionBehavior.SlowProcessData);
					return currentTransaction;
				},
				"test.localhost",
				SmtpServerSecurityParameters.NoSecurity,
				null);

			var trctn = SetUpTransaction (session, sender);

			var mailBodyChunk = "Hello";
			var subSrc = new ArrayBufferedSource (Encoding.ASCII.GetBytes (mailBodyChunk));
			var src = new SizeLimitedBufferedSource (subSrc, mailBodyChunk.Length);
			sender.ReceivedCommands.Enqueue (new SmtpBdatCommand (src, mailBodyChunk.Length, false));
			var cts = new CancellationTokenSource ();
			var task = session.ReceiveCommandSendReplyAsync (cts.Token);
			Assert.True (trctn.SlowOperationInProgressEvent.WaitOne ());
			cts.Cancel ();
			Assert.ThrowsAsync<TaskCanceledException> (() => task).Wait ();
			Assert.Equal (1, createdTransactionsCount);
			Assert.False (trctn.Completed);
			Assert.True (trctn.Disposed);
			Assert.Null (session.CurrentTransaction);
			Assert.Empty (sender.SendedReplies);
		}

		private SmtDataTransferTransactionMock SetUpTransaction (
			SmtpDeliveryProtocolSession session,
			SmtpCommandReplyConnectionSenderReceiverMock sender)
		{
			Assert.Null (session.CurrentTransaction);

			sender.ReceivedCommands.Enqueue (new SmtpHeloCommand ("my-client-ID-007"));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			var reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);

			var mailbox = new AddrSpec ("error", "hacker.net");
			sender.ReceivedCommands.Enqueue (new SmtpMailFromCommand (mailbox, ContentTransferEncoding.SevenBit, null));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);
			Assert.NotNull (session.CurrentTransaction);
			var trctn = (SmtDataTransferTransactionMock)session.CurrentTransaction;
			Assert.Equal (mailbox, trctn.ReversePath);
			Assert.False (trctn.Completed);
			Assert.False (trctn.Disposed);

			mailbox = new AddrSpec ("support", "www.ru");
			sender.ReceivedCommands.Enqueue (new SmtpRcptToCommand (mailbox));
			Assert.True (session.ReceiveCommandSendReplyAsync (CancellationToken.None).Result);
			reply = sender.SendedReplies.Dequeue ();
			Assert.Empty (sender.SendedReplies);
			Assert.Equal (250, reply.Code);
			Assert.Equal (trctn, session.CurrentTransaction);
			Assert.Single (trctn.Recipients);
			Assert.Equal (mailbox, trctn.Recipients[0]);

			return trctn;
		}
	}
}
