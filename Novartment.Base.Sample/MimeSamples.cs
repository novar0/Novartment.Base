using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.Net;
using Novartment.Base.Net.Mime;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Sample
{
	public class MimeSamples
	{
		public static async Task MessageSaveAttachmentsAsync (CancellationToken cancellationToken)
		{
			// асинхронно сохраняем вложение из полученного сообщения
			using (var fs = new FileStream (@".\Pickup\test1.eml", FileMode.Open, FileAccess.Read))
			{
				var message = new MailMessage ();
				await message.LoadAsync (fs.AsBufferedSource (new byte[1024]), EntityBodyFactory.Create, cancellationToken).ConfigureAwait (false);
				var subj = message.Subject;
				var parts = message.GetChildContentParts (true, false);
				var text = ((TextEntityBody)parts[0].Body).GetText ();
				var html = ((TextEntityBody)parts[1].Body).GetText ();
				foreach (var attObj in message.GetAttachments (true))
				{
					string fileName = "async_" + attObj.FileName;
					var dataSrc = (attObj.Body as IDiscreteEntityBody).GetDataSource ();
					using (var destStream = new FileStream (fileName, FileMode.Create, FileAccess.Write))
					{
						await dataSrc.WriteToAsync (destStream.AsBinaryDestination (), cancellationToken).ConfigureAwait (false);
					}
				}
			}
		}

		// создаём новое сообщение простейшего типа "просто текст"
		public static async Task MessageCreateSimpleAsync (CancellationToken cancellationToken)
		{
			var msg = MailMessage.CreateSimpleText ();
			msg.GenerateId ();
			msg.RecipientTo.Add ("someone@server.com", "Адресат Один");
			msg.From.Add ("noone@mailinator.com", "Иван Сидоров");
			msg.Subject = "тема сообщения";
			(msg.Body as TextEntityBody).SetText ("текст сообщения");

			var stream = new FileStream (@"composed1.eml", FileMode.Create, FileAccess.Write);
			await msg.SaveAsync (stream.AsBinaryDestination (), cancellationToken).ConfigureAwait (false);
			stream.Dispose ();
		}

		// создаём новое композитное сообщение, состоящее из частей
		// большие части добавляем асинхронно, результат сохраняем асинхронно
		public static async Task MessageCreateCompositeAsync (CancellationToken cancellationToken)
		{
			var msg = MailMessage.CreateComposite ();
			msg.GenerateId ();
			msg.RecipientTo.Add ("man1@server.com", "Адресат Один");
			msg.RecipientTo.Add ("man2@server.com", "Адресат Два");
			msg.RecipientTo.Add ("man3@server.com", "Адресат Три");
			msg.From.Add ("noone@mail.net", "Иван Сидоров");
			msg.Subject = "тема сообщения";
			msg.References.Add ("fdz.fue8vae@node12.server.ru");

			var part = msg.AddCompositePart (MultipartMediaSubtypeNames.Alternative);
			part.AddTextPart ("текст сообщения");
			part.AddTextPart ("<body><h1>текст сообщения<img src=\"test4.ico\" /></h1></body>", Encoding.UTF8, TextMediaSubtypeNames.Html, ContentTransferEncoding.EightBit);

			await msg.AddAttachmentAsync ("test4.ico", cancellationToken).ConfigureAwait (false);

			using (var stream = new FileStream (@"composed2_async.eml", FileMode.Create, FileAccess.Write))
			{
				await msg.SaveAsync (stream.AsBinaryDestination (), cancellationToken).ConfigureAwait (false);
			}
		}


		// создаём ответ на сообщение
		public static async Task MessageCreateReplyAsync (CancellationToken cancellationToken)
		{
			MailMessage message;
			using (var fs = new FileStream (@".\Pickup\test1.eml", FileMode.Open, FileAccess.Read))
			{
				message = new MailMessage ();
				await message.LoadAsync (fs.AsBufferedSource (new byte[1024]), EntityBodyFactory.Create, cancellationToken).ConfigureAwait (false);
			}

			var reply = message.CreateReply ();
			reply.GenerateId ();
			reply.From.Add ("noone@mailinator.com", "Иван Сидоров");
			reply.AddTextPart ("в ответ на ваше сообщение", Encoding.UTF8, "plain", ContentTransferEncoding.EightBit);

			using (var stream = new FileStream (@"composed3.eml", FileMode.Create, FileAccess.Write))
			{
				await reply.SaveAsync (stream.AsBinaryDestination (), cancellationToken).ConfigureAwait (false);
			}
		}
	}
}
