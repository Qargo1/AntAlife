// В файле AntBrain.cs
using AntAlife.Domain.Enums; // Убедись, что enum AntState в этом пространстве имен
using AntAlife.Domain.Interfaces;
using AntAlife.Domain; // Для доступа к World, Ant, Cell и т.д.
using System;
using System.Collections.Generic;
using System.Linq; // Для FirstOrDefault и т.д.

namespace AntAlife.Logic
{
    public class AntBrain : Brain
    {
        private readonly Ant _ant;
        private const float PheromoneThreshold = 0.1f; // Минимальный уровень феромона для реакции
        
        private int _exploreDx = 0; // Направление по X (1, -1 или 0)
        private int _exploreDy = 0; // Направление по Y (1, -1 или 0)
        private bool _directionChosen = false; // Флаг, что направление уже выбрано
        private bool _isInitialExplorationPhase = true;

        public AntBrain(Ant ant) : base(ant)
        {
            _ant = ant;
            CurrentState = DetermineInitialState();
            if (_ant.AntType == AntType.Queen)
            {
                ChooseRandomDirection(new Random()); // Выбираем начальное направление
            }
        }
        
        // ---> Добавь это публичное свойство <---
        public AntState CurrentState
        {
            get;
            private set;
            // Приватный set не нужен, если мы меняем состояние только внутри AntBrain
            // private set; // Можно добавить, если нужно, чтобы другие классы в той же сборке могли менять, но лучше без него
        }

        private AntState DetermineInitialState()
        {
            // Простая логика для начала:
            return _ant.AntType switch
            {
                AntType.Queen => AntState.Idle, // Королева начинает спокойно
                AntType.Worker => AntState.Wandering, // Рабочий идет исследовать
                AntType.Soldier => AntState.Wandering, // Солдат тоже пока просто ходит
                AntType.Nurse => AntState.Wandering, // Нянька ищет яйца
                _ => AntState.Wandering, // Остальные по умолчанию
            };
        }
        
        // Метод для выбора случайного направления
        private void ChooseRandomDirection(Random random)
        {
            var directions = new List<(int dx, int dy)> { (0, 1), (0, -1), (1, 0), (-1, 0) };
            var chosenDir = directions[random.Next(directions.Count)];
            _exploreDx = chosenDir.dx;
            _exploreDy = chosenDir.dy;
            _directionChosen = true;
            // Console.WriteLine($"Queen's exploration direction set to: ({_exploreDx}, {_exploreDy})"); // Отладка
        }

        private (int x, int y)? ExploreMove(World world, Random random)
        {
            // Если направление еще не выбрано (на всякий случай, хотя должно быть в конструкторе)
            if (!_directionChosen && _ant.AntType == AntType.Queen)
            {
                ChooseRandomDirection(random);
            }

            if (Entity.Energy <= 0) return null;

            var x = Entity.Position.X;
            var y = Entity.Position.Y;

            // --- Получаем все валидные ходы ---
            var neighbors = new List<(int x, int y)>
            {
                (x, y - 1), (x, y + 1), (x - 1, y), (x + 1, y)
            };
            var validMoves = neighbors
                .Where(p => CanWalkOn(world, p.x, p.y))
                .ToList();
            
            if (validMoves.Count < 4) return validMoves[random.Next(validMoves.Count)];

            if (!validMoves.Any())
            {
                Console.WriteLine("ExploreMove: Stuck, no valid moves!"); // Отладка
                return null; // Совсем некуда идти
            }

            // --- Определяем предпочтительный ход ---
            (int x, int y) preferredMove = (x + _exploreDx, y + _exploreDy);
            var isPreferredValid = validMoves.Contains(preferredMove);

            (int x, int y) chosenMove; // Клетка, куда реально пойдем

            // --- Логика выбора с 70% шансом ---
            if (isPreferredValid && random.Next(100) < 70) // 70% шанс пойти в нужную сторону
            {
                chosenMove = preferredMove;
                // Console.WriteLine("ExploreMove: Taking preferred direction."); // Отладка
            }
            else
            {
                // Либо предпочтительный ход невалиден, либо не выпал шанс 70%
                // Выбираем случайно из ВСЕХ доступных ходов (включая предпочтительный, если он валиден,
                // чтобы не застрять, если он единственный)
                chosenMove = validMoves[random.Next(validMoves.Count)];
                 //Console.WriteLine("ExploreMove: Taking random valid direction."); // Отладка
            }

            // --- Делаем ход и открываем клетку ---
            Entity.Position = new IEntity.Point(chosenMove.x, chosenMove.y);
            RevealCell(world, chosenMove.x, chosenMove.y); // Используем твой метод RevealCell
            Entity.Energy -= 1;

            // Твой Console.WriteLine можно убрать или оставить для отладки
            // Console.WriteLine($"Found {validMoves.Count} valid moves. Moved to ({chosenMove.x},{chosenMove.y})");

            return null;
        }

        // Не забудь твой метод RevealCell, он идеален!
        private static void RevealCell(World world, int x, int y)
        {
            if (world.IsValidCoordinate(x, y) && world.Grid[x, y].CellType == CellType.Unexplored)
            {
                world.Grid[x, y].CellType = CellType.Ground;
            }
        }

        public override void Act(World world, Random random)
        {
            if (_ant == null)
            {
                Console.WriteLine("No ants, waiting...");
                return;
            }
            
            if (_ant.IsDead) return; // Проверяем, жив ли муравей

            // 1. Восстановление энергии, если нужно (можно сделать состоянием Resting)
            if (_ant.Energy <= 0)
            {
                _ant.Rest(); // Пока просто отдыхает на месте
                return;
            }

            // 2. Принятие решения о следующем состоянии (ключевой момент!)
            DecideNextState(world, random);

            // 3. Выполнение действия в зависимости от текущего состояния
            ExecuteCurrentStateAction(world, random);

            // 4. Потребление энергии (можно сделать разным для разных действий)
            _ant.ConsumeEnergy(1); // Используем метод муравья для траты энергии
        }

        // --- Логика принятия решений (пока очень простая) ---
        private void DecideNextState(World world, Random random)
        {
            // Приоритеты: Опасность > Голод/Работа > Обычное поведение

            // TODO: Проверить опасность (враги рядом, феромон опасности) -> Fleeing/Fighting

            switch (_ant.AntType)
            {
                // Если рабочий и несет что-то -> ReturningToNest (или CarryingItem)
                case AntType.Worker when _ant.CarriedItem != null:
                    CurrentState = AntState.ReturningToNest; // Или CarryingItem, если хотим разделить
                    return;
                // Если рабочий и голоден (добавить свойство IsHungry?) -> SearchingForFood
                // if (_ant.AntType == AntType.Worker && _ant.IsHungry)
                // {
                //     _currentState = AntState.SearchingForFood;
                //     return;
                // }
                // Если рабочий и чувствует сильный след еды -> FollowingFoodTrail
                case AntType.Worker when HasStrongPheromoneTrail(world, PheromoneType.Food):
                    CurrentState = AntState.FollowingFoodTrail;
                    return;
                case AntType.Queen:
                case AntType.Soldier:
                case AntType.Nurse:
                case AntType.Harvester:
                case AntType.Strategist:
                case AntType.Carrier:
                case AntType.Spy:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Если ничего из вышеперечисленного не сработало, и мы не в бою/бегстве:
            if (CurrentState == AntState.Idle || CurrentState == AntState.Wandering)
            {
                CurrentState = _ant.AntType switch
                {
                    // Можно добавить шанс перейти в активное состояние
                    // 20% шанс начать искать еду
                    AntType.Worker when random.Next(10) < 2 => AntState.SearchingForFood,
                    // Королева может захотеть отложить яйца
                    AntType.Queen when CanLayEgg(world) => AntState.Idle,
                    _ => AntState.Wandering
                };
            }
             // Если мы в состоянии поиска, но нашли еду -> переключаемся
             if (CurrentState == AntState.SearchingForFood && CheckForFoodNearby(world))
             {
                 // Логика взятия еды должна быть в ExecuteCurrentStateAction
                 // Здесь мы только планируем переход в ReturnToNest после взятия
             }

              // Если несем предмет и дошли до гнезда -> переключиться (например, в Wandering)
             if (CurrentState == AntState.ReturningToNest && IsAtNest(world) && _ant.CarriedItem != null)
             {
                 // Логика сброса предмета будет в ExecuteCurrentStateAction
             }
        }

        // --- Выполнение действий ---
        private void ExecuteCurrentStateAction(World world, Random random)
        {
            Console.WriteLine($"Queen state: {CurrentState}");
             switch (CurrentState)
             {
                case AntState.Idle:
                     // Королева откладывает яйца, если может
                    if (_ant.AntType == AntType.Queen && CanLayEgg(world))
                    {
                        LayEgg(world, random);
                        // После кладки можно остаться Idle или перейти в Wandering
                        // CurrentState = AntState.Wandering; // Например
                    }
                    // Иначе просто ничего не делаем
                    break;

                case AntState.Wandering:
                    // Console.WriteLine("Trying to move randomly...");
                    if (_ant.AntType == AntType.Queen && _isInitialExplorationPhase) // - добавить флаг? */ )
                    {
                        var centerOfNest = ExploreMove(world, random); // Используем направленное движение
                        if (centerOfNest != null)
                        {
                            _isInitialExplorationPhase = false;
                            CurrentState = AntState.WaitingForNest; // Ожидает, пока гнездо строится
                            world.StartNestConstruction(centerOfNest.Value.x, centerOfNest.Value.y); // Новый метод в World!
                            
                            LayEgg(world, random);
                        }
                    }
                    else
                    {
                        MoveRandomly(world, random); // Обычное случайное движение для остальных
                    }
                    // При блуждании можем оставлять слабый TrailPheromone
                    LeaveScent(world, PheromoneType.Trail, 0.5f); // Оставляем слабый след "я тут был"
                    break;

                case AntState.SearchingForFood:
                    // Ищем еду, предпочитая клетки с феромоном еды
                    if (!SearchForFoodUsingPheromones(world, random)) // Если феромонов нет
                    {
                        MoveRandomly(world, random); // Двигаемся случайно
                    }
                    // Проверяем, не нашли ли еду прямо здесь
                    CheckForFoodNearby(world);
                    break;

                case AntState.FollowingFoodTrail:
                    if (!FollowPheromoneTrail(world, random, PheromoneType.Food)) // Если след пропал
                    {
                        CurrentState = AntState.SearchingForFood; // Начинаем искать снова
                        MoveRandomly(world, random); // Делаем шаг в случайном направлении
                    }
                     // Проверяем, не дошли ли до еды
                     CheckForFoodNearby(world);
                    break;

                case AntState.ReturningToNest:
                     // Двигаемся к гнезду
                    var moved = Towards(world, world.NestX, world.NestY); // Используем Towards из Brain
                    // Если несем еду, оставляем сильный след еды
                    if (_ant.CarriedItem is Food) // Проверяем тип предмета
                    {
                         LeaveScent(world, PheromoneType.Food, 2.0f); // Сильный след еды
                    }
                    // Если дошли до гнезда и несем предмет - сбрасываем
                    if (IsAtNest(world) && _ant.CarriedItem != null)
                    {
                        DropItemInNest(world);
                        CurrentState = AntState.Wandering; // После сброса идем бродить
                    } else if (!moved) {
                        // Не смогли дойти до гнезда (застряли?)
                         CurrentState = AntState.Wandering; // Начинаем бродить
                    }
                    break;

                 // --- Добавим другие состояния позже ---
                 case AntState.CarryingItem: // Можно объединить с ReturningToNest, если несем в гнездо
                 case AntState.Fighting:
                 case AntState.Fleeing:
                 case AntState.TendingToEggs:
                     // Пока просто блуждаем, если попали в эти состояния
                     MoveRandomly(world, random);
                     break;
                 case AntState.WaitingForNest:
                     if (world.NestIsExisting) CurrentState = AntState.Idle;
                     Console.WriteLine("Queen: Nest is ready! Switching to Idle."); // Отладка
                     break;
                 case AntState.CarryingFood:
                 case AntState.FollowingDangerTrail:
                 case AntState.Digging:
                 case AntState.Building:
                 default:
                     MoveRandomly(world, random);
                     break;
             }
        }

        // --- Вспомогательные методы ---

        private bool CanLayEgg(World world)
        {
            // Королева может откладывать яйца, если достаточно энергии и не слишком много яиц в мире
            return _ant.AntType == AntType.Queen && _ant.Energy >= 50 && world.Eggs.Count < _ant.MaxEggsInNest; // Добавь MaxEggsInNest в Ant
        }

        private void LayEgg(World world, Random random)
        {
            _ant.ConsumeEnergy(30); // Кладка тратит много энергии
            var egg = new Egg(random, _ant.Position.X, _ant.Position.Y); // Используем конструктор Egg
            world.Eggs.Add(egg);
            // Возможно, королева должна оставаться Idle некоторое время после кладки
        }

        // Проверяем феромоны вокруг (возвращает true, если есть след сильнее порога)
        private bool HasStrongPheromoneTrail(World world, PheromoneType type)
        {
             var pos = _ant.Position;
             for (var dx = -1; dx <= 1; dx++) {
                 for (var dy = -1; dy <= 1; dy++) {
                     if (dx == 0 && dy == 0) continue;
                     var checkX = pos.X + dx;
                     var checkY = pos.Y + dy;
                     if (!CanWalkOn(world, checkX, checkY)) continue; // Нужен метод в World или Brain
                     var level = GetPheromoneLevel(world.Grid[checkX, checkY], type);
                     if (level > PheromoneThreshold * 2) // Ищем след сильнее обычного
                     {
                         return true;
                     }
                 }
             }
             return false;
        }


        // Ищем еду, предпочитая клетки с феромоном (возвращает true, если был сделан ход по феромону)
        private bool SearchForFoodUsingPheromones(World world, Random random)
        {
             var bestMove = FindBestPheromoneMove(world, random, PheromoneType.Food);

             if (!bestMove.HasValue) return false; // Феромонов нет
             _ant.Position = new IEntity.Point(bestMove.Value.x, bestMove.Value.y);
             // Можно ослабить феромон, по которому прошли?
             // WeakenPheromone(world, bestMove.Value.x, bestMove.Value.y, PheromoneType.Food);
             return true; // Двинулись по феромону
        }

        // Идем по следу феромона (возвращает true, если был сделан ход)
         private bool FollowPheromoneTrail(World world, Random random, PheromoneType type)
        {
             var bestMove = FindBestPheromoneMove(world, random, type);

             if (!bestMove.HasValue) return false; // След пропал или не найден
             _ant.Position = new IEntity.Point(bestMove.Value.x, bestMove.Value.y);
             // Ослабляем феромон, по которому прошли, чтобы он испарялся
             WeakenPheromone(world, bestMove.Value.x, bestMove.Value.y, type, 0.5f);
             return true; // Двинулись по следу
        }

        // Находит лучшую соседнюю клетку по уровню феромона
        private (int x, int y)? FindBestPheromoneMove(World world, Random random, PheromoneType type)
        {
            var currentPos = _ant.Position;
            var bestPheromone = PheromoneThreshold; // Ищем значения выше порога
            var bestMoves = new List<(int x, int y)>(); // Может быть несколько одинаково хороших клеток

            for (var dx = -1; dx <= 1; dx++)
            {
                for (var dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    var checkX = currentPos.X + dx;
                    var checkY = currentPos.Y + dy;

                    if (!CanWalkOn(world, checkX, checkY)) continue; // Нужен метод IsValidAndWalkable в World
                    var pheromoneLevel = GetPheromoneLevel(world.Grid[checkX, checkY], type);

                    if (pheromoneLevel > bestPheromone)
                    {
                        bestPheromone = pheromoneLevel;
                        bestMoves.Clear(); // Нашли лучше - очищаем список
                        bestMoves.Add((checkX, checkY));
                    }
                    else if (Math.Abs(pheromoneLevel - bestPheromone) < 0.01f && pheromoneLevel > PheromoneThreshold) // Если такой же сильный
                    {
                        bestMoves.Add((checkX, checkY));
                    }
                }
            }

            if (bestMoves.Any())
            {
                // Если есть несколько лучших - выбираем случайно
                return bestMoves[random.Next(bestMoves.Count)];
            }
            return null; // Не нашли подходящих клеток
        }


        // Оставляем запах (нужно указать тип и силу)
        private void LeaveScent(World world, PheromoneType type, float strength)
        {
             if (_ant.Position.X < 0 || _ant.Position.X >= world.Width || _ant.Position.Y < 0 || _ant.Position.Y >= world.Height) return; // Проверка границ

             var currentCell = world.Grid[_ant.Position.X, _ant.Position.Y];
             var currentLevel = GetPheromoneLevel(currentCell, type);
             var newLevel = Math.Min(currentLevel + strength, 10f); // Ограничиваем макс. уровень феромона

             SetPheromoneLevel(currentCell, type, newLevel);
        }

        // Ослабляем феромон в клетке
        private static void WeakenPheromone(World world, int x, int y, PheromoneType type, float amount)
        {
             if (x < 0 || x >= world.Width || y < 0 || y >= world.Height) return;
             var cell = world.Grid[x, y];
             var currentLevel = GetPheromoneLevel(cell, type);
             var newLevel = Math.Max(0, currentLevel - amount); // Не уходим в минус
             SetPheromoneLevel(cell, type, newLevel);
        }


        // Получаем уровень конкретного феромона
        private static float GetPheromoneLevel(Cell cell, PheromoneType type)
        {
             return type switch {
                 PheromoneType.Food => cell.FoodPheromone,
                 PheromoneType.Danger => cell.DangerPheromone,
                 PheromoneType.Trail => cell.TrailPheromone,
                 _ => 0f
             };
        }

        // Устанавливаем уровень конкретного феромона
        private static void SetPheromoneLevel(Cell cell, PheromoneType type, float value)
        {
             switch (type) {
                 case PheromoneType.Food: cell.FoodPheromone = value; break;
                 case PheromoneType.Danger: cell.DangerPheromone = value; break;
                 case PheromoneType.Trail: cell.TrailPheromone = value; break;
                 default:
                     throw new ArgumentOutOfRangeException(nameof(type), type, null);
             }
        }

        // Проверяем, есть ли еда рядом и берем ее
        private bool CheckForFoodNearby(World world)
        {
            // Ищем еду в текущей клетке или соседних
            var foodItem = world.FoodItems.FirstOrDefault(food =>
                Math.Abs(food.Position.X - _ant.Position.X) <= 1 && // В радиусе 1 клетки
                Math.Abs(food.Position.Y - _ant.Position.Y) <= 1 &&
                _ant.CanAntCarry(food)); // Проверяем, может ли муравей унести (нужен метод CanCarry в Ant)

            if (foodItem == null) return false;
            // Взять еду
            _ant.CarriedItem = foodItem;
            world.FoodItems.Remove(foodItem); // Убираем еду из мира
            CurrentState = AntState.ReturningToNest; // Меняем состояние на возврат
            return true;
        }

         // Проверяем, находимся ли мы в гнезде
         private bool IsAtNest(World world)
         {
             // Пока просто проверяем координаты X, Y. Позже можно проверять тип клетки Chamber
             return _ant.Position.X == world.NestX && _ant.Position.Y == world.NestY;
         }

         // Сбрасываем предмет в гнезде
         private void DropItemInNest(World world)
         {
             if (_ant.CarriedItem is Food food) // Если это еда
             {
                 world.NestFood += food.Weight; // Добавляем еду в запасы гнезда
             }
             // Если несли что-то другое (яйцо, ресурс) - другая логика
             _ant.CarriedItem = null; // Освобождаем "руки"
         }
    }
}