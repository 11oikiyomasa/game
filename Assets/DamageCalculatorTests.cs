using NUnit.Framework;
using DiceRoguelike.Gameplay.Combat;

namespace DiceRoguelike.Tests
{
    public sealed class DamageCalculatorTests
    {
        [Test]
        public void DefenseMitigatesRawDamageInOneCentralizedFormula()
        {
            var damage = DamageCalculator.Calculate(new DamageInput(rawDamage: 10, defense: 3));
            Assert.That(damage, Is.EqualTo(7));
        }

        [Test]
        public void SkillModifierAndCriticalAreAppliedBeforeDefense()
        {
            var damage = DamageCalculator.Calculate(new DamageInput(rawDamage: 10, defense: 4, skillModifier: 2, critical: true));
            Assert.That(damage, Is.EqualTo(20));
        }

        [Test]
        public void DefenseCannotCreateNegativeDamage()
        {
            var damage = DamageCalculator.Calculate(new DamageInput(rawDamage: 2, defense: 20));
            Assert.That(damage, Is.EqualTo(0));
        }
    }
}
