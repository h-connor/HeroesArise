using UnityEngine;

public class Hero : Character
{    
    
    // Can this pet use these skills?
    protected bool canUsePrimary = true;
    protected bool canUseSecondary = true;
    protected bool canUseUltimate = true;

    // Are the skills currently on cool down?
    // Note, this is used to prevent multiple coroutine instances if the user presses multiple times
    protected bool primOnCD = false;
    protected bool secOnCD = false;
    protected bool ultOnCD = false;

    protected override void SetStats(int level, int stars) { base.SetStats(level, stars); }
    public override string GetCharacterName() { return base.GetCharacterName(); }
    protected override System.Collections.Generic.Dictionary<SkillType, System.Collections.Generic.Dictionary<bool, LinkedQueue<CharacterSkill>>> InitReadySkills()
    {
        return base.InitReadySkills();
    }

    protected System.Collections.IEnumerator WaitForSkillCD(CharacterSkill skill)
    {
        float totalWait = 0;
        //Debug.Log("Waiting " + GetSkillCoolDown(skill.SkillName));
        while (totalWait <= GetSkillCoolDown (skill.SkillName))
        {
            if (waitForSecondsCache.TryGetValue(COROUTINE_CD_DELAY, out WaitForSeconds cacheCheck))
            {
                // Use cache value
                yield return cacheCheck;
            }
            else
            {
                // Add to the cache
                cacheCheck = new WaitForSeconds(COROUTINE_CD_DELAY);
                waitForSecondsCache.Add(COROUTINE_CD_DELAY, cacheCheck);
                yield return cacheCheck;
            }
            totalWait += COROUTINE_CD_DELAY;
        }

        // The skill has finished coming off cooldown and can now be triggered again
        if (skill.Type != SkillType.MANUAL)
            SkillReady(skill);
        else
        {
            //Debug.Log("Skill ready");
            CanUseManualSkill(skill.ManSkill);
        }
    }

    protected void CanUseManualSkill(ManualSkills type)
    {
        switch (type)
        {
            case ManualSkills.PRIMARY:
                PrimarySkillReady();
                break;
            case ManualSkills.SECONDARY:
                SecondarySkillReady();
                break;
            case ManualSkills.ULTIMATE:
                UltimateSkillReady();
                break;
        }
    }

    protected void PrimarySkillReady()
    {
        canUsePrimary = true;
        primOnCD = false;
    }

    protected void SecondarySkillReady()
    {
        canUseSecondary = true;
        secOnCD = false;
    }

    protected void UltimateSkillReady()
    {
        canUseUltimate = true;
        ultOnCD = false;
    }

    // ~~~~~~ Overriden methods ~~~~~~~ \\
    // These behaviours are overriden by the subclasses
    // If the subclass does not define a behaviour, then the default base class will be used

    protected override Character GetNextTarget()
    {
        return base.GetNextTarget ();
    }

    // This will be overrided if the hero has skills that can be manually used by the player
    protected override bool HaveManualSkills()
    {
        return base.HaveManualSkills ();
    }

    // Returns the names of the skills associated with each manual skill for the hero
    protected override System.Collections.Generic.Dictionary<ManualSkills, string> ManualSkillNames()
    {
        return base.ManualSkillNames();
    }

    // Running the manual skills defined for the hero
    protected override CharacterSkill PrimarySkill() { return base.PrimarySkill (); }
    protected override CharacterSkill SecondarySkill() { return base.SecondarySkill(); }
    protected override CharacterSkill UltimateSkill() { return base.UltimateSkill(); }

    // Reduction stats that are applied to specific damage types
    // These stats are CHARACACTER specific, and are not an actual attribute
    protected override int GetFireReduction() { return base.GetFireReduction(); }
    protected override int GetPoisonReduction() { return base.GetPoisonReduction(); }

    protected override string GetDefaultAttackClipName() { return base.GetDefaultAttackClipName(); }
}
