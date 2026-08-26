using System.Collections.Generic;
using UnityEngine;
using DiceRoguelike.Core;
using DiceRoguelike.Gameplay.Board;
using DiceRoguelike.Gameplay.Combat;
using DiceRoguelike.Gameplay.Dice;
using DiceRoguelike.Gameplay.Encounter;
using DiceRoguelike.Gameplay.Hero;
using DiceRoguelike.Gameplay.Run;

namespace DiceRoguelike.Prototype
{
    public sealed class Phase1GameEntryPoint : MonoBehaviour
    {
        private BoardGraph _board;
        private RunController _run;
        private CombatEncounterFactory _combatFactory;
        private CombatEngine _combat;
        private HeroDefinition _starterHero;
        private BoardNode _combatNode;
        private string _message;
        private int _pendingRoll;
        private IReadOnlyList<IReadOnlyList<string>> _candidatePaths;

        private void Awake()
        {
            var rng = new SeededRng(20260827);
            _board = ProceduralBoardFactory.Create(rng, rows: 10, lanes: 3);
            var state = new RunState("phase3-prototype", _board, maxHealth: 30, startingGold: 5);
            _run = new RunController(_board, state, rng, new Dice(6));
            _combatFactory = new CombatEncounterFactory(rng, new Dice(6));
            _starterHero = new HeroDefinition("starter-sentinel", "Ember Sentinel", HeroRole.Warrior, 30, 6, 1);
            _message = "Run started on a seeded branching board. Roll the die.";
        }

        private void OnGUI()
        {
            var width = Mathf.Min(Screen.width - 32f, 680f);
            GUILayout.BeginArea(new Rect(16f, 16f, width, Screen.height - 32f));
            GUILayout.Label("Dice Roguelike - Phase 3 Branching Run Prototype", HeaderStyle());
            GUILayout.Space(8f);

            if (_combat != null && !_combat.IsComplete)
            {
                DrawCombat();
            }
            else
            {
                DrawRun();
            }

            GUILayout.EndArea();
        }

        private void DrawRun()
        {
            GUILayout.Label($"Hero: {_starterHero.Name}   Node: {_run.State.CurrentNodeId}   HP: {_run.State.CurrentHealth}/{_run.State.MaxHealth}   Gold: {_run.State.Gold}");
            GUILayout.Label($"Last roll: {(_pendingRoll > 0 ? _pendingRoll.ToString() : "-")}");
            GUILayout.Space(8f);
            GUILayout.Label(_message, MessageStyle());
            GUILayout.Space(12f);

            GUI.enabled = !_run.State.IsComplete && !_run.State.IsDefeated && _pendingRoll == 0;
            if (GUILayout.Button("ROLL DICE", GUILayout.Height(64f)))
            {
                _pendingRoll = _run.Roll();
                _candidatePaths = BoardPathFinder.FindPaths(_board, _run.State.CurrentNodeId, _pendingRoll, maxPaths: 8);
                _message = $"Rolled {_pendingRoll}. Choose one of {_candidatePaths.Count} legal routes.";
            }

            GUI.enabled = !_run.State.IsComplete && !_run.State.IsDefeated && _pendingRoll > 0;
            if (_candidatePaths != null)
            {
                for (var i = 0; i < _candidatePaths.Count; i++)
                {
                    var path = _candidatePaths[i];
                    var destination = _board.GetNode(path[path.Count - 1]);
                    if (GUILayout.Button($"ROUTE {i + 1}: {destination.Type} ({destination.Id})", GUILayout.Height(48f)))
                    {
                        MoveSelectedPath(path);
                        break;
                    }
                }
            }

            GUI.enabled = true;
            GUILayout.Space(12f);
            if (_run.State.IsDefeated) GUILayout.Label("RUN DEFEAT", HeaderStyle());
            else if (_run.State.IsComplete) GUILayout.Label("RUN COMPLETE", HeaderStyle());
        }

        private void DrawCombat()
        {
            GUILayout.Label($"{_combat.Player.Name}: {_combat.Player.Hp}/{_combat.Player.MaxHp} HP");
            GUILayout.Label($"{_combat.Enemy.Name}: {_combat.Enemy.Hp}/{_combat.Enemy.MaxHp} HP");
            GUILayout.Label($"Turn: {_combat.Turn + 1}   {_message}", MessageStyle());
            GUILayout.Space(12f);

            if (GUILayout.Button("ATTACK", GUILayout.Height(56f))) ExecuteCombatAction(CombatActionType.Attack);
            if (GUILayout.Button("DEFEND", GUILayout.Height(56f))) ExecuteCombatAction(CombatActionType.Defend);
            if (GUILayout.Button("SKILL", GUILayout.Height(56f))) ExecuteCombatAction(CombatActionType.Skill);
        }

        private void MoveSelectedPath(IReadOnlyList<string> path)
        {
            var destination = _board.GetNode(path[path.Count - 1]);
            _run.MoveByRoll(path);
            _pendingRoll = 0;
            _candidatePaths = null;
            _combatNode = destination;

            if (_run.LastResolution.StartsEncounter)
            {
                _combat = _combatFactory.Create(destination, _starterHero, _run.State.CurrentHealth);
                _message = $"Encounter started: {_combat.Enemy.Name}. Choose an action.";
                return;
            }

            _message = _run.LastResolution.GrantsReward
                ? $"Landed on {destination.Type}. Reward granted."
                : $"Landed on {destination.Type}.";
        }

        private void ExecuteCombatAction(CombatActionType action)
        {
            var healthBefore = _combat.Player.Hp;
            var result = _combat.ExecutePlayerAction(action);
            var damageTaken = healthBefore - _combat.Player.Hp;
            if (damageTaken > 0 && !_run.State.IsDefeated)
            {
                _run.State.TakeDamage(damageTaken);
            }

            _message = result.Damage > 0 ? $"{action} dealt {result.Damage} damage." : $"{action} resolved.";
            if (!_combat.IsComplete) return;

            if (!_combat.PlayerWon || _run.State.IsDefeated)
            {
                _message = "You were defeated. The run is over.";
                return;
            }

            var reward = _combatNode.Type == BoardTileType.Boss ? 20 : (_combatNode.Type == BoardTileType.Elite ? 8 : 4);
            _run.State.AddGold(reward);

            if (_combatNode.Type == BoardTileType.Boss)
            {
                _run.CompleteBoss();
                _message = $"Boss defeated. +{reward} gold. Run complete.";
            }
            else
            {
                _message = $"Enemy defeated. +{reward} gold. Continue the run.";
            }

            _combat = null;
            _combatNode = null;
        }

        private static GUIStyle HeaderStyle() => new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold };
        private static GUIStyle MessageStyle() => new GUIStyle(GUI.skin.label) { wordWrap = true, fontSize = 18 };
    }
}
