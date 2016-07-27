﻿using System.Threading;
using System.Text;
using static System.Linq.Enumerable;
using Xunit;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net.Mime.Test
{
	public class EntityExtensionsTests
	{
		[Fact, Trait ("Category", "Mime")]
		public void GetChildContentParts ()
		{
			var pkcs7Body = new DataEntityBody (ContentTransferEncoding.Base64);
			pkcs7Body.SetDataAsync (new ArrayBufferedSource (new byte[] { 20, 21, 22 }), CancellationToken.None).Wait ();

			var compositeBody = new CompositeEntityBody ();
			compositeBody.Parts.Add (new Entity (new TextEntityBody (Encoding.UTF8, ContentTransferEncoding.Binary), ContentMediaType.Text, "plain"));
			compositeBody.Parts.Add (new Entity (new TextEntityBody (Encoding.UTF8, ContentTransferEncoding.Binary), ContentMediaType.Text, "html"));

			var msgBody = new MessageEntityBody ();
			msgBody.Message = MailMessage.CreateSimpleText ("zip", Encoding.ASCII, ContentTransferEncoding.Binary);

			var singlePartBody = new DataEntityBody (ContentTransferEncoding.Base64);

			var rootBody = new CompositeEntityBody ();
			rootBody.Parts.Add (new Entity (pkcs7Body, ContentMediaType.Application, ApplicationMediaSubtypeNames.OctetStream));
			rootBody.Parts.Add (new Entity (compositeBody, ContentMediaType.Multipart, "alternative"));
			rootBody.Parts.Add (new Entity (msgBody, ContentMediaType.Message, MessageMediaSubtypeNames.Rfc822));
			rootBody.Parts.Add (new Entity (singlePartBody, ContentMediaType.Image, "png"));
			var rootEntity = new Entity (rootBody, ContentMediaType.Multipart, "mixed");

			var entities = rootEntity.GetChildContentParts (false, false);
			Assert.Equal (2, entities.Count);
			Assert.Equal (ContentMediaType.Application, entities[0].Type);
			Assert.Equal ("octet-stream", entities[0].Subtype);
			Assert.Equal (ContentMediaType.Image, entities[1].Type);
			Assert.Equal ("png", entities[1].Subtype);

			entities = rootEntity.GetChildContentParts (true, false);
			Assert.Equal (4, entities.Count);
			Assert.Equal (ContentMediaType.Application, entities[0].Type);
			Assert.Equal ("octet-stream", entities[0].Subtype);
			Assert.Equal (ContentMediaType.Text, entities[1].Type);
			Assert.Equal ("plain", entities[1].Subtype);
			Assert.Equal (ContentMediaType.Text, entities[2].Type);
			Assert.Equal ("html", entities[2].Subtype);
			Assert.Equal (ContentMediaType.Image, entities[3].Type);
			Assert.Equal ("png", entities[3].Subtype);

			entities = rootEntity.GetChildContentParts (true, true);
			Assert.Equal (5, entities.Count);
			Assert.Equal (ContentMediaType.Application, entities[0].Type);
			Assert.Equal ("octet-stream", entities[0].Subtype);
			Assert.Equal (ContentMediaType.Text, entities[1].Type);
			Assert.Equal ("plain", entities[1].Subtype);
			Assert.Equal (ContentMediaType.Text, entities[2].Type);
			Assert.Equal ("html", entities[2].Subtype);
			Assert.Equal (ContentMediaType.Text, entities[3].Type);
			Assert.Equal ("zip", entities[3].Subtype);
			Assert.Equal (ContentMediaType.Image, entities[4].Type);
			Assert.Equal ("png", entities[4].Subtype);
		}
	}
}
