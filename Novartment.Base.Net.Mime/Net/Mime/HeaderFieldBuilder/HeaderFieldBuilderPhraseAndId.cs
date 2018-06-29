using System;
using System.Diagnostics.Contracts;
using System.Text;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Mime
{
	public class HeaderFieldBuilderPhraseAndId : HeaderFieldBuilder
	{
		private readonly string _id;
		private readonly string _phrase = null;
		private int _pos = 0;
		private bool _prevSequenceIsWordEncoded = false;
		private bool _finished = false;
		private byte[] _phraseBytes = null;
		private int _phraseBytesSize = 0;

		/// <summary>
		/// Создает поле заголовка из идентификатора и 'phrase'.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="id">Идентификатор (значение типа 'dot-atom-text').</param>
		/// <param name="phrase">Произвольная 'phrase'.</param>
		/// <returns>Поле заголовка.</returns>
		public HeaderFieldBuilderPhraseAndId (HeaderFieldName name, string id, string phrase = null)
			: base (name)
		{
			if (id == null)
			{
				throw new ArgumentNullException (nameof (id));
			}

			if (!AsciiCharSet.IsValidInternetDomainName (id))
			{
				throw new ArgumentOutOfRangeException (nameof (id), FormattableString.Invariant ($"Invalid value for type 'atom': \"{id}\"."));
			}

			Contract.EndContractBlock ();

			_id = id;
			_phrase = phrase;
		}

		/// <summary>
		/// Подготавливает поле заголовка для вывода в двоичное представление.
		/// </summary>
		/// <param name="oneLineBuffer">Буфер для временного сохранения одной строки (максимально MaxLineLengthRequired байт).</param>
		protected override void PrepareToEncode (byte[] oneLineBuffer)
		{
			_pos = 0;
			_prevSequenceIsWordEncoded = false;
			_finished = false;
			if (_phrase != null)
			{
				_phraseBytes = oneLineBuffer;
				_phraseBytesSize = Encoding.UTF8.GetBytes (_phrase, 0, _phrase.Length, _phraseBytes, 0);
			}
		}

		/// <summary>
		/// Создаёт в указанном буфере очередную часть тела поля заголовка в двоичном представлении.
		/// Возвращает 0 если частей больше нет.
		/// Тело разбивается на части так, чтобы они были пригодны для фолдинга.
		/// </summary>
		/// <param name="buf">Буфер, куда будет записана чать.</param>
		/// <param name="isLast">Получает признак того, что полученная часть является последней.</param>
		/// <returns>Количество байтов, записанный в буфер.</returns>
		protected override int EncodeNextPart (Span<byte> buf, out bool isLast)
		{
			if (_finished)
			{
				isLast = true;
				return 0;
			}

			if (_pos < _phraseBytesSize)
			{
				// текст рабивается на последовательности слов, которые удобно представить в одном виде (напрямую или в виде encoded-word)
				var size = HeaderFieldBodyEncoder.EncodeNextElement (
					_phraseBytes.AsSpan (0, _phraseBytesSize),
					buf,
					TextSemantics.Phrase,
					ref _pos,
					ref _prevSequenceIsWordEncoded);
				isLast = false;
				return size;
			}

			var outPos = 0;
			buf[outPos++] = (byte)'<';
			AsciiCharSet.GetBytes (_id.AsSpan (), buf.Slice (outPos));
			outPos += _id.Length;
			buf[outPos++] = (byte)'>';
			_finished = true;
			isLast = true;
			return outPos;
		}
	}
}
