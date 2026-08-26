using System.Collections.Generic;
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
    public sealed class Phase1RunIntegrationTests
    {
        [Test]
        public void Run_ReachesEnemy_ResolvesCombat_AndReturnsToRun()
        {
            var rng = new FixedRng(6);
            var board = Phase1BoardFactory.Create();
            var state = new RunState("integration-run", board, 30, 0);
            var controller = new RunController(board, state, rng, new Dice(6));
            var hero = new HeroDefinition("starter", "Starter", HeroRole.Warrior, 30, 6, 1);
            var combatFactory = new CombatEncounterFactory(rng, new Dice(6));

            Assert.That(controller.Roll(), Is.EqualTo(6));

            var path = new List<string>();
            var current = board.GetNode(state.CurrentNodeId);
            for (var i = 0; i < 6; i++)
            {
                current = board.GetNode(current.ConnectedNodeIds[0]);
                path.Add(current.Id);
            }

            var destination = controller.MoveByRoll(path);
            Assert.That(destination.Type, Is.EqualTo(BoardTileType.Enemy));
            Assert.That(controller.LastResolution.StartsEncounter, Is.True);

            var combat = combatFactory.Create(destination, hero, state.CurrentHealth);
            Assert.That(combat.IsComplete, Is.False);

            var first = combat.ExecutePlayerAction(CombatActionType.Attack);
            Assert.That(first.Damage, Is.GreaterThan(0));
            Assert.That(combat.IsComplete, Is.False);

            var second = combat.ExecutePlayerAction(CombatActionType.Attack);
            Assert.That(second.IsComplete, Is.True);
            Assert.That(second.PlayerWon, Is.True);
            Assert.That(combat.Enemy.IsAlive, Is.False);
            Assert.That(state.IsComplete, Is.False);
            Assert.That(state.IsDefeated, Is.False);
            Assert.That(state.CurrentNodeId, Is.EqualTo("N06"));
        }

        [Test]
        public void Run_ReachingBoss_CanProduceRealCompletion()
        {
            var rng = new FixedRng(6);
            var board = Phase1BoardFactory.Create();
            var state = new RunState("boss-run", board, 100, 0);
            var controller = new RunController(board, state, rng, new Dice(6));

            for (var step = 0; step < 5; step++)
            {
                Assert.That(controller.Roll(), Is.EqualTo(6));
                var path = new List<string>();
                var current = board.GetNode(state.CurrentNodeId);
                for (var i = 0; i < 6; i++)
                {
                    current = board.GetNode(current.ConnectedNodeIds[0]);
                    path.Add(current.Id);
                }

                controller.MoveByRoll(path);

                if (state.CurrentNodeId == board.BossNodeId)
                    break;
            }

            Assert.That(state.CurrentNodeId, Is.EqualTo(board.BossNodeId));
            Assert.That(board.GetNode(state.CurrentNodeId).Type, Is.EqualTo(BoardTileType.Boss));
        }

        private sealed class FixedRng : IRng
        {
            private readonly int value;

            public FixedRng(int value)
            {
                this.value = value;
            }

            public int NextInt(int minInclusive, int maxExclusive)
            {
                if (value < minInclusive || value >= maxExclusive)
                    throw new System.InvalidOperationException("Fixed RNG value is outside the requested range.");
                return value;
            }

            public float NextFloat() => 0.5f;
        }
    }
}
