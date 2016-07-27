
namespace Novartment.Base
{
	/// <summary>
	/// Уровень детализации информации.
	/// </summary>
	public enum LogLevel : int
	{
		/// <summary>Severe errors that cause premature termination. Expect these to be immediately visible on a status console.</summary>
		Fatal = 0,

		/// <summary>Other runtime errors or unexpected conditions. Expect these to be immediately visible on a status console.</summary>
		Error = 1,

		/// <summary>Use of deprecated APIs, poor use of API, 'almost' errors, other runtime situations that are undesirable or unexpected,
		/// but not necessarily "wrong". Expect these to be immediately visible on a status console.</summary>
		Warn = 2,

		/// <summary>Interesting runtime events (startup/shutdown). Expect these to be immediately visible on a console,
		/// so be conservative and keep to a minimum.</summary>
		Info = 3,

		/// <summary>Detailed information on the flow through the system. Expect these to be written to logs only.</summary>
		Debug = 4,

		/// <summary>Most detailed information. Expect these to be written to logs only.</summary>
		Trace = 5
	}
}
