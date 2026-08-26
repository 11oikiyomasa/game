namespace DiceRoguelike.Core
{
    public interface IRng
    {
        int NextInt(int minInclusive, int maxExclusive);
        float NextFloat();
    }
}
