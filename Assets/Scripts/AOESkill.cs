using UnityEngine;

public class AOESkill : MonoBehaviour
{
    /// <summary>
    /// The caster of the skill [Preventing self collisions]
    /// </summary>
    public Character Caster { private get; set; }

    /// <summary>
    /// Can this AOE skill hit the caster?
    /// </summary>
    public bool CanHitCaster { private get; set; }
    public AOEHeroSkill SkillToRun { get; set; }

    /// <summary>
    /// The number of objects this skill has hit
    /// </summary>
    public int HitCount { get; private set; }

    public void Init (Character caster, bool canHitCaster, AOEHeroSkill skill)
    {
        this.Caster = caster;
        this.CanHitCaster = canHitCaster;
        this.SkillToRun = skill;
    }

    private void OnTriggerEnter(Collider collision)
    {
        Character characterHit = collision.gameObject.GetComponent<Character>();

        if (characterHit != null)
        {
            // Check to see if we hit ourselves (if we are not allowed to)
            if (!CanHitCaster && characterHit == Caster)
                return;

            HitCount++;
            SkillToRun(characterHit, this);
        }
    }
}
