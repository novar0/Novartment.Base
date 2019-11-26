using System;

namespace Novartment.Base.IO
{
	/// <summary>
	/// Параметры, описывающие текущее состояние потока: позицию, размер и дополнительный пользовательский объект-состояние.
	/// </summary>
	public readonly struct FileStreamStatus :
		IEquatable<FileStreamStatus>
	{
		/// <summary>
		/// Инициализирует новый экземпляр FileStreamStatus содержащий указанные позицию, размер и объект-состояние.
		/// </summary>
		/// <param name="position">Позиция в потоке.</param>
		/// <param name="length">Длина потока в байтах.</param>
		/// <param name="state">Пользовательский объект-состояние, связанный с потоком.</param>
		public FileStreamStatus (long position, long length, object state)
		{
			this.Position = position;
			this.Length = length;
			this.State = state;
		}

		/// <summary>Получает позицию в потоке.</summary>
		public readonly long Position { get; }

		/// <summary>Получает размер потока в байтах.</summary>
		public readonly long Length { get; }

		/// <summary>Получает пользовательский объект-состояние, связанный с потоком.</summary>
		public readonly object State { get; }

		/// <summary>
		/// Определяет равенство двух указанных объектов.
		/// </summary>
		/// <param name="first">Первый объект для сравнения.</param>
		/// <param name="second">Второй объект для сравнения.</param>
		/// <returns>True если значение параметра first равно second; в противном случае — False.</returns>
		public static bool operator ==(FileStreamStatus first, FileStreamStatus second)
		{
			return first.Equals(second);
		}

		/// <summary>
		/// Определяет неравенство двух указанных объектов.
		/// </summary>
		/// <param name="first">Первый объект для сравнения.</param>
		/// <param name="second">Второй объект для сравнения.</param>
		/// <returns>True если значение параметра first не равно second; в противном случае — False.</returns>
		public static bool operator !=(FileStreamStatus first, FileStreamStatus second)
		{
			return !first.Equals(second);
		}

		/// <summary>
		/// Деконструирует данные.
		/// </summary>
		/// <param name="position">Получает позицию в потоке.</param>
		/// <param name="length">Получает размер потока в байтах.</param>
		/// <param name="state">Получает пользовательский объект-состояние, связанный с потоком.</param>
		public readonly void Deconstruct (out long position, out long length, out object state)
		{
			position = this.Position;
			length = this.Length;
			state = this.State;
		}

		/// <summary>
		/// Вычисляет хэш-функцию объекта.
		/// </summary>
		/// <returns>Хэш-код для текущего объекта.</returns>
		public override int GetHashCode ()
		{
			return this.Position.GetHashCode () ^
				this.Length.GetHashCode () ^
				(this.State?.GetHashCode () ?? 0);
		}

		/// <summary>
		/// Определяет, равен ли заданный объект текущему объекту.
		/// </summary>
		/// <param name="obj">Объект, который требуется сравнить с текущим объектом. </param>
		/// <returns>True , если указанный объект равен текущему объекту; в противном случае — False.</returns>
		public override bool Equals (object obj)
		{
			return (obj is FileStreamStatus) && Equals ((FileStreamStatus)obj);
		}

		/// <summary>
		/// Определяет, равен ли заданный объект текущему объекту.
		/// </summary>
		/// <param name="other">Объект, который требуется сравнить с текущим объектом. </param>
		/// <returns>True , если указанный объект равен текущему объекту; в противном случае — False.</returns>
		public bool Equals (FileStreamStatus other)
		{
			return (other.Position == this.Position) &&
				(other.Length == this.Length) &&
				(other.State == this.State);
		}
	}
}
