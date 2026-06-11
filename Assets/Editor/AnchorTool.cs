// AnchorTool.cs
// Place this file inside any folder named "Editor" in your Unity project.
// Usage: Select one or more UI GameObjects in the Hierarchy, then press Ctrl + A.
// The anchors of each selected RectTransform will snap to its current position/size
// relative to the parent, so the element stays perfectly in place at any resolution.

using UnityEngine;
using UnityEditor;

public static class AnchorTool
{
    // MenuItem path uses %a  →  % = Ctrl (Cmd on Mac),  a = the 'A' key.
    // The validate method keeps the item (and shortcut) disabled when nothing
    // appropriate is selected, which prevents accidental conflicts.
    [MenuItem("Tools/Set Anchors to Current Rect %a", false, 100)]
    private static void SetAnchors()
    {
        // Support multi-selection
        foreach (GameObject go in Selection.gameObjects)
        {
            RectTransform rt = go.GetComponent<RectTransform>();
            if (rt == null) continue;

            RectTransform parent = rt.parent as RectTransform;
            if (parent == null) continue;

            // Record the object for Undo before modifying it
            Undo.RecordObject(rt, "Set Anchors to Current Rect");

            Vector2 parentSize = parent.rect.size;

            // Guard against a zero-size parent (e.g. Canvas not yet laid out)
            if (parentSize.x == 0 || parentSize.y == 0)
            {
                Debug.LogWarning($"[AnchorTool] Parent of '{go.name}' has zero size – skipping.");
                continue;
            }

            // Current anchors and corners in parent-local space
            Vector2 anchorMin = rt.anchorMin;
            Vector2 anchorMax = rt.anchorMax;

            // Convert the four corners of the RectTransform to parent-local space
            Vector3[] corners = new Vector3[4];
            rt.GetWorldCorners(corners);

            // Transform corners into the parent's local coordinate system
            for (int i = 0; i < 4; i++)
                corners[i] = parent.InverseTransformPoint(corners[i]);

            // corners[0] = bottom-left, corners[2] = top-right  (Unity winding order)
            float newMinX = (corners[0].x - parent.rect.x) / parentSize.x;
            float newMinY = (corners[0].y - parent.rect.y) / parentSize.y;
            float newMaxX = (corners[2].x - parent.rect.x) / parentSize.x;
            float newMaxY = (corners[2].y - parent.rect.y) / parentSize.y;

            // Apply new anchors
            rt.anchorMin = new Vector2(newMinX, newMinY);
            rt.anchorMax = new Vector2(newMaxX, newMaxY);

            // After moving the anchors the offsets need to be zeroed so the
            // element doesn't jump.  The element already fills the anchored area
            // exactly, so offsets should be (0, 0, 0, 0).
            rt.offsetMin = Vector2.zero;   // left / bottom
            rt.offsetMax = Vector2.zero;   // right / top

            EditorUtility.SetDirty(rt);
        }

        Debug.Log($"[AnchorTool] Anchors updated on {Selection.gameObjects.Length} object(s).");
    }

    // Only enable the menu item (and therefore the shortcut) when at least one
    // selected GameObject has a RectTransform with a RectTransform parent.
    [MenuItem("Tools/Set Anchors to Current Rect %a", true)]
    private static bool SetAnchorsValidate()
    {
        foreach (GameObject go in Selection.gameObjects)
        {
            RectTransform rt = go.GetComponent<RectTransform>();
            if (rt != null && rt.parent is RectTransform)
                return true;
        }
        return false;
    }
}