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

		protected override int GetNextPart (Span<byte> buf, out bool isLast)
		{
			// TODO: добавить валидацию каждого значения в parameters
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
