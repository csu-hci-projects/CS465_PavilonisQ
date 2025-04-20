using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ColorButton : MonoBehaviour
{
    [SerializeField] private Color buttonColor = Color.white;
    private XRSimpleInteractable interactable;

    private Vector3 originalScale;
    private static ColorButton currentlySelectedButton;

    private void Start()
    {
     
        originalScale = transform.localScale; // COULDNT GET THIS TO WORK

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

            // remove outline
            Outline prevOutline = currentlySelectedButton.GetComponent<Outline>();
            if (prevOutline != null)
            {
                prevOutline.enabled = false;
            }
        }

        // scale new selected icon
        transform.localScale = originalScale * 1.2f;

        // new outline
        Outline outline = GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = true;
        }

        currentlySelectedButton = this;

        // set selected color on selectionmanager
        SimpleSelectionManager selectionManager = FindObjectOfType<SimpleSelectionManager>();
        if (selectionManager != null)
        {
            int colorIndex = GetColorIndex();
            selectionManager.SetConnectionColor(new Color(buttonColor.r, buttonColor.g, buttonColor.b, 1.0f), colorIndex);
        }
    }

    public Color GetButtonColor()
    {
        return new Color(buttonColor.r, buttonColor.g, buttonColor.b, 1.0f);
    }

    public void SetAsDefault() // ALSO COUDLDNT GET TO WORK, SAME ISSUE AS ABOVE?
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
        if (gameObject.name == "Color2") //MAYBE NOT WORKING TOO
        {
            GetComponent<Renderer>().material.color = newColor;
        }
    }

    public static void UpdateSelectedButtonColor()
    {
        if (currentlySelectedButton != null)
        {
            Color buttonColor = Color.white;
            System.Reflection.FieldInfo fieldInfo = typeof(ColorButton).GetField("buttonColor",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            if (fieldInfo != null)
            {
                buttonColor = (Color)fieldInfo.GetValue(currentlySelectedButton);
            }

            SimpleSelectionManager selectionManager = FindObjectOfType<SimpleSelectionManager>();
            if (selectionManager != null)
            {
                selectionManager.SetConnectionColor(buttonColor);
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

        ColorPaletteManager paletteManager = FindObjectOfType<ColorPaletteManager>();
        if (paletteManager != null)
        {
            System.Reflection.FieldInfo fieldInfo = typeof(ColorPaletteManager).GetField("colorButtons",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            if (fieldInfo != null)
            {
                ColorButton[] buttons = (ColorButton[])fieldInfo.GetValue(paletteManager);
                for (int i = 0; i < buttons.Length; i++)
                {
                    if (buttons[i] == this)
                    {
                        return i;
                    }
                }
            }
        }
        return -1;
    }
}