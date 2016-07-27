using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Data
{
	/// <summary>
	/// Подключение к базе данных.
	/// </summary>
	[SuppressMessage (
		"Microsoft.Design",
		"CA1063:ImplementIDisposableCorrectly",
		Justification = "Implemented correctly.")]
	public class InvariantDbConnectionManager :
		IDbConnectionManager,
		IDisposable
	{
		private readonly DbConnection _workDbConnection;
		private readonly string _namePlaceholder1;
		private readonly string _namePlaceholder2;
		private readonly string _parameterPlaceholder;
		private readonly string _lastIdentityStatement;
		private readonly int _commandTimeOut;
		private readonly IDictionary<DbType, DbType> _dbTypeReplacements = new Dictionary<DbType, DbType> ();

		private readonly object _connectionLocker = new object ();
		private readonly object _transactionLocker = new object ();

		private int _globalParamNumber; // номера параметров для sql server должны быть уникальными в пределах транзакции
		private readonly ReusableDisposable<DbTransaction> _activeTransaction = new ReusableDisposable<DbTransaction> ();
		private readonly ILogWriter _logger;

		/// <summary>
		/// Инициализирует новый экземпляр класса InvariantDbConnectionManager с указанными параметрами.
		/// </summary>
		/// <param name="factory">Провайдер.</param>
		/// <param name="connectionString">Строка подключения.</param>
		/// <param name="commandTimeout">Время (в секундах) ожидания выполнения команды перед тем как она будет прервана с ошибкой.</param>
		/// <param name="logger">Опциональный объект-журнал для записей о событиях. Укажите null если не требуется.</param>
		[SuppressMessage ("Microsoft.Design",
			"CA1026:DefaultParametersShouldNotBeUsed",
			Justification = "Parameter have clear right 'default' value and there is no plausible reason why the default might need to change.")]
		public InvariantDbConnectionManager (DbProviderFactory factory, string connectionString, int commandTimeout, ILogWriter logger = null)
		{
			if (factory == null)
			{
				throw new ArgumentNullException (nameof (factory));
			}
			if (connectionString == null)
			{
				throw new ArgumentNullException (nameof (connectionString));
			}
			if (commandTimeout < 0)
			{
				throw new ArgumentOutOfRangeException (nameof (commandTimeout));
			}
			Contract.EndContractBlock ();

			var factoryTypeName = factory.GetType ().FullName;
			switch (factoryTypeName)
			{
				case "System.Data.SqlServerCe.SqlCeProviderFactory":
				case "System.Data.SqlClient.SqlClientFactory":
					_namePlaceholder1 = "[{0}]";
					_namePlaceholder2 = "[{1}].[{0}]";
					_parameterPlaceholder = "@par{0:G}";
					_lastIdentityStatement = "SELECT @@IDENTITY";
					_dbTypeReplacements.Add (DbType.UInt16, DbType.Int16);
					_dbTypeReplacements.Add (DbType.UInt32, DbType.Int32);
					_dbTypeReplacements.Add (DbType.UInt64, DbType.Int64);
					break;
				case "Oracle.DataAccess.Client.OracleClientFactory":
				case "System.Data.OracleClient.OracleClientFactory":
					_namePlaceholder1 = "\"{0}\"";
					_namePlaceholder2 = "\"{1}\".\"{0}\"";
					_parameterPlaceholder = ":{0:G}";
					_lastIdentityStatement = null;
					break;
				case "System.Data.Odbc.OdbcFactory":
				case "System.Data.OleDb.OleDbFactory":
					_namePlaceholder1 = "[{0}]";
					_namePlaceholder2 = "[{1}].[{0}]";
					_parameterPlaceholder = "?";
					_lastIdentityStatement = null;
					break;
				default:
					throw new ArgumentOutOfRangeException (nameof (factory),
						"Specified provider factory (" + factoryTypeName + ") not supported, please use one of the following:" +
						" SqlClientFactory, SqlCeProviderFactory, OracleClientFactory, OdbcFactory, OleDbFactory.");
			}
			_workDbConnection = factory.CreateConnection (); // тут не происходит никаких подключений к БД
			_workDbConnection.ConnectionString = connectionString;
			_commandTimeOut = commandTimeout;
			_logger = logger;
		}

		/// <summary>
		/// Получает текущее состояние подключения.
		/// </summary>
		public ConnectionState ConnectionState => _workDbConnection.State;

		/// <summary>
		/// Получает уникальный идентификатор, сгенерированный последней выполнявшейся командой.
		/// </summary>
		/// <returns>Уникальный идентификатор, сгенерированный последней выполнявшейся командой.</returns>
		[SuppressMessage (
			"Microsoft.Security",
			"CA2100:Review SQL queries for security vulnerabilities",
			Justification = "No user-input variables.")]
		public object GetLastIdentityValue ()
		{
			if (_lastIdentityStatement == null)
			{
				return null;
			}

			_logger?.Trace (FormattableString.Invariant ($"Executing: {_lastIdentityStatement}"));
			var dbCommand = CreateCommand ();
			dbCommand.CommandText = _lastIdentityStatement;
			return dbCommand.ExecuteScalar ();
		}

		/// <summary>
		/// Форматирует указанное имя объекта из указанной схемы так, чтобы оно соответствовало ограничениям синтаксиса.
		/// </summary>
		/// <param name="objectName">Имя объекта.</param>
		/// <param name="schemaName">Имя схемы. Укажите null если схему указывать не нужно.</param>
		/// <returns>Сформатированное имя объекта.</returns>
		[SuppressMessage ("Microsoft.Design",
			"CA1026:DefaultParametersShouldNotBeUsed",
			Justification = "Parameter have clear right 'default' value and there is no plausible reason why the default might need to change.")]
		public string FormatObjectName (string objectName, string schemaName = null)
		{
			if (objectName == null)
			{
				throw new ArgumentNullException (nameof (objectName));
			}
			Contract.EndContractBlock ();

			return (schemaName == null) ?
				string.Format (CultureInfo.InvariantCulture, _namePlaceholder1, objectName) :
				string.Format (CultureInfo.InvariantCulture, _namePlaceholder2, objectName, schemaName);
		}

		/// <summary>
		/// Начинает транзакцию.
		/// </summary>
		[SuppressMessage ("Microsoft.Globalization",
			"CA1303:Do not pass literals as localized parameters",
			MessageId = "Novartment.Base.ILogWriter.Error(System.String)",
			Justification = "String is not exposed to the end user and will not be localized.")]
		public void BeginTransaction ()
		{
			lock (_connectionLocker)
			{
				if (_workDbConnection.State != ConnectionState.Open)
				{
					var msg = "Can not create new transaction for closed connection.";
					_logger?.Error (msg);
					throw new InvalidOperationException (msg);
				}
				lock (_transactionLocker)
				{
					if (_activeTransaction.Value != null)
					{
						var msg = "Can not create new transaction while previous is not ended.";
						_logger?.Error (msg);
						throw new InvalidOperationException (msg);
					}
					_activeTransaction.Value = _workDbConnection.BeginTransaction ();
				}
			}
			_globalParamNumber = 0;
		}

		/// <summary>
		/// Подтверждает начатую транзакцию.
		/// </summary>
		[SuppressMessage ("Microsoft.Globalization",
			"CA1303:Do not pass literals as localized parameters",
			MessageId = "Novartment.Base.ILogWriter.Error(System.String)",
			Justification = "String is not exposed to the end user and will not be localized.")]
		public void Commit ()
		{
			lock (_transactionLocker)
			{
				if (_activeTransaction.Value == null)
				{
					var msg = "Can not commit not started transaction.";
					_logger?.Error (msg);
					throw new InvalidOperationException (msg);
				}
				_activeTransaction.Value.Commit ();
				_activeTransaction.Value = null;
			}
		}

		/// <summary>
		/// Отменяет начатую транзакцию.
		/// </summary>
		public void Rollback ()
		{
			lock (_transactionLocker)
			{
				if (_activeTransaction.Value == null)
				{
					return;
				}
				try { _activeTransaction.Value.Rollback (); }
				// Try/Catch exception handling should always be used when rolling back a transaction.
				// A Rollback generates an InvalidOperationException if the connection is terminated or if the transaction has already been rolled back on the server.
				catch (InvalidOperationException) { }
				catch (DbException) { }
				finally { _activeTransaction.Value = null; }
			}
		}

		/// <summary>
		/// Открывает подключение.
		/// </summary>
		[SuppressMessage ("Microsoft.Globalization",
			"CA1303:Do not pass literals as localized parameters",
			MessageId = "Novartment.Base.ILogWriter.Debug(System.String)",
			Justification = "String is not exposed to the end user and will not be localized.")]
		public void OpenConnection ()
		{
			lock (_connectionLocker)
			{
				if (_workDbConnection.State == ConnectionState.Closed)
				{
					_logger?.Info (FormattableString.Invariant ($"Opening connection to DB [{_workDbConnection.ConnectionString}]."));
					_workDbConnection.Open ();
					_logger?.Debug ("DB connection opened.");
				}
				else
				{
					_logger?.Warn (FormattableString.Invariant ($"Can not open DB connection wich already opened (State={_workDbConnection.State})."));
				}
			}
		}

		/// <summary>
		/// Закрывает подключение.
		/// </summary>
		[SuppressMessage ("Microsoft.Globalization",
			"CA1303:Do not pass literals as localized parameters",
			MessageId = "Novartment.Base.ILogWriter.Info(System.String)",
			Justification = "String is not exposed to the end user and will not be localized.")]
		public void CloseConnection ()
		{
			lock (_connectionLocker)
			{
				_workDbConnection.Close ();
				_logger?.Info ("DB connection closed.");
			}
		}

		/// <summary>
		/// Создаёт команду без параметров.
		/// </summary>
		/// <returns>Созданная команда.</returns>
		public DbCommand CreateCommand ()
		{
			return CreateCommand (null, false);
		}

		/// <summary>
		/// Создаёт команду с указанными параметрами.
		/// </summary>
		/// <param name="parameters">Параметры команды.</param>
		/// <param name="useRealParameterNames">Признак использования настоящих имён параметром.
		/// Если не указан, то будут использоваться номерные заполнители.</param>
		/// <returns>Созданная команда.</returns>
		[SuppressMessage ("Microsoft.Globalization",
			"CA1303:Do not pass literals as localized parameters",
			MessageId = "Novartment.Base.ILogWriter.Error(System.String)",
			Justification = "String is not exposed to the end user and will not be localized.")]
		public DbCommand CreateCommand (IReadOnlyCollection<InvariantDbCommandParameter> parameters, bool useRealParameterNames)
		{
			DbCommand dbCommand;
			lock (_connectionLocker)
			{
				if (_workDbConnection.State != ConnectionState.Open)
				{
					var msg = "Can not create command for closed connection.";
					_logger?.Error (msg);
					throw new InvalidOperationException (msg);
				}
				dbCommand = _workDbConnection.CreateCommand ();
				lock (_transactionLocker)
				{
					dbCommand.Transaction = _activeTransaction.Value;
				}
			}
			dbCommand.CommandTimeout = _commandTimeOut;
			if (parameters != null)
			{
				foreach (var param in parameters)
				{
					var placeHolder = string.Format (CultureInfo.InvariantCulture, _parameterPlaceholder, _globalParamNumber++);

					var dbValueInfo = param.GetDbValue ();
					var dbType = dbValueInfo.Type;
					DbType newDbType;
					var isReplacementFound = _dbTypeReplacements.TryGetValue (dbType, out newDbType);
					if (isReplacementFound)
					{
						dbType = newDbType;
					}

					var dataParam = dbCommand.CreateParameter ();
					param.Placeholder = placeHolder;
					dataParam.ParameterName = useRealParameterNames ? param.Name : placeHolder;
					dataParam.DbType = dbType;
					dataParam.Value = dbValueInfo.Value;
					dbCommand.Parameters.Add (dataParam);

					var type = (param.Value == null) ? "null" : param.Value.GetType ().Name;
					_logger?.Trace (FormattableString.Invariant ($"name={dataParam.ParameterName} value={param.Value} type={type} DbType={dataParam.DbType}"));
				}
			}
			return dbCommand;
		}

		/// <summary>
		/// Создаёт действие для выполнения в подключении.
		/// </summary>
		/// <returns>Созданное действие для выполнения в подключении.</returns>
		public IDbAction CreateAction ()
		{
			return new InvariantDbAction (this, _logger);
		}

		/// <summary>
		/// Освобождает занятые объектом ресурсы.
		/// </summary>
		[SuppressMessage (
			"Microsoft.Design",
			"CA1063:ImplementIDisposableCorrectly",
			Justification = "Implemented correctly.")]
		public void Dispose ()
		{
			lock (_connectionLocker)
			{
				lock (_transactionLocker)
				{
					_activeTransaction.Dispose ();
					_workDbConnection.Dispose ();
					GC.SuppressFinalize (this);
				}
			}
		}
	}
}
