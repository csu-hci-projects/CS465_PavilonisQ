using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TestUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI instructionsText;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private GameObject nextTaskButton;

    [Header("Settings")]
    [SerializeField] private float hintDelay = 30f; 

    private bool hintShown = false;
    private float taskStartTime;

    private void Start()
    {
        hintText.gameObject.SetActive(false);
    }

    public void SetInstructions(string instructions)
    {
        if (instructionsText != null)
        {
            instructionsText.text = instructions;
        }

        hintShown = false;
        taskStartTime = Time.time;

        if (hintText != null)
        {
            hintText.text = "";
            hintText.gameObject.SetActive(false);
        }
    }

    public void SetHint(string hint)
    {
        if (hintText != null)
        {
            hintText.gameObject.SetActive(false);
            hintText.text = hint;
        }
    }

    public void SetProgress(int current, int total)
    {
        if (progressText != null)
        {
            progressText.text = $"Task {current} of {total}";
        }
    }

    public void ShowNextTaskButton(bool show)
    {
        if (nextTaskButton != null)
        {
            nextTaskButton.SetActive(show);
        }
    }

    private void Update()
    {
        if (!hintShown && (Time.time - taskStartTime > hintDelay) && hintText != null && !string.IsNullOrEmpty(hintText.text))
        {
            hintText.gameObject.SetActive(true);
            hintShown = true;
        }
    }
}