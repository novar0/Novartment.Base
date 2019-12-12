using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.Collections;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// Источник данных, представленный байтовым буфером,
	/// предоставляющий данные последовательно из коллекции источников.
	/// </summary>
	public class EnumerableAggregatingBufferedSource : AggregatingBufferedSourceBase
	{
		private readonly IAsyncEnumerator<IBufferedSource> _sourceProvider;
		private bool _isProviderCompleted = false;

		/// <summary>
		/// Инициализирует новый экземпляр EnumerableAggregatingBufferedSource использующий в качестве буфера предоставленный массив байтов и
		/// предоставляющий данные из источников указанного перечислителя.
		/// </summary>
		/// <param name="buffer">Массив байтов, который будет буфером источника.</param>
		/// <param name="sources">Перечислитель, поставляющий источники данных.</param>
		public EnumerableAggregatingBufferedSource (Memory<byte> buffer, IEnumerable<IBufferedSource> sources)
			: this (buffer, sources?.AsAsyncEnumerable ())
		{
		}

		/// <summary>
		/// Инициализирует новый экземпляр EnumerableAggregatingBufferedSource использующий в качестве буфера предоставленный массив байтов и
		/// предоставляющий данные из источников указанного перечислителя.
		/// </summary>
		/// <param name="buffer">Массив байтов, который будет буфером источника.</param>
		/// <param name="sources">Перечислитель, поставляющий источники данных.</param>
		public EnumerableAggregatingBufferedSource (Memory<byte> buffer, IAsyncEnumerable<IBufferedSource> sources)
			: base (buffer)
		{
			if (sources == null)
			{
				throw new ArgumentNullException (nameof (sources));
			}

			Contract.EndContractBlock ();

			_sourceProvider = sources.GetAsyncEnumerator ();
		}

		/// <summary>
		/// Устанавливает новое задание-источник.
		/// </summary>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Признак успешной установки нового задания-источника.</returns>
		protected override async ValueTask<bool> MoveToNextSource (CancellationToken cancellationToken)
		{
			if (_isProviderCompleted)
			{
				return false;
			}

			var success = await _sourceProvider.MoveNextAsync ().ConfigureAwait (false);
			if (!success)
			{
				_isProviderCompleted = true;
				await _sourceProvider.DisposeAsync ().ConfigureAwait (false);
				return false;
			}

			var newSource = _sourceProvider.Current;
			if (newSource == null)
			{
				throw new InvalidOperationException ("Contract violation: null-source.");
			}

			this.CurrentSource = newSource;
			return true;
		}
	}
}
