using System;

namespace AntAlife.Domain
{
    [Serializable]
    public class Cell : ICell
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public CellType CellType { get; set; } = CellType.Soil;
        public int Durability { get; set; } = 80;
        public float Pheromone { get; set; }
        public ICell.Point Point { get; set; }

        public Cell(CellType cellType, int durability, int x, int y)
        {
            CellType = cellType;
            Durability = durability;
            Point = new ICell.Point() { X = x, Y = y };
        }

        public Cell(int x, int y)
        {
            Point = new ICell.Point() { X = x, Y = y };
        }
    }
}