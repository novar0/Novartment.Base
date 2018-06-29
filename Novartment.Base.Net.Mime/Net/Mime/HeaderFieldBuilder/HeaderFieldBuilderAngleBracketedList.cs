using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Mime
{
	public class HeaderFieldBuilderAngleBracketedList : HeaderFieldBuilder
	{
		private readonly IReadOnlyList<string> _urls;
		private int _idx = 0;

		/// <summary>
		/// Создает поле заголовка из коллекции url-значений.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="urls">Коллекция url-значений.</param>
		/// <returns>Поле заголовка.</returns>
		public HeaderFieldBuilderAngleBracketedList (HeaderFieldName name, IReadOnlyList<string> urls)
			: base (name)
		{
			if (urls == null)
			{
				throw new ArgumentNullException (nameof (urls));
			}

			Contract.EndContractBlock ();

			_urls = urls;
		}

		/// <summary>
		/// Подготавливает поле заголовка для вывода в двоичное представление.
		/// </summary>
		/// <param name="oneLineBuffer">Буфер для временного сохранения одной строки (максимально MaxLineLengthRequired байт).</param>
		protected override void PrepareToEncode (byte[] oneLineBuffer)
		{
			_idx = 0;
			// TODO: добавить валидацию каждого значения в urls
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
			if (_idx >= _urls.Count)
			{
				isLast = true;
				return 0;
			}

			var url = _urls[_idx];
			var outPos = 0;
			buf[outPos++] = (byte)'<';
			AsciiCharSet.GetBytes (url.AsSpan (), buf.Slice (outPos));
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
