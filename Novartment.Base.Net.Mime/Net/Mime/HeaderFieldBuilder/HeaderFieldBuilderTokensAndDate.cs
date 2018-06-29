using System;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Mime
{
	public class HeaderFieldBuilderTokensAndDate : HeaderFieldBuilder
	{
		private readonly string _value;
		private readonly DateTimeOffset _dateTimeOffset;
		private bool _finished = false;
		private bool _isSemicolonCreated = false;
		private int _pos = 0;
		private int _wordStart = -1;

		/// <summary>
		/// Создает поле заголовка из '*tokens' значения и даты.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="value">'*tokens' значение.</param>
		/// <param name="dateTimeOffset">Дата.</param>
		/// <returns>Поле заголовка.</returns>
		public HeaderFieldBuilderTokensAndDate (HeaderFieldName name, string value, DateTimeOffset dateTimeOffset)
			: base (name)
		{
			_value = value;
			_dateTimeOffset = dateTimeOffset;
		}

		/// <summary>
		/// Подготавливает поле заголовка для вывода в двоичное представление.
		/// </summary>
		/// <param name="oneLineBuffer">Буфер для временного сохранения одной строки (максимально MaxLineLengthRequired байт).</param>
		protected override void PrepareToEncode (byte[] oneLineBuffer)
		{
			_finished = false;
			_isSemicolonCreated = false;
			_pos = 0;
			_wordStart = -1;
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

			// An 'encoded-word' MUST NOT be used in a '*tokens' header field.

			while ((_value != null) && (_pos < _value.Length))
			{
				var currentChar = _value[_pos];
				var charClass = (currentChar < AsciiCharSet.Classes.Count) ?
					(AsciiCharClasses)AsciiCharSet.Classes[currentChar] :
					AsciiCharClasses.None;
				var isWhiteSpace = (charClass & AsciiCharClasses.WhiteSpace) != 0;
				var isCrlf = (currentChar == 0x0d) && ((_pos + 1) < _value.Length) && (_value[_pos + 1] == 0x0a);
				if (isWhiteSpace || isCrlf)
				{
					if (_wordStart >= 0)
					{
						// пробельный символ при условии что ранее был непробельный
						// важно знать последнее ли это слово, поэтому проверяем пробелы до конца строки
						var size2 = _pos - _wordStart;
						_pos += isCrlf ? 2 : 1;
						var newPos = _pos;
						while (newPos < _value.Length)
						{
							if ((_value[newPos] != (byte)'\t') && (_value[newPos] != (byte)' '))
							{
								break;
							}

							newPos++;
						}

						AsciiCharSet.GetBytes (_value.AsSpan (_wordStart, size2), buf);
						_wordStart = -1;
						if (newPos >= _value.Length)
						{
							buf[size2++] = (byte)';';
							_isSemicolonCreated = true;
						}

						isLast = false;
						return size2;
					}
				}
				else
				{
					if ((charClass & AsciiCharClasses.Visible) != 0)
					{
						if (_wordStart < 0)
						{
							_wordStart = _pos;
						}
					}
					else
					{
						// встретился символ не входящий в комбинацию WhiteSpace | Visible
						throw new FormatException (FormattableString.Invariant (
							$"Value contains invalid for 'token' character U+{currentChar:x}. Expected characters are U+0009 and U+0020...U+007E."));

					}
				}

				_pos++;
			}

			if (_wordStart >= 0)
			{
				// добавляем только куски со словами (пробельные куски в конце отбрасываем)
				var size2 = _pos - _wordStart;
				AsciiCharSet.GetBytes (_value.AsSpan (_wordStart, size2), buf);
				buf[size2++] = (byte)';';
				_isSemicolonCreated = true;
				isLast = false;
				_wordStart = -1;
				return size2;
			}

			if (!_isSemicolonCreated)
			{
				buf[0] = (byte)';';
				_isSemicolonCreated = true;
				isLast = false;
				return 1;
			}

			var size = _dateTimeOffset.ToInternetUtf8String (buf);
			_finished = true;
			isLast = true;
			return size;
		}
	}
}
