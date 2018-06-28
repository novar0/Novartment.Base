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

			// TODO: добавить валидацию каждого значения в urls
			_urls = urls;
		}

		protected override int GetNextPart (Span<byte> buf, out bool isLast)
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
