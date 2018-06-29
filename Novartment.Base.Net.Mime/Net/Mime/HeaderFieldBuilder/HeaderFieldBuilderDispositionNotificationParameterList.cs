using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Net.Mime
{
	public class HeaderFieldBuilderDispositionNotificationParameterList : HeaderFieldBuilder
	{
		private readonly IReadOnlyList<DispositionNotificationParameter> _parameters;
		private int _idx = 0;

		/// <summary>
		/// Создает поле заголовка из указанной коллекции DispositionNotificationParameter.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="parameters">Коллекция DispositionNotificationParameter.</param>
		/// <returns>Поле заголовка.</returns>
		public HeaderFieldBuilderDispositionNotificationParameterList (HeaderFieldName name, IReadOnlyList<DispositionNotificationParameter> parameters)
			: base (name)
		{
			if (name == HeaderFieldName.Unspecified)
			{
				throw new ArgumentOutOfRangeException (nameof (name));
			}

			if (parameters == null)
			{
				throw new ArgumentNullException (nameof (parameters));
			}

			Contract.EndContractBlock ();

			_parameters = parameters;
		}


		/// <summary>
		/// Подготавливает поле заголовка для вывода в двоичное представление.
		/// </summary>
		/// <param name="oneLineBuffer">Буфер для временного сохранения одной строки (максимально MaxLineLengthRequired байт).</param>
		protected override void PrepareToEncode (byte[] oneLineBuffer)
		{
			// TODO: добавить валидацию каждого значения в parameters
			_idx = 0;
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
			if (_idx >= _parameters.Count)
			{
				isLast = true;
				return 0;
			}

			var parameter = _parameters[_idx];
			var outPos = parameter.ToUtf8String (buf);
			isLast = _idx == (_parameters.Count - 1);
			if (!isLast)
			{
				buf[outPos++] = (byte)';';
			}

			_idx++;
			return outPos;
		}
	}
}
