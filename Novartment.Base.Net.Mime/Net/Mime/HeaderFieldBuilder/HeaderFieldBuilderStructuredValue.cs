using System;
using System.Text;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Построитель поля заголовка из указанного структурированного (состоящего из токенов) значения.
	/// </summary>
	public class HeaderFieldBuilderStructuredValue : HeaderFieldBuilder
	{
		private readonly string _text;
		private int _pos;
		private bool _prevSequenceIsWordEncoded;
		private byte[] _textBytes;
		private int _textBytesSize;

		/// <summary>
		/// Инициализирует новый экземпляр класса HeaderFieldBuilderStructuredValue из указанного структурированного (состоящего из токенов) значения.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="text">Структурированное (состоящеее из токенов) значение.</param>
		public HeaderFieldBuilderStructuredValue (HeaderFieldName name, string text)
			: base (name)
		{
			_text = text ?? throw new ArgumentNullException (nameof (text));
		}

		/// <summary>
		/// Подготавливает поле заголовка для вывода в двоичное представление.
		/// </summary>
		/// <param name="oneLineBuffer">Буфер для временного сохранения одной строки (максимально MaxLineLengthRequired байт).</param>
		protected override void PrepareToEncode (byte[] oneLineBuffer)
		{
			_pos = 0;
			_prevSequenceIsWordEncoded = false;
			_textBytes = oneLineBuffer;
			_textBytesSize = Encoding.UTF8.GetBytes (_text, 0, _text.Length, _textBytes, 0);
		}

		/// <summary>
		/// Создаёт в указанном буфере очередную часть тела поля заголовка в двоичном представлении.
		/// Возвращает 0 если частей больше нет.
		/// Тело разбивается на части так, чтобы они были пригодны для фолдинга.
		/// </summary>
		/// <param name="buf">Буфер, куда будет записана чать.</param>
		/// <param name="isLast">Получает признак того, что полученная часть является последней.</param>
		/// <returns>Количество байтов, записанных в буфер.</returns>
		protected override int EncodeNextPart (Span<byte> buf, out bool isLast)
		{
			if (_pos >= _textBytesSize)
			{
				isLast = true;
				return 0;
			}

			// текст рабивается на последовательности слов, которые удобно представить в одном виде (напрямую или в виде encoded-word)
			var size = HeaderFieldBodyEncoder.EncodeNextElement (
				_textBytes.AsSpan (0, _textBytesSize),
				buf,
				TextSemantics.Phrase,
				ref _pos,
				ref _prevSequenceIsWordEncoded);
			isLast = _pos >= _textBytesSize;
			return size;
		}
	}
}
