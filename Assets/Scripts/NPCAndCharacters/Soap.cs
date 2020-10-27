using UnityEngine;

public class Soap : Pet
{
    const string charName = "soap";
    const float petAttackSpd = 1f;
    const int petBaseHp = 25;
    const int petBaseDmg = 3;
    const float heroBaseAttackRange = 15f;
    const int petBaseArmor = 0;
    const int petBaseElementResist = 0;
    const float petBaseDmgRed = 0; // This is a flat rate, this means PERCENT
    const float petBaseAccuracy = 0;
    const float petBaseEvasion = 0;
    const float petBaseCritRate = 0; // Rate of a critical hit (in percentage)
    const float petBaseCritDmg = 0; // EXTRA damage applied
    const float petCdReduction = 0; // Base CD reduction that is applied to ALL SKILLS
    const float healingAMP = 0;
    const float healingRECVD = 0;

    public override string GetCharacterName() { return charName; }

    private void Awake()
    {

        // Now that we've calculated the heroes extra attributes, lets apply it and we're ready to fight
        // Now that we've calculated the heroes extra attributes, lets apply it and we're ready to fight
        InitCharacterStats(
            new CharacterStats (
            petAttackSpd, petBaseHp, petBaseDmg, petBaseAccuracy, petBaseEvasion, petBaseCritRate, petBaseCritDmg,
            heroBaseAttackRange, petBaseArmor, petBaseElementResist, petBaseDmgRed, petCdReduction, 
            healingAMP, healingRECVD, null
        ));
    }
}
