using System.Threading;

namespace Novartment.Base.Sample
{
	public static class Program
	{
		public static void Main ()
		{
			//SmtpSamples.SendOneMessageAsync (default).Wait ();
			SmtpSamples.StartSmtpServer ().Wait ();
			EbmlSamples.CreateChaptersFromMultipleMkvs (default).Wait ();
			RiffSamples.CreateChaptersFromMultipleAviAsync (default).Wait ();
			MimeSamples.MessageSaveAttachmentsAsync (default).Wait ();
			MimeSamples.MessageCreateSimpleAsync (default).Wait ();
			MimeSamples.MessageCreateCompositeAsync (default).Wait ();
			MimeSamples.MessageCreateReplyAsync (default).Wait ();
		}
	}
}
