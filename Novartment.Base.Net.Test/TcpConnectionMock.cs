using System;
using System.Diagnostics;
using System.Threading;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net.Test
{
	internal sealed class TcpConnectionMock :
		ITcpConnection
	{
		private readonly IPHostEndPoint _localEndpoint;
		private readonly IPHostEndPoint _remoteEndpoint;
		private readonly IBufferedSource _inData;
		private readonly StringWritingStream _outData = new ();
		private readonly Stopwatch _stopwatch = Stopwatch.StartNew ();
		private readonly ManualResetEvent _disposedEvent = new (false);
		private bool _disposed = false;
		private long _lastActivity;

		internal TcpConnectionMock (
			IPHostEndPoint localEndpoint,
			IPHostEndPoint remoteEndpoint,
			ReadOnlyMemory<byte> inData)
		{
			_localEndpoint = localEndpoint;
			_remoteEndpoint = remoteEndpoint;
			_inData = new MemoryBufferedSource (inData);
		}

		public static string RemoteHostName => null;

		public IPHostEndPoint LocalEndPoint => _localEndpoint;

		public IPHostEndPoint RemoteEndPoint => _remoteEndpoint;

		public IBufferedSource Reader
		{
			get
			{
				if (_disposed)
				{
					throw new InvalidOperationException ();
				}

				return _inData;
			}
		}

		public IBinaryDestination Writer
		{
			get
			{
				if (_disposed)
				{
					throw new InvalidOperationException ();
				}

				return _outData;
			}
		}

		public TimeSpan Duration => _stopwatch.Elapsed;

		public TimeSpan IdleDuration => TimeSpan.FromTicks (_stopwatch.ElapsedTicks - _lastActivity);

		public static bool IsAuthenticated => false;

		public static bool IsEncrypted => false;

		internal StringWritingStream OutData => _outData;

		internal bool IsDisposed => _disposed;

		internal EventWaitHandle DisposedEvent => _disposedEvent;

		public void UpdateActivity ()
		{
			_lastActivity = _stopwatch.ElapsedMilliseconds;
		}

		public void Dispose ()
		{
			_disposed = true;
			_disposedEvent.Set ();
		}
	}
}
