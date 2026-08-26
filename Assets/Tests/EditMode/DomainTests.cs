using NUnit.Framework;
using DiceRoguelike.Game.Domain;

namespace DiceRoguelike.Game.Tests
{
    public sealed class DomainTests
    {
        [Test]
        public void SeededDice_IsDeterministic()
        {
            var first = new Dice(new SeededRandom(12345)).Roll(6);
            var second = new Dice(new SeededRandom(12345)).Roll(6);

            Assert.That(first.Value, Is.EqualTo(second.Value));
            Assert.That(first.Sides, Is.EqualTo(6));
        }

        [Test]
        public void Dice_ProducesValueWithinSides()
        {
            var dice = new Dice(new SeededRandom(7));

            for (int i = 0; i < 100; i++)
            {
                var roll = dice.Roll(6);
                Assert.That(roll.Value, Is.InRange(1, 6));
            }
        }

        [Test]
        public void Board_RequiresStartAndGoal()
        {
            var board = new BoardDefinition(new[]
            {
                new BoardNode(0, BoardNodeType.Start),
                new BoardNode(1, BoardNodeType.Normal),
                new BoardNode(2, BoardNodeType.Goal)
            });

            Assert.That(board.Count, Is.EqualTo(3));
            Assert.That(board.GetNode(0).Type, Is.EqualTo(BoardNodeType.Start));
            Assert.That(board.GetNode(2).Type, Is.EqualTo(BoardNodeType.Goal));
        }

        [Test]
        public void RunState_ClampsMovementToGoal()
        {
            var board = new BoardDefinition(new[]
            {
                new BoardNode(0, BoardNodeType.Start),
                new BoardNode(1, BoardNodeType.Normal),
                new BoardNode(2, BoardNodeType.Goal)
            });
            var run = new RunState();

            var result = run.Move(board, new DiceRoll(6, 6));

            Assert.That(result.PreviousPosition, Is.EqualTo(0));
            Assert.That(result.NewPosition, Is.EqualTo(2));
            Assert.That(result.ReachedGoal, Is.True);
            Assert.That(run.IsFinished, Is.True);
            Assert.That(run.IsWon, Is.True);
        }

        [Test]
        public void GameStateMachine_EnforcesPhaseOrder()
        {
            var state = new GameStateMachine();

            state.StartTeamSelection();
            state.StartRun();
            state.FinishRun(true);

            Assert.That(state.Phase, Is.EqualTo(GamePhase.Victory));
        }

        [Test]
        public void GameStateMachine_RejectsInvalidTransition()
        {
            var state = new GameStateMachine();

            Assert.Throws<System.InvalidOperationException>(() => state.StartRun());
        }
    }
}
