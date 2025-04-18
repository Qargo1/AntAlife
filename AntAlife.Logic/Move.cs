using System;
using System.Collections.Generic;
using System.Linq;
using AntAlife.Domain;

namespace AntAlife.Logic
{
    public static class Move
    {
        public static void From(Ant ant, World world, Random random)
        {
            if (ant.Energy <= 0) return; // Нет энергии — не двигаемся

            var x = ant.Position.X;
            var y = ant.Position.Y;

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

            (int x, int y) newPos;
            if (ant.AntType == AntType.Worker && 
                (ant.State == EntityState.Hungry || ant.State == EntityState.Carrying))
            {
                // Следуем за феромонами
                newPos = validMoves
                    .OrderByDescending(p => world.Grid[p.x, p.y].Pheromone)
                    .First();
            }
            else if (ant.AntType == AntType.Soldier)
            {
                // Солдаты патрулируют случайным образом
                newPos = validMoves[random.Next(validMoves.Count)];
            }
            else
            {
                // Случайное движение для остальных
                newPos = validMoves[random.Next(validMoves.Count)];
            }

            ant.Position = new IEntity.Point(newPos.x, newPos.y);
            ant.Energy -= 1; // Движение тратит энергию
        }

        // Движение к цели
        public static void Towards(Ant ant, int targetX, int targetY, World world)
        {
            if (ant.Energy <= 0) return;

            int dx = targetX - ant.Position.X;
            int dy = targetY - ant.Position.Y;

            int moveX = dx != 0 ? Math.Sign(dx) : 0;
            int moveY = dy != 0 ? Math.Sign(dy) : 0;

            int newX = ant.Position.X + moveX;
            int newY = ant.Position.Y + moveY;

            if (newX >= 0 && newX < world.Width && newY >= 0 && newY < world.Height &&
                (world.Grid[newX, newY].CellType == CellType.Tunnel ||
                 world.Grid[newX, newY].CellType == CellType.Chamber ||
                 world.Grid[newX, newY].CellType == CellType.Exit))
            {
                ant.Position = new IEntity.Point(newX, newY);
                ant.Energy -= 1;
            }
        }

        // Обновлённый метод Act
        public static void Act(Random random, World world, Ant ant)
        {
            if (ant.Energy <= 0)
            {
                ant.Rest();
                return;
            }

            switch (ant.AntType)
            {
                case AntType.Queen:
                    if (ant.Energy >= 20 && world.Eggs.Count < 10)
                    {
                        ant.LayEgg(random, ant.Position.X, ant.Position.Y, world);
                    }
                    break;
                case AntType.Worker:
                    if (ant.CarriedItem == null)
                        ant.FindAndCarryFoodOrWater(world, ant);
                    else
                        ReturnToNest(world, ant);
                    break;
                case AntType.Soldier:
                    PatrolOrFight(random, world, ant);
                    break;
                case AntType.Nurse:
                    ProtectEggs(world, ant);
                    break;
                case AntType.Egg:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            ant.Energy -= 1;
        }

        public static void ReturnToNest(World world, Ant ant)
        {
            // ToDo:
            // int nestX = world.NestX; // Предполагаем, что у World есть координаты муравейника
            // int nestY = world.NestY;
            // if (ant.Position.X == nestX && ant.Position.Y == nestY)
            // {
            //     ant.CarriedItem = null;
            // }
            // else
            // {
            //    Towards(ant, nestX, nestY, world);
            // }
        }

        public static void PatrolOrFight(Random random, World world, Ant ant)
        {
            foreach (var enemy in world.Enemies)
            {
                if (DistanceTo(ant, enemy) <= ant.PatrolRadius)
                {
                    ant.AttackEnemy(enemy);
                    return;
                }
            }
            From(ant, world, random); // Случайное патрулирование
        }

        public static void ProtectEggs(World world, Ant ant)
        {
            if (ant.CarriedItem == null)
            {
                foreach (var egg in world.Eggs)
                {
                    if (DistanceTo(ant, egg) <= 1)
                    {
                        ant.CarriedItem = egg;
                        world.Eggs.Remove(egg);
                        return;
                    }
                }
            }
            else
            {
                // ToDO:
                // Towards(ant, world.SafeChamberX, world.SafeChamberY, world); // Предполагаем безопасную комнату
            }
        }

        private static double DistanceTo(Entity a, Entity b)
        {
            return Math.Sqrt(Math.Pow(a.Position.X - b.Position.X, 2) + Math.Pow(a.Position.Y - b.Position.Y, 2));
        }
        
        // Случайное движение
        public static void MoveRandomly(Random random, World world, Ant ant = null)
        {
            if (ant == null) return;
            var dx = random.Next(-ant.Speed, ant.Speed + 1);
            var dy = random.Next(-ant.Speed, ant.Speed + 1);
            var newX = ant.Position.X + dx;
            var newY = ant.Position.Y + dy;
            if (newX >= 0 && newX < world.Width && newY >= 0 && newY < world.Height)
            {
                ant.Position = new IEntity.Point(newX, newY);
            }
        }
        
        public static void MoveRandomly(Random random, World world, Enemy enemy = null)
        {
            if (enemy == null) return;
            var dx = random.Next(-enemy.Speed, enemy.Speed + 1);
            var dy = random.Next(-enemy.Speed, enemy.Speed + 1);
            var newX = enemy.Position.X + dx;
            var newY = enemy.Position.Y + dy;
            if (newX >= 0 && newX < world.Width && newY >= 0 && newY < world.Height)
            {
                enemy.Position = new IEntity.Point(newX, newY);
            }
        }
    }
}