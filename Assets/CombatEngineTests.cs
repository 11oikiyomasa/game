using NUnit.Framework;
using DiceRoguelike.Core;
using DiceRoguelike.Gameplay.Combat;
using DiceRoguelike.Gameplay.Dice;

namespace DiceRoguelike.Tests
{
    public sealed class CombatEngineTests
    {
        [Test]
        public void AttackDealsDamageAndEnemyCanBeDefeated()
        {
            var engine = new CombatEngine(
                new CombatantState("hero", "Hero", 20, 5),
                new CombatantState("enemy", "Enemy", 6, 1),
                new SeededRng(7),
                new Dice(1 + 1));

            var result = engine.ExecutePlayerAction(CombatActionType.Attack);

            Assert.That(result.Damage, Is.GreaterThan(0));
            Assert.That(engine.Enemy.Hp, Is.LessThan(6));
        }

        [Test]
        public void DefendReducesIncomingDamage()
        {
            var engine = new CombatEngine(
                new CombatantState("hero", "Hero", 20, 1),
                new CombatantState("enemy", "Enemy", 20, 4),
                new SeededRng(9),
                new Dice(2));

            engine.ExecutePlayerAction(CombatActionType.Defend);
            var hpAfterDefense = engine.Player.Hp;

            Assert.That(hpAfterDefense, Is.GreaterThan(0));
            Assert.That(hpAfterDefense, Is.LessThanOrEqualTo(20));
        }
    }
}
