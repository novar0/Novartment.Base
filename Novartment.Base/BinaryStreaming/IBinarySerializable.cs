using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// An entity suitable for storing in a binary data destination.
	/// </summary>
	public interface IBinarySerializable
	{
		/// <summary>
		/// Saves this entity in the specified binary data destination.
		/// </summary>
		/// <param name="destination">The binary data destination, in which this entity will be saved.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>A task that represents the operation.</returns>
		Task SaveAsync (IBinaryDestination destination, CancellationToken cancellationToken = default);
	}
}