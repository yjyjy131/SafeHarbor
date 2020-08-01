using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;


namespace DWP2
{
    [CustomEditor(typeof(WaterObject))]
    [CanEditMultipleObjects]
    [ExecuteAlways]
    public class WaterObjectEditor : Editor
    {
        private SerializedProperty simplifyMesh;
        private SerializedProperty convexifyMesh;
        private SerializedProperty targetTris;
        private SerializedProperty density;
        private SerializedProperty mass;
        private SerializedProperty volume;
        private SerializedProperty editorHasErrors;

        private Texture2D originalMeshPreviewTexture;
        private Texture2D simMeshPreviewTexture;

        private float previewHeight;
        private Texture2D greyTexture;
        private GUIStyle centeredStyle;

        private WaterObject waterObject;

        private void OnEnable()
        {
            simplifyMesh = serializedObject.FindProperty("simplifyMesh");
            convexifyMesh = serializedObject.FindProperty("convexifyMesh");
            targetTris = serializedObject.FindProperty("targetTris");
            density = serializedObject.FindProperty("_density");
            mass = serializedObject.FindProperty("_mass");
            volume = serializedObject.FindProperty("_volume");
            editorHasErrors = serializedObject.FindProperty("editorHasErrors");
            
            greyTexture = new Texture2D(10, 10);
            EditorUtils.FillInTexture(ref greyTexture, new Color32(82, 82, 82, 255));
        }

        private void OnValidate()
        {
            if (waterObject == null) return;
            //waterObject.Init();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            waterObject = (WaterObject) target;
            if(!waterObject.Initialized) waterObject.Init();

            EditorUtils.DrawLogo("WaterObjectLogo");

            if (centeredStyle == null)
            {            
                centeredStyle = GUI.skin.GetStyle("Label");
                centeredStyle.alignment = TextAnchor.MiddleCenter;
                centeredStyle.normal.textColor = Color.white;
                centeredStyle.fontStyle = FontStyle.Bold;
            }

            if (!Application.isPlaying)
            {
                GUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("Simulation Mesh Settings", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(simplifyMesh, 
                new GUIContent("Simplify", "Should the simulation mesh be simplified? If the mesh has >50 triangles this option" +
                                                " is recommended. You can adjust simplification strength through 'Simplification Ratio' field."));

                if (waterObject.simplifyMesh)
                {
                    EditorGUILayout.PropertyField(targetTris, 
                        new GUIContent("Target Triangle Count", "Quality of the generated simplified / decimated mesh. Lower setting will result in a simulation mesh " +
                                                          "with lower number of triangles and therefore better performance - O(n). Use lowest acceptable setting."));
                }
            
                EditorGUILayout.PropertyField(convexifyMesh, 
                    new GUIContent("Convexify", "Should the simulation mesh be made convex? " +
                                                     "This must be used if the mesh is not closed (missing one of its surfaces, e.g. only bottom of the hull has triangles)."));

                if (GUILayout.Button("Update Simulation Mesh"))
                {
                    foreach (WaterObject wo in targets)
                    {
                        wo.StopSimMeshPreview();
                        wo.Init();
                        wo.GenerateSimMesh();
                        Undo.RecordObject(wo, "Updated Simulation Mesh");
                        EditorUtility.SetDirty(wo);
                    }
                }
                
                if (GUILayout.Button("Toggle In-Scene Preview"))
                {
                    foreach (WaterObject wo in targets)
                    {
                        if (wo.PreviewEnabled)
                        {
                            wo.StopSimMeshPreview();
                        }
                        else
                        {
                            wo.StartSimMeshPreview();
                        }
                    }
                }

                if (targets.Length == 1)
                {
                    EditorGUILayout.Space();
                    if(Event.current.type == EventType.Repaint) DrawPreviewTexture(waterObject);
                    GUILayout.Space(previewHeight - 8f);
                }
                
                GUILayout.EndVertical();
                

                // Material settings
                if (waterObject.TargetRigidbody != null)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    EditorGUILayout.LabelField("Material Settings", EditorStyles.boldLabel);
                    
                    if (waterObject.TargetRigidbody.GetComponent<RigidbodyMassFromChildren>() == null)
                    {
                        EditorGUILayout.HelpBox($"If you want to set mass of parent rigidbody {waterObject.TargetRigidbody.name} through its children, " +
                                                $"attach 'RigidbodyMassFromChildren' component to it.", MessageType.Info);
                        if(GUILayout.Button("Add RigidbodyMassFromChildren Component to Rigidbody"))
                        {
                            foreach (WaterObject wo in targets)
                            {
                                if(wo.TargetRigidbody != null && wo.TargetRigidbody.GetComponent<RigidbodyMassFromChildren>() == null)
                                {
                                    RigidbodyMassFromChildren rmfc = wo.TargetRigidbody.gameObject.AddComponent<RigidbodyMassFromChildren>();
                                    wo.WaterObjectMaterialIndex = 0;
                                    rmfc.Calculate();
                                }
                            }
                        }
                    }
                    else
                    {
                        int prevIndex = waterObject.WaterObjectMaterialIndex;
                        waterObject.WaterObjectMaterialIndex = EditorGUILayout.Popup(new GUIContent("Material Preset", 
                            "Sets the density of the object from one of the presets. If you want to set your own density, select 'Custom' option."), 
                            prevIndex, WaterObjectMaterials.MaterialNames);

                        if(prevIndex != waterObject.WaterObjectMaterialIndex)
                        {
                            foreach (WaterObject wo in targets) wo.WaterObjectMaterialIndex = waterObject.WaterObjectMaterialIndex;
                        }

                        EditorGUI.BeginDisabledGroup(waterObject.WaterObjectMaterialIndex != 0);

                        // Density
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(density, 
                            new GUIContent("Density", "Density in kg/m3 of the material. Mass will be calculated from this and the volume of the simulation mesh, " +
                            "which is auto-calculated."));
                        if(EditorGUI.EndChangeCheck())
                        {
                            foreach (WaterObject wo in targets)
                            {
                                wo.SetMaterialDensity(density.floatValue);
                            }
                            serializedObject.Update();
                        }

                        // Mass
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(mass, 
                            new GUIContent("Mass", "Mass in kg of this object. When multiple objects that are children of the same rigidbody have this field set, mass" +
                                                   " of the parent rigidbody will be calculated as a sum of all of children's masses."));
                        if (EditorGUI.EndChangeCheck())
                        {
                            foreach (WaterObject wo in targets)
                            {
                                wo.SetMaterialMass(mass.floatValue);
                            }
                            serializedObject.Update();
                        }
                        EditorGUI.EndDisabledGroup();

                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.FloatField(new GUIContent("Volume", "Auto-calculated."), waterObject.Volume);
                        EditorGUI.EndDisabledGroup();

                        if (GUILayout.Button("Update Volume"))
                        {
                            foreach (WaterObject wo in targets)
                            {
                                wo.UpdateVolume();
                                Undo.RecordObject(wo, "Calculate Volume");
                                EditorUtility.SetDirty(wo);
                            }
                        }

                        if (waterObject.Density < 80f)
                        {
                            EditorGUILayout.HelpBox(
                                $"Density of the object might be on the low side (<80 km/m3). Avoid this if you are using low-poly simulation mesh or have large triangles.", MessageType.Warning);
                        }
                        else if (WaterObjectManager.Instance != null && waterObject.Density > WaterObjectManager.Instance.fluidDensity)
                        {
                            EditorGUILayout.HelpBox(
                                $"This object has higher density than the fluid density setting on WaterObjectManager. It will sink.", MessageType.Info);
                        }
                    }
                    
                    EditorGUILayout.EndVertical();
                }

                
                // **** WARNINGS ****
                if (WaterObject.showEditorWarnings && targets.Length == 1)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);
                    
                    EditorGUILayout.LabelField("Warnings", EditorStyles.boldLabel);
                    editorHasErrors.boolValue = false;
                    
                    // Warnings
                    // Missing rigidbody
                    if (waterObject.TargetRigidbody == null)
                    {
                        waterObject.TargetRigidbody = waterObject.transform.FindRootRigidbody();
                        if (waterObject.TargetRigidbody == null)
                        {
                            EditorGUILayout.HelpBox($"WaterObject requires a rigidbody attached to the object or its parent(s) to " +
                                                    $"function. Add a rigidbody to object {waterObject.name} or one of its parents.", MessageType.Error);
                            editorHasErrors.boolValue = true;
                            
                            if(GUILayout.Button("Add a Rigidbody"))
                            {
                                foreach (WaterObject wo in targets)
                                {
                                    wo.gameObject.AddComponent<Rigidbody>();
                                }
                            }
                        }
                    }

                    // Collider count
                    if (waterObject.TargetRigidbody != null)
                    {
                        int colliderCount = waterObject.TargetRigidbody.transform.GetComponentsInChildren<Collider>()
                            .Length;
                        if (waterObject.TargetRigidbody.transform.GetComponentsInChildren<Collider>().Length == 0)
                        {
                            EditorGUILayout.HelpBox($"Found {colliderCount} colliders attached to rigidbody {waterObject.TargetRigidbody.name} " +
                                                    $"and its children. At least one collider is required for a rigidbody to work properly.", MessageType.Error);
                            editorHasErrors.boolValue = true;
                            
                            if(GUILayout.Button("Add a MeshCollider"))
                            {
                                foreach (WaterObject wo in targets)
                                {
                                    MeshCollider mc = wo.gameObject.AddComponent<MeshCollider>();
                                    mc.convex = true;
                                    mc.isTrigger = false;
                                }
                            }
                        }
                    }

                    // Excessive triangle count
                    if (waterObject.TriangleCount > 150)
                    {
                        EditorGUILayout.HelpBox($"Possible excessive number of triangles detected ({waterObject.TriangleCount})." +
                                                $" Use simplify mesh option to reduce the number of triangles, or if this is intentional ignore this message." +
                                                $" Recommended number is 16-128.", MessageType.Warning);
                    }

                    // Scale error
                    if (waterObject.transform.localScale.x <= 0 
                        || waterObject.transform.localScale.y <= 0 
                        || waterObject.transform.localScale.z <= 0)
                    {
                        EditorGUILayout.HelpBox($"Scale of this object is negative or zero on one or more of axes. Scale less than or equal to zero is not supported." +
                                                $" WaterObject will still be calculated but with unpredictable results. ", MessageType.Error);
                    }
                    
                    GUILayout.EndVertical();
                }

                
                GUILayout.Space(3);
                if (targets.Length == 1)
                {
                    if(waterObject.TargetRigidbody != null)
                        EditorGUILayout.HelpBox($"Forces are being applied to '{waterObject.TargetRigidbody}' rigidbody.", MessageType.Info);  
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        $"In-editor preview and warnings are only available when a single WaterObject is selected.", MessageType.Info);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Exit play mode to make changes.", MessageType.Info);
            }

            
            GUILayout.Space(10);
            if (serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
        
        void DrawPreviewTexture(WaterObject waterObject)
        {
            if (!waterObject.Initialized)
            {
                previewHeight = 10f;
                return;
            }
            
            // Tri count
            int originalCount = waterObject.OriginalMesh == null ? 0 : waterObject.OriginalMesh.triangles.Length / 3;
            int simulationTriCount = waterObject.SerializedSimulationMesh.triangles.Length / 3;

            
            // Draw preview texture
            Rect lastRect = GUILayoutUtility.GetLastRect();
            
            originalMeshPreviewTexture = AssetPreview.GetAssetPreview(waterObject.OriginalMesh);
            simMeshPreviewTexture = waterObject.SimulationMesh == null ? 
                AssetPreview.GetAssetPreview(waterObject.OriginalMesh) 
                : AssetPreview.GetAssetPreview(waterObject.SimulationMesh);
            
            float startY = lastRect.y + lastRect.height;
            float previewWidth = lastRect.width;
            float maxPreviewWidth = 480f;
            previewWidth = Mathf.Clamp(previewWidth, 240f, maxPreviewWidth);
            float margin = (lastRect.width - previewWidth) * 0.5f;
            float halfWidth = previewWidth * 0.5f;
            
            Rect leftRect = new Rect(lastRect.x + margin, startY, halfWidth, halfWidth);
            Rect rightRect = new Rect(lastRect.x + halfWidth + margin, startY, halfWidth, halfWidth);

            Material previewMaterial = new Material(Shader.Find("UI/Default"));

            GUI.DrawTexture(leftRect, originalMeshPreviewTexture == null ? greyTexture : originalMeshPreviewTexture);
            GUI.DrawTexture(rightRect, simMeshPreviewTexture == null ? greyTexture : simMeshPreviewTexture);

            GUIStyle centeredStyle = GUI.skin.GetStyle("Label");
            centeredStyle.alignment = TextAnchor.MiddleCenter;
            centeredStyle.normal.textColor = Color.white;
            
            Rect leftLabelRect = leftRect;
            leftLabelRect.height = 20f;
            GUI.Label(leftLabelRect, "ORIGINAL", centeredStyle);

            Rect rightLabelRect = rightRect;
            rightLabelRect.height = 20f;
            GUI.Label(rightLabelRect, "SIMULATION", centeredStyle);

            Rect leftBottomLabelRect = leftRect;
            leftBottomLabelRect.y = leftRect.y + halfWidth - 20f;
            leftBottomLabelRect.height = 20f;
            GUI.Label(leftBottomLabelRect, originalCount + " tris");
            
            Rect rightBottomLabelrect = rightRect;
            rightBottomLabelrect.y = rightRect.y + halfWidth - 20f;
            rightBottomLabelrect.height = 20f;
            GUI.Label(rightBottomLabelrect, simulationTriCount + " tris");
            
            previewHeight = halfWidth + 10f;
        }
    }
}

