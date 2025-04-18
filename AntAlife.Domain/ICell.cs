using System;

namespace AntAlife.Domain
{
    public interface ICell
    {
        Guid Id { get; set; }
        CellType CellType { get; set; }
        int Durability { get; set; }
        float Pheromone { get; set; }

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