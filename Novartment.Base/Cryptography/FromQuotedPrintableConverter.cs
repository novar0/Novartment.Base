using System;
using Novartment.Base.Text;

namespace Novartment.Base
{
	/// <summary>
	/// Трансформация для раскодирования из "Quoted-Printable" согласно RFC 2045 часть 6.7.
	/// </summary>
	public sealed class FromQuotedPrintableConverter :
		ISpanCryptoTransform
	{
		private readonly byte[] _buffer = new byte[3];
		private int _bufferOffset = 0;

		/// <summary>
		/// Инициализирует новый экземпляр класса FromQuotedPrintableConverter.
		/// </summary>
		public FromQuotedPrintableConverter ()
		{
		}

		/// <summary>
		/// Получает размер входного блока.
		/// </summary>
		public int InputBlockSize => 1;

		/// <summary>
		/// Получает размер выходного блока.
		/// </summary>
		public int OutputBlockSize => 1;

		/// <summary>
		/// Получает значение, указывающее на возможность повторного использования текущего преобразования.
		/// </summary>
		public bool CanReuseTransform => true;

		/// <summary>
		/// Получает значение, указывающее на возможность преобразования нескольких блоков.
		/// </summary>
		public bool CanTransformMultipleBlocks => true;

		/// <summary>
		/// Преобразует заданную область входного массива байтов и копирует результат в заданную область выходного массива байтов.
		/// </summary>
		/// <param name="inputBuffer">Входные данные, для которых вычисляется преобразование.</param>
		/// <param name="outputBuffer">Выходной массив, в который записывается результат преобразования.</param>
		/// <returns>Число записанных байтов.</returns>
		public int TransformBlock (ReadOnlySpan<byte> inputBuffer, Span<byte> outputBuffer)
		{
			var inputOffset = 0;
			var inputCount = inputBuffer.Length;
			var outputOffset = 0;
			var ret = 0;

			while (inputCount-- > 0)
			{
				var octet = inputBuffer[inputOffset++];

				if (_bufferOffset == 0)
				{
					if (octet == 0x3d)
					{ // '=' 0x3d
						_buffer[_bufferOffset++] = octet;
					}
					else
					{
						outputBuffer[outputOffset++] = octet;
						ret++;
					}
				}
				else
				{
					// quoted char
					_buffer[_bufferOffset++] = octet;
				}

				if (_bufferOffset == 3)
				{
					// dequote
					if ((_buffer[1] == 0x0d) && (_buffer[2] == 0x0a))
					{
						// soft newline (CarriageReturnLinefeed)
						_bufferOffset = 0;
					}
					else
					{
						if ((_buffer[1] == 0x0d) || (_buffer[1] == 0x0a))
						{
							// soft newline (CR, LF)
							if (_buffer[2] == 0x3d)
							{
								_bufferOffset = 1;
							}
							else
							{
								outputBuffer[outputOffset++] = _buffer[2];
								ret++;

								_bufferOffset = 0;
							}
						}
						else
						{
							outputBuffer[outputOffset++] = Hex.ParseByte ((char)_buffer[1], (char)_buffer[2]);
							ret++;

							_bufferOffset = 0;
						}
					}
				}
			}

			return ret;
		}

		/// <summary>
		/// Преобразует заданную область заданного массива байтов.
		/// </summary>
		/// <param name="inputBuffer">Входные данные, для которых вычисляется преобразование.</param>
		/// <returns>Вычисленное преобразование.</returns>
		public ReadOnlyMemory<byte> TransformFinalBlock (ReadOnlySpan<byte> inputBuffer)
		{
			if (inputBuffer.Length < 1)
			{
				_bufferOffset = 0;
				return default;
			}

			var outputBuffer = new byte[inputBuffer.Length];
			var len = TransformBlock (inputBuffer, outputBuffer);
			_bufferOffset = 0;
			return outputBuffer.AsMemory (0, len);
		}

		/// <summary>
		/// Ничего не делает, так как алгоритм не занимает дополнительных ресурсов.
		/// </summary>
		public void Dispose ()
		{
		}
	}
}
