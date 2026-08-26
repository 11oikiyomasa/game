using NUnit.Framework;
using DiceRoguelike.Core;
using DiceRoguelike.Gameplay.Board;
using DiceRoguelike.Gameplay.Combat;
using DiceRoguelike.Gameplay.Dice;
using DiceRoguelike.Gameplay.Encounter;
using DiceRoguelike.Gameplay.Hero;
using DiceRoguelike.Gameplay.Run;

namespace DiceRoguelike.Tests
{
    public sealed class CombatEncounterIntegrationTests
    {
        [Test]
        public void FactoryCreatesEnemyCombatFromBoardTileAndHeroData()
        {
            var board = Phase1BoardFactory.Create();
            var node = board.GetNode("N06");
            var hero = new HeroDefinition("hero-1", "Ember Sentinel", HeroRole.Warrior, 30, 6, 1);
            var factory = new CombatEncounterFactory(new FixedRng(1), new Dice(6));

            var combat = factory.Create(node, hero, 30);

            Assert.That(node.Type, Is.EqualTo(BoardTileType.Enemy));
            Assert.That(combat.Player.Id, Is.EqualTo(hero.Id));
            Assert.That(combat.Player.MaxHp, Is.EqualTo(hero.BaseHp));
            Assert.That(combat.Player.Attack, Is.EqualTo(hero.BaseAttack));
            Assert.That(combat.Enemy.IsAlive, Is.True);
        }

        [Test]
        public void CombatDamageCanBeAppliedBackToRunStateWithoutDuplicatingDamageLogic()
        {
            var board = Phase1BoardFactory.Create();
            var node = board.GetNode("N06");
            var hero = new HeroDefinition("hero-1", "Ember Sentinel", HeroRole.Warrior, 30, 6, 1);
            var factory = new CombatEncounterFactory(new FixedRng(1), new Dice(6));
            var run = new RunState("combat-sync", board, 30);
            var combat = factory.Create(node, hero, run.CurrentHealth);

            var before = combat.Player.Hp;
            combat.ExecutePlayerAction(CombatActionType.Attack);
            var damageTaken = before - combat.Player.Hp;
            if (damageTaken > 0)
            {
                run.TakeDamage(damageTaken);
            }

            Assert.That(run.CurrentHealth, Is.EqualTo(combat.Player.Hp));
        }

        private sealed class FixedRng : IRng
        {
            private readonly int _value;

            public FixedRng(int value)
            {
                _value = value;
            }

            public int NextInt(int minInclusive, int maxExclusive)
            {
                if (_value < minInclusive || _value >= maxExclusive)
                    throw new System.InvalidOperationException("Fixed value falls outside the requested range.");
                return _value;
            }

            public float NextFloat() => 0.5f;
        }
    }
}
