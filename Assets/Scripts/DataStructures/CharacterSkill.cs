// This defines a skill for the character
// A skill performs some action, and has a cooldown
// Skills can be automatically applied, or activated by the player
// Skill methods are all void with no arguments
public delegate void HeroSkill(CharacterSkill skill, bool waitForCD);
public delegate void AOEHeroSkill(Character target, AOESkill theSkill); // An AOE skill applied to some target
public class CharacterSkill
{
    public HeroSkill Skill { get; private set; }
    public SkillType Type { get; private set; } // Type of the skill
    public ManualSkills ManSkill { get; private set; } // Type of manual skill [If this skill is manual]
    public ManualSkillTypes ManSkillType { get; private set; }
    public Character Target { get; set; } // Depending on the type of the skill, [If its a select], this is the target
    public UnityEngine.Vector2 MouseStartPos { get; set; } // Depending on the skill type [If its DRAG], this is where we started our dragging
    public UnityEngine.Vector2 MouseEndPos { get; set; } // Depending on the skill type [If its DRAG], this is where we stopped dragging
    public bool CanSelectAlly { get; private set; } // If this skill is SELECT, then this will tell us if it can select an ally
    public bool CanSelectEnemy { get; private set; } // If this skill is SELECT, then this will tell us if it can select an enemy
    public int SkillName { get; private set; } 
    public bool MoveUsable { get; private set; } // Can this skill trigger while the player is moving?
    public bool RunAtStart { get; private set; }

    public void RunSkill ()
    {
        Skill(this, true);
    }

    /// <summary>
    /// Defines a characters skill.
    /// A Skill is a reference to the method that will be executed when the character uses that skill
    /// The Type defines the type of the skill, IE: Auto, Manual, Passive
    /// The CD is how long it takes before the skill will be re-casted after being used
    /// The moveUsable attribute defines if the skill can be casted while the character is moving.
    /// The runAtGameStart attribute is an addition for heroes that have a skill that won't start off cooldown when the battle begins
    /// Instead, they will trigger right away, then go off cooldown again
    /// This can be particularily useful for passives that begin at the start of battle, even if there are not enemies.
    /// </summary>
    /// <param name="skill"></param>
    /// <param name="type"></param>
    /// <param name="cd"></param>
    /// <param name="moveUsable"></param>
    /// <param name="runAtGameStart"></param>
    public CharacterSkill(HeroSkill skill, SkillType type, int skillName, bool moveUsable, bool runAtGameStart)
    {
        this.Skill = skill;
        this.Type = type;
        this.SkillName = skillName;
        this.MoveUsable = moveUsable;
        this.RunAtStart = runAtGameStart;

        Skill(this, runAtGameStart);
    }

    // For manual skills, we also define a Manual Skill Type [IE: If the skill is primary secondary or an ultimate]
    public CharacterSkill(HeroSkill skill, ManualSkills manSkill, ManualSkillTypes skillType, int skillName, bool moveUsable, bool runAtGameStart)
    {
        this.Skill = skill;
        this.Type = SkillType.MANUAL;
        this.ManSkillType = skillType;
        this.ManSkill = manSkill;
        this.SkillName = skillName;
        this.MoveUsable = moveUsable;
        this.RunAtStart = runAtGameStart;

        Skill(this, runAtGameStart);
    }

    // For SELECT skills
    // Here we define an additional boolean that tells us if we can select allies, and/or select targetable enemies
    public CharacterSkill(HeroSkill skill, ManualSkills manSkill, int skillName, bool moveUsable, 
                          bool runAtGameStart, bool canSelectAlly, bool canSelectEnem, ManualSkillTypes skillType = ManualSkillTypes.SELECT)
    {
        this.Skill = skill;
        this.Type = SkillType.MANUAL;
        this.ManSkillType = skillType;
        this.ManSkill = manSkill;
        this.SkillName = skillName;
        this.MoveUsable = moveUsable;
        this.RunAtStart = runAtGameStart;
        this.CanSelectAlly = canSelectAlly;
        this.CanSelectEnemy = canSelectEnem;

        Skill(this, runAtGameStart);
    }
}
