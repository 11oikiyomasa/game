using DiceRoguelike.Gameplay.Board;

namespace DiceRoguelike.Gameplay.Board
{
    public static class Phase1BoardFactory
    {
        public static BoardGraph Create()
        {
            var board = new BoardGraph();
            BoardNode previous = null;

            for (var index = 0; index <= 30; index++)
            {
                var type = GetType(index);
                var node = new BoardNode($"N{index:00}", type, difficulty: index / 5, rewardModifier: 1f + index * 0.02f);
                board.AddNode(node);

                if (previous != null)
                {
                    previous.ConnectTo(node);
                }

                previous = node;
            }

            board.Validate();
            return board;
        }

        private static BoardTileType GetType(int index)
        {
            if (index == 0) return BoardTileType.Start;
            if (index == 30) return BoardTileType.Boss;
            if (index == 6 || index == 17 || index == 24) return BoardTileType.Enemy;
            if (index == 12 || index == 21) return BoardTileType.Elite;
            if (index == 9 || index == 19 || index == 27) return BoardTileType.Treasure;
            if (index == 15) return BoardTileType.Heal;
            if (index == 4 || index == 23) return BoardTileType.Resource;
            return BoardTileType.Normal;
        }
    }
}
