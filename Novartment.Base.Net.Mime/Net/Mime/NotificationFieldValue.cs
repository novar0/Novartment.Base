using System;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Значение поля уведомления, состоящее из типа и собственно значения.
	/// Определено в RFC 3464 часть 2.1.2.
	/// </summary>
	public class NotificationFieldValue :
		IValueHolder<string>,
		IEquatable<NotificationFieldValue>
	{
		// RFC 3464 2.1.2.
		// field-value = type ";" value
		// type = atom
		// value = *text

		/// <summary>
		/// Инициализирует новый экземпляр класса NotificationFieldValue с указанными типом и значением.
		/// </summary>
		/// <param name="kind">Тип значения поля.</param>
		/// <param name="value">Значение поля.</param>
		public NotificationFieldValue (NotificationFieldValueKind kind, string value)
		{
			if (kind == NotificationFieldValueKind.Unspecified)
			{
				throw new ArgumentOutOfRangeException (nameof (kind));
			}

			if (value == null)
			{
				throw new ArgumentNullException (nameof (value));
			}

			Contract.EndContractBlock ();

			this.Kind = kind;
			this.Value = value;
		}

		/// <summary>
		/// Получает тип значения поля.
		/// </summary>
		public NotificationFieldValueKind Kind { get; }

		/// <summary>
		/// Получает значение поля.
		/// </summary>
		public string Value { get; }

		/// <summary>
		/// Определяет равенство двух указанных объектов.
		/// </summary>
		/// <param name="first">Первый объект для сравнения.</param>
		/// <param name="second">Второй объект для сравнения.</param>
		/// <returns>True если значение параметра first равно second; в противном случае — False.</returns>
		public static bool operator == (NotificationFieldValue first, NotificationFieldValue second)
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
		public static bool operator != (NotificationFieldValue first, NotificationFieldValue second)
		{
			return !(first is null ?
				second is null :
				first.Equals (second));
		}

		/// <summary>
		/// Создаёт новое значение ValueWithType у которого тип изменён на указанный.
		/// </summary>
		/// <param name="type">Тип значения поля.</param>
		/// <returns>Новое значение ValueWithType у которого тип изменён на указанный.</returns>
		public NotificationFieldValue ChangeType (NotificationFieldValueKind type)
		{
			if (type == NotificationFieldValueKind.Unspecified)
			{
				throw new ArgumentOutOfRangeException (nameof (type));
			}

			Contract.EndContractBlock ();

			return new NotificationFieldValue (type, this.Value);
		}

		/// <summary>
		/// Создаёт новое значение ValueWithType у которого значение изменено на указанное.
		/// </summary>
		/// <param name="value">Значение поля.</param>
		/// <returns>Новое значение ValueWithType у которого значение изменено на указанное.</returns>
		public NotificationFieldValue ChangeValue (string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException (nameof (value));
			}

			Contract.EndContractBlock ();

			return new NotificationFieldValue (this.Kind, value);
		}

		/// <summary>
		/// Получает строковое представление объекта.
		/// </summary>
		/// <returns>Строковое представление объекта.</returns>
		public override string ToString ()
		{
			return this.Kind + "; " + this.Value;
		}

		/// <summary>
		/// Вычисляет хэш-функцию объекта.
		/// </summary>
		/// <returns>Хэш-код для текущего объекта.</returns>
		public override int GetHashCode ()
		{
			return this.Kind.GetHashCode () ^ this.Value.GetHashCode ();
		}

		/// <summary>
		/// Определяет, равен ли заданный объект текущему объекту.
		/// </summary>
		/// <param name="obj">Объект, который требуется сравнить с текущим объектом. </param>
		/// <returns>True , если указанный объект равен текущему объекту; в противном случае — False.</returns>
		public override bool Equals (object obj)
		{
			var typedOther = obj as NotificationFieldValue;
			return (typedOther != null) && Equals (typedOther);
		}

		/// <summary>
		/// Определяет, равен ли заданный объект текущему объекту.
		/// </summary>
		/// <param name="other">Объект, который требуется сравнить с текущим объектом. </param>
		/// <returns>True , если указанный объект равен текущему объекту; в противном случае — False.</returns>
		public bool Equals (NotificationFieldValue other)
		{
			return (other != null) &&
			(this.Kind == other.Kind) &&
			string.Equals (this.Value, other.Value, StringComparison.OrdinalIgnoreCase);
		}
	}
}
