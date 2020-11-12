using System;
using System.Diagnostics.Contracts;
using System.Net;

namespace Novartment.Base.Net
{
	/// <summary>
	/// Represents a network endpoint as an IP address, a port number and a host name.
	/// </summary>
	public sealed class IPHostEndPoint : IPEndPoint
	{
		/// <summary>
		/// Initializes a new instance of the AddrSpec class based on the specified IPEndPoint.
		/// </summary>
		/// <param name="endPoint">Конечная точка подключения.</param>
		public IPHostEndPoint (IPEndPoint endPoint)
			: base (GetIPEndPointAddress (endPoint), GetIPEndPointPort (endPoint))
		{
		}

		/// <summary>
		/// Initializes a new instance of the AddrSpec class
		/// with the specified IP address and the port number.
		/// </summary>
		/// <param name="address">The IP address.</param>
		/// <param name="port">The port number.</param>
		public IPHostEndPoint (IPAddress address, int port)
			: base (address, port)
		{
		}

		/// <summary>
		/// Gets or sets the host name.
		/// </summary>
		public string HostName { get; set; }

		private static IPAddress GetIPEndPointAddress (IPEndPoint endPoint)
		{
			if (endPoint == null)
			{
				throw new ArgumentNullException (nameof (endPoint));
			}

			Contract.EndContractBlock ();

			return endPoint.Address;
		}

		private static int GetIPEndPointPort (IPEndPoint endPoint)
		{
			if (endPoint == null)
			{
				throw new ArgumentNullException (nameof (endPoint));
			}

			Contract.EndContractBlock ();

			return endPoint.Port;
		}
	}
}
