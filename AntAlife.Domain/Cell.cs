using System;
using AntAlife.Domain.Enums;
using AntAlife.Domain.Interfaces;

namespace AntAlife.Domain
{
    [Serializable]
    public class Cell : ICell
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public CellType CellType { get; set; }
        public int Durability { get; set; }

        // Добавляем новые феромоны!
        public float FoodPheromone { get; set; } = 0f; // Феромон, ведущий к еде
        public float DangerPheromone { get; set; } = 0f; // Феромон, сигнализирующий об опасности
        public float TrailPheromone { get; set; } = 0f; // Общий феромон "тропы" домой (опционально)
        public ICell.Point Point { get; set; }

        public Cell(CellType cellType, int x, int y, int durability = 110)
        {
            CellType = cellType;
            Durability = durability;
            Point = new ICell.Point() { X = x, Y = y };
        }

        public Cell(CellType cellType, int x, int y)
        {
            Point = new ICell.Point() { X = x, Y = y };
        }
    }
}