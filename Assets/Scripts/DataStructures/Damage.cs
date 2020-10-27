using UnityEngine;

/// <summary>
/// This struct defines Damage
/// Damage can have a type, whether it be element or normal or pure
/// The type of damage can effect how much it deals, and the effect it has
/// ALL damage taken uses this struct
/// </summary>

/// <summary>
/// Different types of damage
/// Normal: Damages the target, reduced by armor
/// Fire: Burns the target, typically applies a damage over a time period, reduced by elemental reduction
/// Poison: Poisons the target, typically applies a damage over a time period, reduced by elemental reduction
/// Pure: Ignores armor, and any elemental reduction
/// </summary>
public enum DamageTypes { NORMAL, FIRE, POISON, PURE }

// Damage struct
public struct Damage
{
    public int Dmg { get; private set; }
    public DamageTypes Type { get; private set; }

    // Did this damage that was dealt miss?
    public bool HasMissed { get; private set; }

    // Did this damage that was dealt crit?
    public bool HasCrit { get; private set; }

    /// <summary>
    /// Creating a damage without any type
    /// In this case, by default we will just use normal damage type
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="critRate"></param>
    /// <param name="critDmg"></param>
    /// <param name="accuracy"></param>
    public Damage (int damage, float critRate, float critDmg, float accuracy) : this (damage, DamageTypes.NORMAL, critRate, critDmg, accuracy) {}

    // These are used if a copy of the struct is wanted
    // Encase a copy of our heal is used, the randomized accuracy and crit stats will be re-calculated
    int dmgBeforeModifiers;
    float critRate, critDmg, accuracy;

    // Damage dealt must define a hitChance (Accuracy), critRate, and a critDmg stat
    // If the attack misses, then the damage is set to 0
    // Crit rate and Accuracy MUST be in percent format
    public Damage (int damage, DamageTypes type, float critRate, float critDmg, float accuracy)
    {
        this.dmgBeforeModifiers = damage;
        this.critRate = critRate;
        this.critDmg = critDmg;
        this.accuracy = accuracy;

        // Check if our attack will hit
        if (Random.Range(Mathf.Epsilon, 100f) <= accuracy)
        {
            this.Dmg = damage;
            this.HasMissed = false;

            // The attack hit!
            // Lets check if it will crit
            if (Random.Range(Mathf.Epsilon, 100f) <= critRate)
            {
                //Debug.Log("It's a crit!");
                // We crit!
                this.HasCrit = true;

                // Multiply our dmg by the increased amount from the critical dmg stat
                this.Dmg += (int) (this.Dmg * (critDmg / 100f));
            }
            else
            {
                this.HasCrit = false;
            }
        }
        else
        {
            //Debug.Log("Our attack will miss");
            // Attack missed
            this.Dmg = 0;
            this.HasMissed = true;
            this.HasCrit = false;
        }

        this.Type = type;
    }

    // Return a copy of some heal
    // NOTE that this will likely have a different critical hit or accuracy effect
    public Damage Copy(Damage dmgToCopy)
    {
        return new Damage(dmgToCopy.dmgBeforeModifiers, dmgToCopy.critRate, dmgToCopy.critDmg, dmgToCopy.accuracy);
    }
}