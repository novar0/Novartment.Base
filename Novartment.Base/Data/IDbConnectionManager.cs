using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace Novartment.Base.Data
{
	/// <summary>
	/// Подключение к базе данных.
	/// </summary>
	public interface IDbConnectionManager
	{
		/// <summary>
		/// Открывает подключение.
		/// </summary>
		void OpenConnection ();

		/// <summary>
		/// Закрывает подключение.
		/// </summary>
		void CloseConnection ();

		/// <summary>
		/// Начинает транзакцию.
		/// </summary>
		void BeginTransaction ();

		/// <summary>
		/// Подтверждает начатую транзакцию.
		/// </summary>
		void Commit ();

		/// <summary>
		/// Отменяет начатую транзакцию.
		/// </summary>
		void Rollback ();

		/// <summary>
		/// Создаёт команду с указанными параметрами.
		/// </summary>
		/// <param name="parameters">Параметры команды.</param>
		/// <param name="useRealParameterNames">Признак использования настоящих имён параметром.
		/// Если не указан, то будут использоваться номерные заполнители.</param>
		/// <returns>Созданная команда.</returns>
		DbCommand CreateCommand (IReadOnlyCollection<InvariantDbCommandParameter> parameters, bool useRealParameterNames);

		/// <summary>
		/// Создаёт действие для выполнения в подключении.
		/// </summary>
		/// <returns>Созданное действие для выполнения в подключении.</returns>
		IDbAction CreateAction ();

		/// <summary>
		/// Получает уникальный идентификатор, сгенерированный последней выполнявшейся командой.
		/// </summary>
		/// <returns>Уникальный идентификатор, сгенерированный последней выполнявшейся командой.</returns>
		[SuppressMessage ("Microsoft.Design",
			"CA1024:UsePropertiesWhereAppropriate",
			Justification = "The method performs a time-consuming operation and performs a conversion.")]
		object GetLastIdentityValue ();

		/// <summary>
		/// Форматирует указанное имя объекта из указанной схемы так, чтобы оно соответствовало ограничениям синтаксиса.
		/// </summary>
		/// <param name="objectName">Имя объекта.</param>
		/// <param name="schemaName">Имя схемы. Укажите null если схему указывать не нужно.</param>
		/// <returns>Сформатированное имя объекта.</returns>
		[SuppressMessage ("Microsoft.Design",
			"CA1026:DefaultParametersShouldNotBeUsed",
			Justification = "Parameter have clear right 'default' value and there is no plausible reason why the default might need to change.")]
		string FormatObjectName (string objectName, string schemaName = null);

		/// <summary>
		/// Получает текущее состояние подключения.
		/// </summary>
		ConnectionState ConnectionState { get; }
	}
}
