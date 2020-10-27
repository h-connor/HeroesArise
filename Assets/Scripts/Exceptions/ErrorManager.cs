using UnityEngine;
using UnityEngine.UI;

// THIS CLASS REQUIRED A SINGLE CHILD TO BE THE ERRORDIALOG 
public class ErrorManager : MonoBehaviour
{
    static ErrorManager thisSingleton;

    [SerializeField]
    GameObject errorDialogPrompt = null;

    [SerializeField]
    Text errorText = null;

    private void Awake()
    {
        if (thisSingleton == null)
        {
            thisSingleton = this;
        } 

        if (errorDialogPrompt == null || errorText == null)
        {
            throw new ElementNotDefined("Error, text or prompt not properly initialized.");
        }
    }

    // An error has occurred, we will now display the message to the user through a dialog prompt
    // [Enable the prompt]
    public static void OpenMenu (string msg)
    {
        if (thisSingleton == null)
        {
            Debug.Log("Error window has failed to open upon receiving an error.");
        }
        else
        {
            thisSingleton.errorText.text = msg;
            thisSingleton.errorDialogPrompt.SetActive(true);
        }
    }

    // Close the error dialog
    public void CloseDialog ()
    {
        if (thisSingleton == null)
        {
            Debug.Log("Error window has failed to close upon exitting from an error message.");
        }
        else
        {
            thisSingleton.errorDialogPrompt.SetActive(false);
            thisSingleton.errorText.text = string.Empty;
        }
    }
}
