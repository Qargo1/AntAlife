using System;
using AntAlife.Domain.Enums;

namespace AntAlife.Domain.Interfaces
{
    public abstract class Item : Entity
    {
        public int Weight { get; set; }
        public float Amount { get; set;  }
        
        public ItemType ItemType { get; set; }
        
        public Item(Random random, int maxHp, int x, int y, ItemType itemType, int attack = 0, int defense = 0) : base(random, maxHp, attack, defense, x, y)
        {
            Position = new IEntity.Point(x, y);
            Amount = random.Next(10, maxHp);
            Weight = (int)Amount / 2;
            ItemType = itemType;
        }

        public Item(int hp, int x, int y, ItemType itemType) : base(hp, x, y)
        {
            Position = new IEntity.Point(x, y);
            Weight = hp / 2;
            Amount = hp;
            ItemType = itemType;
        }
    }
}