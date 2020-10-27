using System;

// Exception base class
// All errors come here, and start the dialog for to display the error message
public class Error : Exception
{
    public Error(String msg) : base(msg)
    {
        ErrorManager.OpenMenu(msg);
    }
}
