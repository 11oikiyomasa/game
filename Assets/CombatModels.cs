using System;

namespace DiceRoguelike.Gameplay.Combat
{
    public enum CombatActionType
    {
        Attack,
        Defend,
        Skill
    }

    public sealed class CombatantState
    {
        public string Id { get; }
        public string Name { get; }
        public int MaxHp { get; }
        public int Hp { get; private set; }
        public int Attack { get; }
        public int Defense { get; private set; }
        public int CritChancePercent { get; }
        public int CritMultiplier { get; }
        public StatusEffectContainer StatusEffects { get; }

        public bool IsAlive => Hp > 0;
        public bool IsStunned => StatusEffects.Has(StatusEffectType.Stun);

        public CombatantState(
            string id,
            string name,
            int maxHp,
            int attack,
            int startingHp = -1,
            int startingDefense = 0,
            int critChancePercent = 0,
            int critMultiplier = 2)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Combatant id is required.", nameof(id));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Combatant name is required.", nameof(name));
            if (maxHp <= 0) throw new ArgumentOutOfRangeException(nameof(maxHp));
            if (attack < 0) throw new ArgumentOutOfRangeException(nameof(attack));
            if (startingHp == -1) startingHp = maxHp;
            if (startingHp <= 0 || startingHp > maxHp) throw new ArgumentOutOfRangeException(nameof(startingHp));
            if (startingDefense < 0) throw new ArgumentOutOfRangeException(nameof(startingDefense));
            if (critChancePercent < 0 || critChancePercent > 100) throw new ArgumentOutOfRangeException(nameof(critChancePercent));
            if (critMultiplier < 1) throw new ArgumentOutOfRangeException(nameof(critMultiplier));

            Id = id;
            Name = name;
            MaxHp = maxHp;
            Hp = startingHp;
            Attack = attack;
            Defense = startingDefense;
            CritChancePercent = critChancePercent;
            CritMultiplier = critMultiplier;
            StatusEffects = new StatusEffectContainer();
        }

        public int TakeDamage(int rawDamage, int skillModifier = 0, bool critical = false)
        {
            if (rawDamage < 0) throw new ArgumentOutOfRangeException(nameof(rawDamage));
            var actual = DamageCalculator.Calculate(
                new DamageInput(rawDamage, Defense, skillModifier, critical, CritMultiplier));
            Hp = Math.Max(0, Hp - actual);
            Defense = 0;
            return actual;
        }

        public int TakeDirectDamage(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            Hp = Math.Max(0, Hp - amount);
            return amount;
        }

        public void Defend(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            Defense += amount;
        }
    }

    public readonly struct CombatActionResult
    {
        public CombatActionType Action { get; }
        public int Damage { get; }
        public int StatusDamage { get; }
        public bool Critical { get; }
        public bool CombatEnded { get; }
        public bool PlayerWon { get; }

        public CombatActionResult(
            CombatActionType action,
            int damage,
            int statusDamage,
            bool critical,
            bool combatEnded,
            bool playerWon)
        {
            Action = action;
            Damage = damage;
            StatusDamage = statusDamage;
            Critical = critical;
            CombatEnded = combatEnded;
            PlayerWon = playerWon;
        }
    }
}
