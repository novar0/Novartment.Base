namespace Novartment.Base.Net.Smtp
{
	internal readonly struct SmtpReplyWithGroupingMark
	{
		internal SmtpReplyWithGroupingMark (SmtpReply reply, bool canBeGrouped)
		{
			this.Reply = reply;
			this.CanBeGrouped = canBeGrouped;
		}

		internal SmtpReply Reply { get; }

		internal bool CanBeGrouped { get; }
	}
}
