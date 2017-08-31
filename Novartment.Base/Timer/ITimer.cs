using System;

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

#pragma warning disable CA1716 // Identifiers should not match keywords
		/// <summary>
		/// Останавливает таймер.
		/// </summary>
		void Stop ();
#pragma warning restore CA1716 // Identifiers should not match keywords
	}
}
