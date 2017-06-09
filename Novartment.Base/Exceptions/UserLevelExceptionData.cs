using System;
using System.Diagnostics.Contracts;

namespace Novartment.Base
{
	/// <summary>
	/// Информация для отображения пользователю подробностей об исключении.
	/// </summary>
	/// <remarks>
	/// Дополнительно к ExceptionDescription содержит:
	/// имя и версию сборки в которой произошло исключение,
	/// производимое программой действие, приведшее к исключению и
	/// действия, рекомендуемые пользователю.
	/// </remarks>
	public class UserLevelExceptionData
	{
		/// <summary>
		/// Инициализирует новый экземпляр UserLevelExceptionData на основе описаиня исключения, указанной сборки и действий.
		/// </summary>
		/// <param name="exceptionDescription">Описание исключения.</param>
		/// <param name="failedUser">Пользователь, который вызвал исключение.</param>
		/// <param name="failedFramework">Платформа, в которой вызвано исключение.</param>
		/// <param name="failedAssemblyName">Сборка, которая вызвала исключение.</param>
		/// <param name="failedAssemblyVersion">Версия сборки, которая вызвала исключение.</param>
		/// <param name="failedAction">Действие, которое вызвало исключение.</param>
		/// <param name="recommendedSolution">Рекомендуемые пользователю действия.</param>
		public UserLevelExceptionData(
			ExceptionDescription exceptionDescription,
			string failedUser,
			string failedFramework,
			string failedAssemblyName,
			string failedAssemblyVersion,
			string failedAction,
			string recommendedSolution)
		{
			if (exceptionDescription == null)
			{
				throw new ArgumentNullException(nameof(exceptionDescription));
			}

			Contract.EndContractBlock();

			this.Exception = exceptionDescription;
			this.FailedUser = failedUser;
			this.FailedFramework = failedFramework;
			this.FailedAssemblyName = failedAssemblyName;
			this.FailedAssemblyVersion = failedAssemblyVersion;
			this.FailedAction = failedAction;
			this.RecommendedSolution = recommendedSolution;
		}

		/// <summary>
		/// Описание исключения.
		/// </summary>
		public ExceptionDescription Exception { get; }

		/// <summary>
		/// Пользователь, который вызвал исключение.
		/// </summary>
		public string FailedUser { get; }

		/// <summary>
		/// Платформа, в которой вызвано исключение.
		/// </summary>
		public string FailedFramework { get; }

		/// <summary>
		/// Сборка, которая вызвала исключение.
		/// </summary>
		public string FailedAssemblyName { get; }

		/// <summary>
		/// Версия сборки, которая вызвала исключение.
		/// </summary>
		public string FailedAssemblyVersion { get; }

		/// <summary>
		/// Действие, которое вызвало исключение.
		/// </summary>
		public string FailedAction { get; }

		/// <summary>
		/// Рекомендуемые пользователю действия.
		/// </summary>
		public string RecommendedSolution { get; }
	}
}
