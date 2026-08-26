using System;
using DiceRoguelike.Core;

namespace DiceRoguelike.Gameplay.Dice
{
    public readonly struct Dice
    {
        public int Sides { get; }

        public Dice(int sides)
        {
            if (sides < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(sides), "A die must have at least two sides.");
            }

            Sides = sides;
        }

        public int Roll(IRng rng)
        {
            if (rng == null)
            {
                throw new ArgumentNullException(nameof(rng));
            }

            return rng.NextInt(1, Sides + 1);
        }
    }
}
