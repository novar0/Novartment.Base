using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace Novartment.Base.Net.Mime
{
	public class HeaderFieldBuilderMailbox : HeaderFieldBuilder
	{
		private readonly Mailbox _mailbox;
		private byte[] _mailboxNameBytes = null;
		private int _mailboxNameBytesSize = 0;
		private int _pos = 0;
		private bool _prevSequenceIsWordEncoded = false;
		private bool _finished = false;

		/// <summary>
		/// Создает поле заголовка из Mailbox.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="mailbox">Mailbox.</param>
		/// <returns>Поле заголовка.</returns>
		public HeaderFieldBuilderMailbox (HeaderFieldName name, Mailbox mailbox)
			: base (name)
		{
			if (mailbox == null)
			{
				throw new ArgumentNullException (nameof (mailbox));
			}

			Contract.EndContractBlock ();

			_mailbox = mailbox;
		}

		/// <summary>
		/// Подготавливает поле заголовка для вывода в двоичное представление.
		/// </summary>
		/// <param name="oneLineBuffer">Буфер для временного сохранения одной строки (максимально MaxLineLengthRequired байт).</param>
		protected override void PrepareToEncode (byte[] oneLineBuffer)
		{
			_pos = 0;
			_prevSequenceIsWordEncoded = false;
			_finished = false;
			if (!string.IsNullOrEmpty (_mailbox.Name))
			{
				_mailboxNameBytes = oneLineBuffer;
				_mailboxNameBytesSize = Encoding.UTF8.GetBytes (_mailbox.Name, 0, _mailbox.Name.Length, _mailboxNameBytes, 0);
			}
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

			var size = 0;
			if (_pos < _mailboxNameBytesSize)
			{
				size = HeaderFieldBodyEncoder.EncodeNextElement (
					_mailboxNameBytes.AsSpan (0, _mailboxNameBytesSize),
					buf,
					TextSemantics.Phrase,
					ref _pos,
					ref _prevSequenceIsWordEncoded);
			}

			isLast = size < 1;
			if (isLast)
			{
				buf[size++] = (byte)'<';
				size += _mailbox.Address.ToUtf8String (buf.Slice (size));
				buf[size++] = (byte)'>';
				_finished = true;
			}

			return size;
		}
	}
}
