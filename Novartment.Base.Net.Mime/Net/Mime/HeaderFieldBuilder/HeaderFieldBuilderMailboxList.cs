using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace Novartment.Base.Net.Mime
{
	public class HeaderFieldBuilderMailboxList : HeaderFieldBuilder
	{
		private readonly IReadOnlyList<Mailbox> _mailboxes;
		private int _idx = 0;
		private byte[] _mailboxName;
		private AddrSpec _mailboxAddr = null;
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

		protected override int GetNextPart (Span<byte> buf, out bool isLast)
		{
			if (_idx >= _mailboxes.Count)
			{
				isLast = true;
				return 0;
			}

			if (_mailboxAddr == null)
			{
				var mailbox = _mailboxes[_idx];
				_mailboxName = string.IsNullOrEmpty (mailbox.Name) ?
					null :
					_mailboxName = Encoding.UTF8.GetBytes (mailbox.Name);

				_mailboxAddr = mailbox.Address;
				_pos = 0;
				_prevSequenceIsWordEncoded = false;
			}

			var size = 0;
			isLast = false;
			if ((_mailboxName != null) && (_pos < _mailboxName.Length))
			{
				// текст рабивается на последовательности слов, которые удобно представить в одном виде (напрямую или в виде encoded-word)
				size = HeaderFieldBodyEncoder.EncodeNextElement (_mailboxName, buf, TextSemantics.Phrase, ref _pos, ref _prevSequenceIsWordEncoded);
			}

			if (size < 1)
			{
				size = _mailboxAddr.ToAngleUtf8String (buf);
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
