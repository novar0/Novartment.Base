namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// MIME content disposition parameter names. Определено в RFC 2183.
	/// </summary>
	internal static class DispositionParameterNames
	{
		/// <summary>"filename". Suggested filename to be used if the entity is detached and stored in a separate file.</summary>
		internal static readonly string Filename = "filename";

		/// <summary>"creation-date". The date at which the file was created.</summary>
		internal static readonly string CreationDate = "creation-date";

		/// <summary>"modification-date". The date at which the file was last modified.</summary>
		internal static readonly string ModificationDate = "modification-date";

		/// <summary>"read-date". The date at which the file was last read.</summary>
		internal static readonly string ReadDate = "read-date";

		/// <summary>"size". An approximate size of the file in octets.</summary>
		internal static readonly string Size = "size";
	}
}
