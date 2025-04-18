using System;

namespace AntAlife.Domain
{
    public interface IEntity
    {
        Guid Id { get; set; }
        int Hp { get; set; }

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