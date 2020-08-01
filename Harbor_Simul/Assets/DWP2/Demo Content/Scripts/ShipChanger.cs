using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using DWP2.ShipController;

namespace DWP2.DemoContent
{
    public class ShipChanger : MonoBehaviour
    {
        private List<GameObject> ships = new List<GameObject>();
        private List<ShipCameras> cameras = new List<ShipCameras>();

        public int activeShipIndex;
        public int activeCameraIndex;

        public static GameObject shipGo;
        public static GameObject cameraGo;


        void Start()
        {
            if (ships.Count == 0)
            {
                ships = GameObject.FindGameObjectsWithTag("Ship").ToList();
                if (ships.Count == 0) return;
            }

            for (int i = 0; i < ships.Count; i++)
            {
                ShipCameras cs = new ShipCameras();
                foreach (Transform child in ships[i].transform)
                {
                    if (child.CompareTag("ShipCamera") || child.CompareTag("MainCamera"))
                    {
                        cs.cameras.Add(child.gameObject);
                    }
                }
                cameras.Add(cs);
            }

            cameraGo = cameras[activeShipIndex].cameras[activeCameraIndex];
            shipGo = ships[activeShipIndex];

            DisableAllCamerasExceptActive();
            DisableAllShipsExceptActive();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                NextCamera();
            }
            if (Input.GetKeyDown(KeyCode.V))
            {
                ChangeShip(++activeShipIndex);
            }
        }

        public void NextShip()
        {
            ChangeShip(++activeShipIndex);
        }

        /// <summary>
        /// Changes ship to index-th ship in the ships list, if such index exists.
        /// </summary>
        public void ChangeShip(int index)
        {
            int shipCount = ships.Count;
            if (shipCount == 0) return;

            // Check if ship exists and change
            if (index >= shipCount)
            {
                index = 0;
            }
            else if (index < 0)
            {
                index = 0;
            }
            activeShipIndex = index;

            // Check if ship contains camera
            if (cameras[activeShipIndex].cameras.Count == activeCameraIndex)
                activeCameraIndex = 0;

            // Update game object
            shipGo = ships[activeShipIndex];

            DisableAllCamerasExceptActive();
            DisableAllShipsExceptActive();
        }

        /// <summary>
        /// Switches to next camera if multiple cameras are attached to the same ship.
        /// </summary>
        public void NextCamera()
        {
            activeCameraIndex++;
            if (cameras[activeShipIndex].cameras.Count == activeCameraIndex)
            {
                activeCameraIndex = 0;
            }
            cameraGo = cameras[activeShipIndex].cameras[activeCameraIndex];
            DisableAllCamerasExceptActive();
        }

        private void DisableAllCamerasExceptActive()
        {
            for (int i = 0; i < cameras.Count; i++)
            {
                for (int j = 0; j < cameras[i].cameras.Count; j++)
                {
                    cameras[i].cameras[j].gameObject.SetActive(false);
                    cameras[i].cameras[j].gameObject.tag = "ShipCamera";
                }
            }

            cameras[activeShipIndex].cameras[activeCameraIndex].gameObject.SetActive(true);
            cameras[activeShipIndex].cameras[activeCameraIndex].gameObject.tag = "MainCamera";
        }

        private void DisableAllShipsExceptActive()
        {
            for (int i = 0; i < ships.Count; i++)
            {
                ships[i].GetComponent<DWP2.ShipController.AdvancedShipController>().Deactivate();
                for (int j = 0; j < cameras[i].cameras.Count; j++)
                {
                    if (j != activeCameraIndex && i != activeShipIndex)
                    {
                        cameras[i].cameras[j].SetActive(false);
                        cameras[i].cameras[j].tag = "ShipCamera";
                    }
                }
            }

            ships[activeShipIndex].GetComponent<DWP2.ShipController.AdvancedShipController>().Activate();
        }

        // Workaround to enable nested list serialization
        [System.Serializable]
        public class ShipCameras
        {
            [SerializeField]
            public List<GameObject> cameras = new List<GameObject>();
        }
    }
}
