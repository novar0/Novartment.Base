using System;
using System.Collections.Generic;
using Novartment.Base.Collections.Linq;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Параметр сообщения, определяющий доставку уведомления об изменении его дислокации у получателя.
	/// Определено в RFC 3798 часть 2.2.
	/// </summary>
	public sealed class DispositionNotificationParameter
	{
		// RFC 3798 part 2.2:
		// parameter = attribute "=" importance "," value *("," value)
		// importance = "required" / "optional"
		// (value are the same as Content-Type parameter's value)
		private static readonly IReadOnlyList<string> _emptyValues = ReadOnlyList.Empty<string> ();
		private readonly IReadOnlyList<string> _values;

		/// <summary>
		/// Инициализирует новый экземпляр класса DispositionNotificationParameter
		/// с указанным именем, важностью и значением.
		/// </summary>
		/// <param name="name">Имя параметра.</param>
		/// <param name="importance">Важность параметра.</param>
		/// <param name="value">Значение параметра.</param>
		public DispositionNotificationParameter (string name, DispositionNotificationParameterImportance importance, string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException (nameof (value));
			}

			var isValueValid = AsciiCharSet.IsAllOfClass (value, AsciiCharClasses.Token);
			if (!isValueValid)
			{
				throw new ArgumentOutOfRangeException (nameof (value));
			}

			this.Name = name ?? throw new ArgumentNullException (nameof (name));
			this.Importance = importance;
			_values = ReadOnlyList.Repeat (value, 1);
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса DispositionNotificationParameter
		/// с указанным именем, важностью и коллекцией значений.
		/// </summary>
		/// <param name="name">Имя параметра.</param>
		/// <param name="importance">Важность параметра.</param>
		/// <param name="values">Список значений параметра.</param>
		public DispositionNotificationParameter (string name, DispositionNotificationParameterImportance importance, IReadOnlyList<string> values)
		{
			this.Name = name ?? throw new ArgumentNullException (nameof (name));
			_values = values ?? throw new ArgumentNullException (nameof (values));
			this.Importance = importance;
		}

		/// <summary>
		/// Важность параметра.
		/// </summary>
		public DispositionNotificationParameterImportance Importance { get; }

		/// <summary>
		/// Имя параметра.
		/// </summary>
		public string Name { get; }

		/// <summary>
		/// Коллекция значений параметра.
		/// </summary>
		public IReadOnlyList<string> Values => _values ?? _emptyValues;

		/// <summary>
		/// Создаёт новый параметр у которого важность изменена на указанное значение.
		/// </summary>
		/// <param name="importance">Важность параметра.</param>
		/// <returns>Новый параметр у которого важность изменена на указанное значение.</returns>
		public DispositionNotificationParameter ChangeImportance (DispositionNotificationParameterImportance importance)
		{
			return new DispositionNotificationParameter (this.Name, importance, this.Values);
		}

		/// <summary>
		/// Создаёт новый параметр у которого список значений дополнен указанным значением.
		/// </summary>
		/// <param name="value">Дополнительное значение для параметра.</param>
		/// <returns>Новый параметр у которого значений дополнен указанным значением.</returns>
		public DispositionNotificationParameter AddValue (string value)
		{
			if (value == null)
			{
				throw new ArgumentNullException (nameof (value));
			}

			if (_values == null || _values.Count < 1)
			{
				return new DispositionNotificationParameter (this.Name, this.Importance, value);
			}

			var appendedCollection = _values.Concat (ReadOnlyList.Repeat (value, 1));
			return new DispositionNotificationParameter (this.Name, this.Importance, appendedCollection);
		}

		/// <summary>
		/// Получает строковое представление объекта.
		/// </summary>
		/// <returns>Строковое представление объекта.</returns>
		public override string ToString ()
		{
			var values = string.Join (",", _values);
			return FormattableString.Invariant ($"{this.Name}={this.Importance.GetName ()},{values}");
		}

		/// <summary>
		/// Получает строковое представление объекта.
		/// </summary>
		/// <param name="buf">Буфер, куда будет записано строковое представление объекта.</param>
		/// <returns>Количество знаков, записанных в буфер.</returns>
		public int ToUtf8String (Span<byte> buf)
		{
			AsciiCharSet.GetBytes (this.Name.AsSpan (), buf);
			var outPos = this.Name.Length;
			buf[outPos++] = (byte)'=';
			var importanceStr = this.Importance.GetName ();
			AsciiCharSet.GetBytes (importanceStr.AsSpan (), buf[outPos..]);
			outPos += importanceStr.Length;
			buf[outPos++] = (byte)',';

			for (var idx = 0; idx < _values.Count; idx++)
			{
				var value = _values[idx];
				AsciiCharSet.GetBytes (value.AsSpan (), buf[outPos..]);
				outPos += value.Length;

				if (idx != (_values.Count - 1))
				{
					buf[outPos++] = (byte)',';
				}
			}

			return outPos;
		}
	}
}
