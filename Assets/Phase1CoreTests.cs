using System;
using System.Collections.Generic;
using NUnit.Framework;
using DiceRoguelike.Core;
using DiceRoguelike.Gameplay.Board;
using DiceRoguelike.Gameplay.Dice;
using DiceRoguelike.Gameplay.Run;

namespace DiceRoguelike.Tests
{
    public sealed class Phase1CoreTests
    {
        [Test]
        public void StateMachine_StartsAndReturnsToBoard()
        {
            var machine = new GameStateMachine();

            machine.TransitionTo(GameState.MainMenu);
            machine.TransitionTo(GameState.HeroSelection);
            machine.TransitionTo(GameState.RunInitialization);
            machine.TransitionTo(GameState.Board);
            machine.TransitionTo(GameState.DiceRoll);
            machine.TransitionTo(GameState.Movement);
            machine.TransitionTo(GameState.TileResolution);
            machine.TransitionTo(GameState.Reward);
            machine.TransitionTo(GameState.Board);

            Assert.That(machine.Current, Is.EqualTo(GameState.Board));
        }

        [Test]
        public void StateMachine_RejectsCombatFromMainMenu()
        {
            var machine = new GameStateMachine(GameState.MainMenu);

            Assert.That(machine.CanTransitionTo(GameState.Combat), Is.False);
            Assert.Throws<InvalidOperationException>(() => machine.TransitionTo(GameState.Combat));
        }

        [Test]
        public void Dice_StaysWithinConfiguredRange()
        {
            var dice = new Dice(6);
            var rng = new SeededRng(12345);

            for (var i = 0; i < 1000; i++)
            {
                var result = dice.Roll(rng);
                Assert.That(result, Is.InRange(1, 6));
            }
        }

        [Test]
        public void SeededRng_ReproducesTheSameSequence()
        {
            var first = new SeededRng(424242);
            var second = new SeededRng(424242);

            for (var i = 0; i < 100; i++)
            {
                Assert.That(first.NextInt(0, 100000), Is.EqualTo(second.NextInt(0, 100000)));
            }
        }

        [Test]
        public void Phase1Board_HasReachableBoss()
        {
            var board = Phase1BoardFactory.Create();
            var current = board.GetNode(board.StartNodeId);
            var visited = new HashSet<string>();

            while (current.Id != board.BossNodeId)
            {
                Assert.That(visited.Add(current.Id), Is.True, "Board contains an unexpected cycle.");
                Assert.That(current.ConnectedNodeIds.Count, Is.EqualTo(1));
                current = board.GetNode(current.ConnectedNodeIds[0]);
            }

            Assert.That(current.Type, Is.EqualTo(BoardTileType.Boss));
        }

        [Test]
        public void RunController_ConsumesExactlyTheRolledNumberOfSteps()
        {
            var board = Phase1BoardFactory.Create();
            var runState = new RunState("test-run", board, 30);
            var controller = new RunController(board, runState, new FixedRng(3), new Dice(6));

            Assert.That(controller.Roll(), Is.EqualTo(3));

            var start = board.GetNode(runState.CurrentNodeId);
            var path = new List<string>();
            var current = start;
            for (var i = 0; i < 3; i++)
            {
                current = board.GetNode(current.ConnectedNodeIds[0]);
                path.Add(current.Id);
            }

            controller.MoveByRoll(path);
            Assert.That(runState.CurrentNodeId, Is.EqualTo("N03"));
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
                    throw new InvalidOperationException("Fixed value falls outside the requested range.");
                return _value;
            }

            public float NextFloat() => 0.5f;
        }
    }
}
