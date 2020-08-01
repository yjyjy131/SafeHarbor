using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DWP2
{
    public static class EditorUtils
    {
        public static float maxLogoHeight = 80f;
        public static float logoRatio = 6.666667f;
        public static float logoHeight;

        public static void DrawLogo(string name)
        {
            GUILayout.Space(7);
            GUILayout.Label("");
            Texture2D logo = Resources.Load("Logos/" + name) as Texture2D;
            if (logo == null) return;

            if (Event.current.type == EventType.Repaint)
            {
                Rect lastRect = GUILayoutUtility.GetLastRect();
                logoHeight = Mathf.Clamp(lastRect.width / logoRatio, 0f, maxLogoHeight);
                float logoWidth = Mathf.Clamp(lastRect.width, 0f, logoHeight * logoRatio);
                GUI.DrawTexture(new Rect(lastRect.x, lastRect.y, lastRect.width, logoHeight), logo);
            }

            GUILayout.Space(logoHeight - 12f);

        }
        
        public static void ProgressBar (float value, string label) {
            // Get a rect for the progress bar using the same margins as a textfield:
            Rect rect = GUILayoutUtility.GetRect (18, 18, "TextField");
            EditorGUI.ProgressBar (rect, value, label);
            EditorGUILayout.Space ();
        }

        public static void FillInTexture(ref Texture2D tex, Color color)
        {
            Color[] fillColorArray =  tex.GetPixels();
 
            for(var i = 0; i < fillColorArray.Length; i++)
            {
                fillColorArray[i] = color;
            }
  
            tex.SetPixels( fillColorArray );
            tex.Apply();
        }
    }
}

