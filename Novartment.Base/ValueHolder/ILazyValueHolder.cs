namespace Novartment.Base
{
	/// <summary>
	/// Обёртка, содержащая начинку с отложенной инициализацией.
	/// </summary>
	/// <typeparam name="T">Тип объекта-начинки.</typeparam>
	public interface ILazyValueHolder<out T> :
		IValueHolder<T>
	{
		/// <summary>
		/// Получает значение, указывающее, была ли инициализирована начинка.
		/// </summary>
		bool IsValueCreated { get; }
	}
}
