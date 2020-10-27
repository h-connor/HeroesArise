using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// This class will determine if a skill is pressed by the player
/// </summary>

[RequireComponent(typeof(UnityEngine.UI.Image))]
public class SkillPress : MonoBehaviour, IPointerClickHandler
{
    public CharacterSkill SkillToRun { get; set; }

    // When the skill is pressed
    // Note in order for the skill to work the IMAGE [Skill image that is pressed] must be enabled
    public void OnPointerClick (PointerEventData data)
    {
        if (SkillToRun != null && this.GetComponent<UnityEngine.UI.Image>().IsActive())
            ManualSkillsManager.CurrentlyActiveSkill = SkillToRun;
        else
        {
            throw new ElementNotDefined("Error, Image or skill not defined.");
        }
    }
}
