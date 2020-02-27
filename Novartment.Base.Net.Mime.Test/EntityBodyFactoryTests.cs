using Xunit;

namespace Novartment.Base.Net.Mime.Test
{
	public class EntityBodyFactoryTests
	{
		[Fact]
		[Trait ("Category", "Mime")]
		public void Create ()
		{
			var props = new EssentialContentProperties ()
			{
				MediaType = ContentMediaType.Application,
				MediaSubtype = "octet-stream",
				TransferEncoding = ContentTransferEncoding.EightBit,
			};
			Assert.IsAssignableFrom<IDiscreteEntityBody> (EntityBodyFactory.Create (props));

			props = new EssentialContentProperties ()
			{
				MediaType = ContentMediaType.Audio,
				MediaSubtype = "aaa",
				TransferEncoding = ContentTransferEncoding.EightBit,
			};
			Assert.IsAssignableFrom<IDiscreteEntityBody> (EntityBodyFactory.Create (props));

			props = new EssentialContentProperties ()
			{
				MediaType = ContentMediaType.Image,
				MediaSubtype = "aaa",
				TransferEncoding = ContentTransferEncoding.EightBit,
			};
			Assert.IsAssignableFrom<IDiscreteEntityBody> (EntityBodyFactory.Create (props));

			props = new EssentialContentProperties ()
			{
				MediaType = ContentMediaType.Video,
				MediaSubtype = "aaa",
				TransferEncoding = ContentTransferEncoding.EightBit,
			};
			Assert.IsAssignableFrom<IDiscreteEntityBody> (EntityBodyFactory.Create (props));

			props = new EssentialContentProperties ()
			{
				MediaType = ContentMediaType.Text,
				MediaSubtype = "html",
				TransferEncoding = ContentTransferEncoding.EightBit,
			};
			props.Parameters.Add (new HeaderFieldBodyParameter ("charset", "utf-8"));
			Assert.IsType<TextEntityBody> (EntityBodyFactory.Create (props));

			props = new EssentialContentProperties ()
			{
				MediaType = ContentMediaType.Multipart,
				MediaSubtype = "mixed",
				TransferEncoding = ContentTransferEncoding.EightBit,
			};
			props.Parameters.Add (new HeaderFieldBodyParameter ("boundary", "--xxxx"));
			Assert.IsAssignableFrom<ICompositeEntityBody> (EntityBodyFactory.Create (props));

			props = new EssentialContentProperties ()
			{
				MediaType = ContentMediaType.Multipart,
				MediaSubtype = "digest",
				TransferEncoding = ContentTransferEncoding.EightBit,
			};
			props.Parameters.Add (new HeaderFieldBodyParameter ("boundary", "--xxxx"));
			Assert.IsType<DigestEntityBody> (EntityBodyFactory.Create (props));

			props = new EssentialContentProperties ()
			{
				MediaType = ContentMediaType.Multipart,
				MediaSubtype = "report",
				TransferEncoding = ContentTransferEncoding.EightBit,
			};
			props.Parameters.Add (new HeaderFieldBodyParameter ("boundary", "--xxxx"));
			props.Parameters.Add (new HeaderFieldBodyParameter ("report-type", "xxxx"));
			Assert.IsType<ReportEntityBody> (EntityBodyFactory.Create (props));

			props = new EssentialContentProperties ()
			{
				MediaType = ContentMediaType.Message,
				MediaSubtype = "rfc822",
				TransferEncoding = ContentTransferEncoding.EightBit,
			};
			Assert.IsType<MessageEntityBody> (EntityBodyFactory.Create (props));

			props = new EssentialContentProperties ()
			{
				MediaType = ContentMediaType.Message,
				MediaSubtype = "delivery-status",
				TransferEncoding = ContentTransferEncoding.EightBit,
			};
			Assert.IsType<DeliveryStatusEntityBody> (EntityBodyFactory.Create (props));

			props = new EssentialContentProperties ()
			{
				MediaType = ContentMediaType.Message,
				MediaSubtype = "disposition-notification",
				TransferEncoding = ContentTransferEncoding.EightBit,
			};
			Assert.IsType<DispositionNotificationEntityBody> (EntityBodyFactory.Create (props));
		}
	}
}
