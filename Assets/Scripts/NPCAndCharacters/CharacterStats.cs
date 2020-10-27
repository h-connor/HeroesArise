using System.Collections.Generic;

public enum  Attributes 
{ AttackRange, hp, dmg, armor, elementResist, DmgReduction, accuracy, evasion, critRate, critDmg, cdReduction, healingAMP, healingRECVD, attckSpd }

public class CharacterStats
{
    // ~~~~~~~~~~ Heroes stats ~~~~~~~~~~ \\// ~~~~~~~~~~ Heroes stats ~~~~~~~~~~ \\

    public float AttackRange { get; set; }
    public int MaxHP { get; set; }
    public int Dmg { get; set; }
    public int Armor { get; set; }
    public int ElementalResistance { get; set; }
    public float DmgRed { get; set; } // Flat % of dmg reduced
    public float Accuracy { get; set; } // Odds of missing an attack
    public float Evasion { get; set; } // Odds of avoiding taken damage
    public float CritRate { get; set; } // Odds of extra damage being applieedd
    public float CritDmg { get; set; } // Amount of extra damage applied
    public float CdReduction { get; set; } // Extra CD applied to all skills
    public float HealingAMP { get; set; } // % healing AMPED by us
    public float HealingRCVD { get; set; } // % extra additional healing we receive
    public float AttackSpeed { get; set;} // The heroes attack speed


    // The skills cooldowns of this hero
    // The int represents the name of the skill [Based on the skill name defined for that character] as the enum representation
    // The float represents the cooldown of that skill
    public Dictionary<int, float> SkillCooldowns { get; set; }

    public CharacterStats(
        float attackSpeed,
        int hp, int dmg, float accuracy, float evasion, float critRate, float critDmg,
        float attackRange, int armor, int elementalResistance, float dmgRed, float cdReduction, float healingAMP, float healingRCVD,
        Dictionary<int, float> skillCooldowns)
    {
        this.AttackSpeed = attackSpeed;
        this.MaxHP = hp;
        this.Dmg = dmg;
        this.Accuracy = accuracy;
        this.Evasion = evasion;
        this.CritRate = critRate;
        this.CritDmg = critDmg;
        this.AttackRange = attackRange;
        this.Armor = armor;
        this.ElementalResistance = elementalResistance;
        this.DmgRed = dmgRed;
        this.CdReduction = cdReduction;
        this.HealingAMP = healingAMP;
        this.HealingRCVD = healingRCVD;
        this.SkillCooldowns = skillCooldowns;
    }

    public int GetAttributeByName (Attributes attribute)
    {
        switch (attribute)
        {
            case Attributes.hp:
                return this.MaxHP;
            case Attributes.dmg:
                return this.Dmg;
            case Attributes.armor:
                return this.Armor;
            case Attributes.elementResist:
                return this.ElementalResistance;
        }

        // NAN
        return -1;
    }

    public float GetAttrbuteByName (Attributes attribute)
    {
        switch (attribute)
        {
            case Attributes.AttackRange:
                return this.AttackRange;
            case Attributes.DmgReduction:
                return this.DmgRed;
            case Attributes.accuracy:
                return this.Accuracy;
            case Attributes.evasion:
                return this.Evasion;
            case Attributes.critRate:
                return this.CritRate;
            case Attributes.critDmg:
                return this.CritDmg;
            case Attributes.cdReduction:
                return this.CdReduction;
            case Attributes.healingAMP:
                return this.HealingAMP;
            case Attributes.healingRECVD:
                return this.HealingRCVD;
            case Attributes.attckSpd:
                return this.AttackSpeed;
        }

        // NOT VALID
        return -1;
    }

    // Increase the attribute by the given amount, base on the given name.
    // Returns true if an increase was applied
    // Attributes stored as integers cannot receive a base amount increase
    public bool ApplyIncreaseByName (Attributes attribute, int increaseAmount)
    {
        switch (attribute)
        {
            case Attributes.hp:
                this.MaxHP += increaseAmount;
                return true;
            case Attributes.dmg:
                this.Dmg += increaseAmount;
                return true;
            case Attributes.armor:
                this.Armor += increaseAmount;
                return true;
            case Attributes.elementResist:
                this.ElementalResistance += increaseAmount;
                return true;
        }

        // NOT VALID
        return false;
    }

    // Increase the attribute by the given amount, base on the given name.
    // Returns true if an increase was applied
    // This increase is PERCENTAGE BASED
    // Any attribute can receive a percent increase
    public bool ApplyIncreaseByName(Attributes attribute, float percIncrease)
    {
        switch (attribute)
        {
            case Attributes.hp:
                this.MaxHP += (int) (this.MaxHP * percIncrease);
                return true;
            case Attributes.dmg:
                this.Dmg += (int) (this.Dmg * percIncrease);
                return true;
            case Attributes.armor:
                this.Armor += (int) (this.Armor * percIncrease);
                return true;
            case Attributes.elementResist:
                this.ElementalResistance += (int) (this.ElementalResistance * percIncrease);
                return true;
            case Attributes.AttackRange:
                this.AttackRange += this.AttackRange * percIncrease;
                return true;
            case Attributes.DmgReduction:
                this.DmgRed += this.DmgRed * percIncrease;
                return true;
            case Attributes.accuracy:
                this.Accuracy += this.Accuracy * percIncrease;
                return true;
            case Attributes.evasion:
                this.Evasion += this.Evasion * percIncrease;
                return true;
            case Attributes.critRate:
                this.CritRate += this.CritRate * percIncrease;
                return true;
            case Attributes.critDmg:
                this.CritDmg += this.CritDmg * percIncrease;
                return true;
            case Attributes.cdReduction:
                this.CdReduction += this.CdReduction * percIncrease;
                return true;
            case Attributes.healingAMP:
                this.HealingAMP += this.HealingAMP * percIncrease;
                return true;
            case Attributes.healingRECVD:
                this.HealingRCVD += this.HealingRCVD * percIncrease;
                return true;
            case Attributes.attckSpd:
                this.AttackSpeed += this.AttackSpeed * percIncrease;
                return true;
        }

        // NOT VALID
        return false;
    }
}
