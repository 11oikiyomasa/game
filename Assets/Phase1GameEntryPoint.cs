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
        private readonly HeroDefinition[] _starterHeroes =
        {
            new HeroDefinition("starter-sentinel", "Ember Sentinel", HeroRole.Warrior, 30, 6, 1, 10, 2),
            new HeroDefinition("starter-mage", "Ash Mage", HeroRole.Mage, 24, 8, 0, 20, 2),
            new HeroDefinition("starter-warden", "Thorn Warden", HeroRole.Tank, 38, 4, 3, 5, 2)
        };

        private GameStateMachine _stateMachine;
        private BoardGraph _board;
        private RunController _run;
        private CombatEncounterFactory _combatFactory;
        private CombatEngine _combat;
        private HeroDefinition _selectedHero;
        private BoardNode _combatNode;
        private string _message;
        private int _pendingRoll;
        private IReadOnlyList<IReadOnlyList<string>> _candidatePaths;

        private void Awake()
        {
            _stateMachine = new GameStateMachine();
            _stateMachine.TransitionTo(GameState.MainMenu);
            _message = "Enter the dungeon. Choose a hero, then roll the die.";
        }

        private void OnGUI()
        {
            var width = Mathf.Min(Screen.width - 32f, 720f);
            GUILayout.BeginArea(new Rect(16f, 16f, width, Screen.height - 32f));
            GUILayout.Label("ASHEN PATH", HeaderStyle());
            GUILayout.Label($"State: {_stateMachine.Current}", SmallStyle());
            GUILayout.Space(8f);

            switch (_stateMachine.Current)
            {
                case GameState.MainMenu:
                    DrawMainMenu();
                    break;
                case GameState.HeroSelection:
                    DrawHeroSelection();
                    break;
                case GameState.RunInitialization:
                    GUILayout.Label("Preparing the dungeon...", MessageStyle());
                    break;
                case GameState.Board:
                case GameState.DiceRoll:
                case GameState.Movement:
                case GameState.TileResolution:
                case GameState.Encounter:
                case GameState.Reward:
                case GameState.Upgrade:
                    DrawRun();
                    break;
                case GameState.Combat:
                    DrawCombat();
                    break;
                case GameState.RunComplete:
                    DrawTerminal("VICTORY", "The Ashen Warden has fallen. The run is complete.");
                    break;
                case GameState.RunDefeat:
                    DrawTerminal("DEFEAT", "Your expedition ended in darkness.");
                    break;
                default:
                    DrawTerminal("END", "This prototype state is not yet playable.");
                    break;
            }

            GUILayout.EndArea();
        }

        private void DrawMainMenu()
        {
            GUILayout.Label("A deterministic dice-driven dungeon run prototype.", MessageStyle());
            GUILayout.Space(16f);
            if (GUILayout.Button("START RUN", GUILayout.Height(72f)))
            {
                _stateMachine.TransitionTo(GameState.HeroSelection);
            }
        }

        private void DrawHeroSelection()
        {
            GUILayout.Label("Choose your starter hero", HeaderStyle());
            GUILayout.Space(8f);

            foreach (var hero in _starterHeroes)
            {
                GUILayout.BeginVertical("box");
                GUILayout.Label(hero.Name, HeaderStyle());
                GUILayout.Label($"{hero.Role}   HP {hero.BaseHp}   ATK {hero.BaseAttack}   DEF {hero.BaseDefense}");
                GUILayout.Label($"Crit {hero.BaseCritChancePercent}% x{hero.BaseCritMultiplier}");
                if (GUILayout.Button("SELECT", GUILayout.Height(52f)))
                {
                    _selectedHero = hero;
                    InitializeRun();
                    break;
                }
                GUILayout.EndVertical();
                GUILayout.Space(8f);
            }
        }

        private void InitializeRun()
        {
            _stateMachine.TransitionTo(GameState.RunInitialization);

            var rng = new SeededRng(20260827);
            _board = ProceduralBoardFactory.Create(rng, rows: 10, lanes: 3);
            var state = new RunState("phase1-run", _board, maxHealth: _selectedHero.BaseHp, startingGold: 5);
            _run = new RunController(_board, state, rng, new Dice(6));
            _combatFactory = new CombatEncounterFactory(rng, new Dice(6));
            _pendingRoll = 0;
            _candidatePaths = null;
            _combat = null;
            _combatNode = null;
            _message = $"{_selectedHero.Name} enters the dungeon. Roll the die.";

            _stateMachine.TransitionTo(GameState.Board);
        }

        private void DrawRun()
        {
            GUILayout.Label($"Hero: {_selectedHero.Name}");
            GUILayout.Label($"Node: {_run.State.CurrentNodeId}   HP: {_run.State.CurrentHealth}/{_run.State.MaxHealth}   Gold: {_run.State.Gold}");
            GUILayout.Space(8f);
            GUILayout.Label(_message, MessageStyle());
            GUILayout.Space(12f);

            GUI.enabled = _pendingRoll == 0 && !_run.State.IsComplete && !_run.State.IsDefeated;
            if (GUILayout.Button("ROLL D6", GUILayout.Height(72f)))
            {
                _stateMachine.TransitionTo(GameState.DiceRoll);
                _pendingRoll = _run.Roll();
                _candidatePaths = BoardPathFinder.FindPaths(_board, _run.State.CurrentNodeId, _pendingRoll, maxPaths: 8);
                _message = $"Rolled {_pendingRoll}. Choose a legal route.";
                _stateMachine.TransitionTo(GameState.Movement);
            }

            GUI.enabled = _pendingRoll > 0 && !_run.State.IsComplete && !_run.State.IsDefeated;
            if (_candidatePaths != null)
            {
                for (var i = 0; i < _candidatePaths.Count; i++)
                {
                    var path = _candidatePaths[i];
                    var destination = _board.GetNode(path[path.Count - 1]);
                    if (GUILayout.Button($"ROUTE {i + 1} → {destination.Type}", GUILayout.Height(52f)))
                    {
                        MoveSelectedPath(path);
                        break;
                    }
                }
            }
            GUI.enabled = true;
        }

        private void MoveSelectedPath(IReadOnlyList<string> path)
        {
            _stateMachine.TransitionTo(GameState.TileResolution);
            var destination = _board.GetNode(path[path.Count - 1]);
            _run.MoveByRoll(path);
            _pendingRoll = 0;
            _candidatePaths = null;
            _combatNode = destination;

            if (_run.LastResolution.StartsEncounter)
            {
                _stateMachine.TransitionTo(GameState.Encounter);
                _combat = _combatFactory.Create(destination, _selectedHero, _run.State.CurrentHealth);
                _message = $"Encounter: {_combat.Enemy.Name}. Choose an action.";
                _stateMachine.TransitionTo(GameState.Combat);
                return;
            }

            if (_run.LastResolution.GrantsReward)
            {
                _stateMachine.TransitionTo(GameState.Reward);
                _message = $"{destination.Type} resolved. Reward granted.";
                _stateMachine.TransitionTo(GameState.Board);
                return;
            }

            _message = $"Landed on {destination.Type}.";
            _stateMachine.TransitionTo(GameState.Board);
        }

        private void DrawCombat()
        {
            GUILayout.Label($"{_combat.Player.Name}: {_combat.Player.Hp}/{_combat.Player.MaxHp} HP");
            GUILayout.Label($"{_combat.Enemy.Name}: {_combat.Enemy.Hp}/{_combat.Enemy.MaxHp} HP");
            GUILayout.Label($"Turn {_combat.Turn + 1}", SmallStyle());
            GUILayout.Label(_message, MessageStyle());
            GUILayout.Space(12f);

            GUI.enabled = !_combat.IsComplete;
            if (GUILayout.Button("ATTACK", GUILayout.Height(58f))) ExecuteCombatAction(CombatActionType.Attack);
            if (GUILayout.Button("DEFEND", GUILayout.Height(58f))) ExecuteCombatAction(CombatActionType.Defend);
            if (GUILayout.Button("SKILL", GUILayout.Height(58f))) ExecuteCombatAction(CombatActionType.Skill);
            GUI.enabled = true;
        }

        private void ExecuteCombatAction(CombatActionType action)
        {
            var healthBefore = _combat.Player.Hp;
            var result = _combat.ExecutePlayerAction(action);
            var damageTaken = healthBefore - _combat.Player.Hp;
            if (damageTaken > 0)
            {
                _run.State.TakeDamage(damageTaken);
            }

            _message = result.Damage > 0 ? $"{action}: {result.Damage} damage." : $"{action} resolved.";
            if (!_combat.IsComplete) return;

            if (!_combat.PlayerWon || _run.State.IsDefeated)
            {
                _stateMachine.TransitionTo(GameState.RunDefeat);
                return;
            }

            var reward = _combatNode.Type == BoardTileType.Boss ? 20 : (_combatNode.Type == BoardTileType.Elite ? 8 : 4);
            _run.State.AddGold(reward);
            _stateMachine.TransitionTo(GameState.Reward);

            if (_combatNode.Type == BoardTileType.Boss)
            {
                _run.CompleteBoss();
                _stateMachine.TransitionTo(GameState.RunComplete);
                _message = $"Boss defeated. +{reward} gold.";
                return;
            }

            _combat = null;
            _combatNode = null;
            _message = $"Enemy defeated. +{reward} gold. Continue.";
            _stateMachine.TransitionTo(GameState.Board);
        }

        private void DrawTerminal(string title, string subtitle)
        {
            GUILayout.Label(title, HeaderStyle());
            GUILayout.Label(subtitle, MessageStyle());
            GUILayout.Space(16f);
            if (GUILayout.Button("BACK TO MENU", GUILayout.Height(64f)))
            {
                _stateMachine = new GameStateMachine();
                _stateMachine.TransitionTo(GameState.MainMenu);
                _message = "Enter the dungeon. Choose a hero, then roll the die.";
            }
        }

        private static GUIStyle HeaderStyle() => new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold };
        private static GUIStyle MessageStyle() => new GUIStyle(GUI.skin.label) { wordWrap = true, fontSize = 18 };
        private static GUIStyle SmallStyle() => new GUIStyle(GUI.skin.label) { fontSize = 14 };
    }
}
