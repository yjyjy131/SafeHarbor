using System.Collections;
using System.Collections.Generic;
using DWP2.WaterEffects;
using UnityEditor;
using UnityEngine;

namespace DWP2
{
    [CustomEditor(typeof(WaterObjectWizard))]
    [CanEditMultipleObjects]
    public class WaterObjectWizardEditor : Editor
    {
        private WaterObjectWizard wow;
        
        public override void OnInspectorGUI()
        {

            serializedObject.Update();
            wow = (WaterObjectWizard) target;
            if (wow == null) return;

            EditorUtils.DrawLogo("WaterObjectWizardLogo");

            DrawDefaultInspector();
            
            if (GUILayout.Button("Auto-Setup"))
            {
                foreach (WaterObjectWizard wow in targets)
                {
                    RunWizard(wow);
                }
            }

            if (wow == null) return;
            MeshFilter mf = wow.GetComponent<MeshFilter>();
            if (mf != null)
            {
                if (mf.sharedMesh != null)
                {
                    if (mf.sharedMesh.triangles.Length / 3 > 4000)
                    {
                        EditorGUILayout.HelpBox("Large mesh detected. Expect WaterObjectWizard to take a few moments to setup this object.", MessageType.Info);
                    }
                }
            }
        }
        
        
        public static void RunWizard(WaterObjectWizard wow)
        {
            GameObject target = wow.gameObject;
            
            // Check for existing water object
            if (target.GetComponent<WaterObject>() != null)
            {
                Debug.LogWarning($"WaterObjectWizard: {target.name} already contains WaterObject component.");
            }

            // Check for mesh filter
            MeshFilter mf = target.GetComponent<MeshFilter>();
            if (mf == null)
            {
                Debug.LogError($"WaterObjectWizard: MeshFilter not found. WaterObject requires MeshFilter to work.");
                return;
            }

            // Add rigidbody
            Rigidbody parentRigidbody = target.transform.FindRootRigidbody();
            if (parentRigidbody == null)
            {
                Debug.Log("WaterObjectWizard: Parent rigidbody not found. Adding new.");
                parentRigidbody = target.AddComponent<Rigidbody>();
                parentRigidbody.angularDrag = 0.15f;
                parentRigidbody.drag = 0.05f;
                parentRigidbody.interpolation = RigidbodyInterpolation.None;

                RigidbodyMassFromChildren rmfc = target.AddComponent<RigidbodyMassFromChildren>();

                CenterOfMass com = target.AddComponent<CenterOfMass>();
            }
            
            // Add collider
            int colliderCount = parentRigidbody.transform.GetComponentsInChildren<Collider>().Length;
            if (colliderCount == 0)
            {
                Debug.Log($"WaterObjectWizard: Found 0 colliders on object {parentRigidbody.name}. Adding new mesh collider.");
                MeshCollider mc = target.AddComponent<MeshCollider>();
                mc.convex = true;
                mc.isTrigger = false;
            }
            
            // Add water object
            if (target.GetComponent<WaterObject>() == null)
            {
                WaterObject wo = target.AddComponent<WaterObject>();
                wo.SetMaterialDensity(400);
                wo.convexifyMesh = true;
                wo.simplifyMesh = true;
                wo.targetTris = 64;
                wo.GenerateSimMesh();
                wo.Init();
            }
            
            // Add Water Particle System and Particle System
            if (wow.addWaterParticleSystem)
            {
                GameObject waterParticleSystemPrefab = Resources.Load<GameObject>("WaterParticleSystemPrefab");
                if (waterParticleSystemPrefab == null)
                {
                    Debug.LogError("Could not load WaterParticleSystemPrefab from Resources.");
                }
                else
                {
                    if (target.GetComponent<ParticleSystem>() == null)
                    {
                        UnityEditorInternal.ComponentUtility.CopyComponent(waterParticleSystemPrefab
                            .GetComponent<ParticleSystem>());
                        UnityEditorInternal.ComponentUtility.PasteComponentAsNew(target);
                    }

                    if (target.GetComponent<WaterParticleSystem>() == null)
                    {
                        UnityEditorInternal.ComponentUtility.CopyComponent(waterParticleSystemPrefab
                            .GetComponent<WaterParticleSystem>());
                        UnityEditorInternal.ComponentUtility.PasteComponentAsNew(target);
                    }
                }
            }
            
            DestroyImmediate(wow);
        }
    }
}
