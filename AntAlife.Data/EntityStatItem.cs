using System;
using System.Collections.Generic;
using System.Collections.ObjectModel; // Для ObservableCollection
using System.ComponentModel;          // Для INotifyPropertyChanged
using System.Linq;
using System.Runtime.CompilerServices; // Для CallerMemberName
using System.Windows.Threading;      // Для Dispatcher
using AntAlife.Domain;
using AntAlife.Domain.Enums;       // Для AntType, AntState и т.д.
using AntAlife.Logic;             // Для AntBrain

// Класс-обертка для отображения сущности в DataGrid
namespace AntAlife.Data
{
    public class EntityStatItem
    {
        public string Type { get; set; }
        public Guid Id { get; set; }
        public string State { get; set; }
        public string Position { get; set; }
        public int HP { get; set; }
        public int MaxHP { get; set; }
        public int Energy { get; set; }
        public int MaxEnergy { get; set; }
        public string CarriedItem { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
    }

    public class StatsViewModel : INotifyPropertyChanged
    {
        // --- INotifyPropertyChanged Implementation ---
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // --- Свойства для привязки к UI ---
        private int _tick;

        public int Tick
        {
            get => _tick;
            set
            {
                _tick = value;
                OnPropertyChanged();
            }
        }

        private int _antCount;

        public int AntCount
        {
            get => _antCount;
            set
            {
                _antCount = value;
                OnPropertyChanged();
            }
        }

        private int _enemyCount;

        public int EnemyCount
        {
            get => _enemyCount;
            set
            {
                _enemyCount = value;
                OnPropertyChanged();
            }
        }

        private int _eggCount;

        public int EggCount
        {
            get => _eggCount;
            set
            {
                _eggCount = value;
                OnPropertyChanged();
            }
        }

        private int _foodItemCount;

        public int FoodItemCount
        {
            get => _foodItemCount;
            set
            {
                _foodItemCount = value;
                OnPropertyChanged();
            }
        }

        private int _nestFood;

        public int NestFood
        {
            get => _nestFood;
            set
            {
                _nestFood = value;
                OnPropertyChanged();
            }
        }

        private bool _isNestBuilt;

        public bool IsNestBuilt
        {
            get => _isNestBuilt;
            set
            {
                _isNestBuilt = value;
                OnPropertyChanged();
            }
        }

        // Коллекция для DataGrid (ObservableCollection сама уведомляет UI об изменениях списка)
        public ObservableCollection<EntityStatItem> Entities { get; } = new ObservableCollection<EntityStatItem>();

        private readonly World _world;
        private readonly WorldUpdater _worldUpdater; // Нужен для получения состояния мозга

        // Конструктор
        public StatsViewModel(World world, WorldUpdater worldUpdater)
        {
            _world = world;
            _worldUpdater = worldUpdater; // Сохраняем ссылку
            UpdateStats(); // Первоначальное обновление
        }

        // Метод для обновления всех данных
        public void UpdateStats()
        {
            if (_world == null || _worldUpdater == null) return;

            // Обновляем простые свойства
            // Tick можно брать из WorldUpdater или добавить в World
            Tick = _worldUpdater
                .CurrentTick; // Предполагаем, что в WorldUpdater есть public int CurrentTick { get { return Tick; } }
            AntCount = _world.Ants.Count;
            EnemyCount = _world.Enemies.Count;
            EggCount = _world.Eggs.Count;
            FoodItemCount = _world.FoodItems.Count;
            NestFood = _world.NestFood;
            IsNestBuilt = _world.NestIsExisting;

            // Обновляем список сущностей для DataGrid
            Entities.Clear(); // Очищаем старый список

            // Добавляем муравьев
            foreach (var ant in _world.Ants)
            {
                // Находим мозг этого муравья, чтобы получить состояние
                // Добавь в WorldUpdater публичное свойство или метод для доступа к _antBrains
                // public IEnumerable<AntBrain> AntBrains => _antBrains;
                var brain = _worldUpdater.AntBrains.FirstOrDefault(b => b.Entity == ant);
                var state = brain?.CurrentState.ToString() ?? "N/A"; // Получаем состояние из мозга

                Entities.Add(new EntityStatItem
                {
                    Type = $"Ant ({ant.AntType})",
                    Id = ant.Id,
                    State = state,
                    Position = $"{ant.Position.X},{ant.Position.Y}",
                    HP = ant.Hp,
                    MaxHP = Entity.MaxHp,
                    Energy = ant.Energy,
                    MaxEnergy = ant.MaxEnergy,
                    CarriedItem = ant.CarriedItem?.ItemType.ToString() ?? "None", // Отображаем тип предмета
                    Attack = ant.Attack,
                    Defense = ant.Defense
                });
            }

            // Добавляем врагов (когда они появятся)
            foreach (var enemy in _world.Enemies)
            {
                // Точно так же можно получать состояние из EnemyBrain
                // var brain = _worldUpdater.EnemyBrains.FirstOrDefault(b => b.Entity == enemy);
                // string state = brain?.CurrentState.ToString() ?? "N/A";

                Entities.Add(new EntityStatItem
                {
                    Type = $"Enemy ({enemy.EnemyType})", // Добавь EnemyType в Enemy
                    Id = enemy.Id,
                    State = "Idle", // Placeholder
                    Position = $"{enemy.Position.X},{enemy.Position.Y}",
                    HP = enemy.Hp,
                    MaxHP = Entity.MaxHp,
                    Energy = enemy.Energy,
                    MaxEnergy = enemy.MaxEnergy,
                    CarriedItem = "N/A",
                    Attack = enemy.Attack,
                    Defense = enemy.Defense
                });
            }

            // Можно добавить яйца, еду и т.д., если нужно больше деталей
        }
    }

// Не забудь добавить нужные свойства/методы в WorldUpdater и AntBrain:
// В WorldUpdater.cs:
// public int CurrentTick => Tick;
// public IEnumerable<AntBrain> AntBrains => _antBrains;
// public IEnumerable<EnemyBrain> EnemyBrains => _enemyBrains; // Если нужно будет для врагов

// В AntBrain.cs:
// public AntState CurrentState => _currentState; // Публичное свойство для чтения состояния
}
