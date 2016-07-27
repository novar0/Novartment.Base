using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net.Test
{
	internal class StringWritingStream : IBinaryDestination
	{
		private readonly Queue<string> _queue = new Queue<string> ();
		internal Queue<string> Queue => _queue;

		public Task WriteAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			var str = Encoding.UTF8.GetString (buffer, offset, count);
			_queue.Enqueue (str);
			return Task.CompletedTask;
		}

		public void SetComplete ()
		{
		}
	}

	internal class TcpConnectionMock : ITcpConnection
	{
		private readonly IPHostEndPoint _localEndpoint;
		private readonly IPHostEndPoint _remoteEndpoint;
		private readonly IBufferedSource _inData;
		private readonly StringWritingStream _outData = new StringWritingStream ();
		private readonly Stopwatch _stopwatch = Stopwatch.StartNew ();
		private readonly ManualResetEvent _disposedEvent = new ManualResetEvent (false);
		private bool _disposed = false;
		private long _lastActivity;

		internal StringWritingStream OutData => _outData;
		internal bool IsDisposed => _disposed;
		internal EventWaitHandle DisposedEvent => _disposedEvent;

		internal TcpConnectionMock (
			IPHostEndPoint localEndpoint,
			IPHostEndPoint remoteEndpoint,
			byte[] inData) : this (localEndpoint, remoteEndpoint, inData, inData.Length)
		{
		}

		internal TcpConnectionMock (
			IPHostEndPoint localEndpoint,
			IPHostEndPoint remoteEndpoint,
			byte[] inData, int count)
		{
			_localEndpoint = localEndpoint;
			_remoteEndpoint = remoteEndpoint;
			_inData = new ArrayBufferedSource (inData, 0, count);
		}


		internal TcpConnectionMock (
			IPHostEndPoint localEndpoint,
			IPHostEndPoint remoteEndpoint,
			IBufferedSource inData)
		{
			_localEndpoint = localEndpoint;
			_remoteEndpoint = remoteEndpoint;
			_inData = inData;
		}

		public string RemoteHostName => null;

		public IPHostEndPoint LocalEndPoint => _localEndpoint;

		public IPHostEndPoint RemoteEndPoint => _remoteEndpoint;

		public IBufferedSource Reader
		{
			get
			{
				if (_disposed) throw new InvalidOperationException ();
				return _inData;
			}
		}

		public IBinaryDestination Writer
		{
			get
			{
				if (_disposed) throw new InvalidOperationException ();
				return _outData;
			}
		}

		public TimeSpan Duration => _stopwatch.Elapsed;

		public TimeSpan IdleDuration => TimeSpan.FromTicks (_stopwatch.ElapsedTicks - _lastActivity);

		public bool IsAuthenticated => false;

		public bool IsEncrypted => false;

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
