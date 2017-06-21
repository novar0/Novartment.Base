using System.Threading;

namespace Novartment.Base.Sample
{
	public class Program
	{
		public static void Main (string[] args)
		{
			SmtpSamples.SendOneMessageAsync (CancellationToken.None).Wait ();
			SmtpSamples.StartSmtpServer ().Wait ();
			EbmlSamples.CreateChaptersFromMultipleMkvs (CancellationToken.None).Wait ();
			RiffSamples.CreateChaptersFromMultipleAviAsync (CancellationToken.None).Wait ();
			MimeSamples.MessageSaveAttachmentsAsync (CancellationToken.None).Wait ();
			MimeSamples.MessageCreateSimpleAsync (CancellationToken.None).Wait ();
			MimeSamples.MessageCreateCompositeAsync (CancellationToken.None).Wait ();
			MimeSamples.MessageCreateReplyAsync (CancellationToken.None).Wait ();
		}
	}
}
