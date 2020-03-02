using System;

namespace Novartment.Base.Text
{
	public class StructuredStringTokenSeparatorFormat : StructuredStringTokenFormat
	{
		public override int DecodeToken (StructuredStringToken token, ReadOnlySpan<char> source, Span<char> buffer)
		{
			buffer[0] = source[token.Position];
			return 1;
		}
	}
}
