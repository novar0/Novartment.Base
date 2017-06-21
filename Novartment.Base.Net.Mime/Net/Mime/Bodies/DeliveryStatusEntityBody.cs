using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Collections;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Тело MIME-сущности, содержащее уведомление о статусе доставки сообщения.
	/// Определено в 3464.
	/// </summary>
	public class DeliveryStatusEntityBody :
		IEntityBody
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса DeliveryStatusEntityBody.
		/// </summary>
		public DeliveryStatusEntityBody ()
		{
		}

		/// <summary>Получает кодировку передачи содержимого тела сущности.</summary>
		public ContentTransferEncoding TransferEncoding => ContentTransferEncoding.Unspecified;

		/// <summary>Gets or sets envelope identifier that uniquely identifies the transaction during which the message was submitted.</summary>
		public string OriginalEnvelopeId { get; set; }

		/// <summary>Gets or sets MTA that attempted to perform the delivery, relay, or gateway operation described in the DSN.</summary>
		public NotificationFieldValue MailTransferAgent { get; set; }

		/// <summary>Gets or sets name of the gateway or MTA that translated a foreign (non-Internet) delivery status notification into this DSN.</summary>
		public NotificationFieldValue Gateway { get; set; }

		/// <summary>Gets or sets name of the MTA from which the message was received.</summary>
		public NotificationFieldValue ReceivedFromMailTransferAgent { get; set; }

		/// <summary>Gets or sets date and time at which the message arrived at the Reporting MTA.</summary>
		public DateTimeOffset? ArrivalDate { get; set; }

		/// <summary>
		/// Получает коллекцию свойств статуса доставки для каждого адресата.
		/// </summary>
		public IAdjustableList<RecipientDeliveryStatus> Recipients { get; } = new ArrayList<RecipientDeliveryStatus> ();

		/// <summary>
		/// Очищает тело сущности.
		/// </summary>
		public void Clear ()
		{
			this.OriginalEnvelopeId = null;
			this.MailTransferAgent = null;
			this.Gateway = null;
			this.ReceivedFromMailTransferAgent = null;
			this.ArrivalDate = null;
			this.Recipients.Clear ();
		}

		/// <summary>
		/// Загружает тело сущности из указанного источника данных.
		/// </summary>
		/// <param name="source">Источник данных, содержащий тело сущности.</param>
		/// <param name="subBodyFactory">Фабрика, позволяющая создавать тело вложенных сущностей с указанными параметрами.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая операцию загрузки.</returns>
		public Task LoadAsync (
			IBufferedSource source,
			Func<EssentialContentProperties, IEntityBody> subBodyFactory,
			CancellationToken cancellationToken)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			return LoadAsyncStateMachine ();

			async Task LoadAsyncStateMachine ()
			{
				// delivery-status-content = per-message-fields 1*( CarriageReturnLinefeed per-recipient-fields )
				// per-message-fields =
				//  [ original-envelope-id-field CarriageReturnLinefeed ]
				//  reporting-mta-field CarriageReturnLinefeed
				//  [ dsn-gateway-field CarriageReturnLinefeed ]
				//  [ received-from-mta-field CarriageReturnLinefeed ]
				//  [ arrival-date-field CarriageReturnLinefeed ]
				//  *( extension-field CarriageReturnLinefeed )
				// per-recipient-fields =
				//  [ original-recipient-field CarriageReturnLinefeed ]
				//  final-recipient-field CarriageReturnLinefeed
				//  action-field CarriageReturnLinefeed
				//  status-field CarriageReturnLinefeed
				//  [ remote-mta-field CarriageReturnLinefeed ]
				//  [ diagnostic-code-field CarriageReturnLinefeed ]
				//  [ last-attempt-date-field CarriageReturnLinefeed ]
				//  [ final-log-id-field CarriageReturnLinefeed ]
				//  [ will-retry-until-field CarriageReturnLinefeed ]
				//  *( extension-field CarriageReturnLinefeed )

				// Parse per-message fields.
				var headerSource = new TemplateSeparatedBufferedSource (source, HeaderDecoder.CarriageReturnLinefeed2, false);
				var headerFields = await HeaderDecoder.LoadHeaderFieldsAsync (headerSource, cancellationToken).ConfigureAwait (false);
				ParseHeader (headerFields);

				// Parse per-recipient fields.
				this.Recipients.Clear ();
				while (true)
				{
					var crlfFound = await headerSource.TrySkipPartAsync (cancellationToken).ConfigureAwait (false);
					if (!crlfFound || headerSource.IsEmpty ())
					{
						break;
					}

					headerFields = await HeaderDecoder.LoadHeaderFieldsAsync (headerSource, cancellationToken).ConfigureAwait (false);
					if (headerFields.Count < 1)
					{
						break;
					}

					this.Recipients.Add (ParseHeaderRecipient (headerFields));
				}
			}
		}

		/// <summary>
		/// Сохраняет тело сущности в указанный получатель двоичных данных.
		/// </summary>
		/// <param name="destination">Получатель двоичных данных, в который будет сохранено тело сущности.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая операцию сохранения.</returns>
		public Task SaveAsync (IBinaryDestination destination, CancellationToken cancellationToken)
		{
			if (destination == null)
			{
				throw new ArgumentNullException (nameof (destination));
			}

			Contract.EndContractBlock ();

			if (this.MailTransferAgent == null)
			{
				throw new InvalidOperationException ("Required property 'MailTransferAgent' not specified.");
			}

			var header = new ArrayList<HeaderFieldBuilder> ();
			CreateHeader (header);

			return SaveAsyncStateMachine ();

			async Task SaveAsyncStateMachine ()
			{
				await HeaderEncoder.SaveHeaderAsync (header, destination, cancellationToken)
					.ConfigureAwait (false);
				await destination.WriteAsync (HeaderDecoder.CarriageReturnLinefeed, 0, HeaderDecoder.CarriageReturnLinefeed.Length, cancellationToken)
					.ConfigureAwait (false);
				foreach (var recipientBlock in this.Recipients)
				{
					var headerFields = CreateHeaderRecipient (recipientBlock);
					await HeaderEncoder.SaveHeaderAsync (headerFields, destination, cancellationToken)
						.ConfigureAwait (false);
					await destination.WriteAsync (HeaderDecoder.CarriageReturnLinefeed, 0, HeaderDecoder.CarriageReturnLinefeed.Length, cancellationToken)
						.ConfigureAwait (false);
				}
			}
		}

		// Создаёт коллекцию свойств уведомления о статусе доставки сообщения конкретному адресату
		// на основе указанной коллекции полей заголовка уведомления о статусе доставки сообщения конкретному адресату.
		private static RecipientDeliveryStatus ParseHeaderRecipient (IReadOnlyCollection<HeaderField> fields)
		{
			NotificationFieldValue originalRecipient = null;
			NotificationFieldValue finalRecipient = null;
			var action = DeliveryAttemptResult.Unspecified;
			string status = null;
			NotificationFieldValue remoteMta = null;
			NotificationFieldValue diagnosticCode = null;
			DateTimeOffset? lastAttemptDate = null;
			string finalLogId = null;
			DateTimeOffset? willRetryUntil = null;

			foreach (var field in fields)
			{
				switch (field.Name)
				{
					case HeaderFieldName.OriginalRecipient:
						if (originalRecipient != null)
						{
							throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.OriginalRecipient) + "' field.");
						}

						originalRecipient = HeaderDecoder.DecodeNotificationFieldValue (field.Value);
						break;
					case HeaderFieldName.FinalRecipient:
						if (finalRecipient != null)
						{
							throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.FinalRecipient) + "' field.");
						}

						finalRecipient = HeaderDecoder.DecodeNotificationFieldValue (field.Value);
						break;
					case HeaderFieldName.Action:
						if (action != DeliveryAttemptResult.Unspecified)
						{
							throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.Action) + "' field.");
						}

						var actionStr = field.Value.Trim ();
						var isValidAction = DeliveryStatusActionHelper.TryParse (actionStr, out action);
						if (!isValidAction)
						{
							throw new FormatException ("Unrecognized value of Delivery Status Action: '" + actionStr + "'.");
						}

						break;
					case HeaderFieldName.Status:
						if (status != null)
						{
							throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.Status) + "' field.");
						}

						status = field.Value.Trim ();
						break;
					case HeaderFieldName.RemoteMailTransferAgent:
						if (remoteMta != null)
						{
							throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.RemoteMailTransferAgent) + "' field.");
						}

						remoteMta = HeaderDecoder.DecodeNotificationFieldValue (field.Value);
						break;
					case HeaderFieldName.DiagnosticCode:
						if (diagnosticCode != null)
						{
							throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.DiagnosticCode) + "' field.");
						}

						diagnosticCode = HeaderDecoder.DecodeNotificationFieldValue (field.Value);
						break;
					case HeaderFieldName.LastAttemptDate:
						if (lastAttemptDate.HasValue)
						{
							throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.LastAttemptDate) + "' field.");
						}

						lastAttemptDate = InternetDateTime.Parse (field.Value);
						break;
					case HeaderFieldName.FinalLogId:
						if (finalLogId != null)
						{
							throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.FinalLogId) + "' field.");
						}

						finalLogId = HeaderDecoder.DecodeUnstructured (field.Value).Trim ();
						break;
					case HeaderFieldName.WillRetryUntil:
						if (willRetryUntil.HasValue)
						{
							throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.WillRetryUntil) + "' field.");
						}

						willRetryUntil = InternetDateTime.Parse (field.Value);
						break;
				}
			}

			if (finalRecipient == null)
			{
				throw new FormatException ("Required field '" + HeaderFieldNameHelper.GetName (HeaderFieldName.FinalRecipient) + "' not specified.");
			}

			if (action == DeliveryAttemptResult.Unspecified)
			{
				throw new FormatException ("Required field '" + HeaderFieldNameHelper.GetName (HeaderFieldName.Action) + "' not specified.");
			}

			if (status == null)
			{
				throw new FormatException ("Required field '" + HeaderFieldNameHelper.GetName (HeaderFieldName.Status) + "' not specified.");
			}

			return new RecipientDeliveryStatus (finalRecipient, action, status)
			{
				OriginalRecipient = originalRecipient,
				RemoteMailTransferAgent = remoteMta,
				DiagnosticCode = diagnosticCode,
				LastAttemptDate = lastAttemptDate,
				FinalLogId = finalLogId,
				WillRetryUntil = willRetryUntil,
			};
		}

		// Создаёт поля заголовка содержащую все свойства коллекции.
		private static IReadOnlyList<HeaderFieldBuilder> CreateHeaderRecipient (RecipientDeliveryStatus recipient)
		{
			if (recipient.FinalRecipient == null)
			{
				throw new InvalidOperationException ("Required property 'FinalRecipient' not specified.");
			}

			if (recipient.Action == DeliveryAttemptResult.Unspecified)
			{
				throw new InvalidOperationException ("Required property 'Action' not specified.");
			}

			if (recipient.Status == null)
			{
				throw new InvalidOperationException ("Required property 'Status' not specified.");
			}

			var fields = new ArrayList<HeaderFieldBuilder> ();

			// Original-Recipient
			if (recipient.OriginalRecipient != null)
			{
				fields.Add (HeaderFieldBuilder.CreateAtomAndUnstructured (
					HeaderFieldName.OriginalRecipient,
					recipient.OriginalRecipient.Kind.GetName (),
					recipient.OriginalRecipient.Value));
			}

			// Final-Recipient
			fields.Add (HeaderFieldBuilder.CreateAtomAndUnstructured (
				HeaderFieldName.FinalRecipient,
				recipient.FinalRecipient.Kind.GetName (),
				recipient.FinalRecipient.Value));

			// Action
			fields.Add (HeaderFieldBuilder.CreateExactValue (
				HeaderFieldName.Action,
				recipient.Action.GetName ()));

			// Status
			var isValidInternetDomainName = AsciiCharSet.IsValidInternetDomainName (recipient.Status);
			if (!isValidInternetDomainName)
			{
				throw new FormatException ("Ivalid 'dot-atom' identificator: '" + recipient.Status + "'.");
			}

			fields.Add (HeaderFieldBuilder.CreateExactValue (
				HeaderFieldName.Status,
				recipient.Status));

			// Remote-MTA
			if (recipient.RemoteMailTransferAgent != null)
			{
				fields.Add (HeaderFieldBuilder.CreateAtomAndUnstructured (
					HeaderFieldName.RemoteMailTransferAgent,
					recipient.RemoteMailTransferAgent.Kind.GetName (),
					recipient.RemoteMailTransferAgent.Value));
			}

			// Diagnostic-Code
			if (recipient.DiagnosticCode != null)
			{
				fields.Add (HeaderFieldBuilder.CreateAtomAndUnstructured (
					HeaderFieldName.DiagnosticCode,
					recipient.DiagnosticCode.Kind.GetName (),
					recipient.DiagnosticCode.Value));
			}

			// Last-Attempt-Date
			if (recipient.LastAttemptDate.HasValue)
			{
				fields.Add (HeaderFieldBuilder.CreateExactValue (
					HeaderFieldName.LastAttemptDate,
					recipient.LastAttemptDate.Value.ToInternetString ()));
			}

			// Final-Log-ID
			if (recipient.FinalLogId != null)
			{
				fields.Add (HeaderFieldBuilder.CreateUnstructured (
					HeaderFieldName.FinalLogId,
					recipient.FinalLogId));
			}

			// Will-Retry-Until
			if (recipient.WillRetryUntil.HasValue)
			{
				fields.Add (HeaderFieldBuilder.CreateExactValue (
					HeaderFieldName.WillRetryUntil,
					recipient.WillRetryUntil.Value.ToInternetString ()));
			}

			return fields.GetReadOnlyView ();
		}

		// Создаёт коллекцию свойств уведомления о статусе доставки сообщения
		// на основе указанной коллекции полей заголовка уведомления о статусе доставки сообщения.
		private void ParseHeader (IReadOnlyCollection<HeaderField> fields)
		{
			string originalEnvelopeId = null;
			NotificationFieldValue reportingMailTransferAgent = null;
			NotificationFieldValue gateway = null;
			NotificationFieldValue receivedFromMailTransferAgent = null;
			DateTimeOffset? arrivalDate = null;

			foreach (var field in fields)
			{
				switch (field.Name)
				{
					case HeaderFieldName.OriginalEnvelopeId:
						if (originalEnvelopeId != null)
						{
							throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.OriginalEnvelopeId) + "' field.");
						}

						originalEnvelopeId = HeaderDecoder.DecodeUnstructured (field.Value).Trim ();
						break;
					case HeaderFieldName.MailTransferAgent:
						if (reportingMailTransferAgent != null)
						{
							throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.MailTransferAgent) + "' field.");
						}

						// reporting-mta-field = "Reporting-MTA" ":" mta-name-type ";" mta-name
						// mta-name = *text
						reportingMailTransferAgent = HeaderDecoder.DecodeNotificationFieldValue (field.Value);
						break;
					case HeaderFieldName.DsnGateway:
						if (gateway != null)
						{
							throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.DsnGateway) + "' field.");
						}

						gateway = HeaderDecoder.DecodeNotificationFieldValue (field.Value);
						break;
					case HeaderFieldName.ReceivedFromMailTransferAgent:
						if (receivedFromMailTransferAgent != null)
						{
							throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.ReceivedFromMailTransferAgent) + "' field.");
						}

						receivedFromMailTransferAgent = HeaderDecoder.DecodeNotificationFieldValue (field.Value);
						break;
					case HeaderFieldName.ArrivalDate:
						if (arrivalDate.HasValue)
						{
							throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.ArrivalDate) + "' field.");
						}

						// arrival-date-field = "Arrival-Date" ":" date-time
						arrivalDate = InternetDateTime.Parse (field.Value);
						break;
				}
			}

			if (reportingMailTransferAgent == null)
			{
				throw new FormatException ("Required field '" + HeaderFieldNameHelper.GetName (HeaderFieldName.MailTransferAgent) + "' not specified.");
			}

			this.MailTransferAgent = reportingMailTransferAgent;
			this.OriginalEnvelopeId = originalEnvelopeId;
			this.Gateway = gateway;
			this.ReceivedFromMailTransferAgent = receivedFromMailTransferAgent;
			this.ArrivalDate = arrivalDate;
		}

		// Создаёт поля заголовка содержащую все свойства коллекции.
		private void CreateHeader (IAdjustableCollection<HeaderFieldBuilder> fields)
		{
			// Original-Envelope-Id
			if (this.OriginalEnvelopeId != null)
			{
				fields.Add (HeaderFieldBuilder.CreateUnstructured (
					HeaderFieldName.OriginalEnvelopeId,
					this.OriginalEnvelopeId));
			}

			// Reporting-MTA
			if (this.MailTransferAgent != null)
			{
				fields.Add (HeaderFieldBuilder.CreateAtomAndUnstructured (
					HeaderFieldName.MailTransferAgent,
					this.MailTransferAgent.Kind.GetName (),
					this.MailTransferAgent.Value));
			}

			// DSN-Gateway
			if (this.Gateway != null)
			{
				fields.Add (HeaderFieldBuilder.CreateAtomAndUnstructured (
					HeaderFieldName.DsnGateway,
					this.Gateway.Kind.GetName (),
					this.Gateway.Value));
			}

			// Received-From-Mta
			if (this.ReceivedFromMailTransferAgent != null)
			{
				fields.Add (HeaderFieldBuilder.CreateAtomAndUnstructured (
					HeaderFieldName.ReceivedFromMailTransferAgent,
					this.ReceivedFromMailTransferAgent.Kind.GetName (),
					this.ReceivedFromMailTransferAgent.Value));
			}

			// Arrival-Date
			if (this.ArrivalDate.HasValue)
			{
				fields.Add (HeaderFieldBuilder.CreateExactValue (
					HeaderFieldName.ArrivalDate,
					this.ArrivalDate.Value.ToInternetString ()));
			}
		}
	}
}
