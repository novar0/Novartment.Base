using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Collections;
using Novartment.Base.Media;
using static System.Linq.Enumerable;

namespace Novartment.Base.Sample
{
	public static class EbmlSamples
	{
		private static readonly string _folder = @"D:\Temp";
		private static readonly string _mask = @"*.mkv";

		public static async Task CreateChaptersFromMultipleMkvs (CancellationToken cancellationToken)
		{
			int idx = 1;
			var chapsText = new StringBuilder ();
			var durationTotal = new TimeSpan (0L);
			foreach (var file in Directory.EnumerateFiles (_folder, _mask).OrderBy (item => item))
			{
				using (var fs = new FileStream (file, FileMode.Open, FileAccess.Read))
				{
					var stream = fs.AsBufferedSource (new byte[1024]);
					var segment = await MatroskaFile.ParseSegmentInformationAsync (stream, cancellationToken).ConfigureAwait (false);
					if (segment.Tracks.Where (item => item.VideoFormat != null).TryGetFirst (out MatroskaTrackInfo videoStream))
					{
						if (segment.Tracks.Where (item => item.AudioFormat != null).TryGetFirst (out MatroskaTrackInfo audioStream))
						{
							chapsText.AppendFormat (
								CultureInfo.InvariantCulture,
								"CHAPTER{1:00}={0:hh\\:mm\\:ss\\.fff}\r\nCHAPTER{1:00}NAME={6} ({2}x{3}px {4}ch {5}Hz)\r\n",
								durationTotal,
								idx,
								videoStream.VideoFormat.PixelWidth,
								videoStream.VideoFormat.PixelHeight,
								audioStream.AudioFormat.Channels,
								audioStream.AudioFormat.SamplingFrequency,
								Path.GetFileNameWithoutExtension (file));
							durationTotal += new TimeSpan ((long)(segment.Duration * segment.TimeCodeScale / 100d));
							idx++;
						}
					}
				}
			}

			var chapsFileName = Path.Combine (_folder, "chaps.txt");
			File.WriteAllText (chapsFileName, chapsText.ToString ());
		}
	}
}
