using System;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// A binary data destination for sequential writing.
	/// </summary>
	public interface IBinaryDestination
	{
		/// <summary>
		/// Asynchronously writes the specified region of memory to this destination.
		/// </summary>
		/// <param name="buffer">The region of memory to write to this destination.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>A task that represents the write operation.</returns>
		ValueTask WriteAsync (ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default);

		/// <summary>
		/// Mark this destination as being completed, meaning no more data will be written to it.
		/// </summary>
		void SetComplete ();
	}
}
