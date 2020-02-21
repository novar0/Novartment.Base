using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Построитель поля заголовка из указанной коллекции интернет-идентификаторов.
	/// </summary>
	public class HeaderFieldBuilderAddrSpecCollection : HeaderFieldBuilder
	{
		private readonly IReadOnlyList<AddrSpec> _addrSpecs;
		private int _idx = 0;

		/// <summary>
		/// Инициализирует новый экземпляр класса HeaderFieldBuilderAddrSpecCollection из указанной коллекции интернет-идентификаторов.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="addrSpecs">Коллекция интернет-идентификаторов.</param>
		public HeaderFieldBuilderAddrSpecCollection (HeaderFieldName name, IReadOnlyList<AddrSpec> addrSpecs)
			: base (name)
		{
			if (addrSpecs == null)
			{
				throw new ArgumentNullException (nameof (addrSpecs));
			}

			Contract.EndContractBlock ();

			_addrSpecs = addrSpecs;
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
			if (_idx >= _addrSpecs.Count)
			{
				isLast = true;
				return 0;
			}

			var pos = 0;
			buf[pos++] = (byte)'<';
			var addrStr = _addrSpecs[_idx].ToString ();
			AsciiCharSet.GetBytes (addrStr.AsSpan (), buf.Slice (pos));
			pos += addrStr.Length;
			buf[pos++] = (byte)'>';
			isLast = _idx == (_addrSpecs.Count - 1);
			_idx++;
			return pos;
		}
	}
}
