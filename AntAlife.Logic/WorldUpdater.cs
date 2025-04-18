using System;
using System.Linq;
using AntAlife.Domain;

namespace AntAlife.Logic
{
    public class WorldUpdater
    {
        private readonly Random _random = new Random();
        private readonly World _world;
        private int Tick { get; set; }

        public WorldUpdater(World world)
        {
            _world = world;
            Tick = 0;
        }

        public void Update()
        {
            if (_world.Grid == null) return;
            Tick++;
            // ToDo 0. Logic for AntQueen
            // 1. Logic for eggs
            if (_world.Eggs != null)
            {
                foreach (var egg in _world.Eggs)
                {
                    egg.HatchTime--;
                }
                var hutchedEggs = _world.Eggs.Where(egg => egg.HatchTime <= 0).ToList();
                foreach (var egg in hutchedEggs)
                {
                    _world.Eggs.Remove(egg);
                    // ToDo: add more precise logic - first queen! before everything. 
                    // then if there is no worker, soldier, nurse, then spawn worker, next soldier e.t.c
                    // then randomly, but no more then 3 soldiers and 4 nurses
                    _world.Ants.Add(new Ant(_random, egg.Position.X, egg.Position.Y, AntType.Queen));
                }
            }
            
            // 2. Logic for pheromones
            for (int x = 0; x < _world.Grid.GetLength(0); x++)
            for (int y = 0; y < _world.Grid.GetLength(1); y++)
                if (_world.Grid[x, y].Pheromone > 0)
                    _world.Grid[x, y].Pheromone -= 0.1f;
            
            // 3. Logic for ants
            if (_world.Ants == null) return;
            var deadAnts = _world.Ants.Where(ant => ant.Hp <= 0).ToList();
            foreach (var deadAnt in deadAnts) _world.Ants.Remove(deadAnt);
            foreach (var ant in _world.Ants)
            {
                if (ant.AntType == AntType.Worker && ant.State == EntityState.Carrying)
                {
                    IEntity.Point position = ant.Position;
                    _world.Grid[position.X, position.Y].Pheromone++;
                }

                ant.Hunger++;
                if (ant.Hunger > 40) ant.State = EntityState.Hungry;
                else if (ant.Hunger > 80)
                {
                    ant.Hp--;
                    ant.Attack--;
                    ant.Defense--;
                }
                else if (ant.Hunger > 100 && ant.Hp > 0)
                {
                    ant.Hp--;
                }

                // Move the ant
                Move.From(ant, _world, _random);
            }

            // 4. Logic for Enemies
            if (_world.Enemies == null) return;
            // Действие врага на каждом тике.
            // Восстанавливаем здоровье.
            foreach (var enemy in _world.Enemies)
            {
                enemy.Hp += enemy.Regeneration;
                if (enemy.Hp > enemy.MaxHp(enemy.EnemyType)) enemy.Hp = enemy.MaxHp(enemy.EnemyType);

                // Ищем ближайшего муравья
                Ant target = enemy.FindNearestAnt(_world);
                if (target != null && enemy.DistanceTo(target) <= enemy.AttackRange)
                {
                    enemy.AttackAnt(target, enemy.EnemyType);
                }
                else
                {
                    Move.MoveRandomly(_random, _world, enemy);
                }
            }

        }
    }
}