using System;

namespace Novartment.Base.UI
{
	/// <summary>
	/// Данные для старта перетаскивания.
	/// </summary>
	public struct DragStartData
	{
		/// <summary>
		/// Объект, который будет перетаскиваться.
		/// </summary>
		public IDataContainer Object;

		/// <summary>
		/// Набор разрешённых для перетаскивания эффектов.
		/// </summary>
		public DragDropEffects AllowedEffects;
	}

	/// <summary>
	/// Источник для перетаскивания объектов в том числе в другие приложения.
	/// </summary>
	public interface IDragDropSource
	{
		/// <summary>
		/// При старте перетаскивания создаёт объект, который будет перетаскиваться.
		/// </summary>
		/// <param name="positionX">Координата по оси X точки, откуда начато перетаскивание.</param>
		/// <param name="positionY">Координата по оси Y точки, откуда начато перетаскивание.</param>
		/// <param name="dragControl">Орган управления, использованный для старта перетаскивания.</param>
		/// <returns>
		/// Кортеж, состоящий из объекта, который будет перетаскиваться и набора разрешённых для перетаскивания эффектов.
		/// Перетаскивание будет отменено если разрешённых эффектов нет (указано значение DragDropEffect.None).
		/// </returns>
		DragStartData DragStart (double positionX, double positionY, DragControl dragControl);

		/// <summary>
		/// Отображает для пользователя эффект перестакивания.
		/// </summary>
		/// <param name="effects">Выбранный целью эффект перетаскивания.</param>
		/// <returns>Признак использования курсоров по умолчанию.
		/// Если True, то будут автоматически устанавливаться курсоры по умолчанию.
		/// Если False то необходимые курсоры устанавливает источник перетаскивания.</returns>
		bool GiveFeedback (DragDropEffects effects);

		/// <summary>
		/// Определяет дальнейшее действие (продолжить, отменить либо завершить перетаскивание)
		/// в случае измененения параметров перетаскивания.
		/// </summary>
		/// <param name="escapePressed">Признак нажатия клавиши ESC.</param>
		/// <param name="keyStates">Состояние элементов управления, влияющих на перетаскивание.</param>
		/// <returns>Требуемое действие перетаскивания.</returns>
		DragDropAction QueryContinueDrag (bool escapePressed, DragDropKeyStates keyStates);

		/// <summary>
		/// Обрабатывает завершение перетаскивания.
		/// Как правило, требуется действие только для эффекта DragDropEffect.Move.
		/// </summary>
		/// <param name="effects">Выбранный целью эффект перетаскивания.</param>
		void DragEnd (DragDropEffects effects);
	}
}
