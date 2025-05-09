using UnityEngine;

public class IconLayoutManager : MonoBehaviour
{
    [System.Serializable]
    public class IconLayout
    {
         public string layoutName;
         public IconPosition[] iconPositions;
    }

    [System.Serializable]
    public class IconPosition
    {
        public string iconName;
        public Vector3 position;
    }

    [SerializeField] private IconLayout[] layouts;
    [SerializeField] private Transform iconsContainer; 

    public void ApplyLayout(int layoutIndex)
    {
        if (layoutIndex < 0 || layoutIndex >= layouts.Length)
            return;

        IconLayout layout =  layouts[layoutIndex];

        foreach (IconPosition iconPos in layout.iconPositions)
        {
            Transform iconTransform = iconsContainer.Find(iconPos.iconName);
            iconTransform.localPosition = iconPos.position;
        }
    }

    
    public void CaptureCurrentLayout(string layoutName, int layoutIndex)
    {
        if (layouts == null || layoutIndex < 0 || layoutIndex >= layouts.Length)
            return;

        // create new layout
        if (layouts[layoutIndex] == null)
            layouts[layoutIndex] = new IconLayout();

         layouts[layoutIndex].layoutName = layoutName;

        // grabs all icons in icon container
        int childCount = iconsContainer.childCount;
        layouts[layoutIndex].iconPositions = new IconPosition[childCount];

        // gets position of each icon
        for (int i = 0; i < childCount; i++)
        {
            Transform child = iconsContainer.GetChild(i);
            layouts[layoutIndex].iconPositions[i] = new  IconPosition
            {
                iconName = child.name,
                position = child.localPosition
            };
        }
    }
}