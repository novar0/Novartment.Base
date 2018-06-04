using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Novartment.Base.Collections;
using Novartment.Base.Collections.Linq;

namespace Novartment.Base.Data
{
	/// <summary>
	/// Действие, позволяюще удобно выполнять операции в БД без знания синтаксиса SQL и специфики конкретной СУБД.
	/// </summary>
	public class InvariantDbAction :
		IDbAction
	{
		private readonly IAdjustableCollection<InvariantDbCommandParameter> _parameters = new ArrayList<InvariantDbCommandParameter> ();
		private readonly IAdjustableCollection<InvariantDbCommandParameter> _plusParameters = new ArrayList<InvariantDbCommandParameter> ();
		private readonly IAdjustableCollection<InvariantDbCommandParameter> _keyParameters = new ArrayList<InvariantDbCommandParameter> ();
		private readonly IDbConnectionManager _connectionManager;
		private readonly ILogger _logger;

		/// <summary>
		/// Инициализирует новый экземпляр класса InvariantDbAction.
		/// </summary>
		/// <param name="connectionManager">IDbConnectionManager.</param>
		/// <param name="logger">Опциональный объект-журнал для записей о событиях. Укажите null если не требуется.</param>
		public InvariantDbAction (IDbConnectionManager connectionManager, ILogger logger = null)
		{
			if (connectionManager == null)
			{
				throw new ArgumentNullException (nameof (connectionManager));
			}

			Contract.EndContractBlock ();

			_connectionManager = connectionManager;
			_logger = logger;
			this.CommandTimeout = 30;
		}

		/// <summary>
		/// Получает или устанавливает предельное время выполнения действия (в секундах).
		/// </summary>
		public int CommandTimeout { get; set; }

		/// <summary>
		/// Добавляет параметр с указанным именем и значением.
		/// </summary>
		/// <param name="name">Имя параметра.</param>
		/// <param name="value">Значение параметра.</param>
		public void AddParameter (string name, object value)
		{
			if (name == null)
			{
				throw new ArgumentNullException (nameof (name));
			}

			Contract.EndContractBlock ();

			_parameters.Add (new InvariantDbCommandParameter (name, value));
		}

		/// <summary>
		/// Добавляет аккумулирующий с указанным именем и значением.
		/// </summary>
		/// <param name="name">Имя параметра.</param>
		/// <param name="value">Значение параметра.</param>
		public void AddAccumulationParameter (string name, object value)
		{
			if (name == null)
			{
				throw new ArgumentNullException (nameof (name));
			}

			Contract.EndContractBlock ();

			if ((value != null) && (value != DBNull.Value))
			{
				_plusParameters.Add (new InvariantDbCommandParameter (name, value));
			}
		}

		/// <summary>
		/// Добавляет ключевой параметр с указанным именем и значением.
		/// </summary>
		/// <param name="name">Имя параметра.</param>
		/// <param name="value">Значение параметра.</param>
		public void AddKeyParameter (string name, object value)
		{
			if (name == null)
			{
				throw new ArgumentNullException (nameof (name));
			}

			Contract.EndContractBlock ();

			if ((value == null) || (value == DBNull.Value))
			{
				throw new ArgumentOutOfRangeException (nameof (value));
			}

			_keyParameters.Add (new InvariantDbCommandParameter (name, value));
		}

		/// <summary>
		/// Выполняет выборку из указанной таблицы в указанной схеме.
		/// </summary>
		/// <param name="tableName">Имя таблицы.</param>
		/// <param name="schemaName">Имя схемы. Укажите null если схему указывать не нужно.</param>
		/// <returns>Считыватель данных, связанный с командой, которая должна быть освобождена вмесе с ним.</returns>
		public DisposableValueLinkedWithDbCommand<DbDataReader> SelectData (string tableName, string schemaName = null)
		{
			if (tableName == null)
			{
				throw new ArgumentNullException (nameof (tableName));
			}

			Contract.EndContractBlock ();

			var dbCommand = _connectionManager.CreateCommand (_parameters, false);
			dbCommand.CommandTimeout = this.CommandTimeout;
			string cmdText;
			if (_parameters.Count > 0)
			{
				var columnList = string.Join (" AND ", _parameters.Select (param => _connectionManager.FormatObjectName (param.Name) + "=" + param.Placeholder));
				cmdText = "SELECT * FROM " + _connectionManager.FormatObjectName (tableName, schemaName) + " WHERE " + columnList;
			}
			else
			{
				cmdText = "SELECT * FROM " + _connectionManager.FormatObjectName (tableName, schemaName);
			}

			_logger?.LogTrace (FormattableString.Invariant ($"Executing: {cmdText}"));
			dbCommand.CommandText = cmdText;
			var reader = dbCommand.ExecuteReader ();
			return new DisposableValueLinkedWithDbCommand<DbDataReader> (reader, dbCommand);
		}

		/// <summary>
		/// Выполняет вставку в указанную таблицу в указанной схеме.
		/// </summary>
		/// <param name="tableName">Имя таблицы.</param>
		/// <param name="schemaName">Имя схемы. Укажите null если схему указывать не нужно.</param>
		/// <returns>Уникальный идентификатор, присвоенный вставленной записи.</returns>
		public object Insert (string tableName, string schemaName = null)
		{
			if (tableName == null)
			{
				throw new ArgumentNullException (nameof (tableName));
			}

			Contract.EndContractBlock ();

			if ((_parameters.Count == 0) && (_keyParameters.Count == 0))
			{
				return null;
			}

			using (var dbCommand = _connectionManager.CreateCommand (_keyParameters.Concat (_parameters).Concat (_plusParameters), false))
			{
				dbCommand.CommandTimeout = this.CommandTimeout;
				var columnList = string.Join (
					",",
					_keyParameters.Select (param => _connectionManager.FormatObjectName (param.Name))
					.Concat (_parameters.Select (param => _connectionManager.FormatObjectName (param.Name)))
					.Concat (_plusParameters.Select (param => _connectionManager.FormatObjectName (param.Name))));

				var valueList = string.Join (
					",",
					_keyParameters.Select (param => param.Placeholder)
					.Concat (_parameters.Select (param => param.Placeholder))
					.Concat (_plusParameters.Select (param => param.Placeholder)));

				var cmdText = "INSERT INTO " + _connectionManager.FormatObjectName (tableName, schemaName) + " (" + columnList + ") VALUES (" + valueList + ")";
				_logger?.LogTrace (FormattableString.Invariant ($"Executing: {cmdText}"));
				dbCommand.CommandText = cmdText;
				dbCommand.ExecuteNonQuery ();
				return _connectionManager.GetLastIdentityValue ();
			}
		}

		/// <summary>
		/// Выполняет обновление в указанной таблице в указанной схеме.
		/// </summary>
		/// <param name="tableName">Имя таблицы.</param>
		/// <param name="schemaName">Имя схемы. Укажите null если схему указывать не нужно.</param>
		public void Update (string tableName, string schemaName = null)
		{
			if (tableName == null)
			{
				throw new ArgumentNullException (nameof (tableName));
			}

			Contract.EndContractBlock ();

			if ((_parameters.Count + _plusParameters.Count) == 0)
			{
				return;
			}

			using (var dbCommand = _connectionManager.CreateCommand (_parameters.Concat (_plusParameters).Concat (_keyParameters), false))
			{
				dbCommand.CommandTimeout = this.CommandTimeout;
				var columnList = string.Join (
					",",
					_parameters.Select (param => _connectionManager.FormatObjectName (param.Name) + "=" + param.Placeholder)
					.Concat (_plusParameters.Select (param => _connectionManager.FormatObjectName (param.Name) + "=" + _connectionManager.FormatObjectName (param.Name) + "+" + param.Placeholder)));

				var keyColumnList = string.Join (
					" AND ",
					_keyParameters.Select (param => _connectionManager.FormatObjectName (param.Name) + "=" + param.Placeholder));

				var cmdText = "UPDATE " + _connectionManager.FormatObjectName (tableName, schemaName) + " SET " + columnList + " WHERE " + keyColumnList;
				_logger?.LogTrace (FormattableString.Invariant ($"Executing: {cmdText}"));
				dbCommand.CommandText = cmdText;
				dbCommand.ExecuteNonQuery ();
			}
		}

		/// <summary>
		/// Выполняет действие обновление либо вставку (в зависимости от наличия записи) для указанной таблицы в указанной схеме.
		/// </summary>
		/// <param name="tableName">Имя таблицы.</param>
		/// <param name="schemaName">Имя схемы. Укажите null если схему указывать не нужно.</param>
		/// <returns>Уникальный идентификатор, присвоенный вставленной записи.</returns>
		public object UpdateInsert (string tableName, string schemaName = null)
		{
			if (tableName == null)
			{
				throw new ArgumentNullException (nameof (tableName));
			}

			Contract.EndContractBlock ();

			if (_keyParameters.Count == 0)
			{
				return null;
			}

			// неизвестно какой тип вернёт SelectCount, поэтому конвертируем его в максимально ёмкий тип double
			var existingRowsCount = Convert.ToDouble (SelectCount (tableName, schemaName), CultureInfo.InvariantCulture);

			// double нельзя сравнивать с точным значением (x == 0), поэтому сравниваем чтобы было меньше пограничного значения
			if (existingRowsCount < 0.5d)
			{
				Insert (tableName, schemaName); // надо вставлять
				return _connectionManager.GetLastIdentityValue ();
			}

			Update (tableName, schemaName); // надо обновить
			return null;
		}

		/// <summary>
		/// Выполняет выборку количества записей, выбранных из указанной таблицы в указанной схеме.
		/// </summary>
		/// <param name="tableName">Имя таблицы.</param>
		/// <param name="schemaName">Имя схемы. Укажите null если схему указывать не нужно.</param>
		/// <returns>Количества записей, выбранных из таблицы.</returns>
		public object SelectCount (string tableName, string schemaName = null)
		{
			if (tableName == null)
			{
				throw new ArgumentNullException (nameof (tableName));
			}

			Contract.EndContractBlock ();

			using (var dbCommand = _connectionManager.CreateCommand (_keyParameters, false))
			{
				dbCommand.CommandTimeout = this.CommandTimeout;
				var columnList = string.Join (
					" AND ",
					_keyParameters.Select (param => _connectionManager.FormatObjectName (param.Name) + "=" + param.Placeholder));

				var cmdText = "SELECT COUNT(*) FROM " + _connectionManager.FormatObjectName (tableName, schemaName) + " WHERE " + columnList;
				_logger?.LogTrace (FormattableString.Invariant ($"Executing: {cmdText}"));
				dbCommand.CommandText = cmdText;
				return dbCommand.ExecuteScalar ();
			}
		}

		/// <summary>
		/// Выполняет удаление для указанной таблицы в указанной схеме.
		/// </summary>
		/// <param name="tableName">Имя таблицы.</param>
		/// <param name="schemaName">Имя схемы. Укажите null если схему указывать не нужно.</param>
		public void Delete (string tableName, string schemaName = null)
		{
			if (tableName == null)
			{
				throw new ArgumentNullException (nameof (tableName));
			}

			Contract.EndContractBlock ();

			if (_keyParameters.Count == 0)
			{
				return;
			}

			using (var dbCommand = _connectionManager.CreateCommand (_keyParameters, false))
			{
				dbCommand.CommandTimeout = this.CommandTimeout;
				var columnList = string.Join (
					" AND ",
					_keyParameters.Select (param => _connectionManager.FormatObjectName (param.Name) + "=" + param.Placeholder));

				var cmdText = "DELETE " + _connectionManager.FormatObjectName (tableName, schemaName) + " WHERE " + columnList;
				_logger?.LogTrace (FormattableString.Invariant ($"Executing: {cmdText}"));
				dbCommand.CommandText = cmdText;
				dbCommand.ExecuteNonQuery ();
			}
		}

		/// <summary>
		/// Выполняет процедуру с указанным именем в указанной схеме.
		/// </summary>
		/// <param name="procedureName">Имя процедуры.</param>
		/// <param name="schemaName">Имя схемы. Укажите null если схему указывать не нужно.</param>
		public void ExecuteProcedure (string procedureName, string schemaName = null)
		{
			if (procedureName == null)
			{
				throw new ArgumentNullException (nameof (procedureName));
			}

			Contract.EndContractBlock ();

			_logger?.LogTrace (FormattableString.Invariant ($"Executing procedure: {procedureName}"));
			using (var dbCommand = _connectionManager.CreateCommand (_parameters, true))
			{
				dbCommand.CommandTimeout = this.CommandTimeout;
				dbCommand.CommandText = _connectionManager.FormatObjectName (procedureName, schemaName);
				dbCommand.CommandType = CommandType.StoredProcedure;
				dbCommand.ExecuteNonQuery ();
			}
		}

		/// <summary>
		/// Выполняет процедуру, возвращающую данные, с указанным именем в указанной схеме.
		/// </summary>
		/// <param name="procedureName">Имя процедуры.</param>
		/// <param name="schemaName">Имя схемы. Укажите null если схему указывать не нужно.</param>
		/// <returns>Считыватель данных, связанный с командой, которая должна быть освобождена вмесе с ним.</returns>
		public DisposableValueLinkedWithDbCommand<DbDataReader> ExecuteSelectProcedure (string procedureName, string schemaName = null)
		{
			if (procedureName == null)
			{
				throw new ArgumentNullException (nameof (procedureName));
			}

			Contract.EndContractBlock ();

			_logger?.LogTrace (FormattableString.Invariant ($"Executing procedure: {procedureName}"));
			var dbCommand = _connectionManager.CreateCommand (_parameters, true);
			dbCommand.CommandTimeout = this.CommandTimeout;
			dbCommand.CommandText = _connectionManager.FormatObjectName (procedureName, schemaName);
			dbCommand.CommandType = CommandType.StoredProcedure;
			var reader = dbCommand.ExecuteReader ();
			return new DisposableValueLinkedWithDbCommand<DbDataReader> (reader, dbCommand);
		}

		/// <summary>
		/// Выполняет функцию с указанным именем в указанной схеме.
		/// </summary>
		/// <param name="functionName">Имя функции.</param>
		/// <param name="schemaName">Имя схемы. Укажите null если схему указывать не нужно.</param>
		/// <returns>Результат, возвращённый функцией.</returns>
		public object ExecuteFunction (string functionName, string schemaName) // schemaName является обязательным для вызова функции
		{
			if (functionName == null)
			{
				throw new ArgumentNullException (nameof (functionName));
			}

			Contract.EndContractBlock ();

			_logger?.LogTrace (FormattableString.Invariant ($"Executing function: {functionName}"));
			using (var dbCommand = _connectionManager.CreateCommand (_parameters, false))
			{
				dbCommand.CommandTimeout = this.CommandTimeout;
				var paramList = string.Join (",", _parameters.Select (param => param.Placeholder));
				var cmdText = "SELECT " + _connectionManager.FormatObjectName (functionName, schemaName) + " (" + paramList + ")";
				dbCommand.CommandText = cmdText;
				dbCommand.CommandType = CommandType.Text;
				return dbCommand.ExecuteScalar ();
			}
		}
	}
}
