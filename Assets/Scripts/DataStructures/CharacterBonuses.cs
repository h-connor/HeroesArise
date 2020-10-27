public class CharacterBonuses
{
    public string CharacterName { get; set; } // The hero that this script references to [So he can be instantiated with these stats]


    public int Level { get; set; }
    public int Stars { get; set; }

    /// <summary>
    /// Each attribute can Either be a Base [Extra stats]
    /// Or, A percentage increase
    /// Attributes that are stored only in percentage, cannot have a base applied
    /// </summary>
    public int BaseHP { get; set; }
    public float HPPerc { get; set; }
    public int BaseDmg { get; set; }
    public float DmgPerc { get; set; }
    public float AttckRangePerc { get; set; }
    public int BaseArmor { get; set; }
    public float ArmorPerc { get; set; }
    public int BaseElementalResist { get; set; }
    public float ElementalResistPerc { get; set; }
    public float DmgRedPerc { get; set; }
    public float AccuracyPerc { get; set; }
    public float EvasionPerc { get; set; }

    public float CritRate { get; set; }
    public float CritDmg { get; set; }

    public float CDReduction { get; set; }
    public float HealingAMP { get; set; }
    public float HealingRECV { get; set; }
    public float AttackSpdPerc { get; set; }

    // Hero defines this character
    public CharacterBonuses(HeroList charName, int level, int stars)
    {
        this.CharacterName = charName.ToString();
        this.Level = level;
        this.Stars = stars;
    }

    // Pet defines this character
    public CharacterBonuses(PetList charName, int level, int stars)
    {
        this.CharacterName = charName.ToString();
        this.Level = level;
        this.Stars = stars;
    }

    // Boss defines this character
    public CharacterBonuses(BossList charName, int level, int stars)
    {
        this.CharacterName = charName.ToString();
        this.Level = level;
        this.Stars = stars;
    }
}
