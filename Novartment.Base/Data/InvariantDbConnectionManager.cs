using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Globalization;
using Microsoft.Extensions.Logging;

namespace Novartment.Base.Data
{
	/// <summary>
	/// Подключение к базе данных.
	/// </summary>
	public sealed class InvariantDbConnectionManager :
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

		private readonly ReusableDisposable<DbTransaction> _activeTransaction = new ReusableDisposable<DbTransaction> ();
		private readonly ILogger _logger;
		private int _globalParamNumber; // номера параметров для sql server должны быть уникальными в пределах транзакции

		/// <summary>
		/// Инициализирует новый экземпляр класса InvariantDbConnectionManager с указанными параметрами.
		/// </summary>
		/// <param name="factory">Провайдер.</param>
		/// <param name="connectionString">Строка подключения.</param>
		/// <param name="commandTimeout">Время (в секундах) ожидания выполнения команды перед тем как она будет прервана с ошибкой.</param>
		/// <param name="logger">Опциональный объект-журнал для записей о событиях. Укажите null если не требуется.</param>
		public InvariantDbConnectionManager (
			DbProviderFactory factory,
			string connectionString,
			int commandTimeout,
			ILogger<InvariantDbConnectionManager> logger = null)
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
					var excptText = "Specified provider factory (" + factoryTypeName + ") not supported, please use one of the following:" +
						" SqlClientFactory, SqlCeProviderFactory, OracleClientFactory, OdbcFactory, OleDbFactory.";
					throw new ArgumentOutOfRangeException (
						nameof (factory),
						excptText);
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
		public object GetLastIdentityValue ()
		{
			if (_lastIdentityStatement == null)
			{
				return null;
			}

			_logger?.LogTrace (FormattableString.Invariant ($"Executing: {_lastIdentityStatement}"));
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
		public void BeginTransaction ()
		{
			lock (_connectionLocker)
			{
				if (_workDbConnection.State != ConnectionState.Open)
				{
					var msg = "Can not create new transaction for closed connection.";
					_logger?.LogError (msg);
					throw new InvalidOperationException (msg);
				}

				lock (_transactionLocker)
				{
					if (_activeTransaction.Value != null)
					{
						var msg = "Can not create new transaction while previous is not ended.";
						_logger?.LogError (msg);
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
		public void Commit ()
		{
			lock (_transactionLocker)
			{
				if (_activeTransaction.Value == null)
				{
					var msg = "Can not commit not started transaction.";
					_logger?.LogError (msg);
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

				// Try/Catch exception handling should always be used when rolling back a transaction.
				// A Rollback generates an InvalidOperationException if the connection is terminated or if the transaction has already been rolled back on the server.
				try
				{
					_activeTransaction.Value.Rollback ();
				}
				catch (InvalidOperationException)
				{
				}
				catch (DbException)
				{
				}
				finally
				{
					_activeTransaction.Value = null;
				}
			}
		}

		/// <summary>
		/// Открывает подключение.
		/// </summary>
		public void OpenConnection ()
		{
			lock (_connectionLocker)
			{
				if (_workDbConnection.State == ConnectionState.Closed)
				{
					_logger?.LogInformation (FormattableString.Invariant ($"Opening connection to DB [{_workDbConnection.ConnectionString}]."));
					_workDbConnection.Open ();
					_logger?.LogDebug ("DB connection opened.");
				}
				else
				{
					_logger?.LogWarning (FormattableString.Invariant ($"Can not open DB connection wich already opened (State={_workDbConnection.State})."));
				}
			}
		}

		/// <summary>
		/// Закрывает подключение.
		/// </summary>
		public void CloseConnection ()
		{
			lock (_connectionLocker)
			{
				_workDbConnection.Close ();
				_logger?.LogInformation ("DB connection closed.");
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
		public DbCommand CreateCommand (IReadOnlyCollection<InvariantDbCommandParameter> parameters, bool useRealParameterNames)
		{
			DbCommand dbCommand;
			lock (_connectionLocker)
			{
				if (_workDbConnection.State != ConnectionState.Open)
				{
					var msg = "Can not create command for closed connection.";
					_logger?.LogError (msg);
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

					var (dbValue, dbType) = param.GetDbValue ();
					var isReplacementFound = _dbTypeReplacements.TryGetValue (dbType, out DbType newDbType);
					if (isReplacementFound)
					{
						dbType = newDbType;
					}

					var dataParam = dbCommand.CreateParameter ();
					param.Placeholder = placeHolder;
					dataParam.ParameterName = useRealParameterNames ? param.Name : placeHolder;
					dataParam.DbType = dbType;
					dataParam.Value = dbValue;
					dbCommand.Parameters.Add (dataParam);

					var type = (param.Value == null) ? "null" : param.Value.GetType ().Name;
					_logger?.LogTrace (FormattableString.Invariant ($"name={dataParam.ParameterName} value={param.Value} type={type} DbType={dataParam.DbType}"));
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
