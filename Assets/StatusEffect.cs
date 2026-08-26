using System;
using System.Collections.Generic;

namespace DiceRoguelike.Gameplay.Combat
{
    public enum StatusEffectType
    {
        Poison,
        Burn,
        Stun,
        DefenseDown,
        AttackUp,
        AttackDown,
        Regeneration,
        Shield
    }

    public enum StatusTrigger
    {
        TurnStart,
        TurnEnd
    }

    public enum StatusStackingRule
    {
        RefreshDuration,
        AddStacks,
        Replace
    }

    public sealed class StatusEffectDefinition
    {
        public StatusEffectType Type { get; }
        public StatusTrigger Trigger { get; }
        public StatusStackingRule StackingRule { get; }
        public int Duration { get; }
        public int Magnitude { get; }
        public string SourceId { get; }

        public StatusEffectDefinition(
            StatusEffectType type,
            StatusTrigger trigger,
            int duration,
            int magnitude,
            string sourceId,
            StatusStackingRule stackingRule = StatusStackingRule.RefreshDuration)
        {
            if (duration <= 0) throw new ArgumentOutOfRangeException(nameof(duration));
            if (magnitude < 0) throw new ArgumentOutOfRangeException(nameof(magnitude));
            if (string.IsNullOrWhiteSpace(sourceId)) throw new ArgumentException("Status source is required.", nameof(sourceId));

            Type = type;
            Trigger = trigger;
            StackingRule = stackingRule;
            Duration = duration;
            Magnitude = magnitude;
            SourceId = sourceId;
        }
    }

    public sealed class StatusEffectInstance
    {
        public StatusEffectType Type { get; }
        public StatusTrigger Trigger { get; }
        public StatusStackingRule StackingRule { get; }
        public string SourceId { get; }
        public int RemainingTurns { get; private set; }
        public int Stacks { get; private set; }
        public int Magnitude { get; private set; }

        public StatusEffectInstance(StatusEffectDefinition definition)
        {
            Type = definition.Type;
            Trigger = definition.Trigger;
            StackingRule = definition.StackingRule;
            SourceId = definition.SourceId;
            RemainingTurns = definition.Duration;
            Stacks = 1;
            Magnitude = definition.Magnitude;
        }

        public void Refresh(int duration, int magnitude)
        {
            RemainingTurns = Math.Max(RemainingTurns, duration);
            Magnitude = magnitude;
        }

        public void AddStack(int duration, int magnitude)
        {
            Stacks++;
            RemainingTurns = Math.Max(RemainingTurns, duration);
            Magnitude += magnitude;
        }

        public void Replace(int duration, int magnitude)
        {
            RemainingTurns = duration;
            Magnitude = magnitude;
            Stacks = 1;
        }

        public void ConsumeTurn()
        {
            RemainingTurns--;
        }
    }

    public sealed class StatusEffectContainer
    {
        private readonly List<StatusEffectInstance> _effects = new List<StatusEffectInstance>();

        public IReadOnlyList<StatusEffectInstance> Effects => _effects;

        public bool Has(StatusEffectType type)
        {
            for (var i = 0; i < _effects.Count; i++)
            {
                if (_effects[i].Type == type && _effects[i].RemainingTurns > 0) return true;
            }

            return false;
        }

        public void Apply(StatusEffectDefinition definition)
        {
            for (var i = 0; i < _effects.Count; i++)
            {
                var existing = _effects[i];
                if (existing.Type != definition.Type) continue;

                switch (definition.StackingRule)
                {
                    case StatusStackingRule.RefreshDuration:
                        existing.Refresh(definition.Duration, definition.Magnitude);
                        return;
                    case StatusStackingRule.AddStacks:
                        existing.AddStack(definition.Duration, definition.Magnitude);
                        return;
                    case StatusStackingRule.Replace:
                        existing.Replace(definition.Duration, definition.Magnitude);
                        return;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            _effects.Add(new StatusEffectInstance(definition));
        }

        public int ResolveTurnStart(CombatantState target)
        {
            return ResolveTrigger(StatusTrigger.TurnStart, target);
        }

        public int ResolveTurnEnd(CombatantState target)
        {
            return ResolveTrigger(StatusTrigger.TurnEnd, target);
        }

        private int ResolveTrigger(StatusTrigger trigger, CombatantState target)
        {
            var totalDamage = 0;
            for (var i = _effects.Count - 1; i >= 0; i--)
            {
                var effect = _effects[i];
                if (effect.Trigger == trigger)
                {
                    if (effect.Type == StatusEffectType.Poison || effect.Type == StatusEffectType.Burn)
                    {
                        totalDamage += target.TakeDirectDamage(effect.Magnitude * effect.Stacks);
                    }

                    effect.ConsumeTurn();
                }

                if (effect.RemainingTurns <= 0)
                {
                    _effects.RemoveAt(i);
                }
            }

            return totalDamage;
        }
    }
}
