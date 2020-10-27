using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Archer : Hero
{
    // TODO: Reword some of these variables. For example, heroBaseDmg should be heroAttackRating => Because healers also get this bonus
    // Also re-word CritDmg, as CritEffect => For the same reason as stated above
    const string charName = "archer";
    const float ULT_BASE_MVM_SPEED = 7f; // Base movement speed of the object of his ultimate ability
    const float heroAttackSpd = 1f;
    int heroBaseHp = 100000;
    int heroBaseDmg = 300;
    float heroBaseAttackRange = 30f;
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
    enum SKILL_NAMES { ARROW_STRIKE }

    // All of the cool downs for each skill of this hero
    // Note that, ALL of these cooldowns are effected by the CDReduction stat
    // Decreasing a single cooldown would involve modifying this dictionary
    static Dictionary<int, float> skillCD = new Dictionary<int, float>
    {
        { (int)SKILL_NAMES.ARROW_STRIKE, 0f }
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

                // Passives
            };

        return readySkills;
    }

    // Creating the heroes ultimate skill
    protected override CharacterSkill UltimateSkill()
    {
        return new CharacterSkill(new HeroSkill(this.ArrowStrike), ManualSkills.ULTIMATE, ManualSkillTypes.DRAG, (int)SKILL_NAMES.ARROW_STRIKE, true, false);
    }

    // User has casted the skill Lightning Strike
    void ArrowStrike(CharacterSkill skill, bool runSkill)
    {
        // Must be able to use the ultimate skill for this to work
        if (canUseUltimate && !ultOnCD)
        {
            if (runSkill)
            {
                // Getting bow used for image
                GameObject model = CreateTempObject(Resources.Load<GameObject>(FileSystem.GetSkillModelByCharSkill(charName, SKILL_NAMES.ARROW_STRIKE.ToString())));
                model.transform.SetParent(this.transform);
                model.transform.position = this.transform.position;

                // Grab the AOE skill component
                AOESkill aoe = model.GetComponent<AOESkill>();

                if (aoe == null)
                    throw new ComponentNotFound("Error, AOE object not found");

                aoe.Init(this, false, this.ArrowStrikeHit);

                // Apply the motion to the model object
                Rigidbody body = model.GetComponent<Rigidbody>();

                if (body == null)
                    throw new ComponentNotFound("Error, no body found");

                float dist = Vector3.Distance(skill.MouseStartPos, skill.MouseEndPos);
                Vector3 mouseDirection = (skill.MouseStartPos - skill.MouseEndPos).normalized;
                Vector3 worldDirection = new Vector3(mouseDirection.x, 0, mouseDirection.y);

                body.AddForce(worldDirection * dist * ULT_BASE_MVM_SPEED);
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

    // Arrow strike skill has hit this target
    void ArrowStrikeHit (Character hitTarget, AOESkill skill)
    {
        int dmg = 55555;
        this.HurtHero(hitTarget, new Damage(dmg + (int)(0.3f * dmg * (skill.HitCount - 1)), this.heroBaseCritRate, this.heroBaseCritDmg, this.heroBaseAccuracy));
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
            { ManualSkills.ULTIMATE, SKILL_NAMES.ARROW_STRIKE.ToString() }
        };
    }

    public override string GetCharacterName() { return charName; }
    protected override string GetDefaultAttackClipName() { return "LH04_MeleeAttack_01"; }
}
