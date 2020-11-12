using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Служит разделяющей функцией для чтения заголовка в виде отдельных полей.
	/// </summary>
	internal sealed class HeaderFieldSource : EvaluatorPartitionedBufferedSourceBase
	{
		private readonly IBufferedSource _source;
		private int _foundSeparatorLength = -1;

		internal HeaderFieldSource (IBufferedSource headerSource)
			: base (headerSource)
		{
			_source = headerSource;
		}

		protected override bool IsEndOfPartFound => _foundSeparatorLength >= 0;

		protected override int PartEpilogueSize => _foundSeparatorLength;

		protected override int ValidatePartData (int validatedPartLength)
		{
			var buf = _source.BufferMemory.Span;
			var endOffset = _source.Offset + _source.Count;
			var startOffset = _source.Offset + validatedPartLength;
			_foundSeparatorLength = -1;
			while (startOffset < endOffset)
			{
				if (buf[startOffset] == 0x0d)
				{
					if ((startOffset + 1) >= endOffset)
					{
						return validatedPartLength;
					}

					if (buf[startOffset + 1] == 0x0a)
					{
						var foundSeparatorLength = 2;

						// skips repeating CRLF
						while (true)
						{
							if ((startOffset + foundSeparatorLength) >= endOffset)
							{
								_foundSeparatorLength = foundSeparatorLength;
								return validatedPartLength;
							}

							if ((buf[startOffset + foundSeparatorLength] != 0x0d) ||
								((startOffset + foundSeparatorLength + 1) >= endOffset) ||
								(buf[startOffset + foundSeparatorLength + 1] != 0x0a))
							{
								break;
							}

							foundSeparatorLength += 2;
						}

						if (((startOffset + foundSeparatorLength) >= endOffset) ||
							((buf[startOffset + foundSeparatorLength] != 0x20) && (buf[startOffset + foundSeparatorLength] != 0x09)))
						{
							_foundSeparatorLength = foundSeparatorLength;
							return validatedPartLength;
						}
					}
				}

				startOffset++;
				validatedPartLength++;
			}

			return validatedPartLength;
		}
	}
}
