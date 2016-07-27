using System;
using static System.Linq.Enumerable;
using System.Diagnostics.Contracts;
using Novartment.Base.Collections;

namespace Novartment.Base.Net
{
	/// <summary>
	/// Методы расширения для коллекции параметров QualityValueParameter.
	/// </summary>
	public static class QualityValueParameterCollectionExtensions
	{
		/// <summary>
		/// Добавляет к коллекции параметров новый параметр с указанным значением так,
		/// чтобы его важность была меньше остальных параметров в коллекции.
		/// </summary>
		/// <param name="collection">Коллекция параметров.</param>
		/// <param name="value">Значение параметра для добавления в коллекцию.</param>
		/// <returns>Созданный параметр.</returns>
		public static QualityValueParameter AddOrderedByQuality (this IAdjustableCollection<QualityValueParameter> collection, string value)
		{
			if (collection == null)
			{
				throw new ArgumentNullException (nameof (collection));
			}
			if (value == null)
			{
				throw new ArgumentNullException (nameof (value));
			}
			Contract.EndContractBlock ();

			QualityValueParameter data;
			if (collection.Count < 1)
			{
				data = new QualityValueParameter (value, 1.0m);
			}
			else
			{
				var minQuality = collection.Min (item => item.Importance) - 0.01m;
				if (minQuality <= 0.0m) minQuality = 0.001m;
				data = new QualityValueParameter (value, minQuality);
			}
			collection.Add (data);

			return data;
		}
	}
}
