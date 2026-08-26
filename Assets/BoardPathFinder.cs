using System;
using System.Collections.Generic;
using DiceRoguelike.Gameplay.Board;

namespace DiceRoguelike.Gameplay.Run
{
    public static class BoardPathFinder
    {
        public static IReadOnlyList<IReadOnlyList<string>> FindPaths(BoardGraph board, string startNodeId, int steps, int maxPaths = 12)
        {
            if (board == null) throw new ArgumentNullException(nameof(board));
            if (steps <= 0) throw new ArgumentOutOfRangeException(nameof(steps));
            if (maxPaths <= 0) throw new ArgumentOutOfRangeException(nameof(maxPaths));

            var results = new List<IReadOnlyList<string>>();
            var path = new List<string>(steps);
            Visit(board, board.GetNode(startNodeId), steps, maxPaths, path, results);
            return results;
        }

        private static void Visit(BoardGraph board, BoardNode current, int remaining, int maxPaths, List<string> path, List<IReadOnlyList<string>> results)
        {
            if (results.Count >= maxPaths) return;
            if (remaining == 0)
            {
                results.Add(new List<string>(path));
                return;
            }

            foreach (var nextId in current.ConnectedNodeIds)
            {
                if (results.Count >= maxPaths) return;
                var next = board.GetNode(nextId);
                path.Add(next.Id);
                Visit(board, next, remaining - 1, maxPaths, path, results);
                path.RemoveAt(path.Count - 1);
            }
        }
    }
}
