using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.Net
{
	/// <summary>
	/// Listens for connections from TCP network clients.
	/// </summary>
	/// <remarks>
	/// Designed to replace System.Net.Sockets.TcpListener class.
	/// Differences only in the AcceptTcpClientAsync() method:
	/// 1. Accepts the CancellationToken parameter to implement listening cancellation.
	/// 2. Returns interface ITcpConnection instead of the specific library class.
	/// </remarks>
	public interface ITcpListener
	{
		/// <summary>
		/// Gets the underlying endpoint of the current listener.
		/// </summary>
		IPEndPoint LocalEndpoint { get; }

		/// <summary>
		/// Starts listening for incoming connection requests.
		/// </summary>
		void Start ();

#pragma warning disable CA1716 // Identifiers should not match keywords
		/// <summary>
		/// Closes the listener.
		/// </summary>
		void Stop ();
#pragma warning restore CA1716 // Identifiers should not match keywords

		/// <summary>
		/// Accepts a pending connection request as an asynchronous operation.
		/// </summary>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>
		/// The task representing the asynchronous operation.
		/// The result of the task is the established TCP connection used to send and receive data.
		/// </returns>
		Task<ITcpConnection> AcceptTcpClientAsync (CancellationToken cancellationToken = default);
	}
}
