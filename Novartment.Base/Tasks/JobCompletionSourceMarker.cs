namespace Novartment.Base
{
	/// <summary>
	/// Фабрика для создания экземпляров производителя выполнения особого задания-маркера.
	/// </summary>
	public static class JobCompletionSourceMarker
	{
		/// <summary>
		/// Создаёт экземпляр производителя выполнения особого задания-маркера.
		/// </summary>
		/// <typeparam name="TItem">Тип входного параметра заданий.</typeparam>
		/// <typeparam name="TResult">Тип, возвращаемый заданиями.</typeparam>
		/// <returns>Созданный экземпляр производителя выполнения особого задания-маркера.</returns>
		public static JobCompletionSource<TItem, TResult> Create<TItem, TResult> ()
		{
			return new JobCompletionMarker<TItem, TResult> ();
		}

		internal class JobCompletionMarker<TItem, TResult> : JobCompletionSource<TItem, TResult>
		{
			internal JobCompletionMarker ()
				: base (default)
			{
			}

			public override bool IsMarker => true;
		}
	}
}
