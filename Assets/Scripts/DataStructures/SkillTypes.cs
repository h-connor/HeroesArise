/// <summary>
/// The types of the skills of characters
/// Skills can be passives [These will ALWAYS trigger even if another skill is being used]
/// Skills can be auto [Will trigger automatically when available]
/// Skills can be manual [Must be triggered by the player]
/// </summary>
public enum SkillType { AUTO, PASSIVE, MANUAL };

/// <summary>
/// A Character can have up to 3 manual skills 
/// IMPORTANT NOTE
/// If a hero has TWO skills, then one is Secondary and the other is an Ultimate
/// If a hero has ONE skill, then it is an ULTIMATE
/// A Primary skill is for those with 3 skills
/// </summary>
public enum ManualSkills { PRIMARY, SECONDARY, ULTIMATE };

// Defines what type manual skills can be
// AUTO = The skill will cast by itself, always using the 'default' result
// SELECT = The player will have to select a target
// DRAG = The player will have to drag across the screen to define how the skill will be used
// MANUAL skills can however selected to cast with their 'default' option
// All skills MUST have some sort of default option
public enum ManualSkillTypes { AUTO, SELECT, DRAG };
