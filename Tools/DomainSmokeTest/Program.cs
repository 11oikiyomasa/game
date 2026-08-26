using DiceRoguelike.Game.Domain;

static void Assert(bool condition, string message)
{
    if (!condition)
        throw new Exception(message);
}

var a = new Dice(new SeededRandom(12345)).Roll(6);
var b = new Dice(new SeededRandom(12345)).Roll(6);
Assert(a.Value == b.Value, "Seeded dice must be deterministic.");
Assert(a.Value >= 1 && a.Value <= 6, "D6 result must be within 1..6.");

var board = new BoardDefinition(new[]
{
    new BoardNode(0, BoardNodeType.Start),
    new BoardNode(1, BoardNodeType.Normal),
    new BoardNode(2, BoardNodeType.Enemy),
    new BoardNode(3, BoardNodeType.Goal)
});

var run = new RunState();
var move = run.Move(board, new DiceRoll(6, 6));
Assert(move.NewPosition == 3, "Movement must clamp to the final node.");
Assert(move.ReachedGoal, "Reaching Goal must finish the run as a win.");
Assert(run.IsFinished && run.IsWon, "Run state must expose the terminal win state.");

var state = new GameStateMachine();
state.StartTeamSelection();
state.StartRun();
state.FinishRun(true);
Assert(state.Phase == GamePhase.Victory, "State machine must reach Victory after a win.");

Console.WriteLine("DOMAIN SMOKE TEST PASS");
