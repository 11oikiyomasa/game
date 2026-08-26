using System;
using DiceRoguelike.Core;

namespace DiceRoguelike.Gameplay.Board
{
    public static class ProceduralBoardFactory
    {
        public static BoardGraph Create(IRng rng, int rows = 10, int lanes = 3)
        {
            if (rng == null) throw new ArgumentNullException(nameof(rng));
            if (rows < 2) throw new ArgumentOutOfRangeException(nameof(rows));
            if (lanes < 2) throw new ArgumentOutOfRangeException(nameof(lanes));

            var board = new BoardGraph();
            var start = new BoardNode("P00", BoardTileType.Start);
            board.AddNode(start);

            var previous = new BoardNode[lanes];
            for (var lane = 0; lane < lanes; lane++)
            {
                previous[lane] = CreateNode(1, lane, rows, rng);
                board.AddNode(previous[lane]);
                start.ConnectTo(previous[lane]);
            }

            for (var row = 2; row < rows; row++)
            {
                var current = new BoardNode[lanes];
                for (var lane = 0; lane < lanes; lane++)
                {
                    current[lane] = CreateNode(row, lane, rows, rng);
                    board.AddNode(current[lane]);
                }

                for (var lane = 0; lane < lanes; lane++)
                {
                    previous[lane].ConnectTo(current[lane]);
                    var alternateLane = rng.NextInt(0, lanes);
                    if (alternateLane != lane)
                    {
                        previous[lane].ConnectTo(current[alternateLane]);
                    }
                }

                previous = current;
            }

            var boss = new BoardNode("PBOSS", BoardTileType.Boss, difficulty: rows, rewardModifier: 2f);
            board.AddNode(boss);
            for (var lane = 0; lane < lanes; lane++)
            {
                previous[lane].ConnectTo(boss);
            }

            board.Validate();
            return board;
        }

        private static BoardNode CreateNode(int row, int lane, int totalRows, IRng rng)
        {
            var type = RollTileType(rng);
            var difficulty = Math.Max(1, row / 2);
            var rewardModifier = 1f + row * 0.05f;
            return new BoardNode($"P{row:00}_{lane}", type, difficulty, rewardModifier);
        }

        private static BoardTileType RollTileType(IRng rng)
        {
            var roll = rng.NextInt(0, 100);
            if (roll < 38) return BoardTileType.Normal;
            if (roll < 58) return BoardTileType.Enemy;
            if (roll < 68) return BoardTileType.Resource;
            if (roll < 76) return BoardTileType.Heal;
            if (roll < 84) return BoardTileType.Treasure;
            if (roll < 90) return BoardTileType.Event;
            if (roll < 96) return BoardTileType.Shop;
            return BoardTileType.Elite;
        }
    }
}
