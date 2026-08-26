using System;
using DiceRoguelike.Core;
using DiceRoguelike.Gameplay.Dice;

namespace DiceRoguelike.Gameplay.Combat
{
    public sealed class CombatEngine
    {
        private readonly IRng _rng;
        private readonly Dice _dice;

        public CombatantState Player { get; }
        public CombatantState Enemy { get; }
        public int Turn { get; private set; }
        public bool IsComplete { get; private set; }
        public bool PlayerWon { get; private set; }

        public CombatEngine(CombatantState player, CombatantState enemy, IRng rng, Dice dice)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
            Enemy = enemy ?? throw new ArgumentNullException(nameof(enemy));
            _rng = rng ?? throw new ArgumentNullException(nameof(rng));
            _dice = dice ?? throw new ArgumentNullException(nameof(dice));
        }

        public CombatActionResult ExecutePlayerAction(CombatActionType action)
        {
            if (IsComplete) throw new InvalidOperationException("Combat is already complete.");
            if (!Player.IsAlive || !Enemy.IsAlive) throw new InvalidOperationException("A defeated combatant cannot act.");

            var playerStatusDamage = Player.StatusEffects.ResolveTurnStart(Player);
            if (!Player.IsAlive)
            {
                IsComplete = true;
                PlayerWon = false;
                return new CombatActionResult(action, 0, playerStatusDamage, false, true, false);
            }

            var stunned = Player.IsStunned;
            var dieRoll = _dice.Roll(_rng);
            var critical = false;
            var damage = 0;

            if (!stunned)
            {
                critical = action != CombatActionType.Defend && IsCritical(Player);
                var rawDamage = Player.Attack + dieRoll;
                var skillModifier = action == CombatActionType.Skill ? 2 : 0;

                switch (action)
                {
                    case CombatActionType.Attack:
                        damage = Enemy.TakeDamage(rawDamage, 0, critical);
                        break;
                    case CombatActionType.Defend:
                        Player.Defend(2 + dieRoll);
                        break;
                    case CombatActionType.Skill:
                        damage = Enemy.TakeDamage(rawDamage, skillModifier, critical);
                        Enemy.StatusEffects.Apply(new StatusEffectDefinition(
                            StatusEffectType.Burn,
                            StatusTrigger.TurnStart,
                            duration: 2,
                            magnitude: 2,
                            sourceId: Player.Id,
                            stackingRule: StatusStackingRule.AddStacks));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(action), action, null);
                }
            }

            if (!Enemy.IsAlive)
            {
                IsComplete = true;
                PlayerWon = true;
                return new CombatActionResult(action, damage, playerStatusDamage, critical, true, true);
            }

            var enemyStatusDamage = ResolveEnemyTurn();
            Turn++;

            if (!Player.IsAlive)
            {
                IsComplete = true;
                PlayerWon = false;
            }

            return new CombatActionResult(
                action,
                damage,
                playerStatusDamage + enemyStatusDamage,
                critical,
                IsComplete,
                PlayerWon);
        }

        private int ResolveEnemyTurn()
        {
            var statusDamage = Enemy.StatusEffects.ResolveTurnStart(Enemy);
            if (!Enemy.IsAlive) return statusDamage;

            if (Enemy.IsStunned)
            {
                Enemy.StatusEffects.ResolveTurnEnd(Enemy);
                return statusDamage;
            }

            var damage = Enemy.Attack + _dice.Roll(_rng);
            Player.TakeDamage(damage);
            statusDamage += Player.StatusEffects.ResolveTurnEnd(Player);
            Enemy.StatusEffects.ResolveTurnEnd(Enemy);
            return statusDamage;
        }

        private bool IsCritical(CombatantState attacker)
        {
            return attacker.CritChancePercent > 0
                && _rng.NextInt(0, 100) < attacker.CritChancePercent;
        }
    }
}
