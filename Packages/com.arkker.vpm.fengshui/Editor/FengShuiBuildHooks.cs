using aRkker; // Add this to reference the FengShuiMaster namespace
using VRC.SDK3.Components;
using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;






#if !COMPILER_UDONSHARP && UNITY_EDITOR
using VRC.SDKBase.Editor.BuildPipeline;
using VRC.SDKBase;
using UnityEditor;
using UnityEngine;

namespace aRkker
{
    [InitializeOnLoad]
    public class FengShuiMasterPreBuildValidator : IVRCSDKBuildRequestedCallback
    {
        public int callbackOrder => 0;

        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.ExitingEditMode) return;
            Setup();
        }

        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            return Setup();
        }

        public static bool Setup()
        {
            FengShuiMaster fsm = GameObject.FindObjectOfType<FengShuiMaster>();

            if (fsm == null)
            {
                Debug.LogWarning("No FengShuiMaster found in the scene.");
                return true; // Continue build
            }

            VRCPickup[] pickups = GameObject.FindObjectsOfType<VRCPickup>();
            Debug.Log($"Found {pickups.Length} pickups in the scene.");

            foreach (VRCPickup pickup in pickups)
            {
                if (pickup == null) continue;

                // Check if already exists
                if (Array.Exists(fsm.objectsWithPickups, obj => obj == pickup.gameObject))
                {
                    Debug.Log($"Pickup '{pickup.gameObject.name}' already exists in FengShuiMaster.");
                    continue;
                }

                // Add the new pickup
                fsm.AddPickup(pickup.gameObject);
                Debug.Log($"Added pickup '{pickup.gameObject.name}' to FengShuiMaster.");
            }

            // Mark the FengShuiMaster as dirty to ensure it saves the updated array
            UnityEditor.EditorUtility.SetDirty(fsm);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(fsm.gameObject.scene);

            return true; // Continue build
        }


        [MenuItem("FengShui/Import VRChat Log file")]
        static void ImportVRChatLogFile()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appData = Directory.GetParent(localAppData).FullName;
            string localLowPath = Path.Combine(appData, "LocalLow", "VRChat", "VRChat");

            string path = EditorUtility.OpenFilePanel("Select VRChat log file", localLowPath, "txt");

            if (string.IsNullOrEmpty(path))
            {
                EditorUtility.DisplayDialog("Error", "No file selected", "OK");
                return;
            }

            string[] lines;
            using (FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader reader = new StreamReader(fileStream))
            {
                var fileContent = reader.ReadToEnd();
                lines = fileContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            }
            bool isDumping = false;
            List<string> pickupLines = new List<string>();
            List<string> lastPickupLines = new List<string>();

            foreach (string line in lines)
            {
                if (line.Contains("==== FengShuiMaster: Running PickupDump ===="))
                {
                    isDumping = true;
                    pickupLines.Clear();
                }
                else if (line.Contains("==== FengShuiMaster: PickupDump Complete ===="))
                {
                    isDumping = false;
                    lastPickupLines = new List<string>(pickupLines);
                }

                if (isDumping && line.Contains("FSMO"))
                {
                    pickupLines.Add(line);
                }
            }

            if (lastPickupLines.Count == 0)
            {
                EditorUtility.DisplayDialog("Error", "No pickup data found in the selected log file", "OK");
                return;
            }

            foreach (string pickupLine in lastPickupLines)
            {
                Debug.Log("Pickup line: " + pickupLine);
                string processedLine = pickupLine.Replace("(", "").Replace(")", "");

                string[] parts = processedLine.Split('#');
                if (parts.Length != 4)
                {
                    Debug.LogError("Invalid pickup line: " + pickupLine);
                    continue;
                }

                string objectName = parts[0].Substring(parts[0].IndexOf("FSMO|") + 5);

                Debug.Log("Object name: " + objectName);
                Debug.Log("Position: " + parts[1]);
                Debug.Log("Rotation: " + parts[2]);
                Debug.Log("Scale: " + parts[3]);

                string[] positionParts = parts[1].Split(',');
                string[] rotationParts = parts[2].Split(',');
                string[] scaleParts = parts[3].Split(',');
                if (positionParts.Length != 3 || rotationParts.Length != 4 || scaleParts.Length != 3)
                {
                    Debug.LogError("Invalid position, rotation, or scale in pickup line: " + pickupLine);
                    continue;
                }

                Vector3 position = new Vector3(
                    float.Parse(positionParts[0], CultureInfo.InvariantCulture),
                    float.Parse(positionParts[1], CultureInfo.InvariantCulture),
                    float.Parse(positionParts[2], CultureInfo.InvariantCulture)
                );
                Quaternion rotation = new Quaternion(
                    float.Parse(rotationParts[0], CultureInfo.InvariantCulture),
                    float.Parse(rotationParts[1], CultureInfo.InvariantCulture),
                    float.Parse(rotationParts[2], CultureInfo.InvariantCulture),
                    float.Parse(rotationParts[3], CultureInfo.InvariantCulture)
                );

                Vector3 scale = new Vector3(
                    float.Parse(scaleParts[0], CultureInfo.InvariantCulture),
                    float.Parse(scaleParts[1], CultureInfo.InvariantCulture),
                    float.Parse(scaleParts[2], CultureInfo.InvariantCulture)
                );

                // Find the object in the scene
                GameObject obj = GameObject.Find(objectName);
                if (obj == null)
                {
                    Debug.LogError("Could not find object with name: " + objectName);
                    continue;
                }

                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.transform.localScale = scale;
            }

        }

        [MenuItem("FengShui/Add FengShui Master")]
        static void AddFengShuiMaster()
        {
            FengShuiMaster fsm = GameObject.FindObjectOfType<FengShuiMaster>();

            if (fsm != null)
            {
                EditorUtility.DisplayDialog("Error", "FengShui Master already exists in the scene", "OK");
                return;
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
}
#endif
