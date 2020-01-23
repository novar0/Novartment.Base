using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.Net
{
	/// <summary>
	/// The network protocol on base of streaming connection.
	/// </summary>
	public interface ITcpConnectionProtocol
	{
		/// <summary>
		/// Starts processing the specified connection.
		/// </summary>
		/// <param name="connection">The streaming connection.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>
		/// A task that represents the processing.
		/// </returns>
		/// <exception cref="Novartment.Base.Net.UnrecoverableProtocolException">
		/// Critical violation of the protocol in which it cannot continue.
		/// It is highly recommended that you close the connection.
		/// </exception>
		Task StartAsync (ITcpConnection connection, CancellationToken cancellationToken = default);
	}
}
