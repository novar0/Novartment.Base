using System;

namespace Novartment.Base.Text
{
	public class StructuredStringTokenValueFormat : StructuredStringTokenFormat
	{
		public override int DecodeToken (StructuredStringToken token, ReadOnlySpan<char> source, Span<char> buffer)
		{
			var src = source.Slice (token.Position, token.Length);
			src.CopyTo (buffer);
			return src.Length;
		}
	}
}
