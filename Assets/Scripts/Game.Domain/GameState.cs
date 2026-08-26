namespace DiceRoguelike.Game.Domain
{
    public enum GamePhase
    {
        MainMenu,
        TeamSelection,
        Running,
        Victory,
        Defeat
    }

    public sealed class GameStateMachine
    {
        public GamePhase Phase { get; private set; }

        public GameStateMachine()
        {
            Phase = GamePhase.MainMenu;
        }

        public void StartTeamSelection()
        {
            Require(GamePhase.MainMenu);
            Phase = GamePhase.TeamSelection;
        }

        public void StartRun()
        {
            Require(GamePhase.TeamSelection);
            Phase = GamePhase.Running;
        }

        public void FinishRun(bool won)
        {
            Require(GamePhase.Running);
            Phase = won ? GamePhase.Victory : GamePhase.Defeat;
        }

        private void Require(GamePhase expected)
        {
            if (Phase != expected)
                throw new System.InvalidOperationException($"Expected phase {expected}, but was {Phase}.");
        }
    }
}
