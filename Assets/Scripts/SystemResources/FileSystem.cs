// This class provides any necessary information on files
// Such as, A characters path to their models in the file system
public static class FileSystem
{
    // FIXME shouldn't have the same path
    const string HERO_MODEL_PATH = "Models/NPCAndCharacters/Heroes/";
    const string PET_MODEL_PATH = "Models/NPCAndCharacters/Pets/";
    const string BOSS_MODEL_PATH = "Models/NPCAndCharacters/Bosses/";
    const string PLAYER_HP_BARS = "Images/Fight/AllyHP";
    const string ENEM_HP_BARS = "Images/Fight/EnemHP";
    const string CHAR_SKILL_PATH = "Images/Characters/";
    const string HERO_MAN_SKILL_MODEL_PATH = "Models/NPCAndCharacters/Skills/";

    // Returns the directory to the images of the skills of the character
    // Image names are defined by the name of the skills [Handled in the heroes class]
    public static string GetSkillImageByCharSkillName (string charName, string skillName)
    {
        return CHAR_SKILL_PATH + charName + '/' + skillName;
    }

    /// <summary>
    /// Returns the model used for a specific skill of a hero
    /// For example, an arrow used for an archers ultimate skill
    /// </summary>
    /// <param name="heroName"></param>
    /// <returns></returns>
    public static string GetSkillModelByCharSkill (string charName, string skillName)
    {
        return HERO_MAN_SKILL_MODEL_PATH + charName + '/' + skillName;
    }

    // Returns the path of a heroes model from their name
    // Note that this REQUIRES the path to include the name of the hero itself
    // To change a heroes name simply modify the path to their resources and name of enum in HeroList
    public static string GetHeroModelPathByName (HeroList heroName)
    {
        return HERO_MODEL_PATH + heroName.ToString();
    }

    public static string GetHeroModelPathByName(string heroName)
    {
        return HERO_MODEL_PATH + heroName;
    }

    public static string GetBossModelPathByName (BossList bossName)
    {
        return BOSS_MODEL_PATH + bossName.ToString();
    }

    public static string GetBossModelPathByName (string bossName)
    {
        return BOSS_MODEL_PATH + bossName;
    }

    public static string GetPetModelPathByName(PetList petName)
    {
        return PET_MODEL_PATH + petName.ToString();
    }

    public static string GetPetModelPathByName(string petName)
    {
        return PET_MODEL_PATH + petName;
    }

    public static string GetAllyHpImagePath ()
    {
        return PLAYER_HP_BARS;
    }

    public static string GetEnemHpImagePath ()
    {
        return ENEM_HP_BARS;
    }
}
