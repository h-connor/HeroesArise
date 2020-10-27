/// <summary>
/// Stores all stats for each character that this player has unlocked
/// </summary>
public static class PlayerCharacterStats
{
    // Calculates our actual Stats
    // This will take the character stats, and apply all bonuses to those stats
    public static void ApplyStatsAfterBonus(ref CharacterStats statsToApply, CharacterBonuses bonuses)
    {
        // No bonuses applied to this character
        if (bonuses == null)
            return;

        // HP
        statsToApply.MaxHP += bonuses.BaseHP;
        statsToApply.MaxHP = (int)(statsToApply.MaxHP + (statsToApply.MaxHP * (bonuses.HPPerc / 100f)));

        // DMG
        statsToApply.Dmg += bonuses.BaseDmg;
        statsToApply.Dmg = (int)(statsToApply.Dmg + (statsToApply.Dmg * (bonuses.DmgPerc / 100f)));

        // Attack Range
        statsToApply.AttackRange = (statsToApply.AttackRange + (statsToApply.AttackRange * (bonuses.AttckRangePerc / 100f)));

        // Attack Speed
        statsToApply.AttackSpeed = (statsToApply.AttackSpeed + (statsToApply.AttackSpeed * (bonuses.AttackSpdPerc / 100f)));

        // Armor / ElemResist
        statsToApply.Armor += bonuses.BaseArmor;
        statsToApply.ElementalResistance += bonuses.BaseElementalResist;

        statsToApply.Armor = (int)(statsToApply.Armor + (statsToApply.Armor * (bonuses.ArmorPerc / 100f)));
        statsToApply.ElementalResistance = (int)(statsToApply.ElementalResistance + (statsToApply.ElementalResistance * (bonuses.ElementalResistPerc / 100f)));

        // Dmg reduction
        statsToApply.DmgRed += statsToApply.DmgRed * (bonuses.DmgRedPerc / 100f);

        // Accuracy / Evasion
        statsToApply.Accuracy += statsToApply.Accuracy * (bonuses.AccuracyPerc / 100f);
        statsToApply.Evasion += statsToApply.Evasion * (bonuses.EvasionPerc / 100f);

        // Crit rate/dmg
        statsToApply.CritRate += statsToApply.CritRate * (bonuses.CritRate / 100f);
        statsToApply.CritDmg += statsToApply.CritRate * (bonuses.CritRate / 100f);

        // Cd Reduction
        statsToApply.CdReduction += statsToApply.CdReduction * (bonuses.CDReduction / 100f);

        // Healing AMP/RECV
        statsToApply.HealingAMP += statsToApply.HealingAMP * (bonuses.HealingAMP / 100f);
        statsToApply.HealingRCVD += statsToApply.HealingRCVD * (bonuses.HealingRECV / 100f);
    }
}
