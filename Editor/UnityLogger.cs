using UnityEngine;
using UnitySpec.General.Build;

public class UnityLogger : ITaskLoggingWrapper
{
    private bool hasErrors = false;
    public bool HasLoggedErrors()
    {
        return hasErrors;
    }

    public void LogError(string message)
    {
        hasErrors = true;
        Debug.LogError(message);
    }

    public void LogMessage(string message)
    {
        Debug.Log(message);
    }

    public void LogMessageWithLowImportance(string message)
    {
        LogMessage(message);
    }
}