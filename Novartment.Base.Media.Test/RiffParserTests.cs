using System.IO;
using System.Threading;
using Xunit;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Media.Test
{

	public class RiffParserTests
	{
		[Fact, Trait ("Category", "Media.RiffParser")]
		public void AviInfo_Parse ()
		{
			using (var fs = new FileStream (@"test.avi", FileMode.Open, FileAccess.Read))
			{
				IBufferedSource stream = fs.AsBufferedSource (new byte[1024]);
				var aviInfo = AviInfo.ParseAsync (stream, CancellationToken.None).Result;
				Assert.NotNull (aviInfo);
				Assert.Equal (aviInfo.Width, 8U);
				Assert.Equal (aviInfo.Height, 4U);
				Assert.Equal (aviInfo.MicroSecPerFrame, 41708U);
				Assert.Equal (aviInfo.TotalFrames, 30U);
				Assert.Equal (aviInfo.Streams.Count, 2);

				// video
				Assert.Equal (aviInfo.Streams[0].Kind, "vids");
				Assert.Equal (aviInfo.Streams[0].Handler, "DIB ");
				Assert.Equal (aviInfo.Streams[0].Length, 30U);
				Assert.Equal (aviInfo.Streams[0].Rate, 2997U);
				Assert.Equal (aviInfo.Streams[0].Scale, 125U);
				Assert.Equal (aviInfo.Streams[0].Start, 0U);
				Assert.Equal (aviInfo.Streams[0].Left, 0U);
				Assert.Equal (aviInfo.Streams[0].Top, 0U);
				Assert.Equal (aviInfo.Streams[0].Right, 8U);
				Assert.Equal (aviInfo.Streams[0].Bottom, 4U);
				Assert.NotNull (aviInfo.Streams[0].VideoFormat);
				Assert.Null (aviInfo.Streams[0].AudioFormat);
				var videoFormat = aviInfo.Streams[0].VideoFormat;
				Assert.Equal (videoFormat.BitCount, 24U);
				Assert.Equal (videoFormat.Compression, "0");
				Assert.Equal (videoFormat.Width, 8U);
				Assert.Equal (videoFormat.Height, 4U);
				Assert.Equal (videoFormat.SizeImage, 96U);

				// audio
				Assert.Equal (aviInfo.Streams[1].Kind, "auds");
				Assert.Null (aviInfo.Streams[1].Handler);
				Assert.Equal (aviInfo.Streams[1].Length, 48U);
				Assert.Equal (aviInfo.Streams[1].Rate, 44100U);
				Assert.Equal (aviInfo.Streams[1].Scale, 1152U);
				Assert.Equal (aviInfo.Streams[1].Start, 0U);
				Assert.Null (aviInfo.Streams[1].VideoFormat);
				Assert.NotNull (aviInfo.Streams[1].AudioFormat);
				var audioFormat = aviInfo.Streams[1].AudioFormat;
				Assert.Equal (audioFormat.BitsPerSample, 0U);
				Assert.Equal (audioFormat.SamplesPerSec, 44100U);
				Assert.Equal (audioFormat.Channels, 2U);
			}
		}
	}
}
