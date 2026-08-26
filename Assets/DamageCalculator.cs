using System;

namespace DiceRoguelike.Gameplay.Combat
{
    public readonly struct DamageInput
    {
        public int RawDamage { get; }
        public int Defense { get; }
        public int SkillModifier { get; }
        public bool Critical { get; }
        public int CriticalMultiplier { get; }

        public DamageInput(int rawDamage, int defense, int skillModifier = 0, bool critical = false, int criticalMultiplier = 2)
        {
            if (rawDamage < 0) throw new ArgumentOutOfRangeException(nameof(rawDamage));
            if (defense < 0) throw new ArgumentOutOfRangeException(nameof(defense));
            if (skillModifier < 0) throw new ArgumentOutOfRangeException(nameof(skillModifier));
            if (criticalMultiplier < 1) throw new ArgumentOutOfRangeException(nameof(criticalMultiplier));

            RawDamage = rawDamage;
            Defense = defense;
            SkillModifier = skillModifier;
            Critical = critical;
            CriticalMultiplier = criticalMultiplier;
        }
    }

    public static class DamageCalculator
    {
        public static int Calculate(DamageInput input)
        {
            var scaled = input.RawDamage + input.SkillModifier;
            if (input.Critical)
            {
                scaled *= input.CriticalMultiplier;
            }

            return Math.Max(0, scaled - input.Defense);
        }
    }
}
