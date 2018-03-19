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
		/// Получает экземпляр SimpleEventLog, в который записываются все события.
		/// </summary>
		public static readonly SimpleEventLog Logger = new SimpleEventLog ();

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
			return Logger;
		}

		/// <summary>
		/// Освобождает ресурсы, занятые в провайдере.
		/// </summary>
		public void Dispose ()
		{
		}
	}
}
