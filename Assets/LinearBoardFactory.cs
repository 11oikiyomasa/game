namespace DiceRoguelike.Gameplay.Board
{
    public static class LinearBoardFactory
    {
        public static BoardGraph Create(int normalNodes = 6)
        {
            if (normalNodes < 1) throw new System.ArgumentOutOfRangeException(nameof(normalNodes));

            var board = new BoardGraph();
            var start = new BoardNode("start", BoardTileType.Start);
            board.AddNode(start);

            var previous = start;
            for (var i = 1; i <= normalNodes; i++)
            {
                var type = i == 2 ? BoardTileType.Enemy : i == 4 ? BoardTileType.Treasure : BoardTileType.Normal;
                var node = new BoardNode("node_" + i, type, difficulty: i);
                previous.ConnectTo(node);
                board.AddNode(node);
                previous = node;
            }

            var boss = new BoardNode("boss", BoardTileType.Boss, difficulty: normalNodes + 1, rewardModifier: 2f);
            previous.ConnectTo(boss);
            board.AddNode(boss);
            board.Validate();
            return board;
        }
    }
}
