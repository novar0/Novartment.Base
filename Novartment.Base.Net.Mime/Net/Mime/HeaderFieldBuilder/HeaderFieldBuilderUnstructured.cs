using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace Novartment.Base.Net.Mime
{
	public class HeaderFieldBuilderUnstructured : HeaderFieldBuilder
	{
		private readonly byte[] _text;
		private int _pos = 0;
		private bool _prevSequenceIsWordEncoded = false;

		/// <summary>
		/// Создает поле заголовка из указанного значения типа 'unstructured'.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="text">Значение, которое надо представить в ограничениях типа 'unstructured'.</param>
		/// <returns>Поле заголовка.</returns>
		public HeaderFieldBuilderUnstructured (HeaderFieldName name, string text)
			: base (name)
		{
			if (text == null)
			{
				throw new ArgumentNullException (nameof (text));
			}

			Contract.EndContractBlock ();

			_text = Encoding.UTF8.GetBytes (text);
		}

		protected override int GetNextPart (Span<byte> buf, out bool isLast)
		{
			if (_pos >= _text.Length)
			{
				isLast = true;
				return 0;
			}

			// текст рабивается на последовательности слов, которые удобно представить в одном виде (напрямую или в виде encoded-word)
			var size = HeaderFieldBodyEncoder.EncodeNextElement (_text, buf, TextSemantics.Unstructured, ref _pos, ref _prevSequenceIsWordEncoded);
			isLast = _pos >= _text.Length;
			return size;
		}
	}
}
