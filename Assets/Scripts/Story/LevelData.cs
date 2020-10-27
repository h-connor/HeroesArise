public class LevelData
{
    // Stores the position of its cooresponding spawn point, followed by the heroes stats
    // Each array is a new wave
    public CharacterPosition[][] WaveData { get; set; }
    public CharacterBonuses[] BossData { get; set; }
    public Rewards rewards; // List of rewards for completing the stage

    public LevelData (CharacterPosition [][] waveData, CharacterBonuses[] bossData, Rewards rewards)
    {
        this.WaveData = waveData;
        this.BossData = bossData;
        this.rewards = rewards;
    }
}