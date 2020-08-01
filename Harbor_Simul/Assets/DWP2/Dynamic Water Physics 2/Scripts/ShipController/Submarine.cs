using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DWP2.ShipController
{
    [RequireComponent(typeof(AdvancedShipController))]
    [RequireComponent(typeof(CenterOfMass))]
    public class Submarine : MonoBehaviour
    {
        /// <summary>
        /// Depth at which the submarine will aim to be at. This is achieved by changing the weight of the submarine's rigidbody.
        /// </summary>
        [Tooltip("Depth at which the submarine will aim to be at. This is achieved by changing the weight of the submarine's rigidbody.")]
        public float requestedDepth = 0f;

        /// <summary>
        /// Speed of depth change in m/s when a key is used to change depth.
        /// </summary>
        [Tooltip("Speed of depth change in m/s when a key is used to change depth.")]
        public float inputDepthChangeSpeed = 10f;

        /// <summary>
        /// Sensitivity of the depth algorithm to the deviation from the requested depth.
        /// </summary>
        [Tooltip("Sensitivity of the depth algorithm to the deviation from the requested depth.")]
        [Range(0, 1)]
        public float depthSensitivity = 1f;

        /// <summary>
        /// Maximum additional mass that can be added (taking on water) to the base mass of the rigidbody to make submarine sink.
        /// </summary>
        [Tooltip("Maximum additional mass that can be added (taking on water) to the base mass of the rigidbody to make submarine sink.")]
        [Range(1, 2)]
        public float maxMassFactor = 1.5f;

        /// <summary>
        /// If enabled submarine will try to keep horizontal by shifting the center of mass.
        /// </summary>
        [Tooltip("If enabled submarine will try to keep horizontal by shifting the center of mass.")]
        public bool keepHorizontal = true;

        /// <summary>
        /// Sensitivity of calculation trying to keep the submarine horizontal. Higher number will mean faster reaction.
        /// </summary>
        [Tooltip("Sensitivity of calculation trying to keep the submarine horizontal. Higher number will mean faster reaction.")]
        public float keepHorizontalSensitivity = 1f;

        /// <summary>
        /// Maximum rigidbody center of mass offset that can be used to keep the submarine level.
        /// </summary>
        [Tooltip("Maximum rigidbody center of mass offset that can be used to keep the submarine level.")]
        public float maxMassOffset = 5f;

        public Slider depthSlider;

        private Rigidbody rb;
        private CenterOfMass com;
        private float initialMass;
        private Vector3 initialCOM;
        private float depth;
        private float depthCeoff;
        private float mass;
        private float zOffset;
        private bool showWarnings = true;
        private AdvancedShipController asc;

        [HideInInspector][SerializeField] private float _depthInput;
        public float DepthInput
        {
            get { return _depthInput; }
            set { _depthInput = Mathf.Clamp(value, -1f, 1f); }
        }

        void Start()
        {
            rb = GetComponent<Rigidbody>();
            com = GetComponent<CenterOfMass>();
            asc = GetComponent<AdvancedShipController>();
            initialMass = rb.mass;
            initialCOM = com.centerOfMassOffset;
        }

        void FixedUpdate()
        {
            if (!asc.Active) return;

            GetInput();
            if (DepthInput == 0 && depthSlider != null)
            {
                _depthInput = depthSlider.value;
                if (_depthInput > -0.1f && _depthInput < 0.1f) depthSlider.value = 0f;
            }
            requestedDepth -= DepthInput * inputDepthChangeSpeed * Time.fixedDeltaTime;

            depth = Mathf.Abs(transform.position.y);

            depthCeoff = Mathf.Clamp((requestedDepth - depth) * depthSensitivity * 0.1f, 0f, 1f);
            mass = Mathf.Clamp(1f + depthCeoff, 1f, maxMassFactor) * initialMass;
            rb.mass = mass;

            if (keepHorizontal)
            {
                float angle = Vector3.SignedAngle(transform.up, Vector3.up, transform.right);
                zOffset = Mathf.Clamp(Mathf.Sign(angle) * Mathf.Pow(angle * 0.2f, 2f) * keepHorizontalSensitivity, -maxMassOffset, maxMassOffset);
                com.centerOfMassOffset = new Vector3(initialCOM.x, initialCOM.y, initialCOM.z + zOffset);
            }
        }

        //**** MODIFY THIS IF YOU NEED TO USE DIFFERENT INPUT MANAGER ****
        void GetInput()
        {
            try
            {
                DepthInput = Input.GetAxis("SubmarineDepth");
            }
            catch
            {
                // Show once
                if (showWarnings)
                {
                    Debug.LogWarning("Axis 'SubmarineDepth' is not present in Edit > ProjectSettings > Input. Add 'SubmarineDepth' axis to be able to use this feature");
                    showWarnings = false;
                }             
            }
        }

        public void SetDepth(float d)
        {
            requestedDepth = d;
        }
    }
}