using System;
using System.Net;
using System.Threading;
using Xunit;

namespace Novartment.Base.Net.Test
{
	public class TcpConnectionTests
	{
		[Fact]
		[Trait ("Category", "Net")]
		public void DurationCalculation ()
		{
			var point1 = new IPHostEndPoint (IPAddress.Loopback, 2123) { HostName = "Host 101" };
			var point2 = new IPHostEndPoint (IPAddress.Any, 954) { HostName = "Host 2222" };
			var connection = new BinaryTcpConnection (
				point1,
				point2,
				new EndlessSource (),
				new NullDestination ());

			// начальная продолжительность не менее нуля
			var totalDur = connection.Duration.TotalMilliseconds;
			var idleDur = connection.IdleDuration.TotalMilliseconds;
			Assert.InRange (totalDur, 0.0, double.MaxValue);
			Assert.InRange (idleDur, 0.0, double.MaxValue);

			// после паузы продолжительность увеличилась
			Thread.Sleep (100);
			Assert.InRange (connection.Duration.TotalMilliseconds, totalDur, double.MaxValue);
			Assert.InRange (connection.IdleDuration.TotalMilliseconds, idleDur, double.MaxValue);

			// пока не было активности, разница в продолжительностях невелика
			var delta1 = connection.Duration.TotalMilliseconds - connection.IdleDuration.TotalMilliseconds;
			Assert.InRange (Math.Abs (delta1), 0.0, 100.0); // TODO: иногда тут 187

			// после активности разница в продолжительностях стала большой
			connection.Reader.EnsureAvailableAsync (10);
			connection.Reader.Skip (5);
			var delta2 = connection.Duration.TotalMilliseconds - connection.IdleDuration.TotalMilliseconds;
			Assert.InRange (Math.Abs (delta2), 100.0, double.MaxValue); // TODO: иногда тут 94

			connection.Dispose ();
		}
	}
}
