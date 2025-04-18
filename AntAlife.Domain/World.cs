using System;
using System.Collections.Generic;

namespace AntAlife.Domain
{
    public class World
    {
        private readonly Random _random = new Random();
        public int Width { get; }
        public int Height { get; }
        public Cell[,] Grid { get; }
        public List<Ant> Ants { get; set; }
        public List<Food> FoodItems { get; set; }
        public List<Enemy> Enemies { get; set; }
        public List<Egg> Eggs { get; set; }
        private bool IsRaining { get; set; }
        public enum Season { Spring, Summer, Autumn, Winter }
        public Season CurrentSeason { get; set; }
        private int Day { get; set; }
        public int NestX { get; set; }
        public int NestY { get; set; }
        public int NestFood { get; set; }
        
        public World(int width, int height)
        {
            Width = width;
            Height = height;
            Grid = new Cell[width, height];
            
            // Заполняем Soil по умолчанию
            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
                Grid[x, y] = new Cell(CellType.Soil, _random.Next(30, 100), x, y);

            // Добавляем Ground на верхний ряд (поверхность)
            for (var x = 0; x < width; x++)
                Grid[x, 0].CellType = CellType.Ground;

            // Стартовая Chamber для королевы (3x3 в верхнем левом углу)
            for (var x = 0; x < 3 && x < width; x++)
            for (var y = 0; y < 3 && y < height; y++)
                Grid[x, y].CellType = CellType.Chamber;

            // Случайные StoneBlock (5-10% клеток)
            var stoneCount = _random.Next(width * height / 20, width * height / 10);
            for (var i = 0; i < stoneCount; i++)
            {
                var x = _random.Next(width);
                var y = _random.Next(height);
                if (Grid[x, y].CellType != CellType.Chamber) // Не перекрываем Chamber
                    Grid[x, y].CellType = CellType.StoneBlock;
            }
            
            // Случайные Tunnel (2-5% клеток)
            var tunnelCount = _random.Next(width * height / 50, width * height / 20);
            for (var i = 0; i < tunnelCount; i++)
            {
                var x = _random.Next(width);
                var y = _random.Next(height);
                if (Grid[x, y].CellType != CellType.Chamber && Grid[x, y].CellType != CellType.Ground)
                    Grid[x, y].CellType = CellType.Tunnel;
            }

            // Преобразуем Tunnel на границах в Exit
            for (var x = 0; x < width; x++)
            for (var y = 0; y < height; y++)
                if (Grid[x, y].CellType == CellType.Tunnel &&
                    (x == 0 || x == width - 1 || y == 0 || y == height - 1))
                    Grid[x, y].CellType = CellType.Exit;
            
            NestX = 1;
            NestY = 1;
            Ants = new List<Ant> { new Ant(_random, NestX, NestY, AntType.Queen) };
            Enemies = new List<Enemy> { new Enemy(_random, 10, 10, EnemyType.Spider) };
            FoodItems = new List<Food> { new Food(5, 5), new Food(15, 15) };
            Eggs = new List<Egg> { new Egg(_random, 2, 2) { HatchTime = 10 } };
        }
        
        public void UpdateWeather(Random random)
        {
            IsRaining = random.Next(0, 100) < 10; // 10% шанс дождя
            if (!IsRaining) return;
            foreach (var cell in Grid)
            {
                if (cell.CellType == CellType.Tunnel && random.Next(0, 100) < 20)
                    cell.CellType = CellType.Flooded;
            }
        }
        
        public void UpdateSeason()
        {
            Day++;
            CurrentSeason = (Season)((Day % 360) / 90); // 90 дней на сезон
        }
    }
}