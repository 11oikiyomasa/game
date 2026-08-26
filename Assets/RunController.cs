using System;
using System.Collections.Generic;
using DiceRoguelike.Core;
using DiceRoguelike.Gameplay.Board;
using DiceRoguelike.Gameplay.Dice;

namespace DiceRoguelike.Gameplay.Run
{
    public sealed class RunController
    {
        private readonly BoardGraph _board;
        private readonly IRng _rng;
        private readonly Dice _dice;
        private readonly TileResolver _tileResolver;

        public RunState State { get; }
        public int LastRoll { get; private set; }
        public TileResolution LastResolution { get; private set; }

        public RunController(BoardGraph board, RunState state, IRng rng, Dice dice, TileResolver tileResolver = null)
        {
            _board = board ?? throw new ArgumentNullException(nameof(board));
            State = state ?? throw new ArgumentNullException(nameof(state));
            _rng = rng ?? throw new ArgumentNullException(nameof(rng));
            _dice = dice;
            _tileResolver = tileResolver ?? new TileResolver();
            _board.Validate();
        }

        public int Roll()
        {
            EnsureActive();
            LastRoll = _dice.Roll(_rng);
            return LastRoll;
        }

        public BoardNode MoveByRoll(IReadOnlyList<string> path)
        {
            EnsureActive();
            if (LastRoll <= 0) throw new InvalidOperationException("Roll before moving.");
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (path.Count != LastRoll) throw new InvalidOperationException($"The selected path must contain exactly {LastRoll} steps.");

            var current = _board.GetNode(State.CurrentNodeId);
            for (var i = 0; i < path.Count; i++)
            {
                var next = _board.GetNode(path[i]);
                var connected = false;
                foreach (var id in current.ConnectedNodeIds)
                {
                    if (id == next.Id)
                    {
                        connected = true;
                        break;
                    }
                }

                if (!connected)
                {
                    throw new InvalidOperationException($"Path step {i + 1} ({next.Id}) is not connected to {current.Id}.");
                }

                current = next;
            }

            State.MoveTo(current);
            LastResolution = _tileResolver.Resolve(current, State);
            LastRoll = 0;
            return current;
        }

        public void CompleteBoss()
        {
            EnsureActive();
            if (_board.GetNode(State.CurrentNodeId).Type != BoardTileType.Boss)
                throw new InvalidOperationException("The run can only be completed after reaching the boss tile.");
            State.CompleteRun();
        }

        private void EnsureActive()
        {
            if (State.IsComplete || State.IsDefeated) throw new InvalidOperationException("Run is already finished.");
        }
    }
}
