using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Collections;
using Novartment.Base.Collections.Immutable;
using Novartment.Base.Collections.Linq;
using static System.Linq.Enumerable;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Методы расширения для Entity.
	/// </summary>
	public static class EntityExtensions
	{
		/// <summary>
		/// Получает коллекцию вложенных сущностей с простым содержимым.
		/// Позволяет указывать необходимости рекурсивного поиска и обработки вложенных сообщений.
		/// Возвращает список из одной стартовой сущности если она простая.
		/// </summary>
		/// <param name="entity">Сущность, с которой начинать поиск.</param>
		/// <param name="recursive">Признак запроса рекурсивного прохода по сущностям.</param>
		/// <param name="includeNestedMessages">Признак запроса вложенных сообщений.</param>
		/// <returns>Коллекция вложенных сущностей.</returns>
		public static IReadOnlyList<Entity> GetChildContentParts (this Entity entity, bool recursive, bool includeNestedMessages)
		{
			if (entity == null)
			{
				throw new ArgumentNullException (nameof (entity));
			}

			Contract.EndContractBlock ();

			if (!recursive)
			{
				if (entity.Body is ICompositeEntityBody compositeEntityBody)
				{
					var parts = compositeEntityBody.Parts.Where (item => item.Body is IDiscreteEntityBody);
					return new ReadOnlyArray<Entity> (parts.ToArray ());
				}

				if (entity.Body is MessageEntityBody entityBodyMessage && includeNestedMessages)
				{
					entity = entityBodyMessage.Message;
				}

				if (entity.Body is IDiscreteEntityBody entityBodySinglepart)
				{
					return new ReadOnlyArray<Entity> (new[] { entity });
				}

				return ReadOnlyList.Empty<Entity> ();
			}

			var result = new ArrayList<Entity> ();
			var entitiesQueue = new ArrayList<Entity>
			{
				entity,
			};
			while (entitiesQueue.TryTakeFirst (out Entity currentEntity))
			{
				if (currentEntity.Body is ICompositeEntityBody compositeEntityBody)
				{
					int i = 0;
					foreach (var part in compositeEntityBody.Parts)
					{
						entitiesQueue.Insert (i++, part);
					}
				}
				else
				{
					// Add nested message for processing (nested message entities will be included).
					if (currentEntity.Body is MessageEntityBody entityBodyMessage)
					{
						if (includeNestedMessages)
						{
							entitiesQueue.Insert (0, entityBodyMessage.Message);
						}
					}
					else
					{
						if (currentEntity.Body is IDiscreteEntityBody entityBodySinglepart)
						{
							result.Add (currentEntity);
						}
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Получает коллекцию вложений указанной сущности с возможностью указания необходимости возврата вложений из вложенных сообщений.
		/// </summary>
		/// <param name="entity">Сообщение, для которого нужно получить список вложений.</param>
		/// <param name="includeNestedMessages">Признак, указывающий необходимость включения в результирующий список вложенных сообщений.</param>
		/// <returns>Список вложений сущности.</returns>
		public static IReadOnlyList<Entity> GetAttachments (this Entity entity, bool includeNestedMessages)
		{
			if (entity == null)
			{
				throw new ArgumentNullException (nameof (entity));
			}

			Contract.EndContractBlock ();

			return entity.GetChildContentParts (true, includeNestedMessages)
				.Where (e => (e != null) &&
					((e.DispositionType == ContentDispositionType.Attachment) ||
					((e.Body is IDiscreteEntityBody) && !(e.Body is TextEntityBody))))
				.ToArray ();
		}

		/// <summary>
		/// Добавляет к указанной сущности новую текстовую сущность указанного подтипа и кодировки передачи
		/// с указанным содержимым в указанной кодировке символов.
		/// </summary>
		/// <param name="entity">Родительская сущность с множественным содержимым.</param>
		/// <param name="text">Текст создаваемой сущности.</param>
		/// <param name="encoding">Кодировка символов, используя которую будет создано содержимое сущности.
		/// Укажите null чтобы использовать кодировку символов по умолчанию ("utf-8").</param>
		/// <param name="textType">Тип текста согласно <url>http://www.iana.org/assignments/media-types</url>,
		/// например "plain", "xml", "html".
		/// Укажите null чтобы использовать тип по умолчанию ("plain").</param>
		/// <param name="transferEncoding">Кодировка передачи создаваемой сущности.
		/// Укажите ContentTransferEncoding.Unspecified чтобы использовать универсальную (возможно неоптимальную) кодировку.</param>
		/// <returns>Созданная сущность.</returns>
		public static Entity AddTextPart (
			this Entity entity,
			string text,
			Encoding encoding = null,
			string textType = null,
			ContentTransferEncoding transferEncoding = ContentTransferEncoding.Unspecified)
		{
			if (entity == null)
			{
				throw new ArgumentNullException (nameof (entity));
			}

			if (text == null)
			{
				throw new ArgumentNullException (nameof (text));
			}

			Contract.EndContractBlock ();

			if (!(entity.Body is ICompositeEntityBody compositeEntityBody))
			{
				throw new InvalidOperationException("Can not add part to entity with discrete content. Parts can be added only to composite entities.");
			}

			var newBody = new TextEntityBody (encoding, (transferEncoding != ContentTransferEncoding.Unspecified) ? transferEncoding : ContentTransferEncoding.Base64);
			newBody.SetText (text);
			var newEntity = new Entity (newBody, ContentMediaType.Text, textType ?? TextMediaSubtypeNames.Plain);
			compositeEntityBody.Parts.Add (newEntity);
			return newEntity;
		}

		/// <summary>
		/// Добавляет к указанной сущности новую сущность с множественным содержимым.
		/// </summary>
		/// <param name="entity">Родительская сущность с множественным содержимым.</param>
		/// <param name="multipartType">Тип сущности с множественным содержимым согласно <url>http://www.iana.org/assignments/media-types</url>,
		/// например "mixed", "alternative", "parallel".
		/// Укажите null чтобы использовать тип по умолчанию ("mixed").</param>
		/// <returns>Созданная сущность.</returns>
		public static Entity AddCompositePart (this Entity entity, string multipartType)
		{
			if (entity == null)
			{
				throw new ArgumentNullException (nameof (entity));
			}

			Contract.EndContractBlock ();

			if (!(entity.Body is ICompositeEntityBody compositeEntityBody))
			{
				throw new InvalidOperationException("Can not add part to entity with discrete content. Parts can be added only to composite entities.");
			}

			var newBody = new CompositeEntityBody (null);
			var newEntity = new Entity (newBody, ContentMediaType.Multipart, multipartType ?? MultipartMediaSubtypeNames.Mixed);
			compositeEntityBody.Parts.Add (newEntity);
			return newEntity;
		}

		/// <summary>
		/// Добавляет к указанной сущности новую сущность, представляющую дайджест сообщений.
		/// </summary>
		/// <param name="entity">Родительская сущность с множественным содержимым.</param>
		/// <returns>Созданная сущность.</returns>
		public static Entity AddDigestPart (this Entity entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException (nameof (entity));
			}

			Contract.EndContractBlock ();

			if (!(entity.Body is ICompositeEntityBody compositeEntityBody))
			{
				throw new InvalidOperationException("Can not add part to entity with discrete content. Parts can be added only to composite entities.");
			}

			var newBody = new DigestEntityBody ();
			var newEntity = new Entity (newBody, ContentMediaType.Multipart, MultipartMediaSubtypeNames.Digest);
			compositeEntityBody.Parts.Add (newEntity);
			return newEntity;
		}

		/// <summary>
		/// Добавляет к указанной сущности новую сущность, представляющую отдельное интернет-сообщение.
		/// </summary>
		/// <param name="entity">Родительская сущность с множественным содержимым.</param>
		/// <param name="message">Интернет-сообщение, которое будет содержать добавляемая сущность.</param>
		/// <param name="includeContent">Признак вставки полной копии сообщения.
		/// Если false, то будет вставлена только копия заголовка сообщения.</param>
		/// <returns>Созданная сущность.</returns>
		public static Entity AddMessagePart (this Entity entity, MailMessage message, bool includeContent)
		{
			if (entity == null)
			{
				throw new ArgumentNullException (nameof (entity));
			}

			if (message == null)
			{
				throw new ArgumentNullException (nameof (message));
			}

			Contract.EndContractBlock ();

			if (!(entity.Body is ICompositeEntityBody compositeEntityBody))
			{
				throw new InvalidOperationException("Can not add part to entity with discrete content. Parts can be added only to composite entities.");
			}

			var newBody = new MessageEntityBody
			{
				Message = includeContent ? message : message.CreateCopyWithoutContent (),
			};
			var newEntity = new Entity (
				newBody,
				includeContent ? ContentMediaType.Message : ContentMediaType.Text,
				includeContent ? MessageMediaSubtypeNames.Rfc822 : TextMediaSubtypeNames.Rfc822Headers);
			compositeEntityBody.Parts.Add (newEntity);
			return newEntity;
		}

		/// <summary>
		/// Добавляет к указанной сущности новую сущность, представляющую уведомление о статусе доставки сообщения.
		/// </summary>
		/// <param name="entity">Родительская сущность с множественным содержимым.</param>
		/// <returns>Созданная сущность.</returns>
		public static Entity AddDeliveryStatusPart (this Entity entity)
		{
			if (entity == null)
			{
				throw new ArgumentNullException (nameof (entity));
			}

			Contract.EndContractBlock ();

			if (!(entity.Body is ICompositeEntityBody compositeEntityBody))
			{
				throw new InvalidOperationException("Can not add part to entity with discrete content. Parts can be added only to composite entities.");
			}

			var newBody = new DeliveryStatusEntityBody ();
			var newEntity = new Entity (newBody, ContentMediaType.Message, MessageMediaSubtypeNames.DeliveryStatus);
			compositeEntityBody.Parts.Add (newEntity);
			return newEntity;
		}

		/// <summary>
		/// Добавляет к указанной сущности новую сущность, представляющую уведомление об изменении дислокации сообщения указанным адресатом.
		/// </summary>
		/// <param name="entity">Родительская сущность с множественным содержимым.</param>
		/// <param name="recipient">Адресат, для которого создаётся уведомление об изменении дислокации.</param>
		/// <param name="disposition">Действие, предпринятое почтовым агентом от имени пользователя.</param>
		/// <returns>Созданная сущность.</returns>
		public static Entity AddDispositionNotificationPart (
			this Entity entity,
			AddrSpec recipient,
			MessageDispositionChangedAction disposition)
		{
			if (entity == null)
			{
				throw new ArgumentNullException (nameof (entity));
			}

			if (recipient == null)
			{
				throw new ArgumentNullException (nameof (recipient));
			}

			Contract.EndContractBlock ();

			if (!(entity.Body is ICompositeEntityBody compositeEntityBody))
			{
				throw new InvalidOperationException("Can not add part to entity with discrete content. Parts can be added only to composite entities.");
			}

			var newBody = new DispositionNotificationEntityBody ();
			var newEntity = new Entity (newBody, ContentMediaType.Message, MessageMediaSubtypeNames.DispositionNotification);
			((DispositionNotificationEntityBody)newEntity.Body).FinalRecipient = new NotificationFieldValue (NotificationFieldValueKind.Mailbox, recipient.ToString ());
			((DispositionNotificationEntityBody)newEntity.Body).Disposition = disposition;
			compositeEntityBody.Parts.Add (newEntity);
			return newEntity;
		}

		/// <summary>
		/// Добавляет к указанной сущности новую сущность, представляющую изображение указанного типа.
		/// </summary>
		/// <param name="entity">Родительская сущность с множественным содержимым.</param>
		/// <param name="imageType">Тип изображения согласно <url>http://www.iana.org/assignments/media-types</url>,
		/// например "png", "jpg", "svg+xml".</param>
		/// <param name="data">Источник данных, содержащий изображение.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, результатом которой является созданная сущность.</returns>
		public static Task<Entity> AddImagePartAsync (
			this Entity entity,
			string imageType,
			IBufferedSource data,
			CancellationToken cancellationToken)
		{
			if (entity == null)
			{
				throw new ArgumentNullException (nameof (entity));
			}

			if (imageType == null)
			{
				throw new ArgumentNullException (nameof (imageType));
			}

			if (data == null)
			{
				throw new ArgumentNullException (nameof (data));
			}

			Contract.EndContractBlock ();

			if (!(entity.Body is ICompositeEntityBody compositeEntityBody))
			{
				throw new InvalidOperationException("Can not add part to entity with discrete content. Parts can be added only to composite entities.");
			}

			var newBody = new DataEntityBody (ContentTransferEncoding.Base64);
			var task = newBody.SetDataAsync (data, cancellationToken);

			return AddImagePartAsyncFinalizer ();

			async Task<Entity> AddImagePartAsyncFinalizer ()
			{
				await task.ConfigureAwait (false);

				var newEntity = new Entity (newBody, ContentMediaType.Image, imageType);
				compositeEntityBody.Parts.Add (newEntity);
				return newEntity;
			}
		}

		/// <summary>
		/// Добавляет к указанной сущности новую сущность,
		/// представляющую внутренний формат прикладной программы указанного типа.
		/// </summary>
		/// <param name="entity">Родительская сущность с множественным содержимым.</param>
		/// <param name="applicationType">Внутренний формат прикладной программы согласно <url>http://www.iana.org/assignments/media-types</url>,
		/// например "pdf", "zip", "octet-stream".</param>
		/// <param name="data">Источник данных, содержащий данные внутреннего формата прикладной программы.</param>
		/// <param name="transferEncoding">Кодировка передачи создаваемой сущности.
		/// Укажите ContentTransferEncoding.Unspecified чтобы использовать универсальную (возможно неоптимальную) кодировку.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, результатом которой является созданная сущность.</returns>
		public static Task<Entity> AddApplicationPartAsync (
			this Entity entity,
			string applicationType,
			IBufferedSource data,
			ContentTransferEncoding transferEncoding,
			CancellationToken cancellationToken)
		{
			if (entity == null)
			{
				throw new ArgumentNullException (nameof (entity));
			}

			if (applicationType == null)
			{
				throw new ArgumentNullException (nameof (applicationType));
			}

			if (data == null)
			{
				throw new ArgumentNullException (nameof (data));
			}

			Contract.EndContractBlock ();

			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<Entity> (cancellationToken);
			}

			if (!(entity.Body is ICompositeEntityBody compositeEntityBody))
			{
				throw new InvalidOperationException("Can not add part to entity with discrete content. Parts can be added only to composite entities.");
			}

			var newBody = new DataEntityBody (
				(transferEncoding != ContentTransferEncoding.Unspecified) ?
					transferEncoding :
					ContentTransferEncoding.Base64);
			var task = newBody.SetDataAsync (data, cancellationToken);

			return AddApplicationPartAsyncFinalizer ();

			async Task<Entity> AddApplicationPartAsyncFinalizer ()
			{
				await task.ConfigureAwait (false);

				var newEntity = new Entity (newBody, ContentMediaType.Application, applicationType);
				compositeEntityBody.Parts.Add (newEntity);
				return newEntity;
			}
		}

		/// <summary>
		/// Добавляет к указанной сущности новую сущность-вложение с указанным именем,
		/// представляющее содержимое указанного источника данных.
		/// </summary>
		/// <param name="entity">Сущность, в которую нужно добавить вложение.</param>
		/// <param name="data">Источник данных, содержимое которого станет содержимым вложения.</param>
		/// <param name="fileName">Имя, которое будет указано в качестве имени файла вложения.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, результатом которой является созданное вложение в сообщение.</returns>
		public static Task<Entity> AddAttachmentAsync (
			this Entity entity,
			IBufferedSource data,
			string fileName,
			CancellationToken cancellationToken)
		{
			if (entity == null)
			{
				throw new ArgumentNullException (nameof (entity));
			}

			if (data == null)
			{
				throw new ArgumentNullException (nameof (data));
			}

			if (fileName == null)
			{
				throw new ArgumentNullException (nameof (fileName));
			}

			Contract.EndContractBlock ();

			if (!(entity.Body is ICompositeEntityBody compositeEntityBody))
			{
				throw new InvalidOperationException("Can not add part to entity with discrete content. Parts can be added only to composite entities.");
			}

			var attachmentBody = new DataEntityBody (ContentTransferEncoding.Base64);
			var observer = new SizeCalculator ();
			var dataObservable = new ObservableBufferedSource (data, observer);

			var task = attachmentBody.SetDataAsync (dataObservable, cancellationToken);

			return AddAttachmentAsyncFinalizer ();

			async Task<Entity> AddAttachmentAsyncFinalizer ()
			{
				await task.ConfigureAwait (false);

				var attachment = new Entity (attachmentBody, ContentMediaType.Application, ApplicationMediaSubtypeNames.OctetStream)
				{
					DispositionType = ContentDispositionType.Attachment,
					FileName = fileName,
					Size = observer.Size,
				};
				compositeEntityBody.Parts.Add (attachment);
				return attachment;
			}
		}

		/// <summary>
		/// Асинхронно добавляет к указанной сущности новую сущность-вложение,
		/// представляющее указанный файл.
		/// </summary>
		/// <param name="entity">Сущность, в которую нужно добавить вложение.</param>
		/// <param name="fileName">Путь к файлу.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, результатом которой является созданная сущность.</returns>
		public static Task<Entity> AddAttachmentAsync (this Entity entity, string fileName, CancellationToken cancellationToken)
		{
			if (entity == null)
			{
				throw new ArgumentNullException (nameof (entity));
			}

			if (fileName == null)
			{
				throw new ArgumentNullException (nameof (fileName));
			}

			Contract.EndContractBlock ();

			var fileInfo = new FileInfo (fileName);
			var stream = fileInfo.OpenRead ();
			Task<Entity> task;
			try
			{
				// TODO: размер буфера сделать конфигурируемым
				task = AddAttachmentAsync (
					entity,
					stream.AsBufferedSource (new byte[Math.Min (fileInfo.Length, 4096L)]),
					fileInfo.Name,
					cancellationToken);

				return AddAttachmentAsyncFinalizer ();
			}
			catch
			{
				stream?.Dispose ();
				throw;
			}

			async Task<Entity> AddAttachmentAsyncFinalizer ()
			{
				Entity attachment;
				try
				{
					attachment = await task.ConfigureAwait (false);
				}
				finally
				{
					stream?.Dispose ();
				}

				attachment.CreationDate = fileInfo.CreationTime;
				attachment.ModificationDate = fileInfo.LastWriteTime;
				attachment.ReadDate = fileInfo.LastAccessTime;
				return attachment;
			}
		}

		private class SizeCalculator :
			IProgress<long>
		{
			internal long Size { get; set; }

			void IProgress<long>.Report (long value)
			{
				this.Size += value;
			}
		}
	}
}
