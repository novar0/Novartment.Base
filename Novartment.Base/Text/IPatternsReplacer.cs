namespace Novartment.Base.Text
{
	/// <summary>
	/// Support for replacing patterns in the text.
	/// </summary>
	public interface IPatternsReplacer
	{
		/// <summary>
		/// Gets or sets whether the replacement is enabled.
		/// </summary>
		bool ReplacementEnabled { get; set; }

		/// <summary>
		/// Gets or sets the string to be inserted in place of the encountered patterns.
		/// </summary>
		string ReplacementValue { get; set; }

		/// <summary>
		/// Adds a string to the list of patterns to replace.
		/// </summary>
		/// <param name="pattern">The string template for search and replace.</param>
		void AddReplacementStringPattern (string pattern);

		/// <summary>
		/// Adds a regular expression to the list of patterns to replace.
		/// </summary>
		/// <param name="pattern">The regular expression for search and replace.</param>
		void AddReplacementRegexPattern (string pattern);

		/// <summary>
		/// Clears the list of patterns.
		/// </summary>
		void ClearReplacementPatterns ();
	}
}
