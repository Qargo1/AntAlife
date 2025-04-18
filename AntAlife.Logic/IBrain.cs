using System;
using AntAlife.Domain;

namespace AntAlife.Logic
{
    public interface IBrain
    {
        Entity Entity { get; }
        void Act(World world, Random random);
    }
}