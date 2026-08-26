using System;
using DiceRoguelike.Gameplay.Run;

namespace DiceRoguelike.Gameplay.Board
{
    public sealed class TileResolution
    {
        public BoardTileType Type { get; }
        public bool StartsEncounter { get; }
        public bool OpensChoice { get; }
        public bool GrantsReward { get; }

        public TileResolution(BoardTileType type, bool startsEncounter = false, bool opensChoice = false, bool grantsReward = false)
        {
            Type = type;
            StartsEncounter = startsEncounter;
            OpensChoice = opensChoice;
            GrantsReward = grantsReward;
        }
    }

    public sealed class TileResolver
    {
        public TileResolution Resolve(BoardNode node, RunState run)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (run == null) throw new ArgumentNullException(nameof(run));
            if (run.IsComplete || run.IsDefeated) throw new InvalidOperationException("Cannot resolve a finished run.");

            switch (node.Type)
            {
                case BoardTileType.Start:
                case BoardTileType.Normal:
                    return new TileResolution(node.Type);

                case BoardTileType.Resource:
                    run.AddGold(1);
                    return new TileResolution(node.Type, grantsReward: true);

                case BoardTileType.Heal:
                    run.Heal(2);
                    return new TileResolution(node.Type, grantsReward: true);

                case BoardTileType.Treasure:
                    run.AddGold(3);
                    return new TileResolution(node.Type, grantsReward: true);

                case BoardTileType.Enemy:
                case BoardTileType.Elite:
                case BoardTileType.Boss:
                    return new TileResolution(node.Type, startsEncounter: true);

                case BoardTileType.Shop:
                case BoardTileType.Event:
                case BoardTileType.Risk:
                case BoardTileType.Mystery:
                case BoardTileType.Special:
                    return new TileResolution(node.Type, opensChoice: true);

                default:
                    throw new ArgumentOutOfRangeException(nameof(node), node.Type, "Unsupported tile type.");
            }
        }
    }
}
