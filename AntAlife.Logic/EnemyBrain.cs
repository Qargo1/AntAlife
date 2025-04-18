using System;
using AntAlife.Domain;

namespace AntAlife.Logic
{
    public class EnemyBrain : Brain
    {
        private readonly Enemy _enemy;

        public EnemyBrain(Enemy enemy) : base(enemy)
        {
            _enemy = enemy;
        }

        public override void Act(World world, Random random)
        {
            _enemy.Hp += _enemy.Regeneration;
            if (_enemy.Hp > _enemy.MaxHp(_enemy.EnemyType))
                _enemy.Hp = _enemy.MaxHp(_enemy.EnemyType);

            var target = FindNearestAnt(world);
            if (target != null && DistanceTo(_enemy, target) <= _enemy.AttackRange)
            {
                _enemy.AttackAnt(target, _enemy.EnemyType);
            }
            else
            {
                MoveRandomly(world, random);
            }
        }

        private Ant FindNearestAnt(World world)
        {
            Ant nearest = null;
            var minDistance = double.MaxValue;
            foreach (var ant in world.Ants)
            {
                var dist = DistanceTo(_enemy, ant);
                if (!(dist < minDistance)) continue;
                minDistance = dist;
                nearest = ant;
            }
            return nearest;
        }
    }
}