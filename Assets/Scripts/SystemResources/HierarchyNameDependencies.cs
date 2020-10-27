/// <summary>
/// As much as I'd like to avoid naming dependencies as much as possible
/// Some areas seemingly require it.
/// This class is specifically for, if there are any objects defined at compile time (In hierarchy list)
/// That we need to access at runtime, from their name.
/// If the element cannot be found, a NoObjectExists exception should be thrown [From the calling class]
/// This class simply returns what the name SHOULD be for that element
/// </summary>
public class HierarchyDependencies
{
    // This defines the set of UI elements that show the player a characters skills that can be used
    const string playerSelectableSkills = "Char{x} Skills";

    // returns name for the selectable skills of that hero
    // The parameter is the index of the hero
    // Heroes UI skills have the name Charx Skills, where x is the index of the hero in the skills list
    public static string GetSelectableSkillsName (int heroIndex)
    {
        return playerSelectableSkills.Replace("{x}", heroIndex.ToString());
    }
}
