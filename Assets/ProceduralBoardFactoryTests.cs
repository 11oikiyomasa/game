using System.Collections.Generic;
using NUnit.Framework;
using DiceRoguelike.Core;
using DiceRoguelike.Gameplay.Board;

namespace DiceRoguelike.Tests
{
    public sealed class ProceduralBoardFactoryTests
    {
        [Test]
        public void SameSeedProducesTheSameBoardTopologyAndTiles()
        {
            var first = ProceduralBoardFactory.Create(new SeededRng(4242), rows: 8, lanes: 3);
            var second = ProceduralBoardFactory.Create(new SeededRng(4242), rows: 8, lanes: 3);

            Assert.That(first.Nodes.Count, Is.EqualTo(second.Nodes.Count));
            foreach (var pair in first.Nodes)
            {
                var other = second.GetNode(pair.Key);
                Assert.That(pair.Value.Type, Is.EqualTo(other.Type));
                Assert.That(pair.Value.ConnectedNodeIds, Is.EqualTo(other.ConnectedNodeIds));
            }
        }

        [Test]
        public void ProceduralBoardHasBranchingRoutesAndReachableBoss()
        {
            var board = ProceduralBoardFactory.Create(new SeededRng(99), rows: 10, lanes: 3);
            var start = board.GetNode(board.StartNodeId);

            Assert.That(start.ConnectedNodeIds.Count, Is.EqualTo(3));
            Assert.That(board.GetNode(board.BossNodeId).Type, Is.EqualTo(BoardTileType.Boss));

            var reachable = new HashSet<string>();
            var queue = new Queue<string>();
            queue.Enqueue(start.Id);
            reachable.Add(start.Id);

            while (queue.Count > 0)
            {
                var node = board.GetNode(queue.Dequeue());
                foreach (var nextId in node.ConnectedNodeIds)
                {
                    if (reachable.Add(nextId)) queue.Enqueue(nextId);
                }
            }

            Assert.That(reachable.Contains(board.BossNodeId), Is.True);
            Assert.That(board.Nodes.Count, Is.GreaterThan(20));
        }

        [Test]
        public void InvalidDimensionsAreRejected()
        {
            Assert.That(() => ProceduralBoardFactory.Create(new SeededRng(1), rows: 1, lanes: 3), Throws.ArgumentOutOfRangeException);
            Assert.That(() => ProceduralBoardFactory.Create(new SeededRng(1), rows: 3, lanes: 1), Throws.ArgumentOutOfRangeException);
        }
    }
}
