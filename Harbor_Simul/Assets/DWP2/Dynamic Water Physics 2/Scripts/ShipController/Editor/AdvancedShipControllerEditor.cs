using System.Collections;
using System.Collections.Generic;
using DWP2.WaterEffects;
using UnityEditor;
using UnityEngine;

namespace DWP2.ShipController
{
    [CustomEditor(typeof(AdvancedShipController))]
    [CanEditMultipleObjects]
    public class AdvancedShipControllerEditor : Editor
    {
        private AdvancedShipController asc;

        public void OnValidate()
        {
            if (asc == null) return;
            // Check for Anchor script that might be missing if upgrade DWP => DWP2
            Anchor a = asc.GetComponent<Anchor>();
            if(a == null)
            {
                asc.gameObject.AddComponent<Anchor>();
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            asc = (AdvancedShipController)target;

            EditorUtils.DrawLogo("AdvancedShipControllerLogo");

            DrawDefaultInspector();

            serializedObject.ApplyModifiedProperties();
        }
    }
}