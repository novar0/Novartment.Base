using System;
using System.Text;
using Novartment.Base.Collections.Immutable;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Mime
{
	internal static class EstimatingEncoder
	{
		private static readonly HeaderFieldBodyExtendedParameterValueEstimatingEncoder ExtendedParameterEncoder = new HeaderFieldBodyExtendedParameterValueEstimatingEncoder (Encoding.UTF8);
		private static readonly AsciiCharClassEstimatingEncoder AsciiEncoder = new AsciiCharClassEstimatingEncoder (AsciiCharClasses.Token);
		private static readonly QuotedStringEstimatingEncoder QuotedStringEncoder = new QuotedStringEstimatingEncoder (AsciiCharClasses.Visible | AsciiCharClasses.WhiteSpace);

		/// <summary>
		/// Разбивает указанный массив байтов на произвольные порции размером не более указанного,
		/// подбирая для каждой порции подходящий кодировщик из списка указанных,
		/// так чтобы минимизировать суммарный размер результата кодирования.
		/// </summary>
		/// <returns>
		/// Массив кортежей с параметрами порций.
		/// Каждый кортеж содержит выбранный кодировщик, начальную позицию в исходных данных и размер порции.
		/// </returns>
		internal static EstimatingEncoderChunk[] CutBySize (string paramName, byte[] source)
		{
			// TODO: реализовать поддержку смещения и кол-ва в source

			// проверяем что все символы ASCII
			var isEncodingNeeded = false;
			var pos = 0;
			while (pos < source.Length)
			{
				var character = source[pos];
				if ((character >= AsciiCharSet.Classes.Count) || ((AsciiCharSet.Classes[character] & (short)(AsciiCharClasses.Visible | AsciiCharClasses.WhiteSpace)) == 0))
				{
					isEncodingNeeded = true;
					break;
				}

				pos++;
			}

			// дерево сегментов, используещее свойство ParentSegment у каждого сегмента для указания на родительскую ветвь
			// изначально состоит из одного сегмента соответствующего полному источнику
			var segmentTree = new SingleLinkedListNode<EsctimatedSegment> (new EsctimatedSegment { SegmentNumber = -1, Count = source.Length });

			// лучшая конечная ветка дерева (сравнивается по суммарному TotalEncodedCount)
			EsctimatedSegment bestSegment = null;

			// цикл от корня до конечных ветвей, дерево растёт прямо внутри цикла
			while (segmentTree != null)
			{
				// отрезаем родительский элемент из дерева, анализируем его
				var currentSegment = segmentTree.Value;
				segmentTree = segmentTree.Next;

				var segmentNumber = currentSegment.SegmentNumber + 1;
				var segmentNumberChars = (segmentNumber == 0) ? 1 : (1 + (int)Math.Log10 (segmentNumber));
				var pieceExtraLength = paramName.Length + " *=;".Length + segmentNumberChars;

				// в результате оценки дерево пополнится новыми ветвями и, возможно, обновится bestResultOverall
				if (isEncodingNeeded && (segmentNumber == 0))
				{
					// значение, требующее кодирования или первый сегмент многосегментного значения
					// должны использовать только ExtendedParameterEncoder
					segmentTree = TrySegment (source, ExtendedParameterEncoder, segmentNumber, pieceExtraLength + 1, currentSegment, segmentTree, ref bestSegment);
				}
				else
				{
					segmentTree = TrySegment (source, AsciiEncoder,             segmentNumber, pieceExtraLength, currentSegment, segmentTree, ref bestSegment);
					segmentTree = TrySegment (source, QuotedStringEncoder,      segmentNumber, pieceExtraLength, currentSegment, segmentTree, ref bestSegment);
					segmentTree = TrySegment (source, ExtendedParameterEncoder, segmentNumber, pieceExtraLength + 1, currentSegment, segmentTree, ref bestSegment);
				}
			}

			if (bestSegment == null)
			{
				throw new InvalidOperationException ("Data not acceptable for available encoders.");
			}

			var resultValues = new EstimatingEncoderChunk[bestSegment.SegmentNumber + 1];

			// bestSegment является последним, разворачиваем цепочку переходя на предыдущий
			var part = bestSegment;
			while ((part != null) && (part.ProcessedCount > 0))
			{
				resultValues[part.SegmentNumber] = new EstimatingEncoderChunk (part.Encoder, part.Offset, part.ProcessedCount);
				part = part.ParentSegment;
			}

			return resultValues;
		}

		// оцениваем сегмент
		// если сегмент не удастся закодировать полностью, то на нём будет выращена новая ветка с остатком и возвращено обновлённое дерево
		// если сегмент удалось закодировать полностью и суммарная длина от корня меньше bestSegment, то на нём будет выращена новая ветка (не в дереве) и записана в bestSegment
		// в результате оценки segmentTree пополнится новыми ветвями и, возможно, обновится bestSegment
		private static SingleLinkedListNode<EsctimatedSegment> TrySegment (
			byte[] source,
			IEstimatingEncoder encoder,
			int segmentNumber,
			int pieceExtraLength,
			EsctimatedSegment segment,
			SingleLinkedListNode<EsctimatedSegment> segmentTree,
			ref EsctimatedSegment bestSegment)
		{
			var offset = segment.Offset + segment.ProcessedCount;
			var count = segment.Count - segment.ProcessedCount;

			var (bytesProduced, bytesConsumed) = encoder.Estimate (
				source: source,
				offset: offset,
				count: count,
				maxOutCount: HeaderEncoder.MaxLineLengthRecommended - pieceExtraLength,
				segmentNumber: segmentNumber,
				true);
			if (bytesConsumed > 0)
			{
				var newEncodedDestinationCount = bytesProduced + segment.TotalEncodedCount + pieceExtraLength;
				if ((bestSegment == null) || (newEncodedDestinationCount < bestSegment.TotalEncodedCount))
				{
					// если лучший результат ещё не известен или известен, но хуже текущего
					if (bytesConsumed < count)
					{
						// не весь источник потреблён в кодировании и ещё есть шанс стать лучше чем текущий bestResultOverall, поэтому добавляем остаток как следующий вариант
						var newSegment = new EsctimatedSegment
						{
							SegmentNumber = segmentNumber,
							ParentSegment = segment,
							Encoder = encoder,
							Offset = offset,
							Count = count,
							ProcessedCount = bytesConsumed,
							TotalEncodedCount = newEncodedDestinationCount,
						};
						segmentTree = segmentTree.AddItem (newSegment);
					}
					else
					{
						// весь источник удалось закодировать, сравниваем результат с bestResultOverall
						if ((bestSegment == null) || (newEncodedDestinationCount < bestSegment.TotalEncodedCount))
						{
							var newSegment = new EsctimatedSegment
							{
								SegmentNumber = segmentNumber,
								ParentSegment = segment,
								Encoder = encoder,
								Offset = offset,
								Count = count,
								ProcessedCount = bytesConsumed,
								TotalEncodedCount = newEncodedDestinationCount,
							};
							bestSegment = newSegment;
						}
					}
				}
			}
			else
			{
				// для кодировщика который не может взять ни байта
				// находим ближайший подходящий байт и делаем на нём место разрезания для остальных кодировщиков
				var nextValid = encoder.FindValid (
					source,
					offset,
					count);
				if (nextValid >= 0)
				{
					nextValid -= offset;
					if (encoder != ExtendedParameterEncoder)
					{
						segmentTree = CreateSegment (source, ExtendedParameterEncoder, nextValid, segment, pieceExtraLength, segmentNumber, segmentTree);
					}

					if (encoder != AsciiEncoder)
					{
						segmentTree = CreateSegment (source, AsciiEncoder, nextValid, segment, pieceExtraLength, segmentNumber, segmentTree);
					}

					if (encoder != QuotedStringEncoder)
					{
						segmentTree = CreateSegment (source, QuotedStringEncoder, nextValid, segment, pieceExtraLength, segmentNumber, segmentTree);
					}
				}
			}

			return segmentTree;
		}

		private static SingleLinkedListNode<EsctimatedSegment> CreateSegment (
			byte[] source,
			IEstimatingEncoder encoder,
			int count,
			EsctimatedSegment segment,
			int pieceExtraLength,
			int segmentNumber,
			SingleLinkedListNode<EsctimatedSegment> segmentTree)
		{
			var (bytesProduced, bytesConsumed) = encoder.Estimate (
				source: source,
				offset: segment.Offset + segment.ProcessedCount,
				count: count,
				maxOutCount: HeaderEncoder.MaxLineLengthRecommended - pieceExtraLength,
				segmentNumber: segmentNumber,
				isLastSegment: false);
			if (bytesConsumed > 0)
			{
				var newEncodedDestinationCount = bytesProduced + segment.TotalEncodedCount + pieceExtraLength;
				var variant = new EsctimatedSegment
				{
					SegmentNumber = segmentNumber,
					ParentSegment = segment,
					Encoder = encoder,
					Offset = segment.Offset + segment.ProcessedCount,
					Count = segment.Count - segment.ProcessedCount,
					ProcessedCount = bytesConsumed,
					TotalEncodedCount = newEncodedDestinationCount,
				};
				segmentTree = segmentTree.AddItem (variant);
			}

			return segmentTree;
		}

		internal class EsctimatedSegment
		{
			internal int SegmentNumber { get; set; }

			internal EsctimatedSegment ParentSegment { get; set; }

			internal IEstimatingEncoder Encoder { get; set; }

			internal int Offset { get; set; }

			internal int Count { get; set; }

			internal int ProcessedCount { get; set; }

			internal int TotalEncodedCount { get; set; }
		}
	}
}
