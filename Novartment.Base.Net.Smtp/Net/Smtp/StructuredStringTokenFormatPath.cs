using Novartment.Base.Text;

namespace Novartment.Base.Net.Smtp
{
	internal sealed class StructuredStringTokenFormatPath : StructuredStringTokenDelimitedFormat
	{
		internal static readonly StructuredStringTokenDelimitedFormat Instance = new StructuredStringTokenFormatPath ();

		private StructuredStringTokenFormatPath ()
			: base ('<', '>', StructuredStringIngoreTokenType.QuotedValue, false)
		{
		}
	}
}
