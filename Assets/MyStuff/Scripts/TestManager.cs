using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

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
            // Store names in alphabetical order for consistency in comparisons
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

        // cehck if connection passes reuquirements
        public bool Contains(string device1, string device2)
        {
            return (Device1Name == device1 && Device2Name == device2) ||
                   (Device1Name == device2 && Device2Name == device1);
        }
    }

    private void Start()
    {
        if (nextTaskButton != null)
        {
            XRSimpleInteractable buttonInteractable = nextTaskButton.GetComponent<XRSimpleInteractable>();
            if (buttonInteractable != null)
            {
                buttonInteractable.selectEntered.AddListener(OnNextButtonSelected);
            }
        }

        if (iconLayoutManager != null)
        {
            iconLayoutManager.ApplyLayout(0); //forces layout for tutorial, maybe not needed?
        }
        NextTask();

    }

    private void OnNextButtonSelected(SelectEnterEventArgs args)
    {
        NextTask();
    }

    public void NextTask()
    {
        // reset test
        correctConnections.Clear();
        ClearCheckMarks();

        // reset test data
        pendingConnections.Clear();
        completedConnections.Clear();

        // reset timer
        TestTimer testTimer = FindObjectOfType<TestTimer>();
        if (testTimer != null && currentTaskIndex >= 0)
        {
            testTimer.StopTest();
        }

        // panel gaze data output
        GazeTracker gazeTracker = FindObjectOfType<GazeTracker>();
        if (gazeTracker != null)
        {
            gazeTracker.RecordFinalGazeDuration();
        }

        currentTaskIndex++;

        if (currentTaskIndex >= tasks.Length)
        {
            CompleteTest();
            return;
        }

        iconLayoutManager.ApplyLayout(currentTaskIndex);

        // reset connections
        if (selectionManager != null)
        {
            selectionManager.ClearAllConnections();
        }

        // set next task/test
        NetworkingTask task = tasks[currentTaskIndex];
        testUI.SetInstructions(task.instructions);
        testUI.SetHint(task.hint);
        testUI.SetProgress(currentTaskIndex + 1, tasks.Length);
        testUI.ShowNextTaskButton(false);

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

        // start timer
        if (testTimer != null)
        {
            testTimer.StartTest(currentTaskIndex);
        }
    }

    public void CheckTaskProgress(GameObject source, GameObject target, int colorIndex = -1)
    {
        if (currentTaskIndex < 0 || currentTaskIndex >= tasks.Length)
            return;

        NetworkingTask task = tasks[currentTaskIndex];

        // checks if connection is for this task 
        foreach (NetworkingTask.RequiredConnection connection in task.requiredConnections)
        {
            if ((connection.sourceDeviceName == source.name && connection.targetDeviceName == target.name) ||
                (connection.sourceDeviceName == target.name && connection.targetDeviceName == source.name))
            {
                connection.completed = true;

                // gets color requirement
                if (connection.requiredColorIndex < 0 || connection.requiredColorIndex == colorIndex)
                {
                    bool wasAlreadyCorrect = connection.correctColor;
                    connection.correctColor = true;

                    // add connection to correct
                    if (!wasAlreadyCorrect)
                    {
                        correctConnections.Add(new ConnectionPair(source.name, target.name));

                        if (completedConnections.ContainsKey(source.name))
                        {
                            completedConnections[source.name]++;

                            // checks requirements
                            if (completedConnections[source.name] >= pendingConnections[source.name])
                            {
                                ShowDeviceCompletionFeedback(source);
                            }
                        }

                        if (completedConnections.ContainsKey(target.name))
                        {
                            completedConnections[target.name]++;

                            // check all connections for multi connection iconss
                            if (completedConnections[target.name] >= pendingConnections[target.name])
                            {
                                ShowDeviceCompletionFeedback(target);
                            }
                        }
                    }
                }
                else
                {
                    connection.correctColor = false;

                    TestTimer testTimer = FindObjectOfType<TestTimer>();
                    if (testTimer != null)
                    {
                        testTimer.RecordDetailedError("WrongColor", source.name, target.name,
                                                     colorIndex, connection.requiredColorIndex);
                    }
                }
            }
        }

        if (task.IsCompleted())
        {
            testUI.ShowNextTaskButton(true);
        }
    }

    private void CompleteTest()
    {
        TestTimer testTimer = FindObjectOfType<TestTimer>();
        if (testTimer != null)
        {
            testTimer.StopTest();
            testTimer.SaveDataToFile();
        }

        testUI.SetInstructions("All tests completed! Thank you for participating.");
        testUI.SetHint("");
        testUI.SetProgress(tasks.Length, tasks.Length);
    }

    // this ended being checkmark
    private void ShowDeviceCompletionFeedback(GameObject device)
    {
        GameObject checkMark = new GameObject($"{device.name}Checkmark");
        SpriteRenderer renderer = checkMark.AddComponent<SpriteRenderer>();

        renderer.sprite = checkmarkSprite;
        checkMark.transform.position = device.transform.position + new Vector3(-0.09f, 0.22f, -0.05f);
        checkMark.transform.forward = Camera.main.transform.forward;
        checkMark.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        checkMarks.Add(checkMark);
        StartCoroutine(PulseScale(checkMark.transform)); //!!!!
    }

    private IEnumerator PulseScale(Transform obj)
    {
        Vector3 originalScale = obj.localScale;

        float duration = 0.3f;
        float timer = 0;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            obj.localScale = Vector3.Lerp(originalScale, originalScale * 1.5f, t);
            yield return null;
        }

        timer = 0;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;
            obj.localScale = Vector3.Lerp(originalScale * 1.5f, originalScale, t);
            yield return null;
        }
    }

    private void ClearCheckMarks()
    {
        foreach (GameObject check in checkMarks)
        {
            if (check != null)
                Destroy(check);
        }
        checkMarks.Clear();
    }

    public int GetTestCount()
    {
        if (tasks == null)
            return 0;
        return tasks.Length - 1;
    }

    public bool IsConnectionCorrect(string device1Name, string device2Name)
    {
        foreach (ConnectionPair pair in correctConnections)
        {
            if (pair.Contains(device1Name, device2Name))
                return true;
        }
        return false;
    }

    public void RestartTutorial()
    {
        currentTaskIndex = -1;
        NextTask();
    }
}