using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Shapes;
using System.Windows.Media;
using AntAlife.Domain;
using AntAlife.Domain.Enums;
using AntAlife.Logic;

namespace AntAlife.UI
{
    public partial class MainWindow : Window
    {
        private readonly World _world;
        private readonly WorldUpdater _worldUpdater;
        private readonly DispatcherTimer _timer;
        private StatsWindow _statsWindow; // Поле для хранения ссылки на окно статистики
        private const int AntShape = 10;

        public MainWindow()
        {
            InitializeComponent();
            _world = new World(80, 80); // Уменьшенный мир для производительности
            _worldUpdater = new WorldUpdater(_world);
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _timer.Tick += Timer_Tick;
            DrawWorld();
            
            // Добавим обработчик события Loaded для позиционирования окна
            this.Loaded += MainWindow_Loaded;
            
            _timer.Start();
        }
        
        // Обработчик события загрузки главного окна
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Создаем и показываем окно статистики ПОСЛЕ загрузки главного
            _statsWindow = new StatsWindow(_world, _worldUpdater); // Передаем world и updater
            _statsWindow.Owner = this; // Делаем главным окном владельцем

            // Позиционируем справа от главного окна
            _statsWindow.Left = this.Left + this.ActualWidth;
            _statsWindow.Top = this.Top;

            _statsWindow.Show();
        }

        // object sender - сам таймер
        private void Timer_Tick(object sender, EventArgs e)
        {
            _worldUpdater.Update();
            DrawWorld();
        }
        
        // При закрытии главного окна, закроем и окно статистики
        protected override void OnClosing(CancelEventArgs e)
        {
            _statsWindow?.Close(); // Закрываем окно статистики, если оно существует
            base.OnClosing(e);
        }

        private void DrawWorld()
        {
            // 1. Очистить холст: Перед тем как рисовать новое положение, сотри старое!
            WorldCanvas.Children.Clear();
            
            if (_world == null) return;
            
            // 1.1 --- Рисуем Клетки Мира ---
            for (var x = 0; x < _world.Width; x++)
            {
                for (var y = 0; y < _world.Height; y++)
                {
                    var cell = _world.Grid[x, y];
                    if (cell.CellType == CellType.Unexplored) continue; // Рисуем ТОЛЬКО открытые клетки!
                    var cellRect = new Rectangle
                    {
                        Width = 10, // Размер клетки (должен совпадать с масштабом муравьев!)
                        Height = 10,
                        Fill = GetCellBrush(cell.CellType) // Метод для получения цвета клетки
                    };

                    Canvas.SetLeft(cellRect, x * 10); // Масштаб!
                    Canvas.SetTop(cellRect, y * 10);   // Масштаб!

                    WorldCanvas.Children.Add(cellRect);
                    // Клетки Unexplored просто не рисуем, фон Canvas (у тебя черный?) их покажет.
                }
            }
            
            // 2. Получить свежие данные: Нам не нужно брать AntBrains из апдейтера.
            //    Нам нужны сами муравьи из объекта _world, ведь их положение обновилось!
            //    Проходим по списку _world.Ants:
            foreach (var ant in _world.Ants)
            {
                // 3. Нарисовать каждого муравья (как мы делали раньше):
                var antShape = new Ellipse
                {
                    Width = 10,
                    Height = 10,
                    Fill = GetAntBrush(ant.AntType) // Используем наш метод для цвета
                };

                // Берем АКТУАЛЬНЫЕ координаты муравья из _world
                var canvasX = ant.Position.X * 10; // Не забываем масштаб
                var canvasY = ant.Position.Y * 10;

                Canvas.SetLeft(antShape, canvasX);
                Canvas.SetTop(antShape, canvasY);

                WorldCanvas.Children.Add(antShape); // Добавляем фигурку на холст
            }
        }

        private static Brush GetAntBrush(AntType antType)
        {
            switch (antType)
            {
                case AntType.Queen:
                case AntType.Worker:
                case AntType.Soldier:
                case AntType.Nurse:
                case AntType.Harvester:
                case AntType.Strategist:
                case AntType.Carrier:
                case AntType.Spy:
                    return new SolidColorBrush(Colors.Black);
                default:
                    throw new ArgumentOutOfRangeException(nameof(antType), antType, null);
            }
        }
        
        private static Brush GetCellBrush(CellType cellType)
        {
            switch (cellType)
            {
                case CellType.Ground:
                    return new SolidColorBrush(Colors.LightGray);
                case CellType.Unexplored:
                    return new SolidColorBrush(Colors.White);
                case CellType.Soil:
                    return new SolidColorBrush(Colors.Brown);
                case CellType.StoneBlock:
                    return new SolidColorBrush(Colors.DarkGray);
                case CellType.Tunnel:
                case CellType.Chamber:
                case CellType.Exit:
                    return new SolidColorBrush(Colors.White);
                case CellType.Flooded:
                    return new SolidColorBrush(Colors.Blue);
                default:
                    throw new ArgumentOutOfRangeException(nameof(cellType), cellType, null);
            }
        }
    }
}