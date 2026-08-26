using System;

namespace DiceRoguelike.Gameplay.Enemy
{
    public sealed class EnemyDefinition
    {
        public string Id { get; }
        public string Name { get; }
        public int MaxHp { get; }
        public int Attack { get; }

        public EnemyDefinition(string id, string name, int maxHp, int attack)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Enemy id is required.", nameof(id));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Enemy name is required.", nameof(name));
            if (maxHp <= 0) throw new ArgumentOutOfRangeException(nameof(maxHp));
            if (attack < 0) throw new ArgumentOutOfRangeException(nameof(attack));

            Id = id;
            Name = name;
            MaxHp = maxHp;
            Attack = attack;
        }
    }
}
