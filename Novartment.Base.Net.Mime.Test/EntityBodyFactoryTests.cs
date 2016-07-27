using Xunit;

namespace Novartment.Base.Net.Mime.Test
{
	public class EntityBodyFactoryTests
	{
		[Fact, Trait ("Category", "Mime")]
		public void Create ()
		{
			var props = new EssentialContentProperties ()
			{
				Type = ContentMediaType.Application,
				Subtype = "octet-stream",
				TransferEncoding = ContentTransferEncoding.EightBit
			};
			Assert.IsAssignableFrom<IDiscreteEntityBody> (EntityBodyFactory.Create (props));

			props = new EssentialContentProperties ()
			{
				Type = ContentMediaType.Audio,
				Subtype = "aaa",
				TransferEncoding = ContentTransferEncoding.EightBit
			};
			Assert.IsAssignableFrom<IDiscreteEntityBody> (EntityBodyFactory.Create (props));

			props = new EssentialContentProperties ()
			{
				Type = ContentMediaType.Image,
				Subtype = "aaa",
				TransferEncoding = ContentTransferEncoding.EightBit
			};
			Assert.IsAssignableFrom<IDiscreteEntityBody> (EntityBodyFactory.Create (props));

			props = new EssentialContentProperties ()
			{
				Type = ContentMediaType.Video,
				Subtype = "aaa",
				TransferEncoding = ContentTransferEncoding.EightBit
			};
			Assert.IsAssignableFrom<IDiscreteEntityBody> (EntityBodyFactory.Create (props));

			props = new EssentialContentProperties ()
			{
				Type = ContentMediaType.Text,
				Subtype = "html",
				TransferEncoding = ContentTransferEncoding.EightBit
			};
			props.Parameters.Add (new HeaderFieldParameter ("charset", "utf-8"));
			Assert.IsType<TextEntityBody> (EntityBodyFactory.Create (props));

			props = new EssentialContentProperties ()
			{
				Type = ContentMediaType.Multipart,
				Subtype = "mixed",
				TransferEncoding = ContentTransferEncoding.EightBit
			};
			props.Parameters.Add (new HeaderFieldParameter ("boundary", "--xxxx"));
			Assert.IsAssignableFrom<ICompositeEntityBody> (EntityBodyFactory.Create (props));

			props = new EssentialContentProperties ()
			{
				Type = ContentMediaType.Multipart,
				Subtype = "digest",
				TransferEncoding = ContentTransferEncoding.EightBit
			};
			props.Parameters.Add (new HeaderFieldParameter ("boundary", "--xxxx"));
			Assert.IsType<DigestEntityBody> (EntityBodyFactory.Create (props));

			props = new EssentialContentProperties ()
			{
				Type = ContentMediaType.Multipart,
				Subtype = "report",
				TransferEncoding = ContentTransferEncoding.EightBit
			};
			props.Parameters.Add (new HeaderFieldParameter ("boundary", "--xxxx"));
			props.Parameters.Add (new HeaderFieldParameter ("report-type", "xxxx"));
			Assert.IsType<ReportEntityBody> (EntityBodyFactory.Create (props));

			props = new EssentialContentProperties ()
			{
				Type = ContentMediaType.Message,
				Subtype = "rfc822",
				TransferEncoding = ContentTransferEncoding.EightBit
			};
			Assert.IsType<MessageEntityBody> (EntityBodyFactory.Create (props));

			props = new EssentialContentProperties ()
			{
				Type = ContentMediaType.Message,
				Subtype = "delivery-status",
				TransferEncoding = ContentTransferEncoding.EightBit
			};
			Assert.IsType<DeliveryStatusEntityBody> (EntityBodyFactory.Create (props));

			props = new EssentialContentProperties ()
			{
				Type = ContentMediaType.Message,
				Subtype = "disposition-notification",
				TransferEncoding = ContentTransferEncoding.EightBit
			};
			Assert.IsType<DispositionNotificationEntityBody> (EntityBodyFactory.Create (props));
		}
	}
}
