using System;

namespace DiceRoguelike.Core
{
    public sealed class SeededRng : IRng
    {
        private readonly Random _random;

        public SeededRng(int seed)
        {
            _random = new Random(seed);
        }

        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
            {
                throw new ArgumentOutOfRangeException(nameof(maxExclusive));
            }

            return _random.Next(minInclusive, maxExclusive);
        }

        public float NextFloat()
        {
            return (float)_random.NextDouble();
        }
    }
}
