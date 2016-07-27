using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Novartment.Base.Net;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Smtp.Test
{
	internal class StringWritingStream : IBinaryDestination
	{
		private readonly Queue<string> _queue = new Queue<string> ();
		internal Queue<string> Queue => _queue;

		public Task WriteAsync (byte[] buffer, int offset, int count, CancellationToken cancellationToken)
		{
			var str = Encoding.ASCII.GetString (buffer, offset, count);
			_queue.Enqueue (str);
			return Task.CompletedTask;
		}

		public void SetComplete () { }
	}

	internal class TcpConnectionMock : ITcpConnection
	{
		private readonly IPHostEndPoint _localEndpoint;
		private readonly IPHostEndPoint _remoteEndpoint;
		private readonly IBufferedSource _inData;
		private readonly StringWritingStream _outData = new StringWritingStream ();
		private readonly Stopwatch _stopwatch = Stopwatch.StartNew ();
		private bool _disposed = false;
		private long _lastActivity;

		internal StringWritingStream OutData => _outData;
		internal bool IsDisposed => _disposed;

		internal TcpConnectionMock (
			IPEndPoint localEndpoint,
			IPEndPoint remoteEndpoint,
			byte[] inData)
			: this (localEndpoint, remoteEndpoint, inData, inData.Length)
		{
		}

		internal TcpConnectionMock (
			IPEndPoint localEndpoint,
			IPEndPoint remoteEndpoint,
			byte[] inData, int count)
		{
			_localEndpoint = new IPHostEndPoint (localEndpoint);
			_remoteEndpoint = new IPHostEndPoint (remoteEndpoint);
			_inData = new ArrayBufferedSource (inData, 0, count);
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
		}
	}
}
