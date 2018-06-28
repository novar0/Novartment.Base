using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Novartment.Base.Net.Mime
{
	public class HeaderFieldBuilderAddrSpecList : HeaderFieldBuilder
	{
		private readonly IReadOnlyList<AddrSpec> _addrSpecs;
		private int _idx = 0;

		/// <summary>
		/// Создает поле заголовка из указанной коллекции интернет-идентификаторов.
		/// </summary>
		/// <param name="name">Имя поля заголовка.</param>
		/// <param name="addrSpecs">Коллекция языков в формате интернет-идентификаторов.</param>
		/// <returns>Поле заголовка.</returns>
		public HeaderFieldBuilderAddrSpecList (HeaderFieldName name, IReadOnlyList<AddrSpec> addrSpecs)
			: base (name)
		{
			if (addrSpecs == null)
			{
				throw new ArgumentNullException (nameof (addrSpecs));
			}

			Contract.EndContractBlock ();

			_addrSpecs = addrSpecs;
		}

		protected override int GetNextPart (Span<byte> buf, out bool isLast)
		{
			if (_idx >= _addrSpecs.Count)
			{
				isLast = true;
				return 0;
			}

			var size = _addrSpecs[_idx].ToAngleUtf8String (buf);
			isLast = _idx == (_addrSpecs.Count - 1);
			_idx++;
			return size;
		}
	}
}
