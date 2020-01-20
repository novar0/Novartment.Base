using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.BinaryStreaming;
using Xunit;

namespace Novartment.Base.Test
{
	public class BufferedChannelTests
	{
		[Fact]
		[Trait ("Category", "BufferedSource")]
		public void Skip_MoreThanWrited ()
		{
			// пропуск больше чем записано
			var channel = new BufferedChannel (new byte[99]);
			var skipTask = channel.SkipWihoutBufferingAsync (long.MaxValue);
			Assert.False (skipTask.IsCompleted);
			Memory<byte> buf = new byte[0x4000000];
			var writeTask = channel.WriteAsync (buf);
			Assert.True (writeTask.IsCompleted);
			Assert.False (skipTask.IsCompleted);
			writeTask = channel.WriteAsync (buf.Slice (0x2000000, 0x1ffffff));
			Assert.True (writeTask.IsCompleted);
			Assert.False (skipTask.IsCompleted);
			channel.SetComplete ();
			Thread.Sleep (50);
			Assert.True (skipTask.IsCompleted);
			Assert.Equal (0x4000000 + 0x1ffffff, skipTask.Result);
		}

		[Fact]
		[Trait ("Category", "BufferedSource")]
		public void Skip_LongSizeWithTinyBuffer ()
		{
			// пропуск больше чем 32битное число байтов через крохотный буфер
			var channel = new BufferedChannel (new byte[9]);
			var skipTask = channel.SkipWihoutBufferingAsync (0x100000000L);
			Assert.False (skipTask.IsCompleted);
			ValueTask writeTask;
			var buf = new byte[0x4000000];
			for (int i = 0; i < 64; i++)
			{
				writeTask = channel.WriteAsync (buf.AsMemory (0, 0x3FFFFFF));
				Assert.True (writeTask.IsCompleted);
				Assert.False (skipTask.IsCompleted);
			}

			for (int i = 0; i < 80; i++)
			{
				buf[i] = (byte)(i + 120);
			}

			writeTask = channel.WriteAsync (buf.AsMemory (0, 80));
			Thread.Sleep (50);
			Assert.True (skipTask.IsCompleted);
			Assert.Equal (0x100000000L, skipTask.Result);
			Assert.False (writeTask.IsCompleted);
			var ensureTask = channel.EnsureAvailableAsync (9);
			Assert.True (ensureTask.IsCompleted);
			Assert.Equal (9, channel.Count);
			Assert.Equal (184, channel.BufferMemory.Span[0]);
			Assert.Equal (185, channel.BufferMemory.Span[1]);
			Assert.Equal (186, channel.BufferMemory.Span[2]);
			Assert.Equal (187, channel.BufferMemory.Span[3]);
			Assert.Equal (188, channel.BufferMemory.Span[4]);
			Assert.Equal (189, channel.BufferMemory.Span[5]);
			Assert.Equal (190, channel.BufferMemory.Span[6]);
			Assert.Equal (191, channel.BufferMemory.Span[7]);
			Assert.Equal (192, channel.BufferMemory.Span[8]);
			Assert.False (writeTask.IsCompleted);
			channel.Skip (9);
			skipTask = channel.SkipWihoutBufferingAsync (7L);
			Assert.True (skipTask.IsCompleted);
			Assert.Equal (7L, skipTask.Result);
			Thread.Sleep (50);
			Assert.True (writeTask.IsCompleted);
			Assert.Equal (0, channel.Count);
		}

		[Theory]
		[Trait ("Category", "BufferedSource")]
		[InlineData (10, 2560000, 2560)]
		[InlineData (100, 2560, 25)]
		[InlineData (10, 2560, 2560000)]
		[InlineData (100, 25, 2560)]
		public void ConcurrentWriteRead (int chunkCount, int chunkSize, int bufferSize)
		{
			var src = new byte[chunkSize];
			for (int i = 0; i < chunkSize; i++)
			{
				src[i] = (byte)((i + 5) % 256);
			}

			var channel = new BufferedChannel (new byte[bufferSize]);
			var readTask = Task.Run<byte[]> (() =>
			{
				var dstData = new MemoryStream ();
				while (true)
				{
					var vTask = channel.LoadAsync ();
					if (!vTask.IsCompletedSuccessfully)
					{
						vTask.AsTask ().GetAwaiter ().GetResult ();
					}
					if (channel.Count < 1)
					{
						break;
					}

					dstData.Write (channel.BufferMemory.Span.Slice (channel.Offset, channel.Count));
					channel.Skip (channel.Count);
				}
				dstData.Seek (0L, SeekOrigin.Begin);
#pragma warning disable CA5350 // Do not use insecure cryptographic algorithm SHA1.
				var dstHasher = SHA1.Create ();
#pragma warning restore CA5350 // Do not use insecure cryptographic algorithm SHA1.
				Assert.True (dstData.TryGetBuffer (out ArraySegment<byte> buf2));
				return dstHasher.ComputeHash (buf2.Array, buf2.Offset, buf2.Count);
			});

			var srcData = new MemoryStream ();
			for (int cnt = 0; cnt < chunkCount; cnt++)
			{
				srcData.Write (src, 0, chunkSize);
				var vTask = channel.WriteAsync (src.AsMemory (0, chunkSize));
				if (!vTask.IsCompletedSuccessfully)
				{
					vTask.AsTask ().GetAwaiter ().GetResult ();
				}
			}

			channel.SetComplete ();
			readTask.Wait ();

			srcData.Seek (0L, SeekOrigin.Begin);
#pragma warning disable CA5350 // Do not use insecure cryptographic algorithm SHA1.
			var srcHasher = SHA1.Create ();
#pragma warning restore CA5350 // Do not use insecure cryptographic algorithm SHA1.
			Assert.True (srcData.TryGetBuffer (out ArraySegment<byte> buf));
			var srcHash = srcHasher.ComputeHash (buf.Array, buf.Offset, buf.Count);
			var dstHash = readTask.Result;
			Assert.Equal (BitConverter.ToUInt64 (srcHash, 0), BitConverter.ToUInt64 (dstHash, 0));
			Assert.Equal (BitConverter.ToUInt64 (srcHash, 8), BitConverter.ToUInt64 (dstHash, 8));
			Assert.Equal (BitConverter.ToUInt32 (srcHash, 16), BitConverter.ToUInt32 (dstHash, 16));
			srcHasher.Dispose ();
			srcData.Dispose ();
		}

		[Fact]
		[Trait ("Category", "BufferedSource")]
		public void ReadFirst ()
		{
			var channel = new BufferedChannel (new byte[] { 96, 95, 94, 93, 92, 91, 90 });
			var srcData = new byte[99];
			srcData[0] = 123;
			srcData[1] = 30;
			srcData[2] = 205;
			srcData[3] = 0;
			srcData[4] = 9;
			srcData[5] = 8;
			srcData[6] = 7;
			srcData[7] = 255;
			srcData[8] = 111;

			// места в хвосте буфера достаточно для всех предоставляемых данных
			var readTask = channel.LoadAsync ();
			Thread.Sleep (50);
			Assert.False (readTask.IsCompleted);
			var writeTask = channel.WriteAsync (srcData.AsMemory (0, 2));
			Assert.True (writeTask.IsCompleted); // ожидание не нужно, задача должна быть уже выполненной
			Thread.Sleep (50);
			Assert.True (readTask.IsCompleted);
			Assert.Equal (2, channel.Count);
			Assert.Equal (123, channel.BufferMemory.Span[0]);
			Assert.Equal (30, channel.BufferMemory.Span[1]);
			Assert.Equal (94, channel.BufferMemory.Span[2]);
			Assert.Equal (93, channel.BufferMemory.Span[3]);
			Assert.Equal (92, channel.BufferMemory.Span[4]);
			Assert.Equal (91, channel.BufferMemory.Span[5]);
			Assert.Equal (90, channel.BufferMemory.Span[6]);
			readTask = channel.LoadAsync ();
			Thread.Sleep (50);
			Assert.False (readTask.IsCompleted);
			writeTask = channel.WriteAsync (srcData.AsMemory (4, 5));
			Assert.True (writeTask.IsCompleted); // ожидание не нужно, задача должна быть уже выполненной
			Thread.Sleep (50);
			Assert.True (readTask.IsCompleted);
			Assert.Equal (7, channel.Count);
			Assert.Equal (123, channel.BufferMemory.Span[0]);
			Assert.Equal (30, channel.BufferMemory.Span[1]);
			Assert.Equal (9, channel.BufferMemory.Span[2]);
			Assert.Equal (8, channel.BufferMemory.Span[3]);
			Assert.Equal (7, channel.BufferMemory.Span[4]);
			Assert.Equal (255, channel.BufferMemory.Span[5]);
			Assert.Equal (111, channel.BufferMemory.Span[6]);

			// места в хвосте буфера нет
			channel.Skip (4); // 4 свободных байта окажутся в хвосте буфера после вызова ReserveTailSpace ()
			readTask = channel.LoadAsync ();
			Thread.Sleep (50);
			Assert.False (readTask.IsCompleted);
			writeTask = channel.WriteAsync (srcData.AsMemory (2, 2));
			Thread.Sleep (50);
			Assert.True (writeTask.IsCompleted);
			Assert.True (readTask.IsCompleted);
			Assert.Equal (5, channel.Count);
			Assert.Equal (7, channel.BufferMemory.Span[0]);
			Assert.Equal (255, channel.BufferMemory.Span[1]);
			Assert.Equal (111, channel.BufferMemory.Span[2]);
			Assert.Equal (205, channel.BufferMemory.Span[3]);
			Assert.Equal (0, channel.BufferMemory.Span[4]);

			// место в хвосте буфера есть но не хватает
			channel.Skip (3);
			readTask = channel.LoadAsync ();
			Thread.Sleep (50);
			Assert.False (readTask.IsCompleted);
			writeTask = channel.WriteAsync (srcData.AsMemory (0, 3));
			Thread.Sleep (50);
			Assert.True (writeTask.IsCompleted);
			Assert.True (readTask.IsCompleted);
			Assert.Equal (5, channel.Count);
			Assert.Equal (205, channel.BufferMemory.Span[0]);
			Assert.Equal (0, channel.BufferMemory.Span[1]);
			Assert.Equal (123, channel.BufferMemory.Span[2]);
			Assert.Equal (30, channel.BufferMemory.Span[3]);
			Assert.Equal (205, channel.BufferMemory.Span[4]);

			// завершение, которое должно разблокировать ожидание новых данных
			readTask = channel.LoadAsync ();
			Thread.Sleep (50);
			Assert.False (readTask.IsCompleted);
			channel.SetComplete ();
			Thread.Sleep (50);
			Assert.True (readTask.IsCompleted);
			readTask = channel.LoadAsync ();
			Assert.True (readTask.IsCompleted);
			channel.Skip (channel.Count);
			Assert.ThrowsAsync<NotEnoughDataException> (() => channel.EnsureAvailableAsync (1).AsTask ());
		}

		[Fact]
		[Trait ("Category", "BufferedSource")]
		public void WriteFirst ()
		{
			var channel = new BufferedChannel (new byte[] { 96, 95, 94, 93, 92, 91, 90 });
			var srcData = new byte[99];
			srcData[0] = 123;
			srcData[1] = 30;
			srcData[2] = 205;
			srcData[3] = 0;
			srcData[4] = 9;
			srcData[5] = 8;
			srcData[6] = 7;
			srcData[7] = 255;
			srcData[8] = 111;

			// пишем кусок меньше свободного хвоста буфера
			var writeTask = channel.WriteAsync (srcData.AsMemory (6, 1));
			Assert.True (writeTask.IsCompleted); // ожидание не нужно, задача должна быть уже выполненной
			var readTask = channel.LoadAsync ();
			Thread.Sleep (50);
			Assert.True (readTask.IsCompleted);
			Assert.Equal (1, channel.Count);
			Assert.Equal (7, channel.BufferMemory.Span[0]);
			Assert.Equal (95, channel.BufferMemory.Span[1]);
			Assert.Equal (94, channel.BufferMemory.Span[2]);
			Assert.Equal (93, channel.BufferMemory.Span[3]);
			Assert.Equal (92, channel.BufferMemory.Span[4]);
			Assert.Equal (91, channel.BufferMemory.Span[5]);
			Assert.Equal (90, channel.BufferMemory.Span[6]);
			writeTask = channel.WriteAsync (srcData.AsMemory (3, 6));
			Assert.True (writeTask.IsCompleted); // ожидание не нужно, задача должна быть уже выполненной
			readTask = channel.LoadAsync ();
			Thread.Sleep (50);
			Assert.True (readTask.IsCompleted);
			Assert.Equal (7, channel.Count);
			Assert.Equal (7, channel.BufferMemory.Span[0]);
			Assert.Equal (0, channel.BufferMemory.Span[1]);
			Assert.Equal (9, channel.BufferMemory.Span[2]);
			Assert.Equal (8, channel.BufferMemory.Span[3]);
			Assert.Equal (7, channel.BufferMemory.Span[4]);
			Assert.Equal (255, channel.BufferMemory.Span[5]);
			Assert.Equal (111, channel.BufferMemory.Span[6]);

			// пишем когда нет свободного места в хвосте буфера
			// data.AcceptTail (4); // в хвосте нет свободных байтов
			channel.Skip (5);
			writeTask = channel.WriteAsync (srcData.AsMemory (3, 3));
			Thread.Sleep (50);
			Assert.False (writeTask.IsCompleted);
			readTask = channel.LoadAsync ();
			Thread.Sleep (50);
			Assert.True (writeTask.IsCompleted);
			Assert.True (readTask.IsCompleted);
			Assert.Equal (5, channel.Count);
			Assert.Equal (255, channel.BufferMemory.Span[0]);
			Assert.Equal (111, channel.BufferMemory.Span[1]);
			Assert.Equal (0, channel.BufferMemory.Span[2]);
			Assert.Equal (9, channel.BufferMemory.Span[3]);
			Assert.Equal (8, channel.BufferMemory.Span[4]);

			// пишем когда свободное места в хвосте буфера есть, но недостаточно под все данные
			channel.Skip (2);
			writeTask = channel.WriteAsync (srcData.AsMemory (0, 3));
			Thread.Sleep (50);
			Assert.False (writeTask.IsCompleted);
			readTask = channel.LoadAsync ();
			Thread.Sleep (50);
			Assert.True (writeTask.IsCompleted);
			Assert.True (readTask.IsCompleted);
			Assert.Equal (6, channel.Count);
			Assert.Equal (0, channel.BufferMemory.Span[0]);
			Assert.Equal (9, channel.BufferMemory.Span[1]);
			Assert.Equal (8, channel.BufferMemory.Span[2]);
			Assert.Equal (123, channel.BufferMemory.Span[3]);
			Assert.Equal (30, channel.BufferMemory.Span[4]);
			Assert.Equal (205, channel.BufferMemory.Span[5]);

			// завершение, после которого не должны приниматься новые данные
			channel.SetComplete ();
			Assert.ThrowsAsync<InvalidOperationException> (() => channel.WriteAsync (srcData.AsMemory (0, 3)).AsTask ());
			Assert.ThrowsAsync<InvalidOperationException> (() => channel.WriteAsync (srcData.AsMemory (1, 1)).AsTask ());
		}
	}
}
