using System;
using System.Diagnostics;
using System.Threading;
using Novartment.Base.BinaryStreaming;

namespace Novartment.Base.Net
{
	/// <summary>
	/// Установленное TCP-подключение, отслеживающее полное время и время простоя.
	/// </summary>
	public class BinaryTcpConnection :
		ITcpConnection,
		IDisposable
	{
		private readonly Stopwatch _stopwatch = Stopwatch.StartNew ();
		private readonly Reporter _reporter;

		private long _lastActivity = 0L;

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
			if (reader == null)
			{
				throw new ArgumentNullException (nameof (reader));
			}

			if (writer == null)
			{
				throw new ArgumentNullException (nameof (writer));
			}

			this.LocalEndPoint = localEndPoint ?? throw new ArgumentNullException (nameof (localEndPoint));
			this.RemoteEndPoint = remoteEndPoint ?? throw new ArgumentNullException (nameof (remoteEndPoint));
			_reporter = new Reporter (this);
			this.Reader = new ObservableBufferedSource (reader, _reporter);
			this.Writer = writer;
		}

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
		public TimeSpan IdleDuration => TimeSpan.FromSeconds (
			(_stopwatch.ElapsedTicks - Interlocked.Read (ref _lastActivity)) / (double)Stopwatch.Frequency);

		/// <summary>
		/// Освобождает все используемые ресурсы.
		/// </summary>
		public void Dispose ()
		{
			OnDisposing ();
			_reporter.Dispose ();
			_stopwatch.Stop ();
			this.Writer.SetComplete ();
			GC.SuppressFinalize (this);
		}

		/// <summary>
		/// Вызывается в унаследованных классах перед освобождением всех ресурсов базового объекта.
		/// </summary>
		protected virtual void OnDisposing ()
		{
		}

		private sealed class Reporter :
			IProgress<long>,
			IDisposable
		{
			private BinaryTcpConnection _parent;

			internal Reporter (BinaryTcpConnection parent)
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
