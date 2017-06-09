using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Collections;
using Novartment.Base.Text;
using static System.Linq.Enumerable;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// MIME-сущность согласно RFC 2045.
	/// </summary>
	[DebuggerDisplay ("{DebuggerDisplay,nq}")]
	public class Entity :
		IBinarySerializable
	{
		// TODO: добавить валидацию при установке свойств
		/*
		RFC 2045:
			entity-headers := [ content CarriageReturnLinefeed ] [ encoding CarriageReturnLinefeed ] [ id CarriageReturnLinefeed ] [ description CarriageReturnLinefeed ] *( MIME-extension-field CarriageReturnLinefeed )
			content := "Content-Type" ":" type "/" subtype *(";" parameter)
			encoding := "Content-Transfer-Encoding" ":" mechanism
			id := "Content-ID" ":" msg-id
			description := "Content-Description" ":" *text
			MIME-extension-field := <Any RFC 822 header field which begins with the string "Content-">;
		*/

		/// <summary>
		/// Медиатип содержимого по умолчанию.
		/// </summary>
		public static readonly ContentMediaType DefaultType = ContentMediaType.Text;

		/// <summary>
		/// Медиа подтип содержимого по умолчанию.
		/// </summary>
		public static readonly string DefaultSubtype = TextMediaSubtypeNames.Plain;

		/// <summary>
		/// Кодировка передачи содержимого по умолчанию.
		/// </summary>
		public static readonly ContentTransferEncoding DefaultTransferEncoding = ContentTransferEncoding.SevenBit;

		private IEntityBody _body;
		private ContentMediaType _type;
		private string _subtype;
		private ContentDispositionType _dispositionType;
		private byte[] _md5;
		private TimeSpan? _duration;

		/// <summary>
		/// Инициализирует новый экземпляр класса Entity в виде пустой заглушки,
		/// пригодной только для последующей загрузки из внешних источников.
		/// </summary>
		[SuppressMessage (
			"Microsoft.Design",
			"CA1026:DefaultParametersShouldNotBeUsed",
			Justification = "Parameters have clear right 'default' values and there is no plausible reason why the default might need to change.")]
		public Entity ()
		{
			_type = ContentMediaType.Unspecified;
			_subtype = null;
			_body = null;
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса Entity содержащий указанное тело и имеющий указанный медиатип.
		/// </summary>
		/// <param name="body">Тело.</param>
		/// <param name="type">Медиатип.</param>
		/// <param name="subtype">Медиа подтип.</param>
		public Entity (IEntityBody body, ContentMediaType type, string subtype)
		{
			if (body == null)
			{
				throw new ArgumentNullException (nameof (body));
			}

			if (type == ContentMediaType.Unspecified)
			{
				throw new ArgumentOutOfRangeException (nameof (type));
			}

			if (subtype == null)
			{
				throw new ArgumentNullException (nameof (subtype));
			}

			Contract.EndContractBlock ();

			_type = type;
			_subtype = subtype;
			_body = body;
		}

		/// <summary>Получает тело сущности. Возвращает null если тело ещ­­­ё не создано.</summary>
		public IEntityBody Body => _body;

		/// <summary>Получает или устанавливает медиатип содержимого. Возвращает null если тело сущности ещ­­­ё не создано.
		/// Соответствует значению поля заголовка "Content-Type" определённому в RFC 2045 часть 5.</summary>
		public ContentMediaType Type => _type;

		/// <summary>Получает или устанавливает медиа подтип содержимого. Возвращает null если тело сущности ещ­­­ё не создано.
		/// Соответствует значению поля заголовка "Content-Type" определённому в RFC 2045 часть 5.</summary>
		public string Subtype => _subtype;

		/// <summary>Получает или устанавливает расположение в котором предназначено находится содержимое.
		/// Соответствует значению поля заголовка "Content-Disposition" определённому в RFC 2183.</summary>
		public ContentDispositionType DispositionType
		{
			get => _dispositionType;
			set
			{
				_dispositionType = value;
			}
		}

		/// <summary>Получает или устанавливает рекомендуемое имя файла для случая если содержимое будет сохраняться в отдельном файле.
		/// Соответствует параметру "filename" поля заголовка "Content-Disposition" определённому в RFC 2183.</summary>
		public string FileName { get; set; }

		/// <summary>Получает или устанавливает дату последнего изменения содержимого.
		/// Соответствует параметру "modification-date" поля заголовка "Content-Disposition" определённому в RFC 2183.</summary>
		public DateTimeOffset? ModificationDate { get; set; }

		/// <summary>Получает или устанавливает дату создания содержимого.
		/// Соответствует параметру "creation-date" поля заголовка "Content-Disposition" определённому в RFC 2183.</summary>
		public DateTimeOffset? CreationDate { get; set; }

		/// <summary>Получает или устанавливает дату последнего доступа к содержимому.
		/// Соответствует параметру "read-date" поля заголовка "Content-Disposition" определённому в RFC 2183.</summary>
		public DateTimeOffset? ReadDate { get; set; }

		/// <summary>Получает или устанавливает приблизительный размер содержимого в байтах.
		/// Соответствует параметру "size" поля заголовка "Content-Disposition" определённому в RFC 2183.</summary>
		public long? Size { get; set; }

		/// <summary>Получает или устанавливает идентификатор содержимого.
		/// Соответствует полю заголовка "Content-ID" определённому в RFC 2045 часть 7.</summary>
		public AddrSpec Id { get; set; }

		/// <summary>Получает или устанавливает описание содержимого.
		/// Соответствует полю заголовка "Content-Description" определённому в RFC 2045 часть 8.</summary>
		public string Description { get; set; }

		/// <summary>Получает или устанавливает кодировку передачи содержимого.
		/// Соответствует полю заголовка "Content-Transfer-Encoding" определённому в RFC 2045 часть 6.</summary>
		public ContentTransferEncoding TransferEncoding => _body?.TransferEncoding ?? ContentTransferEncoding.Unspecified;

		/// <summary>Получает или устанавливает базовый URI для использования с относительными URI, заданными в содержимом.
		/// Соответствует полю заголовка "Content-Base" определённому в RFC 2110 часть 4.2.</summary>
		public string Base { get; set; }

		/// <summary>Получает или устанавливает URI для получения содержимого.
		/// Соответствует полю заголовка "Content-Location" определённому в RFC 2557 часть 4.2.</summary>
		public string Location { get; set; }

		/// <summary>Получает коллекцию языков, которые используются в содержимом.
		/// Соответствует полю заголовка "Content-Language" определённому в RFC 3282.</summary>
		[DebuggerDisplay ("{LanguagesDebuggerDisplay,nq}")]
		public IAdjustableList<string> Languages { get; } = new ArrayList<string> ();

		/// <summary>Получает или устанавливает медийные характеристики содержимого.
		/// Соответствует полю заголовка "Content-features" определённому в RFC 2912 часть 3.</summary>
		public string Features { get; set; }

		/// <summary>Получает коллекцию идентификаторов доступных в сообщении альтернатив содержимому.
		/// Соответствует полю заголовка "Content-Alternative" определённому в RFC 3297 часть 4.</summary>
		[DebuggerDisplay ("{AlternativesDebuggerDisplay,nq}")]
		public IAdjustableList<string> Alternatives { get; } = new ArrayList<string> ();

		/// <summary>Получает или устанавливает MD5-хэш содержимого.
		/// Соответствует полю заголовка "Content-MD5" определённому в RFC 1864.</summary>
		public byte[] MD5
		{
			get => _md5;
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException (nameof (value));
				}

				if (value.Length != 16)
				{
					throw new ArgumentOutOfRangeException (nameof (value));
				}

				_md5 = value;
			}
		}

		/// <summary>Получает или устанавливает продолжительность содержимого.
		/// Соответствует полю заголовка "Content-Duration" определённому в RFC 2424.</summary>
		public TimeSpan? Duration
		{
			get => _duration;
			set
			{
				if (value.HasValue && (value.Value.Ticks < 0))
				{
					throw new ArgumentOutOfRangeException (nameof (value));
				}

				_duration = value;
			}
		}

		/// <summary>
		/// Получает коллекцию полей, содержащих дополнительные параметры сущности, значение которых не отражено в свойствах.
		/// </summary>
		[DebuggerDisplay ("{ExtraFieldsDebuggerDisplay,nq}")]
		public IAdjustableList<HeaderField> ExtraFields { get; } = new ArrayList<HeaderField> ();

		[SuppressMessage (
			"Microsoft.Performance",
			"CA1811:AvoidUncalledPrivateCode",
			Justification = "Used in DebuggerDisplay attribute.")]
		[DebuggerBrowsable (DebuggerBrowsableState.Never)]
		private string LanguagesDebuggerDisplay => (this.Languages.Count < 1) ? "<empty>" :
			(this.Languages.Count == 1) ?
				this.Languages[0] :
				FormattableString.Invariant ($"Count={this.Languages.Count}: {this.Languages[0]} ...");

		[SuppressMessage (
			"Microsoft.Performance",
			"CA1811:AvoidUncalledPrivateCode",
			Justification = "Used in DebuggerDisplay attribute.")]
		[DebuggerBrowsable (DebuggerBrowsableState.Never)]
		private string AlternativesDebuggerDisplay => (this.Alternatives.Count < 1) ? "<empty>" :
			(this.Alternatives.Count == 1) ?
				this.Alternatives[0] :
				FormattableString.Invariant ($"Count={this.Alternatives.Count}: {this.Alternatives[0]} ...");

		[SuppressMessage (
			"Microsoft.Performance",
			"CA1811:AvoidUncalledPrivateCode",
			Justification = "Used in DebuggerDisplay attribute.")]
		[DebuggerBrowsable (DebuggerBrowsableState.Never)]
		private string ExtraFieldsDebuggerDisplay => (this.ExtraFields.Count < 1) ? "<empty>" :
			(this.ExtraFields.Count == 1) ?
				this.ExtraFields[0].ToString () :
				FormattableString.Invariant ($"Count={this.ExtraFields.Count}: {this.ExtraFields[0]} ...");

		[SuppressMessage (
			"Microsoft.Performance",
			"CA1811:AvoidUncalledPrivateCode",
			Justification = "Used in DebuggerDisplay attribute.")]
		[DebuggerBrowsable (DebuggerBrowsableState.Never)]
		private string DebuggerDisplay => FormattableString.Invariant ($"Type: {_type}/{_subtype}, Encoding: {this.TransferEncoding}");

		/// <summary>
		/// Загружает сущность из указанного источника данных.
		/// </summary>
		/// <param name="source">Источник данных из которого будет загружена сущность.</param>
		/// <param name="bodyFactory">Фабрика, позволяющая создавать тело сущности с указанными параметрами.</param>
		/// <param name="defaultMediaType">Медиа тип по-умолчанию.</param>
		/// <param name="defaultMediaSubtype">Медиа подтип по-умолчанию.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая операцию.</returns>
		public Task LoadAsync (
			IBufferedSource source,
			Func<EssentialContentProperties, IEntityBody> bodyFactory,
			ContentMediaType defaultMediaType,
			string defaultMediaSubtype,
			CancellationToken cancellationToken)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if (bodyFactory == null)
			{
				throw new ArgumentNullException (nameof (bodyFactory));
			}

			Contract.EndContractBlock ();

			if (_body != null)
			{
				throw new InvalidOperationException ("Unable to load already created body.");
			}

			return LoadAsyncStateMachine ();

			async Task LoadAsyncStateMachine ()
			{
				var headerSource = new TemplateSeparatedBufferedSource (source, HeaderDecoder.CarriageReturnLinefeed2, false);
				var fields = await HeaderDecoder.LoadHeaderFieldsAsync (headerSource, cancellationToken).ConfigureAwait (false);
				await headerSource.TrySkipPartAsync (cancellationToken).ConfigureAwait (false);
				var markedFields = fields.Select (item => new HeaderFieldWithMark (item)).DuplicateToArray ();
				var contentProperties = LoadPropertiesFromHeader (markedFields, defaultMediaType, defaultMediaSubtype);
				LoadExtraFields (markedFields);
				this.ExtraFields.Clear ();
				this.ExtraFields.AddRange (markedFields.Where (item => !item.IsMarked).Select (item => item.Field));

				_body = bodyFactory.Invoke (contentProperties);

				await _body.LoadAsync (source, bodyFactory, cancellationToken).ConfigureAwait (false);
			}
		}

		/// <summary>
		/// Сохраняет сущность в указанный получатель двоичных данных.
		/// </summary>
		/// <param name="destination">Получатель двоичных данных, в который будет сохранена сущность.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая операцию.</returns>
		public Task SaveAsync (IBinaryDestination destination, CancellationToken cancellationToken)
		{
			if (destination == null)
			{
				throw new ArgumentNullException (nameof (destination));
			}

			Contract.EndContractBlock ();

			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled (cancellationToken);
			}

			var header = new ArrayList<HeaderFieldBuilder> ();
			SavePropertiesToHeader (header);
			foreach (var field in this.ExtraFields)
			{
				header.Add (HeaderFieldBuilder.CreateExactValue (field.Name, field.Value));
			}

			return SaveAsyncStateMachine ();

			async Task SaveAsyncStateMachine ()
			{
				await HeaderEncoder.SaveHeaderAsync (header, destination, cancellationToken).ConfigureAwait (false);
				await destination.WriteAsync (HeaderDecoder.CarriageReturnLinefeed, 0, HeaderDecoder.CarriageReturnLinefeed.Length, cancellationToken).ConfigureAwait (false);
				if (_body != null)
				{
					await _body.SaveAsync (destination, cancellationToken).ConfigureAwait (false);
				}
			}
		}

		/// <summary>
		/// В наследованных классах обрабатывает неиспользованные (неотмеченные) поля заголовка,
		/// отмечая успешно обработанные.
		/// </summary>
		/// <param name="header">Коллекция полей заголовка,
		/// в которой неотмеченные поля подлежатат обработке и последущей отметке в случае успеха.</param>
		protected virtual void LoadExtraFields (IReadOnlyList<HeaderFieldWithMark> header)
		{
		}

		/// <summary>
		/// Сохраняет свойства сущности в указанный заголовок, представленный коллекцией полей.
		/// </summary>
		/// <param name="header">Коллекция полей заголовка, в которую будут добавлены поля, представляющие свойства сущности.</param>
		protected virtual void SavePropertiesToHeader (IAdjustableCollection<HeaderFieldBuilder> header)
		{
			if (header == null)
			{
				throw new ArgumentNullException (nameof (header));
			}

			Contract.EndContractBlock ();

			if ((_type != ContentMediaType.Unspecified) && (_subtype != null))
			{
				header.Add (CreateContentTypeField ());
			}

			// Content-Transfer-Encoding
			if ((_body != null) && (_body.TransferEncoding != ContentTransferEncoding.Unspecified))
			{
				header.Add (HeaderFieldBuilder.CreateExactValue (
					HeaderFieldName.ContentTransferEncoding,
					_body.TransferEncoding.GetName ()));
			}

			// Content-ID
			if (this.Id != null)
			{
				header.Add (HeaderFieldBuilder.CreateExactValue (
					HeaderFieldName.ContentId,
					this.Id.ToAngleString ()));
			}

			// Content-Description
			if (this.Description != null)
			{
				header.Add (HeaderFieldBuilder.CreateUnstructured (
					HeaderFieldName.ContentDescription,
					this.Description));
			}

			// Content-Disposition
			if (_dispositionType != ContentDispositionType.Unspecified)
			{
				header.Add (CreateContentDispositionField ());
			}

			// Content-Base
			if (this.Base != null)
			{
				header.Add (HeaderFieldBuilder.CreateExactValue (HeaderFieldName.ContentBase, this.Base));
			}

			// Content-Location
			if (this.Location != null)
			{
				header.Add (HeaderFieldBuilder.CreateExactValue (HeaderFieldName.ContentLocation, this.Location));
			}

			// Content-Language
			if (this.Languages.Count > 0)
			{
				header.Add (HeaderFieldBuilder.CreateLanguageList (HeaderFieldName.ContentLanguage, this.Languages));
			}

			// Content-Features
			if (this.Features != null)
			{
				header.Add (HeaderFieldBuilder.CreateUnstructured (
					HeaderFieldName.ContentFeatures,
					this.Features));
			}

			// Content-Alternative
			if (this.Alternatives.Count > 0)
			{
				foreach (var item in this.Alternatives)
				{
					header.Add (HeaderFieldBuilder.CreateUnstructured (
						HeaderFieldName.ContentAlternative,
						item));
				}
			}

			// Content-MD5
			if (_md5 != null)
			{
				header.Add (HeaderFieldBuilder.CreateExactValue (HeaderFieldName.ContentMD5, Convert.ToBase64String (_md5)));
			}

			// Content-Duration
			if (_duration.HasValue)
			{
				header.Add (HeaderFieldBuilder.CreateExactValue (
					HeaderFieldName.ContentDuration,
					((int)_duration.Value.TotalSeconds).ToString (CultureInfo.InvariantCulture)));
			}
		}

		private static void ParseContentTransferEncodingField (
			HeaderFieldWithMark fieldEntry,
			EssentialContentProperties contentProperties,
			ref ContentTransferEncoding transferEncoding)
		{
			if (transferEncoding != ContentTransferEncoding.Unspecified)
			{
				throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.ContentTransferEncoding) + "' field.");
			}

			var isValidTransferEncoding = TransferEncodingHelper.TryParse (HeaderDecoder.DecodeAtom (fieldEntry.Field.Value), out transferEncoding);
			if (isValidTransferEncoding)
			{
				contentProperties.TransferEncoding = transferEncoding;
				fieldEntry.IsMarked = true;
			}
			else
			{
				// According to RFC 2045 part 6.4:
				// Any entity with an unrecognized Content-Transfer-Encoding must be treated as if it has a Content-Type of "application/octet-stream",
				// regardless of what the Content-Type header field actually says.
				contentProperties.Type = ContentMediaType.Application;
				contentProperties.Subtype = ApplicationMediaSubtypeNames.OctetStream;
				contentProperties.TransferEncoding = ContentTransferEncoding.Binary;
				contentProperties.Parameters.Clear ();
			}
		}

		private EssentialContentProperties LoadPropertiesFromHeader (
			IReadOnlyList<HeaderFieldWithMark> header,
			ContentMediaType defaultMediaType,
			string defaultMediaSubtype)
		{
			if (header == null)
			{
				throw new ArgumentNullException (nameof (header));
			}

			Contract.EndContractBlock ();

			ResetProperties ();

			var transferEncoding = ContentTransferEncoding.Unspecified;

			// В contentProperties храним вычисляемые свойства, которые будут использованы для создания тела сущности
			// с учётом множества оговорок по умолчальным значениям и значениям в случае нераспознанных параметров.
			// Например, если указан Content-Transfer-Encoding который не распознан, то для создания тела сущности
			// будет принудительно использован Content-Type: application/octet-stream независимо от фактически указанного.
			var contentProperties = new EssentialContentProperties ();

			// перебираем все поля
			foreach (var field in header.Where (item => !item.IsMarked))
			{
				try
				{
					switch (field.Field.Name)
					{
						case HeaderFieldName.ContentType:
							ParseContentTypeField (field, contentProperties);
							break;
						case HeaderFieldName.ContentDisposition:
							ParseContentDispositionField (field);
							break;
						case HeaderFieldName.ContentTransferEncoding:
							ParseContentTransferEncodingField (field, contentProperties, ref transferEncoding);
							break;
						case HeaderFieldName.ContentId:
							ParseContentIdField (field);
							break;
						case HeaderFieldName.ContentDescription:
							ParseContentDescriptionField (field);
							break;
						case HeaderFieldName.ContentBase:
							ParseContentBaseField (field);
							break;
						case HeaderFieldName.ContentLocation:
							ParseContentLocationField (field);
							break;
						case HeaderFieldName.ContentFeatures:
							ParseContentFeaturesField (field);
							break;
						case HeaderFieldName.ContentLanguage:
							ParseContentLanguageField (field);
							break;
						case HeaderFieldName.ContentAlternative:
							ParseContentAlternativeField (field);
							break;
						case HeaderFieldName.ContentMD5:
							ParseContentMD5Field (field);
							break;
						case HeaderFieldName.ContentDuration:
							ParseContentDurationField (field);
							break;
					}
				}
				catch (FormatException)
				{
					// игнорируем некорректные поля, они останутся немаркированными
				}
			}

			// RFC 2045 part 5.2:
			// default 'Content-type: text/plain; charset=us-ascii' is assumed if no Content-Type header field is specified.
			// RFC 2046 часть 5.1.5:
			// in a digest, the default Content-Type value for a body part is changed from "text/plain" to "message/rfc822".
			if (contentProperties.Type == ContentMediaType.Unspecified)
			{
				contentProperties.Type = defaultMediaType;
				contentProperties.Subtype = defaultMediaSubtype;
			}

			// RFC 2045 part 6.1:
			// "Content-Transfer-Encoding: 7BIT" is assumed if the Content-Transfer-Encoding header field is not present.
			if (contentProperties.TransferEncoding == ContentTransferEncoding.Unspecified)
			{
				contentProperties.TransferEncoding = ContentTransferEncoding.SevenBit;
			}

			return contentProperties;
		}

		private void ParseContentTypeField (HeaderFieldWithMark fieldEntry, EssentialContentProperties contentProperties)
		{
			if ((_type != ContentMediaType.Unspecified) || (_subtype != null))
			{
				throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.ContentType) + "' field.");
			}

			/*
			content   := "Content-Type" ":" type "/" subtype *(";" parameter)
			parameter              := regular-parameter / extended-parameter
			regular-parameter      := regular-name "=" value
			extended-parameter     := (extended-initial-name "=" extended-initial-value) / (extended-other-names "=" extended-other-values)
			extended-initial-name  := attribute [initial-section] "*"
			extended-initial-value := [charset] "'" [language] "'" extended-other-values
			extended-other-names   := attribute other-sections "*"
			extended-other-values  := *(ext-octet / attribute-char)
			regular-name := attribute [section]
			value     := token / quoted-string
			*/
			try
			{
				var data = HeaderDecoder.DecodeAtomAndParameterList (fieldEntry.Field.Value);
				var idx = data.Text.IndexOf ('/');
				if ((idx < 1) || (idx > (data.Text.Length - 2)))
				{
					throw new FormatException ("Invalid format of '" + HeaderFieldNameHelper.GetName (HeaderFieldName.ContentType) + "' field.");
				}

				var mediaTypeStr = data.Text.Substring (0, idx);
				var isValidMediaType = MediaTypeHelper.TryParse (mediaTypeStr, out _type);
				if (!isValidMediaType)
				{
					throw new FormatException ("Unrecognized value of MediaType '" + mediaTypeStr + "'.");
				}

				_subtype = data.Text.Substring (idx + 1);
				contentProperties.Parameters.AddRange (data.Parameters);
				foreach (var parameter in data.Parameters)
				{
					// в старых программах имя файла-вложения было не по стандарту в "Content-Type: xxx/yyy; name=..."
					// копируем его туда где оно должно быть по стандарту ("Content-Disposition: attachment; filename=")
					var isName = MediaParameterNames.Name.Equals (parameter.Name, StringComparison.OrdinalIgnoreCase);
					if (isName)
					{
						if (this.FileName == null)
						{
							this.FileName = parameter.Value;
						}
					}
				}

				if (contentProperties.Type == ContentMediaType.Unspecified)
				{
					// значения могут быть уже установлены в другом месте
					contentProperties.Type = _type;
					contentProperties.Subtype = _subtype;
				}

				fieldEntry.IsMarked = true;
			}
			catch (FormatException)
			{
				// According to RFC 2045 part 5.2:
				// default 'Content-type: text/plain; charset=us-ascii' is assumed if no Content-Type header field is specified.
				// It is also recommend that this default be assumed when a syntactically invalid Content-Type header field is encountered.
				if (contentProperties.Type == ContentMediaType.Unspecified)
				{
					// значения могут быть уже установлены в другом месте
					contentProperties.Type = DefaultType;
					contentProperties.Subtype = DefaultSubtype;
					contentProperties.Parameters.Clear ();
				}
			}
		}

		private void ParseContentDispositionField (HeaderFieldWithMark fieldEntry)
		{
			if (_dispositionType != ContentDispositionType.Unspecified)
			{
				throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.ContentDisposition) + "' field.");
			}

			// disposition := "Content-Disposition" ":" disposition-type *(";" disposition-parm)
			// disposition-type := "inline" / "attachment" / extension-token
			// disposition-parm := filename-parm / creation-date-parm / modification-date-parm / read-date-parm / size-parm / parameter
			var data = HeaderDecoder.DecodeAtomAndParameterList (fieldEntry.Field.Value);
			var isValidDispositionType = DispositionTypeHelper.TryParse (data.Text, out ContentDispositionType dtype);
			if (isValidDispositionType)
			{
				_dispositionType = dtype;
			}

			foreach (var parameter in data.Parameters)
			{
				var isFilename = DispositionParameterNames.Filename.Equals (parameter.Name, StringComparison.OrdinalIgnoreCase);
				if (isFilename)
				{
					this.FileName = parameter.Value;
				}
				else
				{
					var isCreationDate = DispositionParameterNames.CreationDate.Equals (parameter.Name, StringComparison.OrdinalIgnoreCase);
					if (isCreationDate)
					{
						this.CreationDate = InternetDateTime.Parse (parameter.Value);
					}
					else
					{
						var isModificationDate = DispositionParameterNames.ModificationDate.Equals (parameter.Name, StringComparison.OrdinalIgnoreCase);
						if (isModificationDate)
						{
							this.ModificationDate = InternetDateTime.Parse (parameter.Value);
						}
						else
						{
							var isReadDate = DispositionParameterNames.ReadDate.Equals (parameter.Name, StringComparison.OrdinalIgnoreCase);
							if (isReadDate)
							{
								this.ReadDate = InternetDateTime.Parse (parameter.Value);
							}
							else
							{
								var isSize = DispositionParameterNames.Size.Equals (parameter.Name, StringComparison.OrdinalIgnoreCase);
								if (isSize)
								{
									this.Size = long.Parse (
										parameter.Value,
										NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite,
										CultureInfo.InvariantCulture);
								}
							}
						}
					}
				}
			}

			fieldEntry.IsMarked = true;
		}

		private void ParseContentIdField (HeaderFieldWithMark fieldEntry)
		{
			if (this.Id != null)
			{
				throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.ContentId) + "' field.");
			}

			var adrs = HeaderDecoder.DecodeAddrSpecList (fieldEntry.Field.Value);
			this.Id = adrs.Single ();
			fieldEntry.IsMarked = true;
		}

		private void ParseContentDescriptionField (HeaderFieldWithMark fieldEntry)
		{
			if (this.Description != null)
			{
				throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.ContentDescription) + "' field.");
			}

			this.Description = HeaderDecoder.DecodeUnstructured (fieldEntry.Field.Value).Trim ();
			fieldEntry.IsMarked = true;
		}

		private void ParseContentBaseField (HeaderFieldWithMark fieldEntry)
		{
			if (this.Base != null)
			{
				throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.ContentBase) + "' field.");
			}

			this.Base = fieldEntry.Field.Value.Trim ();
			fieldEntry.IsMarked = true;
		}

		private void ParseContentLocationField (HeaderFieldWithMark fieldEntry)
		{
			if (this.Location != null)
			{
				throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.ContentLocation) + "' field.");
			}

			this.Location = fieldEntry.Field.Value.Trim ();
			fieldEntry.IsMarked = true;
		}

		private void ParseContentFeaturesField (HeaderFieldWithMark fieldEntry)
		{
			if (this.Features != null)
			{
				throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.ContentFeatures) + "' field.");
			}

			this.Features = HeaderDecoder.DecodeUnstructured (fieldEntry.Field.Value).Trim ();
			fieldEntry.IsMarked = true;
		}

		private void ParseContentLanguageField (HeaderFieldWithMark fieldEntry)
		{
			if (this.Languages.Count > 0)
			{
				throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.ContentLanguage) + "' field.");
			}

			// Language-List = Language-Tag [CFWS] *("," [CFWS] Language-Tag [CFWS])
			this.Languages.AddRange (HeaderDecoder.DecodeAtomList (fieldEntry.Field.Value));
			fieldEntry.IsMarked = true;
		}

		private void ParseContentAlternativeField (HeaderFieldWithMark fieldEntry)
		{
			this.Alternatives.Add (HeaderDecoder.DecodeUnstructured (fieldEntry.Field.Value).Trim ());
			fieldEntry.IsMarked = true;
		}

		private void ParseContentMD5Field (HeaderFieldWithMark fieldEntry)
		{
			if (_md5 != null)
			{
				throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.ContentMD5) + "' field.");
			}

			var md5 = Convert.FromBase64String (fieldEntry.Field.Value);
			if (md5.Length != 16)
			{
				throw new FormatException ("Invalid format of '" + HeaderFieldNameHelper.GetName (HeaderFieldName.ContentMD5) + "' field.");
			}

			_md5 = md5;
			fieldEntry.IsMarked = true;
		}

		private void ParseContentDurationField (HeaderFieldWithMark fieldEntry)
		{
			if (_duration != null)
			{
				throw new FormatException ("More than one '" + HeaderFieldNameHelper.GetName (HeaderFieldName.ContentDuration) + "' field.");
			}

			var seconds = int.Parse (
				fieldEntry.Field.Value.Trim (),
				NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite,
				CultureInfo.InvariantCulture);
			_duration = new TimeSpan (0, 0, seconds);
			fieldEntry.IsMarked = true;
		}

		private HeaderFieldBuilder CreateContentTypeField ()
		{
			// content := "Content-Type" ":" type "/" subtype *(";" parameter)
			var field = HeaderFieldBuilder.CreateExactValue (HeaderFieldName.ContentType, _type.GetName () + "/" + _subtype);

			if (_body is TextEntityBody textEntityBody)
			{
				field.AddParameter (MediaParameterNames.Charset, textEntityBody.Encoding.WebName);
			}

			if (_body is CompositeEntityBody entityBodyComposite)
			{
				{
					field.AddParameter (MediaParameterNames.Boundary, entityBodyComposite.Boundary);
				}
			}

			if (_body is ReportEntityBody reportEntityBody)
			{
				field.AddParameter (MediaParameterNames.ReportType, reportEntityBody.ReportType);
			}

			return field;
		}

		private HeaderFieldBuilder CreateContentDispositionField ()
		{
			// disposition := "Content-Disposition" ":" disposition-type *(";" disposition-parm)
			var field = HeaderFieldBuilder.CreateExactValue (HeaderFieldName.ContentDisposition, _dispositionType.GetName ());
			if (this.FileName != null)
			{
				field.AddParameter (DispositionParameterNames.Filename, this.FileName);
			}

			if (this.ModificationDate.HasValue)
			{
				field.AddParameter (DispositionParameterNames.ModificationDate, this.ModificationDate.Value.ToInternetString ());
			}

			if (this.CreationDate.HasValue)
			{
				field.AddParameter (DispositionParameterNames.CreationDate, this.CreationDate.Value.ToInternetString ());
			}

			if (this.ReadDate.HasValue)
			{
				field.AddParameter (DispositionParameterNames.ReadDate, this.ReadDate.Value.ToInternetString ());
			}

			if (this.Size.HasValue)
			{
				field.AddParameter (DispositionParameterNames.Size, this.Size.Value.ToString (CultureInfo.InvariantCulture));
			}

			return field;
		}

		// очищаем все свойства
		private void ResetProperties ()
		{
			_type = ContentMediaType.Unspecified;
			_subtype = null;
			_dispositionType = ContentDispositionType.Unspecified;
			this.FileName = null;
			this.Description = null;
			this.Base = null;
			this.Location = null;
			this.Features = null;
			this.Languages.Clear ();
			this.Alternatives.Clear ();
			_duration = null;
			_md5 = null;
			this.CreationDate = null;
			this.ModificationDate = null;
			this.ReadDate = null;
			this.Size = null;
			this.Id = null;
		}
	}
}
