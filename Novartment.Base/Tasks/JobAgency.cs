using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Channels;

namespace Novartment.Base
{
	/// <summary>
	/// Служба, собирающая задания и асинхронно раздающая их выполнение.
	/// </summary>
	/// <typeparam name="TItem">Тип входного параметра заданий.</typeparam>
	/// <typeparam name="TResult">Тип результата, возвращаемого заданиями.</typeparam>
	/// <remarks>
	/// Отличие от библиотечного System.Threading.Channels.Channel в том, что в очередь записываются не просто объекты,
	/// а задачи, для которых потом отслеживается выполнение.
	/// </remarks>
	public class JobAgency<TItem, TResult> :
		IAsyncEnumerable<JobCompletionSource<TItem, TResult>>
	{
		private class JobAgencyAsyncEnumerator :
			IAsyncEnumerator<JobCompletionSource<TItem, TResult>>
		{
			private readonly ChannelReader<JobCompletionSource<TItem, TResult>> _reader;
			private readonly CancellationToken _cancellationToken;
			private JobCompletionSource<TItem, TResult> _current;

			internal JobAgencyAsyncEnumerator (ChannelReader<JobCompletionSource<TItem, TResult>> reader, CancellationToken cancellationToken)
			{
				_reader = reader;
				_cancellationToken = cancellationToken;
			}

			public JobCompletionSource<TItem, TResult> Current => _current;

			public ValueTask DisposeAsync () => default;

			public async ValueTask<bool> MoveNextAsync ()
			{
				_current = await _reader.ReadAsync (_cancellationToken).ConfigureAwait (false);
				return true;
			}
		}

		private readonly ChannelReader<JobCompletionSource<TItem, TResult>> _reader;
		private readonly ChannelWriter<JobCompletionSource<TItem, TResult>> _writer;
		private IAsyncEnumerator<JobCompletionSource<TItem, TResult>> _consumer = null;

		/// <summary>
		/// Инициализирует новый экземпляр JobAgency.
		/// </summary>
		public JobAgency ()
		{
			var channel = Channel.CreateUnbounded<JobCompletionSource<TItem, TResult>> ();
			_reader = channel.Reader;
			_writer = channel.Writer;
		}

		/// <summary>
		/// Returns an enumerator that iterates asynchronously through the collection.
		/// </summary>
		/// <param name="cancellationToken">A CancellationToken that may be used to cancel the asynchronous iteration.</param>
		/// <returns>An enumerator that can be used to iterate asynchronously through the collection.</returns>
		public IAsyncEnumerator<JobCompletionSource<TItem, TResult>> GetAsyncEnumerator (CancellationToken cancellationToken = default)
		{
			if (_consumer != null)
			{
				throw new InvalidOperationException ("Enumerator already created. Multiple enumerators not supported.");
			}

			_consumer = new JobAgencyAsyncEnumerator (_reader, cancellationToken);
			return _consumer;
		}

		/// <summary>
		/// Ставит задание в очередь для последующего выполнения.
		/// Выполнение задания начнётся позже, после того как будет асинхронно запрошено в вызове TakeJobAsync().
		/// </summary>
		/// <param name="jobParameter">Входной параметр задания.</param>
		/// <returns>Задача, представляющая выполнение задания.</returns>
		/// <remarks>
		/// Метод совершенно синхронный и не может быть использован по шаблону Task-based Asynchronous Pattern (TAP).
		/// Возвращаемая задача не имеет отношения к смысловой нагрузке метода.
		/// </remarks>
		public Task<TResult> OfferJob (TItem jobParameter)
		{
			var completionSource = new JobCompletionSource<TItem, TResult> (jobParameter);
			_writer.TryWrite (completionSource);
			return completionSource.Task;
		}

		/// <summary>
		/// Добавляет маркер в очередь заданий.
		/// При получении маркера в методе TakeJobAsync(), у него будет установлен флаг IsMarker.
		/// </summary>
		/// <returns>Задача, представляющая выполнение задания-маркера.</returns>
		public Task PutMarker ()
		{
			var completionSource = JobCompletionSourceMarker.Create<TItem, TResult> ();
			_writer.TryWrite (completionSource);
			return completionSource.Task;
		}
	}
}
