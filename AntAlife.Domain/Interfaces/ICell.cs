using System;
using AntAlife.Domain.Enums;

namespace AntAlife.Domain.Interfaces
{
    public interface ICell
    {
        Guid Id { get; set; }
        CellType CellType { get; set; }
        int Durability { get; set; }
        float FoodPheromone { get; set; } // Феромон, ведущий к еде
        float DangerPheromone { get; set; } // Феромон, сигнализирующий об опасности
        float TrailPheromone { get; set; }

        struct Point
        {
            public int X { get; set; }
            public int Y { get; set; }
            
            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }
        }
    }
}