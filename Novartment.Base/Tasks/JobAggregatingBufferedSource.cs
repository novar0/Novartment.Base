using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// Источник данных, представленный байтовым буфером,
	/// предоставляющий данные последовательно из источников,
	/// поставляемых поставщиком задач.
	/// Потребление источников отражается в статусе задач.
	/// </summary>
	public class JobAggregatingBufferedSource : AggregatingBufferedSourceBase
	{
		private readonly IAsyncEnumerator<JobCompletionSource<IBufferedSource, int>> _sourceProvider;
		private JobCompletionSource<IBufferedSource, int> _currentSourceJob;

		/// <summary>
		/// Инициализирует новый экземпляр AggregatingBufferedSource использующий в качестве буфера предоставленный массив байтов и
		/// предоставляющий данные из источников, поставляемых указанным поставщиком.
		/// </summary>
		/// <param name="buffer">Массив байтов, который будет буфером источника.</param>
		/// <param name="sourceProvider">
		/// Поставщик источников.
		/// Источник-маркер будет означать окончание поставки.</param>
		public JobAggregatingBufferedSource (Memory<byte> buffer, IAsyncEnumerable<JobCompletionSource<IBufferedSource, int>> sourceProvider)
			: base (buffer)
		{
			if (sourceProvider == null)
			{
				throw new ArgumentNullException (nameof (sourceProvider));
			}

			Contract.EndContractBlock ();

			_sourceProvider = sourceProvider.GetAsyncEnumerator ();
			_currentSourceJob = new JobCompletionSource<IBufferedSource, int> (this.CurrentSource);
		}

		/// <summary>
		/// Устанавливает новое задание-источник.
		/// </summary>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Признак успешной установки нового задания-источника.</returns>
		protected override async ValueTask<bool> MoveToNextSource (CancellationToken cancellationToken)
		{
			var success = await _sourceProvider.MoveNextAsync ().ConfigureAwait (false);
			if (!success)
			{
				return false;
			}

			var newJob = _sourceProvider.Current;
			if (newJob == null)
			{
				throw new InvalidOperationException ("Contract violation: IJobProvider.TakeJobAsync() returned null.");
			}

			if (newJob.IsMarker)
			{
				newJob.TrySetResult (0);
				return false;
			}

			this.CurrentSource = newJob.Item;
			_currentSourceJob = newJob;
			return true;
		}

		/// <summary>
		/// Проверяет, не закончился ли текущий источник и устанавливает соответственно признак выполнения задания.
		/// </summary>
		protected override void OnSourceConsumed ()
		{
			if (this.CurrentSource.IsExhausted && (this.CurrentSource.Count < 1))
			{
				_currentSourceJob.TrySetResult (0);
			}
		}
	}
}
