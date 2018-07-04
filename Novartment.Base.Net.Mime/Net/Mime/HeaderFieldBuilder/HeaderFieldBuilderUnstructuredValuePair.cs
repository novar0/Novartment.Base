using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Построитель поля заголовка из двух указанных неструктурированных значений.
	/// </summary>
	public class HeaderFieldBuilderUnstructuredValuePair : HeaderFieldBuilder
	{
		private readonly string _value1;
		private readonly string _value2 = null;
		private byte[] _valueBytes;
		private int _valueBytesSize1;
		private int _valueBytesSize2 = 0;
		private int _pos1 = 0;
		private int _pos2 = -1;
		private bool _prevSequenceIsWordEncoded1 = false;
		private bool _prevSequenceIsWordEncoded2 = false;

		/// <summary>
		/// Инициализирует новый экземпляр класса HeaderFieldBuilderUnstructuredValuePair из двух указанных неструктурированных значений.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="value1">Обязательное неструктурированное значение 1.</param>
		/// <param name="value2">Необязательное неструктурированное значение 2.</param>
		public HeaderFieldBuilderUnstructuredValuePair (HeaderFieldName name, string value1, string value2)
			: base (name)
		{
			if (value1 == null)
			{
				throw new ArgumentNullException (nameof (value1));
			}

			Contract.EndContractBlock ();

			_value1 = value1;
			_value2 = value2;
		}

		/// <summary>
		/// Подготавливает поле заголовка для вывода в двоичное представление.
		/// </summary>
		/// <param name="oneLineBuffer">Буфер для временного сохранения одной строки (максимально MaxLineLengthRequired байт).</param>
		protected override void PrepareToEncode (byte[] oneLineBuffer)
		{
			_pos1 = 0;
			_pos2 = -1;
			_prevSequenceIsWordEncoded1 = false;
			_prevSequenceIsWordEncoded2 = false;
			_valueBytes = oneLineBuffer;
			_valueBytesSize1 = Encoding.UTF8.GetBytes (_value1, 0, _value1.Length, _valueBytes, 0);
			if (_value2 != null)
			{
				_valueBytesSize2 = Encoding.UTF8.GetBytes (_value2, 0, _value2.Length, _valueBytes, _valueBytesSize1);
			}
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
			if (_pos2 < 0)
			{
				if (_pos1 < _valueBytesSize1)
				{
					// текст рабивается на последовательности слов, которые удобно представить в одном виде (напрямую или в виде encoded-word)
					var outPos = HeaderFieldBodyEncoder.EncodeNextElement (
						_valueBytes.AsSpan (0, _valueBytesSize1),
						buf,
						TextSemantics.Unstructured,
						ref _pos1,
						ref _prevSequenceIsWordEncoded1);
					if ((_pos1 >= _valueBytesSize1) && (_value2 != null))
					{
						buf[outPos++] = (byte)';';
					}

					isLast = (_pos1 >= _valueBytesSize1) && (_value2 == null);
					return outPos;
				}

				_pos2 = 0;
			}

			if (_value2 == null)
			{
				isLast = true;
				return 0;
			}

			if (_pos2 >= _valueBytesSize2)
			{
				isLast = true;
				return 0;
			}

			// текст рабивается на последовательности слов, которые удобно представить в одном виде (напрямую или в виде encoded-word)
			var size = HeaderFieldBodyEncoder.EncodeNextElement (
				_valueBytes.AsSpan (_valueBytesSize1, _valueBytesSize2),
				buf,
				TextSemantics.Unstructured,
				ref _pos2,
				ref _prevSequenceIsWordEncoded2);
			isLast = _pos2 >= _valueBytesSize2;
			return size;
		}
	}
}
