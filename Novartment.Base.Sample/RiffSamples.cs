using System;
using static System.Linq.Enumerable;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.Collections;
using Novartment.Base.Media;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Sample
{
	public class RiffSamples
	{
		private static readonly string _folder = @"C:\Temp";
		private static readonly string _mask = @"*.avi";

		public static async Task CreateChaptersFromMultipleAviAsync (CancellationToken cancellationToken)
		{
			int idx = 1;
			var chapsText = new StringBuilder ();
			var duration = new TimeSpan (0L);
			foreach (var file in Directory.EnumerateFiles (_folder, _mask).OrderBy (item => item))
			{
				using (var fs = new FileStream (file, FileMode.Open, FileAccess.Read))
				{
					IBufferedSource stream = fs.AsBufferedSource (new byte[1024]);
					var aviInfo = await AviInfo.ParseAsync (stream, cancellationToken).ConfigureAwait (false);

					var secondsGlobal = (double)aviInfo.MicroSecPerFrame * (double)aviInfo.TotalFrames / 1000000.0f;

					AviStreamInfo videoStream;
					if (aviInfo.Streams.Where (item => item.VideoFormat != null).TryGetFirst (out videoStream))
					{
						var frameDuration = (double)videoStream.Scale / (double)videoStream.Rate;
						var secondsVideo = ((double)videoStream.Length + (double)videoStream.Start) * frameDuration;
					}

					AviStreamInfo audioStream;
					if (aviInfo.Streams.Where (item => item.AudioFormat != null).TryGetFirst (out audioStream))
					{
						var sampleDuration = 1.0f / (double)audioStream.Rate;
						var secondsAudio = ((double)audioStream.Length + (double)audioStream.Start) * sampleDuration;
					}

					chapsText.AppendFormat ("CHAPTER{1:00}={0:hh\\:mm\\:ss\\.fff}\r\nCHAPTER{1:00}NAME={6} ({2}x{3}px {4}ch {5}Hz)\r\n",
						duration,
						idx,
						videoStream.VideoFormat.Width,
						videoStream.VideoFormat.Height,
						audioStream.AudioFormat.Channels,
						audioStream.AudioFormat.SamplesPerSec,
						Path.GetFileNameWithoutExtension (file));
					duration += new TimeSpan ((long)(secondsGlobal * 10000000d));
					idx++;
				}
			}
			var chapsFileName = Path.Combine (_folder, "chaps.txt");
			File.WriteAllText (chapsFileName, chapsText.ToString ());
		}
	}
}
