namespace Novartment.Base
{
	/// <summary>
	/// Обёртка, содержащая начинку.
	/// </summary>
	/// <typeparam name="T">Тип начинки.</typeparam>
	public interface IValueHolder<out T>
	{
		/// <summary>
		/// Получает начинку.
		/// </summary>
		T Value { get; }
	}
}
