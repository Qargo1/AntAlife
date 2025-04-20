using System;
using System.Collections.Generic;
using AntAlife.Domain.Enums;
using AntAlife.Domain.Interfaces;

namespace AntAlife.Domain
{
    public class World
    {
        private readonly Random _random = new Random();
        public int Width { get; }
        public int Height { get; }
        public Cell[,] Grid { get; }
        public List<Ant> Ants { get; set; }
        public List<Enemy> Enemies { get; set; }
        public List<Item> Items { get; set; }
        public List<Egg> Eggs { get; set; }
        public List<Food> FoodItems { get; set; }
        private bool IsRaining { get; set; }
        public Season CurrentSeason { get; set; }
        private int Day { get; set; }
        public int NestX { get; set; }
        public int NestY { get; set; }
        public int NestFood { get; set; }
        public bool NestIsExisting  { get; set; }
        
        // Флаг, что идет строительство
        public bool IsNestUnderConstruction { get; private set; } = false;
        // Текущий прогресс (сколько клеток построено)
        public int CurrentNestRadius { get; private set; } = 0; // Текущий радиус построенного слоя
        public int TargetNestRadius { get; private set; } = 10; // Целевой радиус (для ~20x20)
        // Можно добавить константу для размера гнезда
        private const int NestSize = 5; // Например, 5x5

        public World(int width, int height)
        {
            Width = width;
            Height = height;
            Grid = new Cell[width, height];
            Ants = new List<Ant>();
            Enemies = new List<Enemy>();
            Items = new List<Item>();
            Eggs = new List<Egg>();
            FoodItems = new List<Food>();
            IsRaining = false;
            CurrentSeason = Season.Summer;
            Day = 0;
            NestIsExisting = false;
            
            // 1. Basic Grid initialization
            for (var i = 0; i < Width; i++)
            {
                for (var j = 0; j < Height; j++)
                {
                    Grid[i, j] = new Cell(CellType.Unexplored, i, j);
                }
            }
            
            // 2. Adding queen
            Ants.Add(new Ant(_random, width / 2, height / 2, AntType.Queen));
        }
        
        // Проверяет, можно ли двигаться на клетку
        public bool IsValidCoordinate(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }
        
        public void StartNestConstruction(int centerX, int centerY)
        {
            if (IsNestUnderConstruction) return;
            IsNestUnderConstruction = true;
            NestX = centerX;
            NestY = centerY;
            CurrentNestRadius = 1; // Начинаем с радиуса 1 (камера)
            NestIsExisting = false; // Сброс флага при начале новой стройки
            Console.WriteLine($"World: Starting layered nest construction at ({centerX},{centerY}). Target radius: {TargetNestRadius}");
        }
        
        public void IncrementNestRadius() // Вызывается апдейтером
        {
            if (IsNestUnderConstruction) CurrentNestRadius++;
        }

        // 6. Метод для ЗАВЕРШЕНИЯ строительства (вызывается из WorldUpdater)
        public void FinishNestConstruction()
        {
            Console.WriteLine($"World: Nest finished. Final radius reached: {CurrentNestRadius - 1}.");
            IsNestUnderConstruction = false;
            CurrentNestRadius = 0;
            NestIsExisting = true; // Гнездо готово!
        }
    }
}