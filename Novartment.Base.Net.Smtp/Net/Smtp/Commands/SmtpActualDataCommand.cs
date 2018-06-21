using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net.Smtp
{
	internal class SmtpActualDataCommand : SmtpCommand
	{
		internal SmtpActualDataCommand (IBufferedSource source, bool throwIfEndMarkerNotFound)
			: base (SmtpCommandType.ActualData)
		{
			this.Source = new TemplateSeparatedBufferedSource (
				source,
				new byte[] { 0x0d, 0x0a, (byte)'.', 0x0d, 0x0a },
				throwIfEndMarkerNotFound);
		}

		internal TemplateSeparatedBufferedSource Source { get; }
	}
}
