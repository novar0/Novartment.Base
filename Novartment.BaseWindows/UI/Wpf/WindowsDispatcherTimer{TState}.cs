using System;
using System.Windows.Threading;

namespace Novartment.Base.UI.Wpf
{
	/// <summary>
	/// Реализация таймера на основе System.Windows.Threading.DispatcherTimer.
	/// При срабатывании вызывает указанный делегат с указанным параметром.
	/// </summary>
	/// <typeparam name="TState">Тип объекта, передаваемый в делегат срабатывания таймера.</typeparam>
	public class WindowsDispatcherTimer<TState> : BaseTimer<TState>
	{
		private readonly DispatcherTimer _timer;

		/// <summary>
		/// Инициализирует новый экземпляр WindowsDispatcherTimer, вызывающий при срабатывании указанный делегат с указанным параметром.
		/// Опционально можно указать приоритет, с которым вызовы делегата будут ставиться в очередь диспетчера.
		/// </summary>
		/// <param name="callback">Делегат, вызываемый при срабатывании таймера.</param>
		/// <param name="state">Объект, передавамый в делегат при срабатывании таймера.</param>
		/// <param name="dispatcher">Диспетчер, используемый при создании таймера.</param>
		/// <param name="priority">Приоритет, с которым вызовы делегата будут ставиться в очередь диспетчера.
		/// По-умолчанию используется фоновый приоритет.</param>
		public WindowsDispatcherTimer (
			Action<TState> callback,
			TState state,
			Dispatcher dispatcher,
			DispatcherPriority priority)
			: base (callback, state)
		{
			if (callback == null)
			{
				throw new ArgumentNullException (nameof (callback));
			}

			_timer = new DispatcherTimer (priority, dispatcher);
			_timer.Tick += base.DoCallback;
		}

		/// <summary>
		/// Получает или устанавливает интервал срабатывания таймера.
		/// </summary>
		public override TimeSpan Interval
		{
			get => _timer.Interval;

			set
			{
				_timer.Interval = value;
			}
		}

		/// <summary>
		/// Получает состояние таймера. True если таймер запущен, иначе false.
		/// </summary>
		public override bool Enabled => _timer.IsEnabled;

		/// <summary>
		/// Запускает таймер.
		/// </summary>
		public override void Start () => _timer.Start ();

		/// <summary>
		/// Останавливает таймер.
		/// </summary>
		public override void Stop () => _timer.Stop ();

		/// <summary>
		/// Освобождает занимаемые объектом ресурсы.
		/// </summary>
		public override void Dispose ()
		{
			_timer.Stop ();
			_timer.Tick -= base.DoCallback;
			GC.SuppressFinalize (this);
		}
	}
}
