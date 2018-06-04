using System;

namespace Novartment.Base.Reflection
{
	/// <summary>
	/// Данные об аргументе конструктора атрибута.
	/// </summary>
	public struct AttributeArgument :
		IEquatable<AttributeArgument>
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса AttributeArgument с указанными именем и значением.
		/// </summary>
		/// <param name="name">Имя аргумента.</param>
		/// <param name="value">Значение аргумента.</param>
		public AttributeArgument (string name, object value)
		{
			this.Name = name;
			this.Value = value;
		}

		/// <summary>
		/// Получает имя аргумента.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Получает значение аргумента.
		/// </summary>
		public object Value { get; }

		/// <summary>
		/// Определяет равенство двух указанных объектов.
		/// </summary>
		/// <param name="first">Первый объект для сравнения.</param>
		/// <param name="second">Второй объект для сравнения.</param>
		/// <returns>True если значение параметра first равно second; в противном случае — False.</returns>
		public static bool operator == (AttributeArgument first, AttributeArgument second)
		{
			return first.Equals (second);
		}

		/// <summary>
		/// Определяет неравенство двух указанных объектов.
		/// </summary>
		/// <param name="first">Первый объект для сравнения.</param>
		/// <param name="second">Второй объект для сравнения.</param>
		/// <returns>True если значение параметра first не равно second; в противном случае — False.</returns>
		public static bool operator != (AttributeArgument first, AttributeArgument second)
		{
			return !first.Equals (second);
		}

		/// <summary>
		/// Деконструирует данные.
		/// </summary>
		/// <param name="name">Получает имя аргумента.</param>
		/// <param name="value">Получает значение аргумента.</param>
		public void Deconstruct (out string name, out object value)
		{
			name = this.Name;
			value = this.Value;
		}

		/// <summary>
		/// Преобразовывает значение объекта в эквивалентное ему строковое представление.
		/// </summary>
		/// <returns>Строковое представление значения объекта.</returns>
		public override string ToString ()
		{
			return FormattableString.Invariant ($"Name: {this.Name ?? "<none>"}, Value: {this.Value?.ToString () ?? "<null>"}");
		}

		/// <summary>
		/// Вычисляет хэш-функцию объекта.
		/// </summary>
		/// <returns>Хэш-код для текущего объекта.</returns>
		public override int GetHashCode ()
		{
#if NETCOREAPP2_1
			return this.Name?.GetHashCode (StringComparison.Ordinal) ?? 0 ^ this.Value?.GetHashCode () ?? 0;
#else
			return this.Name?.GetHashCode () ?? 0 ^ this.Value?.GetHashCode () ?? 0;
#endif
		}

		/// <summary>
		/// Определяет, равен ли заданный объект текущему объекту.
		/// </summary>
		/// <param name="obj">Объект, который требуется сравнить с текущим объектом. </param>
		/// <returns>True , если указанный объект равен текущему объекту; в противном случае — False.</returns>
		public bool Equals (AttributeArgument obj)
		{
			return obj.Equals ((object)this);
		}

		/// <summary>
		/// Определяет, равен ли заданный объект текущему объекту.
		/// </summary>
		/// <param name="obj">Объект, который требуется сравнить с текущим объектом. </param>
		/// <returns>True , если указанный объект равен текущему объекту; в противном случае — False.</returns>
		public override bool Equals (object obj)
		{
			if (obj == null)
			{
				return false;
			}

			if (!(obj is AttributeArgument))
			{
				return false;
			}

			var other = (AttributeArgument)obj;
			return other.Name.Equals (this.Name, StringComparison.Ordinal) && (other.Value == this.Value);
		}
	}
}
