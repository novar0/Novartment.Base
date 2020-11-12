using System;
using Novartment.Base.BinaryStreaming;
using Xunit;

namespace Novartment.Base.Test
{
	public class BinaryStreamingStreamExtensionsTests
	{
		[Fact]
		[Trait ("Category", "BufferedSource")]
		public void IsEmpty ()
		{
			var src = new BigBufferedSourceMock (0, 1, FillFunction);
			src.LoadAsync ().AsTask ().Wait ();
			Assert.True (BufferedSourceExtensions.IsEmpty (src));

			src = new BigBufferedSourceMock (1, 1, FillFunction);
			src.LoadAsync ().AsTask ().Wait ();
			Assert.False (BufferedSourceExtensions.IsEmpty (src));
			src.Skip (1);
			Assert.True (BufferedSourceExtensions.IsEmpty (src));

			src = new BigBufferedSourceMock (long.MaxValue, 32768, FillFunction);
			src.LoadAsync ().AsTask ().Wait ();
			Assert.False (BufferedSourceExtensions.IsEmpty (src));
			src.SkipWihoutBufferingAsync (long.MaxValue - 1).AsTask ().Wait ();
			src.LoadAsync ().AsTask ().Wait ();
			Assert.False (BufferedSourceExtensions.IsEmpty (src));
			src.Skip (1);
			Assert.True (BufferedSourceExtensions.IsEmpty (src));
		}

		[Fact]
		[Trait ("Category", "BufferedSource")]
		public void CopyToBufferUntilMarker ()
		{
			int bufSize = 5;
			var dest = new byte[1000];

			// first byte in first buffer
			Array.Fill<byte> (dest, 85);
			var src = new BigBufferedSourceMock (long.MaxValue, bufSize, FillFunction);
			var vTask = src.ReadToMarkerAsync (170, dest).AsTask ();
			Assert.True (vTask.IsCompletedSuccessfully);
			var result = vTask.Result;
			Assert.Equal (0, result); // no bytes copied
			Assert.Equal (85, dest[0]); // destination untouched
			Assert.Equal (170, src.BufferMemory.Span[src.Offset]); // marker in source at start

			// not found with size less than buffer
			Array.Fill<byte> (dest, 85);
			src = new BigBufferedSourceMock (4, bufSize, FillFunction);
			vTask = src.ReadToMarkerAsync (85, dest).AsTask ();
			Assert.True (vTask.IsCompletedSuccessfully);
			result = vTask.Result;
			Assert.Equal (4, result); // no bytes copied
			Assert.Equal (170, dest[0]);
			Assert.Equal (171, dest[1]);
			Assert.Equal (168, dest[2]);
			Assert.Equal (169, dest[3]);
			Assert.Equal (85, dest[4]); // destination untouched
			Assert.Equal (0, src.Count);
			Assert.True (src.IsExhausted);

			// not found with size bigger than buffer
			Array.Fill<byte> (dest, 85);
			src = new BigBufferedSourceMock (7, bufSize, FillFunction);
			vTask = src.ReadToMarkerAsync (85, dest).AsTask ();
			Assert.True (vTask.IsCompletedSuccessfully);
			result = vTask.Result;
			Assert.Equal (7, result); // no bytes copied
			Assert.Equal (170, dest[0]);
			Assert.Equal (171, dest[1]);
			Assert.Equal (168, dest[2]);
			Assert.Equal (169, dest[3]);
			Assert.Equal (174, dest[4]);
			Assert.Equal (175, dest[5]);
			Assert.Equal (172, dest[6]);
			Assert.Equal (85, dest[7]); // destination untouched
			Assert.Equal (0, src.Count); // marker in source at start
			Assert.True (src.IsExhausted);

			// last byte in first buffer
			Array.Fill<byte> (dest, 85);
			src = new BigBufferedSourceMock (long.MaxValue, bufSize, FillFunction);
			vTask = src.ReadToMarkerAsync (174, dest).AsTask ();
			Assert.True (vTask.IsCompletedSuccessfully);
			result = vTask.Result;
			Assert.Equal (4, result);
			Assert.Equal (170, dest[0]);
			Assert.Equal (171, dest[1]);
			Assert.Equal (168, dest[2]);
			Assert.Equal (169, dest[3]);
			Assert.Equal (85, dest[4]); // destination untouched
			Assert.Equal (174, src.BufferMemory.Span[src.Offset]); // marker in source at start

			// first byte in second buffer
			Array.Fill<byte> (dest, 85);
			src = new BigBufferedSourceMock (long.MaxValue, bufSize, FillFunction);
			vTask = src.ReadToMarkerAsync (175, dest).AsTask ();
			Assert.True (vTask.IsCompletedSuccessfully);
			result = vTask.Result;
			Assert.Equal (5, result);
			Assert.Equal (170, dest[0]);
			Assert.Equal (171, dest[1]);
			Assert.Equal (168, dest[2]);
			Assert.Equal (169, dest[3]);
			Assert.Equal (174, dest[4]);
			Assert.Equal (85, dest[5]); // destination untouched
			Assert.Equal (175, src.BufferMemory.Span[src.Offset]); // marker in source at start

			// last byte of source
			Array.Fill<byte> (dest, 85);
			src = new BigBufferedSourceMock (7, bufSize, FillFunction);
			vTask = src.ReadToMarkerAsync (172, dest).AsTask ();
			Assert.True (vTask.IsCompletedSuccessfully);
			result = vTask.Result;
			Assert.Equal (6, result);
			Assert.Equal (170, dest[0]);
			Assert.Equal (171, dest[1]);
			Assert.Equal (168, dest[2]);
			Assert.Equal (169, dest[3]);
			Assert.Equal (174, dest[4]);
			Assert.Equal (175, dest[5]);
			Assert.Equal (85, dest[6]); // destination untouched
			Assert.Equal (172, src.BufferMemory.Span[src.Offset]); // marker in source at start
			Assert.Equal (1, src.Count);
		}

		[Fact]
		[Trait ("Category", "BufferedSource")]
		public void Read ()
		{
			int srcBufSize = 32768;
			int testSampleSize = 68;
			var src = new BigBufferedSourceMock (long.MaxValue, srcBufSize, FillFunction);
			src.LoadAsync ().AsTask ().Wait ();
			var skip = srcBufSize - testSampleSize;
			src.Skip (skip);
			var buf = new byte[testSampleSize];
			var vTask = BufferedSourceExtensions.ReadAsync (src, buf).AsTask ();
			Assert.True (vTask.IsCompletedSuccessfully);
			Assert.Equal (testSampleSize, vTask.Result);

			for (int i = 0; i < testSampleSize; i++)
			{
				Assert.Equal (FillFunction ((long)(skip + i)), buf[i]);
			}

			src.SkipWihoutBufferingAsync (long.MaxValue - (long)srcBufSize - 3).AsTask ().Wait ();
			vTask = BufferedSourceExtensions.ReadAsync (src, buf).AsTask ();
			Assert.True (vTask.IsCompletedSuccessfully);
			Assert.Equal (3, vTask.Result);
		}

		[Theory]
		[Trait ("Category", "BufferedSource")]
		[InlineData (0, 16)] // пустой источник
		[InlineData (1163, 387)] // чтение больше буфера
		[InlineData (1163, 10467)] // чтение меньше буфера
		public void ReadAllBytes (int testSampleSize, int srcBufSize)
		{
			long skipSize = long.MaxValue - (long)testSampleSize;

			var src = new BigBufferedSourceMock (long.MaxValue, srcBufSize, FillFunction);
			src.SkipWihoutBufferingAsync (skipSize).AsTask ().Wait ();
			var vTask = BufferedSourceExtensions.ReadAllBytesAsync (src).AsTask ();
			Assert.True (vTask.IsCompletedSuccessfully);
			var result = vTask.Result;
			Assert.Equal (testSampleSize, result.Length);
			for (int i = 0; i < testSampleSize; i++)
			{
				Assert.Equal (FillFunction ((long)(skipSize + i)), result.Span[i]);
			}
		}

		[Fact]
		[Trait ("Category", "BufferedSource")]
		public void WriteTo ()
		{
			int testSampleSize = 1163;
			long skipSize = long.MaxValue - (long)testSampleSize;
			int srcBufSize = testSampleSize / 3;
			var src = new BigBufferedSourceMock (long.MaxValue, srcBufSize, FillFunction);
			src.SkipWihoutBufferingAsync (skipSize).AsTask ().Wait ();
			var dst = new BinaryDestinationMock (8192);
			var vTask = BufferedSourceExtensions.WriteToAsync (src, dst);
			Assert.True (vTask.IsCompletedSuccessfully);
			Assert.Equal (testSampleSize, vTask.Result);

			for (int i = 0; i < testSampleSize; i++)
			{
				Assert.Equal (FillFunction ((long)(skipSize + i)), dst.Buffer[i]);
			}
		}

		private static byte FillFunction (long position)
		{
// 170 171 168 169 174 175 172 173 162 163 160 161 166 167 164 165 186 187 184 185 190 191 188 189 178 179 176 177 182 183 180 181 138 139 136 137 142 143 140
// 141 130 131 128 129 134 135 132 133 154 155 152 153 158 159 156 157 146 147 144 145 150 151 148 149 234 235 232 233 238 239 236 237 226 227 224 225 230 231
// 228 229 250 251 248 249 254 255 252 253 242 243 240 241 246 247 244 245 202 203 200 201 206 207 204 205 194 195 192 193 198 199 196 197 218 219 216 217 222
// 223 220 221 210 211 208 209 214 215 212 213 42 43 40 41 46 47 44 45 34 35 32 33 38 39 36 37 58 59 56 57 62 63 60 61 50 51 48 49 54 55 52 53 10 11 8 9
// 14 15 12 13 2 3 0 1 6 7 4 5 26 27 24 25 30 31 28 29 18 19 16 17 22 23 20 21 106 107 104 105 110 111 108 109 98 99 96 97 102 103 100 101 122
// 123 120 121 126 127 124 125 114 115 112 113 118 119 116 117 74 75 72 73 78 79 76 77 66 67 64 65 70 71 68 69 90 91 88 89 94 95 92 93 82 83 80 81 86 87 84 85
				return (byte)(0xAA ^ (position & 0xFF));
		}
	}
}
