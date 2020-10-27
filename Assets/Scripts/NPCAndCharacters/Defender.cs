using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Defender : Hero
{
    // TODO: Reword some of these variables. For example, heroBaseDmg should be heroAttackRating => Because healers also get this bonus
    // Also re-word CritDmg, as CritEffect => For the same reason as stated above
    const string charName = "defender";
    const float heroAttackSpd = 1.5f;
    int heroBaseHp = 100000;
    int heroBaseDmg = 100;
    float heroBaseAttackRange = 10f;
    int heroBaseArmor = 0;
    int heroBaseElemenResistance = 0;

    // These stats are in PERCENTAGES
    float heroBaseDmgRed = 0;
    float heroBaseAccuracy = 100;
    float heroBaseEvasion = 0;
    float heroBaseCritRate = 0; // Rate of a critical hit (in percentage) || SHOULD ALSO EFFECT HEALING!!!! TODO
    float heroBaseCritDmg = 200; // EXTRA damage applied
    float cdReduction = 0f; // Base CD reduction that is applied to ALL SKILLS
    float healingAMP = 100f;
    float healingRECVD = 100f;

    // These are the skills of the hero
    // IE: The 'names' of the skills
    enum SKILL_NAMES { PROTECTION }

    // All of the cool downs for each skill of this hero
    // Note that, ALL of these cooldowns are effected by the CDReduction stat
    // Decreasing a single cooldown would involve modifying this dictionary
    static Dictionary<int, float> skillCD = new Dictionary<int, float>
    {
        {(int)SKILL_NAMES.PROTECTION, 2f}
    };

    /// <summary>
    /// Sets up the base stats for this hero
    /// The base stats are calculated by the heroes stars and level rating, based on how the hero scales
    /// Once this is calculated, the attributes are passed to the parent, where the parent applies any necessary bonus stats
    /// </summary>
    /// <param name="level"></param>
    /// <param name="stars"></param>
    protected override void SetStats(int level, int stars)
    {
        // Now that we've calculated the heroes extra attributes, lets apply it and we're ready to fight
        InitCharacterStats(
            new CharacterStats(
            heroAttackSpd, heroBaseHp, heroBaseDmg, heroBaseAccuracy, heroBaseEvasion, heroBaseCritRate, heroBaseCritDmg,
            heroBaseAttackRange, heroBaseArmor, heroBaseElemenResistance, heroBaseDmgRed, cdReduction,
            healingAMP, healingRECVD, skillCD
        ));
    }

    protected override Dictionary<SkillType, Dictionary<bool, LinkedQueue<CharacterSkill>>> InitReadySkills()
    {
        // Initialize the heroes auto and passive skills
        Dictionary<SkillType, Dictionary<bool, LinkedQueue<CharacterSkill>>> readySkills =
            new Dictionary<SkillType, Dictionary<bool, LinkedQueue<CharacterSkill>>>
            {
                // Create skill list
                // Auto skills
                //{ SkillType.AUTO, new Dictionary<bool, LinkedQueue<CharacterSkill>>() },

                // Passives
                //{ SkillType.PASSIVE, new Dictionary<bool, LinkedQueue<CharacterSkill>>() }
            };

        // Create all possible skill paths for this character
        //readySkills[SkillType.PASSIVE].Add(true, new LinkedQueue<CharacterSkill>());
        //readySkills[SkillType.PASSIVE].Add(false, new LinkedQueue<CharacterSkill>());
        //readySkills[SkillType.AUTO].Add(true, new LinkedQueue<CharacterSkill>());
        //readySkills[SkillType.AUTO].Add(false, new LinkedQueue<CharacterSkill>());

        return readySkills;
    }

    // Creating the heroes ultimate skill
    protected override CharacterSkill UltimateSkill()
    {
        return new CharacterSkill(new HeroSkill(this.PROTECTION), ManualSkills.ULTIMATE, ManualSkillTypes.AUTO, (int)SKILL_NAMES.PROTECTION, true, false);
    }

    // User has casted the skill PROTECTION
    void PROTECTION(CharacterSkill skill, bool runSkill)
    {
        // Must be able to use the ultimate skill for this to work
        if (canUseUltimate && !ultOnCD)
        {
            if (runSkill)
            {
                // Run the skill
                this.ApplyAttributeOverTimeEFFectToTarget(Attributes.armor, 10000, true, 10f, this);
                Debug.Log("MAJOR ARMOR INCREASE APPLIED");
            }

            this.canUseSkill = true;
            this.usingSkill = false;
            this.canUseUltimate = false;

            if (!ultOnCD)
            {
                ultOnCD = true;

                // Skill is now on CD
                StartCoroutine(WaitForSkillCD(skill));
            }
        }
    }

    // Overriding methods
    // These are methods that uniquely define this hero
    // This hero has manual skills
    protected override bool HaveManualSkills()
    {
        return true;
    }

    // Returns the names of the skills associated with each manual skill for the hero
    protected override Dictionary<ManualSkills, string> ManualSkillNames()
    {
        return new Dictionary<ManualSkills, string>
        {
            { ManualSkills.ULTIMATE, SKILL_NAMES.PROTECTION.ToString() }
        };
    }

    public override string GetCharacterName() { return charName; }
    protected override string GetDefaultAttackClipName() { return "LH02_MeleeAttack_01"; }
}
