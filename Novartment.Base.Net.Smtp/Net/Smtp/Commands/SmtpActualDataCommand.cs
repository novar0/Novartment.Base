using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net.Smtp
{
	internal class SmtpActualDataCommand : SmtpCommand
	{
		internal SmtpActualDataCommand ()
			: base (SmtpCommandType.ActualData)
		{
		}

		internal TemplateSeparatedBufferedSource Source { get; private set; }

		internal void SetSource (IBufferedSource source, bool throwIfEndMarkerNotFound)
		{
			this.Source = new TemplateSeparatedBufferedSource (
				source,
				new byte[] { 0x0d, 0x0a, (byte)'.', 0x0d, 0x0a },
				throwIfEndMarkerNotFound);
		}
	}
}
