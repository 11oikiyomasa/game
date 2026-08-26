using System;

namespace DiceRoguelike.Gameplay.Hero
{
    public enum HeroRole
    {
        Tank,
        Warrior,
        Assassin,
        Mage,
        Support,
        Healer,
        Controller,
        Marksman
    }

    public sealed class HeroDefinition
    {
        public string Id { get; }
        public string Name { get; }
        public HeroRole Role { get; }
        public int BaseHp { get; }
        public int BaseAttack { get; }
        public int BaseDefense { get; }
        public int BaseCritChancePercent { get; }
        public int BaseCritMultiplier { get; }

        public HeroDefinition(
            string id,
            string name,
            HeroRole role,
            int baseHp,
            int baseAttack,
            int baseDefense,
            int baseCritChancePercent = 0,
            int baseCritMultiplier = 2)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Hero id is required.", nameof(id));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Hero name is required.", nameof(name));
            if (baseHp <= 0) throw new ArgumentOutOfRangeException(nameof(baseHp));
            if (baseAttack < 0) throw new ArgumentOutOfRangeException(nameof(baseAttack));
            if (baseDefense < 0) throw new ArgumentOutOfRangeException(nameof(baseDefense));
            if (baseCritChancePercent < 0 || baseCritChancePercent > 100) throw new ArgumentOutOfRangeException(nameof(baseCritChancePercent));
            if (baseCritMultiplier < 1) throw new ArgumentOutOfRangeException(nameof(baseCritMultiplier));

            Id = id;
            Name = name;
            Role = role;
            BaseHp = baseHp;
            BaseAttack = baseAttack;
            BaseDefense = baseDefense;
            BaseCritChancePercent = baseCritChancePercent;
            BaseCritMultiplier = baseCritMultiplier;
        }
    }
}
