using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

/// <summary>
/// This script manages the movement of all of the characters
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    float radius;
    static Vector2 centerPosition;
    static bool canMoveCharacters = false;
    static Dictionary<Character, float> charThetas = new Dictionary<Character, float>();

    void Update()
    {
        if (canMoveCharacters)
        {
            CheckForInput ();
        }
    }

    void CheckForInput ()
    {
#if UNITY_STANDALONE
    if (Input.GetMouseButton(0))
        {
            MoveHeroesRight();
        }
#endif
    }

    void MoveHeroesRight ()
    {
        foreach (KeyValuePair<Character, float> i in charThetas)
        {
            float radius = Vector2.Distance(
                    new Vector2(i.Key.transform.position.x, i.Key.transform.position.z), centerPosition
                );
           Debug.Log (i.Key.transform.position + " name: " + i.Key.name + " distance:  "+ radius);
            i.Key.transform.position =
                new Vector3
                (
                    centerPosition.x + radius * (float)Math.Sin (i.Value + i.Key.MovementSpeed),
                    i.Key.transform.position.y,
                    centerPosition.y + radius * (float)Math.Cos(i.Value + i.Key.MovementSpeed)
                );
        }
    }

    void MoveHeroesLeft ()
    {

    }

    public static void EnabledPlayerMovement (Vector2 centerCircle, params LinkedList<Character>[] characters)
    {
        canMoveCharacters = true;
        Debug.Log(centerCircle);
        Debug.Log(centerCircle);
        // Find the theta for each character
        // This is the current characters position in the circle
        foreach (LinkedList<Character> k in characters)
        {
            foreach (Character i in k)
            {
                Vector3 charPos = i.transform.position;
                float theta = Mathf.Asin ((charPos.x - centerCircle.x) / Vector2.Distance(centerCircle, new Vector2(charPos.x, charPos.z)));


                charThetas.Add(i, theta);
            }
        }
    }

    public static void DisabledPlayerMovement ()
    {
        canMoveCharacters = false;
    }
}
