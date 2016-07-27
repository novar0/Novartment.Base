using System;
using System.IO;
using System.Threading;
using Xunit;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Media.Test
{

	public class EbmlReaderTests
	{
		[Fact, Trait ("Category", "Media.EbmlParser")]
		public void EbmlParser_Read ()
		{
			using (var fs = new FileStream (@"test.mkv", FileMode.Open, FileAccess.Read))
			{
				var stream = fs.AsBufferedSource (new byte[1024]);

				var reader = new EbmlElementCollectionEnumerator (stream);
				Assert.True (reader.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x1a45dfa3UL, reader.Current.Id);
				var subReader1 = reader.Current.ReadSubElements ();
				Assert.True (subReader1.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x4286UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x42f7UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x42f2UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x42f3UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x4282UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x4287UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x4285UL, subReader1.Current.Id);
				Assert.False (subReader1.MoveNextAsync (CancellationToken.None).Result);

				Assert.True (reader.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x18538067UL, reader.Current.Id);
				subReader1 = reader.Current.ReadSubElements ();
				Assert.True (subReader1.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x114d9b74UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0xecUL, subReader1.Current.Id);

				// common
				Assert.True (subReader1.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x1549a966UL, subReader1.Current.Id);
				var subReader2 = subReader1.Current.ReadSubElements ();
				Assert.True (subReader2.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x2ad7b1UL, subReader2.Current.Id);
				Assert.Equal (1000000UL, subReader2.Current.ReadUInt ());
				Assert.True (subReader2.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x4d80UL, subReader2.Current.Id);
				Assert.Equal ("libebml v1.3.0 + libmatroska v1.4.0", subReader2.Current.ReadUtf ());
				Assert.True (subReader2.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x5741UL, subReader2.Current.Id);
				Assert.Equal ("mkvmerge v6.2.0 ('Promised Land') built on Apr 28 2013 12:22:01", subReader2.Current.ReadUtf ());
				Assert.True (subReader2.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x4489UL, subReader2.Current.Id);
				Assert.Equal (544.0F, subReader2.Current.ReadFloat ());
				Assert.True (subReader2.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x4461UL, subReader2.Current.Id);
				Assert.Equal (new DateTime (629383704700000000L), subReader2.Current.ReadDate ());
				Assert.True (subReader2.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x7ba9UL, subReader2.Current.Id);
				Assert.Equal ("тестовый файл", subReader2.Current.ReadUtf ());
				Assert.True (subReader2.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x73a4UL, subReader2.Current.Id);
				Assert.False (subReader2.MoveNextAsync (CancellationToken.None).Result);

				// tracks
				Assert.True (subReader1.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x1654ae6bUL, subReader1.Current.Id);
				subReader2 = subReader1.Current.ReadSubElements ();
				// tracks.track 1
				Assert.True (subReader2.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0xaeUL, subReader2.Current.Id);
				var subReader3 = subReader2.Current.ReadSubElements ();
				Assert.True (subReader3.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0xd7UL, subReader3.Current.Id);//UnsignedInteger
				Assert.Equal (1UL, subReader3.Current.ReadUInt ());
				Assert.True (subReader3.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x73c5UL, subReader3.Current.Id);//UnsignedInteger
				Assert.True (subReader3.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x83UL, subReader3.Current.Id);//UnsignedInteger
				Assert.Equal (1UL, subReader3.Current.ReadUInt ());
				Assert.True (subReader3.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x9cUL, subReader3.Current.Id);//UnsignedInteger
				Assert.True (subReader3.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x86UL, subReader3.Current.Id);//AsciiString
				Assert.Equal ("V_MS/VFW/FOURCC", subReader3.Current.ReadAscii ());
				Assert.True (subReader3.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x63a2UL, subReader3.Current.Id);//Binary
				Assert.True (subReader3.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x23e383UL, subReader3.Current.Id);//UnsignedInteger
				Assert.Equal (40000000UL, subReader3.Current.ReadUInt ());
				Assert.True (subReader3.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x536eUL, subReader3.Current.Id);//Utf8String
				Assert.Equal ("видео трэк", subReader3.Current.ReadUtf ());
				Assert.True (subReader3.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0xe0UL, subReader3.Current.Id);//MasterElement
				var subReader4 = subReader3.Current.ReadSubElements ();
				Assert.True (subReader4.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0xb0UL, subReader4.Current.Id);
				Assert.Equal (8UL, subReader4.Current.ReadUInt ());
				Assert.True (subReader4.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0xbaUL, subReader4.Current.Id);
				Assert.Equal (4UL, subReader4.Current.ReadUInt ());
				Assert.True (subReader4.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x54b0UL, subReader4.Current.Id);
				Assert.Equal (8UL, subReader4.Current.ReadUInt ());
				Assert.True (subReader4.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x54baUL, subReader4.Current.Id);
				Assert.Equal (4UL, subReader4.Current.ReadUInt ());
				Assert.False (subReader4.MoveNextAsync (CancellationToken.None).Result);
				Assert.False (subReader3.MoveNextAsync (CancellationToken.None).Result);

				// tracks.track 2
				Assert.True (subReader2.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0xaeUL, subReader2.Current.Id);
				subReader3 = subReader2.Current.ReadSubElements ();
				Assert.True (subReader3.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0xd7UL, subReader3.Current.Id);//UnsignedInteger
				Assert.Equal (2UL, subReader3.Current.ReadUInt ());
				Assert.True (subReader3.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x73c5UL, subReader3.Current.Id);//UnsignedInteger
				Assert.True (subReader3.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x83UL, subReader3.Current.Id);//UnsignedInteger
				Assert.Equal (2UL, subReader3.Current.ReadUInt ());
				Assert.True (subReader3.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x86UL, subReader3.Current.Id);//AsciiString
				Assert.Equal ("A_AC3", subReader3.Current.ReadAscii ());
				Assert.True (subReader3.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x23e383UL, subReader3.Current.Id);//UnsignedInteger
				Assert.Equal (32000000UL, subReader3.Current.ReadUInt ());
				Assert.True (subReader3.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x22b59cUL, subReader3.Current.Id);//AsciiString
				Assert.Equal ("rus", subReader3.Current.ReadAscii ());
				Assert.True (subReader3.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x536eUL, subReader3.Current.Id);//Utf8String
				Assert.Equal ("аудио трэк", subReader3.Current.ReadUtf ());
				Assert.True (subReader3.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0xe1UL, subReader3.Current.Id);//MasterElement
				subReader4 = subReader3.Current.ReadSubElements ();
				Assert.True (subReader4.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0xb5UL, subReader4.Current.Id);//Float
				Assert.Equal (48000.0F, subReader4.Current.ReadFloat ());
				Assert.True (subReader4.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x9fUL, subReader4.Current.Id);//UnsignedInteger
				Assert.Equal (2UL, subReader4.Current.ReadUInt ());
				Assert.False (subReader4.MoveNextAsync (CancellationToken.None).Result);
				Assert.False (subReader3.MoveNextAsync (CancellationToken.None).Result);

				// tracks.track 3
				Assert.True (subReader2.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0xaeUL, subReader2.Current.Id);
				subReader3 = subReader2.Current.ReadSubElements ();
				Assert.True (subReader3.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0xd7UL, subReader3.Current.Id);//UnsignedInteger
				Assert.Equal (3UL, subReader3.Current.ReadUInt ());
				Assert.True (subReader3.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x73c5UL, subReader3.Current.Id);//UnsignedInteger
				Assert.True (subReader3.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x83UL, subReader3.Current.Id);//UnsignedInteger
				Assert.Equal (17UL, subReader3.Current.ReadUInt ());
				Assert.True (subReader3.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x55aaUL, subReader3.Current.Id);//UnsignedInteger
				Assert.Equal (1UL, subReader3.Current.ReadUInt ());
				Assert.True (subReader3.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x9cUL, subReader3.Current.Id);//UnsignedInteger
				Assert.True (subReader3.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x86UL, subReader3.Current.Id);//AsciiString
				Assert.Equal ("S_TEXT/UTF8", subReader3.Current.ReadAscii ());
				Assert.True (subReader3.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x22b59cUL, subReader3.Current.Id);//AsciiString
				Assert.Equal ("rus", subReader3.Current.ReadAscii ());
				Assert.True (subReader3.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x536eUL, subReader3.Current.Id);//Utf8String
				Assert.Equal ("субтитры форсированные", subReader3.Current.ReadUtf ());
				Assert.False (subReader3.MoveNextAsync (CancellationToken.None).Result);

				Assert.False (subReader2.MoveNextAsync (CancellationToken.None).Result);
				Assert.True (subReader1.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0xecUL, subReader1.Current.Id);

				// attachments
				Assert.True (subReader1.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x1941a469UL, subReader1.Current.Id);
				subReader2 = subReader1.Current.ReadSubElements ();
				// attachments.attach 1
				Assert.True (subReader2.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x61a7UL, subReader2.Current.Id);
				subReader3 = subReader2.Current.ReadSubElements ();
				Assert.True (subReader3.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x466eUL, subReader3.Current.Id);//Utf8String
				Assert.Equal ("пальмовая ветвь", subReader3.Current.ReadUtf ());
				Assert.True (subReader3.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x4660UL, subReader3.Current.Id);//AsciiString
				Assert.Equal ("image/png", subReader3.Current.ReadAscii ());
				Assert.True (subReader3.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x465cUL, subReader3.Current.Id);//Binary
				Assert.True (subReader3.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x46aeUL, subReader3.Current.Id);//UnsignedInteger
				Assert.False (subReader3.MoveNextAsync (CancellationToken.None).Result);

				Assert.False (subReader2.MoveNextAsync (CancellationToken.None).Result);

				// chapters
				Assert.True (subReader1.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x1043a770UL, subReader1.Current.Id);
				subReader2 = subReader1.Current.ReadSubElements ();
				Assert.True (subReader2.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x45b9UL, subReader2.Current.Id);
				Assert.False (subReader2.MoveNextAsync (CancellationToken.None).Result);

				Assert.True (subReader1.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0xecUL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x1f43b675UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x1f43b675UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x1f43b675UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x1f43b675UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x1f43b675UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x1f43b675UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x1f43b675UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x1f43b675UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x1f43b675UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x1f43b675UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x1f43b675UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x1f43b675UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x1f43b675UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync (CancellationToken.None).Result); Assert.Equal (0x1c53bb6bUL, subReader1.Current.Id);
				Assert.False (subReader1.MoveNextAsync (CancellationToken.None).Result);

				Assert.Equal (fs.Length, fs.Position);
			}
		}

		[Fact, Trait ("Category", "Media.EbmlParser")]
		public void MatroskaSegmentInfo_Parse ()
		{
			using (var fs = new FileStream (@"test.mkv", FileMode.Open, FileAccess.Read))
			{
				var stream = fs.AsBufferedSource (new byte[1024]);
				var segment = MatroskaFile.ParseSegmentInformationAsync (stream, CancellationToken.None).Result;
				Assert.Equal (segment.Title, "тестовый файл");
				Assert.Equal (segment.Date, new DateTime (629383704700000000L));
				Assert.Equal (segment.Duration, 544.0F);
				Assert.Equal (segment.TimeCodeScale, 1000000UL);
				Assert.Equal (segment.Tracks.Count, 3);
				Assert.Equal (segment.Attachments.Count, 1);
				Assert.Equal (segment.Attachments[0].Name, "пальмовая ветвь");
				Assert.Equal (segment.Attachments[0].ContentType, "image/png");

				// track1
				Assert.Equal (segment.Tracks[0].TrackType, MatroskaTrackType.Video);
				Assert.Equal (segment.Tracks[0].Forced, false);
				Assert.Equal (segment.Tracks[0].Codec, "V_MS/VFW/FOURCC");
				Assert.Equal (segment.Tracks[0].DefaultDuration, 40000000UL);
				Assert.Null (segment.Tracks[0].Language);
				Assert.Equal (segment.Tracks[0].Name, "видео трэк");
				Assert.Null (segment.Tracks[0].AudioFormat);
				Assert.NotNull (segment.Tracks[0].VideoFormat);
				Assert.Equal (segment.Tracks[0].VideoFormat.PixelWidth, 8UL);
				Assert.Equal (segment.Tracks[0].VideoFormat.PixelHeight, 4UL);
				Assert.Equal (segment.Tracks[0].VideoFormat.DisplayWidth, 8UL);
				Assert.Equal (segment.Tracks[0].VideoFormat.DisplayHeight, 4UL);

				// track2
				Assert.Equal (segment.Tracks[1].TrackType, MatroskaTrackType.Audio);
				Assert.Equal (segment.Tracks[1].Forced, false);
				Assert.Equal (segment.Tracks[1].Codec, "A_AC3");
				Assert.Equal (segment.Tracks[1].DefaultDuration, 32000000UL);
				Assert.Equal (segment.Tracks[1].Language, "rus");
				Assert.Equal (segment.Tracks[1].Name, "аудио трэк");
				Assert.Null (segment.Tracks[1].VideoFormat);
				Assert.NotNull (segment.Tracks[1].AudioFormat);
				Assert.Equal (segment.Tracks[1].AudioFormat.SamplingFrequency, 48000.0F);
				Assert.Equal (segment.Tracks[1].AudioFormat.Channels, 2UL);

				// track2
				Assert.Equal (segment.Tracks[2].TrackType, MatroskaTrackType.Subtitle);
				Assert.Equal (segment.Tracks[2].Forced, true);
				Assert.Equal (segment.Tracks[2].Codec, "S_TEXT/UTF8");
				Assert.Null (segment.Tracks[2].DefaultDuration);
				Assert.Equal (segment.Tracks[2].Language, "rus");
				Assert.Equal (segment.Tracks[2].Name, "субтитры форсированные");
				Assert.Null (segment.Tracks[2].VideoFormat);
				Assert.Null (segment.Tracks[2].AudioFormat);
			}
		}
	}
}
