using System;
using System.Collections.Generic;

namespace DiceRoguelike.Gameplay.Board
{
    public enum BoardTileType
    {
        Start,
        Normal,
        Enemy,
        Elite,
        Boss,
        Treasure,
        Shop,
        Heal,
        Event,
        Risk,
        Mystery,
        Resource,
        Special
    }

    public sealed class BoardNode
    {
        private readonly List<string> _connectedNodeIds = new List<string>();

        public string Id { get; }
        public BoardTileType Type { get; }
        public int Difficulty { get; }
        public float RewardModifier { get; }
        public IReadOnlyList<string> ConnectedNodeIds => _connectedNodeIds;

        public BoardNode(string id, BoardTileType type, int difficulty = 0, float rewardModifier = 1f)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Node id is required.", nameof(id));
            }

            if (difficulty < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(difficulty));
            }

            if (rewardModifier < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(rewardModifier));
            }

            Id = id;
            Type = type;
            Difficulty = difficulty;
            RewardModifier = rewardModifier;
        }

        public void ConnectTo(BoardNode other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            if (other.Id == Id)
            {
                throw new InvalidOperationException("A node cannot connect to itself.");
            }

            if (!_connectedNodeIds.Contains(other.Id))
            {
                _connectedNodeIds.Add(other.Id);
            }
        }
    }
}
