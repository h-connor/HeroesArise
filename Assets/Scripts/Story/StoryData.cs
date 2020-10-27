using UnityEngine;

// StoryData
// This class contains all data of every story mission
// An array is stored containing all wave fights, and all boss information
// This includes the rewards and character information
public static class StoryData
{
    const int CHAPTER1_LEVELS = 8;

    // All wave fights for chapter 1
    public static LevelData [] Chapter1 { get; set; }
    readonly static Vector2 center = new Vector2(0.5f, 0.5f);

    static StoryData () {
        if (Chapter1 == null)
            Chapter1 = new LevelData[CHAPTER1_LEVELS];

        Chapter1[0] = new LevelData(
            new CharacterPosition [][]
            {
                // First wave Data
                new CharacterPosition [] {
                    new CharacterPosition (
                        new CharacterBonuses (HeroList.zeus, 4, 4),
                        new Vector2 (0.5f, 0.5f)                        
                        ),
                    new CharacterPosition (
                        new CharacterBonuses (HeroList.defender, 2, 4),
                        new Vector2 (0.4f, 0.5f)
                        ),
                    new CharacterPosition (
                        new CharacterBonuses (HeroList.defender, 1, 4),
                        new Vector2 (0.6f, 0.5f)
                        ),
                    new CharacterPosition (
                        new CharacterBonuses (HeroList.archer, 1, 4),
                        new Vector2 (0.6f, 0.8f)
                        )
                },
                new CharacterPosition [] {
                    new CharacterPosition (
                        new CharacterBonuses (HeroList.zeus, 4, 4),
                        new Vector2 (0.5f, 0.5f)
                        ),
                    new CharacterPosition (
                        new CharacterBonuses (HeroList.defender, 2, 4),
                        new Vector2 (0.4f, 0.5f)
                        ),
                    new CharacterPosition (
                        new CharacterBonuses (HeroList.defender, 1, 4),
                        new Vector2 (0.6f, 0.5f)
                        )
                }
            },

            new CharacterBonuses[] {
                new CharacterBonuses (BossList.Faphim, 4, 4)
            },

            new Rewards ()
            ) ;
    }

    public static LevelData GetLevelDataByChapterIndex (int chapter, int level)
    {
        switch (chapter)
        {
            case 1:
                return Chapter1[level - 1];
            default:
                break;
        }

        return null;
    }
}
