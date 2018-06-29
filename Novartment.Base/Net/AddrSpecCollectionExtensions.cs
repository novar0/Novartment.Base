using System;
using System.Diagnostics.Contracts;
using Novartment.Base.Collections;

namespace Novartment.Base.Net
{
	/// <summary>
	/// Методы расширения для коллекции AddrSpec.
	/// </summary>
	public static class AddrSpecCollectionExtensions
	{
		/// <summary>
		/// Добавляет к коллекции интернет-идентификаторов указанное строковое представление интернет-идентификатора.
		/// </summary>
		/// <param name="collection">Коллекции интернет-идентификаторов.</param>
		/// <param name="address">Строковое представление интернет-идентификатора.</param>
		/// <returns>Созданный интернет-идентификатор, который был добавлен в коллецию.</returns>
		public static AddrSpec Add (this IAdjustableCollection<AddrSpec> collection, string address)
		{
			if (collection == null)
			{
				throw new ArgumentNullException (nameof (collection));
			}

			if (address == null)
			{
				throw new ArgumentNullException (nameof (address));
			}

			Contract.EndContractBlock ();

			var addrSpec = AddrSpec.Parse (address.AsSpan ());
			collection.Add (addrSpec);

			return addrSpec;
		}
	}
}
