using System.ComponentModel; // Для Closing
using System.Windows;
using AntAlife.Data;
using AntAlife.Domain; // Для World
using AntAlife.Logic; // Для WorldUpdater

namespace AntAlife.UI
{
    public partial class StatsWindow : Window
    {
        private readonly StatsViewModel _viewModel;
        private readonly WorldUpdater _worldUpdater;

        // Принимаем World и WorldUpdater при создании окна
        public StatsWindow(World world, WorldUpdater worldUpdater)
        {
            InitializeComponent();
            _worldUpdater = worldUpdater;

            // Создаем ViewModel и устанавливаем его как DataContext
            _viewModel = new StatsViewModel(world, worldUpdater);
            this.DataContext = _viewModel;

            // Подписываемся на событие обновления мира
            if (_worldUpdater != null)
            {
                _worldUpdater.WorldUpdated += WorldUpdater_WorldUpdated;
            }
        }

        // Обработчик события обновления мира
        private void WorldUpdater_WorldUpdated()
        {
            // Обновляем ViewModel В ПОТОКЕ UI!
            // Dispatcher нужен, т.к. событие может прийти из другого потока (хотя DispatcherTimer обычно в UI)
            Dispatcher.Invoke(() =>
            {
                _viewModel.UpdateStats();
            });
        }

        // Отписываемся от события при закрытии окна, чтобы избежать утечек памяти
        protected override void OnClosing(CancelEventArgs e)
        {
            if (_worldUpdater != null)
            {
                _worldUpdater.WorldUpdated -= WorldUpdater_WorldUpdated;
            }
            base.OnClosing(e);
        }
    }
}