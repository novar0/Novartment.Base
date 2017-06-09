using System;
using System.Diagnostics.Contracts;
using System.IO;

namespace Novartment.Base.BinaryStreaming
{
	/// <summary>
	/// Методы расширения для преобразования Stream - IBufferedSource/IBinaryDestination.
	/// </summary>
	public static partial class StreamExtensions
	{
		/// <summary>
		/// Создаёт источник данных, представленный байтовым буфером,
		/// при запросе данных из которого, данные будут считаны из указанного потока.
		/// </summary>
		/// <param name="readableStream">Поток, из которого будут считываться данные, запрошенные для чтения в источнике данных,
		/// представленным байтовым буфером.</param>
		/// <param name="buffer">Байтовый буфер, в котором будут содержаться считанные из потока данные.</param>
		/// <returns>Источник данных, представленный байтовым буфером, при запросе данных из которого,
		/// данные будут считаны из указанного потока.</returns>
		public static IFastSkipBufferedSource AsBufferedSource (this Stream readableStream, byte[] buffer)
		{
			if (readableStream == null)
			{
				throw new ArgumentNullException (nameof (readableStream));
			}

			if (buffer == null)
			{
				throw new ArgumentNullException (nameof (buffer));
			}

			if (!readableStream.CanRead)
			{
				throw new ArgumentOutOfRangeException (nameof (readableStream));
			}

			if (buffer.Length < 1)
			{
				throw new ArgumentOutOfRangeException (nameof (buffer));
			}

			Contract.EndContractBlock ();

			// нельзя использовать (readableStream as _BufferedSourceStream).BaseStream, потому что не будет использован указанный buffer
			return new StreamBufferedSource (readableStream, buffer);
		}

		/// <summary>
		/// Создаёт получатель двоичных данных, при записи в который данные будут переданы в указанный поток.
		/// При окончании записи в получатель, поток будет закрыт.
		/// </summary>
		/// <param name="writableStream">Поток, в который будут передаваться все данные, записываемые в получатель двоичных данных.</param>
		/// <returns>Получатель двоичных данных, при записи в который данные будут переданы в указанный поток.</returns>
		public static IBinaryDestination AsBinaryDestination (this Stream writableStream)
		{
			if (writableStream == null)
			{
				throw new ArgumentNullException (nameof (writableStream));
			}

			if (!writableStream.CanWrite)
			{
				throw new ArgumentOutOfRangeException (nameof (writableStream));
			}

			Contract.EndContractBlock ();

			var destinaton = writableStream as BinaryDestinationStream;
			return (destinaton != null) ?
				destinaton.BaseBinaryDestination :
				new StreamBinaryDestination (writableStream);
		}

		/// <summary>
		/// Создаёт поток только для чтения, получающий данные из указанного источника данных, представленного байтовым буфером.
		/// </summary>
		/// <param name="source">Источник данных, представленный байтовым буфером, из котрого будет получать данные поток.</param>
		/// <returns>Поток только для чтения, получающий данные из указанного источника данных, представленного байтовым буфером.</returns>
		public static Stream AsReadOnlyStream (this IBufferedSource source)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			Contract.EndContractBlock ();

			// нельзя использовать (source as _StreamBufferedSource).BaseStream, потому что у потока должен быть только один считыватель
			return new BufferedSourceStream (source);
		}

		/// <summary>
		/// Создаёт поток только для записи, передающий все записываемые данные в указанный получатель двоичных данных.
		/// Закрытие потока будет означать завершение записи для получателя двоичных данных.
		/// </summary>
		/// <param name="destination">Получатель двоичных данных, в который будут передаваться все записываемые в поток данные.</param>
		/// <returns>Поток только для записи, передающий все записываемые данные в указанный получатель двоичных данных.</returns>
		public static Stream AsWriteOnlyStream (this IBinaryDestination destination)
		{
			if (destination == null)
			{
				throw new ArgumentNullException (nameof (destination));
			}

			Contract.EndContractBlock ();

			var strm = destination as StreamBinaryDestination;
			return (strm != null) ?
				strm.BaseStream :
				new BinaryDestinationStream (destination);
		}
	}
}
