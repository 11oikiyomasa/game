namespace DiceRoguelike.Game.Domain
{
    public enum BoardNodeType
    {
        Start,
        Normal,
        Enemy,
        Goal
    }

    public readonly struct BoardNode
    {
        public int Index { get; }
        public BoardNodeType Type { get; }

        public BoardNode(int index, BoardNodeType type)
        {
            if (index < 0)
                throw new System.ArgumentOutOfRangeException(nameof(index));

            Index = index;
            Type = type;
        }
    }

    public sealed class BoardDefinition
    {
        private readonly BoardNode[] nodes;

        public int Count => nodes.Length;

        public BoardDefinition(BoardNode[] nodes)
        {
            if (nodes == null)
                throw new System.ArgumentNullException(nameof(nodes));
            if (nodes.Length < 2)
                throw new System.ArgumentException("A board requires at least a start and a goal node.", nameof(nodes));

            for (int i = 0; i < nodes.Length; i++)
            {
                if (nodes[i].Index != i)
                    throw new System.ArgumentException("Board node indices must be contiguous and ordered.", nameof(nodes));
            }

            if (nodes[0].Type != BoardNodeType.Start)
                throw new System.ArgumentException("The first node must be Start.", nameof(nodes));
            if (nodes[nodes.Length - 1].Type != BoardNodeType.Goal)
                throw new System.ArgumentException("The last node must be Goal.", nameof(nodes));

            this.nodes = (BoardNode[])nodes.Clone();
        }

        public BoardNode GetNode(int index)
        {
            if (index < 0 || index >= nodes.Length)
                throw new System.ArgumentOutOfRangeException(nameof(index));

            return nodes[index];
        }
    }

    public readonly struct MoveResult
    {
        public int PreviousPosition { get; }
        public int NewPosition { get; }
        public DiceRoll Roll { get; }
        public bool ReachedGoal { get; }

        public MoveResult(int previousPosition, int newPosition, DiceRoll roll, bool reachedGoal)
        {
            PreviousPosition = previousPosition;
            NewPosition = newPosition;
            Roll = roll;
            ReachedGoal = reachedGoal;
        }
    }

    public sealed class RunState
    {
        public int Position { get; private set; }
        public bool IsFinished { get; private set; }
        public bool IsWon { get; private set; }

        public RunState()
        {
            Position = 0;
        }

        public MoveResult Move(BoardDefinition board, DiceRoll roll)
        {
            if (board == null)
                throw new System.ArgumentNullException(nameof(board));
            if (IsFinished)
                throw new System.InvalidOperationException("The run is already finished.");
            if (roll.Value < 1)
                throw new System.ArgumentOutOfRangeException(nameof(roll));

            int previous = Position;
            Position = System.Math.Min(Position + roll.Value, board.Count - 1);
            IsWon = board.GetNode(Position).Type == BoardNodeType.Goal;
            IsFinished = IsWon;

            return new MoveResult(previous, Position, roll, IsWon);
        }
    }
}
