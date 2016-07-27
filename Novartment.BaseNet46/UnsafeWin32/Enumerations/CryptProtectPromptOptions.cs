using System;

namespace Novartment.Base.UnsafeWin32
{
	/// <summary>
	/// Опции, определяющие когда будет отображён запрос подтверждения пользователя при работе с зашифрованными данными.
	/// </summary>
	/// <remarks>Значение используется в поле dwPromptFlags структуры CRYPTPROTECT_PROMPTSTRUCT.</remarks>
	[Flags]
	internal enum CryptProtectPromptOptions : int
	{
		None = 0,

		/// <summary>Запрос подтверждения при дешифрации.</summary>
		/// <remarks>Константа CRYPTPROTECT_PROMPT_ON_UNPROTECT.</remarks>
		OnUnprotect = 1,

		/// <summary>Запрос подтверждения при шифровании.</summary>
		/// <remarks>Константа CRYPTPROTECT_PROMPT_ON_PROTECT.</remarks>
		OnProtect = 2,

		/// <summary>Рекомендуется строгий вариант запроса.</summary>
		/// <remarks>Константа CRYPTPROTECT_PROMPT_STRONG.</remarks>
		DefaultStrong = 8,

		/// <summary>Требуется строгий варианта запроса.</summary>
		/// <remarks>Константа CRYPTPROTECT_PROMPT_REQUIRE_STRONG.</remarks>
		RequireStrong = 0x10
	}
}
