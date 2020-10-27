using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

[RequireComponent (typeof (Animator))]
public class Character : MonoBehaviour
{
    /// <summary>
    /// Animation variables
    /// </summary>
    
    protected Animator anim;
    // Hashing states names
    static readonly int isDeadStateAnim = Animator.StringToHash("Dead");
    static readonly int isAttackingStateAnim = Animator.StringToHash("Base Attack");
    static readonly int isWalkingStateAnim = Animator.StringToHash("Walk");

    // Hashing conditions thatt can transition between states
    static readonly int baseAttackDurationAnim = Animator.StringToHash("baseAttackSpd");
    static readonly int isMovingAnim = Animator.StringToHash("isMoving");
    static readonly int isDeadAnim = Animator.StringToHash("isDead");
    static readonly int isAttackingAnim = Animator.StringToHash("isAttacking");
    AnimationClip attackAnimClip;

    // How quickly the hero can turn towards a target
    const float TURN_SPEED = 4.5f;

    /// <summary>
    /// This string defines if the character is targetable or not
    /// IE: This means the player can target it with their skills
    /// </summary>
    public const string TARGET_TAG_ENEM = "Targetable Enemy";
    public const string TARGET_TAG_ALLY = "Targetable Ally";
    public void IsTargetableEnemy()
    {
        this.gameObject.tag = TARGET_TAG_ENEM;
    }
    public void IsTargetableAlly()
    {
        this.gameObject.tag = TARGET_TAG_ALLY;
    }

    public float ExtraBossAttackRange { get; set; }

    /// <summary>
    /// Defined a queue of all the skills that are ready to be used
    /// IE: Skills that are off cooldowns
    /// Each queue of skills is separated by their skill types
    /// Within each skill types, contains another dictionary
    /// This dictionary separates skills that can be used while moving
    /// With skills that cannot be used while moving
    /// IE: Are the skills interruptabled??
    /// This is for optimization purposes, so if we are moving we only taking ready skills that can be activated whilst doing so
    /// This will Grab the next available skill for the character in O(1) for any Skill
    /// </summary>
    Dictionary<SkillType, Dictionary<bool, LinkedQueue<CharacterSkill>>> readySkills;

    // A skill is now ready and off cooldown
    // Add it to the cooresponding list of ready skills
    protected void SkillReady (CharacterSkill skill)
    {
        readySkills[skill.Type][skill.MoveUsable].Push(skill);
    }

    // This is the wait time for any cooldowns of skills
    // Passives will wait for this many seconds
    // Then, this will be substracted from the total wait time
    // Once the total wait time is <= 0, then the skill is ready to be casted again
    protected const float COROUTINE_CD_DELAY = 0.1f;

    // Cache of wait times
    // Wait for x seconds
    static protected readonly Dictionary<float, WaitForSeconds> waitForSecondsCache = new Dictionary<float, WaitForSeconds>();

    protected virtual void SetStats(int level, int stars) {}

    // Enemies that can be attacked
    // Note that for an enemy, this is the players pets
    protected LinkedList<Character> FightableEnemies { get; set; }

    // Bosses that can be attacked
    // That for an enemy, this is the players heroes
    // Bosses take default aggresion FIRST, if any exist
    protected LinkedList<Character> FightableBosses { get; set; }

    // Ally heroes
    protected LinkedList<Character> AllyHeroes { get; set; }
    // Ally pets
    protected LinkedList<Character> AllyPets { get; set; }

    // Character UI 
    // The Hp of the hero
    public Image HpBar { get; set; }

    // Is this hero a hero of the player??
    // This is used for a hidden mechanic that will display the hp bar slightly less than it really is
    // This visual effect is for gameplay enhancement
    protected bool IsPlayerCharacter { get; set; }
    protected bool IsEnemyBoss { get; set; } // Is this an enemy boss?

    // The transform of the manual skills a player can use
    // When this hero dies, we disable the transform so the player cannot use the heroes skills again
    public Transform ManSkills { protected get; set; }

    // ~~~~~~~~Protected Variables~~~~~~~~~~~~ \\

    // Is the character ready for battle?
    // This will return true once all stats have been applied and the character is ready to fight
    bool readyStatus = false;

    // The character is now ready to fight
    // The subclass defined when the character is ready
    // [Once they've initialized all of their skills and such]
    protected void ReadyToFight ()
    {
        readyStatus = true;
    }

    // Can this character attack?
    // If they are currently not attacking or using a skill, then they can
    // This is for their DEFAULT attack
    bool canAttack = false;

    // Is the character currently moving towards a target?
    bool isMoving = false;

    // Is this character currently attacking?
    bool isAttacking = false;

    // Are we currently using a skill?
    // Note that, this must be a skill that will PREVENT auto attacks
    // IE: An AUTO or MANUAL skill
    protected bool usingSkill = false;

    // Can the character use a skill?
    // IE: Is a skill already in use
    // If a manual skill is used during this time, then the current skill is interrupted
    protected bool canUseSkill = false;

    // Is the current character already dieing?
    bool isDieing = false;

    // The target. IE: Which character we are trying to attack
    protected Character Target { get; set; }

    protected bool CanFight ()
    {
        return readyStatus;
    }

    int curHP;
    protected int CurHP
    {
        get { return curHP; }
        set
        {   // Change the UI of the hero
            ChangeHPUIDisplay();
            this.curHP = value;
        }
    }

    protected CharacterStats HeroStats; // Stats of the hero that are USED

    // Bonus stats applied to this character
    // This can define bonus applifyers to stats (like hp), or the progress of the character such as level and stars
    CharacterBonuses BonusStats;

    const float DEFAULT_MVM_SPEED = 6.5f;

    float movementSpeed;
    public float MovementSpeed { 
        get { return this.movementSpeed; } 
        set
        {
            this.movementSpeed = value;
        }
    }

    // Offset of attack Range
    // This defines how far away a hero can be from his range before having to move towards a target again
    const byte RANGE_OFFSET = 3;

    // Aggro defines how well the hero is targetted by other heroes
    readonly int aggro = 10;

    // Definitions for temporary objects
    const int TEMP_OBJ_DURATION = 3;

    bool stoppedFighting = false; // Once we stop, we won't do anything

    bool isInBossFight = false;

    /// <summary>
    /// The hero is moving towards some position as an animation
    /// </summary>
    bool movingToPosition;
    // The position we're trying to reach
    Vector3 targetPosition;
    // An interrupt we call when we reach that position
    FightInterrupt methodOnPositionReach;

    /// <summary>
    /// Locations that the character would spawn in with respect to the spawn area for boss fights
    /// </summary>
    Vector2 bossSpawnLocation;
    const float MOVE_TO_LOCATION_SPEED_MULTIPLYER = 1.3f; // Additional rate at which a player moves to a location

    // Can this Character even move??
    // IE: Will they sit statically
    bool canMove;

    // Setting up for heroes
    public void Init (LinkedList<Character> enemies, LinkedList<Character> bosses, LinkedList<Character> allyHeroes, LinkedList<Character> allyPets,
        bool IsPlayerCharacter, CharacterBonuses bonusStats, Vector2 bossSpawnLocation, bool canMove)
    {
        this.FightableEnemies = enemies;
        this.FightableBosses = bosses;
        this.IsPlayerCharacter = IsPlayerCharacter; 
        this.BonusStats = bonusStats;
        this.bossSpawnLocation = bossSpawnLocation;
        this.canMove = canMove;
        this.AllyHeroes = allyHeroes;
        this.AllyPets = allyPets;
        this.MovementSpeed = DEFAULT_MVM_SPEED;
        SetStats(bonusStats.Level, bonusStats.Stars);

        // Have it so that we can attack if we cannot move
        // This way we don't have to first reach the target to determine if we can attack or not
        if (!canMove)
            canAttack = true;
    }

    // Initialize the character to have these cooresponding stats at all times
    // Note that, these are not temporary buffs. But instead the stats of the character throughout the fight.
    // This is called from the child class, which provides a set of stats based on the heroes level and stars
    // We then increase those stats by the bonusStats
    protected void InitCharacterStats (CharacterStats stats)
    {
        this.HeroStats = stats;
        this.readySkills = InitReadySkills();

        // Apply the additional bonus stats to the stats given
        PlayerCharacterStats.ApplyStatsAfterBonus(ref stats, this.BonusStats);
        this.HeroStats = stats;
        CurHP = stats.MaxHP; // Heroes will start at full hp

        // Modify the stats based on the level and star rating of the hero
        // We're ready to fight!
        ReadyToFight();
    }

    protected virtual Dictionary<SkillType, Dictionary<bool, LinkedQueue<CharacterSkill>>> InitReadySkills ()
    {
        return null;
    }

    private void Start()
    {
        this.anim = GetComponent<Animator>();
        this.attackAnimClip = AnimationHandler.GetAnimationClipByName(this.anim, this.GetDefaultAttackClipName ());

        if (this.attackAnimClip == null && !(this is Soap || this is Faphim)) // TODO remove this is Soap
            throw new AnimationFailure("Error, could not locate attack animation clip.");
    }

    // MAIN CALL
    // This is the main loop of the character
    private void Update()
    {
        if (movingToPosition)
        {
            if (targetPosition == Vector3.zero)
                throw new CharacterError("Error, no position thats being moved towards defined");

            // We're just trying to move to a specific location
            if (MoveTo(targetPosition) == true)
            {
                this.movingToPosition = false;
                this.targetPosition = Vector3.zero;

                if (this.methodOnPositionReach != null)
                    this.methodOnPositionReach(); // Call the interrupt
                
                this.methodOnPositionReach = null;
                anim.SetBool(isMovingAnim, false);
            }
        }
        else
        {
            // Check if hero is ready to fight
            // [IE: If the heroes stats are all applied]
            if (!readyStatus || stoppedFighting)
                return;

            // If we don't have a target and their are some we can target, then grab one
            if (this.Target == null && IsTargets())
            {
                this.Target = GetNextTarget();
                return;
            }
            else if (this.Target == null)
            {
                // There are no more targets available, and we no longer have one
                // In this case, we essentially won the fight
                // Debug.Log("We've won!");
                // StopEverything();
                return;
            }
            else
            {
                if (!isAttacking && this.canMove) // Don't move while doing default attack
                    MoveToTarget();

                DefaultAttackState();
                LookAtTarget();
            }

            ActivateAutoSkills();
            ActivatePassives();
        }

        // Moving the HP bar of the hero to face the player
        if (!(this is Boss)) // Bosses have separate HP bars that are static
            this.HpBar.transform.LookAt(
                    transform.position + Camera.main.transform.rotation * Vector3.back,
                    Camera.main.transform.rotation * Vector3.up
             );
    }

    void ActivatePassives ()
    {
        if (readySkills != null)
        {
            if (readySkills.TryGetValue(SkillType.PASSIVE, out Dictionary<bool, LinkedQueue<CharacterSkill>> skillDict))
            {
                // If we are moving, only activate passives that can trigger when we're moving
                // Else, activate all passives
                if (skillDict.TryGetValue(true, out LinkedQueue<CharacterSkill> skillQueue))
                    ActivateAllSkills(skillQueue);
                if (!isMoving)
                {
                    if (skillDict.TryGetValue(false, out skillQueue))
                        ActivateAllSkills(skillQueue);
                }
            }
        }
    }

    void ActivateAutoSkills ()
    {
        if (canUseSkill == true && readySkills != null)
        {
            if (readySkills.TryGetValue(SkillType.AUTO, out Dictionary<bool, LinkedQueue<CharacterSkill>> skillDict))
            {
                // Activate the AUTO skill that's applied
                // If 2 skills are ready, and we're not running
                // then by default the skill that cannot be used while running will be chosen
                if (!isMoving && skillDict.TryGetValue(false, out LinkedQueue<CharacterSkill> skillQueue) && skillQueue.Size > 0)
                {
                    canUseSkill = false;
                    usingSkill = true;
                    StopAttackingCheck();
                    canAttack = false;
                    skillQueue.Pop().RunSkill();
                }
                else if (skillDict.TryGetValue(true, out skillQueue) && skillQueue.Size > 0)
                {
                    canUseSkill = false;
                    usingSkill = true;
                    StopAttackingCheck();
                    canAttack = false;
                    skillQueue.Pop().RunSkill();
                }
            }
        }
    }

    // Stop using the default attack
    protected void StopAttackingCheck ()
    {
        if (isAttacking)
            StopCoroutine(DefaultAttack(this.Target));
        isAttacking = false;
        canAttack = true;
    }

    // This function will activate all skills in the linkedQueue
    void ActivateAllSkills (LinkedQueue<CharacterSkill> skills)
    {
        if (skills != null && skills.Size > 0)
        {
            while (skills.Size > 0)
            {
                skills.Pop().RunSkill();
            }
        }
    }

    // Move the hero towards the target
    // Will go as close as the range of the hero will allow them to
    protected void MoveToTarget()
    {
        // check if target fell out of range
        if (Vector3.Distance(this.transform.position, this.Target.transform.position) > HeroStats.AttackRange + RANGE_OFFSET + ExtraBossAttackRange)
        {
            canAttack = false;
            isMoving = true;
            transform.position = Vector3.MoveTowards(transform.position, Target.transform.position, Time.deltaTime * movementSpeed);

            // Start our walking animation
            anim.SetBool(isMovingAnim, true);
            anim.SetTrigger(isWalkingStateAnim);
        }
        else if (isMoving && Vector3.Distance(this.transform.position, this.Target.transform.position) > HeroStats.AttackRange + ExtraBossAttackRange)
        {
            transform.position = Vector3.MoveTowards(transform.position, Target.transform.position, Time.deltaTime * movementSpeed);
        }
        else
        {
            // Don't attack if we're casting something
            canAttack = true;

            if (isMoving)
            {
                isMoving = !isMoving;
                anim.SetBool(isMovingAnim, false);
            }
        }
    }

    void LookAtTarget()
    {
        Quaternion rotation = Quaternion.LookRotation(Target.transform.position - this.transform.position);
        transform.rotation = Quaternion.Slerp(this.transform.rotation, rotation, Time.deltaTime * TURN_SPEED);
    }

    // This hero has killed somebody
    protected void KilledAHero(){}

    protected void DefaultAttackState ()
    {
        if (canAttack && !isAttacking && !isMoving && !usingSkill)
        {
            StartCoroutine(DefaultAttack(this.Target));
        }
    }

    // Armor function
    // Apply our equation to determine how much damage is dealt
    // 100x / (x + 45) is our function for armor
    // Where x is our armor. This is the damage being reduced. 
    // Divide by 100 to get the percentage. [Adding this by our dmg reduction]
    // If we have no armor, we skip this and return the full damage
    int GetDmgAfterArmor (int dmg)
    {
        if (HeroStats.Armor == 0)
            return dmg;
        else if (HeroStats.Armor > 0)
            // Get result
            return dmg - ((int)(dmg * (((100f * HeroStats.Armor) / (HeroStats.Armor + 85f)) / 100f)));
        else if (HeroStats.Armor < 0)
            // Get result. We will treat armor as positive then flip the result [otherwise our equation will bring it positive]
            return dmg - ((int)(dmg * -(((100f * Mathf.Abs(HeroStats.Armor)) / (Mathf.Abs(HeroStats.Armor) + 85f)) / 100f)));

        return dmg;
    }

    // Same as armor function, except with elemental damage reduction
    int GetDmgAfterElementalRed (int dmg)
    {
        if (HeroStats.ElementalResistance == 0)
            return dmg;
        else if (HeroStats.ElementalResistance > 0)
            // Get result
            return dmg - ((int)(dmg * (((100f * HeroStats.ElementalResistance) / (HeroStats.ElementalResistance + 85f)) / 100f)));
        else if (HeroStats.ElementalResistance < 0)
            // Get result. We will treat armor as positive then flip the result [otherwise our equation will bring it positive]
            return dmg - ((int)(dmg * -(((100f * Mathf.Abs(HeroStats.ElementalResistance)) / (Mathf.Abs(HeroStats.ElementalResistance) + 85f)) / 100f)));

        return dmg;
    }

    // Damage the hero by some amount of damage
    // Returns true if the hero dies by taking that amount of damage
    protected bool TakeDamage(Damage dmg)
    {
       // Debug.Log(dmg.Dmg + " Damage is attempted to be dealt to me");
        // Cannot take negative damage [it does nothing]
        // If the attack missed, it will also do nothing
        if (dmg.Dmg <= 0 || dmg.HasMissed)
            return false;

        // Next, we test if we avoided this damage [From evasion]
        // Note that, any amount of damage can be avoided
        if (Random.Range(Mathf.Epsilon, 100f) <= HeroStats.Evasion)
        {
            Debug.Log("We evaded the attack!!!");
        }
        else
        {
            // Apply our damage reduction
            int dmgNum = dmg.Dmg - ((int)(dmg.Dmg * (HeroStats.DmgRed / 100f)));

            // Apply armor reduction
            if (dmg.Type == DamageTypes.NORMAL)
            {
                dmgNum = GetDmgAfterArmor(dmgNum);
            }
            else if (dmg.Type == DamageTypes.FIRE)
            {
                // Fire damage specific reduction is a flat rate
                dmgNum -= ((int)(dmgNum * (GetFireReduction() / 100f)));

                // Apply elemental reduction [Note that elemental reduction would be stacked ontop of the fireDamage rate
                dmgNum = GetDmgAfterElementalRed(dmgNum);
            }
            else if (dmg.Type == DamageTypes.POISON)
            {
                // Poison damage specific reduction is a flat rate
                dmgNum -= (int)(dmgNum * (GetPoisonReduction() / 100f));

                // Apply elemental reduction [Note that elemental reduction would be stacked ontop of the fireDamage rate
                dmgNum = GetDmgAfterElementalRed(dmgNum);
            }
            // Nothing happens for PURE damage
            else if (dmg.Type == DamageTypes.PURE) { }

            //Debug.Log("The resulting damage: " + dmgNum);
            if (dmgNum > 0)
            {
                // Apply the damage to us
                this.CurHP -= dmgNum;

                //Debug.Log("We;re taking  " + dmgNum + " And we have " + HeroStats.Armor + " Armor");

                // Check if the hero is dead
                return IsDead();
            }
        }

        return false;
    }

    // Heal the target character by the given amount of hp
    // This amount of healing is Amplified by our healing AMP
    protected void HealTarget (Heal heal, Character target)
    {
        target.Heal((int) (heal.HealAmount + (heal.HealAmount * (HeroStats.HealingAMP / 100f))));
    }

    // Heal ourselves by this amount
    // Amplified by our healing received
    protected void Heal (int healAmount)
    {
        healAmount += (int)(healAmount * (HeroStats.HealingRCVD / 100f));
        Debug.Log("Healed by " + healAmount);

        // Do not heal by a negative amount, unless specified otherwise
        if (healAmount <= 0)
            return;

        this.CurHP += healAmount;
    }

    // Changes the HP display so that the hp of the hero cooresponds with their current hp
    protected void ChangeHPUIDisplay ()
    {
        if (this.HpBar != null)
        {
            if (CurHP <= 0)
            {
                this.HpBar.fillAmount = 0;
                return;
            }

            // For non-player heroes, we display it normally
            if (!IsPlayerCharacter)
            {
                this.HpBar.fillAmount = (float)CurHP / HeroStats.MaxHP;
            }
            else
            {
                // Otherwise, we follow the formula to make it look slightly lesser than it actually is
                this.HpBar.fillAmount = Mathf.Sin(((float)CurHP / HeroStats.MaxHP) / 250f) * 240f;
            }
        }
    }

    // Our hero has died
    // Remove the gameobject from the list and destroy it
    // Apply death animation [if any]
    protected void Die ()
    {
        if (!isDieing)
        {
            isDieing = true;
            FightSetup.Died(this);

            // If this character had any skills usable by the player, we will now disable them
            if (ManSkills != null)
                ManSkills.gameObject.SetActive(false);

            GameObject.Destroy(this.gameObject);
        }
    }

    // Check if our hero is dead
    // If we are, then kill the hero
    protected bool IsDead ()
    {
        bool ret = this.CurHP <= 0;

        if (ret)
        {
            anim.SetBool(isDeadAnim, true);
            anim.SetTrigger(isDeadStateAnim);
            Die();
        }

        return ret;
    }

    /// <summary>
    /// This is a list storing each effect of the character
    /// The list is separated in a dictionary by each effect type
    /// If the list is empty, then the character is not receiving any over time damage or healing of that type
    /// </summary>
    readonly Dictionary<EffectTypes, LinkedList<OTEffect>> effectsOccuring = new Dictionary<EffectTypes, LinkedList<OTEffect>>()
    {
        { EffectTypes.NORMAL, new LinkedList<OTEffect>()},
        { EffectTypes.BURNING, new LinkedList<OTEffect>()},
        { EffectTypes.POISONED, new LinkedList<OTEffect>()}
    };

    const float OT_EFFECT_INTERVALS_IN_SECONDS = 1f; // This is the amount of time elapsed before an effect re-occurs
                                                    // For example, suppose a healing effect was applied to the hero for 5s
                                                    // They would be healed every interval until 5s has elapsed
    bool isRunningEffects = false;

    // Apply an over time damage or heal effect to the target hero
    // An effect has a defined EffectType and applies this effect over the course of its duration
    protected void ApplyOTDmgOrHealEffect (OTEffect effect, Character target)
    {
        // This does nothing
        if (effect.Duration <= 0)
            return;

        // Add the effect
        if (target.effectsOccuring.TryGetValue (effect.TypeOfEffect, out LinkedList<OTEffect> val))
        {
            val.AddLast(effect);
        }
        else
        {
            // List not yet created, it should be created already
            throw new ElementNotDefined("Error, list not defined for effect dictionary. But used.");
        }

        // Run the coroutine if we aren't already
        if (!target.isRunningEffects)
        {
            target.StartCoroutine(RunAllDmgHealOTEffects ());
        }
    }

    // Apply an effect on an attribute over time
    // The amount changed will be modified directly
    // If the effect is not a BUFF [Not a positive effect], then it can be reduced by the heroes TENACITY / STATUS EFFECT reduction stat
    // This is a PERCENTAGE buff, apply a percent increase / decrease
    protected void ApplyAttributeOverTimeEFFectToTarget (Attributes attribute, float percChange, bool isBuff, float duration, Character target)
    {
        target.StartCoroutine(AttributeOverTime(attribute, percChange, isBuff, duration));
    }

    // Gain or decrease an attribute over time
    IEnumerator AttributeOverTime (Attributes attribute, float percChange, bool isBuff, float duration)
    {
        if (HeroStats.ApplyIncreaseByName(attribute, percChange))
        {
            if (isBuff)
                Debug.Log("Applied buff");

            // Check cache to see if we've already waited for that amount of time
            if (waitForSecondsCache.TryGetValue(duration, out WaitForSeconds wait))
            {
                yield return wait;
            }
            else
            {
                WaitForSeconds time = new WaitForSeconds(duration);
                waitForSecondsCache.Add(duration, time);
                yield return time;
            }

            Debug.Log("Buff has ended");

            // Reduce the amount to go back to normal
            if (!HeroStats.ApplyIncreaseByName(attribute, -percChange))
            {
                // This SHOULD be impossible to reach. Something must be modifying the attribute elsewhere if this error occurs from here.
                throw new CharacterError("Error, attribute not defined.");
            }
        }
        else
        {
            throw new CharacterError("Error, attribute not defined.");
        }
    }

    // Apply an effect on an attribute over time
    // The amount changed will be modified directly
    // If the effect is not a BUFF [Not a positive effect], then it can be reduced by the heroes TENACITY / STATUS EFFECT reduction stat
    // THIS IS A BASE BUFF [DIRECTLY apply this to the attribute]
    protected void ApplyAttributeOverTimeEFFectToTarget(Attributes attribute, int changeAmount, bool isBuff, float duration, Character target)
    {
        target.StartCoroutine(AttributeOverTime(attribute, changeAmount, isBuff, duration));
    }

    // Gain or decrease an attribute over time
    IEnumerator AttributeOverTime(Attributes attribute, int percChange, bool isBuff, float duration)
    {
        if (HeroStats.ApplyIncreaseByName(attribute, percChange))
        {
            if (isBuff)
                Debug.Log("Applied buff");

            // Check cache to see if we've already waited for that amount of time
            if (waitForSecondsCache.TryGetValue(duration, out WaitForSeconds wait))
            {
                yield return wait;
            }
            else
            {
                WaitForSeconds time = new WaitForSeconds(duration);
                waitForSecondsCache.Add(duration, time);
                yield return time;
            }

            Debug.Log("Buff has ended");

            // Reduce the amount to go back to normal
            if (!HeroStats.ApplyIncreaseByName(attribute, -percChange))
            {
                // This SHOULD be impossible to reach. Something must be modifying the attribute elsewhere if this error occurs from here.
                throw new CharacterError("Error, attribute not defined.");
            }
        }
        else
        {
            throw new CharacterError("Error, attribute not defined.");
        }
    }


    // Runs all the Heal and Damage over time effects that are applied to this character
    // The courotine will run for the longest current time remaining effect, check if any have ended
    // And stop applying anything to any ended effects
    // IMPORTANT: The purpose of this being separate from any other effects (THIS IS JUST HEALING AND DAMAGE)
    // Is because the attribute status effect reductions (tenacity) DOES NOT apply to damage or healing over time
    // It applies to ALL other types of debuffs [Such as accuracy over x seconds, stuns, etc..]
    IEnumerator RunAllDmgHealOTEffects ()
    {
        isRunningEffects = true;

        // Keep on applying those effects until all effects are finished
        while (effectsOccuring[EffectTypes.NORMAL].Count > 0 ||
               effectsOccuring[EffectTypes.BURNING].Count > 0 ||
               effectsOccuring[EffectTypes.POISONED].Count > 0
            )
        {
            // Check cache to see if we've already waited for that amount of time
            if (waitForSecondsCache.TryGetValue(OT_EFFECT_INTERVALS_IN_SECONDS, out WaitForSeconds wait))
            {
                yield return wait;
            }
            else
            {
                WaitForSeconds time = new WaitForSeconds(OT_EFFECT_INTERVALS_IN_SECONDS);
                waitForSecondsCache.Add(OT_EFFECT_INTERVALS_IN_SECONDS, time);
                yield return time;
            }

            // Apply each effect for every type that exists [If any exists]
            if (effectsOccuring.TryGetValue(EffectTypes.NORMAL, out LinkedList<OTEffect> lists))
            {
                RunOTEffects(lists, OT_EFFECT_INTERVALS_IN_SECONDS);
            }
            else if (effectsOccuring.TryGetValue(EffectTypes.BURNING, out lists))
            {
                RunOTEffects(lists, OT_EFFECT_INTERVALS_IN_SECONDS);
            }
            else if (effectsOccuring.TryGetValue(EffectTypes.POISONED, out lists))
            {
                RunOTEffects(lists, OT_EFFECT_INTERVALS_IN_SECONDS);
            }

            StatusCheck();
        }

        isRunningEffects = false;
    }

    // Run the OT Effects for the following duration [IE: Reduce their duration] 
    void RunOTEffects (LinkedList<OTEffect> listOfEffects, float durationChange)
    {
        // Apply all currently active effects
        LinkedListNode<OTEffect> cur = listOfEffects.First;
        while (cur != null)
        {
            OTEffect val = cur.Value;

            // Execute the effect
            // Copies of our OTEffect are used so that any randomization is re-calculated
            if (val.HasHeal)
            {
                this.Heal(val.Heal.Copy (val.Heal).HealAmount);
            }
            if (val.HasDmg)
            {
                this.TakeDamage(val.Dmg.Copy (val.Dmg));
            }

            val.Duration -= durationChange; // Subtract duration by the time we've currently ran
            if (val.Duration <= 0)
            {
                // Remove it from the list of active effects
                listOfEffects.Remove(cur);
            }

            cur = cur.Next;
        }
    }

    // Show the User the currently active effects on the hero
    void StatusCheck ()
    {
        /*if (effectsOccuring[EffectTypes.BURNING].Count > 0)
            Debug.Log("We're currently burning!!!!!");
        else if (effectsOccuring[EffectTypes.POISONED].Count > 0)
            Debug.Log("We're currently poisoned!!!!");
        else if (effectsOccuring[EffectTypes.NORMAL].Count > 0)
            Debug.Log("We;re taking some damage over time!!!");
            */
    }

    // Attack the default with our default attack
    // The default attack is to do our attack animation
    // Wait for the amount of time of our attack speed
    // Then, deal damage to our target based on our dmg stat
    IEnumerator DefaultAttack (Character target)
    {
        canAttack = false;
        isAttacking = true;
        if (!(this is Soap || this is Faphim)) // TODO remove this line once pets animations are in
            anim.SetFloat(baseAttackDurationAnim, this.HeroStats.AttackSpeed / attackAnimClip.length);
        anim.SetBool(isAttackingAnim, true);
        anim.SetTrigger(isAttackingStateAnim);

        // Attack animation takes attackSpeed time to complete
        if (waitForSecondsCache.TryGetValue(HeroStats.AttackSpeed, out WaitForSeconds cacheCheck))
        {
            // Use cache value
            yield return cacheCheck;
        }
        else
        {
            // Add to the cache
            cacheCheck = new WaitForSeconds(HeroStats.AttackSpeed);
            waitForSecondsCache.Add(HeroStats.AttackSpeed, cacheCheck);
            yield return cacheCheck;
        }

        // Hit the target with our attack
        HurtHero(target, new Damage (HeroStats.Dmg, this.GetHeroNormalDamageType (), HeroStats.CritRate, HeroStats.CritDmg, HeroStats.Accuracy));

        // Finished the attack
        canAttack = true;
        isAttacking = false;
        anim.SetBool(isAttackingAnim, false);
    }

    // Hit the target for the specified amount of damage
    // FIXME should I apply the crit dmg here instead of in the 'dmg' stat??
    public void HurtHero (Character target, Damage dmgDealt)
    {
        // Deal the damage, and test if the attack killed the hero
        if (target != null && target.TakeDamage(dmgDealt) == true)
            KilledAHero();
    }

    // Methods that define the behaviour of a Character
    // Get the next target defined by this target
    // By default we will target the boss with the highest aggro or if there is no boss, the enemy with the highest aggro
    protected virtual Character GetNextTarget ()
    {
        Character nextTarget;

        // Check if we'll be targetting a boss
        if (FightableBosses != null && FightableBosses.Count > 0)
        {
            // Target boss
            nextTarget = GetCharWithLargestAggro(FightableBosses);
        }
        else
        {
            // Target enemy
            nextTarget = GetCharWithLargestAggro(FightableEnemies);
        }

        return nextTarget;
    }

    // Returns the character with the largest aggro in the list
    Character GetCharWithLargestAggro (LinkedList<Character> list)
    {
        if (list == null)
            throw new FightConfigError("Error occured. Largest aggro list undefined.");

        LinkedListNode<Character> curChar = list.First;
        Character highestAggro = null;

        if (curChar != null)
        {
            highestAggro = curChar.Value;
            while (curChar.Next != null)
            {
                if (highestAggro == null || curChar.Next.Value.aggro > highestAggro.aggro)
                    highestAggro = curChar.Next.Value;

                curChar = curChar.Next;
            }
        }

        return highestAggro;
    }

    // Are there even any targets that we can fight??
    bool IsTargets ()
    {
        return (FightableEnemies != null || FightableBosses != null) && (FightableEnemies.Count > 0 || FightableBosses.Count > 0);
    }


    /// <summary>
    /// Move this character to the cooresponding position
    /// Calls the interrupt function once they have all completed the movement
    /// The hero cannot attack during this
    /// preparing for boss fight will determine if the hero is currently going to fight a boss
    /// This will later be used to allow the player to move heroes manually
    /// </summary>
    /// <param name="playerChars"></param>
    /// <param name="positions"></param>
    public void MoveToPos(Vector3 position, FightInterrupt interrupt, bool preparingForBossFight)
    {
        this.canAttack = false;
        this.canUseSkill = false;
        this.movingToPosition = true;
        this.targetPosition = position;
        this.methodOnPositionReach = interrupt;
        this.isInBossFight = preparingForBossFight;
    }

    /// <summary>
    /// Moves this character towards the cooresponding position
    /// Returns true if the character has reached this position
    /// A character has reached the position if they are within 0.5 units from them (Basically there)
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    bool MoveTo (Vector3 position)
    {
        anim.SetBool(isMovingAnim, true);
        this.transform.LookAt(position);
        //this.transform.position = Vector3.Lerp(this.transform.position, position, this.movementSpeed * Time.deltaTime * MOVE_TO_LOCATION_SPEED_MULTIPLYER);
        this.transform.position = Vector3.MoveTowards(this.transform.position, position, Time.deltaTime * this.movementSpeed * MOVE_TO_LOCATION_SPEED_MULTIPLYER);

        if (Vector2.Distance(this.transform.position, position) <= 0.09)
            return true;

        return false;
    }

    // Check to see if the character that died is our target
    // If it is, STOP our current attack and move on to the next target
    public void DeadTargetCheck (Character deadCharacter)
    {
        if (deadCharacter == this.Target)
        {
            // Stop any attacks that may exist
            StopCoroutine(DefaultAttack(this.Target));
            this.Target = null;
        }
    }

    // Returns the cooldown of a skill, based on that skills CD reduction
    // In other words, this applied the characters cd reduction attributes to return the 'actual' cool down
    // Note that, cooldowns cannot be negative, and any negative cooldown is 0 (CD >= 100% = 0)
    // Negative cool down reduction on the other hand, implies that the skills cool down is increased.
    protected float GetSkillCoolDown (int skill)
    {
        float cd = HeroStats.SkillCooldowns[skill] - (HeroStats.SkillCooldowns[skill] * (HeroStats.CdReduction / 100f));

        return cd < 0 ? 0 : cd;
    }

    // Does this character have any skills that the player can use?
    // IE: Anything that requires the players interactions
    public bool HasManualSkills ()
    {
        return HaveManualSkills();
    }

    // Overrided member
    // This will be overrided if the hero has skills for that hero
    protected virtual bool HaveManualSkills ()
    {
        return false;
    }

    public Dictionary<ManualSkills, string> GetManualSkillNames ()
    {
        return ManualSkillNames();
    }

    // By default, a character has no skills that the player can use
    protected virtual Dictionary<ManualSkills, string> ManualSkillNames()
    {
        return null;
    }

    // Get the manual character skills
    public CharacterSkill GetPrimarySkill ()
    {
        return PrimarySkill();
    }
    public CharacterSkill GetSecondarySkill() 
    {
        return SecondarySkill();
    }
    public CharacterSkill GetUltimateSkill()
    {
        return UltimateSkill();
    }

    protected virtual CharacterSkill PrimarySkill (){ return null; }
    protected virtual CharacterSkill SecondarySkill() { return null; }
    protected virtual CharacterSkill UltimateSkill() { return null; }

    // Returns the heroes normal attack (default attack) damage type
    protected virtual DamageTypes GetHeroNormalDamageType () { return DamageTypes.NORMAL; }

    // Reduction stats that are applied to specific damage types
    // These stats are CHARACACTER specific, and are not an actual attribute
    protected virtual int GetFireReduction() { return 0; }
    protected virtual int GetPoisonReduction() { return 0; }

    public virtual string GetCharacterName () { return "default"; }

    // FIXME see if there's a better way around this
    // This function is override to get the name of the animation clip for the default attack of the hero
    // Not the best solution, since it's different for every hero
    // I only use this to get the length of the animation so i can change the speed of it based on the heroes attack speed
    protected virtual string GetDefaultAttackClipName() { return "default attack"; }

    /// Stops all animations and any on going event
    void StopEverything ()
    {
        this.anim.SetBool(isMovingAnim, false);
        this.anim.SetBool(isDeadAnim, false);
        this.anim.SetBool(isAttackingAnim, false);
        StopAllCoroutines();
        stoppedFighting = true;
    }

    /// <summary>
    /// Creates and returns a temporary object that can be used on this character
    /// The object will then be destroyed after some amount of time
    /// A time can optionally be specified if wanted
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    protected GameObject CreateTempObject (GameObject obj, int destroyAfterTime = TEMP_OBJ_DURATION)
    {
        GameObject tempObj = Instantiate(obj);
        StartCoroutine(DestroyAfterSeconds(tempObj, destroyAfterTime));

        return tempObj;
    }

    IEnumerator DestroyAfterSeconds (GameObject obj, int duration)
    {

        // Check cache to see if we've already waited for that amount of time
        if (waitForSecondsCache.TryGetValue(duration, out WaitForSeconds wait))
        {
            yield return wait;
        }
        else
        {
            WaitForSeconds time = new WaitForSeconds(duration);
            waitForSecondsCache.Add(duration, time);
            yield return time;
        }

        Destroy(obj);
    }

    /// <summary>
    /// Get the location where the position should first spawn in with respect to boss fights
    /// </summary>
    /// <returns></returns>
    public Vector2 GetBossSpawnLocation ()
    {
        return this.bossSpawnLocation;
    }
}