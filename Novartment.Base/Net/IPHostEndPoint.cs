using System;
using System.Net;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Net
{
	/// <summary>
	/// Конечная точка подключения узла.
	/// </summary>
	public class IPHostEndPoint : IPEndPoint
	{
		/// <summary>
		/// Получает или устанавливает имя узла.
		/// </summary>
		public string HostName { get; set; }

		/// <summary>
		/// Инициализирует новый экземпляр IPHostEndPoint с указанной конечной точкой подключения.
		/// </summary>
		/// <param name="endPoint">Конечная точка подключения.</param>
		public IPHostEndPoint (IPEndPoint endPoint)
			: base (GetIPEndPointAddress (endPoint), GetIPEndPointPort (endPoint))
		{
		}

		/// <summary>
		/// Инициализирует новый экземпляр IPHostEndPoint с указанными атрибутами.
		/// </summary>
		/// <param name="address">Адрес.</param>
		/// <param name="port">Порт.</param>
		public IPHostEndPoint (IPAddress address, int port)
			: base (address, port)
		{
		}

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
