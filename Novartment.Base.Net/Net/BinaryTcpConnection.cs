using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net
{
	/// <summary>
	/// Установленное TCP-подключение, отслеживающее полное время и время простоя.
	/// </summary>
	[SuppressMessage (
		"Microsoft.Design",
		"CA1063:ImplementIDisposableCorrectly",
		Justification = "Implemented correctly.")]
	public class BinaryTcpConnection :
		ITcpConnection,
		IDisposable
	{
		private readonly Stopwatch _stopwatch = Stopwatch.StartNew ();
		private readonly _Reporter _reporter;

		private long _lastActivity = 0L;

		/// <summary>
		/// Получает источник входящих через подключение данных.
		/// </summary>
		public IBufferedSource Reader { get; }

		/// <summary>
		/// Получает получатель двоичных данных для записи исходящих через подключение данных.
		/// </summary>
		public IBinaryDestination Writer { get; }

		/// <summary>
		/// Получает атрибуты локальной конечной точки подключения.
		/// </summary>
		public IPHostEndPoint LocalEndPoint { get; }

		/// <summary>
		/// Получает атрибуты удалённой конечной точки подключения.
		/// </summary>
		public IPHostEndPoint RemoteEndPoint { get; }

		/// <summary>
		/// Получает промежуток времени, прошедший с момента установки подключения.
		/// </summary>
		public TimeSpan Duration => _stopwatch.Elapsed;

		/// <summary>
		/// Получает промежуток времени, прошедший с момента последнего получения входящих данных подключения.
		/// </summary>
		public TimeSpan IdleDuration
		{
			get
			{
				return TimeSpan.FromTicks (_stopwatch.ElapsedTicks - Interlocked.Read (ref _lastActivity));
			}
		}

		/// <summary>
		/// Инициализирует новый экземпляр класса TcpConnection с указанными параметрами.
		/// </summary>
		/// <param name="localEndPoint">Атрибуты локальной конечной точки подключения.</param>
		/// <param name="remoteEndPoint">Атрибуты удалённой конечной точки подключения.</param>
		/// <param name="reader">Источник данных чтения.</param>
		/// <param name="writer">Получатель данных записи.</param>
		public BinaryTcpConnection (
			IPHostEndPoint localEndPoint,
			IPHostEndPoint remoteEndPoint,
			IBufferedSource reader,
			IBinaryDestination writer)
		{
			if (localEndPoint == null)
			{
				throw new ArgumentNullException (nameof (localEndPoint));
			}
			if (remoteEndPoint == null)
			{
				throw new ArgumentNullException (nameof (remoteEndPoint));
			}
			if (reader == null)
			{
				throw new ArgumentNullException (nameof (reader));
			}
			if (writer == null)
			{
				throw new ArgumentNullException (nameof (writer));
			}
			Contract.EndContractBlock ();

			this.LocalEndPoint = localEndPoint;
			this.RemoteEndPoint = remoteEndPoint;
			_reporter = new _Reporter (this);
			this.Reader = new ObservableBufferedSource (reader, _reporter);
			this.Writer = writer;
		}

		/// <summary>
		/// Освобождает все используемые ресурсы.
		/// </summary>
		[SuppressMessage ("Microsoft.Usage",
			"CA1816:CallGCSuppressFinalizeCorrectly",
			Justification = "There is no meaning to introduce a finalizer in derived type."),
		SuppressMessage (
			"Microsoft.Design",
			"CA1063:ImplementIDisposableCorrectly",
			Justification = "Implemented correctly.")]
		public void Dispose ()
		{
			_reporter.Dispose ();
			_stopwatch.Stop ();
			this.Writer.SetComplete ();
		}

		private class _Reporter :
			IProgress<long>,
			IDisposable
		{
			private BinaryTcpConnection _parent;

			internal _Reporter (BinaryTcpConnection parent)
			{
				_parent = parent;
			}

			public void Report (long value)
			{
				var parent = _parent;
				if (parent != null)
				{
					Interlocked.Exchange (ref parent._lastActivity, parent._stopwatch.ElapsedTicks);
				}
			}

			public void Dispose ()
			{
				_parent = null;
			}
		}
	}
}
