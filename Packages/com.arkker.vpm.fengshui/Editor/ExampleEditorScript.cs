using UnityEditor;
using UnityEngine;

public class ExampleEditorScript
{

    [MenuItem("aRkker's Crap/Add FengShui Master")]
    static void AddFengShuiMaster()
    {
        GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name == "FengShuiMaster")
            {
                EditorUtility.DisplayDialog(
                    "Error",
                    "An object named 'FengShuiMaster' already exists in the scene. You cannot add more than one.",
                    "OK"
                );
                return;
            }
        }
        string prefabPath = "Packages/com.arkker.vpm.fengshui/Runtime/Prefabs/FengShuiMaster.prefab";

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

        if (prefab == null)
        {
            EditorUtility.DisplayDialog("Error", "Could not find FengShui Master prefab at path:\n" + prefabPath, "OK");
            return;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

        if (instance != null)
        {
            // Optionally position the prefab at the origin
            instance.transform.position = Vector3.zero;
            Undo.RegisterCreatedObjectUndo(instance, "Add FengShui Master");
            EditorUtility.DisplayDialog("Success", "FengShui Master added to the scene. You should see it in your inspector after clicking OK ", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Error", "Failed to instantiate FengShui Master prefab.", "OK");
        }
    }
}
