namespace DiceRoguelike.Game.Domain
{
    public interface IRandomSource
    {
        int NextInt(int minInclusive, int maxExclusive);
    }

    public sealed class SeededRandom : IRandomSource
    {
        private readonly System.Random random;

        public SeededRandom(int seed)
        {
            random = new System.Random(seed);
        }

        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (minInclusive >= maxExclusive)
                throw new System.ArgumentOutOfRangeException(nameof(minInclusive));

            return random.Next(minInclusive, maxExclusive);
        }
    }

    public readonly struct DiceRoll
    {
        public int Sides { get; }
        public int Value { get; }

        public DiceRoll(int sides, int value)
        {
            if (sides < 1)
                throw new System.ArgumentOutOfRangeException(nameof(sides));
            if (value < 1 || value > sides)
                throw new System.ArgumentOutOfRangeException(nameof(value));

            Sides = sides;
            Value = value;
        }
    }

    public sealed class Dice
    {
        private readonly IRandomSource random;

        public Dice(IRandomSource random)
        {
            this.random = random ?? throw new System.ArgumentNullException(nameof(random));
        }

        public DiceRoll Roll(int sides)
        {
            if (sides < 1)
                throw new System.ArgumentOutOfRangeException(nameof(sides));

            return new DiceRoll(sides, random.NextInt(1, sides + 1));
        }
    }
}
