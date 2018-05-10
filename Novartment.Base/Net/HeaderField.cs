using System;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Net
{
	/// <summary>
	/// Поле заголовка.
	/// </summary>
	public class HeaderField :
		IValueHolder<string>,
		IEquatable<HeaderField>
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса HeaderField с указанным именем и значением.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="value">Значение поля заголовка.</param>
		public HeaderField (HeaderFieldName name, string value)
		{
			if (name == HeaderFieldName.Unspecified)
			{
				throw new ArgumentOutOfRangeException (nameof (name));
			}

			Contract.EndContractBlock ();

			this.Name = name;
			this.Value = value;
		}

		/// <summary>
		/// Получает имя поля заголовка.
		/// </summary>
		public HeaderFieldName Name { get; }

		/// <summary>
		/// Получает значение поля заголовка.
		/// </summary>
		public string Value { get; }

		/// <summary>
		/// Определяет равенство двух указанных объектов.
		/// </summary>
		/// <param name="first">Первый объект для сравнения.</param>
		/// <param name="second">Второй объект для сравнения.</param>
		/// <returns>True если значение параметра first равно second; в противном случае — False.</returns>
		public static bool operator == (HeaderField first, HeaderField second)
		{
			return first is null ?
				second is null :
				first.Equals(second);
		}

		/// <summary>
		/// Определяет неравенство двух указанных объектов.
		/// </summary>
		/// <param name="first">Первый объект для сравнения.</param>
		/// <param name="second">Второй объект для сравнения.</param>
		/// <returns>True если значение параметра first не равно second; в противном случае — False.</returns>
		public static bool operator != (HeaderField first, HeaderField second)
		{
			return !(first is null ?
				second is null :
				first.Equals(second));
		}

		/// <summary>
		/// Преобразовывает значение объекта в эквивалентное ему строковое представление.
		/// </summary>
		/// <returns>Строковое представление значения объекта.</returns>
		public override string ToString ()
		{
			return (this.Value != null) ? (this.Name + ": " + this.Value) : this.Name + ":";
		}

		/// <summary>
		/// Вычисляет хэш-функцию объекта.
		/// </summary>
		/// <returns>Хэш-код для текущего объекта.</returns>
		public override int GetHashCode ()
		{
			return this.Name.GetHashCode () ^ this.Value.GetHashCode ();
		}

		/// <summary>
		/// Определяет, равен ли заданный объект текущему объекту.
		/// </summary>
		/// <param name="obj">Объект, который требуется сравнить с текущим объектом. </param>
		/// <returns>True , если указанный объект равен текущему объекту; в противном случае — False.</returns>
		public override bool Equals (object obj)
		{
			var typedOther = obj as HeaderField;
			return (typedOther != null) && Equals (typedOther);
		}

		/// <summary>
		/// Определяет, равен ли заданный объект текущему объекту.
		/// </summary>
		/// <param name="other">Объект, который требуется сравнить с текущим объектом. </param>
		/// <returns>True , если указанный объект равен текущему объекту; в противном случае — False.</returns>
		public bool Equals (HeaderField other)
		{
			if (other == null)
			{
				return false;
			}

			return
				(this.Name == other.Name) &&
				string.Equals (this.Value, other.Value, StringComparison.Ordinal);
		}
	}
}
