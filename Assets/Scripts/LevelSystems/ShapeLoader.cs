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
        TextAsset jsonFile = Resources.Load<TextAsset>("shapes");
        database = JsonUtility.FromJson<ShapeDatabase>(jsonFile.text);
        Debug.Log("Loaded Shapes: " + database.shapes.Count);
    }
}