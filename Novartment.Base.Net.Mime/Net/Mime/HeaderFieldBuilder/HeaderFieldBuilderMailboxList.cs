using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace Novartment.Base.Net.Mime
{
	public class HeaderFieldBuilderMailboxList : HeaderFieldBuilder
	{
		private readonly IReadOnlyList<Mailbox> _mailboxes;
		private int _idx;
		private byte[] _mailboxNameBytes;
		private int _mailboxNameBytesSize;
		private AddrSpec _mailboxAddr;
		private int _pos;
		private bool _prevSequenceIsWordEncoded;

		/// <summary>
		/// Создает поле заголовка из коллекции Mailbox.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="mailboxes">Коллекция Mailbox.</param>
		/// <returns>Поле заголовка.</returns>
		public HeaderFieldBuilderMailboxList (HeaderFieldName name, IReadOnlyList<Mailbox> mailboxes)
			: base (name)
		{
			if (mailboxes == null)
			{
				throw new ArgumentNullException (nameof (mailboxes));
			}

			Contract.EndContractBlock ();

			_mailboxes = mailboxes;
		}

		/// <summary>
		/// Подготавливает поле заголовка для вывода в двоичное представление.
		/// </summary>
		/// <param name="oneLineBuffer">Буфер для временного сохранения одной строки (максимально MaxLineLengthRequired байт).</param>
		protected override void PrepareToEncode (byte[] oneLineBuffer)
		{
			_idx = 0;
			_mailboxAddr = null;
			_pos = 0;
			_prevSequenceIsWordEncoded = false;
			_mailboxNameBytes = oneLineBuffer;
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
			if (_idx >= _mailboxes.Count)
			{
				isLast = true;
				return 0;
			}

			if (_mailboxAddr == null)
			{
				var mailbox = _mailboxes[_idx];
				_mailboxNameBytesSize = 0;
				if (!string.IsNullOrEmpty (mailbox.Name))
				{
					_mailboxNameBytesSize = Encoding.UTF8.GetBytes (mailbox.Name, 0, mailbox.Name.Length, _mailboxNameBytes, 0);
				}

				_mailboxAddr = mailbox.Address;
				_pos = 0;
				_prevSequenceIsWordEncoded = false;
			}

			var size = 0;
			isLast = false;
			if (_pos < _mailboxNameBytesSize)
			{
				// текст рабивается на последовательности слов, которые удобно представить в одном виде (напрямую или в виде encoded-word)
				size = HeaderFieldBodyEncoder.EncodeNextElement (
					_mailboxNameBytes.AsSpan (0, _mailboxNameBytesSize),
					buf,
					TextSemantics.Phrase,
					ref _pos,
					ref _prevSequenceIsWordEncoded);
			}

			if (size < 1)
			{
				buf[size++] = (byte)'<';
				size += _mailboxAddr.ToUtf8String (buf.Slice (size));
				buf[size++] = (byte)'>';
				isLast = _idx == (_mailboxes.Count - 1);
				if (!isLast)
				{
					buf[size++] = (byte)',';
				}

				_mailboxAddr = null;
				_idx++;
			}

			return size;
		}
	}
}
