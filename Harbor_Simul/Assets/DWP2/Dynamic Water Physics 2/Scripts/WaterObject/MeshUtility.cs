using DWP2.Lib;
using DWP2MiConvexHull;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DWP2
{
	public static class MeshUtility
	{
		/// <summary>
		/// Create dummy mesh from original mesh
		/// </summary>
		public static void GenerateDummyMesh(ref Mesh originalMesh, ref Mesh dummyMesh,
			bool simplifyMesh = false, bool convexifyMesh = false, float simplificationRatio = 1f)
		{
			if (simplifyMesh)
			{
				// Decimate original mesh
				dummyMesh = GenerateSimplifiedMesh(ref originalMesh, ref dummyMesh, simplificationRatio);
				
				// Generate convex mesh from pre-simplified dummy mesh
				if (convexifyMesh)
				{
					dummyMesh = GenerateConvexMesh(dummyMesh);
				}

                dummyMesh.name = "DWP_SIM_MESH";
				return;
			}

			// Generate convex mesh directly from original mesh
			if (convexifyMesh)
			{
				dummyMesh = GenerateConvexMesh(originalMesh);
                dummyMesh.name = "DWP_SIM_MESH";
            }
		}
		
		
		/// <summary>
		/// Generate mesh from vertices and triangles.
		/// </summary>
		/// <param name="vertices">Array of vertices.</param>
		/// <param name="triangles">Array of triangles (indices).</param>
		/// <returns></returns>
		public static Mesh GenerateMesh(Vector3[] vertices, int[] triangles)
		{
			Mesh m = new Mesh();
			m.vertices = vertices;
			m.triangles = triangles;
			m.RecalculateBounds();
			m.RecalculateNormals();
            m.name = "DWP_SIM_MESH";
            return m;
		}

		/// <summary>
		/// Reduces poly count of the mesh while trying to preserve features.
		/// </summary>
		/// <param name="om">Mesh to simplify.</param>
		/// <param name="ratio">Percent of the triangles the new mesh will have</param>
		/// <returns></returns>
		private static Mesh GenerateSimplifiedMesh(ref Mesh om, ref Mesh dummyMesh, float ratio)
		{
			MeshDecimate meshDecimate = new MeshDecimate();
			meshDecimate.ratio = ratio;
			meshDecimate.PreCalculate(om);
			meshDecimate.Calculate(om);

			Mesh sm = new Mesh();
			sm.vertices = meshDecimate.finalVertices;
			sm.triangles = meshDecimate.finalTriangles;
			sm.normals = meshDecimate.finalNormals;
            sm.name = "DWP_SIM_MESH";
            return sm;
		}
		
		/// <summary>
	    /// Calculate signed volume of a triangle given by its vertices.
	    /// </summary>
	    public static float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
	    {
	        float v321 = p3.x * p2.y * p1.z;
	        float v231 = p2.x * p3.y * p1.z;
	        float v312 = p3.x * p1.y * p2.z;
	        float v132 = p1.x * p3.y * p2.z;
	        float v213 = p2.x * p1.y * p3.z;
	        float v123 = p1.x * p2.y * p3.z;
	        return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
	    }

	    /// <summary>
	    /// Calculates volume of the given mesh.
	    /// Scale-sensitive
	    /// </summary>
	    public static float VolumeOfMesh(Mesh mesh, Transform transform)
	    {
	        float volume = 0;
	        Vector3[] vertices = mesh.vertices;
	        int[] triangles = mesh.triangles;
	        Matrix4x4 transformMatrix = transform.localToWorldMatrix;
	        for (int i = 0; i < mesh.triangles.Length; i += 3)
	        {
	            Vector3 p1 = transformMatrix.MultiplyPoint(vertices[triangles[i + 0]]);
	            Vector3 p2 = transformMatrix.MultiplyPoint(vertices[triangles[i + 1]]);
	            Vector3 p3 = transformMatrix.MultiplyPoint(vertices[triangles[i + 2]]);
	            volume += SignedVolumeOfTriangle(p1, p2, p3);
	        }
	        return Mathf.Abs(volume);
	    }

	    /// <summary>
	    /// Generates convex mesh.
	    /// </summary>
	    public static Mesh GenerateConvexMesh(Mesh mesh)
	    {
	        IEnumerable<Vector3> stars = mesh.vertices;
	        Mesh m = new Mesh();

	        List<int> triangles = new List<int>();
            List<DWP2MiConvexHull.Vertex> vertices = stars.Select(x => new DWP2MiConvexHull.Vertex(x)).ToList();

            var result = DWP2MiConvexHull.ConvexHull.Create(vertices);
	        m.vertices = result.Points.Select(x => x.ToVec()).ToArray();
	        var xxx = result.Points.ToList();

	        foreach (var face in result.Faces)
	        {
	            triangles.Add(xxx.IndexOf(face.Vertices[0]));
	            triangles.Add(xxx.IndexOf(face.Vertices[1]));
	            triangles.Add(xxx.IndexOf(face.Vertices[2]));
	        }

	        m.triangles = triangles.ToArray();
	        m.RecalculateNormals();
            m.name = "DWP_SIM_MESH";
            return m;
	    }
	}
}

