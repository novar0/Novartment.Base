using System;
using System.Security.Cryptography;
using System.Diagnostics.Contracts;
using Novartment.Base.Text;

namespace Novartment.Base
{
	/// <summary>
	/// Трансформация для раскодирования из "Quoted-Printable" согласно RFC 2045 часть 6.7.
	/// </summary>
	public class FromQuotedPrintableConverter :
		ICryptoTransform
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
		/// <param name="inputOffset">Смещение во входном массиве байтов, начиная с которого следует использовать данные.</param>
		/// <param name="inputCount">Число байтов во входном массиве для использования в качестве данных.</param>
		/// <param name="outputBuffer">Выходной массив, в который записывается результат преобразования.</param>
		/// <param name="outputOffset">Смещение в выходном массиве байтов, начиная с которого следует записывать данные.</param>
		/// <returns>Число записанных байтов.</returns>
		public int TransformBlock (byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
		{
			if (inputBuffer == null)
			{
				throw new ArgumentNullException (nameof (inputBuffer));
			}
			if ((inputOffset < 0) || (inputOffset > inputBuffer.Length) || ((inputOffset == inputBuffer.Length) && (inputCount > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (inputOffset));
			}
			if ((inputCount < 0) || ((inputOffset + inputCount) > inputBuffer.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (inputCount));
			}
			if (outputBuffer == null)
			{
				throw new ArgumentNullException (nameof (outputBuffer));
			}
			if ((outputOffset < 0) || (outputOffset > outputBuffer.Length) || ((outputOffset == outputBuffer.Length) && (inputCount > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (outputOffset));
			}
			Contract.EndContractBlock ();
			var ret = 0;

			while (0 < inputCount--)
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
		/// <param name="inputOffset">Смещение в массиве байтов, начиная с которого следует использовать данные.</param>
		/// <param name="inputCount">Число байтов в массиве для использования в качестве данных.</param>
		/// <returns>Вычисленное преобразование.</returns>
		public byte[] TransformFinalBlock (byte[] inputBuffer, int inputOffset, int inputCount)
		{
			if (inputBuffer == null)
			{
				throw new ArgumentNullException (nameof (inputBuffer));
			}
			if ((inputOffset < 0) || (inputOffset > inputBuffer.Length) || ((inputOffset == inputBuffer.Length) && (inputCount > 0)))
			{
				throw new ArgumentOutOfRangeException (nameof (inputOffset));
			}
			if ((inputCount < 0) || ((inputOffset + inputCount) > inputBuffer.Length))
			{
				throw new ArgumentOutOfRangeException (nameof (inputCount));
			}
			Contract.EndContractBlock ();

			var outputBuffer = new byte[inputCount];
			if (inputCount > 0)
			{
				var len = TransformBlock (inputBuffer, inputOffset, inputCount, outputBuffer, 0);
				Array.Resize (ref outputBuffer, len);
			}

			_bufferOffset = 0;

			return outputBuffer;
		}

		/// <summary>
		/// Ничего не делает, так как алгоритм не занимает дополнительных ресурсов.
		/// </summary>
		public void Dispose ()
		{
		}
	}
}
