using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class TestTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TestManager testManager;

    private float testStartTime;
    private float testEndTime;
    private bool isTimerRunning = false;
    private float elapsedTime = 0f;

    private float[] testCompletionTimes;
    private int[] testErrorCounts;
    private int currentTestIndex = -1;

    private Dictionary<int, int>[] colorUsageCounts;

    public class TestData
    {
        public float[] TestCompletionTimes;
        public int[] TestErrorCounts;
    }

    private void Start()
    {
        // create data storage
        int testCount = testManager.GetTestCount();
        testCompletionTimes = new float[testCount];
        testErrorCounts = new int[testCount];

        // reset timer
        UpdateTimer(0);

        colorUsageCounts = new Dictionary<int, int>[testCount];

        for (int i = 0; i < testCount; i++ )
        {
            colorUsageCounts[i] = new Dictionary<int, int>();
        }
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            elapsedTime = Time.time - testStartTime;
            UpdateTimer(elapsedTime);
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
            UpdateTimer(elapsedTime);
        }
        else
        {
            isTimerRunning = false;
            elapsedTime = 0f;
            UpdateTimer(elapsedTime);
        }
    }

    public void StopTest()
    {
        bool isTutorial = (currentTestIndex == 0);

        if ( !isTimerRunning && !isTutorial) return;

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

    public void RecordError( string device1, string device2,
                                   int usedColorIndex, int requiredColorIndex)
    {
        if (currentTestIndex <= 0 || !isTimerRunning)
            return;

        int dataIndex = currentTestIndex - 1;

        if (dataIndex >= 0 && dataIndex < testErrorCounts.Length)
        {
            testErrorCounts[dataIndex]++;
        }
    }

    private void UpdateTimer(float timeInSeconds)
    {
        if (timerText != null)
        {
            timerText.text = FormatTime(timeInSeconds);
        }
    }

    private string FormatTime(float timeInSeconds)
    {
        int m = Mathf.FloorToInt(timeInSeconds / 60);
        int s = Mathf.FloorToInt(timeInSeconds % 60);
        return string.Format("{0:00}:{1:00}", m,  s );
    }


    public TestData GetAllTestData()
    {
        return new TestData
        {
            TestCompletionTimes = this.testCompletionTimes,
            TestErrorCounts = this.testErrorCounts,
        };
    }
}