using System;
using System.Diagnostics.Contracts;
using System.Text;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Mime
{
	public class HeaderFieldBuilderAtomAndUnstructured : HeaderFieldBuilder
	{
		private readonly string _type;
		private readonly string _value;
		private byte[] _valueBytes;
		private int _pos = -1;
		private bool _prevSequenceIsWordEncoded = false;

		/// <summary>
		/// Создает поле заголовка из типа и 'unstructured'-значения.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="type">Тип (значение типа 'atom').</param>
		/// <param name="value">'unstructured' значение.</param>
		/// <returns>Поле заголовка.</returns>
		public HeaderFieldBuilderAtomAndUnstructured (HeaderFieldName name, string type, string value)
			: base (name)
		{
			if (type == null)
			{
				throw new ArgumentNullException (nameof (type));
			}

			var isValidAtom = AsciiCharSet.IsAllOfClass (type, AsciiCharClasses.Atom);
			if (!isValidAtom)
			{
				throw new ArgumentOutOfRangeException (nameof (type));
			}

			Contract.EndContractBlock ();

			_type = type;
			_value = value;
		}

		protected override int GetNextPart (Span<byte> buf, out bool isLast)
		{
			if (_pos < 0)
			{
				AsciiCharSet.GetBytes (_type.AsSpan (), buf);
				_pos = 0;
				isLast = false;
				buf[_type.Length] = (byte)';';
				_valueBytes = Encoding.UTF8.GetBytes (_value);
				return _type.Length + 1;
			}

			if (_pos >= _valueBytes.Length)
			{
				isLast = true;
				return 0;
			}

			// текст рабивается на последовательности слов, которые удобно представить в одном виде (напрямую или в виде encoded-word)
			var size = HeaderFieldBodyEncoder.EncodeNextElement (_valueBytes, buf, TextSemantics.Unstructured, ref _pos, ref _prevSequenceIsWordEncoded);
			isLast = _pos >= _valueBytes.Length;
			return size;
		}
	}
}
