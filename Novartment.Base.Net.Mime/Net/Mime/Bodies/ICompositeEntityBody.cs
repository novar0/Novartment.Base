using Novartment.Base.Collections;

namespace Novartment.Base.Net.Mime
{
	/// <summary>
	/// Тело MIME-сущности с множественным (composite) содержимым согласно RFC 2045.
	/// </summary>
	public interface ICompositeEntityBody :
		IEntityBody
	{
		/// <summary>Получает коллекцию дочерних сущностей, которые содержатся в теле сущности.</summary>
		IAdjustableList<Entity> Parts { get; }

		/// <summary>Получает разграничитель, разделяющий на части сущность с множественным содержимым.</summary>
		string Boundary { get; }
	}
}
