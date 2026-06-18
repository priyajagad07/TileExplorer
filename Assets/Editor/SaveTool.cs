#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public class SaveTool
{
    [MenuItem("Tools/Clear Save Data")]
    public static void ClearData()
    {
        string path = Application.persistentDataPath + "/savegame.json";
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("✅ JSON Save Data deleted successfully!");
        }
        else
        {
            Debug.Log("⚠️ No JSON save data found to delete.");
        }

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("✅ Leftover PlayerPrefs wiped.");
    }
}
#endif