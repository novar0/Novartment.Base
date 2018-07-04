using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Построитель поля заголовка из указанной коллекции структурированных (состоящих из токенов) значений.
	/// </summary>
	public class HeaderFieldBuilderStructuredValueCollection : HeaderFieldBuilder
	{
		private readonly IReadOnlyList<string> _values;
		private int _idx = -1;
		private int _pos;
		private bool _prevSequenceIsWordEncoded;
		private byte[] _valueBytes;
		private int _valueBytesSize;

		/// <summary>
		/// Инициализирует новый экземпляр класса HeaderFieldBuilderStructuredValueCollection из указанной коллекции структурированных (состоящих из токенов) значений.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="values">Коллекция структурированных (состоящих из токенов) значений.</param>
		public HeaderFieldBuilderStructuredValueCollection (HeaderFieldName name, IReadOnlyList<string> values)
			: base (name)
		{
			if (values == null)
			{
				throw new ArgumentNullException (nameof (values));
			}

			Contract.EndContractBlock ();

			_values = values;
		}

		/// <summary>
		/// Подготавливает поле заголовка для вывода в двоичное представление.
		/// </summary>
		/// <param name="oneLineBuffer">Буфер для временного сохранения одной строки (максимально MaxLineLengthRequired байт).</param>
		protected override void PrepareToEncode (byte[] oneLineBuffer)
		{
			_idx = -1;
			_valueBytes = oneLineBuffer;
			_valueBytesSize = -1;
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
			if (_valueBytesSize < 0)
			{
				_idx++;
				if (_idx >= _values.Count)
				{
					isLast = true;
					return 0;
				}

				var value = _values[_idx];
				_valueBytesSize = Encoding.UTF8.GetBytes (value, 0, value.Length, _valueBytes, 0);
				_pos = 0;
				_prevSequenceIsWordEncoded = false;
			}

			// текст рабивается на последовательности слов, которые удобно представить в одном виде (напрямую или в виде encoded-word)
			var size = HeaderFieldBodyEncoder.EncodeNextElement (
				_valueBytes.AsSpan (0, _valueBytesSize),
				buf,
				TextSemantics.Phrase,
				ref _pos,
				ref _prevSequenceIsWordEncoded);
			var valueOver = _pos >= _valueBytesSize;
			if (valueOver)
			{
				_valueBytesSize = -1;
				isLast = _idx == (_values.Count - 1);
				if (!isLast)
				{
					buf[size++] = (byte)',';
				}
			}
			else
			{
				isLast = false;
			}

			return size;
		}
	}
}
