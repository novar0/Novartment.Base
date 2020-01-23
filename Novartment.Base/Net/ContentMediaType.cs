namespace Novartment.Base.Net
{
	/// <summary>
	/// The IANA-standardized media type.
	/// </summary>
	public enum ContentMediaType
	{
		/// <summary>No media type specified.</summary>
		Unspecified = 0,

		/// <summary>
		/// A discrete data which do not fit in any of the other categories,
		/// and particularly for data to be processed by some type of application program.
		/// Defined in RFC 2046 part 4.5 as an "application".
		/// </summary>
		Application = 1,

		/// <summary>
		/// The data contains an audio.
		/// Defined in RFC 2046 part 4.3 as an "audio".
		/// </summary>
		Audio = 2,

		/// <summary>
		/// The data requires a certain graphic subsystem such as a font rendering engine
		/// to process it as font data. Defined in RFC 8081 as a "font".
		/// </summary>
		Font = 3,

		/// <summary>
		/// Used for examples.
		/// Intended only for use in documents providing examples involving specification of some media type,
		/// where the actual media type used is irrelevant. Defined in RFC 4735 as an "example".
		/// </summary>
		Example = 4,

		/// <summary>
		/// The data contains an image.
		/// Defined in RFC 2046 part 4.2 as an "image".
		/// </summary>
		Image = 5,

		/// <summary>
		/// The data encapsulates mail message.
		/// Defined in RFC 2046 part 5.2 as a "message".
		/// </summary>
		Message = 6,

		/// <summary>
		/// The data is electronically exchangeable behavioral or physical representation within a given domain.
		/// Defined in RFC 2077 as a "model".
		/// </summary>
		Model = 7,

		/// <summary>
		/// The data in which one or more different sets of data are combined.
		/// Defined in RFC 2046 part 5.1 as a "multipart".
		/// </summary>
		Multipart = 8,

		/// <summary>
		/// The data is principally textual in form.
		/// Defined in RFC 2046 part 4.1 as a "text".
		/// </summary>
		Text = 9,

		/// <summary>
		/// The data contains a time-varying-picture image, possibly with color and coordinated sound.
		/// Defined in RFC 2046 part 4.4 as a "video".
		/// </summary>
		Video = 10,
	}
}
