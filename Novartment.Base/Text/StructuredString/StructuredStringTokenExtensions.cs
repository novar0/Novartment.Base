using System;
using System.Runtime.CompilerServices;

namespace Novartment.Base.Text
{
	/// <summary>
	/// Методы расширения для StructuredStringToken.
	/// </summary>
	public static class StructuredStringTokenExtensions
	{
		/// <summary>
		/// Проверяет, является ли токен указанным символом-сепаратором.
		/// </summary>
		/// <param name="token">Токен для проверки.</param>
		/// <param name="source">Исходная строка.</param>
		/// <param name="separator">Проверяемый символ-сепаратор.</param>
		/// <returns>True если токен является указанным символом-сепаратором.</returns>
		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		public static bool IsSeparator (this StructuredStringToken token, ReadOnlySpan<char> source, char separator)
		{
			return (token.Format is StructuredStringTokenSeparatorFormat) && (source[token.Position] == separator);
		}

		/// <summary>
		/// Декодирует значение лексического токена, помещая его декодированное значение в указанный буфер.
		/// </summary>
		/// <param name="token">Лексический токен.</param>
		/// <param name="source">Исходной строка, в которой найден лексический токен.</param>
		/// <param name="buffer">Буфер, куда будет записано декодировенное значение лексического токена, начиная с позиции bufferPosition.</param>
		/// <param name="bufferPosition">Позиция, начиная с которой в buffer будет записано декодировенное значение лексического токена.</param>
		/// <returns>Количество знаков, записанных в buffer.</returns>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int Decode (this StructuredStringToken token, ReadOnlySpan<char> source, Span<char> buffer, int bufferPosition)
		{
			return token.Format.DecodeToken (source.Slice (token.Position, token.Length), buffer[bufferPosition..]);
		}
	}
}
