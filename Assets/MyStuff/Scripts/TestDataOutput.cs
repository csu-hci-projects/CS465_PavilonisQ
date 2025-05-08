using UnityEngine;
using System;
using System.IO;

public class TestDataOutput
{
    private TestTimer.TestData testData;
    
    public TestDataOutput(TestTimer timer)
    {
        testData = timer.GetAllTestData();
    }

    public string ExportData()
    {
        // networking test data
        string data = "Test,CompletionTime,ErrorCount\n";
        for (int i = 0; i < testData.TestCompletionTimes.Length; i++)
        {
            data += $"{i + 1},{testData.TestCompletionTimes[i]},{testData.TestErrorCounts[i]}\n";
        }

        // color usage data
        data += "\n\nColor Usage Data:\n";
        data += "Test,ColorIndex,UsageCount\n";

        for (int i = 0; i < testData.ColorUsageCounts.Length; i++)
        {
            foreach (var kvp in testData.ColorUsageCounts[i])
            {
                data += $"{i + 1},{kvp.Key},{kvp.Value}\n";
            }
        }

        // color change sequences data
        data += "\n\nColor Change Sequences:\n";
        data += "Test,FromColor,ToColor,TimeStamp\n";

        for (int i = 0; i < testData.ColorChangeSequences.Length; i++)
        {
            foreach (TestTimer.ColorChange change in testData.ColorChangeSequences[i])
            {
                data += $"{i + 1},{change.FromColorIndex},{change.ToColorIndex},{change.TimeStamp}\n";
            }
        }

        // connection time data (between icons)
        data += "\n\nConnection Timing Data:\n";
        data += "Test,Connection,TimeTaken\n";

        for (int i = 0; i < testData.ConnectionTimings.Length; i++)
        {
            foreach (var kvp in testData.ConnectionTimings[i])
            {
                data += $"{i + 1},{kvp.Key},{kvp.Value}\n";
            }
        }

        // connection details data
        data += "\n\nConnection Events:\n";
        data += "Test,Device1,Device2,ColorIndex,StartTime,CompletionTime,TimeSinceLastColorSelection\n";

        for (int i = 0; i < testData.ConnectionEvents.Length; i++)
        {
            foreach (TestTimer.ConnectionEvent evt in testData.ConnectionEvents[i])
            {
                data += $"{i + 1},{evt.IconDevice1},{evt.IconDevice2},{evt.ColorIndex},{evt.SelectionStartTime}," +
                        $"{evt.CompletionTime},{evt.TimeSinceLastColorSelection}\n";
            }
        }

        // error details data
        data += "\n\nError Events:\n";
        data += "Test,ErrorType,Device1,Device2,UsedColorIndex,RequiredColorIndex,TimeStamp\n";

        for (int i = 0; i < testData.ErrorEvents.Length; i++)
        {
            foreach (TestTimer.ErrorEvent evt in testData.ErrorEvents[i])
            {
                data += $"{i + 1},{evt.ErrorType},{evt.IconDevice1},{evt.IconDevice2},{evt.UsedColorIndex}," +
                        $"{evt.RequiredColorIndex},{evt.TimeStamp}\n";
            }
        }

        // gaze panel tracking data
        data += "\n\nAttention Distribution:\n";
        data += "Test,Area,Duration\n";

        for (int i = 0; i < testData.PanelFocus.Length; i++)
        {
            foreach (var kvp in testData.PanelFocus[i])
            {
                data += $"{i + 1},{kvp.Key},{kvp.Value}\n";
            }
        }

        return data;
    }

    public void SaveDataToFile()
    {
        string data = ExportData();
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
        ColorPaletteManager paletteManager = UnityEngine.Object.FindObjectOfType<ColorPaletteManager>();
        if (paletteManager != null)
        {
            return paletteManager.GetCurrentPaletteName();
        }
        return "Unknown";
    }
}