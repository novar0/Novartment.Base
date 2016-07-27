using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Data
{
	/// <summary>
	/// Значение и его тип для обмена с БД.
	/// </summary>
	public struct DbValue
	{
		/// <summary>
		/// Значение.
		/// </summary>
		public object Value;

		/// <summary>
		/// Тип значения.
		/// </summary>
		public DbType Type;
	}

	/// <summary>
	/// Параметр команды.
	/// </summary>
	public class InvariantDbCommandParameter :
		IValueHolder<object>
	{
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
		/// Инициализирует новый экземпляр класса InvariantDbCommandParameter с указанным именем и значением.
		/// </summary>
		public InvariantDbCommandParameter (string name, object value)
		{
			if (name == null)
			{
				throw new ArgumentNullException (nameof (name));
			}
			Contract.EndContractBlock ();
			this.Name = name;
			this.Value = value;
		}

		/// <summary>
		/// Получает значение и тип параметра в ограничениях БД.
		/// </summary>
		/// <returns>Значение и тип пригодные для БД.</returns>
		[SuppressMessage ("Microsoft.Design",
			"CA1024:UsePropertiesWhereAppropriate",
			Justification = "The method performs a time-consuming operation and performs a conversion.")]
		public DbValue GetDbValue ()
		{
			if ((this.Value == null) || (this.Value == DBNull.Value))
			{
				return new DbValue () { Type = DbType.String };
			}
			var type = this.Value.GetType ();
			if (this.Value is Array)
			{
				return new DbValue () { Value = this.Value, Type = DbType.Binary };
			}
			if (this.Value is Enum) // перечисление конвертируем в тип-значение
			{
				type = Enum.GetUnderlyingType (type);
			}

			return GetDbValueByTypeName (type.FullName);
		}

		[SuppressMessage ("Microsoft.Maintainability",
			"CA1502:AvoidExcessiveComplexity",
			Justification = "Method not too complex.")]
		private DbValue GetDbValueByTypeName (string typeName)
		{
			object dbValue;
			DbType dbType;
			switch (typeName)
			{
				case "System.Boolean": dbType = DbType.Boolean; dbValue = this.Value; break;
				case "System.Byte": dbType = DbType.Byte; dbValue = (Byte)this.Value; break;
				case "System.SByte": dbType = DbType.SByte; dbValue = (SByte)this.Value; break;
				case "System.Int16": dbType = DbType.Int16; dbValue = (Int16)this.Value; break;
				case "System.UInt16": dbType = DbType.UInt16; dbValue = (UInt16)this.Value; break;
				case "System.Int32": dbType = DbType.Int32; dbValue = (Int32)this.Value; break;
				case "System.UInt32": dbType = DbType.UInt32; dbValue = (UInt32)this.Value; break;
				case "System.Int64": dbType = DbType.Int64; dbValue = (Int64)this.Value; break;
				case "System.UInt64": dbType = DbType.UInt64; dbValue = (UInt64)this.Value; break;
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
			return new DbValue () { Value = dbValue, Type = dbType };
		}
	}
}
