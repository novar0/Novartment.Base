using System;

namespace Novartment.Base.Text
{
	public static class StructuredStringTokenExtensions
	{
		/// <summary>
		/// Проверяет, является ли токен указанным символом-сепаратором.
		/// </summary>
		/// <param name="token">Токен для проверки.</param>
		/// <param name="source">Исходная строка.</param>
		/// <param name="separator">Проверяемый символ-сепаратор.</param>
		/// <returns>True если токен является указанным символом-сепаратором.</returns>
		public static bool IsSeparator (this StructuredStringToken token, ReadOnlySpan<char> source, char separator)
		{
			return (token.Format is StructuredStringTokenSeparatorFormat) && (source[token.Position] == separator);
		}
	}
}
