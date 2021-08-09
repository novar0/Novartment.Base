using System;
using System.Data;

namespace Novartment.Base.Data.SqlWrapper
{
	/// <summary>
	/// Параметр команды.
	/// </summary>
	public sealed class InvariantDbCommandParameter :
		IValueHolder<object>
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса InvariantDbCommandParameter с указанным именем и значением.
		/// </summary>
		/// <param name="name">Имя параметра.</param>
		/// <param name="value">Значение параметра.</param>
		public InvariantDbCommandParameter (string name, object value)
		{
			this.Name = name ?? throw new ArgumentNullException (nameof (name)); ;
			this.Value = value;
		}

		/// <summary>
		/// Получает имя параметра.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Получает значение параметра.
		/// </summary>
		public object Value { get; }

		/// <summary>
		/// Получает заполнитель, который вставляется в текст команды вместо параметра.
		/// </summary>
		public string Placeholder { get; set; }

		/// <summary>
		/// Получает значение и тип параметра в ограничениях БД.
		/// </summary>
		/// <returns>Значение и тип пригодные для БД.</returns>
		public DbValue GetDbValue ()
		{
			if ((this.Value == null) || (this.Value == DBNull.Value))
			{
				return new DbValue (null, DbType.String);
			}

			var type = this.Value.GetType ();
			if (this.Value is Array)
			{
				return new DbValue (this.Value, DbType.Binary);
			}

			if (this.Value is Enum)
			{
				// перечисление конвертируем в тип-значение
				type = Enum.GetUnderlyingType (type);
			}

			return GetDbValueByTypeName (type.FullName);
		}

		private DbValue GetDbValueByTypeName (string typeName)
		{
			object dbValue;
			DbType dbType;
			switch (typeName)
			{
				case "System.Boolean": dbType = DbType.Boolean; dbValue = this.Value; break;
				case "System.Byte": dbType = DbType.Byte; dbValue = (byte)this.Value; break;
				case "System.SByte": dbType = DbType.SByte; dbValue = (sbyte)this.Value; break;
				case "System.Int16": dbType = DbType.Int16; dbValue = (short)this.Value; break;
				case "System.UInt16": dbType = DbType.UInt16; dbValue = (ushort)this.Value; break;
				case "System.Int32": dbType = DbType.Int32; dbValue = (int)this.Value; break;
				case "System.UInt32": dbType = DbType.UInt32; dbValue = (uint)this.Value; break;
				case "System.Int64": dbType = DbType.Int64; dbValue = (long)this.Value; break;
				case "System.UInt64": dbType = DbType.UInt64; dbValue = (ulong)this.Value; break;
				case "System.DateTime": dbType = DbType.DateTime; dbValue = this.Value; break;
				case "System.Decimal": dbType = DbType.Decimal; dbValue = this.Value; break;
				case "System.Single": dbType = DbType.Single; dbValue = this.Value; break;
				case "System.Double": dbType = DbType.Double; dbValue = this.Value; break;
				case "System.String": dbType = DbType.String; dbValue = this.Value; break;
				case "System.Guid": dbType = DbType.Guid; dbValue = this.Value; break;

				// следующие типы автоматически не конвертируются
				case "System.TimeSpan": // промежуток времени конвертируем в количество секунд
					dbType = DbType.Double;
					var ts = (TimeSpan)this.Value;
					dbValue = ts.TotalSeconds;
					break;
				case "System.Char":
					dbType = DbType.String; // символ конвертируем в строку
					dbValue = new string ((char)this.Value, 1);
					break;
				default:
					throw new InvalidOperationException ("Type [" + this.Value.GetType ().FullName + "] can not be converted to DbType.");
			}

			return new DbValue (dbValue, dbType);
		}
	}
}
