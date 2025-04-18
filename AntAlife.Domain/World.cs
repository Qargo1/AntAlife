using System;
using System.Collections.Generic;

namespace AntAlife.Domain
{
    public class World
    {
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

        public World(int width, int height)
        {
            Width = width;
            Height = height;

            Grid = new Cell[width, height];
            
            if (width <= 0 || height <= 0)
                throw new ArgumentException("Width and height must be positive.");

            Ants = new List<Ant>();
            FoodItems = new List<Food>();
            Enemies = new List<Enemy>();

            for (var i = 0; i < width; i++)
            for (var j = 0; j < height; j++)
                Grid[i, j] = new Cell(i, j);
        }
        
        public void UpdateWeather(Random random)
        {
            IsRaining = random.Next(0, 100) < 10; // 10% шанс дождя
            if (IsRaining)
            {
                foreach (var cell in Grid)
                {
                    if (cell.CellType == CellType.Tunnel && random.Next(0, 100) < 20)
                        cell.CellType = CellType.Flooded;
                }
            }
        }
        
        public void UpdateSeason()
        {
            Day++;
            CurrentSeason = (Season)((Day % 360) / 90); // 90 дней на сезон
        }
    }
}