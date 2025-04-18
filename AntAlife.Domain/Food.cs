using System;
using AntAlife.Domain.Properties;

namespace AntAlife.Domain
{
    public class Food : Item
    {
        public Food(Random random, int maxHp, int x, int y, ItemType itemType, int attack = 0, int defense = 0) : base(random, maxHp, x, y, itemType, attack, defense)
        {
        }

        public Food(int x, int y, ItemType itemType = ItemType.Food, int hp = 100) : base(hp, x, y, itemType)
        {
        }
    }
}