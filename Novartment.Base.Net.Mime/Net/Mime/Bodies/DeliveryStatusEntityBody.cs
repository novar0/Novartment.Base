using System;
using System.Buffers;
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
			CancellationToken cancellationToken = default)
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
				var header = await HeaderDecoder.LoadHeaderAsync (headerSource, cancellationToken).ConfigureAwait (false);

				var fieldBodyBuffer = ArrayPool<char>.Shared.Rent (HeaderDecoder.MaximumHeaderFieldBodySize);
				var fieldBodyElementBuffer = ArrayPool<char>.Shared.Rent (HeaderDecoder.MaximumHeaderFieldBodySize);
				try
				{
					ParseHeader (header, fieldBodyBuffer, fieldBodyElementBuffer);

					// Parse per-recipient fields.
					this.Recipients.Clear ();
					while (true)
					{
						var crlfFound = await headerSource.TrySkipPartAsync (cancellationToken).ConfigureAwait (false);
						if (!crlfFound || headerSource.IsEmpty ())
						{
							break;
						}

						header = await HeaderDecoder.LoadHeaderAsync (headerSource, cancellationToken).ConfigureAwait (false);
						if (header.Count < 1)
						{
							break;
						}

						this.Recipients.Add (ParseHeaderRecipient (header, fieldBodyBuffer, fieldBodyElementBuffer));
					}
				}
				finally
				{
					ArrayPool<char>.Shared.Return (fieldBodyElementBuffer);
					ArrayPool<char>.Shared.Return (fieldBodyBuffer);
				}
			}
		}

		/// <summary>
		/// Saves this entity in the specified binary data destination.
		/// </summary>
		/// <param name="destination">The binary data destination, in which this entity will be saved.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>A task that represents the operation.</returns>
		public Task SaveAsync (IBinaryDestination destination, CancellationToken cancellationToken = default)
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
				await HeaderFieldBuilder.SaveHeaderAsync (header, destination, cancellationToken)
					.ConfigureAwait (false);
				await destination.WriteAsync (HeaderDecoder.CarriageReturnLinefeed, cancellationToken)
					.ConfigureAwait (false);
				foreach (var recipientBlock in this.Recipients)
				{
					var headerFields = CreateHeaderRecipient (recipientBlock);
					await HeaderFieldBuilder.SaveHeaderAsync (headerFields, destination, cancellationToken)
						.ConfigureAwait (false);
					await destination.WriteAsync (HeaderDecoder.CarriageReturnLinefeed, cancellationToken)
						.ConfigureAwait (false);
				}
			}
		}

		// Создаёт коллекцию свойств уведомления о статусе доставки сообщения конкретному адресату
		// на основе указанной коллекции полей заголовка уведомления о статусе доставки сообщения конкретному адресату.
		private static RecipientDeliveryStatus ParseHeaderRecipient (IReadOnlyCollection<EncodedHeaderField> fields, char[] fieldBodyBuffer, char[] fieldBodyElementBuffer)
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

			int unfoldedBodySize;
			foreach (var field in fields)
			{
				switch (field.Name)
				{
					case HeaderFieldName.OriginalRecipient:
						if (originalRecipient != null)
						{
							throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.OriginalRecipient) + "' field.");
						}

						unfoldedBodySize = HeaderDecoder.CopyWithUnfold (field.Body.Span, fieldBodyBuffer);
						originalRecipient = HeaderDecoder.DecodeNotificationFieldValue (fieldBodyBuffer.AsSpan (0, unfoldedBodySize), fieldBodyElementBuffer);
						break;
					case HeaderFieldName.FinalRecipient:
						if (finalRecipient != null)
						{
							throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.FinalRecipient) + "' field.");
						}

						unfoldedBodySize = HeaderDecoder.CopyWithUnfold (field.Body.Span, fieldBodyBuffer);
						finalRecipient = HeaderDecoder.DecodeNotificationFieldValue (fieldBodyBuffer.AsSpan (0, unfoldedBodySize), fieldBodyElementBuffer);
						break;
					case HeaderFieldName.Action:
						if (action != DeliveryAttemptResult.Unspecified)
						{
							throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.Action) + "' field.");
						}

						unfoldedBodySize = HeaderDecoder.CopyWithUnfold (field.Body.Span, fieldBodyBuffer);
						var isValidAction = DeliveryStatusActionHelper.TryParse (HeaderDecoder.DecodeAtom (fieldBodyBuffer.AsSpan (0, unfoldedBodySize)), out action);
						if (!isValidAction)
						{
							throw new FormatException ("Unrecognized value of Delivery Status Action.");
						}

						break;
					case HeaderFieldName.Status:
						if (status != null)
						{
							throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.Status) + "' field.");
						}

						unfoldedBodySize = HeaderDecoder.CopyWithUnfold (field.Body.Span, fieldBodyBuffer);
						status = new string (fieldBodyBuffer, 0, unfoldedBodySize).Trim ();
						break;
					case HeaderFieldName.RemoteMailTransferAgent:
						if (remoteMta != null)
						{
							throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.RemoteMailTransferAgent) + "' field.");
						}

						unfoldedBodySize = HeaderDecoder.CopyWithUnfold (field.Body.Span, fieldBodyBuffer);
						remoteMta = HeaderDecoder.DecodeNotificationFieldValue (fieldBodyBuffer.AsSpan (0, unfoldedBodySize), fieldBodyElementBuffer);
						break;
					case HeaderFieldName.DiagnosticCode:
						if (diagnosticCode != null)
						{
							throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.DiagnosticCode) + "' field.");
						}

						unfoldedBodySize = HeaderDecoder.CopyWithUnfold (field.Body.Span, fieldBodyBuffer);
						diagnosticCode = HeaderDecoder.DecodeNotificationFieldValue (fieldBodyBuffer.AsSpan (0, unfoldedBodySize), fieldBodyElementBuffer);
						break;
					case HeaderFieldName.LastAttemptDate:
						if (lastAttemptDate.HasValue)
						{
							throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.LastAttemptDate) + "' field.");
						}

						unfoldedBodySize = HeaderDecoder.CopyWithUnfold (field.Body.Span, fieldBodyBuffer);
						lastAttemptDate = InternetDateTime.Parse (fieldBodyBuffer.AsSpan (0, unfoldedBodySize));
						break;
					case HeaderFieldName.FinalLogId:
						if (finalLogId != null)
						{
							throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.FinalLogId) + "' field.");
						}

						unfoldedBodySize = HeaderDecoder.CopyWithUnfold (field.Body.Span, fieldBodyBuffer);
						finalLogId = HeaderDecoder.DecodeUnstructured (fieldBodyBuffer.AsSpan (0, unfoldedBodySize), true, fieldBodyElementBuffer);
						break;
					case HeaderFieldName.WillRetryUntil:
						if (willRetryUntil.HasValue)
						{
							throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.WillRetryUntil) + "' field.");
						}

						unfoldedBodySize = HeaderDecoder.CopyWithUnfold (field.Body.Span, fieldBodyBuffer);
						willRetryUntil = InternetDateTime.Parse (fieldBodyBuffer.AsSpan (0, unfoldedBodySize));
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
				fields.Add (new HeaderFieldBuilderAtomAndUnstructuredValue (
					HeaderFieldName.OriginalRecipient,
					recipient.OriginalRecipient.Kind.GetName (),
					recipient.OriginalRecipient.Value));
			}

			// Final-Recipient
			fields.Add (new HeaderFieldBuilderAtomAndUnstructuredValue (
				HeaderFieldName.FinalRecipient,
				recipient.FinalRecipient.Kind.GetName (),
				recipient.FinalRecipient.Value));

			// Action
			fields.Add (new HeaderFieldBuilderExactValue (
				HeaderFieldName.Action,
				recipient.Action.GetName ()));

			// Status
			var isValidInternetDomainName = AsciiCharSet.IsValidInternetDomainName (recipient.Status);
			if (!isValidInternetDomainName)
			{
				throw new FormatException ("Ivalid 'dot-atom' identificator: '" + recipient.Status + "'.");
			}

			fields.Add (new HeaderFieldBuilderExactValue (
				HeaderFieldName.Status,
				recipient.Status));

			// Remote-MTA
			if (recipient.RemoteMailTransferAgent != null)
			{
				fields.Add (new HeaderFieldBuilderAtomAndUnstructuredValue (
					HeaderFieldName.RemoteMailTransferAgent,
					recipient.RemoteMailTransferAgent.Kind.GetName (),
					recipient.RemoteMailTransferAgent.Value));
			}

			// Diagnostic-Code
			if (recipient.DiagnosticCode != null)
			{
				fields.Add (new HeaderFieldBuilderAtomAndUnstructuredValue (
					HeaderFieldName.DiagnosticCode,
					recipient.DiagnosticCode.Kind.GetName (),
					recipient.DiagnosticCode.Value));
			}

			// Last-Attempt-Date
			if (recipient.LastAttemptDate.HasValue)
			{
				fields.Add (new HeaderFieldBuilderExactValue (
					HeaderFieldName.LastAttemptDate,
					recipient.LastAttemptDate.Value.ToInternetString ()));
			}

			// Final-Log-ID
			if (recipient.FinalLogId != null)
			{
				fields.Add (new HeaderFieldBuilderUnstructuredValue (
					HeaderFieldName.FinalLogId,
					recipient.FinalLogId));
			}

			// Will-Retry-Until
			if (recipient.WillRetryUntil.HasValue)
			{
				fields.Add (new HeaderFieldBuilderExactValue (
					HeaderFieldName.WillRetryUntil,
					recipient.WillRetryUntil.Value.ToInternetString ()));
			}

			return fields;
		}

		// Создаёт коллекцию свойств уведомления о статусе доставки сообщения
		// на основе указанной коллекции полей заголовка уведомления о статусе доставки сообщения.
		private void ParseHeader (IReadOnlyCollection<EncodedHeaderField> header, char[] fieldBodyBuffer, char[] fieldBodyElementBuffer)
		{
			string originalEnvelopeId = null;
			NotificationFieldValue reportingMailTransferAgent = null;
			NotificationFieldValue gateway = null;
			NotificationFieldValue receivedFromMailTransferAgent = null;
			DateTimeOffset? arrivalDate = null;

			int unfoldedBodySize;
			foreach (var field in header)
			{
				switch (field.Name)
				{
					case HeaderFieldName.OriginalEnvelopeId:
						if (originalEnvelopeId != null)
						{
							throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.OriginalEnvelopeId) + "' field.");
						}

						unfoldedBodySize = HeaderDecoder.CopyWithUnfold (field.Body.Span, fieldBodyBuffer);
						originalEnvelopeId = HeaderDecoder.DecodeUnstructured (fieldBodyBuffer.AsSpan (0, unfoldedBodySize), true, fieldBodyElementBuffer);
						break;
					case HeaderFieldName.MailTransferAgent:
						if (reportingMailTransferAgent != null)
						{
							throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.MailTransferAgent) + "' field.");
						}

						// reporting-mta-field = "Reporting-MTA" ":" mta-name-type ";" mta-name
						// mta-name = *text
						unfoldedBodySize = HeaderDecoder.CopyWithUnfold (field.Body.Span, fieldBodyBuffer);
						reportingMailTransferAgent = HeaderDecoder.DecodeNotificationFieldValue (fieldBodyBuffer.AsSpan (0, unfoldedBodySize), fieldBodyElementBuffer);
						break;
					case HeaderFieldName.DsnGateway:
						if (gateway != null)
						{
							throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.DsnGateway) + "' field.");
						}

						unfoldedBodySize = HeaderDecoder.CopyWithUnfold (field.Body.Span, fieldBodyBuffer);
						gateway = HeaderDecoder.DecodeNotificationFieldValue (fieldBodyBuffer.AsSpan (0, unfoldedBodySize), fieldBodyElementBuffer);
						break;
					case HeaderFieldName.ReceivedFromMailTransferAgent:
						if (receivedFromMailTransferAgent != null)
						{
							throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.ReceivedFromMailTransferAgent) + "' field.");
						}

						unfoldedBodySize = HeaderDecoder.CopyWithUnfold (field.Body.Span, fieldBodyBuffer);
						receivedFromMailTransferAgent = HeaderDecoder.DecodeNotificationFieldValue (fieldBodyBuffer.AsSpan (0, unfoldedBodySize), fieldBodyElementBuffer);
						break;
					case HeaderFieldName.ArrivalDate:
						if (arrivalDate.HasValue)
						{
							throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.ArrivalDate) + "' field.");
						}

						// arrival-date-field = "Arrival-Date" ":" date-time
						unfoldedBodySize = HeaderDecoder.CopyWithUnfold (field.Body.Span, fieldBodyBuffer);
						arrivalDate = InternetDateTime.Parse (fieldBodyBuffer.AsSpan (0, unfoldedBodySize));
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
				fields.Add (new HeaderFieldBuilderUnstructuredValue (
					HeaderFieldName.OriginalEnvelopeId,
					this.OriginalEnvelopeId));
			}

			// Reporting-MTA
			if (this.MailTransferAgent != null)
			{
				fields.Add (new HeaderFieldBuilderAtomAndUnstructuredValue (
					HeaderFieldName.MailTransferAgent,
					this.MailTransferAgent.Kind.GetName (),
					this.MailTransferAgent.Value));
			}

			// DSN-Gateway
			if (this.Gateway != null)
			{
				fields.Add (new HeaderFieldBuilderAtomAndUnstructuredValue (
					HeaderFieldName.DsnGateway,
					this.Gateway.Kind.GetName (),
					this.Gateway.Value));
			}

			// Received-From-Mta
			if (this.ReceivedFromMailTransferAgent != null)
			{
				fields.Add (new HeaderFieldBuilderAtomAndUnstructuredValue (
					HeaderFieldName.ReceivedFromMailTransferAgent,
					this.ReceivedFromMailTransferAgent.Kind.GetName (),
					this.ReceivedFromMailTransferAgent.Value));
			}

			// Arrival-Date
			if (this.ArrivalDate.HasValue)
			{
				fields.Add (new HeaderFieldBuilderExactValue (
					HeaderFieldName.ArrivalDate,
					this.ArrivalDate.Value.ToInternetString ()));
			}
		}
	}
}
