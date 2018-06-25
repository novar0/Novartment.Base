using System.IO;
using System.Threading;
using Novartment.Base.BinaryStreaming;
using Xunit;

namespace Novartment.Base.Media.Test
{
	public class RiffParserTests
	{
		[Fact]
		[Trait ("Category", "Media.RiffParser")]
		public void AviInfo_Parse ()
		{
			using (var fs = new FileStream (@"test.avi", FileMode.Open, FileAccess.Read))
			{
				IBufferedSource stream = fs.AsBufferedSource (new byte[1024]);
				var aviInfo = AviInfo.ParseAsync (stream).Result;
				Assert.NotNull (aviInfo);
				Assert.Equal (8U, aviInfo.Width);
				Assert.Equal (4U, aviInfo.Height);
				Assert.Equal (41708U, aviInfo.MicroSecPerFrame);
				Assert.Equal (30U, aviInfo.TotalFrames);
				Assert.Equal (2, aviInfo.Streams.Count);

				// video
				Assert.Equal ("vids", aviInfo.Streams[0].Kind);
				Assert.Equal ("DIB ", aviInfo.Streams[0].Handler);
				Assert.Equal (30U, aviInfo.Streams[0].Length);
				Assert.Equal (2997U, aviInfo.Streams[0].Rate);
				Assert.Equal (125U, aviInfo.Streams[0].Scale);
				Assert.Equal (0U, aviInfo.Streams[0].Start);
				Assert.Equal (0U, aviInfo.Streams[0].Left);
				Assert.Equal (0U, aviInfo.Streams[0].Top);
				Assert.Equal (8U, aviInfo.Streams[0].Right);
				Assert.Equal (4U, aviInfo.Streams[0].Bottom);
				Assert.NotNull (aviInfo.Streams[0].VideoFormat);
				Assert.Null (aviInfo.Streams[0].AudioFormat);
				var videoFormat = aviInfo.Streams[0].VideoFormat;
				Assert.Equal (24U, videoFormat.BitCount);
				Assert.Equal ("0", videoFormat.Compression);
				Assert.Equal (8U, videoFormat.Width);
				Assert.Equal (4U, videoFormat.Height);
				Assert.Equal (96U, videoFormat.SizeImage);

				// audio
				Assert.Equal ("auds", aviInfo.Streams[1].Kind);
				Assert.Null (aviInfo.Streams[1].Handler);
				Assert.Equal (48U, aviInfo.Streams[1].Length);
				Assert.Equal (44100U, aviInfo.Streams[1].Rate);
				Assert.Equal (1152U, aviInfo.Streams[1].Scale);
				Assert.Equal (0U, aviInfo.Streams[1].Start);
				Assert.Null (aviInfo.Streams[1].VideoFormat);
				Assert.NotNull (aviInfo.Streams[1].AudioFormat);
				var audioFormat = aviInfo.Streams[1].AudioFormat;
				Assert.Equal (0U, audioFormat.BitsPerSample);
				Assert.Equal (44100U, audioFormat.SamplesPerSec);
				Assert.Equal (2U, audioFormat.Channels);
			}
		}
	}
}
