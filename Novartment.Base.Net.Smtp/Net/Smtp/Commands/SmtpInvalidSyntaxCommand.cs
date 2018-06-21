namespace Novartment.Base.Net.Smtp
{
	internal class SmtpInvalidSyntaxCommand : SmtpCommand
	{
		internal SmtpInvalidSyntaxCommand (SmtpCommandType commandType, string message)
			: base (commandType)
		{
			this.Message = message;
		}

		internal string Message { get; }
	}
}
