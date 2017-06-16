using Microsoft.Extensions.Logging;

namespace Novartment.Base
{
	/// <summary>
	/// Провайдер для создания журнала событий, пригодного для многопоточного асинхронного использования.
	/// </summary>
	public class SimpleEventLogProvider :
		ILoggerProvider
	{
		/// <summary>
		/// Инициализирует новый экземпляр SimpleEventLogProvider.
		/// </summary>
		public SimpleEventLogProvider ()
		{
		}

		/// <summary>
		/// Создаёт экземпляр журнала.
		/// </summary>
		/// <param name="categoryName">Имя категории для событий журнала.</param>
		/// <returns>Созданный экземпляр журнала.</returns>
		public ILogger CreateLogger (string categoryName)
		{
			return new SimpleEventLog ();
		}

		/// <summary>
		/// Освобождает ресурсы, занятые в провайдере.
		/// </summary>
		public void Dispose ()
		{
		}
	}
}
