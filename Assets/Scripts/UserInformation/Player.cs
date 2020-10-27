using UnityEngine;

// FIXME
// This class should not be monobehaviour, the Start data should be loaded when the game is opened
public class Player : MonoBehaviour
{
    // Players first custom lineup
    static PlayerLineupData [] lineups;
    static int activeLineupIndex;

    void Awake()
    {
        activeLineupIndex = 0; // FIXME stuff is temporary
                                // Change to whatever the players progress is
        lineups = new PlayerLineupData[1];
        lineups[0] = new PlayerLineupData();
        CharacterPosition[] chars = new CharacterPosition[3];
        CharacterPosition[] pets = new CharacterPosition[1]; // No pets just using chars
        chars[0].CharPosition = new Vector2(0.2f, 0.5f);
        chars[0].CharacterStats = new CharacterBonuses(HeroList.defender, 1, 1);
        pets[0].CharacterStats = new CharacterBonuses(PetList.soap, 1, 1);
        chars[0].CharacterStats.DmgPerc = 100;
        pets[0].CharPosition = new Vector2(0.8f, 0.2f);

        // Making zeus
        chars[1].CharPosition = new Vector2(0.6f, 0.8f);
        chars[1].CharacterStats = new CharacterBonuses(HeroList.zeus, 1, 1);

        // Making the archer
        chars[2].CharPosition = new Vector2(0.2f, 0.9f);

        // FIXME characterBonuses and Character Stats is sort of confusing
        // That is, why am I creating a character bonuses as the characters stats?
        chars[2].CharacterStats = new CharacterBonuses(HeroList.archer, 1, 1);
        chars[2].CharacterStats.AttackSpdPerc = 20;

        lineups[0].UpdateWaveList(chars, pets);

    }

    public static PlayerLineupData GetPlayerLineup ()
    {
        return lineups[activeLineupIndex];
    }
}
