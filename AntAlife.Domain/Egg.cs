using System;
using AntAlife.Domain.Properties;

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

        public Egg(int hp, int x, int y, ItemType itemType) : base(hp, x, y, itemType)
        {
            HatchTime = 100;
        }
    }
}