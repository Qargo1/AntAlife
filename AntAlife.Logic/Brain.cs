using System;
using System.Collections.Generic;
using System.Linq;
using AntAlife.Domain;
using AntAlife.Domain.Enums;
using AntAlife.Domain.Interfaces;

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
        
        protected virtual bool CanWalkOn(World world, int x, int y)
        {
            // Проверка координат
            if (!world.IsValidCoordinate(x, y))
            {
                // Console.WriteLine($"CanWalkOn: ({x},{y}) is INVALID coordinate.");
                return false;
            }

            // Тип клетки
            var cellType = world.Grid[x, y].CellType;
            bool result = false; // Переменная для хранения результата

            // Определяем результат в зависимости от типа Entity
            switch (Entity)
            {
                case Ant ant:
                    // --- Правила для Муравьев ---
                    switch (ant.AntType)
                    {
                        case AntType.Queen:
                            result = cellType == CellType.Ground || cellType == CellType.Chamber ||
                                     cellType == CellType.Tunnel || cellType == CellType.Exit ||
                                     cellType == CellType.Unexplored; // Королева ходит везде (пока)
                            break;
                        case AntType.Worker:
                            result = cellType == CellType.Ground || cellType == CellType.Chamber ||
                                     cellType == CellType.Tunnel || cellType == CellType.Exit;
                            break;
                        // Добавить case для других типов муравьев по мере необходимости
                        default:
                            // Поведение по умолчанию для других типов муравьев
                            result = cellType == CellType.Ground || cellType == CellType.Chamber ||
                                     cellType == CellType.Tunnel || cellType == CellType.Exit;
                            break;
                    }
                    break;

                case Enemy enemy:
                    // --- Правила для Врагов ---
                    // result = cellType == CellType.Ground || cellType == CellType.Unexplored;
                    result = false; // Пока враги не ходят
                    break;

                default:
                    // Неизвестный тип Entity
                    result = false;
                    break;
            }
            
            // Console.WriteLine($"CanWalkOn check: Entity={Entity?.GetType().Name ?? "null"}, Pos=({x},{y}), CellType={cellType}, Result={result}");

            return result; // Возвращаем вычисленный результат
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
                .Where(p => CanWalkOn(world, p.x, p.y))
                .ToList();

            if (!validMoves.Any()) return;

            var newPos = validMoves[random.Next(validMoves.Count)];
            Entity.Position = new IEntity.Point(newPos.x, newPos.y);
            Entity.Energy -= 1;
            
            Console.WriteLine($"Found {validMoves.Count} valid moves.");
        }
        
                 // Движение к цели (у тебя уже есть в Brain, можно его использовать)
         // Возвращает true, если ход был сделан, false если нет (достигли цели или застряли)
         protected bool Towards(World world, int targetX, int targetY)
         {
             if (Entity.Position.X == targetX && Entity.Position.Y == targetY) return false; // Уже на месте

             var dx = targetX - Entity.Position.X;
             var dy = targetY - Entity.Position.Y;

             // Приоритет движения по оси с большим расстоянием
             var moveX = 0;
             var moveY = 0;

             if (Math.Abs(dx) > Math.Abs(dy))
             {
                 moveX = Math.Sign(dx);
             } else if (Math.Abs(dy) > Math.Abs(dx))
             {
                  moveY = Math.Sign(dy);
             } else if (dx != 0) // Если расстояния равны
             {
                 moveX = Math.Sign(dx); // Пытаемся по X
                 // Можно добавить случайность выбора оси при равенстве
             } else if (dy != 0)
             {
                 moveY = Math.Sign(dy); // Пытаемся по Y
             }


             var nextX = Entity.Position.X + moveX;
             var nextY = Entity.Position.Y + moveY;

             // Пытаемся сначала по приоритетной оси
             if (CanWalkOn(world, nextX, nextY))
             {
                 Entity.Position = new IEntity.Point(nextX, nextY);
                 return true;
             }

             // Если по приоритетной не вышло, пробуем по другой оси (если есть движение по ней)
             if (moveX == 0 && moveY != 0) // Если приоритет был Y, а мы пробуем X
             {
                nextX = Entity.Position.X + Math.Sign(dx); // Используем исходный dx
                nextY = Entity.Position.Y;
             } else if (moveY == 0 && moveX != 0) // Если приоритет был X, а мы пробуем Y
             {
                 nextX = Entity.Position.X;
                 nextY = Entity.Position.Y + Math.Sign(dy); // Используем исходный dy
             } else {
                  // Если приоритет был смешанный или не удалось по основной оси
                  // Можно попробовать обе неосновные комбинации, или остановиться
                   return false; // Не смогли найти ход
             }

             if (!CanWalkOn(world, nextX, nextY)) return false; // Не нашли путь
             Entity.Position = new IEntity.Point(nextX, nextY);
             return true;

         }

        // Расстояние до другой сущности
        protected static double DistanceTo(Entity a, Entity b)
        {
            return Math.Sqrt(Math.Pow(a.Position.X - b.Position.X, 2) + Math.Pow(a.Position.Y - b.Position.Y, 2));
        }
    }
}