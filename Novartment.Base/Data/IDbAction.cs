using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace Novartment.Base.Data
{
	/// <summary>
	/// Действие, позволяюще удобно выполнять операции в БД без знания синтаксиса SQL и специфики конкретной СУБД.
	/// </summary>
	public interface IDbAction
	{
		/// <summary>
		/// Получает или устанавливает предельное время выполнения действия (в секундах).
		/// </summary>
		int CommandTimeout { get; set; }

		/// <summary>
		/// Добавляет параметр с указанным именем и значением.
		/// </summary>
		/// <param name="name">Имя параметра.</param>
		/// <param name="value">Значение параметра</param>
		void AddParameter (string name, object value);

		/// <summary>
		/// Добавляет ключевой параметр с указанным именем и значением.
		/// </summary>
		/// <param name="name">Имя параметра.</param>
		/// <param name="value">Значение параметра</param>
		void AddKeyParameter (string name, object value);

		/// <summary>
		/// Добавляет аккумулирующий с указанным именем и значением.
		/// </summary>
		/// <param name="name">Имя параметра.</param>
		/// <param name="value">Значение параметра</param>
		void AddAccumulationParameter (string name, object value);

		/// <summary>
		/// Выполняет выборку количества записей, выбранных из указанной таблицы в указанной схеме.
		/// </summary>
		/// <param name="tableName">Имя таблицы.</param>
		/// <param name="schemaName">Имя схемы. Укажите null если схему указывать не нужно.</param>
		/// <returns>Количества записей, выбранных из таблицы.</returns>
		[SuppressMessage (
		"Microsoft.Design",
			"CA1026:DefaultParametersShouldNotBeUsed",
			Justification = "Parameter have clear right 'default' value and there is no plausible reason why the default might need to change.")]
		object SelectCount (string tableName, string schemaName = null);

		/// <summary>
		/// Выполняет вставку в указанную таблицу в указанной схеме.
		/// </summary>
		/// <param name="tableName">Имя таблицы.</param>
		/// <param name="schemaName">Имя схемы. Укажите null если схему указывать не нужно.</param>
		/// <returns>Уникальный идентификатор, присвоенный вставленной записи.</returns>
		[SuppressMessage (
		"Microsoft.Design",
			"CA1026:DefaultParametersShouldNotBeUsed",
			Justification = "Parameter have clear right 'default' value and there is no plausible reason why the default might need to change.")]
		object Insert (string tableName, string schemaName = null);

		/// <summary>
		/// Выполняет обновление в указанной таблице в указанной схеме.
		/// </summary>
		/// <param name="tableName">Имя таблицы.</param>
		/// <param name="schemaName">Имя схемы. Укажите null если схему указывать не нужно.</param>
		[SuppressMessage (
		"Microsoft.Design",
			"CA1026:DefaultParametersShouldNotBeUsed",
			Justification = "Parameter have clear right 'default' value and there is no plausible reason why the default might need to change.")]
		void Update (string tableName, string schemaName = null);

		/// <summary>
		/// Выполняет действие обновление либо вставку (в зависимости от наличия записи) для указанной таблицы в указанной схеме.
		/// </summary>
		/// <param name="tableName">Имя таблицы.</param>
		/// <param name="schemaName">Имя схемы. Укажите null если схему указывать не нужно.</param>
		/// <returns>Уникальный идентификатор, присвоенный вставленной записи.</returns>
		[SuppressMessage (
		"Microsoft.Design",
			"CA1026:DefaultParametersShouldNotBeUsed",
			Justification = "Parameter have clear right 'default' value and there is no plausible reason why the default might need to change.")]
		object UpdateInsert (string tableName, string schemaName = null);

		/// <summary>
		/// Выполняет удаление для указанной таблицы в указанной схеме.
		/// </summary>
		/// <param name="tableName">Имя таблицы.</param>
		/// <param name="schemaName">Имя схемы. Укажите null если схему указывать не нужно.</param>
		[SuppressMessage (
		"Microsoft.Design",
			"CA1026:DefaultParametersShouldNotBeUsed",
			Justification = "Parameter have clear right 'default' value and there is no plausible reason why the default might need to change.")]
		void Delete (string tableName, string schemaName = null);

		/// <summary>
		/// Выполняет процедуру с указанным именем в указанной схеме.
		/// </summary>
		/// <param name="procedureName">Имя процедуры.</param>
		/// <param name="schemaName">Имя схемы. Укажите null если схему указывать не нужно.</param>
		[SuppressMessage (
		"Microsoft.Design",
			"CA1026:DefaultParametersShouldNotBeUsed",
			Justification = "Parameter have clear right 'default' value and there is no plausible reason why the default might need to change.")]
		void ExecuteProcedure (string procedureName, string schemaName = null);

		/// <summary>
		/// Выполняет функцию с указанным именем в указанной схеме.
		/// </summary>
		/// <param name="functionName">Имя функции.</param>
		/// <param name="schemaName">Имя схемы. Укажите null если схему указывать не нужно.</param>
		/// <returns>Результат, возвращённый функцией.</returns>
		[SuppressMessage (
		"Microsoft.Design",
			"CA1026:DefaultParametersShouldNotBeUsed",
			Justification = "Parameter have clear right 'default' value and there is no plausible reason why the default might need to change.")]
		object ExecuteFunction (string functionName, string schemaName = null);

		/// <summary>
		/// Выполняет выборку из указанной таблицы в указанной схеме.
		/// </summary>
		/// <param name="tableName">Имя таблицы.</param>
		/// <param name="schemaName">Имя схемы. Укажите null если схему указывать не нужно.</param>
		/// <returns>Считыватель данных, связанный с командой, которая должна быть освобождена вмесе с ним.</returns>
		[SuppressMessage (
		"Microsoft.Design",
			"CA1026:DefaultParametersShouldNotBeUsed",
			Justification = "Parameter have clear right 'default' value and there is no plausible reason why the default might need to change.")]
		DisposableValueLinkedWithDbCommand<DbDataReader> SelectData (string tableName, string schemaName = null);

		/// <summary>
		/// Выполняет процедуру, возвращающую данные, с указанным именем в указанной схеме.
		/// </summary>
		/// <param name="procedureName">Имя процедуры.</param>
		/// <param name="schemaName">Имя схемы. Укажите null если схему указывать не нужно.</param>
		/// <returns>Считыватель данных, связанный с командой, которая должна быть освобождена вмесе с ним.</returns>
		[SuppressMessage (
		"Microsoft.Design",
			"CA1026:DefaultParametersShouldNotBeUsed",
			Justification = "Parameter have clear right 'default' value and there is no plausible reason why the default might need to change.")]

		// создает сразу два объекта, подлежащих освобождению после использования (IDbCommand и IDataReader)
		// поэтому возращают обёртку, при особождении которой освобождаются оба созданных объекта
		DisposableValueLinkedWithDbCommand<DbDataReader> ExecuteSelectProcedure (string procedureName, string schemaName = null);
	}
}
