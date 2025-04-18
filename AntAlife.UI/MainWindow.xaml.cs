using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using AntAlife.Domain;
using AntAlife.Logic;

namespace AntAlife.UI
{
    public partial class MainWindow : Window
    {
        private readonly World _world;
        private readonly WorldUpdater _worldUpdater;
        private readonly DispatcherTimer _timer;
        private readonly Rectangle[,] _gridCells; // Кэшируем клетки сетки

        public MainWindow()
        {
            InitializeComponent();
            _world = new World(450, 450); // Уменьшенный мир для производительности
            _worldUpdater = new WorldUpdater(_world);
            _worldUpdater.WorldUpdated += () => DrawWorld(_world);
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            _timer.Tick += (s, e) => _worldUpdater.Update();
            _timer.Start();

            // Инициализируем кэш сетки
            _gridCells = new Rectangle[_world.Width, _world.Height];
            InitializeGrid();
        }

        // Инициализация сетки (создаём клетки один раз)
        private void InitializeGrid()
        {
            for (var x = 0; x < _world.Width; x++)
            {
                for (var y = 0; y < _world.Height; y++)
                {
                    var cellType = _world.Grid[x, y].CellType;
                    var cell = new Rectangle
                    {
                        Width = 20,
                        Height = 20,
                        Fill = Brushes.Transparent
                    };
                    Canvas.SetLeft(cell, x * 20);
                    Canvas.SetTop(cell, y * 20);
                    WorldCanvas.Children.Add(cell);
                    _gridCells[x, y] = cell;
                }
            }
        }

        private void DrawWorld(World world)
        {
            // Не очищаем Canvas, только обновляем динамические элементы
            UpdateGrid(world);
            WorldCanvas.Children.Clear(); // Очищаем только динамические элементы
            DrawNest(world);
            foreach (var ant in world.Ants) DrawAnt(ant);
            foreach (var enemy in world.Enemies) DrawEnemy(enemy);
            foreach (var food in world.FoodItems) DrawFood(food);
            foreach (var egg in world.Eggs) DrawEggs(egg);
        }

        private void UpdateGrid(World world)
        {
            for (var x = 0; x < world.Width; x++)
            {
                for (var y = 0; y < world.Height; y++)
                {
                    var cellType = world.Grid[x, y].CellType;
                    _gridCells[x, y].Fill = cellType switch
                    {
                        CellType.Ground => Brushes.Green,
                        CellType.Chamber => Brushes.LightGray,
                        CellType.Tunnel => Brushes.DarkGray,
                        CellType.Exit => Brushes.DarkGray,
                        _ => Brushes.Transparent
                    };
                }
            }
        }

        private void DrawNest(World world)
        {
            var nest = new Rectangle // Меньший размер, круг
            {
                Width = 300,
                Height = 300,
                Fill = Brushes.Brown
            };
            Canvas.SetLeft(nest, world.NestX * 20 - 15);
            Canvas.SetTop(nest, world.NestY * 20 - 15);
            WorldCanvas.Children.Add(nest);
        }

        private void DrawAnt(Ant ant)
        {
            var antEllipse = new Ellipse // Вернули круг для муравьёв
            {
                Width = 5,
                Height = 5,
                Fill = ant.AntType switch
                {
                    AntType.Queen => Brushes.Purple,
                    AntType.Worker => Brushes.Black,
                    AntType.Soldier => Brushes.Blue,
                    AntType.Nurse => Brushes.White,
                    _ => Brushes.Gray
                }
            };
            Canvas.SetLeft(antEllipse, ant.Position.X * 20 - 5);
            Canvas.SetTop(antEllipse, ant.Position.Y * 20 - 5);
            WorldCanvas.Children.Add(antEllipse);
        }

        private void DrawEnemy(Enemy enemy)
        {
            var triangle = new Polygon // Вернули треугольник
            {
                Points = new PointCollection(new[]
                {
                    new Point(0, -5),
                    new Point(5, 5),
                    new Point(-5, 5)
                }),
                Fill = Brushes.Red
            };
            Canvas.SetLeft(triangle, enemy.Position.X * 20 + 200);
            Canvas.SetTop(triangle, enemy.Position.Y * 20 + 200);
            WorldCanvas.Children.Add(triangle);
        }

        private void DrawFood(Food food)
        {
            var foodRect = new Rectangle
            {
                Width = 10,
                Height = 10,
                Fill = Brushes.Yellow
            };
            Canvas.SetLeft(foodRect, food.Position.X * 20 - 4);
            Canvas.SetTop(foodRect, food.Position.Y * 20 - 4);
            WorldCanvas.Children.Add(foodRect);
        }

        private void DrawEggs(Egg egg)
        {
            var eggRect = new Ellipse // Круг для яиц
            {
                Width = 5,
                Height = 5,
                Fill = Brushes.Gray
            };
            Canvas.SetLeft(eggRect, egg.Position.X * 20 - 3);
            Canvas.SetTop(eggRect, egg.Position.Y * 20 - 3);
            WorldCanvas.Children.Add(eggRect);
        }
    }
}