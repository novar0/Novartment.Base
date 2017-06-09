namespace Novartment.Base.Net.Smtp
{
	internal struct SmtpReplyWithGroupingMark
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
