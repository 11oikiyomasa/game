using NUnit.Framework;
using DiceRoguelike.Core;
using DiceRoguelike.Gameplay.Board;
using DiceRoguelike.Gameplay.Run;

namespace DiceRoguelike.Tests
{
    public sealed class BoardPathFinderTests
    {
        [Test]
        public void FindsExactLengthPathsOnProceduralBoard()
        {
            var board = ProceduralBoardFactory.Create(new SeededRng(123), rows: 6, lanes: 3);
            var paths = BoardPathFinder.FindPaths(board, board.StartNodeId, 3, maxPaths: 100);

            Assert.That(paths.Count, Is.GreaterThan(1));
            foreach (var path in paths)
            {
                Assert.That(path.Count, Is.EqualTo(3));
                Assert.That(path[0], Is.Not.EqualTo(board.StartNodeId));
            }
        }

        [Test]
        public void PathLimitPreventsUnboundedEnumeration()
        {
            var board = ProceduralBoardFactory.Create(new SeededRng(321), rows: 10, lanes: 3);
            var paths = BoardPathFinder.FindPaths(board, board.StartNodeId, 7, maxPaths: 5);

            Assert.That(paths.Count, Is.LessThanOrEqualTo(5));
        }

        [Test]
        public void InvalidPathArgumentsAreRejected()
        {
            var board = ProceduralBoardFactory.Create(new SeededRng(1));
            Assert.That(() => BoardPathFinder.FindPaths(board, board.StartNodeId, 0), Throws.ArgumentOutOfRangeException);
            Assert.That(() => BoardPathFinder.FindPaths(board, board.StartNodeId, 2, 0), Throws.ArgumentOutOfRangeException);
        }
    }
}
