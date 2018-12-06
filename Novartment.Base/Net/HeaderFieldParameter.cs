using System;
using System.Diagnostics.Contracts;
using Novartment.Base.Text;

namespace Novartment.Base.Net
{
	/// <summary>
	/// Параметр поля заголовка.
	/// </summary>
	public class HeaderFieldParameter :
		IValueHolder<string>,
		IEquatable<HeaderFieldParameter>
	{
		private string _value;

		/// <summary>
		/// Инициализирует новый экземпляр класса HeaderFieldParameter с указанным именем и значением.
		/// </summary>
		/// <param name="name">Имя параметра поля заголовка.</param>
		/// <param name="value">Значение параметра поля заголовка.</param>
		public HeaderFieldParameter(string name, string value)
		{
			if (name == null)
			{
				throw new ArgumentNullException(nameof(name));
			}

			if ((name.Length < 1) ||
				!AsciiCharSet.IsAllOfClass(name, AsciiCharClasses.Token))
			{
				throw new ArgumentOutOfRangeException(nameof(name));
			}

			Contract.EndContractBlock();

			this.Name = name;
			_value = value;
		}

		/// <summary>
		/// Получает имя параметра поля заголовка.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Получает значение параметра поля заголовка.
		/// </summary>
		public string Value
		{
			get => _value;
			set { _value = value; }
		}

		/// <summary>
		/// Определяет равенство двух указанных объектов.
		/// </summary>
		/// <param name="first">Первый объект для сравнения.</param>
		/// <param name="second">Второй объект для сравнения.</param>
		/// <returns>True если значение параметра first равно second; в противном случае — False.</returns>
		public static bool operator ==(HeaderFieldParameter first, HeaderFieldParameter second)
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
		public static bool operator !=(HeaderFieldParameter first, HeaderFieldParameter second)
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
			return this.Name + "=" + _value;
		}

		/// <summary>
		/// Вычисляет хэш-функцию объекта.
		/// </summary>
		/// <returns>Хэш-код для текущего объекта.</returns>
		public override int GetHashCode ()
		{
#if NETCOREAPP2_2
			return StringComparer.OrdinalIgnoreCase.GetHashCode (this.Name) ^ (_value?.GetHashCode (StringComparison.Ordinal) ?? 0);
#else
			return StringComparer.OrdinalIgnoreCase.GetHashCode (this.Name) ^ (_value?.GetHashCode () ?? 0);
#endif
		}

		/// <summary>
		/// Определяет, равен ли заданный объект текущему объекту.
		/// </summary>
		/// <param name="obj">Объект, который требуется сравнить с текущим объектом. </param>
		/// <returns>True , если указанный объект равен текущему объекту; в противном случае — False.</returns>
		public override bool Equals (object obj)
		{
			var typedOther = obj as HeaderFieldParameter;
			return (typedOther != null) && Equals (typedOther);
		}

		/// <summary>
		/// Определяет, равен ли заданный объект текущему объекту.
		/// </summary>
		/// <param name="other">Объект, который требуется сравнить с текущим объектом. </param>
		/// <returns>True , если указанный объект равен текущему объекту; в противном случае — False.</returns>
		public bool Equals (HeaderFieldParameter other)
		{
			if (other == null)
			{
				return false;
			}

			return
				string.Equals (this.Name, other.Name, StringComparison.OrdinalIgnoreCase) &&
				string.Equals (_value, other._value, StringComparison.Ordinal);
		}
	}
}
