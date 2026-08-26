using System;
using System.Collections.Generic;

namespace DiceRoguelike.Core
{
    public sealed class GameStateMachine
    {
        private static readonly Dictionary<GameState, HashSet<GameState>> AllowedTransitions =
            new Dictionary<GameState, HashSet<GameState>>
            {
                { GameState.Boot, Set(GameState.MainMenu) },
                { GameState.MainMenu, Set(GameState.HeroSelection) },
                { GameState.HeroSelection, Set(GameState.RunInitialization, GameState.MainMenu) },
                { GameState.RunInitialization, Set(GameState.Board, GameState.RunDefeat) },
                { GameState.Board, Set(GameState.DiceRoll, GameState.RunDefeat) },
                { GameState.DiceRoll, Set(GameState.Movement, GameState.Board, GameState.RunDefeat) },
                { GameState.Movement, Set(GameState.TileResolution, GameState.RunDefeat) },
                { GameState.TileResolution, Set(GameState.Encounter, GameState.Reward, GameState.Upgrade, GameState.Board, GameState.Boss, GameState.RunDefeat) },
                { GameState.Encounter, Set(GameState.Combat, GameState.Reward, GameState.Board, GameState.RunDefeat) },
                { GameState.Combat, Set(GameState.Reward, GameState.Boss, GameState.RunComplete, GameState.RunDefeat) },
                { GameState.Reward, Set(GameState.Upgrade, GameState.Board, GameState.RunComplete) },
                { GameState.Upgrade, Set(GameState.Board, GameState.RunComplete) },
                { GameState.Boss, Set(GameState.Combat, GameState.RunComplete, GameState.RunDefeat) },
                { GameState.RunComplete, Set(GameState.MetaProgression, GameState.MainMenu) },
                { GameState.RunDefeat, Set(GameState.MetaProgression, GameState.MainMenu) },
                { GameState.MetaProgression, Set(GameState.MainMenu, GameState.HeroSelection) }
            };

        public GameState Current { get; private set; }

        public GameStateMachine(GameState initialState = GameState.Boot)
        {
            Current = initialState;
        }

        public bool CanTransitionTo(GameState nextState)
        {
            return AllowedTransitions.TryGetValue(Current, out var allowed) && allowed.Contains(nextState);
        }

        public void TransitionTo(GameState nextState)
        {
            if (!CanTransitionTo(nextState))
                throw new InvalidOperationException($"Invalid game-state transition: {Current} -> {nextState}.");

            Current = nextState;
        }

        private static HashSet<GameState> Set(params GameState[] states)
        {
            return new HashSet<GameState>(states);
        }
    }
}
