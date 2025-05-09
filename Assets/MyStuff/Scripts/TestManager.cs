using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class TestManager : MonoBehaviour
{
    [SerializeField] private TestUI testUI;
    [SerializeField] private NetworkingTask[] tasks;
    [SerializeField] private SimpleSelectionManager selectionManager;
    [SerializeField] private GameObject nextTaskButton;
    [SerializeField] private IconLayoutManager iconLayoutManager;
    [SerializeField] private Sprite checkmarkSprite;


    private int currentTaskIndex = -1;
    private List<GameObject> checkMarks = new List<GameObject>();
    private List<ConnectionPair> correctConnections = new List<ConnectionPair>();
    private Dictionary<string, int> pendingConnections = new Dictionary<string, int>();
    private Dictionary<string, int> completedConnections = new Dictionary<string, int>();

    private struct ConnectionPair
    {
        public string Device1Name;
        public string Device2Name;

        public ConnectionPair(string device1, string device2)
        {
            if (string.Compare(device1, device2) < 0)
            {
                Device1Name = device1;
                Device2Name = device2;
            }
            else
            {
                Device1Name = device2;
                Device2Name = device1;
            }
        }

        // check if connection passes reuquirements
        public bool Contains(string device1,  string device2)
        {
            return (Device1Name == device1 && Device2Name == device2) ||
                   (Device1Name == device2 && Device2Name == device1);
        }
    }


    private void Start()
    {
        nextTaskButton.GetComponent<XRSimpleInteractable>().selectEntered.AddListener((args) => NextTask());
        iconLayoutManager.ApplyLayout(0);
        NextTask();
    }


    public void NextTask()
    {
        resetTest();

        currentTaskIndex++;
        if (currentTaskIndex >= tasks.Length)
        {
            // test over when test 3 complete
            CompleteTest();
            return;
        }

        setNextTest();
        setTestConnections();
        startTimer();
    }


    public void CheckTestProgress(GameObject source,  GameObject target, int colorIndex = -1)
    {
        if (currentTaskIndex < 0 || currentTaskIndex >= tasks.Length)
            return;

        NetworkingTask task = tasks[currentTaskIndex];

        checkTestConnections(task, source, target, colorIndex);

        if (task.IsCompleted())
        {
            testUI.ShowNextTaskButton(true);
        }
    }


    private void resetTest()
    {
        correctConnections.Clear();
        ClearCheckMarks();
        pendingConnections.Clear();
        completedConnections.Clear();

        TestTimer testTimer = FindObjectOfType<TestTimer>();
        if (currentTaskIndex >= 0)
        {
            testTimer.StopTest();
        }
    }


    private void startTimer()
    {
        TestTimer testTimer = FindObjectOfType<TestTimer>();
        testTimer.StartTest(currentTaskIndex);
    }


    private void setNextTest()
    {
        // set device icon layout for tests
        iconLayoutManager.ApplyLayout(currentTaskIndex);

        //  reset connections
        selectionManager.ClearAllConnections();

        // set next test
        NetworkingTask task = tasks[currentTaskIndex];
        testUI.SetInstructions(task.instructions);
        testUI.SetHint(task.hint);
        testUI.SetProgress(currentTaskIndex + 1, tasks.Length);
        testUI.ShowNextTaskButton(false);
    }


    private void setTestConnections()
    {
        NetworkingTask task = tasks[currentTaskIndex];
        // required connection amount
        foreach (NetworkingTask.RequiredConnection connection in task.requiredConnections)
        {
            // reset connections
            connection.completed = false;
            connection.correctColor = false;

            // source device count
            if (!pendingConnections.ContainsKey(connection.sourceDeviceName))
            {
                pendingConnections[connection.sourceDeviceName] = 1;
                completedConnections[connection.sourceDeviceName] = 0;
            }
            else
            {
                pendingConnections[connection.sourceDeviceName]++;
            }

            // target device count
            if (!pendingConnections.ContainsKey(connection.targetDeviceName))
            {
                pendingConnections[connection.targetDeviceName] = 1;
                completedConnections[connection.targetDeviceName] = 0;
            }
            else
            {
                pendingConnections[connection.targetDeviceName]++;
            }
        }
    }


     private void checkTestConnections(NetworkingTask task, GameObject source, GameObject target, int colorIndex )
    {
        foreach (NetworkingTask.RequiredConnection connection in task.requiredConnections)
        {
            if ((connection.sourceDeviceName == source.name && connection.targetDeviceName == target.name) ||
                (connection.sourceDeviceName == target.name && connection.targetDeviceName == source.name))
            {
                connection.completed = true;

                // gets color requirement
                if (connection.requiredColorIndex < 0 || connection.requiredColorIndex == colorIndex)
                {
                    addConnection(connection, source, target);
                }
                else
                {
                    connection.correctColor = false;
                    TestTimer testTimer = FindObjectOfType<TestTimer>();
                    if (testTimer != null)
                    {
                        testTimer.RecordError(source.name, target.name,
                                                     colorIndex, connection.requiredColorIndex);
                    }
                }
            }
        }
    }


    private void CompleteTest()
    {
        TestTimer testTimer = FindObjectOfType<TestTimer>();
        testTimer.StopTest();
        TestDataOutput dataOutput = new TestDataOutput(testTimer);
        dataOutput.SaveDataToFile();

        testUI.SetInstructions("All tests completed! Thank you for participating.");
        testUI.SetHint("");
        testUI.SetProgress(tasks.Length, tasks.Length);
    }


    private void ShowCheckmark(GameObject device)
    {
        GameObject checkMark = new  GameObject($"{device.name}Checkmark");
        SpriteRenderer renderer = checkMark.AddComponent<SpriteRenderer>();

        renderer.sprite =   checkmarkSprite;
        checkMark.transform.position = device.transform.position + new Vector3(-0.09f, 0.22f, -0.05f); // checkmark slightly above icon position
        checkMark.transform.forward = Camera.main.transform.forward;
        checkMark.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        checkMarks.Add(checkMark);
    }


    private void ClearCheckMarks()
    {
        foreach (GameObject check in checkMarks )
        {
            if (check != null)
                Destroy(check);
        }
        checkMarks.Clear();
    }


    private void updateConnections(GameObject device)
    {
        if (completedConnections.ContainsKey(device.name))
        {
            completedConnections[device.name]++;

            // check all connections for icons with multiple required connections
            if (completedConnections[device.name] >= pendingConnections[device.name])
            {
                ShowCheckmark(device);
            }
        }
    }


    private void addConnection(NetworkingTask.RequiredConnection connection, GameObject source,  GameObject target)
    {
        bool wasAlreadyCorrect = connection.correctColor;
        connection.correctColor = true;

        if (!wasAlreadyCorrect)
        {
            correctConnections.Add(new ConnectionPair(source.name, target.name));
            updateConnections(source);
            updateConnections(target);
        }
    }


    public int GetTestCount()
    {
        if (tasks == null )
            return 0;
        return tasks.Length - 1;
    }


    public bool connectionCompleted(string device1Name, string device2Name)
    {
        foreach (ConnectionPair pair in correctConnections)
        {
            if (pair.Contains(device1Name, device2Name))
                return true;
        }
        return false;
    }

}
