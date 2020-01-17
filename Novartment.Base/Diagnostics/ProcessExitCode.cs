namespace Novartment.Base
{
	/// <summary>
	/// Reason for the end of the process.
	/// </summary>
	public enum ProcessExitCode : int
	{
		/// <summary>Normal exit.</summary>
		Ok = 0,

		/// <summary>Unhandled exception.</summary>
		UnhandledException = 1,

		/// <summary>Prevents the second instance of an already running application from running.</summary>
		AlreadyRunningInstanceDetected = 2,

		/// <summary>Incorrect parameters are specified in the command line.</summary>
		UnknownArguments = 3,

		/// <summary>Access violation.</summary>
		AccessViolation = 4,

		/// <summary>Restart required.</summary>
		NeedRestart = 5,

		/// <summary>Disable startup during the installation process.</summary>
		InstallationRunningLaunchNotAllowed = 6,
	}
}
