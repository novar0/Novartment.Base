using System;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base
{
	/// <summary>
	/// Получатель двоичных данных для последовательной записи на основе массива байтов.
	/// По мере записи массив заменяется на другой, большего размера.
	/// </summary>
	/// <remarks>
	/// Аналог System.IO.MemoryStream только для последовательной записи.
	/// Отличается тем, что записанное содержимое доступно как ReadOnlyMemory&lt;byte&gt;.
	/// </remarks>
	public class ArrayBinaryDestination :
		IBinaryDestination
	{
		private const int MaxByteArrayLength = 0x7FFFFFC7;

		private byte[] _buffer;
		private int _position;

		/// <summary>
		/// Инициализирует новый экземпляр класса ArrayBinaryDestination.
		/// </summary>
		public ArrayBinaryDestination ()
			: this (0)
		{
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса ArrayBinaryDestination c указанной начальной ёмкостью.
		/// </summary>
		/// <param name="capacity">Начальной ёмкость.</param>
		public ArrayBinaryDestination (int capacity)
		{
			if (capacity < 0)
			{
				throw new ArgumentOutOfRangeException (nameof (capacity));
			}

			_buffer = capacity != 0 ? new byte[capacity] : Array.Empty<byte> ();
		}

		/// <summary>
		/// Получает буфер, заполненный при записи.
		/// </summary>
		public ReadOnlyMemory<byte> Buffer => _buffer.AsMemory (0, _position);

		/// <summary>
		/// Асинхронно записывает в получатель указанный диапазон байтов.
		/// </summary>
		/// <param name="buffer">Буфер, из которого записываются данные.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Задача, представляющая асинхронную операцию записи.</returns>
		public Task WriteAsync (ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
		{
			Write (buffer.Span);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Записывает в получатель указанный диапазон байтов.
		/// </summary>
		/// <param name="buffer">Диапазон байтов, которые будут записаны в получатель.</param>
		public void Write (ReadOnlySpan<byte> buffer)
		{
			int newPosition = _position + buffer.Length;

			if (newPosition > _buffer.Length)
			{
				IncreaseCapacity (newPosition);
			}

			buffer.CopyTo (new Span<byte> (_buffer, _position, buffer.Length));
			_position = newPosition;
		}

		/// <summary>
		/// Указывает что запись окончена.
		/// </summary>
		public void SetComplete ()
		{
		}

		private void IncreaseCapacity (int requiredCapacity)
		{
			int newCapacity = Math.Max (Math.Max (256, requiredCapacity), _buffer.Length * 2);

			if ((uint)(_buffer.Length * 2) > MaxByteArrayLength)
			{
				newCapacity = requiredCapacity > MaxByteArrayLength ? requiredCapacity : MaxByteArrayLength;
			}

			if (newCapacity != _buffer.Length)
			{
				var newBuffer = new byte[newCapacity];
				if (_position > 0)
				{
					Array.Copy (_buffer, 0, newBuffer, 0, _position);
				}

				_buffer = newBuffer;
			}

			return;
		}
	}
}