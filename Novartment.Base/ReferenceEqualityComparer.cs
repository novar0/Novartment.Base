using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Novartment.Base
{
	/// <summary>
	/// Поддержка операций сравнения объектов по ссылке.
	/// </summary>
	public sealed class ReferenceEqualityComparer :
		IEqualityComparer<object>
	{
		private static readonly IEqualityComparer<object> _default = new ReferenceEqualityComparer ();

		private ReferenceEqualityComparer ()
		{
		}

		/// <summary>
		/// Получает экземпляр ссылочного компаратора.
		/// </summary>
		public static IEqualityComparer<object> Default => _default;

		/// <summary>
		/// Определяет, указывают ли обе указанных ссылки на один объект.
		/// </summary>
		/// <param name="obj1">Первая сравниваемая ссылка на объект.</param>
		/// <param name="obj2">Вторая сравниваемая ссылка на объект.</param>
		/// <returns>True, если обе ссылки указывают на один объект; в противном случае — False.</returns>
		bool IEqualityComparer<object>.Equals (object obj1, object obj2)
		{
			return object.ReferenceEquals (obj1, obj2);
		}

		/// <summary>
		/// Возвращает хэш-код указанного объекта используя базовый алгоритм типа Object
		/// (не зависящий от переопределения метода GetHashCode()).
		/// </summary>
		/// <param name="obj">Объект, для которого будет посчитан хэш-код.</param>
		/// <returns>Хэш-код указанного объекта.</returns>
		int IEqualityComparer<object>.GetHashCode (object obj)
		{
			return RuntimeHelpers.GetHashCode (obj);
		}
	}
}
