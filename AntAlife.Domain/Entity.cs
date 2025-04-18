using System;

namespace AntAlife.Domain
{
    public abstract class Entity : IEntity
    {
        public Guid Id { get; set; }
        public int Hp { get; set; }
        public int Hunger { get; set; }
        public int Thirst { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public EntityState State { get; set; }
        public IEntity.Point Position { get; set; }
        public int Energy { get; set; } // Текущая энергия
        public int Speed { get; set; } // Скорость (тиков на ход)
        public int MaxEnergy { get; set; } // Максимальная энергия


        
        protected Entity(Random random, int maxHp, int attack, int defense, int x, int y)
        {
            Id = Guid.NewGuid();
            Hp = random.Next(30, maxHp);
            Hunger = 0;
            Thirst = 0;
            Attack = attack;
            Defense = defense;
            State = EntityState.Exploring;
            Position = new IEntity.Point() { X = x, Y = y };
        }

        protected Entity(Random random, int maxHp, int x, int y)
        {
            Id = Guid.NewGuid();
            Hp = random.Next(30, maxHp);
            Hunger = 0;
            Thirst = 0;
            Attack = 0;
            Defense = 0;
            State = EntityState.Exploring;
            Position = new IEntity.Point() { X = x, Y = y };
        }

        protected Entity(int hp, int x, int y)
        {
            Id = Guid.NewGuid();
            Hp = hp;
            Hunger = 0;
            Thirst = 0;
            Attack = 0;
            Defense = 0;
            State = EntityState.Exploring;
            Position = new IEntity.Point() { X = x, Y = y };
        }
    }
}