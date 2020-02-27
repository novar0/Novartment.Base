using Novartment.Base.Text;

namespace Novartment.Base.Net.Smtp
{
	internal class StructuredStringTokenFormatPath : StructuredStringTokenFormat
	{
		internal static readonly StructuredStringTokenFormat Instance = new StructuredStringTokenFormatPath ();

		private StructuredStringTokenFormatPath ()
			: base ('<', '>', IngoreTokenType.QuotedValue, false)
		{
		}
	}
}
