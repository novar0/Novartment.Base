using System;
using System.Diagnostics;
using System.Net;
using Novartment.Base.BinaryStreaming;
using Novartment.Base.Net;

namespace Novartment.Base.Smtp.Test
{
	internal class TcpConnectionMock : ITcpConnection
	{
		private readonly IPHostEndPoint _localEndpoint;
		private readonly IPHostEndPoint _remoteEndpoint;
		private readonly IBufferedSource _inData;
		private readonly StringWritingStream _outData = new StringWritingStream ();
		private readonly Stopwatch _stopwatch = Stopwatch.StartNew ();
		private bool _disposed = false;
		private long _lastActivity;

		internal TcpConnectionMock (
			IPEndPoint localEndpoint,
			IPEndPoint remoteEndpoint,
			Memory<byte> inData)
		{
			_localEndpoint = new IPHostEndPoint (localEndpoint);
			_remoteEndpoint = new IPHostEndPoint (remoteEndpoint);
			_inData = new ArrayBufferedSource (inData);
		}

		internal TcpConnectionMock (
			IPEndPoint localEndpoint,
			IPEndPoint remoteEndpoint,
			IBufferedSource inData)
		{
			_localEndpoint = new IPHostEndPoint (localEndpoint) { HostName = "test.domain.net" };
			_remoteEndpoint = new IPHostEndPoint (remoteEndpoint);
			_inData = inData;
		}

		public string RemoteHostName => null;

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

		public bool IsAuthenticated => false;

		public bool IsEncrypted => false;

		internal StringWritingStream OutData => _outData;

		internal bool IsDisposed => _disposed;

		public void UpdateActivity ()
		{
			_lastActivity = _stopwatch.ElapsedMilliseconds;
		}

		public void Dispose ()
		{
			_disposed = true;
		}
	}
}
