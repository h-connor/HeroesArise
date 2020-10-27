using UnityEngine;

/// <summary>
/// Defining a Heal
/// All heroes HEAL by using this struct
/// This struct defines the heal amount, and any amplifications that would be done before the heal is applied
/// </summary>
public struct Heal
{
    public int HealAmount { get; private set; }

    // Did this heal miss?
    public bool HasMissed { get; private set; }

    // Did this heal crit?
    public bool HasCrit { get; private set; }

    // These are used if a copy of the struct is wanted
    // Encase a copy of our heal is used, the randomized accuracy and crit stats will be re-calculated
    int healBeforeModifiers;
    float critRate, critDmg, accuracy;

    // Damage dealt must define a hitChance (Accuracy), critRate, and a critDmg stat
    // If the attack misses, then the damage is set to 0
    // Crit rate and Accuracy MUST be in percent format
    public Heal(int healAmount, float critRate, float critDmg, float accuracy)
    {
        this.healBeforeModifiers = healAmount;
        this.critRate = critRate;
        this.critDmg = critDmg;
        this.accuracy = accuracy;

        // Check if our attack will hit
        if (Random.Range(Mathf.Epsilon, 100f) <= accuracy)
        {
            this.HealAmount = healAmount;
            this.HasMissed = false;

            // The attack hit!
            // Lets check if it will crit
            if (Random.Range(Mathf.Epsilon, 100f) <= critRate)
            {
                Debug.Log("It's a crit [HEAL]!");
                // We crit!
                this.HasCrit = true;

                // Multiply our dmg by the increased amount from the critical dmg stat
                this.HealAmount += (int)(this.HealAmount * (critDmg / 100f));
            }
            else
            {
                this.HasCrit = false;
            }
        }
        else
        {
            Debug.Log("Our Heal will miss");
            // Attack missed
            this.HealAmount = 0;
            this.HasMissed = true;
            this.HasCrit = false;
        }
    }

    // Return a copy of some heal
    // NOTE that this will likely have a different critical hit or accuracy effect
    public Heal Copy (Heal healToCopy)
    {
        return new Heal(healToCopy.healBeforeModifiers, healToCopy.critRate, healToCopy.critDmg, healToCopy.accuracy);
    }
}
