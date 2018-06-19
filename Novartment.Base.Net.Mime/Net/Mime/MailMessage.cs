using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Text;
using Novartment.Base.Collections;
using Novartment.Base.Collections.Linq;
using Novartment.Base.Text;
using static System.Linq.Enumerable;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Интернет-сообщение согласно RFC 5322 с учётом RFC 2045.
	/// </summary>
	[DebuggerDisplay ("{DebuggerDisplay,nq}")]
	public class MailMessage : Entity,
		IMailMessage<AddrSpec>
	{
		// TODO: добавить валидацию при установке свойств

		// RFC 5322:
		// fields =
		//   *trace-block
		//   *(orig-date / from / sender / reply-to / to / cc / bcc / message-id / in-reply-to / references / subject / comments / keywords / optional-field)
		// </remarks>
		private AddrSpec _returnPath;

		/// <summary>
		/// Инициализирует новый экземпляр класса MailMessage в виде пустой заглушки,
		/// пригодной только для последующей загрузки из внешних источников.
		/// </summary>
		public MailMessage ()
			: base ()
		{
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса MailMessage содержащий указанное тело и имеющий указанный медиатип.
		/// </summary>
		/// <param name="body">Тело сущности, которое станет корневым телом сообщения.</param>
		/// <param name="type">Медиатип.</param>
		/// <param name="subtype">Медиа подтип.</param>
		public MailMessage (IEntityBody body, ContentMediaType type, string subtype)
			: base (body, type, subtype)
		{
		}

		/// <summary>Получает коллекцию блоков трассировки сообщения.</summary>
		[DebuggerDisplay ("{TraceDebuggerDisplay,nq}")]
		public IAdjustableList<TraceBlock> Trace { get; } = new ArrayList<TraceBlock> ();

		/// <summary>Получает адрес возврата в случае неудачи, когда невозможно доставить письмо по адресу назначения.
		/// Соответствует полю заголовка "Return-Path" определённому в RFC 2822.</summary>
		/// <remarks>
		/// Подробно описан в RFC 5321 часть 4.4 "Trace Information".
		/// </remarks>
		public AddrSpec ReturnPath => _returnPath;

		/// <summary>Получает или устанавливает версию MIME, в формате которого создано сообщение. Обычно равно "1.0".
		/// Соответствует полю заголовка "MIME-Version" определённому в RFC 2045 section 4.</summary>
		public Version MimeVersion { get; set; }

		/// <summary>Получает или устанавливает дату отправки письма.
		/// Соответствует полю заголовка "Date" определённому в RFC 2822.</summary>
		public DateTimeOffset? OriginationDate { get; set; }

		/// <summary>Получает коллекцию почтовых ящиков авторов сообщения.
		/// Соответствует полю заголовка "From" определённому в RFC 2822.</summary>
		[DebuggerDisplay ("{FromDebuggerDisplay,nq}")]
		public IAdjustableList<Mailbox> From { get; } = new ArrayList<Mailbox> ();

		/// <summary>Получает или устанавливает почтовый ящик отправителя сообщения.
		/// Соответствует полю заголовка "Sender" определённому в RFC 2822.</summary>
		public Mailbox Sender { get; set; }

		/// <summary>Получает коллекцию почтовых ящиков, на которые можно посылать ответ на сообщение.
		/// Соответствует полю заголовка "Reply-To" определённому в RFC 2822.</summary>
		[DebuggerDisplay ("{ReplyToDebuggerDisplay,nq}")]
		public IAdjustableList<Mailbox> ReplyTo { get; } = new ArrayList<Mailbox> ();

		/// <summary>Получает коллекцию почтовых ящиков получателей сообщения.
		/// Соответствует полю заголовка "To" определённому в RFC 2822.</summary>
		[DebuggerDisplay ("{ToDebuggerDisplay,nq}")]
		public IAdjustableList<Mailbox> RecipientTo { get; } = new ArrayList<Mailbox> ();

		/// <summary>Получает коллекцию почтовых ящиков получателей копии сообщения.
		/// Соответствует полю заголовка "CC" определённому в RFC 2822.</summary>
		[DebuggerDisplay ("{CcDebuggerDisplay,nq}")]
		public IAdjustableList<Mailbox> RecipientCC { get; } = new ArrayList<Mailbox> ();

		/// <summary>Получает коллекцию почтовых ящиков получателей скрытой копии сообщения.
		/// Соответствует полю заголовка "Bcc" определённому в RFC 2822.</summary>
		[DebuggerDisplay ("{BccDebuggerDisplay,nq}")]
		public IAdjustableList<Mailbox> RecipientBcc { get; } = new ArrayList<Mailbox> ();

		/// <summary>Получает или устанавливает идентификатор сообщения.
		/// Соответствует полю заголовка "Message-ID" определённому в RFC 2822.</summary>
		public AddrSpec MessageId { get; set; }

		/// <summary>Получает или устанавливает идентификатор оригинального сообщения.
		/// Соответствует полю заголовка "Original-Message-ID" определённому в RFC 3297.</summary>
		public AddrSpec OriginalMessageId { get; set; }

		/// <summary>Получает коллекцию идентификаторов сообщений, ответом на которые является сообщение.
		/// Соответствует полю заголовка "In-Reply-To" определённому в RFC 2822.</summary>
		[DebuggerDisplay ("{InReplyToDebuggerDisplay,nq}")]
		public IAdjustableList<AddrSpec> InReplyTo { get; } = new ArrayList<AddrSpec> ();

		/// <summary>Получает коллекцию идентификаторов сообщений, которые связаны с сообщением.
		/// Соответствует полю заголовка "References" определённому в RFC 2822.</summary>
		[DebuggerDisplay ("{ReferencesDebuggerDisplay,nq}")]
		public IAdjustableList<AddrSpec> References { get; } = new ArrayList<AddrSpec> ();

		/// <summary>Получает или устанавливает тему сообщения.
		/// Соответствует полю заголовка "Subject" определённому в RFC 2822.</summary>
		public string Subject { get; set; }

		/// <summary>Получает или устанавливает комментарии к сообщению.
		/// Соответствует полям заголовка "Comments" определённым в RFC 2822.</summary>
		public string Comments { get; set; }

		/// <summary>Получает коллекцию ключевых слов/фраз сообщения.
		/// Соответствует полям заголовка "Keywords" определённым в RFC 2822.</summary>
		[DebuggerDisplay ("{KeywordsDebuggerDisplay,nq}")]
		public IAdjustableList<string> Keywords { get; } = new ArrayList<string> ();

		/// <summary>Получает почтовый ящик, куда можно посылать уведомления об изменении его дислокации у получателя.
		/// Соответствует полю заголовка "Disposition-Notification-To" определённому в RFC 2298.</summary>
		[DebuggerDisplay ("{DispositionNotificationToDebuggerDisplay,nq}")]
		public IAdjustableList<Mailbox> DispositionNotificationTo { get; } = new ArrayList<Mailbox> ();

		/// <summary>Получает коллекцию параметров сообщения, определяющих доставку уведомления об изменении его дислокации у получателя.
		/// Соответствует полю заголовка "Disposition-Notification-Options" определённому в RFC 2298.</summary>
		public IAdjustableList<DispositionNotificationParameter> DispositionNotificationOptions { get; } =
			new ArrayList<DispositionNotificationParameter> ();

		/// <summary>Получает коллекцию языков, предпочитамых создателем сообщения.
		/// Соответствует полю заголовка "Accept-Language" определённому в RFC 3282.</summary>
		[DebuggerDisplay ("{AcceptLanguagesDebuggerDisplay,nq}")]
		public IAdjustableList<QualityValueParameter> AcceptLanguages { get; } = new ArrayList<QualityValueParameter> ();

		/// <summary>Получает или устанавливает список рассылки, к которому относится сообщение.
		/// Соответствует полю заголовка "List-" определённому в RFC 2369 and 2919.</summary>
		public MailingList MailingList { get; set; }

		/// <summary>Получает коллекцию почтовых ящиков авторов сообщения.</summary>
		IReadOnlyList<AddrSpec> IMailMessage<AddrSpec>.Originators
		{
			get
			{
				var count = this.From.Count;
				if (this.Sender != null)
				{
					count++;
				}

				var tempBuf = new AddrSpec[count];
				var idx = 0;
				if (this.Sender != null)
				{
					tempBuf[idx++] = this.Sender.Address;
				}

				foreach (var originator in this.From)
				{
					tempBuf[idx++] = originator.Address;
				}

				return tempBuf;
			}
		}

		/// <summary>Получает коллекцию почтовых ящиков получателей сообщения.</summary>
		IReadOnlyList<AddrSpec> IMailMessage<AddrSpec>.Recipients
		{
			get
			{
				var tempBuf = new AddrSpec[this.RecipientTo.Count + this.RecipientCC.Count + this.RecipientBcc.Count];
				var idx = 0;
				foreach (var recipient in this.RecipientTo)
				{
					tempBuf[idx++] = recipient.Address;
				}

				foreach (var recipient in this.RecipientCC)
				{
					tempBuf[idx++] = recipient.Address;
				}

				foreach (var recipient in this.RecipientBcc)
				{
					tempBuf[idx++] = recipient.Address;
				}

				return tempBuf;
			}
		}

		[DebuggerBrowsable (DebuggerBrowsableState.Never)]
		private string TraceDebuggerDisplay => (this.Trace.Count < 1) ? "<empty>" :
			(this.Trace.Count == 1) ?
				FormattableString.Invariant ($"{this.Trace[0].ReceivedTime.Value.LocalDateTime} {this.Trace[0].ReceivedParameters}") :
				FormattableString.Invariant ($"Count={this.Trace.Count}: {this.Trace[0].ReceivedTime.Value.LocalDateTime} {this.Trace[0].ReceivedParameters} ...");

		[DebuggerBrowsable (DebuggerBrowsableState.Never)]
		private string FromDebuggerDisplay => (this.From.Count < 1) ? "<empty>" :
			(this.From.Count == 1) ?
				this.From[0].ToString () :
				FormattableString.Invariant ($"Count={this.From.Count}: {this.From[0]} ...");

		[DebuggerBrowsable (DebuggerBrowsableState.Never)]
		private string ReplyToDebuggerDisplay => (this.ReplyTo.Count < 1) ? "<empty>" :
			(this.ReplyTo.Count == 1) ?
				this.ReplyTo[0].ToString () :
				FormattableString.Invariant ($"Count={this.ReplyTo.Count}: {this.ReplyTo[0]} ...");

		[DebuggerBrowsable (DebuggerBrowsableState.Never)]
		private string ToDebuggerDisplay => (this.RecipientTo.Count < 1) ? "<empty>" :
			(this.RecipientTo.Count == 1) ?
				this.RecipientTo[0].ToString () :
				FormattableString.Invariant ($"Count={this.RecipientTo.Count}: {this.RecipientTo[0]} ...");

		[DebuggerBrowsable (DebuggerBrowsableState.Never)]
		private string CcDebuggerDisplay => (this.RecipientCC.Count < 1) ? "<empty>" :
			(this.RecipientCC.Count == 1) ?
				this.RecipientCC[0].ToString () :
				FormattableString.Invariant ($"Count={this.RecipientCC.Count}: {this.RecipientCC[0]} ...");

		[DebuggerBrowsable (DebuggerBrowsableState.Never)]
		private string BccDebuggerDisplay => (this.RecipientBcc.Count < 1) ? "<empty>" :
			(this.RecipientBcc.Count == 1) ?
				this.RecipientBcc[0].ToString () :
				FormattableString.Invariant ($"Count={this.RecipientBcc.Count}: {this.RecipientBcc[0]} ...");

		[DebuggerBrowsable (DebuggerBrowsableState.Never)]
		private string InReplyToDebuggerDisplay => (this.InReplyTo.Count < 1) ? "<empty>" :
			(this.InReplyTo.Count == 1) ?
				this.InReplyTo[0].ToString () :
				FormattableString.Invariant ($"Count={this.InReplyTo.Count}: {this.InReplyTo[0]} ...");

		[DebuggerBrowsable (DebuggerBrowsableState.Never)]
		private string ReferencesDebuggerDisplay => (this.References.Count < 1) ? "<empty>" :
			(this.References.Count == 1) ?
				this.References[0].ToString () :
				FormattableString.Invariant ($"Count={this.References.Count}: {this.References[0]} ...");

		[DebuggerBrowsable (DebuggerBrowsableState.Never)]
		private string KeywordsDebuggerDisplay => (this.Keywords.Count < 1) ? "<empty>" :
			(this.Keywords.Count == 1) ?
				this.Keywords[0] :
				FormattableString.Invariant ($"Count={this.Keywords.Count}: {this.Keywords[0]} ...");

		[DebuggerBrowsable (DebuggerBrowsableState.Never)]
		private string DispositionNotificationToDebuggerDisplay => (this.DispositionNotificationTo.Count < 1) ? "<empty>" :
			(this.DispositionNotificationTo.Count == 1) ?
				this.DispositionNotificationTo[0].ToString () :
				FormattableString.Invariant ($"Count={this.DispositionNotificationTo.Count}: {this.DispositionNotificationTo[0]} ...");

		[DebuggerBrowsable (DebuggerBrowsableState.Never)]
		private string AcceptLanguagesDebuggerDisplay => (this.AcceptLanguages.Count < 1) ? "<empty>" :
			(this.AcceptLanguages.Count == 1) ?
				this.AcceptLanguages[0].ToString () :
				FormattableString.Invariant ($"Count={this.AcceptLanguages.Count}: {this.AcceptLanguages[0]} ...");

		[DebuggerBrowsable (DebuggerBrowsableState.Never)]
		private string DebuggerDisplay
		{
			get
			{
				var date = this.OriginationDate.HasValue ?
#pragma warning disable CA1305 // Specify IFormatProvider
					this.OriginationDate.Value.LocalDateTime.ToString () :
#pragma warning restore CA1305 // Specify IFormatProvider
					string.Empty;
				return FormattableString.Invariant ($"{date} From: {string.Join (", ", this.From)}, To: {string.Join (", ", this.RecipientTo)}, Subject: {this.Subject}");
			}
		}

		/// <summary>
		/// Создает простое текстовое сообщение.
		/// </summary>
		/// <param name="subtype">Тип текста согласно <url>http://www.iana.org/assignments/media-types</url>,
		/// например "plain", "xml", "html".
		/// Укажите null чтобы использовать тип по умолчанию ("plain").</param>
		/// <param name="encoding">Кодировка символов, используя которую будет создано содержимое сущности.
		/// Укажите null чтобы использовать кодировку символов по умолчанию ("utf-8").</param>
		/// <param name="transferEncoding">Кодировка передачи создаваемой сущности.
		/// Укажите ContentTransferEncoding.Unspecified чтобы использовать универсальную (возможно неоптимальную) кодировку.</param>
		/// <returns>Созданное сообщение.</returns>
		public static MailMessage CreateSimpleText (
			string subtype = null,
			Encoding encoding = null,
			ContentTransferEncoding transferEncoding = ContentTransferEncoding.Unspecified)
		{
			var body = new TextEntityBody (
				encoding ?? Encoding.UTF8,
				(transferEncoding != ContentTransferEncoding.Unspecified) ? transferEncoding : ContentTransferEncoding.Base64);
			var message = new MailMessage (body, ContentMediaType.Text, subtype ?? TextMediaSubtypeNames.Plain);
			return message;
		}

		/// <summary>
		/// Создает сложное сообщение в которое можно добавлять произвольные части (сущности).
		/// </summary>
		/// <param name="subtype">Тип сущности с множественным содержимым согласно <url>http://www.iana.org/assignments/media-types</url>,
		/// например "mixed", "alternative", "parallel".
		/// Укажите null чтобы использовать тип по умолчанию ("mixed").</param>
		/// <returns>Созданное сообщение.</returns>
		public static MailMessage CreateComposite (string subtype = null)
		{
			var body = new CompositeEntityBody ();
			var message = new MailMessage (body, ContentMediaType.Multipart, subtype ?? MultipartMediaSubtypeNames.Mixed);
			return message;
		}

		/// <summary>
		/// Создает сообщение-отчет, такое как отчет о доставке или отчет об изменении дислокации сообщения.
		/// </summary>
		/// <param name="reportType">Тип отчёта, например "delivery-status" или "disposition-notification".</param>
		/// <returns>Созданное сообщение.</returns>
		public static MailMessage CreateReport (string reportType)
		{
			var body = new ReportEntityBody (reportType, null);
			var message = new MailMessage (body, ContentMediaType.Multipart, MultipartMediaSubtypeNames.Report);
			return message;
		}

		/// <summary>
		/// Генерирует уникальный идентификатор сообщения согласно формату RFC 5322 часть 3.6.4.
		/// </summary>
		public void GenerateId ()
		{
			// RFC 5322 3.6.4.Identification Fields
			// The "Message-ID:" field provides a unique message identifier that refers to a particular version of a particular message.
			// The uniqueness of the message identifier is guaranteed by the host that generates it (see below).
			// This message identifier is intended to be machine readable and not necessarily meaningful to humans.
#pragma warning disable CA1305 // Specify IFormatProvider
			this.MessageId = new AddrSpec (Guid.NewGuid ().ToString ("N"), "global");
#pragma warning restore CA1305 // Specify IFormatProvider
		}

		/// <summary>
		/// Создает копию сообщения, содержащую только заголовок (без содержимого).
		/// Используется для вставки в отчеты о событиях с сообщением.
		/// </summary>
		/// <returns>Копия сообщения, содержащая только заголовок.</returns>
		public MailMessage CreateCopyWithoutContent ()
		{
			var blankBody = new TextEntityBody (TextEntityBody.DefaultEncoding, Entity.DefaultTransferEncoding);
			var messageCopy = new MailMessage (blankBody, ContentMediaType.Unspecified, null);

			messageCopy.Trace.AddRange (this.Trace);
			messageCopy.From.AddRange (this.From);
			messageCopy.ReplyTo.AddRange (this.ReplyTo);
			messageCopy.InReplyTo.AddRange (this.InReplyTo);
			messageCopy.References.AddRange (this.References);
			messageCopy.RecipientTo.AddRange (this.RecipientTo);
			messageCopy.RecipientCC.AddRange (this.RecipientCC);
			messageCopy.Keywords.AddRange (this.Keywords);
			messageCopy.AcceptLanguages.AddRange (this.AcceptLanguages);
			messageCopy.DispositionNotificationOptions.AddRange (this.DispositionNotificationOptions);
			messageCopy.DispositionNotificationTo.AddRange (this.DispositionNotificationTo);
			messageCopy._returnPath = _returnPath;
			messageCopy.MimeVersion = this.MimeVersion;
			messageCopy.OriginationDate = this.OriginationDate;
			messageCopy.Sender = this.Sender;
			messageCopy.MessageId = this.MessageId;
			messageCopy.OriginalMessageId = this.OriginalMessageId;
			messageCopy.Subject = this.Subject;
			messageCopy.Comments = this.Comments;
			messageCopy.MailingList = this.MailingList;
			messageCopy.ExtraFields.AddRange (this.ExtraFields);

			return messageCopy;
		}

		/// <summary>
		/// Устанавливает свойства сообщения получая их из указанного заголовка,
		/// используя только неотмеченные поля и отмечая успешно использованые.
		/// </summary>
		/// <param name="header">Коллекция полей заголовка, по которым будут установлены свойства сообщения.</param>
		protected override void LoadExtraFields (IReadOnlyList<HeaderFieldWithMark> header)
		{
			ResetProperties ();

			var trace = new ArrayList<TraceBlock> ();
			var traceBlock = new TraceBlock ();

			var mailingList = new MailingList ();

			var buffer = ArrayPool<char>.Shared.Rent (HeaderDecoder.MaximumHeaderFieldBodySize);
			try
			{
				foreach (var fieldEntry in header.Where (item => !item.IsMarked))
				{
					try
					{
						ReadOnlySpan<char> unfoldedBody;
						switch (fieldEntry.Field.Name)
						{
							// trace fields
							case HeaderFieldName.ReturnPath:
								unfoldedBody = HeaderDecoder.UnfoldFieldBody (fieldEntry.Field.Body.Span, buffer);
								fieldEntry.IsMarked = ParseReturnPathField (unfoldedBody);
								break;
							case HeaderFieldName.Received:
								unfoldedBody = HeaderDecoder.UnfoldFieldBody (fieldEntry.Field.Body.Span, buffer);
								fieldEntry.IsMarked = ParseReceivedField (unfoldedBody, trace, ref traceBlock);
								break;

							// common fields
							case HeaderFieldName.MimeVersion:
								unfoldedBody = HeaderDecoder.UnfoldFieldBody (fieldEntry.Field.Body.Span, buffer);
								fieldEntry.IsMarked = ParseMimeVersionField (unfoldedBody);
								break;
							case HeaderFieldName.Date:
								unfoldedBody = HeaderDecoder.UnfoldFieldBody (fieldEntry.Field.Body.Span, buffer);
								fieldEntry.IsMarked = ParseDateField (unfoldedBody);
								break;
							case HeaderFieldName.MessageId:
								unfoldedBody = HeaderDecoder.UnfoldFieldBody (fieldEntry.Field.Body.Span, buffer);
								fieldEntry.IsMarked = ParseMessageIdField (unfoldedBody);
								break;
							case HeaderFieldName.InReplyTo:
								unfoldedBody = HeaderDecoder.UnfoldFieldBody (fieldEntry.Field.Body.Span, buffer);
								fieldEntry.IsMarked = ParseInReplyToField (unfoldedBody);
								break;
							case HeaderFieldName.References:
								unfoldedBody = HeaderDecoder.UnfoldFieldBody (fieldEntry.Field.Body.Span, buffer);
								fieldEntry.IsMarked = ParseReferencesField (unfoldedBody);
								break;
							case HeaderFieldName.Subject:
								unfoldedBody = HeaderDecoder.UnfoldFieldBody (fieldEntry.Field.Body.Span, buffer);
								fieldEntry.IsMarked = ParseSubjectField (unfoldedBody);
								break;
							case HeaderFieldName.Comments:
								unfoldedBody = HeaderDecoder.UnfoldFieldBody (fieldEntry.Field.Body.Span, buffer);
								fieldEntry.IsMarked = ParseCommentsField (unfoldedBody);
								break;
							case HeaderFieldName.Keywords:
								unfoldedBody = HeaderDecoder.UnfoldFieldBody (fieldEntry.Field.Body.Span, buffer);
								fieldEntry.IsMarked = ParseKeywordsField (unfoldedBody);
								break;

							// optional fields
							case HeaderFieldName.DispositionNotificationTo:
								unfoldedBody = HeaderDecoder.UnfoldFieldBody (fieldEntry.Field.Body.Span, buffer);
								fieldEntry.IsMarked = ParseDispositionNotificationToField (unfoldedBody);
								break;
							case HeaderFieldName.DispositionNotificationOptions:
								unfoldedBody = HeaderDecoder.UnfoldFieldBody (fieldEntry.Field.Body.Span, buffer);
								fieldEntry.IsMarked = ParseDispositionNotificationOptionsField (unfoldedBody);
								break;
							case HeaderFieldName.AcceptLanguage:
								unfoldedBody = HeaderDecoder.UnfoldFieldBody (fieldEntry.Field.Body.Span, buffer);
								fieldEntry.IsMarked = ParseAcceptLanguageField (unfoldedBody);
								break;
							case HeaderFieldName.OriginalMessageId:
								unfoldedBody = HeaderDecoder.UnfoldFieldBody (fieldEntry.Field.Body.Span, buffer);
								ParseOriginalMessageIdField (unfoldedBody);
								break;
							default:
								unfoldedBody = HeaderDecoder.UnfoldFieldBody (fieldEntry.Field.Body.Span, buffer);
								var isMarked = ParseResentFields (fieldEntry.Field.Name, unfoldedBody, traceBlock);
								isMarked = isMarked || ParsePersonalFields (fieldEntry.Field.Name, unfoldedBody);
								isMarked = isMarked || ParseMailingListFields (fieldEntry.Field.Name, unfoldedBody, mailingList);
								fieldEntry.IsMarked = isMarked;
								break;
						}
					}
					catch (FormatException)
					{
					}
				}
			}
			finally
			{
				ArrayPool<char>.Shared.Return (buffer);
			}

			if (traceBlock.ReceivedParameters != null)
			{
				trace.Add (traceBlock);
			}

			// trace records are in reverse order
			if (trace.Count > 0)
			{
				this.Trace.AddRange (trace.Reverse ());
			}

			if (mailingList.Id != null)
			{
				this.MailingList = mailingList;
			}
		}

		/// <summary>
		/// Сохраняет свойства сущности в указанный заголовок, представленный коллекцией полей.
		/// </summary>
		/// <param name="header">Коллекция полей заголовка, в которую будут добавлены поля, представляющие свойства сущности.</param>
		protected override void SavePropertiesToHeader (IAdjustableCollection<HeaderFieldBuilder> header)
		{
			if (header == null)
			{
				throw new ArgumentNullException (nameof (header));
			}

			Contract.EndContractBlock ();

			// The origination date specifies the date and time at which the creator of the message indicated
			// that the message was complete and ready to enter the mail delivery system.
			// For instance, this might be the time that a user pushes the "send" or "submit" button in an application program.
			if (!this.OriginationDate.HasValue)
			{
				this.OriginationDate = DateTime.Now;
			}

			// проверка обязательных полей
			if (this.From.Count < 1)
			{
				throw new InvalidOperationException ("Required property 'From' not specified.");
			}

			// If the from field contains more than one mailbox specification in the mailbox-list, then the sender field,
			// containing the field name "Sender" and a single mailbox specification, MUST appear in the message.
			if ((this.From.Count > 1) && (this.Sender == null))
			{
				throw new InvalidOperationException ("Required property 'Sender' not specified. It is required when multiple authors specified in 'From' property.");
			}

			if (this.MimeVersion == null)
			{
				this.MimeVersion = new Version (1, 0);
			}

			// Return-Path, Received, Resent-*
			// эти поля не должны быть указаны при поставке письма
			// RFC 5321 part 4.4: A message-originating SMTP system SHOULD NOT send a message that already contains a Return-path header field.

			// Date
			header.Add (HeaderFieldBuilder.CreateExactValue (HeaderFieldName.Date, this.OriginationDate.Value.ToInternetString ()));

			// From, Sender, Reply-To, To, CC
			// Bcc не "светим" для соблюдения приватности
			CreateAddressFields (header, this.From, this.Sender, this.ReplyTo, this.RecipientTo, this.RecipientCC);

			// Message-ID, In-Reply-To, References, Original-Message-ID
			CreateIdentificationFields (header, this.MessageId, this.InReplyTo, this.References, this.OriginalMessageId);

			// Subject, Comments, Keywords
			CreateDescriptiveFields (header, this.Subject, this.Comments, this.Keywords);

			// MIME-Version
			if (this.MimeVersion != null)
			{
				header.Add (HeaderFieldBuilder.CreateExactValue (HeaderFieldName.MimeVersion, this.MimeVersion.ToString (2)));
			}

			// Disposition-Notification-To, Disposition-Notification-Options
			CreateDispositionNotificationFields (header, this.DispositionNotificationTo, this.DispositionNotificationOptions);

			// Accept-Language
			if (this.AcceptLanguages.Count > 0)
			{
				header.Add (HeaderFieldBuilder.CreateLanguageList (
					HeaderFieldName.AcceptLanguage,
					this.AcceptLanguages
					.OrderByDescending (item => item.Importance)
					.Select (item => item.Value))); // значения перечисляются в порядке уменьшения качества, поэтому само качество не указывается
			}

			// List-ID, List-Archive, List-Help, List-Owner, List-Post, List-Subscribe, List-Unsubscribe
			if (this.MailingList != null)
			{
				CreateMailingListFields (header, this.MailingList);
			}

			base.SavePropertiesToHeader (header);
		}

		private static bool ParseReceivedField (ReadOnlySpan<char> body, IAdjustableCollection<TraceBlock> trace, ref TraceBlock traceBlock)
		{
			if (traceBlock.ReceivedParameters != null)
			{
				trace.Add (traceBlock);
				traceBlock = new TraceBlock ();
			}

			// received       = "Received:" *received-token ";" date-time
			// received-token = word / angle-addr / addr-spec / domain
			var data = HeaderDecoder.DecodeUnstructuredAndDate (body);
			traceBlock.ReceivedParameters = data.Text.Trim ();
			traceBlock.ReceivedTime = data.Time;
			return true;
		}

		private static bool ParseResentFields (HeaderFieldName name, ReadOnlySpan<char> body, TraceBlock traceBlock)
		{
			switch (name)
			{
				case HeaderFieldName.ResentDate:
					// resent-date = "Resent-Date:" date-time
					traceBlock.ResentDate = InternetDateTime.Parse (body);
					return true;
				case HeaderFieldName.ResentFrom:
					// resent-from    =  "Resent-From:" mailbox-list
					// mailbox-list   =  (mailbox *("," mailbox))
					traceBlock.ResentFrom.AddRange (HeaderDecoder.DecodeMailboxList (body));
					return true;
				case HeaderFieldName.ResentSender:
					// resent-sender   =   "Resent-Sender:" mailbox
					traceBlock.ResentSender = Mailbox.Parse (body);
					return true;
				case HeaderFieldName.ResentTo:
					// address-list = (address *("," address))
					// address      = mailbox / group
					// group        = display-name ":" [mailbox-list / CFWS]
					// mailbox-list = (mailbox *("," mailbox))
					traceBlock.ResentTo.AddRange (HeaderDecoder.DecodeMailboxList (body));
					return true;
				case HeaderFieldName.ResentCC:
					// address-list = (address *("," address))
					// address      = mailbox / group
					// group        = display-name ":" [mailbox-list / CFWS]
					// mailbox-list = (mailbox *("," mailbox))
					traceBlock.ResentCC.AddRange (HeaderDecoder.DecodeMailboxList (body));
					return true;
				case HeaderFieldName.ResentBcc:
					// address-list = (address *("," address))
					// address      = mailbox / group
					// group        = display-name ":" [mailbox-list / CFWS]
					// mailbox-list = (mailbox *("," mailbox))
					traceBlock.ResentBcc.AddRange (HeaderDecoder.DecodeMailboxList (body));
					return true;
				case HeaderFieldName.ResentMessageId:
					// resent-msg-id   =   "Resent-Message-ID:" msg-id
					var adrs = HeaderDecoder.DecodeAddrSpecList (body);
					traceBlock.ResentMessageId = adrs.Single ();
					return true;
			}

			return false;
		}

		private static bool ParseMailingListFields (HeaderFieldName name, ReadOnlySpan<char> body, MailingList list)
		{
			switch (name)
			{
				case HeaderFieldName.ListId:
					if (list.Id != null)
					{
						throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.ListId) + "' field.");
					}

					// list-id-field    = "List-ID:" [phrase] "<" list-id ">"
					// list-id           = list-label "." list-id-namespace
					// list-label        = dot-atom-text
					// list-id-namespace = domain-name / unmanaged-list-id-namespace
					// domain-name       = dot-atom-text
					// unmanaged-list-id-namespace = "localhost"
					var data = HeaderDecoder.DecodePhraseAndId (body);
					list.Description = data.Value1;
					list.Id = data.Value2;
					return true;
				case HeaderFieldName.ListArchive:
					if (list.ArchiveCommands.Count > 0)
					{
						throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.ListArchive) + "' field.");
					}

					// A list of multiple, alternate, URLs MAY be specified by a comma-separated list of angle-bracket enclosed URLs.
					list.ArchiveCommands.AddRange (HeaderDecoder.DecodeAngleBracketedlList (body));
					return true;
				case HeaderFieldName.ListHelp:
					if (list.HelpCommands.Count > 0)
					{
						throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.ListHelp) + "' field.");
					}

					// A list of multiple, alternate, URLs MAY be specified by a comma-separated list of angle-bracket enclosed URLs.
					list.HelpCommands.AddRange (HeaderDecoder.DecodeAngleBracketedlList (body));
					return true;
				case HeaderFieldName.ListOwner:
					if (list.OwnerCommands.Count > 0)
					{
						throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.ListOwner) + "' field.");
					}

					// A list of multiple, alternate, URLs MAY be specified by a comma-separated list of angle-bracket enclosed URLs.
					list.OwnerCommands.AddRange (HeaderDecoder.DecodeAngleBracketedlList (body));
					return true;
				case HeaderFieldName.ListPost:
					if (list.PostCommands.Count > 0)
					{
						throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.ListPost) + "' field.");
					}

					// A list of multiple, alternate, URLs MAY be specified by a comma-separated list of angle-bracket enclosed URLs.
					list.PostCommands.AddRange (HeaderDecoder.DecodeAngleBracketedlList (body));
					return true;
				case HeaderFieldName.ListSubscribe:
					if (list.SubscribeCommands.Count > 0)
					{
						throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.ListSubscribe) + "' field.");
					}

					// A list of multiple, alternate, URLs MAY be specified by a comma-separated list of angle-bracket enclosed URLs.
					list.SubscribeCommands.AddRange (HeaderDecoder.DecodeAngleBracketedlList (body));
					return true;
				case HeaderFieldName.ListUnsubscribe:
					if (list.UnsubscribeCommands.Count > 0)
					{
						throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.ListUnsubscribe) + "' field.");
					}

					// A list of multiple, alternate, URLs MAY be specified by a comma-separated list of angle-bracket enclosed URLs.
					list.UnsubscribeCommands.AddRange (HeaderDecoder.DecodeAngleBracketedlList (body));
					return true;
			}

			return false;
		}

		private static void CreateDescriptiveFields (
			IAdjustableCollection<HeaderFieldBuilder> header,
			string subject,
			string comments,
			IReadOnlyList<string> keywords)
		{
			// Subject
			if (subject != null)
			{
				header.Add (HeaderFieldBuilder.CreateUnstructured (HeaderFieldName.Subject, subject));
			}

			// Comments
			if (comments != null)
			{
				header.Add (HeaderFieldBuilder.CreateUnstructured (HeaderFieldName.Comments, comments));
			}

			// Keywords
			if (keywords.Count > 0)
			{
				header.Add (HeaderFieldBuilder.CreatePhraseList (HeaderFieldName.Keywords, keywords));
			}
		}

		private static void CreateIdentificationFields (
			IAdjustableCollection<HeaderFieldBuilder> header,
			AddrSpec messageId,
			IReadOnlyCollection<AddrSpec> inReplyTo,
			IReadOnlyCollection<AddrSpec> references,
			AddrSpec originalMessageId)
		{
			// Message-ID
			if (messageId != null)
			{
				header.Add (HeaderFieldBuilder.CreateExactValue (
					HeaderFieldName.MessageId,
					messageId.ToAngleString ()));
			}

			// In-Reply-To
			if (inReplyTo.Count > 0)
			{
				header.Add (HeaderFieldBuilder.CreateAddrSpecList (
					HeaderFieldName.InReplyTo,
					inReplyTo.WhereNotNull ().ToArray ().AsReadOnlyList ()));
			}

			// References
			if (references.Count > 0)
			{
				header.Add (HeaderFieldBuilder.CreateAddrSpecList (
					HeaderFieldName.References,
					references.WhereNotNull ().ToArray ().AsReadOnlyList ()));
			}

			// Original-Message-ID
			if (originalMessageId != null)
			{
				header.Add (HeaderFieldBuilder.CreateExactValue (
					HeaderFieldName.OriginalMessageId,
					originalMessageId.ToAngleString ()));
			}
		}

		private static void CreateAddressFields (
			IAdjustableCollection<HeaderFieldBuilder> header,
			IReadOnlyList<Mailbox> from,
			Mailbox sender,
			IReadOnlyList<Mailbox> replyTo,
			IReadOnlyList<Mailbox> to,
			IReadOnlyList<Mailbox> cc)
		{
			// From
			header.Add (HeaderFieldBuilder.CreateMailboxList (HeaderFieldName.From, from));

			// Sender
			// If the originator of the message can be indicated by a single mailbox and the author and transmitter are identical,
			// the "Sender:" field SHOULD NOT be used.
			var isSenderRequired = (sender != null) && ((from.Count != 1) || !from[0].Equals (sender));
			if (isSenderRequired)
			{
				header.Add (HeaderFieldBuilder.CreateMailbox (HeaderFieldName.Sender, sender));
			}

			// Reply-To
			if (replyTo.Count > 0)
			{
				header.Add (HeaderFieldBuilder.CreateMailboxList (HeaderFieldName.ReplyTo, replyTo));
			}

			// To
			if (to.Count > 0)
			{
				header.Add (HeaderFieldBuilder.CreateMailboxList (HeaderFieldName.To, to));
			}

			// CC
			if (cc.Count > 0)
			{
				header.Add (HeaderFieldBuilder.CreateMailboxList (HeaderFieldName.CC, cc));
			}
		}

		private static void CreateDispositionNotificationFields (
			IAdjustableCollection<HeaderFieldBuilder> header,
			IReadOnlyList<Mailbox> dispositionNotificationTo,
			IReadOnlyList<DispositionNotificationParameter> dispositionNotificationOptions)
		{
			// Disposition-Notification-To
			if (dispositionNotificationTo.Count > 0)
			{
				header.Add (HeaderFieldBuilder.CreateMailboxList (
					HeaderFieldName.DispositionNotificationTo,
					dispositionNotificationTo));
			}

			// Disposition-Notification-Options
			if (dispositionNotificationOptions.Count > 0)
			{
				header.Add (HeaderFieldBuilder.CreateDispositionNotificationParameterList (
					HeaderFieldName.DispositionNotificationOptions,
					dispositionNotificationOptions));
			}
		}

		private static void CreateMailingListFields (
			IAdjustableCollection<HeaderFieldBuilder> header,
			MailingList mailingList)
		{
			// List-ID
			header.Add (HeaderFieldBuilder.CreatePhraseAndId (
				HeaderFieldName.ListId,
				mailingList.Id,
				mailingList.Description));

			// List-Archive
			if (mailingList.ArchiveCommands.Count > 0)
			{
				header.Add (HeaderFieldBuilder.CreateAngleBracketedList (
					HeaderFieldName.ListArchive,
					mailingList.ArchiveCommands));
			}

			// List-Help
			if (mailingList.HelpCommands.Count > 0)
			{
				header.Add (HeaderFieldBuilder.CreateAngleBracketedList (
					HeaderFieldName.ListHelp,
					mailingList.HelpCommands));
			}

			// List-Owner
			if (mailingList.OwnerCommands.Count > 0)
			{
				header.Add (HeaderFieldBuilder.CreateAngleBracketedList (
					HeaderFieldName.ListOwner,
					mailingList.OwnerCommands));
			}

			// List-Post
			if (mailingList.PostCommands.Count > 0)
			{
				header.Add (HeaderFieldBuilder.CreateAngleBracketedList (
					HeaderFieldName.ListPost,
					mailingList.PostCommands));
			}

			// List-Subscribe
			if (mailingList.SubscribeCommands.Count > 0)
			{
				header.Add (HeaderFieldBuilder.CreateAngleBracketedList (
					HeaderFieldName.ListSubscribe,
					mailingList.SubscribeCommands));
			}

			// List-Unsubscribe
			if (mailingList.UnsubscribeCommands.Count > 0)
			{
				header.Add (HeaderFieldBuilder.CreateAngleBracketedList (
					HeaderFieldName.ListUnsubscribe,
					mailingList.UnsubscribeCommands));
			}
		}

		// очищаем все свойства
		private void ResetProperties ()
		{
			this.Trace.Clear ();
			_returnPath = null;
			this.MimeVersion = null;
			this.OriginationDate = null;
			this.From.Clear ();
			this.Sender = null;
			this.ReplyTo.Clear ();
			this.RecipientTo.Clear ();
			this.RecipientCC.Clear ();
			this.RecipientBcc.Clear ();
			this.MessageId = null;
			this.OriginalMessageId = null;
			this.InReplyTo.Clear ();
			this.References.Clear ();
			this.Subject = null;
			this.Comments = null;
			this.Keywords.Clear ();
			this.DispositionNotificationTo.Clear ();
			this.DispositionNotificationOptions.Clear ();
			this.AcceptLanguages.Clear ();
			this.MailingList = null;
		}

		private bool ParseReturnPathField (ReadOnlySpan<char> body)
		{
			if (_returnPath != null)
			{
				throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.ReturnPath) + "' field.");
			}

			// return = "Return-Path:" path
			// path   = angle-addr / ([CFWS] "<" [CFWS] ">" [CFWS])
			var adrs = HeaderDecoder.DecodeAddrSpecList (body, true);
			_returnPath = adrs.SingleOrDefault ();
			return true;
		}

		private bool ParseMimeVersionField (ReadOnlySpan<char> body)
		{
			if (this.MimeVersion != null)
			{
				throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.MimeVersion) + "' field.");
			}

			// version := "MIME-Version" ":" 1*DIGIT "." 1*DIGIT
			this.MimeVersion = HeaderDecoder.DecodeVersion (body);
			return true;
		}

		private bool ParseDateField (ReadOnlySpan<char> body)
		{
			if (this.OriginationDate.HasValue)
			{
				throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.Date) + "' field.");
			}

			// resent-date     =   "Resent-Date:" date-time
			this.OriginationDate = InternetDateTime.Parse (body);
			return true;
		}

		private bool ParsePersonalFields (HeaderFieldName name, ReadOnlySpan<char> body)
		{
			switch (name)
			{
				case HeaderFieldName.From:
					if (this.From.Count > 0)
					{
						throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.From) + "' field.");
					}

					// from         = "From:" mailbox-list
					// mailbox-list = (mailbox *("," mailbox))
					this.From.AddRange (HeaderDecoder.DecodeMailboxList (body));
					return true;
				case HeaderFieldName.Sender:
					if (this.Sender != null)
					{
						throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.Sender) + "' field.");
					}

					// sender = "Sender:" mailbox
					this.Sender = Mailbox.Parse (body);
					return true;
				case HeaderFieldName.ReplyTo:
					if (this.ReplyTo.Count > 0)
					{
						throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.ReplyTo) + "' field.");
					}

					// address-list = (address *("," address))
					// address      = mailbox / group
					// group        = display-name ":" [mailbox-list / CFWS]
					// mailbox-list = (mailbox *("," mailbox))
					this.ReplyTo.AddRange (HeaderDecoder.DecodeMailboxList (body));
					return true;
				case HeaderFieldName.To:
					if (this.RecipientTo.Count > 0)
					{
						throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.To) + "' field.");
					}

					// address-list = (address *("," address))
					// address      = mailbox / group
					// group        = display-name ":" [mailbox-list / CFWS]
					// mailbox-list = (mailbox *("," mailbox))
					this.RecipientTo.AddRange (HeaderDecoder.DecodeMailboxList (body));
					return true;
				case HeaderFieldName.CC:
					if (this.RecipientCC.Count > 0)
					{
						throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.CC) + "' field.");
					}

					// address-list = (address *("," address))
					// address      = mailbox / group
					// group        = display-name ":" [mailbox-list / CFWS]
					// mailbox-list = (mailbox *("," mailbox))
					this.RecipientCC.AddRange (HeaderDecoder.DecodeMailboxList (body));
					return true;
				case HeaderFieldName.Bcc:
					if (this.RecipientBcc.Count > 0)
					{
						throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.Bcc) + "' field.");
					}

					// address-list = (address *("," address))
					// address      = mailbox / group
					// group        = display-name ":" [mailbox-list / CFWS]
					// mailbox-list = (mailbox *("," mailbox))
					this.RecipientBcc.AddRange (HeaderDecoder.DecodeMailboxList (body));
					return true;
			}

			return false;
		}

		private bool ParseMessageIdField (ReadOnlySpan<char> body)
		{
			if (this.MessageId != null)
			{
				throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.MessageId) + "' field.");
			}

			// message-id = "Message-ID:" msg-id
			var adrs = HeaderDecoder.DecodeAddrSpecList (body);
			this.MessageId = adrs.Single ();
			return true;
		}

		private bool ParseInReplyToField (ReadOnlySpan<char> body)
		{
			if (this.InReplyTo.Count > 0)
			{
				throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.InReplyTo) + "' field.");
			}

			// in-reply-to = "In-Reply-To:" 1*msg-id
			var addrs = HeaderDecoder.DecodeAddrSpecList (body);
			this.InReplyTo.AddRange (addrs);
			return true;
		}

		private bool ParseReferencesField (ReadOnlySpan<char> body)
		{
			if (this.References.Count > 0)
			{
				throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.References) + "' field.");
			}

			// references = "References:" 1*msg-id
			var addrs = HeaderDecoder.DecodeAddrSpecList (body);
			this.References.AddRange (addrs);
			return true;
		}

		private bool ParseSubjectField (ReadOnlySpan<char> body)
		{
			if (this.Subject != null)
			{
				throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.Subject) + "' field.");
			}

			// subject = "Subject:" unstructured
			this.Subject = HeaderDecoder.DecodeUnstructured (body).Trim ();
			return true;
		}

		private bool ParseCommentsField (ReadOnlySpan<char> body)
		{
			var prevComments = (this.Comments != null) ? (this.Comments + "\r\n") : string.Empty;

			// comments = "Comments:" unstructured
			this.Comments = prevComments + HeaderDecoder.DecodeUnstructured (body).Trim ();
			return true;
		}

		private bool ParseKeywordsField (ReadOnlySpan<char> body)
		{
			// keywords = "Keywords:" phrase *("," phrase)
			this.Keywords.AddRange (HeaderDecoder.DecodePhraseList (body));
			return true;
		}

		private bool ParseDispositionNotificationToField (ReadOnlySpan<char> body)
		{
			if (this.DispositionNotificationTo.Count > 0)
			{
				throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.DispositionNotificationTo) + "' field.");
			}

			// mdn-request-field = "Disposition-Notification-To" ":" mailbox *("," mailbox)
			this.DispositionNotificationTo.AddRange (HeaderDecoder.DecodeMailboxList (body));
			return true;
		}

		private bool ParseDispositionNotificationOptionsField (ReadOnlySpan<char> body)
		{
			if (this.DispositionNotificationOptions.Count > 0)
			{
				throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.DispositionNotificationOptions) + "' field.");
			}

			// disposition-notification-parameters = parameter *(";" parameter)
			// parameter                           = attribute "=" importance "," 1#value
			// importance                          = "required" / "optional"
			this.DispositionNotificationOptions.AddRange (HeaderDecoder.DecodeDispositionNotificationParameterList (body));
			return true;
		}

		private bool ParseAcceptLanguageField (ReadOnlySpan<char> body)
		{
			if (this.AcceptLanguages.Count > 0)
			{
				throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.AcceptLanguage) + "' field.");
			}

			// Accept-Language = "Accept-Language:" [CFWS] language-q *( "," [CFWS] language-q )
			// language-q      = language-range [";" [CFWS] "q=" qvalue ] [CFWS]
			// value           = ( "0" [ "." 0*3DIGIT ] ) / ( "1" [ "." 0*3("0") ] )
			this.AcceptLanguages.AddRange (HeaderDecoder.DecodeQualityValueParameterList (body));
			return true;
		}

		private bool ParseOriginalMessageIdField (ReadOnlySpan<char> body)
		{
			if (this.OriginalMessageId != null)
			{
				throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.OriginalMessageId) + "' field.");
			}

			// "Original-Message-ID" ":" msg-id
			var adrs = HeaderDecoder.DecodeAddrSpecList (body);
			this.OriginalMessageId = adrs.Single ();
			return true;
		}
	}
}