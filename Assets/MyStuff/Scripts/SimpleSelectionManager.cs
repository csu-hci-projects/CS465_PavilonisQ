using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SimpleSelectionManager : MonoBehaviour
{
    [SerializeField] private Color outlineColor = Color.black;
    [SerializeField] private float outlineWidth = 15f;
    [SerializeField] private Outline.Mode outlineMode = Outline.Mode.OutlineAll;
    [SerializeField] private GameObject lineRendererPrefab;

    private List<ConnectionInfo> connections = new List<ConnectionInfo>();
    private GameObject firstSelectedIcon = null;
    private GameObject secondSelectedIcon = null;
    private int currentColorIndex = 0;
    private Color currentLineColor = Color.white;
    private TestTimer testTimer;
    private TestManager testManager;

    private class ConnectionInfo
    {
        public GameObject connectionObject;
        public GameObject startIcon;
        public GameObject endIcon;

        public ConnectionInfo(GameObject connection, GameObject start, GameObject end)
        {
            connectionObject = connection;
            startIcon = start;
            endIcon = end;
        }

        public bool ConnectsIcons(GameObject icon1, GameObject icon2)
        {
            return (startIcon == icon1 && endIcon == icon2) ||
                   (startIcon == icon2 && endIcon == icon1);
        }
    }


    private void Start()
    {
        // grab all icons with XR simple interactable script (network icons, menu buttons, color buttons)
        XRSimpleInteractable[] interactables = FindObjectsOfType<XRSimpleInteractable>();

        testTimer = FindObjectOfType<TestTimer>();
        testManager = FindObjectOfType<TestManager>();

        foreach (XRSimpleInteractable interactable in interactables)
        {
            interactable.selectEntered.AddListener(OnIconSelected);
        }

        foreach (var button in FindObjectsOfType<ColorButton>())
        {
            if (!button.CompareTag("ColorButton"))
            {
                button.gameObject.tag = "ColorButton";
            }
        }

        Transform colorPalette = GameObject.Find("ColorPalette")?.transform;
        if (colorPalette != null)
        {
            Transform defaultButtonTransform = colorPalette.Find("Color1");
            if (defaultButtonTransform != null)
            {
                    ColorButton defaultButton = defaultButtonTransform.GetComponent<ColorButton>();
                    if (defaultButton != null)
                    {
                        currentLineColor = defaultButton.GetButtonColor();
                        defaultButton.ForceSelection(new Vector3(0.4441015f, 0.409564f, 0.01f)); // position of the WAN button (default selected color button)
                    }
            }
        }
     }


    private void OnIconSelected(SelectEnterEventArgs args)
    {
        GameObject selectedObject = args.interactableObject.transform.gameObject;

        if (checkColorButton(selectedObject))
        { 
            return; 
        }

        // first icon, if nothing selected already
        if (firstSelectedIcon == null)
        {
            firstSelectedIcon = selectedObject;
            AddOutline(firstSelectedIcon);

            // record first device select
            if (testTimer != null)
            {
                testTimer.RecordConnectionFirstDevice(selectedObject.name);
            }
        }
        // check if selecting same icon as first, deselect if so
        else if (selectedObject == firstSelectedIcon)
        {
            RemoveOutline(firstSelectedIcon);
            firstSelectedIcon = null;
        }
        // create connection if second icon is different from first
        else
        {
            completeConnection(selectedObject);
        }
    }


    private bool checkColorButton(GameObject selectedObject)
    {
        if (selectedObject.CompareTag("ColorButton"))
        {
            // only color function for color buttons
            ColorButton colorButton = selectedObject.GetComponent<ColorButton>();
            if (colorButton != null)
            {
                SetConnectionColor(colorButton.GetButtonColor(), colorButton.GetColorIndex());
            }
            return true;
        }
        return false;
    }


    private void completeConnection(GameObject selectedObject)
    {
        secondSelectedIcon = selectedObject;

        // check for previous connection between two selected icons
        ConnectionInfo existingConnection = FindConnection(firstSelectedIcon, secondSelectedIcon);

        if (existingConnection != null)
        {
            if (checkConnection())
            {
                return;
            }
            RemoveConnection(existingConnection);
        }
        else
        {
            CreateConnection(firstSelectedIcon, secondSelectedIcon);

            if (testTimer != null)
            {
                testTimer.RecordConnectionSecondDevice(firstSelectedIcon.name, secondSelectedIcon.name,
                                                  currentColorIndex);
            }
        }

        // remove highlight on first selected
        RemoveOutline(firstSelectedIcon);

        // resets
        firstSelectedIcon = null;
        secondSelectedIcon = null;
    }


    private bool checkConnection()
    {
        // check if connection is correct (cannot remove correct), remove if not
        if (testManager != null && testManager.connectionCompleted(firstSelectedIcon.name, secondSelectedIcon.name))
        {
            // deselect icon
            RemoveOutline(firstSelectedIcon);
            firstSelectedIcon = null;
            secondSelectedIcon = null;
            return true;
        }
        return false;
    }


    private ConnectionInfo FindConnection(GameObject icon1, GameObject icon2)
    {
        foreach (ConnectionInfo connection in connections)
        {
            if (connection.ConnectsIcons(icon1, icon2))
            {
                return connection;
            }
        }
        return null;
    }


    private void RemoveConnection(ConnectionInfo connection)
    {
        connections.Remove(connection);
        Destroy(connection.connectionObject);
    }


    private void CreateConnection(GameObject fromIcon, GameObject toIcon)
    {
        if (lineRendererPrefab == null)
        {
            return;
        }

        GameObject lineObj = Instantiate(lineRendererPrefab);
        LineRenderer lineRenderer = lineObj.GetComponent<LineRenderer>();
        //position
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, fromIcon.transform.position);
        lineRenderer.SetPosition(1, toIcon.transform.position);
        //color
        lineRenderer.startColor = currentLineColor;
        lineRenderer.endColor = currentLineColor;
        // connection
        ConnectionInfo newConnection = new ConnectionInfo(lineObj, fromIcon, toIcon);
        connections.Add(newConnection);
        testManager.CheckTestProgress(fromIcon, toIcon, currentColorIndex);
    }


    public void SetConnectionColor(Color color, int colorIndex = -1)
{
    currentLineColor = color;
    currentColorIndex = colorIndex;
    
    if (testTimer != null && colorIndex >= 0)
    {
        testTimer.RecordColorNameForCSV(colorIndex);
    }
}


    public void ClearAllConnections()
    {
        foreach (ConnectionInfo connection in connections)
        {
            if (connection.connectionObject != null)
            {
                Destroy(connection.connectionObject);
            }
        }

        connections.Clear();

        if (firstSelectedIcon != null)
        {
            RemoveOutline(firstSelectedIcon);
            firstSelectedIcon = null;
        }
    }


    private void AddOutline(GameObject obj)
    {
        OutlineTool.AddOutline(obj, outlineColor, outlineWidth, outlineMode);    
    }


    private void RemoveOutline(GameObject obj)
    {
        OutlineTool.RemoveOutline(obj);
    }


}