using System;

namespace Novartment.Base.Text
{
	public enum IngoreTokenType
	{
		Unspecified,
		QuotedValue,
		EscapedChar,
	}

	public abstract class StructuredStringTokenFormat
	{
		public StructuredStringTokenFormat (char startMarker, char endMarker, IngoreTokenType ignoreToken, bool allowNesting)
		{
			this.StartMarker = startMarker;
			this.EndMarker = endMarker;
			this.IgnoreToken = ignoreToken;
			this.AllowNesting = allowNesting;
		}

		public char StartMarker { get; }

		public char EndMarker { get; }

		public IngoreTokenType IgnoreToken { get; }

		public bool AllowNesting { get; }

		/// <summary>
		/// Декодирует значение токена в соответствии с его типом.
		/// </summary>
		/// <param name="token">Токен для декодирования.</param>
		/// <param name="source">Исходная строка.</param>
		/// <param name="buffer">Буфер, куда будет записано декодировенное значение токена.</param>
		/// <returns>Количество знаков, записанных в buffer.</returns>
		public virtual int DecodeToken (StructuredStringToken token, ReadOnlySpan<char> source, Span<char> buffer)
		{
			var src = source.Slice (token.Position, token.Length);
			src.CopyTo (buffer);
			return src.Length;
		}
	}

	public class StructuredStringTokenFormatSeparator : StructuredStringTokenFormat
	{
		internal StructuredStringTokenFormatSeparator ()
			: base (default, default, IngoreTokenType.Unspecified, false)
		{
		}

		public override int DecodeToken (StructuredStringToken token, ReadOnlySpan<char> source, Span<char> buffer)
		{
			buffer[0] = source[token.Position];
			return 1;
		}
	}

	public class StructuredStringTokenFormatValue : StructuredStringTokenFormat
	{
		internal StructuredStringTokenFormatValue ()
			: base (default, default, IngoreTokenType.Unspecified, false)
		{
		}
	}
}
