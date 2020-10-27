using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zeus : Hero
{
    // TODO: Reword some of these variables. For example, heroBaseDmg should be heroAttackRating => Because healers also get this bonus
    // Also re-word CritDmg, as CritEffect => For the same reason as stated above
    const string charName = "zeus";
    const float heroAttackSpd = 1.5f;
    int heroBaseHp = 100000;
    int heroBaseDmg = 100;
    float heroBaseAttackRange = 20f;
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
    enum SKILL_NAMES { PASSIVE_HEAL = 0, AUTO_ATTACK, LightningStrike}
    
    // All of the cool downs for each skill of this hero
    // Note that, ALL of these cooldowns are effected by the CDReduction stat
    // Decreasing a single cooldown would involve modifying this dictionary
    static Dictionary<int, float> skillCD = new Dictionary<int, float>
    {
        {(int)SKILL_NAMES.PASSIVE_HEAL, 8f},
        {(int)SKILL_NAMES.AUTO_ATTACK, 3f},
        {(int)SKILL_NAMES.LightningStrike, 1f}
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
            new CharacterStats (
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
                { SkillType.AUTO, new Dictionary<bool, LinkedQueue<CharacterSkill>>() },

                // Passives
                { SkillType.PASSIVE, new Dictionary<bool, LinkedQueue<CharacterSkill>>() }
            };

        // Create all possible skill paths for this character
        readySkills[SkillType.PASSIVE].Add(true, new LinkedQueue<CharacterSkill>());
        //readySkills[SkillType.PASSIVE].Add(false, new LinkedQueue<CharacterSkill>());
        //readySkills[SkillType.AUTO].Add(true, new LinkedQueue<CharacterSkill>());
        readySkills[SkillType.AUTO].Add(false, new LinkedQueue<CharacterSkill>());

        // Create all the skills for this character
        CharacterSkill[] zeusSkills = new CharacterSkill[]
        {
            new CharacterSkill(new HeroSkill(this.PassiveHeal), SkillType.PASSIVE, (int)SKILL_NAMES.PASSIVE_HEAL, true, false),
            new CharacterSkill(new HeroSkill(this.AutoAttack), SkillType.AUTO, (int)SKILL_NAMES.AUTO_ATTACK, false, false)
        };

        return readySkills;
    }

    // Creating the heroes ultimate skill
    protected override CharacterSkill UltimateSkill()
    {
        return new CharacterSkill(new HeroSkill(this.LightningStrike), ManualSkills.ULTIMATE, (int)SKILL_NAMES.LightningStrike, true, false, false, true);
    }

    // User has casted the skill Lightning Strike
    void LightningStrike(CharacterSkill skill, bool runSkill)
    {
        // Must be able to use the ultimate skill for this to work
        if (canUseUltimate && !ultOnCD)
        {
            if (runSkill)
            {
                // Run the skill
                this.HurtHero(skill.Target, new Damage (100000, DamageTypes.NORMAL, heroBaseCritRate, heroBaseCritDmg, heroBaseAccuracy));
                Debug.Log("BOOOOOOOM");
                Debug.Log("Just smoked" + skill.Target.name);
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

    public void AutoAttack (CharacterSkill ourSkill, bool runSkill)
    {
        if (runSkill)
        {
            // Perform the skill
            this.HurtHero(this.Target, new Damage(heroBaseDmg * 2, DamageTypes.NORMAL, heroBaseCritRate, heroBaseCritDmg, heroBaseAccuracy));
        }

        // The hero can now use another skill
        this.canUseSkill = true;
        this.usingSkill = false;

        // Skill is now on CD
        StartCoroutine(WaitForSkillCD(ourSkill));
    }

    // Perform the skill, and then wait until its off cooldown
    // The runSkill parameter will not execute the skill, and instead put it onto CD
    public void PassiveHeal (CharacterSkill ourSkill, bool runSkill)
    {
        if (runSkill)
        {
            // Apply A Heal over 5s
            ApplyOTDmgOrHealEffect(
                new OTEffect(EffectTypes.NORMAL, 3, new Damage(1000, heroBaseCritRate, heroBaseCritDmg, heroBaseAccuracy)), this
                );
        }

        // The hero can now use another skill
        this.canUseSkill = true;
        this.usingSkill = false;
        
        // Skill is now on CD
        StartCoroutine(WaitForSkillCD(ourSkill));
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
            { ManualSkills.ULTIMATE, SKILL_NAMES.LightningStrike.ToString() }
        };
    }

    public override string GetCharacterName() { return charName; }

    protected override string GetDefaultAttackClipName() { return "LH03_MeleeAttack_01"; }
}
