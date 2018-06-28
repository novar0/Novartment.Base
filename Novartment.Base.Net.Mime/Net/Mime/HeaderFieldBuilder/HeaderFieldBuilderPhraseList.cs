using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace Novartment.Base.Net.Mime
{
	public class HeaderFieldBuilderPhraseList : HeaderFieldBuilder
	{
		private readonly IReadOnlyList<string> _values;
		private int _idx = -1;
		private int _pos;
		private bool _prevSequenceIsWordEncoded;
		private byte[] _value = null;

		/// <summary>
		/// Создает поле заголовка из коллекции 'phrase'.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="values">Коллекция 'phrase'.</param>
		/// <returns>Поле заголовка.</returns>
		public HeaderFieldBuilderPhraseList (HeaderFieldName name, IReadOnlyList<string> values)
			: base (name)
		{
			if (values == null)
			{
				throw new ArgumentNullException (nameof (values));
			}

			Contract.EndContractBlock ();

			_values = values;
		}

		protected override int GetNextPart (Span<byte> buf, out bool isLast)
		{
			if (_value == null)
			{
				_idx++;
				if (_idx >= _values.Count)
				{
					isLast = true;
					return 0;
				}

				_value = Encoding.UTF8.GetBytes (_values[_idx]);
				_pos = 0;
				_prevSequenceIsWordEncoded = false;
			}

			if (_pos >= _value.Length)
			{
			}

			// текст рабивается на последовательности слов, которые удобно представить в одном виде (напрямую или в виде encoded-word)
			var size = HeaderFieldBodyEncoder.EncodeNextElement (_value.AsSpan (), buf, TextSemantics.Phrase, ref _pos, ref _prevSequenceIsWordEncoded);
			var valueOver = _pos >= _value.Length;
			if (valueOver)
			{
				_value = null;
				isLast = _idx == (_values.Count - 1);
				if (!isLast)
				{
					buf[size++] = (byte)',';
				}
			}
			else
			{
				isLast = false;
			}

			return size;
		}
	}
}
