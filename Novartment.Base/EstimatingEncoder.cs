using System;
using System.Collections.Generic;
using static System.Linq.Enumerable;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Novartment.Base.Collections.Immutable;

namespace Novartment.Base
{
	/// <summary>
	/// Методы для применения набора кодировщиков.
	/// </summary>
	public static class EstimatingEncoder
	{
		/// <summary>
		/// Позиция/размер порции данных и выбранный для неё кодировщик.
		/// </summary>
		public struct ChunkEncoderSelection
		{
			/// <summary>
			/// Кодировщик, который выбран для порции.
			/// </summary>
			public IEstimatingEncoder Encoder { get; }

			/// <summary>
			/// Начальная позиция порции в исходных данных.
			/// </summary>
			public int Offset { get; }

			/// <summary>
			/// Размер порции.
			/// </summary>
			public int Count { get; }

			/// <summary>
			/// Инициализирует новый экземпляр класса ChunkEncoderSelection с указанным
			/// кодировщиком, начальной позицией и размером порции.
			/// </summary>
			/// <param name="encoder">Получает кодировщик, который выбран для порции.</param>
			/// <param name="offset">Получает начальная позиция порции в исходных данных.</param>
			/// <param name="count">Получает размер порции.</param>
			public ChunkEncoderSelection (IEstimatingEncoder encoder, int offset, int count)
			{
				this.Encoder = encoder;
				this.Offset = offset;
				this.Count = count;
			}

			/// <summary>
			/// Деконструирует данные.
			/// </summary>
			/// <param name="encoder">Получает количество байтов, произведённых в результате кодирования.</param>
			/// <param name="offset">Получает количество байтов, использованных при кодировании.</param>
			/// <param name="count">Получает количество байтов, использованных при кодировании.</param>
			public void Deconstruct (out IEstimatingEncoder encoder, out int offset, out int count)
			{
				encoder = this.Encoder;
				offset = this.Offset;
				count = this.Count;
			}
		}

		/// <summary>
		/// Разбивает указанный массив байтов на произвольные порции размером не более указанного,
		/// подбирая для каждой порции подходящий кодировщик из списка указанных,
		/// так чтобы минимизировать суммарный размер результата кодирования.
		/// </summary>
		/// <param name="funcEncoders">Список кодировщиков, которые будут использованы для подбора оптимального результата.</param>
		/// <param name="source">Исходный массив байтов, который должен быть закодирован кодировщиками.</param>
		/// <param name="maxOutCount">Максимально допустимый размер результата кодирования одной порции.</param>
		/// <param name="funcExtraLength">
		/// Функция, возвращающая размер дополнительных данных (дополнительно к результату кодирования исходных данных),
		/// которые порождает переданный ей кодировщик для переданного номера порции.
		/// Этот размер будет вычтен из указанного максимально допустимого результата кодирования одной порции.
		/// </param>
		/// <returns>
		/// Массив кортежей с параметрами порций.
		/// Каждый кортеж содержит выбранный кодировщик, начальную позицию в исходных данных и размер порции.
		/// </returns>
		[SuppressMessage (
			"Microsoft.Design",
			"CA1006:DoNotNestGenericTypesInMemberSignatures",
			Justification = "The caller doesn't have to cope with nested generics, he is just passing a lambda expression.")]
		public static ChunkEncoderSelection[] CutBySize (
			Func<int, IReadOnlyCollection<IEstimatingEncoder>> funcEncoders,
			byte[] source,
			int maxOutCount,
			Func<int, IEstimatingEncoder, int> funcExtraLength)
		{
			// TODO: реализовать поддержку смещения и кол-ва в source

			if (funcEncoders == null)
			{
				throw new ArgumentNullException (nameof (funcEncoders));
			}
			if (source == null)
			{
				throw new ArgumentNullException (nameof (source));
			}
			if (maxOutCount < 1)
			{
				throw new ArgumentOutOfRangeException (nameof (maxOutCount));
			}
			if (funcExtraLength == null)
			{
				throw new ArgumentNullException (nameof (funcExtraLength));
			}
			Contract.EndContractBlock ();

			if (source.Length < 1)
			{
				return Array.Empty<ChunkEncoderSelection> ();
			}

			EncodeResult bestResultOverall = null;
			var variants = new SingleLinkedListNode<EncodeResult> (
				new EncodeResult { Index = -1, SourceCount = source.Length },
				null);
			while (variants != null)
			{
				var variant = variants.Value;
				variants = variants.Next;

				var segmentNumber = variant.Index + 1;
				var encoders = funcEncoders.Invoke (segmentNumber);
				foreach (var encoder in encoders)
				{
					var pieceExtraLength = funcExtraLength.Invoke (segmentNumber, encoder);
					var (bytesProduced, bytesConsumed) = encoder.Estimate (
						source,
						variant.SourceOffset + variant.EncodedSource,
						variant.SourceCount - variant.EncodedSource,
						maxOutCount - pieceExtraLength,
						segmentNumber, true);
					if (bytesConsumed > 0)
					{
						var newEncodedDestination = bytesProduced + variant.EncodedDestination + pieceExtraLength;
						if ((bestResultOverall == null) || (newEncodedDestination < bestResultOverall.EncodedDestination))
						{
							var resultVariant = new EncodeResult
							{
								Index = segmentNumber,
								Parent = variant,
								Encoder = encoder,
								SourceOffset = variant.SourceOffset + variant.EncodedSource,
								SourceCount = variant.SourceCount - variant.EncodedSource,
								EncodedSource = bytesConsumed,
								EncodedDestination = newEncodedDestination
							};
							if (resultVariant.EncodedSource < resultVariant.SourceCount)
							{
								variants = variants.AddItem (resultVariant);
							}
							else
							{
								if ((bestResultOverall == null) || (resultVariant.EncodedDestination < bestResultOverall.EncodedDestination))
								{
									bestResultOverall = resultVariant;
								}
							}
						}
					}
					else
					{
						// для кодировщика который не может взять ни байта
						// находим ближайший подходящий байт и
						// делаем на нём место разрезания для остальных кодировщиков
						variants = FindEncoderValidPositionAndCreateVariantsForOtherEncoders (encoder, encoders, source, variant, pieceExtraLength, segmentNumber, maxOutCount, variants);
					}
				}
			}
			if (bestResultOverall == null)
			{
				throw new InvalidOperationException ("Data not acceptable for specified encoders.");
			}

			var resultValues = new ChunkEncoderSelection[bestResultOverall.Index + 1];
			var part = bestResultOverall;
			while ((part != null) && (part.EncodedSource > 0))
			{
				var idx = part.Index;
				resultValues[idx] = new ChunkEncoderSelection (part.Encoder, part.SourceOffset, part.EncodedSource);
				part = part.Parent;
			}

			return resultValues;
		}

		private static SingleLinkedListNode<EncodeResult> FindEncoderValidPositionAndCreateVariantsForOtherEncoders (
			IEstimatingEncoder encoder,
			IReadOnlyCollection<IEstimatingEncoder> encoders,
			byte[] source,
			EncodeResult variant,
			int pieceExtraLength,
			int segmentNumber,
			int maxOutCount,
			SingleLinkedListNode<EncodeResult> variants)
		{
			// находим ближайший подходящий байт
			var nextValid = encoder.FindValid (
				source,
				variant.SourceOffset + variant.EncodedSource,
				variant.SourceCount - variant.EncodedSource);
			if (nextValid >= 0)
			{
				// делаем место разрезания для остальных кодировщиков
				foreach (var encoder2 in encoders)
				{
					if (encoder2 == encoder)
					{
						continue;
					}
					var tuple = encoder2.Estimate (
						source,
						variant.SourceOffset + variant.EncodedSource,
						nextValid - (variant.SourceOffset + variant.EncodedSource),
						maxOutCount - pieceExtraLength,
						segmentNumber, false);
					if (tuple.BytesConsumed > 0)
					{
						var newEncodedDestination2 = tuple.BytesProduced + variant.EncodedDestination + pieceExtraLength;
						var resultVariant2 = new EncodeResult
						{
							Index = segmentNumber,
							Parent = variant,
							Encoder = encoder2,
							SourceOffset = variant.SourceOffset + variant.EncodedSource,
							SourceCount = variant.SourceCount - variant.EncodedSource,
							EncodedSource = tuple.BytesConsumed,
							EncodedDestination = newEncodedDestination2
						};
						variants = variants.AddItem (resultVariant2);
					}
				}
			}
			return variants;
		}

		internal class EncodeResult
		{
			internal int Index;
			internal EncodeResult Parent;
			internal IEstimatingEncoder Encoder;
			internal int SourceOffset;
			internal int SourceCount;
			internal int EncodedSource;
			internal int EncodedDestination;
		}
	}
}
