using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace Novartment.Base.Net.Mime
{
	public class HeaderFieldBuilderMailbox : HeaderFieldBuilder
	{
		private readonly byte[] _mailboxName;
		private readonly AddrSpec _mailboxAddr;
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

			_mailboxName = string.IsNullOrEmpty (mailbox.Name) ?
				null :
				_mailboxName = Encoding.UTF8.GetBytes (mailbox.Name);

			_mailboxAddr = mailbox.Address;
		}

		protected override int GetNextPart (Span<byte> buf, out bool isLast)
		{
			if (_finished)
			{
				isLast = true;
				return 0;
			}

			var size = 0;
			if ((_mailboxName != null) && (_pos < _mailboxName.Length))
			{
				// текст рабивается на последовательности слов, которые удобно представить в одном виде (напрямую или в виде encoded-word)
				size = HeaderFieldBodyEncoder.EncodeNextElement (_mailboxName, buf, TextSemantics.Phrase, ref _pos, ref _prevSequenceIsWordEncoded);
			}

			isLast = size < 1;
			if (isLast)
			{
				size = _mailboxAddr.ToAngleUtf8String (buf);
				_finished = true;
			}

			return size;
		}
	}
}
