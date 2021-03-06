using System;
using System.Security;
using System.Security.Principal;
using Novartment.Base.Collections;

namespace Novartment.Base
{
	/// <summary>
	/// Методы получения описания текущего окружения.
	/// </summary>
	public static class WindowsEnvironmentDescription
	{
		/// <summary>
		/// Создаёт строковое описание окружения,
		/// включающее версию ОС и версию среды выполнения.
		/// </summary>
		/// <returns>Строковое описание окружения.</returns>
		public static string Framework =>
			FormattableString.Invariant (
				$"{Environment.OSVersion} {(Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit")} CLR {Environment.Version}");

		/// <summary>
		/// Создаёт строковое описание текущего пользователя,
		/// включающее логин и членство в основных группах.
		/// </summary>
		/// <returns>Строковое описание текущего пользователя.</returns>
		public static string CurrentUser
		{
			get
			{
				WindowsIdentity user = null;
				try
				{
					user = WindowsIdentity.GetCurrent ();
				}
				catch (SecurityException)
				{
				}

				if (user == null)
				{
					user = WindowsIdentity.GetAnonymous ();
				}

				var principal = new WindowsPrincipal (user);
				var roles = new ArrayList<string> (3);
				var isAdmin = principal.IsInRole (WindowsBuiltInRole.Administrator);
				if (isAdmin)
				{
					roles.Add ("Administrator");
				}

				var isUser = principal.IsInRole (WindowsBuiltInRole.User);
				if (isUser)
				{
					roles.Add ("User");
				}

				var isGuest = principal.IsInRole (WindowsBuiltInRole.Guest);
				if (isGuest)
				{
					roles.Add ("Guest");
				}

				return
					(user.IsAnonymous ? "<Anonymous>" : (user.IsGuest ? "<Guest>" : (user.IsSystem ? "<System>" : user.Name)))
					+ " (" + string.Join (",", roles) + ")";
			}
		}
	}
}
