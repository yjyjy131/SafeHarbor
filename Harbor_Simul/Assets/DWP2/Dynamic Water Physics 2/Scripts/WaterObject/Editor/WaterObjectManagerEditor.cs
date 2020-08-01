using System;
using System.Linq;
using DWP2.WaterEffects;
using UnityEditor;
using UnityEngine;


namespace DWP2
{
    [CustomEditor(typeof(WaterObjectManager))]
    [CanEditMultipleObjects]
    [ExecuteInEditMode]
    public class WaterObjectManagerEditor : Editor
    {
        private WaterObjectManager wm;
        private SerializedProperty fluidDensity;
        private SerializedProperty dynamicForceCoefficient;
        private SerializedProperty highResolutionWaterQueries;
        private SerializedProperty queryWaterHeights;
        private SerializedProperty queryWaterVelocities;
        private SerializedProperty generateGizmos;
        private SerializedProperty waterObjectTag;
        private SerializedProperty skinFrictionDrag;
        private SerializedProperty dynamicForcePower;
        private SerializedProperty velocityDotPower;

        private void OnEnable()
        {
            fluidDensity = serializedObject.FindProperty("fluidDensity");
            dynamicForceCoefficient = serializedObject.FindProperty("dynamicForceCoefficient");
            skinFrictionDrag = serializedObject.FindProperty("skinFrictionDrag");
            dynamicForcePower = serializedObject.FindProperty("dynamicForcePower");
            velocityDotPower = serializedObject.FindProperty("velocityDotPower");
            
            highResolutionWaterQueries = serializedObject.FindProperty("highResolutionWaterQueries");
            queryWaterHeights = serializedObject.FindProperty("queryWaterHeights");
            queryWaterVelocities = serializedObject.FindProperty("queryWaterVelocities");
            generateGizmos = serializedObject.FindProperty("generateGizmos");
            waterObjectTag = serializedObject.FindProperty("waterObjectTag");
        }


        public override void OnInspectorGUI()
        {
            wm = (WaterObjectManager) target;
            serializedObject.Update();

            EditorUtils.DrawLogo("WaterObjectManagerLogo");
            
            GUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Simulation Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(fluidDensity,  
                new GUIContent("Fluid Density [kg/m3]", "Density of the fluid being simulated. 1000 for fresh water and" +
                                                        "1030 for salt water. Affects both buoyant forces and dynamic forces."));
            EditorGUILayout.PropertyField(dynamicForceCoefficient, 
                new GUIContent("Dynamic Force Coeff.", "Coefficient by which all forces except buoyant force are multiplied." +
                                                       "Default is 1. Coefficient is relative to the standard force for the set fluid density."));
            EditorGUILayout.PropertyField(skinFrictionDrag);
            EditorGUILayout.PropertyField(dynamicForcePower);
            EditorGUILayout.PropertyField(velocityDotPower);

            EditorGUILayout.PropertyField(highResolutionWaterQueries,
                new GUIContent("High Resolution Water Queries", "If enabled water height and velocity will be queried at vertex instead of triangle level. Expect improved accuracy at the cost of performance."));
            if(wm.ActiveTriCount > 1000 && highResolutionWaterQueries.boolValue)
            {
                EditorGUILayout.HelpBox("High triangle count detected. You might want to disable High Resolution Water Queries.", MessageType.Info);
            }
            EditorGUILayout.PropertyField(queryWaterHeights,
                new GUIContent("Query Water Heights", "Should water heights be used in calculations? When disabled water asset is not queried for water height. This can be helpful when wavy water asset is used, but without" +
                "waves. E.g. Crest with wave amplitude set to 0. Improves performance."));
            EditorGUILayout.PropertyField(queryWaterVelocities,
                new GUIContent("Query Water Velocities", "Should water velocities be used in calculations? When disabled water asset is not queried for water velocity."));
            EditorGUILayout.PropertyField(waterObjectTag,
                new GUIContent("Water Object Tag", "Game object that contains the main water script should be tagged with this tag. If no objects with this tag are found " +
                "in the scene it will be assumed that the water is at y=0."));
            EditorGUILayout.PropertyField(generateGizmos,
                new GUIContent("Generate Debug Gizmos", "Should WaterObject related gizmos be generated?"));

            if (Application.isPlaying)
            {
                EditorGUILayout.HelpBox($"Simulating a total of {wm.TriangleData.Count} tris on " +
                                        $"{wm.WaterObjects.Count} WaterObject(s)", MessageType.Info);
                
                EditorGUILayout.LabelField($"Active Tri Count: {wm.ActiveTriCount}");
                EditorGUILayout.LabelField($"Active Underwater Tri Count: {wm.ActiveUnderwaterTriCount}");
                EditorGUILayout.LabelField($"Active Above Water Tri Count: {wm.ActiveAboveWaterTriCount}");
                EditorGUILayout.LabelField($"Disabled Tri Count: {wm.DisabledTriCount}");
                EditorGUILayout.LabelField($"Destroyed Tri Count: {wm.DestroyedTriCount}");
                EditorGUILayout.LabelField($"Inactive Tri Count: {wm.InactiveTriCount}");
            }
            GUILayout.EndVertical();

            if(Application.isPlaying)
            {
                if (GUILayout.Button("Synchronize"))
                {
                    WaterObjectManager.Instance.Synchronize();
                }
                
                EditorGUILayout.HelpBox("Exit play mode to make changes.", MessageType.Info);
            }

            GUILayout.Space(10);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
