using System;
using System.Linq;
using AntAlife.Domain;
using AntAlife.Domain.Properties;

namespace AntAlife.Logic
{
    public class AntBrain : Brain
    {
        private readonly Ant _ant;

        public AntBrain(Ant ant) : base(ant)
        {
            _ant = ant;
        }

        public override void Act(World world, Random random)
        {
            if (_ant.Energy <= 0)
            {
                _ant.Rest();
                return;
            }

            switch (_ant.AntType)
            {
                case AntType.Queen:
                    if (_ant.Energy >= 20 && world.Eggs.Count < 10)
                    {
                        LayEgg(world, random);
                    }
                    else
                    {
                        MoveRandomly(world, random);
                    }
                    break;
                case AntType.Worker:
                    if (_ant.CarriedItem == null)
                        FindAndCarryFoodOrWater(world);
                    else
                        ReturnToNest(world);
                    break;
                case AntType.Soldier:
                    PatrolOrFight(world, random);
                    break;
                case AntType.Nurse:
                    ProtectEggs(world);
                    break;
                case AntType.Harvester:
                case AntType.Strategist:
                case AntType.Carrier:
                case AntType.Spy:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            _ant.Energy -= 1;
        }

        private void FindAndCarryFoodOrWater(World world)
        {
            foreach (var item in world.FoodItems.Where(item => DistanceTo(_ant, item) <= 1 && _ant.CarryCapacity >= item.Weight))
            {
                _ant.CarriedItem = item;
                world.FoodItems.Remove(item);
                LeaveScent(world);
                return;
            }

            MoveRandomly(world, new Random());
        }

        private void ReturnToNest(World world)
        {
            var nestX = world.NestX;
            var nestY = world.NestY;
            if (_ant.Position.X == nestX && _ant.Position.Y == nestY)
            {
                world.NestFood += _ant.CarriedItem.Weight;
                _ant.CarriedItem = null;
            }
            else
            {
                Towards(world, nestX, nestY);
                LeaveScent(world);
            }
        }

        private void LayEgg(World world, Random random)
        {
            _ant.SetEnergy(_ant, -20);
            var egg = new Egg(random, _ant.Position.X, _ant.Position.Y, ItemType.Egg);
            world.Eggs.Add(egg);
        }

        private void PatrolOrFight(World world, Random random)
        {
            foreach (var enemy in world.Enemies.Where(enemy => DistanceTo(_ant, enemy) <= _ant.PatrolRadius))
            {
                _ant.AttackEnemy(enemy);
                return;
            }

            MoveRandomly(world, random);
        }

        private void ProtectEggs(World world)
        {
            if (_ant.CarriedItem == null)
            {
                foreach (var egg in world.Eggs.Where(egg => DistanceTo(_ant, egg) <= 1))
                {
                    _ant.CarriedItem = egg;
                    world.Eggs.Remove(egg);
                    return;
                }
            }
            else
            {
                // Позже добавить движение к безопасной комнате
            }
        }

        private void LeaveScent(World world)
        {
            world.Grid[_ant.Position.X, _ant.Position.Y].Pheromone += _ant.ScentStrength;
        }
    }
}