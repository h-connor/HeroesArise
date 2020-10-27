/// This defines OTEffects that are either DAMAGE OR HEAL BASED
/// In other words, This does not include attribute effects
public enum EffectTypes { NORMAL, BURNING, POISONED }
public class OTEffect
{
    public EffectTypes TypeOfEffect { get; private set; }
    public Damage Dmg { get; private set; }
    public Heal Heal { get; private set; }
    public bool HasHeal { get; private set; }
    public bool HasDmg { get; private set; }

    // The duration of this effect
    public float Duration { get; set; }

    public OTEffect (EffectTypes type, float duration, Heal healAmt)
    {
        this.TypeOfEffect = type;
        this.Heal = healAmt;
        this.Duration = duration;
        this.HasHeal = true;
    }

    public OTEffect(EffectTypes type, float duration, Damage dmg) 
    {
        this.TypeOfEffect = type;
        this.Dmg = dmg;
        this.Duration = duration;
        this.HasDmg = true;
    }

    public OTEffect(EffectTypes type, float duration, Damage dmg, Heal healAmount)
    {
        this.TypeOfEffect = type;
        this.Dmg = dmg;
        this.Duration = duration;
        this.Heal = healAmount;
        this.HasDmg = true;
        this.HasHeal = true;
    }

    // Returns a copy of a OTEffect
    // Note that, copies of damage and heal structs will re-calculate any randomized effects (Such as critical hits or misses)
    public OTEffect Copy (OTEffect effectCopy)
    {
        return new OTEffect(effectCopy.TypeOfEffect, effectCopy.Duration, effectCopy.Dmg.Copy(effectCopy.Dmg), effectCopy.Heal.Copy(effectCopy.Heal));
    }
}