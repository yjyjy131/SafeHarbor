using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DWP2
{
    /// <summary>
    /// Approximates the behavior of a real anchor by keeping the object near the anchored position, but
    /// allowing for some movement.
    /// </summary>
    public class Anchor : MonoBehaviour
    {
        /// <summary>
        /// Should the anchor be dropped at start?
        /// </summary>
        [Tooltip("Should the anchor be dropped at start?")]
        public bool dropOnStart = true;
        
        /// <summary>
        /// Coefficient by which the force will be multiplied when the object starts pulling on the anchor.
        /// </summary>
        [Tooltip("Coefficient by which the force will be multiplied when the object starts pulling on the anchor.")]
        public float forceCoefficient = 10f;
        
        /// <summary>
        /// Radius around anchor in which the chain/rope is slack and in which no force will be applied.
        /// </summary>
        [Tooltip("Radius around anchor in which the chain/rope is slack and in which no force will be applied.")]
        public float zeroForceRadius = 2f;
        
        /// <summary>
        /// Maximum force that can be applied to anchor before it starts to drag.
        /// </summary>
        [Tooltip("Maximum force that can be applied to anchor before it starts to drag.")]
        public float dragForce = 500f;

        /// <summary>
        /// Point in coordinates local to the object this script is attached to.
        /// </summary>
        [Tooltip("Point in coordinates local to the object this script is attached to.")]
        public Vector3 localAnchorPoint = Vector3.zero;

        private Rigidbody _parentRigidbody;
        /// <summary>
        /// Rigidbody to which the force will be applied.
        /// </summary>
        public Rigidbody ParentRigidbody => _parentRigidbody;

        public Vector3 AnchorPoint => transform.TransformPoint(localAnchorPoint);
        
        private Vector3 _anchorPosition;
        
        /// <summary>
        /// Position of the anchor.
        /// </summary>
        public Vector3 AnchorPosition
        {
            get => _anchorPosition;
            set => _anchorPosition = value;
        }

        private bool _dropped;
        
        /// <summary>
        /// Has the anchor been dropped?
        /// </summary>
        public bool Dropped => _dropped;

        private bool _isDragging;
        /// <summary>
        /// Is the anchor dragging on the floor?
        /// </summary>
        public bool IsDragging => _isDragging;

        private Vector3 _force;
        private Vector3 _distance;
        private Vector3 _prevDistance;

        /// <summary>
        /// Drops the anchor. Opposite of Weigh()
        /// </summary>
        public void Drop()
        {
            if (_dropped) return;
            _dropped = true;
            _anchorPosition = AnchorPoint;
        }

        /// <summary>
        /// Weighs (retracts) the anchor.
        /// </summary>
        public void Weigh()
        {
            if (!_dropped) return;
            _dropped = false;
        }

        private void Start()
        {
            _parentRigidbody = GetComponentInParent<Rigidbody>();
            if (_parentRigidbody == null)
            {
                Debug.LogError($"No rigidbody found on object {name} or its parents. Anchor script needs rigidbody to function.");
            }

            if (dropOnStart) Drop();
        }

        private void FixedUpdate()
        {
            if (!_dropped) return;

            _prevDistance = _distance;
            _distance = AnchorPoint - AnchorPosition;
            _distance.y = 0;
            float distMag = _distance.magnitude - zeroForceRadius;
            if (distMag < 0) return;
            _force = distMag * distMag * 100f * forceCoefficient * -_distance.normalized;

            _isDragging = false;
            if (_force.magnitude > dragForce)
            {
                _isDragging = true;
                _force = _force.normalized * dragForce;
                _anchorPosition += (_distance - _prevDistance);
            }

            _parentRigidbody.AddForceAtPosition(_force, AnchorPoint);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(AnchorPoint, 0.2f);

            if(Dropped)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(AnchorPoint, AnchorPosition);
            }
        }
    }
}

