namespace Novartment.Base.Text
{
	/// <summary>
	/// Поддержка замены (скрытия) образцов в тексте, например
	/// в котором непредсказуемо может встречаться секретная информация (пароли, имена, номера, адреса и тд)
	/// при этом вся выдаваемая информация должна скрывать секретную часть.
	/// </summary>
	public interface IPatternsReplacer
	{
		/// <summary>
		/// Получает или устанавливает признак включения замены.
		/// </summary>
		bool ReplacementEnabled { get; set; }

		/// <summary>
		/// Получает или устанавливает строку которая будет вставлена вместо встреченных шаблонов.
		/// </summary>
		string ReplacementValue { get; set; }

		/// <summary>
		/// Добавляет строку в список заменяемых шаблонов.
		/// </summary>
		/// <param name="pattern">Строка-шаблон для поиска и замены.</param>
		void AddReplacementStringPattern (string pattern);

		/// <summary>
		/// Добавляет регулярное выражение в список заменяемых шаблонов.
		/// </summary>
		/// <param name="pattern">Строка-шаблон для поиска и замены.</param>
		void AddReplacementRegexPattern (string pattern);

		/// <summary>
		/// Очищает список заменяемых шаблонов.
		/// </summary>
		void ClearReplacementPatterns ();
	}
}
