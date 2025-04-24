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
    private Dictionary<string, float>[] colorSelectionTimings; // time between picking colors + not using icons
    private List<ConnectionEvent>[] connectionEvents; // type of connection
    private List<ErrorEvent>[] errorEvents; // errors
    private Dictionary<string, float>[] attentionDistribution; // time spend gazing at panels + the name of panel
    private string firstSelectedDevice = null;
    private float firstSelectionTime = 0f;
    private Dictionary<string, float> deviceHoverStartTimes = new Dictionary<string, float>(); // time controller is spent pointing at icons,

    private class ColorChange
    {
        public int FromColorIndex;
        public int ToColorIndex;
        public float TimeStamp;
    }

    private class ConnectionEvent
    {
        public string Device1;
        public string Device2;
        public int ColorIndex;
        public float SelectionStartTime;
        public float CompletionTime;
        public string ConnectionType; // WAN + LAN +VLAN and wireless
        public float TimeSinceLastColorSelection;
    }

    private class ErrorEvent
    {
        public string ErrorType; // wrong connection and wrong color
        public string Device1; // left and right controller for XR interation setup
        public string Device2;
        public int UsedColorIndex;
        public int RequiredColorIndex;
        public float TimeStamp;
        public float RecoveryTime;
    }

    private void Start()
    {
        // createa data storage
        int testCount = testManager.GetTestCount();
        testCompletionTimes = new float[testCount];
        testErrorCounts = new int[testCount];

        // reset timer
        UpdateTimerDisplay(0);

        // start all tracking sets
        colorUsageCounts = new Dictionary<int, int>[testCount];
        colorChangeSequences = new List<ColorChange>[testCount];
        connectionTimings = new Dictionary<string, float>[testCount];
        colorSelectionTimings = new Dictionary<string, float>[testCount];
        connectionEvents = new List<ConnectionEvent>[testCount];
        errorEvents = new List<ErrorEvent>[testCount];
        attentionDistribution = new Dictionary<string, float>[testCount];

        for (int i = 0; i < testCount; i++)
        {
            colorUsageCounts[i] = new Dictionary<int, int>();
            colorChangeSequences[i] = new List<ColorChange>();
            connectionTimings[i] = new Dictionary<string, float>();
            colorSelectionTimings[i] = new Dictionary<string, float>();
            connectionEvents[i] = new List<ConnectionEvent>();
            errorEvents[i] = new List<ErrorEvent>();
            attentionDistribution[i] = new Dictionary<string, float>()
            {
                { "Instructions", 0f },
                { "Network", 0f },
                { "ColorPalette", 0f }
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
            // doesnt count for tutorial
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

    public void RecordError()
    {
        if (currentTestIndex >= 0 && currentTestIndex < testErrorCounts.Length)
        {
            testErrorCounts[currentTestIndex]++;
        }
    }

    public void RecordDetailedError(string errorType, string device1, string device2,
                                   int usedColorIndex, int requiredColorIndex)
    {
        if (currentTestIndex <= 0 || !isTimerRunning)
            return;

        int dataIndex = currentTestIndex - 1;

        ErrorEvent error = new ErrorEvent
        {
            ErrorType = errorType,
            Device1 = device1,
            Device2 = device2,
            UsedColorIndex = usedColorIndex,
            RequiredColorIndex = requiredColorIndex,
            TimeStamp = Time.time - testStartTime,
            RecoveryTime = 0f // updates when error is rescolved
        };

        errorEvents[dataIndex].Add(error);
    }

    public void RecordErrorRecovery()
    {
        if (currentTestIndex <= 0 || !isTimerRunning)
            return;

        int dataIndex = currentTestIndex - 1;

        if (errorEvents[dataIndex].Count > 0)
        {
            ErrorEvent lastError = errorEvents[dataIndex][errorEvents[dataIndex].Count - 1]; //needs to be fixed
            if (lastError.RecoveryTime == 0f)
            {
                lastError.RecoveryTime = (Time.time - testStartTime) - lastError.TimeStamp;
                errorEvents[dataIndex][errorEvents[dataIndex].Count - 1] = lastError; // update the eror
            }
        }
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

    // first device in connection
    public void RecordDeviceSelectionStart(string deviceName)
    {
        if (currentTestIndex <= 0 || !isTimerRunning) return;

        if (firstSelectedDevice == null)
        {
            firstSelectedDevice = deviceName;
            firstSelectionTime = Time.time - testStartTime;
        }
    }

    // second device in connection
    public void RecordConnectionCompleted(string device1, string device2, int colorIndex, string connectionType)
    {
        if (currentTestIndex <= 0 || !isTimerRunning) return;
        int dataIndex = currentTestIndex - 1;
        if (firstSelectedDevice == null) return;

        ConnectionEvent connectionEvent = new ConnectionEvent
        {
            Device1 = firstSelectedDevice,
            Device2 = device2,
            ColorIndex = colorIndex,
            SelectionStartTime = firstSelectionTime,
            CompletionTime = Time.time - testStartTime,
            ConnectionType = connectionType,
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

    // track when hovering over device icon
    public void RecordDeviceHoverStart(string deviceName)
    {
        if (!deviceHoverStartTimes.ContainsKey(deviceName))
        {
            deviceHoverStartTimes[deviceName] = Time.time;
        }
    }

    public void RecordDeviceHoverEnd(string deviceName)
    {
        if (currentTestIndex <= 0 || !isTimerRunning) return;

        int dataIndex = currentTestIndex - 1;

        if (deviceHoverStartTimes.ContainsKey(deviceName))
        {
            float hoverDuration = Time.time - deviceHoverStartTimes[deviceName];
            string key = $"Hover-{deviceName}";

            if (!colorSelectionTimings[dataIndex].ContainsKey(key))
            {
                colorSelectionTimings[dataIndex][key] = hoverDuration;
            }
            else
            {
                colorSelectionTimings[dataIndex][key] += hoverDuration;
            }

            deviceHoverStartTimes.Remove(deviceName);
        }
    }

    // track gaze time for panels
    public void RecordGaze(string area, float duration)
    {
        if (currentTestIndex <= 0 || !isTimerRunning) return;

        int dataIndex = currentTestIndex - 1;

        if (attentionDistribution[dataIndex].ContainsKey(area))
        {
            attentionDistribution[dataIndex][area] += duration;
        }
        else
        {
            attentionDistribution[dataIndex][area] = duration;
        }
    }

    public float[] GetTestCompletionTimes()
    {
        return testCompletionTimes;
    }

    public int[] GetTestErrorCounts()
    {
        return testErrorCounts;
    }

    public void RecordColorSelection(int colorIndex)
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

    public string ExportTimerData() // FOR ALL DATA
    {
        // netowrking test data
        string data = "Test,CompletionTime,ErrorCount\n";
        for (int i = 0; i < testCompletionTimes.Length; i++)
        {
            data += $"{i + 1},{testCompletionTimes[i]},{testErrorCounts[i]}\n";
        }

        // color usage data
        data += "\n\nColor Usage Data:\n";
        data += "Test,ColorIndex,UsageCount\n";

        for (int i = 0; i < colorUsageCounts.Length; i++)
        {
            foreach (var kvp in colorUsageCounts[i])
            {
                data += $"{i + 1},{kvp.Key},{kvp.Value}\n";
            }
        }

        // color change sequences data
        data += "\n\nColor Change Sequences:\n";
        data += "Test,FromColor,ToColor,TimeStamp\n";

        for (int i = 0; i < colorChangeSequences.Length; i++)
        {
            foreach (ColorChange change in colorChangeSequences[i])
            {
                data += $"{i + 1},{change.FromColorIndex},{change.ToColorIndex},{change.TimeStamp}\n";
            }
        }

        // connection time data (between icons)
        data += "\n\nConnection Timing Data:\n";
        data += "Test,Connection,TimeTaken\n";

        for (int i = 0; i < connectionTimings.Length; i++)
        {
            foreach (var kvp in connectionTimings[i])
            {
                data += $"{i + 1},{kvp.Key},{kvp.Value}\n";
            }
        }

        // connection details data
        data += "\n\nConnection Events:\n";
        data += "Test,Device1,Device2,ColorIndex,StartTime,CompletionTime,ConnectionType,TimeSinceLastColorSelection\n";

        for (int i = 0; i < connectionEvents.Length; i++)
        {
            foreach (ConnectionEvent evt in connectionEvents[i])
            {
                data += $"{i + 1},{evt.Device1},{evt.Device2},{evt.ColorIndex},{evt.SelectionStartTime}," +
                        $"{evt.CompletionTime},{evt.ConnectionType},{evt.TimeSinceLastColorSelection}\n";
            }
        }

        // error details data
        data += "\n\nError Events:\n";
        data += "Test,ErrorType,Device1,Device2,UsedColorIndex,RequiredColorIndex,TimeStamp,RecoveryTime\n";

        for (int i = 0; i < errorEvents.Length; i++)
        {
            foreach (ErrorEvent evt in errorEvents[i])
            {
                data += $"{i + 1},{evt.ErrorType},{evt.Device1},{evt.Device2},{evt.UsedColorIndex}," +
                        $"{evt.RequiredColorIndex},{evt.TimeStamp},{evt.RecoveryTime}\n";
            }
        }

        // gaze panel tracking data
        data += "\n\nAttention Distribution:\n";
        data += "Test,Area,Duration\n";

        for (int i = 0; i < attentionDistribution.Length; i++)
        {
            foreach (var kvp in attentionDistribution[i])
            {
                data += $"{i + 1},{kvp.Key},{kvp.Value}\n";
            }
        }

        return data;
    }

    public void SaveDataToFile()
    {
        string data = ExportTimerData();
        string directoryPath = Application.dataPath + "/TestData";
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // creates a unique file name for each test output, has color palette in filename
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string colorPalette = GetColorPaletteName();
        string filename = $"{directoryPath}/ColorTest_{colorPalette}_{timestamp}.csv";


        File.WriteAllText(filename, data);
    }

    private string GetColorPaletteName()
    {
        ColorPaletteManager paletteManager = FindObjectOfType<ColorPaletteManager>();
        if (paletteManager != null)
        {
            // access palette from color ui
            System.Reflection.FieldInfo fieldInfo = typeof(ColorPaletteManager).GetField("currentPaletteIndex",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            if (fieldInfo != null)
            {
                int index = (int)fieldInfo.GetValue(paletteManager);

                // get palette name for csv outpout
                System.Reflection.FieldInfo palettesField = typeof(ColorPaletteManager).GetField("colorPalettes",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                if (palettesField != null)
                {
                    var palettes = palettesField.GetValue(paletteManager) as List<ColorPaletteManager.ColorPalette>;
                    if (palettes != null && index >= 0 && index < palettes.Count)
                    {
                        return palettes[index].paletteName.Replace(" ", "");
                    }
                }
            }
        }

        return "Unknown";
    }
}