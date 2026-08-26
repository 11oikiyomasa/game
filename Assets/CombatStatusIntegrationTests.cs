using System;
using NUnit.Framework;
using DiceRoguelike.Core;
using DiceRoguelike.Gameplay.Combat;
using DiceRoguelike.Gameplay.Dice;

namespace DiceRoguelike.Tests
{
    public sealed class CombatStatusIntegrationTests
    {
        [Test]
        public void SkillAppliesBurnAndBurnTicksAtEnemyTurnStart()
        {
            var engine = new CombatEngine(
                new CombatantState("hero", "Hero", 30, 5),
                new CombatantState("enemy", "Enemy", 20, 1),
                new SequenceRng(1, 1),
                new Dice(2));

            var result = engine.ExecutePlayerAction(CombatActionType.Skill);

            Assert.That(result.Damage, Is.EqualTo(8));
            Assert.That(result.StatusDamage, Is.EqualTo(2));
            Assert.That(engine.Enemy.Hp, Is.EqualTo(10));
            Assert.That(engine.Enemy.StatusEffects.Has(StatusEffectType.Burn), Is.True);
        }

        [Test]
        public void ConfiguredCriticalChanceAndMultiplierAreUsedByCombat()
        {
            var engine = new CombatEngine(
                new CombatantState("hero", "Hero", 30, 5, critChancePercent: 100, critMultiplier: 3),
                new CombatantState("enemy", "Enemy", 40, 1),
                new SequenceRng(1, 0, 1),
                new Dice(2));

            var result = engine.ExecutePlayerAction(CombatActionType.Attack);

            Assert.That(result.Critical, Is.True);
            Assert.That(result.Damage, Is.EqualTo(18));
        }

        [Test]
        public void StunPreventsEnemyActionForTheAffectedTurn()
        {
            var engine = new CombatEngine(
                new CombatantState("hero", "Hero", 30, 5),
                new CombatantState("enemy", "Enemy", 20, 8),
                new SequenceRng(1, 1),
                new Dice(2));
            engine.Enemy.StatusEffects.Apply(new StatusEffectDefinition(
                StatusEffectType.Stun,
                StatusTrigger.TurnEnd,
                duration: 1,
                magnitude: 0,
                sourceId: "test"));

            var before = engine.Player.Hp;
            engine.ExecutePlayerAction(CombatActionType.Attack);

            Assert.That(engine.Player.Hp, Is.EqualTo(before));
            Assert.That(engine.Enemy.StatusEffects.Has(StatusEffectType.Stun), Is.False);
        }

        private sealed class SequenceRng : IRng
        {
            private readonly int[] _values;
            private int _index;

            public SequenceRng(params int[] values)
            {
                _values = values ?? throw new ArgumentNullException(nameof(values));
                if (_values.Length == 0) throw new ArgumentException("At least one RNG value is required.", nameof(values));
            }

            public int NextInt(int minInclusive, int maxExclusive)
            {
                if (_index >= _values.Length) throw new InvalidOperationException("Test RNG sequence was exhausted.");
                var value = _values[_index++];
                if (value < minInclusive || value >= maxExclusive)
                    throw new InvalidOperationException("Test RNG value falls outside the requested range.");
                return value;
            }

            public float NextFloat() => 0.5f;
        }
    }
}
