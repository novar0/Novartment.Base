using System;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Net
{
	/// <summary>
	/// Параметр с характеризующей его относительной важностью
	/// согласно спецификации "qvalue" в RFC 2616 часть 3.9.
	/// </summary>
	public class QualityValueParameter :
		IValueHolder<string>,
		IEquatable<QualityValueParameter>
	{
		/// <summary>
		/// Инициализирует новый экземпляр QualityValueParameter с указанным значением и относительной важностью.
		/// </summary>
		/// <param name="value">Значение параметра.</param>
		/// <param name="importance">Относительная важность параметра в диапазоне от 0…1.</param>
		public QualityValueParameter (string value, decimal importance)
		{
			if (value == null)
			{
				throw new ArgumentNullException (nameof (value));
			}

			if (importance <= 0.0m || importance > 1.0m)
			{
				throw new ArgumentOutOfRangeException (nameof (importance));
			}

			Contract.EndContractBlock ();

			this.Importance = importance;
			this.Value = value;
		}

		/// <summary>
		/// Получает относительную важность параметра.
		/// </summary>
		public decimal Importance { get; }

		/// <summary>
		/// Получает значение параметра.
		/// </summary>
		public string Value { get; }

		/// <summary>
		/// Определяет равенство двух указанных объектов.
		/// </summary>
		/// <param name="first">Первый объект для сравнения.</param>
		/// <param name="second">Второй объект для сравнения.</param>
		/// <returns>True если значение параметра first равно second; в противном случае — False.</returns>
		public static bool operator == (QualityValueParameter first, QualityValueParameter second)
		{
			return first is null ?
				second is null :
				first.Equals (second);
		}

		/// <summary>
		/// Определяет неравенство двух указанных объектов.
		/// </summary>
		/// <param name="first">Первый объект для сравнения.</param>
		/// <param name="second">Второй объект для сравнения.</param>
		/// <returns>True если значение параметра first не равно second; в противном случае — False.</returns>
		public static bool operator != (QualityValueParameter first, QualityValueParameter second)
		{
			return !(first is null ?
				second is null :
				first.Equals (second));
		}

		/// <summary>
		/// Создаёт новый параметр, имеющий тоже значение и указанную относительную важность.
		/// </summary>
		/// <param name="importance">Относительная важность параметра в диапазоне от 0…1.</param>
		/// <returns>Новый параметр, имеющий тоже значение и указанную относительную важность..</returns>
		public QualityValueParameter ChangeImportance (decimal importance)
		{
			if (importance <= 0.0m || importance > 1.0m)
			{
				throw new ArgumentOutOfRangeException (nameof (importance));
			}

			Contract.EndContractBlock ();

			return new QualityValueParameter (this.Value, importance);
		}

		/// <summary>
		/// Создаёт новый параметр, имеющий туже относительную важность и указанное значение.
		/// </summary>
		/// <param name="value">Значение параметра.</param>
		/// <returns>Новый параметр, имеющий туже относительную важность и указанное значение.</returns>
		public QualityValueParameter ChangeValue (string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException (nameof (value));
			}

			Contract.EndContractBlock ();

			return new QualityValueParameter (value, this.Importance);
		}

		/// <summary>
		/// Получает строковое представление параметра.
		/// </summary>
		/// <returns>Строковое представление параметра.</returns>
		public override string ToString ()
		{
			return FormattableString.Invariant ($"{this.Value};q={this.Importance}");
		}

		/// <summary>
		/// Вычисляет хэш-функцию объекта.
		/// </summary>
		/// <returns>Хэш-код для текущего объекта.</returns>
		public override int GetHashCode ()
		{
#if NETSTANDARD2_1
			return this.Importance.GetHashCode () ^ this.Value.GetHashCode (StringComparison.Ordinal);
#else
			return this.Importance.GetHashCode () ^ this.Value.GetHashCode ();
#endif
		}

		/// <summary>
		/// Определяет, равен ли заданный объект текущему объекту.
		/// </summary>
		/// <param name="obj">Объект, который требуется сравнить с текущим объектом. </param>
		/// <returns>True , если указанный объект равен текущему объекту; в противном случае — False.</returns>
		public override bool Equals (object obj)
		{
			var typedOther = obj as QualityValueParameter;
			return (typedOther != null) && Equals (typedOther);
		}

		/// <summary>
		/// Определяет, равен ли заданный объект текущему объекту.
		/// </summary>
		/// <param name="other">Объект, который требуется сравнить с текущим объектом. </param>
		/// <returns>True , если указанный объект равен текущему объекту; в противном случае — False.</returns>
		public bool Equals (QualityValueParameter other)
		{
			if (other == null)
			{
				return false;
			}

			return
				(this.Importance == other.Importance) &&
				string.Equals (this.Value, other.Value, StringComparison.Ordinal);
		}
	}
}
