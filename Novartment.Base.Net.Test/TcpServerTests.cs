using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Xunit;

namespace Novartment.Base.Net.Test
{
	public class TcpServerTests
	{
		[Fact, Trait ("Category", "Net")]
		public void ProtocolCalledOnConnect ()
		{
			var listeners = new List<TcpListenerMock> ();
			var listenerFactory = new Func<IPEndPoint, ITcpListener> (endpoint =>
				{
					var newListener = new TcpListenerMock (endpoint);
					listeners.Add (newListener);
					return newListener;
				});
			var protocol1 = new TcpConnectionProtocolMock ();
			var protocol2 = new TcpConnectionProtocolMock ();

			var srv = new TcpServer (listenerFactory, null);
			var localAddr1 = new IPAddress (9087423L);
			var localPort1 = 2554;
			var localAddr2 = new IPAddress (522209L);
			var localPort2 = 19720;
			var remoteAddr1 = new IPAddress (1144955L);
			var remotePort1 = 32701;
			var remoteAddr2 = new IPAddress (38755L);
			var remotePort2 = 25;
			var remoteAddr3 = new IPAddress (3124777L);
			var remotePort3 = 8008;
			var remoteEndPoint1 = new IPEndPoint (remoteAddr1, remotePort1);
			var remoteEndPoint2 = new IPEndPoint (remoteAddr2, remotePort2);
			var remoteEndPoint3 = new IPEndPoint (remoteAddr3, remotePort3);

			// на каждую добавленную точку прослушивания должен быть создан запущенный прослушиватель на указанный адрес/порт
			Assert.Equal (0, listeners.Count);
			srv.AddListenEndpoint (new IPEndPoint (localAddr1, localPort1), protocol1);
			Assert.Equal (1, listeners.Count);
			var listener1 = listeners[0];
			Assert.Equal (localAddr1, listener1.LocalEndpoint.Address);
			Assert.Equal (localPort1, listener1.LocalEndpoint.Port);
			Assert.True (listener1.IsStarted);

			srv.AddListenEndpoint (new IPEndPoint (localAddr2, localPort2), protocol2);
			Assert.Equal (2, listeners.Count);
			var listener2 = listeners[1];
			Assert.Equal (localAddr2, listener2.LocalEndpoint.Address);
			Assert.Equal (localPort2, listener2.LocalEndpoint.Port);
			Assert.True (listener2.IsStarted);

			// на каждое установленное соединение должен быть вызыван обработчик
			Assert.Equal (0, protocol1.Connections.Count);
			listener1.SimulateIncomingConnection (remoteEndPoint1);
			protocol1.StartedEvent.WaitOne (); // ждём пока в TcpServer создаётся объект соединения и обработчика
			Assert.Equal (1, protocol1.Connections.Count);
			var listener1connection1 = (TcpConnectionMock)protocol1.Connections[0];
			Assert.False (listener1connection1.IsDisposed);
			Assert.Equal (localAddr1, listener1connection1.LocalEndPoint.Address);
			Assert.Equal (localPort1, listener1connection1.LocalEndPoint.Port);
			Assert.Equal (remoteAddr1, listener1connection1.RemoteEndPoint.Address);
			Assert.Equal (remotePort1, listener1connection1.RemoteEndPoint.Port);

			Assert.Equal (0, protocol2.Connections.Count);
			listener2.SimulateIncomingConnection (remoteEndPoint3);
			protocol2.StartedEvent.WaitOne (); // ждём пока в TcpServer создаётся объект соединения и обработчика
			Assert.Equal (1, protocol2.Connections.Count);
			var listener2connection1 = (TcpConnectionMock)protocol2.Connections[0];
			Assert.False (listener2connection1.IsDisposed);
			Assert.Equal (localAddr2, listener2connection1.LocalEndPoint.Address);
			Assert.Equal (localPort2, listener2connection1.LocalEndPoint.Port);
			Assert.Equal (remoteAddr3, listener2connection1.RemoteEndPoint.Address);
			Assert.Equal (remotePort3, listener2connection1.RemoteEndPoint.Port);

			listener1.SimulateIncomingConnection (remoteEndPoint2);
			protocol1.StartedEvent.WaitOne (); // ждём пока в TcpServer создаётся объект соединения и обработчика
			Assert.Equal (2, protocol1.Connections.Count);
			var listener1connection2 = (TcpConnectionMock)protocol1.Connections[1];
			Assert.False (listener1connection2.IsDisposed);
			Assert.Equal (localAddr1, listener1connection2.LocalEndPoint.Address);
			Assert.Equal (localPort1, listener1connection2.LocalEndPoint.Port);
			Assert.Equal (remoteAddr2, listener1connection2.RemoteEndPoint.Address);
			Assert.Equal (remotePort2, listener1connection2.RemoteEndPoint.Port);

			// после завершения обработки соединение должно быть закрыто/освобождено
			protocol1.FinishHandlingConnection (remoteEndPoint2);
			listener1connection2.DisposedEvent.WaitOne ();
			Assert.False (listener1connection1.IsDisposed);
			Assert.False (listener2connection1.IsDisposed);

			protocol2.FinishHandlingConnection (remoteEndPoint3);
			listener2connection1.DisposedEvent.WaitOne ();
			Assert.False (listener1connection1.IsDisposed);
			Assert.True (listener1connection2.IsDisposed);
			Assert.True (listener2connection1.IsDisposed);

			// после остановки сервера прослушиватели должны быть остановлены.
			// обработка первого соединения ещё не завершилась и должна быть отменена
			srv.StopAsync (true).Wait ();
			listener1connection1.DisposedEvent.WaitOne ();
			Assert.True (listener1connection1.IsDisposed);
			WaitHandle.WaitAll (new WaitHandle[] { listener1.StopedEvent, listener2.StopedEvent });
			Assert.False (listener2.IsStarted);
			Assert.False (listener1.IsStarted);
		}

		[Fact, Trait ("Category", "Net")]
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
