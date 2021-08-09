using System;
using System.Collections.Generic;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Построитель поля заголовка из указанной коллекции url-значений.
	/// </summary>
	public class HeaderFieldBuilderAngleBracketedList : HeaderFieldBuilder
	{
		private readonly IReadOnlyList<string> _urls;
		private int _idx = 0;

		/// <summary>
		/// Инициализирует новый экземпляр класса HeaderFieldBuilderAngleBracketedList из указанной коллекции url-значений.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="urls">Коллекция url-значений.</param>
		public HeaderFieldBuilderAngleBracketedList (HeaderFieldName name, IReadOnlyList<string> urls)
			: base (name)
		{
			_urls = urls ?? throw new ArgumentNullException (nameof (urls));
		}

		/// <summary>
		/// Подготавливает поле заголовка для вывода в двоичное представление.
		/// </summary>
		/// <param name="oneLineBuffer">Буфер для временного сохранения одной строки (максимально MaxLineLengthRequired байт).</param>
		protected override void PrepareToEncode (byte[] oneLineBuffer)
		{
			// TODO: добавить валидацию каждого значения в urls
			_idx = 0;
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
			if (_idx >= _urls.Count)
			{
				isLast = true;
				return 0;
			}

			var url = _urls[_idx];
			var outPos = 0;
			buf[outPos++] = (byte)'<';
			AsciiCharSet.GetBytes (url.AsSpan (), buf[outPos..]);
			outPos += url.Length;
			buf[outPos++] = (byte)'>';
			isLast = _idx == (_urls.Count - 1);
			if (!isLast)
			{
				buf[outPos++] = (byte)',';
			}

			_idx++;
			return outPos;
		}
	}
}
