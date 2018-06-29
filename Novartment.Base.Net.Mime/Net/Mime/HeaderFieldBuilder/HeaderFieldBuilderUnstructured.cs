﻿using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace Novartment.Base.Net.Mime
{
	public class HeaderFieldBuilderUnstructured : HeaderFieldBuilder
	{
		private readonly string _text;
		private byte[] _textBytes;
		private int _textBytesSize;
		private int _pos = 0;
		private bool _prevSequenceIsWordEncoded = false;

		/// <summary>
		/// Создает поле заголовка из указанного значения типа 'unstructured'.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="text">Значение, которое надо представить в ограничениях типа 'unstructured'.</param>
		/// <returns>Поле заголовка.</returns>
		public HeaderFieldBuilderUnstructured (HeaderFieldName name, string text)
			: base (name)
		{
			if (text == null)
			{
				throw new ArgumentNullException (nameof (text));
			}

			Contract.EndContractBlock ();

			_text = text;
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
		/// <returns>Количество байтов, записанный в буфер.</returns>
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
				TextSemantics.Unstructured,
				ref _pos,
				ref _prevSequenceIsWordEncoded);
			isLast = _pos >= _textBytesSize;
			return size;
		}
	}
}