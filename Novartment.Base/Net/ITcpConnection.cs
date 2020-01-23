namespace Novartment.Base.Net
{
	/// <summary>
	/// An established TCP connection with monitoring of idle time and total time.
	/// </summary>
	public interface ITcpConnection :
		ITimedStreamConnection
	{
		/// <summary>
		/// Gets the local network endpoint.
		/// </summary>
		IPHostEndPoint LocalEndPoint { get; }

		/// <summary>
		/// Gets the remote network endpoint.
		/// </summary>
		IPHostEndPoint RemoteEndPoint { get; }
	}
}
