using NUnit.Framework;
using DiceRoguelike.Core;

namespace DiceRoguelike.Tests
{
    public sealed class CoreStateMachineTests
    {
        [Test]
        public void StateMachine_AllowsTheCoreRunSequence()
        {
            var machine = new GameStateMachine();

            machine.TransitionTo(GameState.MainMenu);
            machine.TransitionTo(GameState.HeroSelection);
            machine.TransitionTo(GameState.RunInitialization);
            machine.TransitionTo(GameState.Board);
            machine.TransitionTo(GameState.DiceRoll);
            machine.TransitionTo(GameState.Movement);
            machine.TransitionTo(GameState.TileResolution);
            machine.TransitionTo(GameState.Encounter);
            machine.TransitionTo(GameState.Combat);
            machine.TransitionTo(GameState.Reward);
            machine.TransitionTo(GameState.Upgrade);
            machine.TransitionTo(GameState.Board);

            Assert.That(machine.Current, Is.EqualTo(GameState.Board));
        }

        [Test]
        public void StateMachine_RejectsInvalidTransitions()
        {
            var machine = new GameStateMachine();

            Assert.That(machine.CanTransitionTo(GameState.Combat), Is.False);
            Assert.Throws<System.InvalidOperationException>(() => machine.TransitionTo(GameState.Combat));
        }
    }
}
