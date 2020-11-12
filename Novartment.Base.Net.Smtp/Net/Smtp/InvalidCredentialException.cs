using System.Security;

namespace Novartment.Base.Net.Smtp
{
	internal sealed class InvalidCredentialException : SecurityException
	{
		public InvalidCredentialException ()
			: base ()
		{
		}
	}
}
