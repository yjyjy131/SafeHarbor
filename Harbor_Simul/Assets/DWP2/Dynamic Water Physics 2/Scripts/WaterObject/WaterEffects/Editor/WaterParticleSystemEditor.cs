using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;


namespace DWP2.WaterEffects
{
    [CustomEditor(typeof(WaterParticleSystem))]
    [CanEditMultipleObjects]
    [ExecuteAlways]
    public class WaterParticleSystemEditor : Editor
    {
        private WaterParticleSystem wps;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            wps = (WaterParticleSystem)target;

            EditorUtils.DrawLogo("WaterParticleSystemLogo");

            DrawDefaultInspector();

            if(!wps.GetComponent<ParticleSystem>())
            {
                GameObject waterParticleSystemPrefab = Resources.Load<GameObject>("WaterParticleSystemPrefab");
                if (waterParticleSystemPrefab == null)
                {
                    Debug.LogError("Could not load WaterParticleSystemPrefab from Resources.");
                }
                else
                {
                    UnityEditorInternal.ComponentUtility.CopyComponent(waterParticleSystemPrefab
                        .GetComponent<ParticleSystem>());
                    UnityEditorInternal.ComponentUtility.PasteComponentAsNew(wps.gameObject);
                }
            }
        }
    }
}