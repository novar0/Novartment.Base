using System;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Media
{
	/// <summary>
	/// Методы обработки файлов в формате "матрёшка".
	/// </summary>
	[CLSCompliant (false)]
	public static class MatroskaFile
	{
		/// <summary>
		/// Создаёт коллекцию сегментов matroska-файла, представленного указанным источником данных.
		/// </summary>
		/// <param name="source">Источник данных для чтения информации о сегменте matroska-файла.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Коллекция сегментов matroska-файла, представленном указанным источником данных.</returns>
		public static async Task<MatroskaSegmentInfo> ParseSegmentInformationAsync (
			IBufferedSource source,
			CancellationToken cancellationToken = default)
		{
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}

			var reader = new EbmlElementCollectionEnumerator (source);
			while (await reader.MoveNextAsync (cancellationToken).ConfigureAwait (false))
			{
				if (reader.Current.Id == 0x18538067UL)
				{
					var element = await MatroskaSegmentInfo.ParseAsync (reader.Current.ReadSubElements (), cancellationToken).ConfigureAwait (false);
					return element;
				}
			}

			throw new InvalidOperationException ("Segment Information not found");
		}
	}
}
