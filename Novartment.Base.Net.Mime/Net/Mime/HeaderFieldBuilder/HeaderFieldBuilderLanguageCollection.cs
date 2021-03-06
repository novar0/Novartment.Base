using System;
using System.Collections.Generic;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Построитель поля заголовка из указанной коллекции значений-идентификаторов языка.
	/// </summary>
	public class HeaderFieldBuilderLanguageCollection : HeaderFieldBuilder
	{
		private readonly IReadOnlyList<string> _languages;
		private int _idx = 0;

		/// <summary>
		/// Инициализирует новый экземпляр класса HeaderFieldBuilderLanguageCollection из указанной коллекции значений-идентификаторов языка.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="languages">Коллекция значений-идентификаторов языка.</param>
		public HeaderFieldBuilderLanguageCollection (HeaderFieldName name, IReadOnlyList<string> languages)
			: base (name)
		{
			_languages = languages ?? throw new ArgumentNullException (nameof (languages));
		}

		/// <summary>
		/// Подготавливает поле заголовка для вывода в двоичное представление.
		/// </summary>
		/// <param name="oneLineBuffer">Буфер для временного сохранения одной строки (максимально MaxLineLengthRequired байт).</param>
		protected override void PrepareToEncode (byte[] oneLineBuffer)
		{
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
			if (_idx >= _languages.Count)
			{
				isLast = true;
				return 0;
			}

			var lang = _languages[_idx];
			AsciiCharSet.GetBytes (lang.AsSpan (), buf);
			var outPos = lang.Length;
			isLast = _idx == (_languages.Count - 1);
			if (!isLast)
			{
				buf[outPos++] = (byte)',';
			}

			_idx++;
			return outPos;
		}
	}
}
