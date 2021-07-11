using Microsoft.Extensions.Logging;

namespace Novartment.Base
{
	/// <summary>
	/// Провайдер для создания журнала событий, пригодного для многопоточного асинхронного использования.
	/// </summary>
	public sealed class SimpleEventLogProvider :
		ILoggerProvider
	{
		/// <summary>
		/// Get the singleton instance of SimpleEventLog.
		/// </summary>
		public static readonly SimpleEventLog Logger = new ();

		/// <summary>
		/// Initializes a new instance of the SimpleEventLogProvider class.
		/// </summary>
		public SimpleEventLogProvider ()
		{
		}

		/// <summary>
		/// Returns a singleton instance of SimpleEventLog.
		/// </summary>
		/// <param name="categoryName">The category name not used.</param>
		/// <returns>The singleton instance of SimpleEventLog.</returns>
		public ILogger CreateLogger (string categoryName)
		{
			return Logger;
		}

		/// <summary>
		/// Does nothing.
		/// </summary>
		public void Dispose ()
		{
		}
	}
}
