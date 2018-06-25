using System;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Media
{
	/// <summary>
	/// Перечислитель дочерних элементов EBML-элемента.
	/// </summary>
	[CLSCompliant (false)]
	public class EbmlElementCollectionEnumerator
	{
		private readonly IBufferedSource _data;
		private EbmlElement _current;
		private bool _ended;

		/// <summary>
		/// Инициализирует новый экземпляр класса EbmlElementCollectionEnumerator на основе указанного источника данных.
		/// </summary>
		/// <param name="source">Источник данных, который представляет коллекцию.</param>
		public EbmlElementCollectionEnumerator (IBufferedSource source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			if (source.BufferMemory.Length < 2)
			{
				throw new ArgumentOutOfRangeException (nameof (source));
			}

			Contract.EndContractBlock ();

			_data = source;
		}

		/// <summary>
		/// Получает текущий элемент перечислителя.
		/// </summary>
		public EbmlElement Current
		{
			get
			{
				if (_current == null)
				{
					throw new InvalidOperationException ("Can not get current element of enumeration because it not started.");
				}

				if (_ended)
				{
					throw new InvalidOperationException ("Can not get current element of enumeration because it already ended.");
				}

				return _current;
			}
		}

		/// <summary>
		/// Перемещает перечислитель к следующему элементу строки.
		/// </summary>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>true, если перечислитель был успешно перемещен к следующему элементу;
		/// false, если перечислитель достиг конца.</returns>
		public async Task<bool> MoveNextAsync (CancellationToken cancellationToken = default)
		{
			if (_ended)
			{
				return false;
			}

			// пропускаем все данные текущего элемента
			if (_current != null)
			{
				await _current.SkipAllAsync (cancellationToken).ConfigureAwait (false);
			}

			await _data.FillBufferAsync (cancellationToken).ConfigureAwait (false);
			if (_data.Count < 2)
			{
				// no more data
				_ended = true;
				_current = null;
				return false;
			}

			_current = await EbmlElement.ParseAsync (_data, cancellationToken).ConfigureAwait (false);
			return true;
		}
	}
}
