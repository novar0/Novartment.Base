using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Collections;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Кодер для семантических элементов MIME-сообщений.
	/// На входе получает объект с исходными параметрами,
	/// на выходе выдаёт его MIME-кодированное текстовое представление.
	/// </summary>
	internal static class HeaderEncoder
	{
		/// <summary>
		/// Максимально допустимая длина одной строки.
		/// Определено в RFC 2822 часть 2.1.1.
		/// </summary>
		internal static readonly int MaxLineLengthRequired = 998;

		/// <summary>
		/// Максимально рекомендуемая длина одной строки.
		/// Определено в RFC 2822 часть 2.1.1.
		/// </summary>
		internal static readonly int MaxLineLengthRecommended = 78;

		/// <summary>
		/// Максимально допустимая длина 'encoded-word'.
		/// Определено в RFC 2047 часть 2.
		/// </summary>
		internal static readonly int MaxEncodedWordLength = 75;

		/// <summary>
		/// Кодирует указанное значение в значение поля заголовка типа 'unstructured'.
		/// </summary>
		/// <param name="result">Набор элементов значения поля заголовка куда будут добавлены кодированные элементы.</param>
		/// <param name="text">Значение, которое надо представить в ограничениях типа 'unstructured'.</param>
		/// <remarks>
		/// Используется значение поля DefaultEncoding.
		/// Возвращаются отдельные слова, а склеиваются только если это даёт большой выигрыш по размеру.
		/// </remarks>
		internal static void EncodeUnstructured (IAdjustableCollection<string> result, string text)
		{
			// важно сохранять читаемость после кодировки, поэтому в ущерб размеру используем более читаемую кодировку
			if (text == null)
			{
				throw new ArgumentNullException (nameof (text));
			}

			if (text.Length >= MaxLineLengthRequired)
			{
				throw new ArgumentOutOfRangeException (nameof (text));
			}

			Contract.EndContractBlock ();

			if (text.Length > 0)
			{
				// текст рабивается на последовательности слов, которые удобно представить в одном виде (напрямую или в виде encoded-word)
				var bytes = Encoding.UTF8.GetBytes (text);
				var position = 0;
				var prevSequenceIsWordEncoded = false;
				while (true)
				{
					var wordStr = HeaderFieldBodyEncoder.EncodeNextElement (bytes, TextSemantics.Unstructured, ref position, ref prevSequenceIsWordEncoded);
					if (wordStr == null)
					{
						break;
					}

					result.Add (wordStr);
				}
			}
		}

		/// <summary>
		/// Кодирует указанное '*tokens' значение в значение поля заголовка.
		/// </summary>
		/// <param name="result">Набор элементов значения поля заголовка куда будут добавлены кодированные элементы.</param>
		/// <param name="value">'*tokens' значение.</param>
		/// <remarks>Все последовательности пробельных символов будут удалены.</remarks>
		internal static void EncodeTokens (IAdjustableCollection<string> result, string value)
		{
			// An 'encoded-word' MUST NOT be used in a '*tokens' header field.
			var isEmpty = string.IsNullOrEmpty (value);
			if (isEmpty)
			{
				return;
			}

			var pos = 0;
			int wordStart = -1;
			while (pos < value.Length)
			{
				var currentChar = value[pos];
				var charClass = (currentChar < AsciiCharSet.Classes.Count) ?
					(AsciiCharClasses)AsciiCharSet.Classes[currentChar] :
					AsciiCharClasses.None;
				if ((charClass & AsciiCharClasses.WhiteSpace) != 0)
				{
					if (wordStart >= 0)
					{
						result.Add (value.Substring (wordStart, pos - wordStart));
						wordStart = -1;
					}
				}
				else
				{
					if ((charClass & AsciiCharClasses.Visible) != 0)
					{
						if (wordStart < 0)
						{
							wordStart = pos;
						}
					}
					else
					{
						if ((currentChar != 0x0d) || ((pos + 1) >= value.Length) || (value[pos + 1] != 0x0a))
						{
							throw new FormatException (FormattableString.Invariant (
								$"Value contains invalid for 'token' character U+{currentChar:x}. Expected characters are U+0009 and U+0020...U+007E."));
						}

						if (wordStart >= 0)
						{
							result.Add (value.Substring (wordStart, pos - wordStart));
							wordStart = -1;
							pos++;
						}
					}
				}

				pos++;
			}

			if (wordStart >= 0)
			{
				// добавляем только куски со словами (пробельные куски в конце отбрасываем)
				result.Add (value.Substring (wordStart, pos - wordStart));
			}
		}

		/// <summary>
		/// Кодирует указанное значение в значение поля заголовка типа 'phrase'.
		/// </summary>
		/// <param name="result">Набор элементов значения поля заголовка куда будут добавлены кодированные элементы.</param>
		/// <param name="text">Значение, которое надо представить в ограничениях типа 'phrase'.</param>
		/// <remarks>
		/// Используется значение поля DefaultEncoding.
		/// Приоритет представления элементов значения: атом / квотированная строка / encoded-word.
		/// Возвращаются отдельные слова, а склеиваются только если это даёт большой выигрыш по размеру.
		/// </remarks>
		internal static void EncodePhrase (IAdjustableCollection<string> result, string text)
		{
			if (text == null)
			{
				throw new ArgumentNullException (nameof (text));
			}

			if ((text.Length < 1) || (text.Length >= MaxLineLengthRequired))
			{
				throw new ArgumentOutOfRangeException (nameof (text));
			}

			Contract.EndContractBlock ();

			var bytes = Encoding.UTF8.GetBytes (text);
			var position = 0;
			var prevSequenceIsWordEncoded = false;
			while (true)
			{
				var wordStr = HeaderFieldBodyEncoder.EncodeNextElement (bytes, TextSemantics.Phrase, ref position, ref prevSequenceIsWordEncoded);
				if (wordStr == null)
				{
					break;
				}

				result.Add (wordStr);
			}
		}

		/// <summary>
		/// Кодирует Mailbox в набор элементов значения поля заголовка.
		/// </summary>
		/// <param name="result">Набор элементов значения поля заголовка куда будут добавлены кодированные элементы.</param>
		/// <param name="mailbox">Mailbox который будет представлен в виде значения поля заголовка.</param>
		/// <reamarks>
		/// Mailbox будет представлен одной или двумя строками (в зависимости от наличия Name).
		/// </reamarks>
		internal static void EncodeMailbox (IAdjustableCollection<string> result, Mailbox mailbox)
		{
			if (mailbox == null)
			{
				throw new ArgumentNullException (nameof (mailbox));
			}

			Contract.EndContractBlock ();

			var isMailboxEmpty = string.IsNullOrEmpty (mailbox.Name);
			if (!isMailboxEmpty)
			{
				EncodePhrase (result, mailbox.Name);
			}

			result.Add (mailbox.Address.ToAngleString ());
		}

		internal static void EncodeHeaderFieldParameter (IAdjustableCollection<string> result, HeaderFieldParameter parameter)
		{
			var encoder = HeaderFieldBodyParameterEncoder.Parse (parameter.Name, parameter.Value);
			while (true)
			{
				var element = encoder.GetNextSegment ();
				if (element == null)
				{
					break;
				}

				result.Add (element);
			}
		}

		/// <summary>
		/// Записывает коллекцию полей в указанный получатель двоичных данных.
		/// </summary>
		/// <param name="fields">Коллекция полей, которая будет записана в получатель двоичных данных.</param>
		/// <param name="destination">Получатель двоичных данных, в который будет записана указанная коллекция полей.</param>
		/// <param name="cancellationToken">Токен для отслеживания запросов отмены.</param>
		/// <returns>Суммарное количество байтов, записанных в получатель двоичных данных.</returns>
		internal static Task<int> SaveHeaderAsync (
			IReadOnlyCollection<HeaderFieldBuilder> fields,
			IBinaryDestination destination,
#pragma warning disable CA1801 // Review unused parameters
			CancellationToken cancellationToken = default)
#pragma warning restore CA1801 // Review unused parameters
		{
			if (fields == null)
			{
				throw new ArgumentNullException (nameof (fields));
			}

			if (destination == null)
			{
				throw new ArgumentNullException (nameof (destination));
			}

			Contract.EndContractBlock ();

			return SaveHeaderAsyncStateMachine ();

			async Task<int> SaveHeaderAsyncStateMachine ()
			{
				int totalSize = 0;
				var bytes = ArrayPool<byte>.Shared.Rent (HeaderDecoder.MaximumHeaderFieldBodySize);
				foreach (var fieldBuilder in fields)
				{
					cancellationToken.ThrowIfCancellationRequested ();
					var size = fieldBuilder.CreateBinaryTransportRepresentation (bytes, HeaderEncoder.MaxLineLengthRecommended);
					await destination.WriteAsync (bytes.AsMemory (0, size), cancellationToken).ConfigureAwait (false);
					totalSize += size;
				}

				ArrayPool<byte>.Shared.Return (bytes);

				return totalSize;
			}
		}
	}
}
