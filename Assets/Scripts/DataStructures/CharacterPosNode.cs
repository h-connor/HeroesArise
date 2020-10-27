// A node for storing a reference to a character with its cooresponding position
// Position defines where it the character is located when spawned
public struct CharacterPosition
{
    public CharacterBonuses CharacterStats { get; set; }
    public UnityEngine.Vector2 CharPosition { get; set; }

    public CharacterPosition (CharacterBonuses stats, UnityEngine.Vector2 pos)
    {
        this.CharacterStats = stats;
        this.CharPosition = pos;
    }
}