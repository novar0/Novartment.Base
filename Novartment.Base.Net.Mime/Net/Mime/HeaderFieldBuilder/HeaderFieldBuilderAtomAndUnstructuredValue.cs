using System;
using System.Diagnostics.Contracts;
using System.Text;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Построитель поля заголовка из указанных типа и 'unstructured'-значения.
	/// </summary>
	public class HeaderFieldBuilderAtomAndUnstructuredValue : HeaderFieldBuilder
	{
		private readonly string _type;
		private readonly string _value;
		private byte[] _valueBytes;
		private int _valueBytesSize;
		private int _pos = -1;
		private bool _prevSequenceIsWordEncoded = false;

		/// <summary>
		/// Инициализирует новый экземпляр класса HeaderFieldBuilderAtomAndUnstructuredValue из указанных типа и 'unstructured'-значения.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="type">Тип (значение типа 'atom').</param>
		/// <param name="value">'unstructured' значение.</param>
		public HeaderFieldBuilderAtomAndUnstructuredValue (HeaderFieldName name, string type, string value)
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

		/// <summary>
		/// Подготавливает поле заголовка для вывода в двоичное представление.
		/// </summary>
		/// <param name="oneLineBuffer">Буфер для временного сохранения одной строки (максимально MaxLineLengthRequired байт).</param>
		protected override void PrepareToEncode (byte[] oneLineBuffer)
		{
			_pos = -1;
			_prevSequenceIsWordEncoded = false;
			_valueBytes = oneLineBuffer;
			_valueBytesSize = Encoding.UTF8.GetBytes (_value, 0, _value.Length, _valueBytes, 0);
		}

		/// <summary>
		/// Создаёт в указанном буфере очередную часть тела поля заголовка в двоичном представлении.
		/// Возвращает 0 если частей больше нет.
		/// Тело разбивается на части так, чтобы они были пригодны для фолдинга.
		/// </summary>
		/// <param name="buf">Буфер, куда будет записана чать.</param>
		/// <param name="isLast">Получает признак того, что полученная часть является последней.</param>
		/// <returns>Количество байтов, записанных в буфер.</returns>
		protected override int EncodeNextPart (Span<byte> buf, out bool isLast)
		{
			if (_pos < 0)
			{
				AsciiCharSet.GetBytes (_type.AsSpan (), buf);
				_pos = 0;
				isLast = false;
				buf[_type.Length] = (byte)';';
				return _type.Length + 1;
			}

			if (_pos >= _valueBytesSize)
			{
				isLast = true;
				return 0;
			}

			// текст рабивается на последовательности слов, которые удобно представить в одном виде (напрямую или в виде encoded-word)
			var size = HeaderFieldBodyEncoder.EncodeNextElement (
				_valueBytes.AsSpan (0, _valueBytesSize),
				buf,
				TextSemantics.Unstructured,
				ref _pos,
				ref _prevSequenceIsWordEncoded);
			isLast = _pos >= _valueBytesSize;
			return size;
		}
	}
}
