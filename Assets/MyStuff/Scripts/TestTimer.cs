using UnityEngine;
using TMPro;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class TestTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TestManager testManager;

    private float testStartTime;
    private float testEndTime;
    private bool isTimerRunning = false;
    private float elapsedTime = 0f;

    // network test grouped data points
    private float[] testCompletionTimes;
    private int[] testErrorCounts;
    private int currentTestIndex = -1;

    // color use data points
    private Dictionary<int, int>[] colorUsageCounts; // array of data collections
    private List<ColorChange>[] colorChangeSequences; // timerr for color changes

    // HCI grouped data points
    private Dictionary<string, float>[] connectionTimings; // time to complete a connection
    private List<ConnectionEvent>[] connectionEvents; // type of connection
    private List<ErrorEvent>[] errorEvents; // errors
    private Dictionary<string, float>[] panelFocus; // time spend gazing at panels + the name of panel
    private string firstSelectedDevice = null;
    private float firstSelectionTime = 0f;

    public class TestData
    {
        public float[] TestCompletionTimes;
        public int[] TestErrorCounts;
        public Dictionary<int, int>[] ColorUsageCounts;
        public List<ColorChange>[] ColorChangeSequences;
        public Dictionary<string, float>[] ConnectionTimings;
        public Dictionary<string, float>[] ColorSelectionTimings;
        public List<ConnectionEvent>[] ConnectionEvents;
        public List<ErrorEvent>[] ErrorEvents;
        public Dictionary<string, float>[] PanelFocus;
    }

    public class ColorChange
    {
        public int FromColorIndex;
        public int ToColorIndex;
        public float TimeStamp;
    }

    public class ConnectionEvent
    {
        public string IconDevice1;
        public string IconDevice2;
        public int ColorIndex;
        public float SelectionStartTime;
        public float CompletionTime;
        public float TimeSinceLastColorSelection;
    }

    public class ErrorEvent
    {
        public string ErrorType; // wrong connection and wrong color
        public string IconDevice1; // first connected device
        public string IconDevice2; // second conneced device
        public int UsedColorIndex;
        public int RequiredColorIndex;
        public float TimeStamp;
    }

    private void Start()
    {
        // create data storage
        int testCount = testManager.GetTestCount();
        testCompletionTimes = new float[testCount];
        testErrorCounts = new int[testCount];

        // reset timer
        UpdateTimerDisplay(0);

        // start all tracking sets
        colorUsageCounts = new Dictionary<int, int>[testCount];
        colorChangeSequences = new List<ColorChange>[testCount];
        connectionTimings = new Dictionary<string, float>[testCount];
        connectionEvents = new List<ConnectionEvent>[testCount];
        errorEvents = new List<ErrorEvent>[testCount];
        panelFocus = new Dictionary<string, float>[testCount];

        for (int i = 0; i < testCount; i++)
        {
            colorUsageCounts[i] = new Dictionary<int, int>();
            colorChangeSequences[i] = new List<ColorChange>();
            connectionTimings[i] = new Dictionary<string, float>();
            connectionEvents[i] = new List<ConnectionEvent>();
            errorEvents[i] = new List<ErrorEvent>();
            panelFocus[i] = new Dictionary<string, float>()
            {
                { "Instructions", 0f }, { "Network", 0f }, { "ColorPalette", 0f }
            };
        }
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            elapsedTime = Time.time - testStartTime;
            UpdateTimerDisplay(elapsedTime);
        }
    }

    public void StartTest(int testIndex)
    {
        currentTestIndex = testIndex;
        bool isTutorial = (testIndex == 0);

        if (!isTutorial)
        {
            testStartTime = Time.time;
            isTimerRunning = true;
            elapsedTime = 0f;
            UpdateTimerDisplay(elapsedTime);
        }
        else
        {
            isTimerRunning = false;
            elapsedTime = 0f;
            UpdateTimerDisplay(elapsedTime);
        }
    }

    public void StopTest()
    {
        bool isTutorial = (currentTestIndex == 0);

        if (!isTimerRunning && !isTutorial) return;

        if (!isTutorial)
        {
            testEndTime = Time.time;
            isTimerRunning = false;

            // current test complete time
            float completionTime = testEndTime - testStartTime;

            // skip tutorial
            int dataIndex = currentTestIndex - 1;

            if (dataIndex >= 0 && dataIndex < testCompletionTimes.Length)
            {
                testCompletionTimes[dataIndex] = completionTime;
            }
        }
        else
        {
            isTimerRunning = false;
        }
    }

    public void RecordError(string errorType, string device1, string device2,
                                   int usedColorIndex, int requiredColorIndex)
    {
        if (currentTestIndex <= 0 || !isTimerRunning)
            return;

        int dataIndex = currentTestIndex - 1;

        if (dataIndex >= 0 && dataIndex < testErrorCounts.Length)
        {
            testErrorCounts[dataIndex]++;
        }

        ErrorEvent error = new ErrorEvent
        {
            ErrorType = errorType,
            IconDevice1 = device1,
            IconDevice2 = device2,
            UsedColorIndex = usedColorIndex,
            RequiredColorIndex = requiredColorIndex,
            TimeStamp = Time.time - testStartTime,
        };

        errorEvents[dataIndex].Add(error);
    }

    private void UpdateTimerDisplay(float timeInSeconds)
    {
        if (timerText != null)
        {
            timerText.text = FormatTime(timeInSeconds);
        }
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }


    public void RecordConnectionFirstDevice(string deviceName)
    {
        if (currentTestIndex <= 0 || !isTimerRunning) return;

        if (firstSelectedDevice == null)
        {
            firstSelectedDevice = deviceName;
            firstSelectionTime = Time.time - testStartTime;
        }
    }

    public void RecordConnectionSecondDevice(string device1, string device2, int colorIndex)
    {
        if (currentTestIndex <= 0 || !isTimerRunning) return;
        int dataIndex = currentTestIndex - 1;
        if (firstSelectedDevice == null) return;

        ConnectionEvent connectionEvent = new ConnectionEvent
        {
            IconDevice1 = firstSelectedDevice,
            IconDevice2 = device2,
            ColorIndex = colorIndex,
            SelectionStartTime = firstSelectionTime,
            CompletionTime = Time.time - testStartTime,
            TimeSinceLastColorSelection = 0f //calulated below
        };

        if (colorChangeSequences[dataIndex].Count > 0)
        {
            ColorChange lastColorChange = colorChangeSequences[dataIndex].Last();
            connectionEvent.TimeSinceLastColorSelection = connectionEvent.CompletionTime - lastColorChange.TimeStamp;
        }

        connectionEvents[dataIndex].Add(connectionEvent);

        string connectionKey = $"{device1}-{device2}";
        connectionTimings[dataIndex][connectionKey] = connectionEvent.CompletionTime - connectionEvent.SelectionStartTime;
        firstSelectedDevice = null;
    }

    // track gaze time for panels
    public void RecordGaze(string area, float duration)
    {
        if (currentTestIndex <= 0 || !isTimerRunning) return;

        int dataIndex = currentTestIndex - 1;

        if (panelFocus[dataIndex].ContainsKey(area))
        {
            panelFocus[dataIndex][area] += duration;
        }
    }

    public void RecordColorNameForCSV(int colorIndex)
    {
        if (currentTestIndex <= 0 || !isTimerRunning)
            return;

        int dataIndex = currentTestIndex - 1;

        // color button usage count
        if (!colorUsageCounts[dataIndex].ContainsKey(colorIndex))
        {
            colorUsageCounts[dataIndex][colorIndex] = 1;
        }
        else
        {
            colorUsageCounts[dataIndex][colorIndex]++;
        }

        // color button changes + only for previous
        if (colorChangeSequences[dataIndex].Count > 0)
        {
            int lastColorIndex = colorChangeSequences[dataIndex].Last().ToColorIndex;

            if (lastColorIndex != colorIndex)
            {
                ColorChange change = new ColorChange
                {
                    FromColorIndex = lastColorIndex,
                    ToColorIndex = colorIndex,
                    TimeStamp = Time.time - testStartTime
                };

                colorChangeSequences[dataIndex].Add(change);
            }
        }
        else
        {
            ColorChange change = new ColorChange
            {
                FromColorIndex = -1, // -1 means no previous color for csv output
                ToColorIndex = colorIndex,
                TimeStamp = Time.time - testStartTime
            };

            colorChangeSequences[dataIndex].Add(change);
        }
    }

    public TestData GetAllTestData()
    {
        return new TestData
        {
            TestCompletionTimes = this.testCompletionTimes,
            TestErrorCounts = this.testErrorCounts,
            ColorUsageCounts = this.colorUsageCounts,
            ColorChangeSequences = this.colorChangeSequences,
            ConnectionTimings = this.connectionTimings,
            ConnectionEvents = this.connectionEvents,
            ErrorEvents = this.errorEvents,
            PanelFocus = this.panelFocus
        };
    }
}