using UnityEngine;

/// <summary>
/// This class manages any manual skills that are being used by the player
/// A skill can only be used once
/// Activating a different skill will reset the currently active skill
/// </summary>
public class ManualSkillsManager : MonoBehaviour
{
    static CharacterSkill currentlyActiveSkill;
    static bool skillPressed;

    // For raycasting from mouse/input position
    static Ray ray;
    static RaycastHit hit;

    static bool dragReady; // Ready to drag the skill
    static bool mouseEndSet; // Ending location of a skill being dragged
    static bool mouseStartSet; // Starting location of a skill being dragged

    public static CharacterSkill CurrentlyActiveSkill 
    {
        private get { return currentlyActiveSkill; } 
        set 
        {
            skillPressed = true;
            currentlyActiveSkill = value;
            dragReady = false;
            mouseStartSet = false;
            mouseEndSet = false;

            // Active an auto ability right away
            if (currentlyActiveSkill.ManSkillType == ManualSkillTypes.AUTO)
            {
                RunTheSkill();
            }
        }
    }

    // Here we need to keep track of the users mouse/finger pressing
    // If the skill involves a target, the user can select it on a target
    // If the skill involves dragging, the user can drag their finger to execute the skill along the defined path
    private void Update()
    {
        if (skillPressed && currentlyActiveSkill != null)
        {
            if (currentlyActiveSkill.ManSkillType == ManualSkillTypes.SELECT)
            {
                ManageSelect();
            }
            else if (currentlyActiveSkill.ManSkillType == ManualSkillTypes.DRAG)
            {
                ManageDrag();
            }
        }
    }

    void ManageDrag ()
    {
#if UNITY_STANDALONE
        if (dragReady && Input.GetMouseButtonDown(0))
        {
            if (!mouseStartSet)
            {
                currentlyActiveSkill.MouseStartPos = Input.mousePosition;
                mouseStartSet = true;
            }
        }
        else if (dragReady && mouseStartSet && Input.GetMouseButtonUp(0))
        {
            if (!mouseEndSet)
            {
                currentlyActiveSkill.MouseEndPos = Input.mousePosition;
                mouseEndSet = true;
            }
        }

        if (mouseStartSet && mouseEndSet)
            RunTheSkill();

        if (!dragReady && Input.GetMouseButtonUp(0))
            dragReady = true;
#endif
    }

    void ManageSelect ()
    {
        // Determine if mouse was pressed on an object
#if UNITY_STANDALONE
        if (!Input.GetMouseButton(0))
            return;

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
#endif
#if UNITY_ANDROID || UNITY_IOS && !UNITY_STANDALONE

        // TODO: First check if / when Input has been pressed before getting the touch position [OR ELSE A EXCEPTION WILL THROW]
        ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
#endif

        if (Physics.Raycast(ray, out hit))
        {
            // Targetting an ally
            if (currentlyActiveSkill.CanSelectAlly && hit.transform.tag.Equals(Character.TARGET_TAG_ALLY))
            {
                currentlyActiveSkill.Target = hit.transform.GetComponent<Character>();
                if (currentlyActiveSkill.Target == null)
                {
                    throw new ComponentNotFound("Error, no Character found.");
                }
                else
                {
                    RunTheSkill();
                }
            }
            // Targetting an enemy
            else if (currentlyActiveSkill.CanSelectEnemy && hit.transform.tag.Equals(Character.TARGET_TAG_ENEM))
            {
                currentlyActiveSkill.Target = hit.transform.GetComponent<Character>();
                if (currentlyActiveSkill.Target == null)
                {
                    throw new ComponentNotFound("Error, no Character found.");
                }
                else
                {
                    RunTheSkill();
                }
            }
        }
    }

    static void RunTheSkill ()
    {
        skillPressed = false;
        dragReady = false;
        mouseStartSet = false;
        mouseEndSet = false;
        currentlyActiveSkill.RunSkill();
        currentlyActiveSkill = null;
    }
}
