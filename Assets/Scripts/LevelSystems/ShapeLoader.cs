using UnityEngine;

public class ShapeLoader : MonoBehaviour
{
    public static ShapeDatabase database;

    void Awake()
    {
        LoadShapes();
    }

    void LoadShapes()
    {
        database = Resources.Load<ShapeDatabase>("ShapeDatabase");

        if (database == null)
        {
            Debug.LogError("ShapeDatabase not found!");
            return;
        }

        Debug.Log("Loaded Shapes: " + database.shapes.Count);
    }
}