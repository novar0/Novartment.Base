using System;
using System.Diagnostics.CodeAnalysis;

namespace Novartment.Base
{
	/// <summary>
	/// Останавливаемый/перезапускаемый таймер с изменяемым интервалом срабатывания.
	/// </summary>
	public interface ITimer :
		IDisposable
	{
		/// <summary>
		/// Получает или устанавливает интервал срабатывания таймера.
		/// </summary>
		TimeSpan Interval { get; set; }

		/// <summary>
		/// Получает состояние таймера. True если таймер запущен, иначе False.
		/// </summary>
		bool Enabled { get; }

		/// <summary>
		/// Запускает таймер.
		/// </summary>
		void Start ();

		/// <summary>
		/// Останавливает таймер.
		/// </summary>
		[SuppressMessage ("Microsoft.Naming",
			"CA1716:IdentifiersShouldNotMatchKeywords",
			MessageId = "Stop",
			Justification = "No other name could be applied. All library timers have method 'Stop()'.")]
		void Stop ();
	}
}
