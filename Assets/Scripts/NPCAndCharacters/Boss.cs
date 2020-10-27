using UnityEngine;

public class Boss : Character
{
    // ~~~~~~ Overriden methods ~~~~~~~ \\
    // These behaviours are overriden by the subclasses
    // If the subclass does not define a behaviour, then the default base class will be used

    protected override Character GetNextTarget()
    {
        return base.GetNextTarget();
    }

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
        while (totalWait <= GetSkillCoolDown(skill.SkillName))
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
    }

    // Reduction stats that are applied to specific damage types
    // These stats are CHARACACTER specific, and are not an actual attribute
    protected override int GetFireReduction() { return base.GetFireReduction(); }
    protected override int GetPoisonReduction() { return base.GetPoisonReduction(); }

    protected override string GetDefaultAttackClipName() { return base.GetDefaultAttackClipName(); }
}
