using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DWP2
{
    [CustomEditor(typeof(RigidbodyMassFromChildren))]
    [ExecuteInEditMode]
    [System.Serializable]
    public class RigidbodyMassFromChildrenEditor : Editor
    {
        RigidbodyMassFromChildren t;

        public void OnValidate()
        {
            if (t != null) t.Calculate();
        }

        public void OnEnable()
        {
            t = (RigidbodyMassFromChildren)target;
            if (t != null) t.Calculate();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (t == null) return;

            WaterObject[] waterObjects = t.GetComponentsInChildren<WaterObject>();
            int n = waterObjects.Length;
            string names = "";
            for(int i = 0; i < n; i++)
            {
                names += waterObjects[i].gameObject.name;
                if (i != n - 1) names += ", ";
            }
            
            EditorGUILayout.HelpBox(
                $"Mass is being calculated from {waterObjects.Length} WaterObject(s): {names}", MessageType.Info);

            if (t.GetComponent<Rigidbody>().mass == 1) t.Calculate();

            if (GUILayout.Button("Calculate Mass From Density"))
            {
                t.Calculate();
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
