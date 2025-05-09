using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class OutlineTool
{
    public static void AddOutline(GameObject obj, Color color, float width, Outline.Mode mode)
    {
        Outline outline = obj.GetComponent<Outline>();
        if (outline == null)
        {
            outline = obj.AddComponent<Outline>();
        }

        outline.OutlineColor = color;
        outline.OutlineWidth = width;
        outline.OutlineMode = mode;
        outline.enabled = true;
    }


    public  static void RemoveOutline(GameObject obj)
    {
        Outline outline = obj.GetComponent<Outline>();

        if (outline != null)
        {
             outline.enabled = false;
        }
    }
}
