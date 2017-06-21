namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Type of symantic atom.
	/// </summary>
	internal enum AtomType
	{
		/// <summary>
		/// RFC 822 'dot-atom' text.
		/// </summary>
		DotAtom = 1,

		/// <summary>RFC 822 'atom' text.</summary>
		/// <remarks>
		/// Printable US-ASCII characters not including <code>()&lt;&gt;@,;:\[]".</code>.
		/// </remarks>
		Atom = 2,

		/// <summary>
		/// RFC 2045 'token' used as parameter names and values text in parameterized headers such as
		/// 'Content-Type', 'Content-Disposition' and 'Disposition-Notification-Options'.
		/// </summary>
		/// <remarks>
		/// Same as Atom, but additionaly excludes symbols '/', '?' and '='.
		/// </remarks>
		Token = 3,
	}
}
