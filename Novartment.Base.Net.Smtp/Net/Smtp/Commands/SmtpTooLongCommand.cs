namespace Novartment.Base.Net.Smtp
{
	internal sealed class SmtpTooLongCommand : SmtpCommand
	{
		internal SmtpTooLongCommand ()
			: base (SmtpCommandType.Unknown)
		{
		}
	}
}
