using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// A data source for sequential reading, represented by a byte buffer, that represents data in individual parts.
	/// Provides data within one part and the ability to transition to the next part.
	/// </summary>
	/// <remarks>
	/// Changes the semantics of the inherited property IsExhausted.
	/// Now it means the exhaustion of one part, which does not exclude the transition to the next.
	/// </remarks>
	public interface IPartitionedBufferedSource :
		IBufferedSource
	{
		/// <summary>
		/// Asynchronously tries to skip all source data belonging to the current part,
		/// and transition to the next part.
		/// </summary>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>A task that represents the asynchronous skip and transition operation. 
		/// The result of a task will indicate success of the transition.
		/// It will be True if the source has been transitioned to the next part,
		/// and False if the source has been exhausted.
		/// </returns>
		ValueTask<bool> TrySkipPartAsync (CancellationToken cancellationToken = default);
	}
}
