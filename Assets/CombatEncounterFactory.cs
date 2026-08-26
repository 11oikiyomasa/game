using System;
using DiceRoguelike.Core;
using DiceRoguelike.Gameplay.Board;
using DiceRoguelike.Gameplay.Combat;
using DiceRoguelike.Gameplay.Dice;
using DiceRoguelike.Gameplay.Hero;

namespace DiceRoguelike.Gameplay.Encounter
{
    public sealed class CombatEncounterFactory
    {
        private readonly IRng _rng;
        private readonly Dice _combatDice;

        public CombatEncounterFactory(IRng rng, Dice combatDice)
        {
            _rng = rng ?? throw new ArgumentNullException(nameof(rng));
            _combatDice = combatDice ?? throw new ArgumentNullException(nameof(combatDice));
        }

        public CombatEngine Create(BoardNode node, HeroDefinition hero, int currentHealth)
        {
            if (node == null) throw new ArgumentNullException(nameof(node));
            if (hero == null) throw new ArgumentNullException(nameof(hero));
            if (currentHealth <= 0 || currentHealth > hero.BaseHp)
                throw new ArgumentOutOfRangeException(nameof(currentHealth));

            if (node.Type != BoardTileType.Enemy && node.Type != BoardTileType.Elite && node.Type != BoardTileType.Boss)
                throw new InvalidOperationException("A combat encounter requires an enemy, elite, or boss tile.");

            var enemy = BuildEnemy(node);
            var player = new CombatantState(
                hero.Id,
                hero.Name,
                hero.BaseHp,
                hero.BaseAttack,
                currentHealth,
                hero.BaseDefense,
                hero.BaseCritChancePercent,
                hero.BaseCritMultiplier);

            return new CombatEngine(player, enemy, _rng, _combatDice);
        }

        private static CombatantState BuildEnemy(BoardNode node)
        {
            var difficulty = node.Difficulty;
            switch (node.Type)
            {
                case BoardTileType.Boss:
                    return new CombatantState("boss-warden", "Ashen Warden", 45 + difficulty * 4, 7 + difficulty);
                case BoardTileType.Elite:
                    return new CombatantState("elite-raider", "Grove Raider", 24 + difficulty * 2, 5 + difficulty);
                default:
                    return new CombatantState("forest-fiend", "Forest Fiend", 14 + difficulty * 2, 3 + difficulty);
            }
        }
    }
}
