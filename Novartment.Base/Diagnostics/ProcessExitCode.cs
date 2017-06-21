namespace Novartment.Base
{
	/// <summary>
	/// Причина завершения процесса.
	/// </summary>
	public enum ProcessExitCode : int
	{
		/// <summary>Нормальное завершение.</summary>
		Ok = 0,

		/// <summary>Необработанное исключение.</summary>
		UnhandledException = 1,

		/// <summary>Запрет запуска второго экземпляра уже запущенного приложения.</summary>
		AlreadyRunningInstanceDetected = 2,

		/// <summary>Указаны некорректные параметры в командной строке.</summary>
		UnknownArguments = 3,

		/// <summary>Нарушение прав доступа.</summary>
		AccessViolation = 4,

		/// <summary>Требуется перезапуск.</summary>
		NeedRestart = 5,

		/// <summary>Запрет запуска во время процесса установки.</summary>
		InstallationRunningLaunchNotAllowed = 6,
	}
}
