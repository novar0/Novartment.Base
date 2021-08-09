using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using static System.Linq.Enumerable;

namespace Novartment.Base.Net
{
	/// <summary>
	/// Методы расширения для System.Net.IPAddress.
	/// </summary>
	public static class IPAddressExtensions
	{
		/// <summary>
		/// Получает последовательность адресов указанного типа, назначенный локальному узлу.
		/// </summary>
		/// <param name="family">Тип адресов, которые надо получить.</param>
		/// <returns>Последовательность адресов указанного типа, назначенный локальному узлу.</returns>
		public static IReadOnlyList<IPAddress> GetLocalhostNetworkAddresses (AddressFamily family)
		{
			var networkInterfaces = NetworkInterface
				.GetAllNetworkInterfaces ()
				.Where (item => item.Supports ((family == AddressFamily.InterNetworkV6) ? NetworkInterfaceComponent.IPv6 : NetworkInterfaceComponent.IPv4));
			return networkInterfaces
				.SelectMany (intf => intf
					.GetIPProperties ()
					.UnicastAddresses
					.Select (addressInfo => addressInfo.Address)
					.Where (item => item.AddressFamily == family)
					.Where (IsValidHostAddress))
					.ToArray ();
		}

		/// <summary>
		/// Проверяет является ли указанный объект обычным (не специальным) адресом.
		/// </summary>
		/// <param name="address">Адрес для проверки.</param>
		/// <returns>True если переданный адрес является обычным адресом.</returns>
		public static bool IsValidHostAddress (this IPAddress address)
		{
			if (address == null)
			{
				throw new ArgumentNullException (nameof (address));
			}

			return
				!IPAddress.IsLoopback (address) &&
				!address.Equals (IPAddress.Any) &&
				!address.Equals (IPAddress.Broadcast) &&
				!address.Equals (IPAddress.Loopback) &&
				!address.Equals (IPAddress.None);
		}

		/// <summary>
		/// Получает адрес казанного типа для узла с указанным именем.</summary>
		/// <param name="hostName">Имя узла, для которого необходимо получить адрес.</param>
		/// <param name="family">Необходимый тип адреса.</param>
		/// <returns>Задача, результатом которой будет найденный адрес.</returns>
		public static async Task<IPAddress> GetActualAddressAsync (string hostName, AddressFamily family)
		{
			if (hostName == null)
			{
				throw new ArgumentNullException (nameof (hostName));
			}

			try
			{
				var collection = await Dns.GetHostAddressesAsync (hostName).ConfigureAwait (false);
				var result = collection.FirstOrDefault (address => (address.AddressFamily == family) && address.IsValidHostAddress ());
				if (result == null)
				{
					throw new InvalidOperationException (FormattableString.Invariant (
						$"Not found addresses of type {family} for host [{hostName}]."));
				}

				return result;
			}
			catch (SocketException exception)
			{
				throw new InvalidOperationException (
					FormattableString.Invariant ($"Not found addresses of type {family} for host [{hostName}]."),
					exception);
			}
		}
	}
}
