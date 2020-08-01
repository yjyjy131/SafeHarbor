using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DWP2
{
    public struct TriangleData
    {
        public int index;
        public Vector3 p0;
        public Vector3 p1;
        public Vector3 p2;
        private WaterObject _waterObject;
        private Transform _transform;

        public Vector3 P0
        {
            get { return _transform.TransformPoint(p0); }
        }

        public Vector3 P1
        {
            get { return _transform.TransformPoint(p1); }
        }

        public Vector3 P2
        {
            get { return _transform.TransformPoint(p2); }
        }

        public Transform Transform => _transform;
        
        public WaterObject WaterObject
        {
            get
            {
                return _waterObject;
            }
            set 
            { 
                _waterObject = value;
                _transform = _waterObject.transform;
            }
        }

        public TriangleData(int index, Vector3 p0, Vector3 p1, Vector3 p2, WaterObject waterObject)
        {
            this.index = index;
            this._waterObject = waterObject;
            _transform = waterObject.transform;

            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
        }
    }
}
