using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class ColorPaletteManager : MonoBehaviour
{
    [SerializeField] private ColorButton[] colorButtons;
    [SerializeField] private List<ColorPalette> colorPalettes = new List<ColorPalette>();
    private int currentPaletteIndex = 0;
    [System.Serializable]

    public class ColorPalette
    {
        public string paletteName;
        public Color[] colors = new Color[4]; 
    }

    void Awake()
    {
        if (colorPalettes.Count == 0)
        {
            SetupDefaultPalettes();
        }
    }

    void Start()
    {
        if (colorButtons == null || colorButtons.Length == 0)
        {
            colorButtons = FindObjectsOfType<ColorButton>();
        }

        ApplyPalette(0);
    }

    private void SetupDefaultPalettes()
    {
        ColorPalette palette1 = new ColorPalette
        {
            paletteName = "Standard (RGBY)",
            colors = new Color[4]
            {
                Color.blue,
                Color.yellow,
                Color.red,
                Color.green
            }
        };
        colorPalettes.Add(palette1);

        ColorPalette palette2 = new ColorPalette
        {
            paletteName = "Networking Standard",
            colors = new Color[4]
            {
                new Color(1.0f, 0.5f, 0.0f), 
                new Color(0.0f, 0.5f, 0.0f),
                new Color(0.0f, 0.0f, 0.5f), 
                new Color(0.5f, 0.0f, 0.5f) 
            }
        };
        colorPalettes.Add(palette2);

        ColorPalette palette3 = new ColorPalette
        {
            paletteName = "Colorblind-Friendly",
            colors = new Color[4]
            {
                new Color(0.0f, 0.45f, 0.7f), 
                new Color(0.9f, 0.6f, 0.0f), 
                new Color(0.8f, 0.4f, 0.7f),
                new Color(0.4f, 0.65f, 0.3f)  
            }
        };
        colorPalettes.Add(palette3);

        ColorPalette palette4 = new ColorPalette
        {
            paletteName = "Distracting",
            colors = new Color[4]
            {
                new Color(1.0f, 0.0f, 1.0f),  
                new Color(0.5f, 1.0f, 0.0f),   
                new Color(0.0f, 1.0f, 1.0f),
                new Color(0.5f, 0.0f, 0.0f)    
            }
        };
        colorPalettes.Add(palette4);
    }

    public void ApplyPalette(int paletteIndex)
    {
        currentPaletteIndex = paletteIndex;
        ColorPalette palette = colorPalettes[paletteIndex];

        // update colors
        for (int i = 0; i < 4 && i < colorButtons.Length; i++)
        {
            ColorButton button = colorButtons[i];
            if (button != null)
            {
                Color newColor = palette.colors[i];

                //update button colors
                button.SetButtonColor(newColor);
                Renderer renderer = button.GetComponent<Renderer>();
            }
        }

        ColorButton.UpdateSelectedButtonColor();
    }

    void Update() // USE KEYBOARD 1-4 to change palette for tetsting
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                ApplyPalette(0);
            }
            else if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                ApplyPalette(1);
            }
            else if (Keyboard.current.digit3Key.wasPressedThisFrame)
            {
                ApplyPalette(2);
            }
            else if (Keyboard.current.digit4Key.wasPressedThisFrame)
            {
                ApplyPalette(3);
            }
        }
    }
}