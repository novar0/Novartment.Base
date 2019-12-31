using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	///  A data source for sequential reading, represented by a byte buffer and
	///  supports fast skip of data.
	/// </summary>
	public interface IFastSkipBufferedSource :
		IBufferedSource
	{
		/// <summary>
		/// Asynchronously tries to skip specified amount of source data, including data already available in the buffer.
		/// Properties Offset, Count and IsExhausted may be changed in the process.
		/// </summary>
		/// <param name="size">Size of data to skip, including data already available in the buffer.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is System.Threading.CancellationToken.None.</param>
		/// <returns>
		/// A task that represents the asynchronous skip operation.
		/// The result of a task will indicate the number of actually skipped bytes of data, including data already available in the buffer.
		/// It may be less than specified if the source is exhausted.
		/// Upon completion of a task, regardless of the result, the source will provide data coming right after skipped.
		/// </returns>
		ValueTask<long> TryFastSkipAsync (long size, CancellationToken cancellationToken = default);
	}
}
