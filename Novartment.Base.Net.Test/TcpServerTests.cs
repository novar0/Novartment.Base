using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Xunit;

namespace Novartment.Base.Net.Test
{
	public class TcpServerTests
	{
		[Fact]
		[Trait ("Category", "Net")]
		public void ProtocolCalledOnConnect ()
		{
			var listeners = new Stack<TcpListenerMock> ();
			var listenerFactory = new Func<IPEndPoint, ITcpListener> (endpoint =>
				{
					var newListener = new TcpListenerMock (endpoint);
					listeners.Push (newListener);
					return newListener;
				});
			var protocol1 = new TcpConnectionProtocolMock ();
			var protocol2 = new TcpConnectionProtocolMock ();

			var srv = new TcpServer (listenerFactory, null);
			var protocol1localEndPoint = new IPEndPoint (new IPAddress (9087423L), 2554);
			var protocol2localEndPoint = new IPEndPoint (new IPAddress (522209L), 19720);
			var protocol1remoteEndPoint1 = new IPEndPoint (new IPAddress (1144955L), 32701);
			var protocol1remoteEndPoint2 = new IPEndPoint (new IPAddress (38755L), 25);
			var protocol2remoteEndPoint = new IPEndPoint (new IPAddress (3124777L), 8008);

			// на каждую добавленную точку прослушивания должен быть создан запущенный прослушиватель на указанный адрес/порт
			Assert.Equal (0, listeners.Count);
			srv.AddListenEndpoint (protocol1localEndPoint, protocol1);
			Assert.Equal (1, listeners.Count);
			var protocol1listener = listeners.Pop ();
			Assert.Equal (protocol1localEndPoint.Address, protocol1listener.LocalEndpoint.Address);
			Assert.Equal (protocol1localEndPoint.Port, protocol1listener.LocalEndpoint.Port);
			Assert.True (protocol1listener.IsStarted);

			srv.AddListenEndpoint (protocol2localEndPoint, protocol2);
			Assert.Equal (1, listeners.Count);
			var protocol2listener = listeners.Pop ();
			Assert.Equal (protocol2localEndPoint.Address, protocol2listener.LocalEndpoint.Address);
			Assert.Equal (protocol2localEndPoint.Port, protocol2listener.LocalEndpoint.Port);
			Assert.True (protocol2listener.IsStarted);

			// на каждое установленное соединение должен быть вызыван обработчик
			Assert.Equal (0, protocol1.Connections.Count);
			protocol1listener.SimulateIncomingConnection (protocol1remoteEndPoint1);
			protocol1.StartedEvent.WaitOne (); // ждём пока в TcpServer создаётся объект соединения и обработчика
			Assert.Equal (1, protocol1.Connections.Count);
			var protocol1connection1 = (TcpConnectionMock)protocol1.Connections[0];
			Assert.False (protocol1connection1.IsDisposed);
			Assert.Equal (protocol1localEndPoint.Address, protocol1connection1.LocalEndPoint.Address);
			Assert.Equal (protocol1localEndPoint.Port, protocol1connection1.LocalEndPoint.Port);
			Assert.Equal (protocol1remoteEndPoint1.Address, protocol1connection1.RemoteEndPoint.Address);
			Assert.Equal (protocol1remoteEndPoint1.Port, protocol1connection1.RemoteEndPoint.Port);

			Assert.Equal (0, protocol2.Connections.Count);
			protocol2listener.SimulateIncomingConnection (protocol2remoteEndPoint);
			protocol2.StartedEvent.WaitOne (); // ждём пока в TcpServer создаётся объект соединения и обработчика
			Assert.Equal (1, protocol2.Connections.Count);
			var protocol2connection = (TcpConnectionMock)protocol2.Connections[0];
			Assert.False (protocol2connection.IsDisposed);
			Assert.Equal (protocol2localEndPoint.Address, protocol2connection.LocalEndPoint.Address);
			Assert.Equal (protocol2localEndPoint.Port, protocol2connection.LocalEndPoint.Port);
			Assert.Equal (protocol2remoteEndPoint.Address, protocol2connection.RemoteEndPoint.Address);
			Assert.Equal (protocol2remoteEndPoint.Port, protocol2connection.RemoteEndPoint.Port);

			protocol1listener.SimulateIncomingConnection (protocol1remoteEndPoint2);
			protocol1.StartedEvent.WaitOne (); // ждём пока в TcpServer создаётся объект соединения и обработчика
			Assert.Equal (2, protocol1.Connections.Count);
			var protocol1connection2 = (TcpConnectionMock)protocol1.Connections[1];
			Assert.False (protocol1connection2.IsDisposed);
			Assert.Equal (protocol1localEndPoint.Address, protocol1connection2.LocalEndPoint.Address);
			Assert.Equal (protocol1localEndPoint.Port, protocol1connection2.LocalEndPoint.Port);
			Assert.Equal (protocol1remoteEndPoint2.Address, protocol1connection2.RemoteEndPoint.Address);
			Assert.Equal (protocol1remoteEndPoint2.Port, protocol1connection2.RemoteEndPoint.Port);

			// после завершения обработки соединение должно быть закрыто/освобождено
			protocol1.FinishHandlingConnection (protocol1remoteEndPoint2);
			protocol1connection2.DisposedEvent.WaitOne ();
			Assert.False (protocol1connection1.IsDisposed);
			Assert.True (protocol1connection2.IsDisposed);
			Assert.False (protocol2connection.IsDisposed);

			protocol2.FinishHandlingConnection (protocol2remoteEndPoint);
			protocol2connection.DisposedEvent.WaitOne ();
			Assert.False (protocol1connection1.IsDisposed);
			Assert.True (protocol1connection2.IsDisposed);
			Assert.True (protocol2connection.IsDisposed);

			// после остановки сервера прослушиватели должны быть остановлены.
			// обработка первого соединения ещё не завершилась и должна быть отменена
			srv.StopAsync (true).GetAwaiter ().GetResult ();
			protocol1connection1.DisposedEvent.WaitOne ();
			Assert.True (protocol1connection1.IsDisposed);
			WaitHandle.WaitAll (new WaitHandle[] { protocol1listener.StopedEvent, protocol2listener.StopedEvent });
			Assert.False (protocol2listener.IsStarted);
			Assert.False (protocol1listener.IsStarted);
		}

		[Fact]
		[Trait ("Category", "Net")]
		public void IdleTimeoutExpiration ()
		{
			TcpListenerMock listener = null;
			var listenerFactory = new Func<IPEndPoint, ITcpListener> (endpoint =>
			{
				listener = new TcpListenerMock (endpoint);
				return listener;
			});
			var protocol = new TcpConnectionProtocolMock ();

			var srv = new TcpServer (listenerFactory, null);
			srv.ConnectionIdleTimeout = TimeSpan.FromMilliseconds (200.0);
			srv.AddListenEndpoint (new IPEndPoint (new IPAddress (9087423L), 2554), protocol);

			// отключение по простою
			listener.SimulateIncomingConnection (new IPEndPoint (new IPAddress (1144955L), 32701));
			protocol.StartedEvent.WaitOne (); // ждём пока в TcpServer создаётся объект соединения и обработчика
			var connection = (TcpConnectionMock)protocol.Connections[0];
			connection.UpdateActivity ();
			Thread.Sleep ((int)(srv.ConnectionIdleTimeout.TotalMilliseconds / 2.0));
			Assert.False (connection.IsDisposed);
			// тут должен сработать таймаут простоя
			Thread.Sleep ((int)(srv.ConnectionIdleTimeout.TotalMilliseconds * 3.0));
			Assert.True (connection.IsDisposed);

			srv.StopAsync (true);
		}

		[Fact, Trait ("Category", "Net")]
		public void TotalTimeoutExpiration ()
		{
			TcpListenerMock listener = null;
			var listenerFactory = new Func<IPEndPoint, ITcpListener> (endpoint =>
			{
				listener = new TcpListenerMock (endpoint);
				return listener;
			});
			var protocol = new TcpConnectionProtocolMock ();

			var srv = new TcpServer (listenerFactory, null);
			srv.ConnectionTimeout = TimeSpan.FromMilliseconds (500.0);
			srv.AddListenEndpoint (new IPEndPoint (new IPAddress (9087423L), 2554), protocol);

			// отключение по полному времени
			var updateInterval = (int)(srv.ConnectionTimeout.TotalMilliseconds / 10.0);
			listener.SimulateIncomingConnection (new IPEndPoint (new IPAddress (1144955L), 32701));
			protocol.StartedEvent.WaitOne (); // ждём пока в TcpServer создаётся объект соединения и обработчика
			var connection = (TcpConnectionMock)protocol.Connections[0];
			connection.UpdateActivity ();

			Thread.Sleep (updateInterval);
			Assert.False (connection.IsDisposed);
			// тут должен сработать таймаут полного времени коннекта
			for (int i = 0; i < 10; i++)
			{
				connection.UpdateActivity ();
				Thread.Sleep (updateInterval);
			}
			Assert.True (connection.IsDisposed);

			srv.StopAsync (true);
		}
	}
}
