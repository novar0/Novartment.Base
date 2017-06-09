namespace Novartment.Base.Net.Smtp
{
	internal enum SmtpCommandType
	{
		Unknown = 0,
		Data = 1,
		Bdat = 2,
		Ehlo = 3,
		Helo = 4,
		MailFrom = 5,
		RcptTo = 6,
		Rset = 7,
		Vrfy = 8,
		Noop = 9,
		Quit = 10,
		ActualData = 11,
		StartTls = 12,
		Auth = 13,
		SaslResponse = 14,
		NoCommand = 255
	}
}
