using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DWP2.ShipController;

namespace DWP2.DemoContent
{
    public class GUIHandler : MonoBehaviour
    {

        public ShipChanger changer;
        public Text speedText;
        public Text rudderText;
        public Image anchorImage;
        public bool reset = false;

        private AdvancedShipController asc;

        private void Update()
        {
            Rigidbody shipRb = ShipChanger.shipGo.GetComponent<Rigidbody>();
            if (shipRb != null)
            {
                asc = shipRb.GetComponent<DWP2.ShipController.AdvancedShipController>();

                float speed = shipRb.velocity.magnitude * 1.95f;
                speedText.text = "SPEED: " + string.Format("{0:0.0}", speed) + "kts";

                if (asc.rudders.Count > 0)
                {
                    float rudderAngle = asc.rudders[0].Angle;
                    rudderText.text = "RUDDER: " + string.Format("{0:0.0}", rudderAngle) + "°";
                }

                if (asc.Anchor != null)
                {
                    if(asc.Anchor.Dropped)
                    {
                        anchorImage.gameObject.SetActive(true);
                    }
                    else
                    {
                        anchorImage.gameObject.SetActive(false);
                    }
                }
            }
        }

        public void ResetScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }
    }
}
