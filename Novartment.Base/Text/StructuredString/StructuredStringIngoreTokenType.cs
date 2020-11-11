namespace Novartment.Base.Text
{
	/// <summary>
	/// Token types to ignore in value.
	/// </summary>
	public enum StructuredStringIngoreTokenType
	{
		/// <summary>Unspecified.</summary>
		Unspecified,

		/// <summary>Quoted values.</summary>
		QuotedValue,

		/// <summary>Escaped values.</summary>
		EscapedChar,
	}
}
