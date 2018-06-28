using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Mime
{
	public class HeaderFieldBuilderLanguageList : HeaderFieldBuilder
	{
		private readonly IReadOnlyList<string> _languages;
		private int _idx = 0;

		/// <summary>
		/// Создает поле заголовка из указанной коллекции значений-идентификаторов языка.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="languages">Коллекция значений-идентификаторов языка.</param>
		/// <returns>Поле заголовка.</returns>
		public HeaderFieldBuilderLanguageList (HeaderFieldName name, IReadOnlyList<string> languages)
			: base (name)
		{
			if (languages == null)
			{
				throw new ArgumentNullException (nameof (languages));
			}

			Contract.EndContractBlock ();

			_languages = languages;
		}

		protected override int GetNextPart (Span<byte> buf, out bool isLast)
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
