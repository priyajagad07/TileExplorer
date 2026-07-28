using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;

[CustomEditor(typeof(WorldData))]
public class WorldDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        if (GUILayout.Button("✨ Auto Generate Destinations"))
        {
            Generate((WorldData)target);
        }
    }

    private void Generate(WorldData world)
    {
        if (world.backgrounds == null || world.backgrounds.Length == 0)
        {
            Debug.LogWarning("Assign backgrounds first.");
            return;
        }

        world.destinations = new DestinationData[world.backgrounds.Length];

        for (int i = 0; i < world.backgrounds.Length; i++)
        {
            Sprite sprite = world.backgrounds[i];

            if (sprite == null)
            {
                Debug.LogWarning($"Background {i + 1} is missing in {world.worldName}");
                continue;
            }

            DestinationData destination = new DestinationData();

            destination.background = sprite;

            string name = CleanSpriteName(sprite.name);

            destination.destinationName = name;
            destination.description = GenerateDescription(name);

            world.destinations[i] = destination;
        }

        EditorUtility.SetDirty(world);
        AssetDatabase.SaveAssets();

        Debug.Log($"{world.worldName}: Generated {world.destinations.Length} destinations.");
    }

    private string CleanSpriteName(string spriteName)
    {
        // Remove leading numbers (1-, 2-, etc.)
        spriteName = Regex.Replace(spriteName, @"^\d+[-_]", "");

        // Remove trailing _0, _1...
        spriteName = Regex.Replace(spriteName, @"_\d+$", "");

        // Replace separators
        spriteName = spriteName.Replace("-", " ");
        spriteName = spriteName.Replace("_", " ");

        return spriteName.Trim();
    }

    private string GenerateDescription(string destinationName)
    {
        if (DestinationDescriptions.Data.TryGetValue(destinationName, out string description))
            return description;

        return $"Explore {destinationName} and begin a new adventure.";
    }
}