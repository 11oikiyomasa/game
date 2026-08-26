using System;
using DiceRoguelike.Gameplay.Board;

namespace DiceRoguelike.Gameplay.Run
{
    public sealed class RunState
    {
        public string RunId { get; }
        public string CurrentNodeId { get; private set; }
        public int CurrentHealth { get; private set; }
        public int MaxHealth { get; }
        public int Gold { get; private set; }
        public bool IsComplete { get; private set; }
        public bool IsDefeated { get; private set; }

        public RunState(string runId, BoardGraph board, int maxHealth, int startingGold = 0)
        {
            if (string.IsNullOrWhiteSpace(runId)) throw new ArgumentException("Run id is required.", nameof(runId));
            if (board == null) throw new ArgumentNullException(nameof(board));
            if (maxHealth <= 0) throw new ArgumentOutOfRangeException(nameof(maxHealth));
            if (startingGold < 0) throw new ArgumentOutOfRangeException(nameof(startingGold));

            board.Validate();
            RunId = runId;
            CurrentNodeId = board.StartNodeId;
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            Gold = startingGold;
        }

        public void MoveTo(BoardNode node)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (IsComplete || IsDefeated) throw new InvalidOperationException("Run is already finished.");
            CurrentNodeId = node.Id;
        }

        public void CompleteRun()
        {
            if (IsDefeated) throw new InvalidOperationException("A defeated run cannot be completed.");
            IsComplete = true;
        }

        public void TakeDamage(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            CurrentHealth = Math.Max(0, CurrentHealth - amount);
            if (CurrentHealth == 0) IsDefeated = true;
        }

        public void Heal(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            CurrentHealth = Math.Min(MaxHealth, CurrentHealth + amount);
        }

        public void AddGold(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));
            Gold += amount;
        }
    }
}
