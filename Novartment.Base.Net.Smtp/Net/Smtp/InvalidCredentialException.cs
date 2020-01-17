using System.Security;

namespace Novartment.Base.Net.Smtp
{
	internal class InvalidCredentialException : SecurityException
	{
		public InvalidCredentialException ()
			: base ()
		{
		}
	}
}
