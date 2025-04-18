using System;
using System.Collections.Generic;
using System.Linq;
using AntAlife.Domain;

namespace AntAlife.Logic
{
    public abstract class Brain : IBrain 
    {
        public Entity Entity { get; }
        public abstract void Act(World world, Random random);
        
        protected Brain(Entity entity)
        {
            Entity = entity;
        }

        // Проверяет, можно ли двигаться на клетку
        private static bool IsWalkable(World world, int x, int y)
        {
            if (x < 0 || x >= world.Width || y < 0 || y >= world.Height)
                return false;
            var cellType = world.Grid[x, y].CellType;
            return cellType == CellType.Tunnel || cellType == CellType.Chamber ||
                   cellType == CellType.Exit || cellType == CellType.Ground;
        }

        // Случайное движение
        protected void MoveRandomly(World world, Random random)
        {
            if (Entity.Energy <= 0) return;

            var x = Entity.Position.X;
            var y = Entity.Position.Y;
            var neighbors = new List<(int x, int y)>
            {
                (x, y - 1), (x, y + 1), (x - 1, y), (x + 1, y)
            };

            var validMoves = neighbors
                .Where(p => IsWalkable(world, p.x, p.y))
                .ToList();

            if (!validMoves.Any()) return;

            var newPos = validMoves[random.Next(validMoves.Count)];
            Entity.Position = new IEntity.Point(newPos.x, newPos.y);
            Entity.Energy -= 1;
        }
        
        // Движение к цели
        protected void Towards(World world, int targetX, int targetY)
        {
            if (Entity.Energy <= 0) return;

            var dx = targetX - Entity.Position.X;
            var dy = targetY - Entity.Position.Y;

            var moveX = dx != 0 ? Math.Sign(dx) : 0;
            var moveY = dy != 0 ? Math.Sign(dy) : 0;

            var newX = Entity.Position.X + moveX;
            var newY = Entity.Position.Y + moveY;

            if (newX < 0 || newX >= world.Width || newY < 0 || newY >= world.Height ||
                (world.Grid[newX, newY].CellType != CellType.Tunnel &&
                 world.Grid[newX, newY].CellType != CellType.Chamber &&
                 world.Grid[newX, newY].CellType != CellType.Exit)) return;
            Entity.Position = new IEntity.Point(newX, newY);
            Entity.Energy -= 1;
        }
        
        protected void From(Entity entity, World world, Random random)
        {
            if (entity.Energy <= 0) return; // Нет энергии — не двигаемся

            var x = entity.Position.X;
            var y = entity.Position.Y;

            // Возможные направления: вверх, вниз, влево, вправо
            var neighbors = new List<(int x, int y)>
            {
                (x, y - 1), (x, y + 1), (x - 1, y), (x + 1, y)
            };

            // Фильтруем допустимые ходы (в пределах карты и проходимые клетки)
            var validMoves = neighbors
                .Where(p => p.x >= 0 && p.x < world.Grid.GetLength(0) &&
                            p.y >= 0 && p.y < world.Grid.GetLength(1) &&
                            (world.Grid[p.x, p.y].CellType == CellType.Tunnel ||
                             world.Grid[p.x, p.y].CellType == CellType.Chamber ||
                             world.Grid[p.x, p.y].CellType == CellType.Exit))
                .ToList();

            if (!validMoves.Any()) return;
            var newPos = validMoves[random.Next(validMoves.Count)];
            entity.Position = new IEntity.Point(newPos.x, newPos.y);
            entity.Energy -= 1; // Движение тратит энергию
        }

        // Расстояние до другой сущности
        protected static double DistanceTo(Entity a, Entity b)
        {
            return Math.Sqrt(Math.Pow(a.Position.X - b.Position.X, 2) + Math.Pow(a.Position.Y - b.Position.Y, 2));
        }
    }
}