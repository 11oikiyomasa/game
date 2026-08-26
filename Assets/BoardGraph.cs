using System;
using System.Collections.Generic;

namespace DiceRoguelike.Gameplay.Board
{
    public sealed class BoardGraph
    {
        private readonly Dictionary<string, BoardNode> _nodes = new Dictionary<string, BoardNode>();

        public string StartNodeId { get; private set; }
        public string BossNodeId { get; private set; }
        public IReadOnlyDictionary<string, BoardNode> Nodes => _nodes;

        public void AddNode(BoardNode node)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (!_nodes.TryAdd(node.Id, node))
            {
                throw new InvalidOperationException($"Duplicate board node id: {node.Id}");
            }

            if (node.Type == BoardTileType.Start && StartNodeId == null)
            {
                StartNodeId = node.Id;
            }

            if (node.Type == BoardTileType.Boss && BossNodeId == null)
            {
                BossNodeId = node.Id;
            }
        }

        public BoardNode GetNode(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !_nodes.TryGetValue(id, out var node))
            {
                throw new KeyNotFoundException($"Board node not found: {id}");
            }

            return node;
        }

        public void Validate()
        {
            if (_nodes.Count == 0)
            {
                throw new InvalidOperationException("Board cannot be empty.");
            }

            if (StartNodeId == null)
            {
                throw new InvalidOperationException("Board must contain a start node.");
            }

            if (BossNodeId == null)
            {
                throw new InvalidOperationException("Board must contain a boss node.");
            }

            foreach (var node in _nodes.Values)
            {
                foreach (var connectedId in node.ConnectedNodeIds)
                {
                    if (!_nodes.ContainsKey(connectedId))
                    {
                        throw new InvalidOperationException($"Node {node.Id} references missing node {connectedId}.");
                    }
                }
            }
        }
    }
}
