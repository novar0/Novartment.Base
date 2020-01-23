using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;

namespace Novartment.Base.Tasks
{
	/// <summary>
	/// Extension method for System.Threading.Channels.ChannelReader.
	/// </summary>
	public static class ChannelReaderExtensions
	{
		/// <summary>Creates an <see cref="IAsyncEnumerable{T}"/> that enables reading all of the data from the channel.</summary>
		/// <param name="channelReader">The reader of channel.</param>
		/// <param name="cancellationToken">The <see cref="CancellationToken"/> to use to cancel the enumeration.</param>
		/// <remarks>
		/// Each <see cref="IAsyncEnumerator{T}.MoveNextAsync"/> call that returns <c>true</c> will read the next item out of the channel.
		/// <see cref="IAsyncEnumerator{T}.MoveNextAsync"/> will return false once no more data is or will ever be available to read.
		/// </remarks>
		/// <returns>The created asynchronous enumerable representaion of the ChannelReader.</returns>
		public static async IAsyncEnumerable<T> AsAsyncEnumerable<T> (this ChannelReader<T> channelReader, [EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			while (await channelReader.WaitToReadAsync (cancellationToken).ConfigureAwait (false))
			{
				while (channelReader.TryRead (out T item))
				{
					yield return item;
				}
			}
		}
	}
}
