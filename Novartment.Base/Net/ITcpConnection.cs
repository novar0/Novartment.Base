namespace Novartment.Base.Net
{
	/// <summary>
	/// Установленное TCP-подключение с отслеживанием полного времени и времени простоя.
	/// </summary>
	public interface ITcpConnection :
		ITimedStreamConnection
	{
		/// <summary>
		/// Получает локальную конечную точку подключения.
		/// </summary>
		IPHostEndPoint LocalEndPoint { get; }

		/// <summary>
		/// Получает удалённую конечную точку подключения.
		/// </summary>
		IPHostEndPoint RemoteEndPoint { get; }
	}
}
