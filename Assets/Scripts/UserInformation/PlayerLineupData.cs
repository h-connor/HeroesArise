using UnityEngine;
using System.Collections;

// Stores the data for the lineup, followed by the cooresponding heroes in the players lineup
public class PlayerLineupData
{
    // playersWaveData
    CharacterPosition[] waveHeroes;
    CharacterPosition[] wavePets;

    // Updates the current players lineup that is active
    // [Re-fill its contents]
    // The hero positions index each coorespond to the same index in its hero list
    public void UpdateWaveList(CharacterPosition[] heroList, CharacterPosition[] petList)
    {
        waveHeroes = heroList;
        wavePets = petList;
    }

    public CharacterPosition[] GetHeroWaveData ()
    {
        return this.waveHeroes;
    }

    public CharacterPosition[] GetPetWaveData ()
    {
        return this.wavePets;
    }
}
