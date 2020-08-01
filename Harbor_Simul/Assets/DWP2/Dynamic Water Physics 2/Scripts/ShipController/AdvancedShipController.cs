using UnityEngine;
using System.Collections.Generic;

namespace DWP2.ShipController
{
    /// <summary>
    /// Script for controling ships, boats and other vessels.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Anchor))]
    [System.Serializable]
    public class AdvancedShipController : MonoBehaviour
    {
        private bool _active = true;
        public bool Active { get => _active; }

        [Tooltip("Handles all of the user input.")]
        [SerializeField]
        public InputStates input = new InputStates();

        /// <summary>
        /// Possible throttle / thruster inputs.
        /// </summary>
        public enum InputMapping { LeftThrottle, RightThrottle, Throttle, BowThruster, SternThruster }

        [Tooltip("List of engines. Each engine is a propulsion system in itself consisting of the engine and the propeller.")]
        [SerializeField]
        public List<Engine> engines = new List<Engine>();

        [Tooltip("List of rudders.")]
        [SerializeField]
        public List<Rudder> rudders = new List<Rudder>();

        [Tooltip("List of either bow or stern thrusters.")]
        [SerializeField]
        public List<Thruster> thrusters = new List<Thruster>();

        public bool dropAnchorWhenInactive = true;
        public bool weighAnchorWhenActive = true;

        private Rigidbody _rb;
        public Rigidbody ShipRigidbody { get { return _rb; } }

        private Anchor _anchor;
        public Anchor Anchor => _anchor;


        /// <summary>
        /// Local Velocity vector of the rigidbody.
        /// Use LocalVelocity.z to get forward velocity, .x to get velocity in right direction and .y in up direction
        /// </summary>
        public Vector3 LocalVelocity
        {
            get { return transform.InverseTransformDirection(ShipRigidbody.velocity); }
        }

        /// <summary>
        /// Speed in m/s.
        /// </summary>
        public float Speed
        {
            get { return LocalVelocity.z; }
        }

        /// <summary>
        /// Speed in knots.
        /// </summary>
        public float SpeedKnots
        {
            get { return Speed * 1.944f; }
        }

        public void Activate()
        {
            foreach (Engine e in engines) e.StartEngine();
            if (weighAnchorWhenActive && _anchor != null) _anchor.Weigh();
            _active = true;
        }

        public void Deactivate()
        {
            foreach (Engine e in engines) e.StopEngine();
            if (dropAnchorWhenInactive && _anchor != null) _anchor.Drop();
            _active = false;
        }

        void Start()
        {
            _rb = GetComponent<Rigidbody>();

            foreach (Thruster thruster in thrusters)
                thruster.Initialize(this);

            foreach (Rudder rudder in rudders)
                rudder.Initialize(this);

            foreach (Engine engine in engines)
            {
                engine.Initialize(this);
            }

            _anchor = GetComponent<Anchor>();
            if(_anchor == null)
            {
                Debug.LogWarning($"Object {name} is missing 'Anchor' component which is required for AdvancedShipController to work properly.");
                _anchor = gameObject.AddComponent<Anchor>();
            }

            input.Initialize(this);
        }

        private void Update()
        {
            input.Update();
        }

        private void FixedUpdate()
        {
            if (!_active) return;

            foreach (Engine engine in engines)
                engine.Update();

            foreach (Rudder rudder in rudders)
                rudder.Update();

            foreach (Thruster thruster in thrusters)
                thruster.Update();

            if(input.AnchorDropWeigh && _active)
            {
                if (Anchor.Dropped) Anchor.Weigh();
                else Anchor.Drop();
            }

            input.PostFixedUpdate();
        }

        private void OnDrawGizmos()
        {
            Start();

            foreach (Rudder rudder in rudders)
            {
                Gizmos.color = Color.magenta;
            }

            foreach (Engine e in engines)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(e.ThrustPosition, 0.2f);
                Gizmos.DrawRay(new Ray(e.ThrustPosition, e.ThrustDirection));
            }

            foreach (Thruster thruster in thrusters)
            {
                if (thruster.inputMapping == AdvancedShipController.InputMapping.BowThruster)
                    Gizmos.color = Color.yellow;
                else
                    Gizmos.color = Color.cyan;

                Gizmos.DrawSphere(transform.TransformPoint(thruster.position), 0.2f);
                Gizmos.DrawRay(new Ray(thruster.WorldPosition, transform.right));
            }
        }
    }
}
