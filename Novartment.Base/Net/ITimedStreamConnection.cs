using System;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net
{
	/// <summary>
	/// Установленное потоковое подключение с отслеживанием полного времени и времени простоя.
	/// </summary>
	/// <remarks>
	/// Подключение считается установленным, поэтому методы Connect()/Disconnect() не предусмотрены.
	/// Неустановленное подключение не имеет смысла, так как до установки подключения не имеет смысла ни один член интерфейса.
	/// При реализации метод Disconnect() заменяется методом Dispose().
	/// </remarks>
	public interface ITimedStreamConnection :
			IDisposable
	{
		/// <summary>
		/// Получает источник входящих через подключение данных.
		/// </summary>
		IBufferedSource Reader { get; }

		/// <summary>
		/// Получатель двоичных данных, осуществляющий запись данных в подключение.
		/// </summary>
		IBinaryDestination Writer { get; }

		/// <summary>
		/// Получает промежуток времени, прошедший с момента установки подключения.
		/// </summary>
		TimeSpan Duration { get; }

		/// <summary>
		/// Получает промежуток времени, прошедший с момента последнего получения входящих данных подключения.
		/// </summary>
		TimeSpan IdleDuration { get; }
	}
}
