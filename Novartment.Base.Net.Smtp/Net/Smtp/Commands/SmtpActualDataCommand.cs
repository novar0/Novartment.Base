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
				SmtpCommand.MessageEndMarker,
				throwIfEndMarkerNotFound);
		}
	}
}
