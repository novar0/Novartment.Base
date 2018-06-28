using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace Novartment.Base.Net.Mime
{
	public class HeaderFieldBuilderUnstructuredPair : HeaderFieldBuilder
	{
		private readonly string _value1;
		private readonly string _value2 = null;
		private byte[] _bytes1 = null;
		private byte[] _bytes2 = null;
		private int _pos1 = 0;
		private int _pos2 = -1;
		private bool _prevSequenceIsWordEncoded1 = false;
		private bool _prevSequenceIsWordEncoded2 = false;

		/// <summary>
		/// Создает поле заголовка из двух 'unstructured' значений.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="value1">Обязательное 'unstructured' значение 1.</param>
		/// <param name="value2">Необязательное 'unstructured' значение 2.</param>
		/// <returns>Поле заголовка.</returns>
		public HeaderFieldBuilderUnstructuredPair (HeaderFieldName name, string value1, string value2)
			: base (name)
		{
			if (value1 == null)
			{
				throw new ArgumentNullException (nameof (value1));
			}

			Contract.EndContractBlock ();

			_value1 = value1;
			_value2 = value2;
		}

		protected override int GetNextPart (Span<byte> buf, out bool isLast)
		{
			if (_pos2 < 0)
			{
				if (_bytes1 == null)
				{
					_bytes1 = Encoding.UTF8.GetBytes (_value1);
				}

				if (_pos1 < _value1.Length)
				{
					// текст рабивается на последовательности слов, которые удобно представить в одном виде (напрямую или в виде encoded-word)
					var outPos = HeaderFieldBodyEncoder.EncodeNextElement (_bytes1, buf, TextSemantics.Unstructured, ref _pos1, ref _prevSequenceIsWordEncoded1);
					if ((_pos1 >= _bytes1.Length) && (_value2 != null))
					{
						buf[outPos++] = (byte)';';
					}

					isLast = (_pos1 >= _bytes1.Length) && (_value2 == null);
					return outPos;
				}

				_pos2 = 0;
			}

			if (_value2 == null)
			{
				isLast = true;
				return 0;
			}

			if (_bytes2 == null)
			{
				_bytes2 = Encoding.UTF8.GetBytes (_value2);
			}

			if (_pos2 >= _bytes2.Length)
			{
				isLast = true;
				return 0;
			}

			// текст рабивается на последовательности слов, которые удобно представить в одном виде (напрямую или в виде encoded-word)
			var size = HeaderFieldBodyEncoder.EncodeNextElement (_bytes2, buf, TextSemantics.Unstructured, ref _pos2, ref _prevSequenceIsWordEncoded2);
			isLast = _pos2 >= _bytes2.Length;
			return size;
		}
	}
}
