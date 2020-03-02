using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Text
{
	/// <summary>
	/// An individual lexical token as part of the structured string.
	/// Contains the type, position, and number of characters of an individual token.
	/// </summary>
	[DebuggerDisplay ("{Format}: {Position}...{Length}")]
	public readonly ref struct StructuredStringToken
	{
		/// <summary>
		/// Инициализирует новый экземпляр класса StructuredStringToken с указанным форматом, позицией и количеством знаков.
		/// </summary>
		/// <param name="format">Формат токена.</param>
		/// <param name="position">Позиция токена.</param>
		/// <param name="length">Количество знаков токена.</param>
		public StructuredStringToken (StructuredStringTokenFormat format, int position, int length)
		{
			if (position < 0)
			{
				throw new ArgumentOutOfRangeException (nameof (position));
			}

			if ((length < 0) || ((format is StructuredStringTokenSeparatorFormat) && (length != 1)))
			{
				throw new ArgumentOutOfRangeException (nameof (length));
			}

			Contract.EndContractBlock ();

			this.Format = format;
			this.Position = position;
			this.Length = length;
		}

		/// <summary>
		/// Получает формат токена. Если null-ссылка, то токен не действителен.
		/// </summary>
		public readonly StructuredStringTokenFormat Format { get; }

		/// <summary>
		/// Получает начальную позицию токена.
		/// </summary>
		public readonly int Position { get; }

		/// <summary>
		/// Получает количество знаков токена.
		/// </summary>
		public readonly int Length { get; }
	}
}
