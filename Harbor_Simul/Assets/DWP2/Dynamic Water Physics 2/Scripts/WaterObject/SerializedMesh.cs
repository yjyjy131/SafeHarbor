using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DWP2
{
    [Serializable]
    public struct SerializedMesh
    {
        [SerializeField] public Vector3[] vertices;
        [SerializeField] public int[] triangles;
        
        public void Serialize(Mesh mesh)
        {
            vertices = mesh.vertices;
            triangles = mesh.triangles;
        }

        public Mesh Deserialize()
        {
            if (vertices != null && triangles != null)
            {
                Mesh m = MeshUtility.GenerateMesh(vertices, triangles);
                m.name = "DWP_SIM_MESH";
                return m;
            }

            return null;
        }
    }
}

