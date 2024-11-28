
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

namespace aRkker
{

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public partial class FengShuiMaster : UdonSharpBehaviour
    {
        [SerializeField] public GameObject[] objectsWithPickups = new GameObject[0];

        void Start()
        {
            // We need to search the scene for all objects with VRCPickup component
            // and add them to the objectsWithPickups array

        }

        public void AddPickup(GameObject obj)
        {
            Debug.Log("Adding tracking to object: " + obj.name);
            GameObject[] newObjectsWithPickups = new GameObject[objectsWithPickups.Length + 1];
            for (int i = 0; i < objectsWithPickups.Length; i++)
            {
                newObjectsWithPickups[i] = objectsWithPickups[i];
            }
            newObjectsWithPickups[objectsWithPickups.Length] = obj;
            objectsWithPickups = newObjectsWithPickups;
        }


        public void RunPickupDump()
        {
            Debug.Log("==== FengShuiMaster: Running PickupDump ====");
            foreach (GameObject obj in objectsWithPickups)
            {
                if (obj == null) continue;
                Transform objTransform = obj.transform;

                // Log the scale, position, and rotation of each object
                Debug.Log($"FSMO|{obj.name}#{objTransform.position}#{objTransform.rotation}#{objTransform.localScale}");
            }
            Debug.Log("==== FengShuiMaster: PickupDump Complete ====");
        }
    }
}