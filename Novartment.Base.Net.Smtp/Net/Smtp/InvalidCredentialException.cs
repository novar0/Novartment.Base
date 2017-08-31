using System.Security;

namespace Novartment.Base.Net.Smtp
{
#pragma warning disable CA1032 // Implement standard exception constructors
	internal class InvalidCredentialException : SecurityException
#pragma warning restore CA1032 // Implement standard exception constructors
	{
		public InvalidCredentialException ()
			: base ()
		{
		}
	}
}
