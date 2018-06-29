using System;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Mime
{
	public class HeaderFieldBuilderExactValue : HeaderFieldBuilder
	{
		private readonly string _value;
		private bool _finished = false;

		/// <summary>
		/// Создает поле заголовка из указанного значения.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="value">Значение поля заголовка.</param>
		/// <returns>Поле заголовка.</returns>
		public HeaderFieldBuilderExactValue (HeaderFieldName name, string value)
			: base (name)
		{
			_value = value;
		}

		/// <summary>
		/// Подготавливает поле заголовка для вывода в двоичное представление.
		/// </summary>
		/// <param name="oneLineBuffer">Буфер для временного сохранения одной строки (максимально MaxLineLengthRequired байт).</param>
		protected override void PrepareToEncode (byte[] oneLineBuffer)
		{
			_finished = false;
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
			isLast = true;

			if ((_value == null) || _finished)
			{
				return 0;
			}

			AsciiCharSet.GetBytes (_value.AsSpan (), buf);
			_finished = true;
			return _value.Length;
		}
	}
}
