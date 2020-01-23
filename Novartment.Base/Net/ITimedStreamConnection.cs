using System;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net
{
	/// <summary>
	/// An established streaming connection with monitoring of idle time and total time.
	/// </summary>
	/// <remarks>
	/// The connection is considered established, so the Connect()/Disconnect() methods are not provided.
	/// An unestablished connection does not make sense, since no interface member makes sense before the connection is established.
	/// When implemented, the dicconnection is implemented in the Dispose () method.
	/// </remarks>
	public interface ITimedStreamConnection :
			IDisposable
	{
		/// <summary>
		/// Gets the data source, from which connection data can be read.
		/// </summary>
		IBufferedSource Reader { get; }

		/// <summary>
		/// Gets the data destination, that writes data to the connection.
		/// </summary>
		IBinaryDestination Writer { get; }

		/// <summary>
		/// Gets the amount of time that has elapsed since the connection was established.
		/// </summary>
		TimeSpan Duration { get; }

		/// <summary>
		/// Gets the amount of time that has elapsed since incoming connection data was last received.
		/// </summary>
		TimeSpan IdleDuration { get; }
	}
}
