using System.Data.Common;

namespace Novartment.Base.Data.SqlWrapper
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
		/// <param name="value">Значение параметра.</param>
		void AddParameter (string name, object value);

		/// <summary>
		/// Добавляет ключевой параметр с указанным именем и значением.
		/// </summary>
		/// <param name="name">Имя параметра.</param>
		/// <param name="value">Значение параметра.</param>
		void AddKeyParameter (string name, object value);

		/// <summary>
		/// Добавляет аккумулирующий параметр с указанным именем и значением.
		/// </summary>
		/// <param name="name">Имя параметра.</param>
		/// <param name="value">Значение параметра.</param>
		void AddAccumulationParameter (string name, object value);

		/// <summary>
		/// Выполняет выборку количества записей, выбранных из указанной таблицы в указанной схеме.
		/// </summary>
		/// <param name="tableName">Имя таблицы.</param>
		/// <param name="schemaName">Имя схемы. Укажите null если схему указывать не нужно.</param>
		/// <returns>Количества записей, выбранных из таблицы.</returns>
		object SelectCount (string tableName, string schemaName = null);

		/// <summary>
		/// Выполняет вставку в указанную таблицу в указанной схеме.
		/// </summary>
		/// <param name="tableName">Имя таблицы.</param>
		/// <param name="schemaName">Имя схемы. Укажите null если схему указывать не нужно.</param>
		/// <returns>Уникальный идентификатор, присвоенный вставленной записи.</returns>
		object Insert (string tableName, string schemaName = null);

		/// <summary>
		/// Выполняет обновление в указанной таблице в указанной схеме.
		/// </summary>
		/// <param name="tableName">Имя таблицы.</param>
		/// <param name="schemaName">Имя схемы. Укажите null если схему указывать не нужно.</param>
		void Update (string tableName, string schemaName = null);

		/// <summary>
		/// Выполняет действие обновление либо вставку (в зависимости от наличия записи) для указанной таблицы в указанной схеме.
		/// </summary>
		/// <param name="tableName">Имя таблицы.</param>
		/// <param name="schemaName">Имя схемы. Укажите null если схему указывать не нужно.</param>
		/// <returns>Уникальный идентификатор, присвоенный вставленной записи.</returns>
		object UpdateInsert (string tableName, string schemaName = null);

		/// <summary>
		/// Выполняет удаление для указанной таблицы в указанной схеме.
		/// </summary>
		/// <param name="tableName">Имя таблицы.</param>
		/// <param name="schemaName">Имя схемы. Укажите null если схему указывать не нужно.</param>
		void Delete (string tableName, string schemaName = null);

		/// <summary>
		/// Выполняет процедуру с указанным именем в указанной схеме.
		/// </summary>
		/// <param name="procedureName">Имя процедуры.</param>
		/// <param name="schemaName">Имя схемы. Укажите null если схему указывать не нужно.</param>
		void ExecuteProcedure (string procedureName, string schemaName = null);

		/// <summary>
		/// Выполняет функцию с указанным именем в указанной схеме.
		/// </summary>
		/// <param name="functionName">Имя функции.</param>
		/// <param name="schemaName">Имя схемы. Укажите null если схему указывать не нужно.</param>
		/// <returns>Результат, возвращённый функцией.</returns>
		object ExecuteFunction (string functionName, string schemaName = null);

		/// <summary>
		/// Выполняет выборку из указанной таблицы в указанной схеме.
		/// </summary>
		/// <param name="tableName">Имя таблицы.</param>
		/// <param name="schemaName">Имя схемы. Укажите null если схему указывать не нужно.</param>
		/// <returns>Считыватель данных, связанный с командой, которая должна быть освобождена вмесе с ним.</returns>
		DisposableValueLinkedWithDbCommand<DbDataReader> SelectData (string tableName, string schemaName = null);

		/// <summary>
		/// Выполняет процедуру, возвращающую данные, с указанным именем в указанной схеме.
		/// </summary>
		/// <param name="procedureName">Имя процедуры.</param>
		/// <param name="schemaName">Имя схемы. Укажите null если схему указывать не нужно.</param>
		/// <returns>Считыватель данных, связанный с командой, которая должна быть освобождена вмесе с ним.</returns>
		/// <remarks>
		/// Создает сразу два объекта, подлежащих освобождению после использования (IDbCommand и IDataReader)
		/// поэтому возращают обёртку, при особождении которой освобождаются оба созданных объекта.
		/// </remarks>
		DisposableValueLinkedWithDbCommand<DbDataReader> ExecuteSelectProcedure (string procedureName, string schemaName = null);
	}
}
