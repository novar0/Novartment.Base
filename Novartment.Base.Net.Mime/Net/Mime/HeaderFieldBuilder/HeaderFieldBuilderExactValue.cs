using System;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Mime
{
	public class HeaderFieldBuilderExactValue : HeaderFieldBuilder
	{
		private readonly string _value;
		private bool _finished = false;

		/// <summary>
		/// Создает поле заголовка из указанного значения.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="value">Значение поля заголовка.</param>
		/// <returns>Поле заголовка.</returns>
		public HeaderFieldBuilderExactValue (HeaderFieldName name, string value)
			: base (name)
		{
			_value = value;
		}

		protected override int GetNextPart (Span<byte> buf, out bool isLast)
		{
			isLast = true;

			if ((_value == null) || _finished)
			{
				return 0;
			}

			AsciiCharSet.GetBytes (_value.AsSpan (), buf);
			_finished = true;
			return _value.Length;
		}
	}
}
