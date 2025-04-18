using System;
using System.Collections.Generic;
using System.Linq;
using AntAlife.Domain;

namespace AntAlife.Logic
{
    public class WorldUpdater
    {
        private readonly Random _random = new Random();
        private readonly World _world;
        private readonly List<AntBrain> _antBrains;
        private readonly List<EnemyBrain> _enemyBrains;
        private int Tick { get; set; }
        public event Action WorldUpdated;

        public WorldUpdater(World world)
        {
            _world = world;
            _antBrains = world.Ants.Select(ant => new AntBrain(ant)).ToList();
            _enemyBrains = world.Enemies.Select(enemy => new EnemyBrain(enemy)).ToList();
            Tick = 0;
        }

        public void Update()
        {
            if (_world.Grid == null) return;
            Tick++;

            // Логика для яиц
            foreach (var egg in _world.Eggs)
            {
                egg.HatchTime--;
            }
            var hutchedEggs = _world.Eggs.Where(egg => egg.HatchTime <= 0).ToList();
            foreach (var egg in hutchedEggs)
            {
                _world.Eggs.Remove(egg);
                AntType type;
                if (_world.Ants.All(a => a.AntType != AntType.Queen))
                    type = AntType.Queen;
                else if (_world.Ants.All(a => a.AntType != AntType.Worker))
                    type = AntType.Worker;
                else if (_world.Ants.Count(a => a.AntType == AntType.Soldier) < 3)
                    type = AntType.Soldier;
                else if (_world.Ants.Count(a => a.AntType == AntType.Nurse) < 4)
                    type = AntType.Nurse;
                else
                    type = AntType.Worker; // По умолчанию
                var newAnt = new Ant(_random, egg.Position.X, egg.Position.Y, type);
                _world.Ants.Add(newAnt);
                _antBrains.Add(new AntBrain(newAnt));
            }

            // Логика для феромонов
            for (int x = 0; x < _world.Grid.GetLength(0); x++)
            for (int y = 0; y < _world.Grid.GetLength(1); y++)
                if (_world.Grid[x, y].Pheromone > 0)
                    _world.Grid[x, y].Pheromone -= 0.1f;

            // Удаление мёртвых муравьёв
            var deadAnts = _antBrains.Where(brain => brain.Entity.Hp <= 0).ToList();
            foreach (var dead in deadAnts)
            {
                _world.Ants.Remove((Ant)dead.Entity);
                _antBrains.Remove(dead);
            }

            // Действия муравьёв
            foreach (var brain in _antBrains)
            {
                brain.Act(_world, _random);
            }

            // Действия врагов
            foreach (var brain in _enemyBrains)
            {
                brain.Act(_world, _random);
            }

            WorldUpdated?.Invoke();
        }
    }
}