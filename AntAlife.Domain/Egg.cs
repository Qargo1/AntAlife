using System;
using AntAlife.Domain.Enums;
using AntAlife.Domain.Interfaces;

namespace AntAlife.Domain
{
    public class Egg : Item
    {
        public int HatchTime { set; get; } // Для яйца
        
        public Egg(Random random, int x, int y, ItemType itemType, int maxHp = 100, int attack = 0, int defense = 0) : base(random, maxHp, x, y, itemType, attack, defense)
        {
            Hp = random.Next(maxHp);
            HatchTime = random.Next(70, 100);
        }

        public Egg(Random random, int x, int y, int hp = 40, ItemType itemType = ItemType.Egg) : base(hp, x, y, itemType)
        {
            Hp = random.Next(10, hp);
            HatchTime = 100;
        }
    }
}