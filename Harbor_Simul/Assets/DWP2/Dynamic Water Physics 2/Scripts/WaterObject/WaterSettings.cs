using UnityEngine;

namespace Water
{
    public class WaterSettings : MonoBehaviour
    {
        public const int dragCurveArrLength = 1000;
        public static float[] dragCurveArray = new float[dragCurveArrLength];

        public AnimationCurve dragCurveGUI = new AnimationCurve();
        

        public void Awake()
        {
            for (int i = 0; i < dragCurveArrLength; i++)
            {
                dragCurveArray[i] = dragCurveGUI.Evaluate((float) i / (float) dragCurveArrLength);
            }
        }
    }
}