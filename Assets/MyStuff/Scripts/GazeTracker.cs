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

        // *** visble red ray only used in scene view of play mode ***
        Debug.DrawRay(gazeOrigin.position, adjustedDirection * rayDistance, Color.red);

        RaycastHit hit;
        if (Physics.Raycast(gazeOrigin.position, adjustedDirection, out hit, rayDistance, trackingLayers))
        {

            // get gaze panel (instructions, color ui, icon panel)
            string newGazeArea = DetermineGazeArea(hit.transform.gameObject);
            

            // check if new area
            if (newGazeArea != currentGazeArea)
            {
                // record previous panel gaze time 
                float duration = Time.time - gazeStartTime;
                areaDurations[currentGazeArea] += duration;

                // record gaze time only in test 1-3
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
            // track when gaze is not on one of three panels
            if (currentGazeArea != "None")
            {
                // record previous panel gaze time
                float duration = Time.time - gazeStartTime;
                areaDurations[currentGazeArea] += duration;

                // record gaze time only in test 1-3
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

        // set none if none of three panels are gazed at
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