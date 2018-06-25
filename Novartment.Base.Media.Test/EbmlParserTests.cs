using System;
using System.IO;
using System.Threading;
using Novartment.Base.BinaryStreaming;
using Xunit;

namespace Novartment.Base.Media.Test
{
	public class EbmlParserTests
	{
		[Fact]
		[Trait ("Category", "Media.EbmlParser")]
		public void EbmlParser_Read ()
		{
			using (var fs = new FileStream (@"test.mkv", FileMode.Open, FileAccess.Read))
			{
				var stream = fs.AsBufferedSource (new byte[1024]);

				var reader = new EbmlElementCollectionEnumerator (stream);
				Assert.True (reader.MoveNextAsync ().Result);
				Assert.Equal (0x1a45dfa3UL, reader.Current.Id);
				var subReader1 = reader.Current.ReadSubElements ();
				Assert.True (subReader1.MoveNextAsync ().Result);
				Assert.Equal (0x4286UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync ().Result);
				Assert.Equal (0x42f7UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync ().Result);
				Assert.Equal (0x42f2UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync ().Result);
				Assert.Equal (0x42f3UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync ().Result);
				Assert.Equal (0x4282UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync ().Result);
				Assert.Equal (0x4287UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync ().Result);
				Assert.Equal (0x4285UL, subReader1.Current.Id);
				Assert.False (subReader1.MoveNextAsync ().Result);

				Assert.True (reader.MoveNextAsync ().Result);
				Assert.Equal (0x18538067UL, reader.Current.Id);
				subReader1 = reader.Current.ReadSubElements ();
				Assert.True (subReader1.MoveNextAsync ().Result);
				Assert.Equal (0x114d9b74UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync ().Result);
				Assert.Equal (0xecUL, subReader1.Current.Id);

				// common
				Assert.True (subReader1.MoveNextAsync ().Result);
				Assert.Equal (0x1549a966UL, subReader1.Current.Id);
				var subReader2 = subReader1.Current.ReadSubElements ();
				Assert.True (subReader2.MoveNextAsync ().Result);
				Assert.Equal (0x2ad7b1UL, subReader2.Current.Id);
				Assert.Equal (1000000UL, subReader2.Current.ReadUInt ());
				Assert.True (subReader2.MoveNextAsync ().Result);
				Assert.Equal (0x4d80UL, subReader2.Current.Id);
				Assert.Equal ("libebml v1.3.0 + libmatroska v1.4.0", subReader2.Current.ReadUtf ());
				Assert.True (subReader2.MoveNextAsync ().Result);
				Assert.Equal (0x5741UL, subReader2.Current.Id);
				Assert.Equal ("mkvmerge v6.2.0 ('Promised Land') built on Apr 28 2013 12:22:01", subReader2.Current.ReadUtf ());
				Assert.True (subReader2.MoveNextAsync ().Result);
				Assert.Equal (0x4489UL, subReader2.Current.Id);
				Assert.Equal (544.0F, subReader2.Current.ReadFloat ());
				Assert.True (subReader2.MoveNextAsync ().Result);
				Assert.Equal (0x4461UL, subReader2.Current.Id);
				Assert.Equal (new DateTime (629383704700000000L), subReader2.Current.ReadDate ());
				Assert.True (subReader2.MoveNextAsync ().Result);
				Assert.Equal (0x7ba9UL, subReader2.Current.Id);
				Assert.Equal ("тестовый файл", subReader2.Current.ReadUtf ());
				Assert.True (subReader2.MoveNextAsync ().Result);
				Assert.Equal (0x73a4UL, subReader2.Current.Id);
				Assert.False (subReader2.MoveNextAsync ().Result);

				// tracks
				Assert.True (subReader1.MoveNextAsync ().Result);
				Assert.Equal (0x1654ae6bUL, subReader1.Current.Id);
				subReader2 = subReader1.Current.ReadSubElements ();

				// tracks.track 1
				Assert.True (subReader2.MoveNextAsync ().Result);
				Assert.Equal (0xaeUL, subReader2.Current.Id);
				var subReader3 = subReader2.Current.ReadSubElements ();
				Assert.True (subReader3.MoveNextAsync ().Result);
				Assert.Equal (0xd7UL, subReader3.Current.Id); // UnsignedInteger
				Assert.Equal (1UL, subReader3.Current.ReadUInt ());
				Assert.True (subReader3.MoveNextAsync ().Result);
				Assert.Equal (0x73c5UL, subReader3.Current.Id); // UnsignedInteger
				Assert.True (subReader3.MoveNextAsync ().Result);
				Assert.Equal (0x83UL, subReader3.Current.Id); // UnsignedInteger
				Assert.Equal (1UL, subReader3.Current.ReadUInt ());
				Assert.True (subReader3.MoveNextAsync ().Result);
				Assert.Equal (0x9cUL, subReader3.Current.Id); // UnsignedInteger
				Assert.True (subReader3.MoveNextAsync ().Result);
				Assert.Equal (0x86UL, subReader3.Current.Id); // AsciiString
				Assert.Equal ("V_MS/VFW/FOURCC", subReader3.Current.ReadAscii ());
				Assert.True (subReader3.MoveNextAsync ().Result);
				Assert.Equal (0x63a2UL, subReader3.Current.Id); // Binary
				Assert.True (subReader3.MoveNextAsync ().Result);
				Assert.Equal (0x23e383UL, subReader3.Current.Id); // UnsignedInteger
				Assert.Equal (40000000UL, subReader3.Current.ReadUInt ());
				Assert.True (subReader3.MoveNextAsync ().Result);
				Assert.Equal (0x536eUL, subReader3.Current.Id); // Utf8String
				Assert.Equal ("видео трэк", subReader3.Current.ReadUtf ());
				Assert.True (subReader3.MoveNextAsync ().Result);
				Assert.Equal (0xe0UL, subReader3.Current.Id); // MasterElement
				var subReader4 = subReader3.Current.ReadSubElements ();
				Assert.True (subReader4.MoveNextAsync ().Result);
				Assert.Equal (0xb0UL, subReader4.Current.Id);
				Assert.Equal (8UL, subReader4.Current.ReadUInt ());
				Assert.True (subReader4.MoveNextAsync ().Result);
				Assert.Equal (0xbaUL, subReader4.Current.Id);
				Assert.Equal (4UL, subReader4.Current.ReadUInt ());
				Assert.True (subReader4.MoveNextAsync ().Result);
				Assert.Equal (0x54b0UL, subReader4.Current.Id);
				Assert.Equal (8UL, subReader4.Current.ReadUInt ());
				Assert.True (subReader4.MoveNextAsync ().Result);
				Assert.Equal (0x54baUL, subReader4.Current.Id);
				Assert.Equal (4UL, subReader4.Current.ReadUInt ());
				Assert.False (subReader4.MoveNextAsync ().Result);
				Assert.False (subReader3.MoveNextAsync ().Result);

				// tracks.track 2
				Assert.True (subReader2.MoveNextAsync ().Result);
				Assert.Equal (0xaeUL, subReader2.Current.Id);
				subReader3 = subReader2.Current.ReadSubElements ();
				Assert.True (subReader3.MoveNextAsync ().Result);
				Assert.Equal (0xd7UL, subReader3.Current.Id); // UnsignedInteger
				Assert.Equal (2UL, subReader3.Current.ReadUInt ());
				Assert.True (subReader3.MoveNextAsync ().Result);
				Assert.Equal (0x73c5UL, subReader3.Current.Id); // UnsignedInteger
				Assert.True (subReader3.MoveNextAsync ().Result);
				Assert.Equal (0x83UL, subReader3.Current.Id); // UnsignedInteger
				Assert.Equal (2UL, subReader3.Current.ReadUInt ());
				Assert.True (subReader3.MoveNextAsync ().Result);
				Assert.Equal (0x86UL, subReader3.Current.Id); // AsciiString
				Assert.Equal ("A_AC3", subReader3.Current.ReadAscii ());
				Assert.True (subReader3.MoveNextAsync ().Result);
				Assert.Equal (0x23e383UL, subReader3.Current.Id); // UnsignedInteger
				Assert.Equal (32000000UL, subReader3.Current.ReadUInt ());
				Assert.True (subReader3.MoveNextAsync ().Result);
				Assert.Equal (0x22b59cUL, subReader3.Current.Id); // AsciiString
				Assert.Equal ("rus", subReader3.Current.ReadAscii ());
				Assert.True (subReader3.MoveNextAsync ().Result);
				Assert.Equal (0x536eUL, subReader3.Current.Id); // Utf8String
				Assert.Equal ("аудио трэк", subReader3.Current.ReadUtf ());
				Assert.True (subReader3.MoveNextAsync ().Result);
				Assert.Equal (0xe1UL, subReader3.Current.Id); // MasterElement
				subReader4 = subReader3.Current.ReadSubElements ();
				Assert.True (subReader4.MoveNextAsync ().Result);
				Assert.Equal (0xb5UL, subReader4.Current.Id); // Float
				Assert.Equal (48000.0F, subReader4.Current.ReadFloat ());
				Assert.True (subReader4.MoveNextAsync ().Result);
				Assert.Equal (0x9fUL, subReader4.Current.Id); // UnsignedInteger
				Assert.Equal (2UL, subReader4.Current.ReadUInt ());
				Assert.False (subReader4.MoveNextAsync ().Result);
				Assert.False (subReader3.MoveNextAsync ().Result);

				// tracks.track 3
				Assert.True (subReader2.MoveNextAsync ().Result);
				Assert.Equal (0xaeUL, subReader2.Current.Id);
				subReader3 = subReader2.Current.ReadSubElements ();
				Assert.True (subReader3.MoveNextAsync ().Result);
				Assert.Equal (0xd7UL, subReader3.Current.Id); // UnsignedInteger
				Assert.Equal (3UL, subReader3.Current.ReadUInt ());
				Assert.True (subReader3.MoveNextAsync ().Result);
				Assert.Equal (0x73c5UL, subReader3.Current.Id); // UnsignedInteger
				Assert.True (subReader3.MoveNextAsync ().Result);
				Assert.Equal (0x83UL, subReader3.Current.Id); // UnsignedInteger
				Assert.Equal (17UL, subReader3.Current.ReadUInt ());
				Assert.True (subReader3.MoveNextAsync ().Result);
				Assert.Equal (0x55aaUL, subReader3.Current.Id); // UnsignedInteger
				Assert.Equal (1UL, subReader3.Current.ReadUInt ());
				Assert.True (subReader3.MoveNextAsync ().Result);
				Assert.Equal (0x9cUL, subReader3.Current.Id); // UnsignedInteger
				Assert.True (subReader3.MoveNextAsync ().Result);
				Assert.Equal (0x86UL, subReader3.Current.Id); // AsciiString
				Assert.Equal ("S_TEXT/UTF8", subReader3.Current.ReadAscii ());
				Assert.True (subReader3.MoveNextAsync ().Result);
				Assert.Equal (0x22b59cUL, subReader3.Current.Id); // AsciiString
				Assert.Equal ("rus", subReader3.Current.ReadAscii ());
				Assert.True (subReader3.MoveNextAsync ().Result);
				Assert.Equal (0x536eUL, subReader3.Current.Id); // Utf8String
				Assert.Equal ("субтитры форсированные", subReader3.Current.ReadUtf ());
				Assert.False (subReader3.MoveNextAsync ().Result);

				Assert.False (subReader2.MoveNextAsync ().Result);
				Assert.True (subReader1.MoveNextAsync ().Result);
				Assert.Equal (0xecUL, subReader1.Current.Id);

				// attachments
				Assert.True (subReader1.MoveNextAsync ().Result);
				Assert.Equal (0x1941a469UL, subReader1.Current.Id);
				subReader2 = subReader1.Current.ReadSubElements ();

				// attachments.attach 1
				Assert.True (subReader2.MoveNextAsync ().Result);
				Assert.Equal (0x61a7UL, subReader2.Current.Id);
				subReader3 = subReader2.Current.ReadSubElements ();
				Assert.True (subReader3.MoveNextAsync ().Result);
				Assert.Equal (0x466eUL, subReader3.Current.Id); // Utf8String
				Assert.Equal ("пальмовая ветвь", subReader3.Current.ReadUtf ());
				Assert.True (subReader3.MoveNextAsync ().Result);
				Assert.Equal (0x4660UL, subReader3.Current.Id); // AsciiString
				Assert.Equal ("image/png", subReader3.Current.ReadAscii ());
				Assert.True (subReader3.MoveNextAsync ().Result);
				Assert.Equal (0x465cUL, subReader3.Current.Id); // Binary
				Assert.True (subReader3.MoveNextAsync ().Result);
				Assert.Equal (0x46aeUL, subReader3.Current.Id); // UnsignedInteger
				Assert.False (subReader3.MoveNextAsync ().Result);

				Assert.False (subReader2.MoveNextAsync ().Result);

				// chapters
				Assert.True (subReader1.MoveNextAsync ().Result);
				Assert.Equal (0x1043a770UL, subReader1.Current.Id);
				subReader2 = subReader1.Current.ReadSubElements ();
				Assert.True (subReader2.MoveNextAsync ().Result);
				Assert.Equal (0x45b9UL, subReader2.Current.Id);
				Assert.False (subReader2.MoveNextAsync ().Result);

				Assert.True (subReader1.MoveNextAsync ().Result);
				Assert.Equal (0xecUL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync ().Result);
				Assert.Equal (0x1f43b675UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync ().Result);
				Assert.Equal (0x1f43b675UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync ().Result);
				Assert.Equal (0x1f43b675UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync ().Result);
				Assert.Equal (0x1f43b675UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync ().Result);
				Assert.Equal (0x1f43b675UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync ().Result);
				Assert.Equal (0x1f43b675UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync ().Result);
				Assert.Equal (0x1f43b675UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync ().Result);
				Assert.Equal (0x1f43b675UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync ().Result);
				Assert.Equal (0x1f43b675UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync ().Result);
				Assert.Equal (0x1f43b675UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync ().Result);
				Assert.Equal (0x1f43b675UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync ().Result);
				Assert.Equal (0x1f43b675UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync ().Result);
				Assert.Equal (0x1f43b675UL, subReader1.Current.Id);
				Assert.True (subReader1.MoveNextAsync ().Result);
				Assert.Equal (0x1c53bb6bUL, subReader1.Current.Id);
				Assert.False (subReader1.MoveNextAsync ().Result);

				Assert.Equal (fs.Length, fs.Position);
			}
		}

		[Fact]
		[Trait ("Category", "Media.EbmlParser")]
		public void MatroskaSegmentInfo_Parse ()
		{
			using (var fs = new FileStream (@"test.mkv", FileMode.Open, FileAccess.Read))
			{
				var stream = fs.AsBufferedSource (new byte[1024]);
				var segment = MatroskaFile.ParseSegmentInformationAsync (stream).Result;
				Assert.Equal ("тестовый файл", segment.Title);
				Assert.Equal (new DateTime (629383704700000000L), segment.Date);
				Assert.Equal (544.0F, segment.Duration);
				Assert.Equal (1000000UL, segment.TimeCodeScale);
				Assert.Equal (3, segment.Tracks.Count);
				Assert.Equal (1, segment.Attachments.Count);
				Assert.Equal ("пальмовая ветвь", segment.Attachments[0].Name);
				Assert.Equal ("image/png", segment.Attachments[0].ContentType);

				// track1
				Assert.Equal (MatroskaTrackType.Video, segment.Tracks[0].TrackType);
				Assert.False (segment.Tracks[0].Forced);
				Assert.Equal ("V_MS/VFW/FOURCC", segment.Tracks[0].Codec);
				Assert.Equal (40000000UL, segment.Tracks[0].DefaultDuration);
				Assert.Null (segment.Tracks[0].Language);
				Assert.Equal ("видео трэк", segment.Tracks[0].Name);
				Assert.Null (segment.Tracks[0].AudioFormat);
				Assert.NotNull (segment.Tracks[0].VideoFormat);
				Assert.Equal (8UL, segment.Tracks[0].VideoFormat.PixelWidth);
				Assert.Equal (4UL, segment.Tracks[0].VideoFormat.PixelHeight);
				Assert.Equal (8UL, segment.Tracks[0].VideoFormat.DisplayWidth);
				Assert.Equal (4UL, segment.Tracks[0].VideoFormat.DisplayHeight);

				// track2
				Assert.Equal (MatroskaTrackType.Audio, segment.Tracks[1].TrackType);
				Assert.False (segment.Tracks[1].Forced);
				Assert.Equal ("A_AC3", segment.Tracks[1].Codec);
				Assert.Equal (32000000UL, segment.Tracks[1].DefaultDuration);
				Assert.Equal ("rus", segment.Tracks[1].Language);
				Assert.Equal ("аудио трэк", segment.Tracks[1].Name);
				Assert.Null (segment.Tracks[1].VideoFormat);
				Assert.NotNull (segment.Tracks[1].AudioFormat);
				Assert.Equal (48000.0F, segment.Tracks[1].AudioFormat.SamplingFrequency);
				Assert.Equal (2UL, segment.Tracks[1].AudioFormat.Channels);

				// track2
				Assert.Equal (MatroskaTrackType.Subtitle, segment.Tracks[2].TrackType);
				Assert.True (segment.Tracks[2].Forced);
				Assert.Equal ("S_TEXT/UTF8", segment.Tracks[2].Codec);
				Assert.Null (segment.Tracks[2].DefaultDuration);
				Assert.Equal ("rus", segment.Tracks[2].Language);
				Assert.Equal ("субтитры форсированные", segment.Tracks[2].Name);
				Assert.Null (segment.Tracks[2].VideoFormat);
				Assert.Null (segment.Tracks[2].AudioFormat);
			}
		}
	}
}
