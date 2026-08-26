using NUnit.Framework;
using DiceRoguelike.Gameplay.Combat;

namespace DiceRoguelike.Tests
{
    public sealed class StatusEffectTests
    {
        [Test]
        public void PoisonDealsDirectDamageAndExpiresAfterDuration()
        {
            var target = new CombatantState("target", "Target", 20, 1, startingDefense: 10);
            target.StatusEffects.Apply(new StatusEffectDefinition(
                StatusEffectType.Poison,
                StatusTrigger.TurnStart,
                duration: 2,
                magnitude: 3,
                sourceId: "test"));

            Assert.That(target.StatusEffects.ResolveTurnStart(target), Is.EqualTo(3));
            Assert.That(target.Hp, Is.EqualTo(17));
            Assert.That(target.Defense, Is.EqualTo(10), "Damage over time must not consume defense.");

            Assert.That(target.StatusEffects.ResolveTurnStart(target), Is.EqualTo(3));
            Assert.That(target.Hp, Is.EqualTo(14));
            Assert.That(target.StatusEffects.Has(StatusEffectType.Poison), Is.False);
        }

        [Test]
        public void AddStacksIncreasesMagnitudeAndRefreshesDuration()
        {
            var target = new CombatantState("target", "Target", 30, 1);
            target.StatusEffects.Apply(new StatusEffectDefinition(
                StatusEffectType.Poison,
                StatusTrigger.TurnStart,
                duration: 1,
                magnitude: 2,
                sourceId: "test",
                stackingRule: StatusStackingRule.AddStacks));
            target.StatusEffects.Apply(new StatusEffectDefinition(
                StatusEffectType.Poison,
                StatusTrigger.TurnStart,
                duration: 2,
                magnitude: 3,
                sourceId: "test",
                stackingRule: StatusStackingRule.AddStacks));

            Assert.That(target.StatusEffects.Effects[0].Stacks, Is.EqualTo(2));
            Assert.That(target.StatusEffects.Effects[0].Magnitude, Is.EqualTo(5));
            Assert.That(target.StatusEffects.Effects[0].RemainingTurns, Is.EqualTo(2));
        }

        [Test]
        public void TurnEndStunBlocksTheNextEnemyTurnUntilItExpires()
        {
            var target = new CombatantState("target", "Target", 20, 1);
            target.StatusEffects.Apply(new StatusEffectDefinition(
                StatusEffectType.Stun,
                StatusTrigger.TurnEnd,
                duration: 1,
                magnitude: 0,
                sourceId: "test"));

            Assert.That(target.IsStunned, Is.True);
            target.StatusEffects.ResolveTurnEnd(target);
            Assert.That(target.IsStunned, Is.False);
        }
    }
}
