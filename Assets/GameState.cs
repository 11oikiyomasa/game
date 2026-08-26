namespace DiceRoguelike.Core
{
    public enum GameState
    {
        Boot,
        MainMenu,
        HeroSelection,
        RunInitialization,
        Board,
        DiceRoll,
        Movement,
        TileResolution,
        Encounter,
        Combat,
        Reward,
        Upgrade,
        Boss,
        RunComplete,
        RunDefeat,
        MetaProgression
    }
}
