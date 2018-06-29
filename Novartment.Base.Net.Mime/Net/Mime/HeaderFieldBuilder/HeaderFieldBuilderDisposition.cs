using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Mime
{
	public class HeaderFieldBuilderDisposition : HeaderFieldBuilder
	{
		private readonly string _actionMode;
		private readonly string _sendingMode;
		private readonly string _type;
		private readonly IReadOnlyList<string> _modifiers;
		private bool _transitionedToType = false;
		private bool _finished = false;

		/// <summary>
		/// Создает поле заголовка из трех атомов плюс опциональной коллекции атомов
		/// в формате 'actionMode/sendingMode; type/4_1,4_2,4_3,4_4 ...'.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="actionMode">Обязательное значение 1.</param>
		/// <param name="sendingMode">Обязательное значение 2.</param>
		/// <param name="type">Обязательное значение 3.</param>
		/// <param name="modifiers">Необязательная коллекция значений.</param>
		/// <returns>Поле заголовка.</returns>
		public HeaderFieldBuilderDisposition (HeaderFieldName name, string actionMode, string sendingMode, string type, IReadOnlyList<string> modifiers = null)
			: base (name)
		{
			if (actionMode == null)
			{
				throw new ArgumentNullException (nameof (actionMode));
			}

			if (sendingMode == null)
			{
				throw new ArgumentNullException (nameof (sendingMode));
			}

			if (type == null)
			{
				throw new ArgumentNullException (nameof (type));
			}

			var isAtom = AsciiCharSet.IsAllOfClass (actionMode, AsciiCharClasses.Atom);
			if (!isAtom)
			{
				throw new ArgumentOutOfRangeException (nameof (actionMode));
			}

			isAtom = AsciiCharSet.IsAllOfClass (sendingMode, AsciiCharClasses.Atom);
			if (!isAtom)
			{
				throw new ArgumentOutOfRangeException (nameof (sendingMode));
			}

			isAtom = AsciiCharSet.IsAllOfClass (type, AsciiCharClasses.Atom);
			if (!isAtom)
			{
				throw new ArgumentOutOfRangeException (nameof (type));
			}

			Contract.EndContractBlock ();

			_actionMode = actionMode;
			_sendingMode = sendingMode;
			_type = type;
			_modifiers = modifiers;
		}

		/// <summary>
		/// Подготавливает поле заголовка для вывода в двоичное представление.
		/// </summary>
		/// <param name="oneLineBuffer">Буфер для временного сохранения одной строки (максимально MaxLineLengthRequired байт).</param>
		protected override void PrepareToEncode (byte[] oneLineBuffer)
		{
			_transitionedToType = false;
			_finished = false;
		}

		/// <summary>
		/// Создаёт в указанном буфере очередную часть тела поля заголовка в двоичном представлении.
		/// Возвращает 0 если частей больше нет.
		/// Тело разбивается на части так, чтобы они были пригодны для фолдинга.
		/// </summary>
		/// <param name="buf">Буфер, куда будет записана чать.</param>
		/// <param name="isLast">Получает признак того, что полученная часть является последней.</param>
		/// <returns>Количество байтов, записанный в буфер.</returns>
		protected override int EncodeNextPart (Span<byte> buf, out bool isLast)
		{
			if (_finished)
			{
				isLast = true;
				return 0;
			}

			if (!_transitionedToType)
			{
				AsciiCharSet.GetBytes (_actionMode.AsSpan (), buf);
				var outPos2 = _actionMode.Length;
				buf[outPos2++] = (byte)'/';
				AsciiCharSet.GetBytes (_sendingMode.AsSpan (), buf.Slice (outPos2));
				outPos2 += _sendingMode.Length;
				buf[outPos2++] = (byte)';';
				_transitionedToType = true;
				isLast = false;
				return outPos2;
			}

			AsciiCharSet.GetBytes (_type.AsSpan (), buf);
			var outPos = _type.Length;
			if ((_modifiers != null) && (_modifiers.Count > 0))
			{
				buf[outPos++] = (byte)'/';
				for (var idx = 0; idx < _modifiers.Count; idx++)
				{
					var modifier = _modifiers[idx];
					var isAtom = AsciiCharSet.IsAllOfClass (modifier, AsciiCharClasses.Atom);
					if (!isAtom)
					{
						throw new FormatException (FormattableString.Invariant ($"Invalid value for type 'atom': \"{modifier}\"."));
					}

					AsciiCharSet.GetBytes (modifier.AsSpan (), buf.Slice (outPos));
					outPos += modifier.Length;
					if (idx != (_modifiers.Count - 1))
					{
						buf[outPos++] = (byte)',';
					}
				}
			}

			_finished = true;
			isLast = true;
			return outPos;
		}
	}
}
