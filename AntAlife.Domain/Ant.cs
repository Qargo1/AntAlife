using System;
using AntAlife.Domain.Properties;

namespace AntAlife.Domain
{
    public class Ant : Entity
    {
        public AntType AntType { get; set; } // Тип муравья
        public int Speed { get; set; } // Скорость (тиков на ход)
        private int CarryCapacity { get; set; } // Грузоподъёмность
        public int Energy { get; set; } // Текущая энергия
        private int MaxEnergy { get; set; } // Максимальная энергия
        public int ScentStrength { get; set; } // Сила запаха (для следов)
        public bool CanDig { get; set; } // Может ли копать
        public bool CanCarry { get; set; } // Может ли переносить
        public bool CanFight { get; set; } // Может ли сражаться
        public Item CarriedItem { get; set; } // Что несёт (еда, яйцо и т.д.)
        public int PatrolRadius { get; set; } // Радиус патрулирования (для солдат)
        public bool IsSick { get; set; }

        public Ant(Random random, int x, int y, AntType antType) : base(random, GetMaxHp(antType), GetAttack(antType), GetDefense(antType), x, y)
        {
            AntType = antType;
            Speed = GetSpeed(antType);
            CarryCapacity = GetCarryCapacity(antType);
            MaxEnergy = GetMaxEnergy(antType);
            Energy = MaxEnergy;
            ScentStrength = GetScentStrength(antType);
            CanDig = antType == AntType.Worker;
            CanCarry = antType == AntType.Worker || antType == AntType.Nurse;
            CanFight = antType == AntType.Soldier;
            PatrolRadius = antType == AntType.Soldier ? 3 : 0;
        }

        // Королева откладывает яйцо
        public void LayEgg(Random random, int x, int y, World world, ItemType itemType = ItemType.Egg)
        {
            Energy -= 20;
            var egg = new Egg(random, x, y, itemType);
            world.Eggs.Add(egg);
        }
        
        // Рабочий ищет еду или воду
        public void FindAndCarryFoodOrWater(World world, Ant ant)
        {
            foreach (var item in world.FoodItems)
            {
                if (DistanceTo(item) <= 1 && CarryCapacity >= item.Weight)
                {
                    CarriedItem = item;
                    world.FoodItems.Remove(item);
                    // ToDo: LeaveScent(world); // Оставляем феромоны
                    return;
                }
            }
        }
        
        private double DistanceTo(Entity other)
        {
            return Math.Sqrt(Math.Pow(Position.X - other.Position.X, 2) + Math.Pow(Position.Y - other.Position.Y, 2));
        }

        public void TakeDamage(int damage, Ant ant)
        {
            ant.Hp -= damage;
        }
        
        public void GetSick(Random random)
        {
            if (random.Next(0, 100) < 5) IsSick = true; // 5% шанс болезни
        }
        public void Heal(Ant nurse)
        {
            if (nurse.AntType == AntType.Nurse && DistanceTo(nurse) <= 1)
            {
                IsSick = false;
                nurse.Energy -= 10;
            }
        }

        // Атака врага
        public void AttackEnemy(Enemy enemy)
        {
            enemy.TakeDamage(Attack);
        }

        // Отдых для восстановления энергии
        public void Rest()
        {
            Energy += 10;
            if (Energy > MaxEnergy) Energy = MaxEnergy;
        }

        // ToDO
        // Оставляем запах на клетке
        // private void LeaveScent(World world)
        // {
        // world.Grid[X, Y].Scent += ScentStrength;
        // }

        // Характеристики по типу муравья
        private static int GetMaxHp(AntType type) => type switch
        {
            AntType.Queen => 200,
            AntType.Egg => 30,
            AntType.Worker => 50,
            AntType.Soldier => 100,
            AntType.Nurse => 60,
            _ => 50
        };

        private static int GetAttack(AntType type) => type switch
        {
            AntType.Soldier => 20,
            AntType.Egg => 0,
            _ => 5
        };

        private static int GetDefense(AntType type) => type switch
        {
            AntType.Soldier => 15,
            _ => 3
        };

        private static int GetSpeed(AntType type) => type switch
        {
            AntType.Soldier => 1,
            AntType.Egg => 0,
            AntType.Nurse => 2,
            _ => 3
        };

        private static int GetCarryCapacity(AntType type) => type switch
        {
            AntType.Worker => 10,
            AntType.Nurse => 5,
            _ => 0
        };

        private static int GetMaxEnergy(AntType type) => type switch
        {
            AntType.Worker => 100,
            AntType.Egg => 0,
            AntType.Soldier => 80,
            _ => 50
        };

        private static int GetScentStrength(AntType type) => type switch
        {
            AntType.Worker => 5,
            _ => 1
        };
    }
}