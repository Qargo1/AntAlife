using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using AntAlife.Domain;
using AntAlife.Domain.Enums;

namespace AntAlife.Logic
{
    public class WorldUpdater
    {
        private readonly Random _random = new Random();
        private readonly World _world;
        public readonly List<AntBrain> AntBrains;
        public readonly List<EnemyBrain> EnemyBrains;
        private int Tick { get; set; }
        public int CurrentTick => Tick;

        public WorldUpdater(World world)
        {
            _world = world;
            AntBrains = world.Ants.Select(ant => new AntBrain(ant)).ToList();
            EnemyBrains = world.Enemies.Select(enemy => new EnemyBrain(enemy)).ToList();
            Tick = 0;
        }

        public void Update()
        {
            Tick++; // Счетчик времени симуляции
            
            if (_world.IsNestUnderConstruction)
            {
                UpdateNestConstruction(); // Вызываем новый метод
            }
            
            foreach (var antBrain in AntBrains)
            {
                antBrain.Act(_world, _random);
            }

            foreach (var egg in _world.Eggs)
            {
                egg.HatchTime--;
                if (egg.HatchTime <= 0) _world.Ants.Add(new Ant(_random, egg.Position.X, egg.Position.Y, AntType.Worker));
            }
            
            var hatchedEggs = _world.Eggs.Where(egg => egg.HatchTime <= 0).ToList();
            foreach (var egg in hatchedEggs) _world.Eggs.Remove(egg);
            
            // --- Обновление Мира (испарение феромонов) ---
            // UpdatePheromones();
            
            WorldUpdated?.Invoke();
        }
        
        private void UpdateNestConstruction()
        {
            if (!_world.IsNestUnderConstruction) return;

            int radius = _world.CurrentNestRadius;
            int targetRadius = _world.TargetNestRadius;

            if (radius <= targetRadius)
            {
                Console.WriteLine($"Updater: Building layer with radius {radius}");
                int centerX = _world.NestX;
                int centerY = _world.NestY;
                bool layerBuilt = false; // Флаг, что хоть одна клетка построена в этом слое

                // Строим периметр квадрата с полустороной 'radius'
                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        // Строим только сам периметр, а не весь квадрат заново
                        if (Math.Abs(dx) != radius && Math.Abs(dy) != radius)
                        {
                            continue; // Пропускаем внутренние клетки
                        }

                        int x = centerX + dx;
                        int y = centerY + dy;

                        if (_world.IsValidCoordinate(x, y))
                        {
                            // Определяем тип клетки
                            CellType targetType;
                            if (radius == 1) // Самый первый слой (3x3) - камера
                            {
                                 // Чтобы получить камеру 3x3 при radius=1, условие должно быть такое:
                                 if (Math.Abs(dx) <= 1 && Math.Abs(dy) <= 1) targetType = CellType.Chamber;
                                 else targetType = CellType.Soil; // Это не должно сработать, т.к. мы на периметре radius=1

                            } else if (radius == 2) // Второй слой (5x5) - внутренняя камера и внешний Soil
                            {
                                 if (Math.Abs(dx) <= 1 && Math.Abs(dy) <= 1) targetType = CellType.Chamber; // Центр 3x3
                                 else targetType = CellType.Soil; // Остальное на периметре 5x5
                            }
                            else // Внешние слои - все Soil
                            {
                                targetType = CellType.Soil;
                            }


                             // --- Упрощенная логика для старта ---
                             // Все слои, кроме самого центрального, будут Soil
                             // А центральный (3x3) - Chamber
                             if (radius <= 1) // radius 0 (не бывает) и 1 - это центр 3х3
                             {
                                  targetType = CellType.Chamber;
                             }
                             else
                             {
                                 targetType = CellType.Soil;
                             }


                            // Меняем тип клетки, ТОЛЬКО если она еще не исследована или земля
                            // Чтобы не перезатирать уже построенное
                             var currentCellType = _world.Grid[x,y].CellType;
                             if(currentCellType == CellType.Unexplored || currentCellType == CellType.Ground)
                             {
                                 _world.Grid[x, y].CellType = targetType;
                                 _world.Grid[x, y].Durability = (targetType == CellType.Chamber) ? 100 : 80;
                                 layerBuilt = true;
                             }
                        }
                    }
                }

                 // Если в этом слое ничего не построили (например, мир кончился),
                 // все равно увеличиваем радиус, чтобы не застрять.
                _world.IncrementNestRadius();

            }
            else // radius > targetRadius
            {
                // Стройка завершена
                _world.FinishNestConstruction();
                // Смена состояния королевы (через проверку флага в AntBrain)
            }
        }
        
        public event Action WorldUpdated;
    }
}