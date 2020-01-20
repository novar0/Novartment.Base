namespace Novartment.Base.IO
{
	/// <summary>
	/// Parameters that describe the current state of the stream: position, size, and an additional custom state object.
	/// </summary>
	public readonly struct FileStreamStatus
	{
		/// <summary>
		/// Initializes a new instance of the FileStreamStatus class
		/// with the specified position, size, and an additional custom state object.
		/// </summary>
		/// <param name="position">The position in the stream.</param>
		/// <param name="length">The size of the stream.</param>
		/// <param name="state">The state object.</param>
		public FileStreamStatus (long position, long length, object state)
		{
			this.Position = position;
			this.Length = length;
			this.State = state;
		}

		/// <summary>Gets the position in the stream.</summary>
		public readonly long Position { get; }

		/// <summary>Gets the size of the stream.</summary>
		public readonly long Length { get; }

		/// <summary>Gets the state object</summary>
		public readonly object State { get; }

		/// <summary>
		/// Deconstruct object into individual properties.
		/// </summary>
		/// <param name="position">When this method returns, the position in the stream.</param>
		/// <param name="length">When this method returns, the size of the stream.</param>
		/// <param name="state">When this method returns, the state object.</param>
		public readonly void Deconstruct (out long position, out long length, out object state)
		{
			position = this.Position;
			length = this.Length;
			state = this.State;
		}
	}
}
