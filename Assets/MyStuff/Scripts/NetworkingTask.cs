using UnityEngine;

[System.Serializable]
public class NetworkingTask
{
    [TextArea(3, 10)]
    public string instructions;

    [TextArea(2, 5)]
    public string hint;

    [System.Serializable]
    public class RequiredConnection
    {
        public string sourceDeviceName;
        public string targetDeviceName;
        public int requiredColorIndex = -1; 
        public bool completed;
        public bool correctColor;
    }

    public RequiredConnection[] requiredConnections;

    public bool IsCompleted()
    {
        foreach (RequiredConnection connection in requiredConnections)
        {
            if (!connection.completed || !connection.correctColor)
                return false;
        }
        return true;
    }
}