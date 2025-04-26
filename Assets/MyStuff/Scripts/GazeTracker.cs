using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class GazeTracker : MonoBehaviour
{
    [SerializeField] private Transform gazeOrigin; 
    [SerializeField] private LayerMask trackingLayers;
    [SerializeField] private float rayDistance = 20f;
    [SerializeField] private float gazeYOffset = 0.15f;

    [Header("Areas to Track")]
    [SerializeField] private GameObject instructionsArea;
    [SerializeField] private GameObject networkArea;
    [SerializeField] private GameObject colorPaletteArea;

    private string currentGazeArea = "None";
    private float gazeStartTime;
    private Dictionary<string, float> areaDurations = new Dictionary<string, float>();

    private TestTimer testTimer;

    private void Start()
    {
        testTimer = FindObjectOfType<TestTimer>();

        areaDurations["Instructions"] = 0f;
        areaDurations["Network"] = 0f;
        areaDurations["ColorPalette"] = 0f;
        areaDurations["None"] = 0f;

        if (gazeOrigin == null)
        {
            XRGazeInteractor gazeInteractor = FindObjectOfType<XRGazeInteractor>();
            gazeOrigin = gazeInteractor.transform;
        }

        gazeStartTime = Time.time;
    }

    private void Update()
    {
        if (gazeOrigin == null) return;

        Vector3 adjustedDirection = gazeOrigin.forward;
        adjustedDirection.y -= gazeYOffset;
        adjustedDirection.Normalize();

        RaycastHit hit;
        if (Physics.Raycast(gazeOrigin.position, adjustedDirection, out hit, rayDistance, trackingLayers))
        {

            // get gaze panel (instructions, color ui, icon panel)
            string newGazeArea = DetermineGazeArea(hit.transform.gameObject);
            
            if (newGazeArea != currentGazeArea)
            {
                float duration = Time.time - gazeStartTime;
                areaDurations[currentGazeArea] += duration;

                if (testTimer != null)
                {
                    testTimer.RecordGaze(currentGazeArea, duration);
                }

                // update gaze panel area
                currentGazeArea = newGazeArea;
                gazeStartTime = Time.time;
            }
        }
        else
        {
            if (currentGazeArea != "None")
            {
                float duration = Time.time - gazeStartTime;
                areaDurations[currentGazeArea] += duration;

                if (testTimer != null)
                {
                    testTimer.RecordGaze(currentGazeArea, duration);
                }

                currentGazeArea = "None";
                gazeStartTime = Time.time;
            }
        }
    }

    private string DetermineGazeArea(GameObject hitObject)
    {
        Transform current = hitObject.transform;

        while (current != null)
        {
            if (instructionsArea != null && current.gameObject == instructionsArea)
                return "Instructions";

            if (networkArea != null && current.gameObject == networkArea)
                return "Network";

            if (colorPaletteArea != null && current.gameObject == colorPaletteArea)
                return "ColorPalette";

            current = current.parent;
        }
        return "None";
    }

    public void RecordFinalGazeDuration()
    {
        // gaze time for current panel
        float duration = Time.time - gazeStartTime;
        areaDurations[currentGazeArea] += duration;

        // record timer only for tests and not tutorial
        if (testTimer != null)
        {
            testTimer.RecordGaze(currentGazeArea, duration);
        }

        gazeStartTime = Time.time;
    }
}