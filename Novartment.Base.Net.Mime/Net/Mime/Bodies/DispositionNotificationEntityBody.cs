using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Collections;
using static System.Linq.Enumerable;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Тело MIME-сущности, содержащее уведомление об изменении дислокации сообщения.
	/// Определено в RFC 3798.
	/// </summary>
	public class DispositionNotificationEntityBody :
		IEntityBody
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса DispositionNotificationEntityBody.
		/// </summary>
		public DispositionNotificationEntityBody ()
		{
		}

		/// <summary>Получает кодировку передачи содержимого тела сущности.</summary>
		public ContentTransferEncoding TransferEncoding => ContentTransferEncoding.Unspecified;

		/// <summary>Gets or sets the DNS name of the particular instance of the MUA that generated the MDN.</summary>
		public string ReportingUserAgentName { get; set; }

		/// <summary>Gets or sets name of the product that generated the MDN.</summary>
		public string ReportingUserAgentProduct { get; set; }

		/// <summary>Gets or sets name of the gateway or MTA that translated a foreign (non-Internet) message disposition notification into this MDN.</summary>
		public NotificationFieldValue Gateway { get; set; }

		/// <summary>Gets or sets original recipient address as specified by the sender of the message for which the MDN is being issued.</summary>
		public NotificationFieldValue OriginalRecipient { get; set; }

		/// <summary>Gets or sets recipient for which the MDN is being issued.</summary>
		public NotificationFieldValue FinalRecipient { get; set; }

		/// <summary>Gets or sets Message-ID of the message for which the MDN is being issued.</summary>
		public AddrSpec OriginalMessageId { get; set; }

		/// <summary>Gets or sets action performed by the Reporting-MUA on behalf of the user.</summary>
		public MessageDispositionChangedAction Disposition { get; set; }

		/// <summary>Gets collection of additional disposition modifiers.</summary>
		public IAdjustableList<string> DispositionModifiers { get; } = new ArrayList<string> ();

		/// <summary>Gets or sets failure additional information.</summary>
		public IAdjustableList<string> FailureInfo { get; } = new ArrayList<string> ();

		/// <summary>Gets or sets error additional information.</summary>
		public IAdjustableList<string> ErrorInfo { get; } = new ArrayList<string> ();

		/// <summary>Gets or sets warning additional information.</summary>
		public IAdjustableList<string> WarningInfo { get; } = new ArrayList<string> ();

		/// <summary>
		/// Очищает тело сущности.
		/// </summary>
		public void Clear ()
		{
			this.DispositionModifiers.Clear ();
			this.FailureInfo.Clear ();
			this.ErrorInfo.Clear ();
			this.WarningInfo.Clear ();
			ReportingUserAgentName = null;
			ReportingUserAgentProduct = null;
			Gateway = null;
			OriginalRecipient = null;
			FinalRecipient = null;
			OriginalMessageId = null;
			Disposition = MessageDispositionChangedAction.Unspecified;
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

			var headerSource = new TemplateSeparatedBufferedSource (source, HeaderDecoder.CarriageReturnLinefeed2, false);
			var task = HeaderDecoder.LoadHeaderFieldsAsync (headerSource, cancellationToken);

			return LoadAsyncFinalizer ();

			async Task LoadAsyncFinalizer ()
			{
				var fields = await task.ConfigureAwait (false);
				ParseHeader (fields);
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

			if (this.FinalRecipient == null)
			{
				throw new InvalidOperationException ("Required property 'FinalRecipient' not specified.");
			}

			if (this.Disposition == MessageDispositionChangedAction.Unspecified)
			{
				throw new InvalidOperationException ("Required property 'Disposition' not specified.");
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
			}
		}

		// Создаёт коллекцию свойств уведомления об изменении дислокации сообщения
		// на основе указанной коллекции полей заголовка уведомления об изменении дислокации сообщения.
		private void ParseHeader (IReadOnlyCollection<HeaderField> fields)
		{
			/*
			disposition-notification-content =
				[ reporting-ua-field TargetsTrackingExtensionBase ]
				[ mdn-gateway-field TargetsTrackingExtensionBase ]
				[ original-recipient-field TargetsTrackingExtensionBase ]
				final-recipient-field TargetsTrackingExtensionBase
				[ original-message-id-field TargetsTrackingExtensionBase ]
				disposition-field TargetsTrackingExtensionBase
				*( failure-field TargetsTrackingExtensionBase )
				*( error-field TargetsTrackingExtensionBase )
				*( warning-field TargetsTrackingExtensionBase )
				*( extension-field TargetsTrackingExtensionBase
			reporting-ua-field =
				"Reporting-UA" ":" ua-name [ ";" ua-product ]
			disposition-field =
				"Disposition" ":" disposition-mode ";" disposition-type [ "/" disposition-modifier *( "," disposition-modifier ) ]
			disposition-mode =
				action-mode "/" sending-mode
			action-mode =
				"manual-action" / "automatic-action"
			sending-mode =
				"MDN-sent-manually" / "MDN-sent-automatically"
			disposition-type =
				"displayed" / "deleted"
			*/

			foreach (var field in fields)
			{
				switch (field.Name)
				{
					case HeaderFieldName.ReportingUA:
						ParseReportingUAField (field.Value);
						break;
					case HeaderFieldName.MdnGateway:
						if (this.Gateway != null)
						{
							throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.MdnGateway) + "' field.");
						}

						this.Gateway = HeaderDecoder.DecodeNotificationFieldValue (field.Value);
						break;
					case HeaderFieldName.OriginalRecipient:
						if (this.OriginalRecipient != null)
						{
							throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.OriginalRecipient) + "' field.");
						}

						this.OriginalRecipient = HeaderDecoder.DecodeNotificationFieldValue (field.Value);
						break;
					case HeaderFieldName.FinalRecipient:
						if (this.FinalRecipient != null)
						{
							throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.FinalRecipient) + "' field.");
						}

						this.FinalRecipient = HeaderDecoder.DecodeNotificationFieldValue (field.Value);
						break;
					case HeaderFieldName.OriginalMessageId:
						if (this.OriginalMessageId != null)
						{
							throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.OriginalMessageId) + "' field.");
						}

						this.OriginalMessageId = HeaderDecoder.DecodeAddrSpecList (field.Value).Single ();
						break;
					case HeaderFieldName.Disposition:
						ParseDispositionField (field.Value);
						break;
					case HeaderFieldName.Failure:
						this.FailureInfo.Add (HeaderDecoder.DecodeUnstructured (field.Value).Trim ());
						break;
					case HeaderFieldName.Error:
						this.ErrorInfo.Add (HeaderDecoder.DecodeUnstructured (field.Value).Trim ());
						break;
					case HeaderFieldName.Warning:
						this.WarningInfo.Add (HeaderDecoder.DecodeUnstructured (field.Value).Trim ());
						break;
				}
			}

			if (this.FinalRecipient == null)
			{
				throw new FormatException ("Required field '" + HeaderFieldNameHelper.GetName (HeaderFieldName.FinalRecipient) + "' not specified.");
			}

			if (this.Disposition == MessageDispositionChangedAction.Unspecified)
			{
				throw new FormatException ("Not specified valid value for property 'Disposition'.");
			}
		}

		private void ParseReportingUAField (string value)
		{
			if (this.ReportingUserAgentName != null)
			{
				throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.ReportingUA) + "' field.");
			}

			var data = HeaderDecoder.DecodeUnstructuredPair (value);
			this.ReportingUserAgentName = data.Value1.Trim ();
			var isReportingUserAgentProductEmpty = string.IsNullOrWhiteSpace (data.Value2);
			if (!isReportingUserAgentProductEmpty)
			{
				this.ReportingUserAgentProduct = data.Value2.Trim ();
			}
		}

		private void ParseDispositionField (string value)
		{
			if (this.Disposition != MessageDispositionChangedAction.Unspecified)
			{
				throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.Disposition) + "' field.");
			}

			// disposition-field = "Disposition" ":" disposition-mode ";" disposition-type [ "/" disposition-modifier *( "," disposition-modifier ) ]
			// disposition-mode = action-mode "/" sending-mode
			// action-mode = "manual-action" / "automatic-action"
			// sending-mode = "MDN-sent-manually" / "MDN-sent-automatically"
			// disposition-type = "displayed" / "deleted"
			var data = HeaderDecoder.DecodeDispositionAction (value);
			var isManualAction = MdnActionModeNames.ManualAction.Equals (data.Value1, StringComparison.OrdinalIgnoreCase);

			// var isSendManually = MdnSendingModeNames.MdnSentManually.Equals (data.Item2, StringComparison.OrdinalIgnoreCase);
			var isDiplayed = MdnDispositionTypeNames.Displayed.Equals (data.Value3, StringComparison.OrdinalIgnoreCase);
			if (isManualAction)
			{
				this.Disposition = isDiplayed ?
					MessageDispositionChangedAction.ManuallyDisplayed :
					MessageDispositionChangedAction.ManuallyDeleted;
			}
			else
			{
				this.Disposition = isDiplayed ?
					MessageDispositionChangedAction.AutomaticallyDisplayed :
					MessageDispositionChangedAction.AutomaticallyDeleted;
			}

			if (data.List.Count > 0)
			{
				this.DispositionModifiers.AddRange (data.List);
			}
		}

		// Создаёт поля заголовка содержащую все свойства коллекции.
		private void CreateHeader (IAdjustableCollection<HeaderFieldBuilder> header)
		{
			// Reporting-UA
			if (this.ReportingUserAgentName != null)
			{
				header.Add (HeaderFieldBuilder.CreateUnstructuredPair (
					HeaderFieldName.ReportingUA,
					this.ReportingUserAgentName,
					this.ReportingUserAgentProduct));
			}

			// MDN-Gateway
			if (this.Gateway != null)
			{
				header.Add (HeaderFieldBuilder.CreateAtomAndUnstructured (
					HeaderFieldName.MdnGateway,
					this.Gateway.Kind.GetName (),
					this.Gateway.Value));
			}

			// Original-Recipient
			if (this.OriginalRecipient != null)
			{
				header.Add (HeaderFieldBuilder.CreateAtomAndUnstructured (
					HeaderFieldName.OriginalRecipient,
					this.OriginalRecipient.Kind.GetName (),
					this.OriginalRecipient.Value));
			}

			// Final-Recipient
			header.Add (HeaderFieldBuilder.CreateAtomAndUnstructured (
				HeaderFieldName.FinalRecipient,
				this.FinalRecipient.Kind.GetName (),
				this.FinalRecipient.Value));

			// Original-Message-ID
			if (this.OriginalMessageId != null)
			{
				header.Add (HeaderFieldBuilder.CreateExactValue (
					HeaderFieldName.OriginalMessageId,
					this.OriginalMessageId.ToAngleString ()));
			}

			// Disposition
			var actionMode = (this.Disposition == MessageDispositionChangedAction.ManuallyDeleted) || (this.Disposition == MessageDispositionChangedAction.ManuallyDisplayed) ?
				MdnActionModeNames.ManualAction : MdnActionModeNames.AutomaticAction;
			var sendingMode = MdnSendingModeNames.MdnSentAutomatically;
			var dispositionType = (this.Disposition == MessageDispositionChangedAction.ManuallyDisplayed) || (this.Disposition == MessageDispositionChangedAction.AutomaticallyDisplayed) ?
				MdnDispositionTypeNames.Displayed : MdnDispositionTypeNames.Deleted;
			header.Add (HeaderFieldBuilder.CreateDisposition (
				HeaderFieldName.Disposition,
				actionMode,
				sendingMode,
				dispositionType,
				this.DispositionModifiers));

			// Failure
			foreach (var info in this.FailureInfo)
			{
				header.Add (HeaderFieldBuilder.CreateUnstructured (HeaderFieldName.Failure, info));
			}

			// Error
			foreach (var info in this.ErrorInfo)
			{
				header.Add (HeaderFieldBuilder.CreateUnstructured (HeaderFieldName.Error, info));
			}

			// Warning
			foreach (var info in this.WarningInfo)
			{
				header.Add (HeaderFieldBuilder.CreateUnstructured (HeaderFieldName.Warning, info));
			}
		}
	}
}
