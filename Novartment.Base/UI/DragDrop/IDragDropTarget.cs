namespace Novartment.Base.UI
{
	/// <summary>
	/// Цель, на которую можно перетаскивать объекты в том числе из других приложений.
	/// </summary>
	public interface IDragDropTarget
	{
		/// <summary>
		/// Обрабатывает вхождение перетаскиваемого объекта в область цели.
		/// </summary>
		/// <param name="data">Объект, предоставляющий перетаскиваемые данные.</param>
		/// <param name="keyStates">Состояние элементов управления, влияющих на перетаскивание.</param>
		/// <param name="positionX">Координата по оси X точки, где находится перетаскиваемый объект.</param>
		/// <param name="positionY">Координата по оси Y точки, где находится перетаскиваемый объект.</param>
		/// <param name="allowedEffects">Допустимые эффекты перетаскивания, разрешённые источником.</param>
		/// <returns>Эффект перетаскивания, выбранный целью из списка, предложенного источником.</returns>
		DragDropEffects DragEnter (IDataContainer data, DragDropKeyStates keyStates, double positionX, double positionY, DragDropEffects allowedEffects);

		/// <summary>
		/// Обрабатывает покидание объектом области цели.
		/// </summary>
		void DragLeave ();

		/// <summary>
		/// Обрабатывает изменение позиции или клавишных модификаторов при перетаскивании объекта над областью цели.
		/// </summary>
		/// <param name="keyStates">Состояние элементов управления, влияющих на перетаскивание.</param>
		/// <param name="positionX">Координата по оси X точки, где находится перетаскиваемый объект.</param>
		/// <param name="positionY">Координата по оси Y точки, где находится перетаскиваемый объект.</param>
		/// <param name="allowedEffects">Допустимые эффекты перетаскивания, разрешённые источником.</param>
		/// <returns>Эффект перетаскивания, выбранный целью из списка, предложенного источником.</returns>
		DragDropEffects DragOver (DragDropKeyStates keyStates, double positionX, double positionY, DragDropEffects allowedEffects);

		/// <summary>
		/// Обрабатывает отпускание объекта на цель.
		/// </summary>
		/// <param name="data">Объект, предоставляющий перетаскиваемые данные.</param>
		/// <param name="keyStates">Состояние элементов управления, влияющих на перетаскивание.</param>
		/// <param name="positionX">Координата по оси X точки, где находится перетаскиваемый объект.</param>
		/// <param name="positionY">Координата по оси Y точки, где находится перетаскиваемый объект.</param>
		/// <param name="allowedEffects">Допустимые эффекты перетаскивания, разрешённые источником.</param>
		/// <returns>Эффект перетаскивания, выбранный целью из списка, предложенного источником.</returns>
		DragDropEffects Drop (IDataContainer data, DragDropKeyStates keyStates, double positionX, double positionY, DragDropEffects allowedEffects);
	}
}
