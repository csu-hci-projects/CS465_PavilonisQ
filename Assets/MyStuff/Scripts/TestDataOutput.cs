using UnityEngine;
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
        string timestamp =  System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string colorPalette = GetColorPaletteName();
        string filename = $"{directoryPath}/ColorTest_{colorPalette}_{timestamp}.csv";

        File.WriteAllText(filename, data);
    }

    private string GetColorPaletteName()
    {
        ColorPaletteManager paletteManager =  UnityEngine.Object.FindObjectOfType<ColorPaletteManager>();
        if (paletteManager != null)
        {
          return paletteManager.GetCurrentPaletteName();
        }
        return "Unknown";
    }
}