using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Collections;
using Novartment.Base.Net;
using Novartment.Base.Net.Mime;
using Novartment.Base.Net.Smtp;
using Novartment.Base.Text;

namespace Novartment.Base.Sample
{
	public static class SmtpSamples
	{
		public static async Task SendOneMessageAsync (CancellationToken cancellationToken)
		{
			// создаём сообщение
			var msg = MailMessage.CreateSimpleText ();
			msg.RecipientTo.Add ("someone@mailinator.com", "Адресат Один");
			msg.From.Add ("noone@server.net", "Иван Сидоров");
			msg.Subject = "тема сообщения";
			(msg.Body as TextEntityBody).SetText ("текст сообщения");

			// отправляем сообщение
			var hostName = "localhost";
			var addrs = await Dns.GetHostAddressesAsync (hostName).ConfigureAwait (false);
			var endpoint = new IPHostEndPoint (addrs[0], 25) { HostName = hostName };
			using (var connection = await SocketBinaryTcpConnection.CreateAsync (endpoint, cancellationToken)
				.ConfigureAwait (false))
			{
				var protocol = new SmtpOriginatorProtocol (
					msg.PerformTransferTransaction,
					SmtpClientSecurityParameters.AllowNoSecurity);
					/* SmtpClientSecurityParameters.RequireEncryptionUseCredentials (new NetworkCredential ("user", "password"))); */
				await protocol.StartAsync (connection, cancellationToken).ConfigureAwait (false);
			}
		}

		public static async Task StartSmtpServer ()
		{
			Encoding.RegisterProvider (CodePagesEncodingProvider.Instance);

			var loggerFactory = new LoggerFactory (
				new ILoggerProvider[] { new DebugLoggerProvider () },
				new LoggerFilterOptions () { MinLevel = LogLevel.Trace });
			var tcpServerLogger = loggerFactory.CreateLogger<TcpServer> ();
			var deliveryLogger = loggerFactory.CreateLogger<SmtpDeliveryProtocol> ();
			var originatorLogger = loggerFactory.CreateLogger<SmtpOriginatorProtocol> ();

			var localDomains = new string[] { "mychel.ru", "domain.org" };
			var mailPickupDirectory = @".\Pickup";
			var mailDropDirectory = @".\Drop";

			Directory.CreateDirectory (mailDropDirectory);
			Directory.CreateDirectory (mailPickupDirectory);

			// запускаем SMTP сервер, который будет принимать и сохранять сообщения в файл
			var deliveryServer = new TcpServer (endpoint => new SocketTcpListener (endpoint), tcpServerLogger);
			var localDomainsSet = new HashSet<string> (localDomains);
			var serverCertificate = new X509Certificate2 ("MyServer.pfx", "password");
			var deliveryProtocol = new SmtpDeliveryProtocol (
				srcAttribs => new DeliveryToFileDataTransferTransaction (srcAttribs, mailDropDirectory, mailPickupDirectory, localDomainsSet),
				SmtpServerSecurityParameters.NoSecurity,
				/* SmtpServerSecurityParameters.UseServerAndRequireClientCertificate (serverCertificate), */
				/* SmtpServerSecurityParameters.UseServerCertificateAndClientAuthenticator (serverCertificate, FindUser), */
				deliveryLogger);
			deliveryServer.AddListenEndpoint (new IPEndPoint (IPAddress.Any, 25), deliveryProtocol);

			// формируем список писем (отправитель,получатели) из папки, группируя их по домену назначения и файлу
			var msgLoadBuf = new byte[1024];
			var outgoingMailMessages = new CompetentDictionary<string, CompetentDictionary<string, MailMessageData>> (key => new CompetentDictionary<string, MailMessageData> (file => new MailMessageData (), StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);
			foreach (var fileName in Directory.EnumerateFiles (mailPickupDirectory, "test*.eml"))
			{
				MailMessage message;
				using (var fs = new FileStream (fileName, FileMode.Open, FileAccess.Read))
				{
					message = new MailMessage ();
					await message.LoadAsync (fs.AsBufferedSource (msgLoadBuf), EntityBodyFactory.Create)
						.ConfigureAwait (false);
					var returnPath = (message.Sender ?? message.From[0]).Address;
					foreach (var recipient in message.RecipientTo)
					{
						var data = outgoingMailMessages[recipient.Address.Domain][fileName];
						data.ReturnPath = returnPath;
						data.Recipients.Add (recipient.Address);
					}
				}
			}

			var tcs = new CancellationTokenSource ();
			var originateTask = SendMessagesAsync (outgoingMailMessages, originatorLogger, tcs.Token);
			Console.WriteLine ("SMTP server started. Press ENTER to quit...");
			Console.ReadLine ();
			Console.WriteLine ("Waiting clients and server to stop...");

			// командуем СТОП и ожидаем остановки клиентов и сервера
			tcs.Cancel ();
			var deliveryTask = deliveryServer.StopAsync (true);
			await Task.WhenAll (originateTask, deliveryTask).ConfigureAwait (false);
		}

		private static Task<object> FindUser (string userName, string password)
		{
			var userFound = (userName == "User Name") && (password == "paSsWORd");
			return Task.FromResult<object> (userFound ? (object)"postmater" : null); // valid user
		}

		private static async Task SendMessagesAsync (
			IReadOnlyDictionary<string, CompetentDictionary<string, MailMessageData>> outgoingMailMessages,
			ILogger<SmtpOriginatorProtocol> originatorLogger,
			CancellationToken cancellationToken)
		{
			var clientCertificates = new X509CertificateCollection
			{
				new X509Certificate2 ("MyClient.pfx", "password"),
			};

			// соединяемся с каждым доменом назначения и отправляем предназначенные ему письма
			foreach (var domainData in outgoingMailMessages)
			{
				var serverNameAndAddr = GetMailServerOfDomain (domainData.Key);
				var originatorProtocol = new SmtpOriginatorProtocol (
					(factory, ct) => OriginateTransactionsAsync (domainData.Value, factory, ct),
					SmtpClientSecurityParameters.AllowNoSecurity,
					/* SmtpClientSecurityParameters.RequireEncryptionUseCredentials (new NetworkCredential ("User Name", "paSsWORd")), */
					/* SmtpClientSecurityParameters.RequireEncryptionUseClientCertificate (clientCertificates), */
					originatorLogger);
				using (var connection = await SocketBinaryTcpConnection.CreateAsync (serverNameAndAddr, cancellationToken)
					.ConfigureAwait (false))
				{
					await Task.Delay (1000).ConfigureAwait (false); // имитация задержки при установке соединения
					await originatorProtocol.StartAsync (connection, cancellationToken).ConfigureAwait (false);
				}
			}
		}

		private static IPHostEndPoint GetMailServerOfDomain (string domain)
		{
			// запрашиваем у DNS адрес почтового сервера для указанного домена
			return new IPHostEndPoint (IPAddress.Loopback, 25) { HostName = "localhost" };
		}

		private static async Task OriginateTransactionsAsync (
			CompetentDictionary<string, MailMessageData> messagesToSend,
			TransactionHandlerFactory transactionFactory,
			CancellationToken cancellationToken)
		{
			foreach (var messageFileData in messagesToSend)
			{
				var transaction = transactionFactory.Invoke (ContentTransferEncoding.SevenBit);
				await transaction.StartAsync (
					messageFileData.Value.ReturnPath,
					cancellationToken).ConfigureAwait (false);
				foreach (var recipient in messageFileData.Value.Recipients)
				{
					await transaction.TryAddRecipientAsync (recipient, cancellationToken).ConfigureAwait (false);
				}

				using (var fs = new FileStream (messageFileData.Key, FileMode.Open, FileAccess.Read))
				{
					var msgSource = fs.AsBufferedSource (new byte[1000]);
					await transaction.TransferDataAndFinishAsync (msgSource, fs.Length, cancellationToken).ConfigureAwait (false);
				}
			}
		}

		internal class MailMessageData
		{
			internal AddrSpec ReturnPath { get; set; }

			internal ArrayList<AddrSpec> Recipients { get; } = new ArrayList<AddrSpec> (1);
		}

		internal class DeliveryToFileDataTransferTransaction : IMailTransferTransactionHandler
		{
			private readonly string _mailDropDirectory;
			private readonly string _mailPickupDirectory;
			private readonly ISet<string> _localDomains;
			private readonly MailDeliverySourceData _deliverySourceAttributes;
			private readonly ArrayList<AddrSpec> _recipients = new ArrayList<AddrSpec> (1);
			private AddrSpec _returnPath = null;

			internal DeliveryToFileDataTransferTransaction (
				MailDeliverySourceData deliverySourceAttributes,
				string mailDropDirectory,
				string mailPickupDirectory,
				ISet<string> localDomains)
			{
				_deliverySourceAttributes = deliverySourceAttributes;
				_mailDropDirectory = mailDropDirectory;
				_mailPickupDirectory = mailPickupDirectory;
				_localDomains = localDomains;
			}

			public void Dispose ()
			{
				_returnPath = null;
				_recipients.Clear ();
			}

			public Task StartAsync (AddrSpec returnPath, CancellationToken cancellationToken)
			{
				_returnPath = returnPath;
				return Task.CompletedTask;
			}

			public Task<RecipientAcceptanceState> TryAddRecipientAsync (AddrSpec recipient, CancellationToken cancellationToken)
			{
				_recipients.Add (recipient);
				return Task.FromResult (RecipientAcceptanceState.Success);
			}

			public async Task TransferDataAndFinishAsync (IBufferedSource source, long exactSize, CancellationToken cancellationToken)
			{
				var isMailDestinationLocal = _localDomains.Contains (_recipients[0].Domain);
				var fileName = CreateUniqueFileName ();
				using (var destStream = new FileStream (
					Path.Combine (
						isMailDestinationLocal ? _mailDropDirectory : _mailPickupDirectory,
						fileName),
					FileMode.Create,
					FileAccess.Write))
				{
					var destination = destStream.AsBinaryDestination ();

					// RFC 5321 part 3.6.3:
					// a relay SMTP has no need to inspect or act upon the header section or body of the message data and
					// MUST NOT do so except to add its own "Received:" header field
					if (isMailDestinationLocal && (_returnPath != null))
					{
						// RFC 5321 part 4.4:
						// When the delivery SMTP server makes the "final delivery" of a message,
						// it inserts a return-path line at the beginning of the mail data.
						var returnPath = "Return-Path:<" + _returnPath + ">\r\n";
						var buf = Encoding.ASCII.GetBytes (returnPath);
						await destination.WriteAsync (buf.AsMemory (), cancellationToken).ConfigureAwait (false);
					}

					// RFC 5321 part 4.4:
					// When an SMTP server receives a message for delivery or further processing,
					// it MUST insert trace ("time stamp" or "Received") information at the beginning of the message content.
					var ipProps = IPGlobalProperties.GetIPGlobalProperties ();
					var localHostFqdn = string.IsNullOrWhiteSpace (ipProps.DomainName) ?
						ipProps.HostName :
						ipProps.HostName + "." + ipProps.DomainName;
					var received = string.Format (
						CultureInfo.InvariantCulture,
						"Received: FROM {0} ({1}) BY {2};\r\n {3}\r\n",
						_deliverySourceAttributes.EndPoint.HostName,
						_deliverySourceAttributes.EndPoint.Address,
						localHostFqdn,
						DateTimeOffset.Now.ToInternetString ());
					var buf2 = Encoding.ASCII.GetBytes (received);
					await destination.WriteAsync (buf2.AsMemory (), cancellationToken).ConfigureAwait (false);
					await source.WriteToAsync (destination, cancellationToken).ConfigureAwait (false);
				}
			}

			private string CreateUniqueFileName ()
			{
				return string.Format (
					CultureInfo.InvariantCulture,
					"{0:yyyy-MM-ddTHH:mm:ss.fff} from {1}.eml",
					DateTime.Now,
					_returnPath)
					.Replace (":", "-", StringComparison.Ordinal)
					.Replace ("<", "{", StringComparison.Ordinal)
					.Replace (">", "}", StringComparison.Ordinal);
			}
		}
	}
}
