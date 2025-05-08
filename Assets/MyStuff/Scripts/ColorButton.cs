using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ColorButton : MonoBehaviour
{
    private XRSimpleInteractable interactable;

    private Vector3 originalScale;
    private static ColorButton currentlySelectedButton;

    [SerializeField] private Color buttonColor = Color.white;

    public Color ButtonColor
    {
        get { return buttonColor; }
        private set { buttonColor = value; }
    }

    private void Start()
    {

        originalScale = transform.localScale;

        interactable = GetComponent<XRSimpleInteractable>();
        if (interactable != null)
        {
            interactable.selectEntered.AddListener(OnSelected);
        }

        // adds outline component to icons but disbables for start
        Outline outline = gameObject.GetComponent<Outline>();
        if (outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
            outline.OutlineColor = Color.white;
            outline.OutlineWidth = 3f;
            outline.enabled = false;
        }
    }

    private void OnSelected(SelectEnterEventArgs args)
    {
        if (currentlySelectedButton != null && currentlySelectedButton != this)
        {
            // reset selected icon scale to normal
            currentlySelectedButton.transform.localScale = currentlySelectedButton.originalScale;

            OutlineTool.RemoveOutline(currentlySelectedButton.gameObject);
        }

        // scale new selected icon
        transform.localScale = originalScale * 1.2f;

        // new outline
        OutlineTool.AddOutline(gameObject, Color.white, 3f, Outline.Mode.OutlineAll);

        currentlySelectedButton = this;

        // set selected color on selectionManager
        SimpleSelectionManager  selectionManager = FindObjectOfType<SimpleSelectionManager>();
        if (selectionManager != null)
        {
            int colorIndex = GetColorIndex();
            selectionManager.SetConnectionColor(new Color(buttonColor.r,  buttonColor.g, buttonColor.b,1.0f), colorIndex);
        }
    }

    public Color GetButtonColor()
    {
        return new Color(buttonColor.r, buttonColor.g, buttonColor.b, 1.0f);
    }

    public void SetAsDefault()
    {
        if (currentlySelectedButton != null && currentlySelectedButton != this)
        {
            currentlySelectedButton.transform.localScale = currentlySelectedButton.originalScale;
            Outline prevOutline = currentlySelectedButton.GetComponent<Outline>();
            if (prevOutline != null)
            {
                prevOutline.enabled = false;
            }
        }

        transform.localScale = originalScale * 1.2f; 

        if (transform.localScale.magnitude < 0.01f)
        {
            transform.localScale = new Vector3(0.4441015f, 0.409564f, 0.01f);
        }

        Outline outline = GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = true;
        }
        currentlySelectedButton = this;
    }

    public void ForceSelection(Vector3 selectedScale)
    {
        transform.localScale = selectedScale;

        Outline outline = GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = true;
        }

        currentlySelectedButton = this;
    }

    public void SetButtonColor(Color newColor)
    {
        buttonColor = newColor;

        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = buttonColor;
        }
        if (gameObject.name == "Color2")
        {
            GetComponent<Renderer>().material.color = newColor;
        }
    }

    public static void  UpdateSelectedButtonColor()
    {
        if (currentlySelectedButton != null)
        {
            SimpleSelectionManager selectionManager = Object.FindObjectOfType<SimpleSelectionManager>();
            if (selectionManager != null)
            {
                selectionManager.SetConnectionColor(currentlySelectedButton.ButtonColor);
            }
        }
    }

    public int GetColorIndex()
    {
        string buttonName = gameObject.name;
        if (buttonName.StartsWith("Color") && buttonName.Length > 5)
        {
            if (int.TryParse(buttonName.Substring(5), out int index))
            {
                return index - 1;
            }
        }

        ColorPaletteManager paletteManager = Object.FindObjectOfType<ColorPaletteManager>();
        if (paletteManager != null)
        {
            return paletteManager.GetButtonIndex(this);
        }
        return -1;
    }
}
