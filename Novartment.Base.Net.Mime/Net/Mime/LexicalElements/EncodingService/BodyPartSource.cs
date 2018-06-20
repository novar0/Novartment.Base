using System;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Text;

namespace Novartment.Base.Net.Mime
{
	internal class BodyPartSource : EvaluatorPartitionedBufferedSourceBase
	{
		private readonly byte[] _dashBoundary;
		private readonly IBufferedSource _source;
		private int _foundBoundaryLength = -1;
		private bool _lastBoundaryClosed = false;

		internal BodyPartSource (string boundary, IBufferedSource source)
			: base (source)
		{
			_dashBoundary = new byte[boundary.Length + 2];
			_dashBoundary[0] = (byte)'-';
			_dashBoundary[1] = (byte)'-';
			AsciiCharSet.GetBytes (boundary.AsSpan (), _dashBoundary.AsSpan (2));
			_source = source;
		}

		internal bool LastBoundaryClosed => _lastBoundaryClosed;

		protected override bool IsEndOfPartFound => _foundBoundaryLength >= 0;

		protected override int PartEpilogueSize => _foundBoundaryLength;

		protected override int ValidatePartData (int validatedPartLength)
		{
			if (validatedPartLength >= _source.Count)
			{
				_foundBoundaryLength = -1;
				return validatedPartLength;
			}

			var buf = _source.BufferMemory.Span;
			var endOffset = _source.Offset + _source.Count;
			var startOffset = _source.Offset + validatedPartLength;

			// Boundary string comparisons must compare the boundary value with the beginning of each candidate line.
			// An exact match of the entire candidate line is not required;
			// it is sufficient that the boundary appear in its entirety following the CRLF.
			int sourceIdx = 0;
			int templateIdx = 0;

			// search for dash-boundary
			while (((startOffset + sourceIdx) < endOffset) && (templateIdx < _dashBoundary.Length))
			{
				if (buf[startOffset + sourceIdx] == _dashBoundary[templateIdx])
				{
					sourceIdx++;
					templateIdx++;
				}
				else
				{
					startOffset++;
					validatedPartLength++;
					sourceIdx = 0;
					templateIdx = 0;

					// skips optional CRLF
					if (((startOffset + 1) < endOffset) && (buf[startOffset] == 0x0d) && (buf[startOffset + 1] == 0x0a))
					{
						sourceIdx = 2;
					}
				}
			}

			_foundBoundaryLength = -1;
			if (templateIdx < _dashBoundary.Length)
			{
				return validatedPartLength;
			}

			var boundaryClosed = false;

			// skips optional '--'
			if ((startOffset + sourceIdx) >= endOffset)
			{
				return validatedPartLength;
			}

			if (buf[startOffset + sourceIdx] == 0x2d)
			{
				if (((startOffset + sourceIdx + 1) < endOffset) && (buf[startOffset + sourceIdx + 1] == 0x2d))
				{
					sourceIdx += 2;
					boundaryClosed = true;
				}
			}

			// skips optional LWSP
			while (((startOffset + sourceIdx) < endOffset) &&
				(
					(buf[startOffset + sourceIdx] == 0x09) ||
					(buf[startOffset + sourceIdx] == 0x20)))
			{
				sourceIdx++;
			}

			// search for CRLF
			if ((startOffset + sourceIdx) >= endOffset)
			{
				// если источник закончился то считаем завершающий CRLF необязательным
				_lastBoundaryClosed = boundaryClosed;
				_foundBoundaryLength = sourceIdx;
				return validatedPartLength;
			}

			if (buf[startOffset + sourceIdx] != 0x0d)
			{
				return validatedPartLength + sourceIdx;
			}

			sourceIdx++;
			if ((startOffset + sourceIdx) >= endOffset)
			{
				return validatedPartLength;
			}

			if (buf[startOffset + sourceIdx] != 0x0a)
			{
				return validatedPartLength + sourceIdx;
			}

			_lastBoundaryClosed = boundaryClosed;
			_foundBoundaryLength = sourceIdx + 1;
			return validatedPartLength;
		}
	}
}
