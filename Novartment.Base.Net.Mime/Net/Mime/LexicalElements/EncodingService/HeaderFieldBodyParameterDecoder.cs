using System;
using System.Collections.Generic;
using System.Text;
using Novartment.Base.Collections;

namespace Novartment.Base.Net.Mime
{
	internal class HeaderFieldBodyParameterDecoder
	{
		private readonly ArrayList<HeaderFieldParameter> _parameters = new ArrayList<HeaderFieldParameter> ();
		private string _parameterName = null;
		private Encoding _parameterEncoding = null;
		private string _parameterValue = string.Empty;

		internal HeaderFieldBodyParameterDecoder ()
		{
		}

		internal void AddPart (HeaderFieldParameterPart part)
		{
			if (part.Section == 0)
			{
				// начался новый параметр
				// возвращаем предыдущий параметр если был
				if (_parameterName != null)
				{
					_parameters.Add (new HeaderFieldParameter (_parameterName, _parameterValue));
				}

				_parameterName = part.Name;
				_parameterValue = string.Empty;
				try
				{
					_parameterEncoding = Encoding.GetEncoding (part.Encoding ?? "us-ascii");
				}
				catch (ArgumentException excpt)
				{
					throw new FormatException (
						FormattableString.Invariant ($"'{part.Encoding}' is not valid code page name."),
						excpt);
				}
			}

			_parameterValue += part.GetValue (_parameterEncoding);
		}

		internal IReadOnlyList<HeaderFieldParameter> GetResult ()
		{
			// возвращаем предыдущий параметр если был
			if (_parameterName != null)
			{
				_parameters.Add (new HeaderFieldParameter (_parameterName, _parameterValue));
			}

			return _parameters;
		}
	}
}
