using System;

namespace AntAlife.Domain
{
    public class Enemy : Entity
    {
        public EnemyType EnemyType { get; set; }
        public int Speed { get; set; } // Скорость движения (клеток за тик)
        public int AttackRange { get; set; } // Дальность атаки
        public int Regeneration { get; set; } // Регенерация здоровья за тик

        public Enemy(Random random, int x, int y, EnemyType enemyType) 
            : base(random, GetMaxHp(enemyType), GetAttack(enemyType), GetDefense(enemyType), x, y)
        {
            EnemyType = enemyType;
            Speed = GetSpeed(enemyType);
            AttackRange = GetAttackRange(enemyType);
            Regeneration = GetRegeneration(enemyType);
        }

        // Нахождение ближайшего муравья
        public Ant FindNearestAnt(World world)
        {
            Ant nearest = null;
            var minDistance = double.MaxValue;
            foreach (var ant in world.Ants)
            {
                var dist = DistanceTo(ant);
                if (!(dist < minDistance)) continue;
                minDistance = dist;
                nearest = ant;
            }
            return nearest;
        }

        // Атака муравья
        public void AttackAnt(Ant ant, EnemyType enemyType)
        {
            ant.TakeDamage(GetAttack(enemyType), ant);
        }

        // Расстояние до другой сущности
        public double DistanceTo(Entity other)
        {
            return Math.Sqrt(Math.Pow(Position.X - other.Position.X, 2) + Math.Pow(Position.Y - other.Position.Y, 2));
        }

        public int MaxHp(EnemyType enemyType)
        {
            return GetMaxHp(enemyType);
        }

        // Характеристики по типу врага
        private static int GetMaxHp(EnemyType enemyType) => enemyType switch
        {
            EnemyType.AntLion => 150,
            EnemyType.Spider => 100,
            EnemyType.Wasp => 80,
            _ => 100
        };

        private static int GetAttack(EnemyType enemyType) => enemyType switch
        {
            EnemyType.AntLion => 25,
            EnemyType.Spider => 15,
            EnemyType.Wasp => 20,
            _ => 10
        };

        private static int GetDefense(EnemyType enemyType) => enemyType switch
        {
            EnemyType.AntLion => 10,
            EnemyType.Spider => 5,
            EnemyType.Wasp => 3,
            _ => 5
        };

        private static int GetSpeed(EnemyType enemyType) => enemyType switch
        {
            EnemyType.AntLion => 1,
            EnemyType.Spider => 3,
            EnemyType.Wasp => 5,
            _ => 2
        };

        private static int GetAttackRange(EnemyType enemyType) => enemyType switch
        {
            EnemyType.Wasp => 2, // Летает, атакует на расстоянии
            _ => 1
        };

        private static int GetRegeneration(EnemyType enemyType) => enemyType switch
        {
            EnemyType.Spider => 5, // Паук быстро восстанавливается
            _ => 0
        };

        public void TakeDamage(int damage)
        {
            Hp -= Math.Max(damage - Defense, 0); // Учитываем защиту
        }
    }
}