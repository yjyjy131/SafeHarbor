#if DWP_ENABLE_BURST
using Unity.Burst;
#endif
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace DWP2
{
    /// <summary>
    /// Job for calculating water-object forces.
    /// </summary>
#if DWP_ENABLE_BURST
    [BurstCompile]
#endif
    public struct WaterTriangleJob : IJobParallelFor
    {
        [ReadOnly] public Vector3 Gravity;
        [ReadOnly] public float FluidDensity;
        [ReadOnly] public float DynamicForceFactor;
        [ReadOnly] public float DynamicForcePower;
        [ReadOnly] public float SurfaceDrag;
        [ReadOnly] public float VelocityDotPower;
        
        [ReadOnly] public NativeArray<float> WaterHeights0;
        [ReadOnly] public NativeArray<float> WaterHeights1;
        [ReadOnly] public NativeArray<float> WaterHeights2;
        [ReadOnly] public NativeArray<Vector3> WaterVelocities0;
        [ReadOnly] public NativeArray<Vector3> WaterVelocities1;
        [ReadOnly] public NativeArray<Vector3> WaterVelocities2;

        /// <summary>
        /// 0 - Under water
        /// 1 - At surface
        /// 2 - Above water
        /// 3 - Disabled
        /// 4 - Destroyed
        /// </summary>
        public NativeArray<byte> States;
        public NativeArray<Vector3> ResultForces;
        public NativeArray<Vector3> ResultPoints;

        public NativeArray<float> Areas;
        public NativeArray<Vector3> Normals;
        public NativeArray<Vector3> Velocities;
        
        [ReadOnly] public NativeArray<Vector3> P0S;
        [ReadOnly] public NativeArray<Vector3> P1S;
        [ReadOnly] public NativeArray<Vector3> P2S;
        [ReadOnly] public NativeArray<Vector3> RigidbodyCOMs;
        [ReadOnly] public NativeArray<Vector3> RigidbodyLinearVels;
        [ReadOnly] public NativeArray<Vector3> RigidbodyAngularVels;
        [ReadOnly] public NativeArray<Matrix4x4> LocalToWorldMatrices;

        public NativeArray<Vector3> P00S;
        public NativeArray<Vector3> P01S;
        public NativeArray<Vector3> P02S;
        public NativeArray<Vector3> P10S;
        public NativeArray<Vector3> P11S;
        public NativeArray<Vector3> P12S;

        public NativeArray<float> Distances;

        public void Execute(int i)
        {
            if(States[i] >= 3) return;
            
            Matrix4x4 localToWorldMatrix = LocalToWorldMatrices[i];

            Vector3 P0 = localToWorldMatrix.MultiplyPoint3x4(P0S[i]);
            Vector3 P1 = localToWorldMatrix.MultiplyPoint3x4(P1S[i]);
            Vector3 P2 = localToWorldMatrix.MultiplyPoint3x4(P2S[i]);

            float wY0 = WaterHeights0[i];
            float wY1 = WaterHeights1[i];
            float wY2 = WaterHeights2[i];
            float y0 = P0.y - wY0;
            float y1 = P1.y - wY1;
            float y2 = P2.y - wY2;

            // All vertices are above water
            if (y0 >= 0 && y1 >= 0 && y2 >= 0)
            {
                P00S[i] = Vector3.zero;
                P01S[i] = Vector3.zero;
                P02S[i] = Vector3.zero;

                P10S[i] = Vector3.zero;
                P11S[i] = Vector3.zero;
                P12S[i] = Vector3.zero;
                
                ResultPoints[i] = Vector3.zero;
                ResultForces[i] = Vector3.zero;

                States[i] = 2;

                return;
            }
            
            // All vertices are underwater
            if (y0 <= 0 && y1 <= 0 && y2 <= 0)
            {
                ThreeUnderWater(P0, P1, P2, y0, y1, y2, 0, 1, 2, i);
            }
            // 1 or 2 vertices are below the water
            else
            {
                // v0 > v1
                if (y0 > y1)
                {
                    // v0 > v2
                    if (y0 > y2)
                    {
                        // v1 > v2                  
                        if (y1 > y2)
                        {
                            if (y0 > 0 && y1 < 0 && y2 < 0)
                            {
                                // 0 1 2
                                TwoUnderWater(P0, P1, P2, y0, y1, y2, 0, 1, 2, i);
                            }
                            else if (y0 > 0 && y1 > 0 && y2 < 0)
                            {
                                // 0 1 2
                                OneUnderWater(P0, P1, P2, y0, y1, y2, 0, 1, 2, i);
                            }
                        }
                        // v2 > v1
                        else
                        {
                            if (y0 > 0 && y2 < 0 && y1 < 0)
                            {
                                // 0 2 1
                                TwoUnderWater(P0, P2, P1, y0, y2, y1, 0, 2, 1, i);
                            }
                            else if (y0 > 0 && y2 > 0 && y1 < 0)
                            {
                                // 0 2 1
                                OneUnderWater(P0, P2, P1, y0, y2, y1, 0, 2, 1, i);
                            }
                        }
                    }
                    // v2 > v0
                    else
                    {
                        if (y2 > 0 && y0 < 0 && y1 < 0)
                        {
                            // 2 0 1
                            TwoUnderWater(P2, P0, P1, y2, y0, y1, 2, 0, 1, i);
                        }
                        else if (y2 > 0 && y0 > 0 && y1 < 0)
                        {
                            // 2 0 1
                            OneUnderWater(P2, P0, P1, y2, y0, y1, 2, 0, 1, i);
                        }
                    }
                }
                // v0 < v1
                else
                {
                    // v0 < v2
                    if (y0 < y2)
                    {
                        // v1 < v2
                        if (y1 < y2)
                        {
                            if (y2 > 0 && y1 < 0 && y0 < 0)
                            {
                                // 2 1 0
                                TwoUnderWater(P2, P1, P0, y2, y1, y0, 2, 1, 0, i);
                            }
                            else if (y2 > 0 && y1 > 0 && y0 < 0)
                            {
                                // 2 1 0
                                OneUnderWater(P2, P1, P0, y2, y1, y0, 2, 1, 0, i);
                            }
                        }
                        // v2 < v1
                        else
                        {
                            if (y1 > 0 && y2 < 0 && y0 < 0)
                            {
                                // 1 2 0
                                TwoUnderWater(P1, P2, P0, y1, y2, y0, 1, 2, 0, i);
                            }
                            else if (y1 > 0 && y2 > 0 && y0 < 0)
                            {
                                // 1 2 0
                                OneUnderWater(P1, P2, P0, y1, y2, y0, 1, 2, 0, i);
                            }
                        }
                    }
                    // v2 < v0
                    else
                    {
                        if (y1 > 0 && y0 < 0 && y2 < 0)
                        {
                            // 1 0 2
                            TwoUnderWater(P1, P0, P2, y1, y0, y2, 1, 0, 2, i);
                        }
                        else if (y1 > 0 && y0 > 0 && y2 < 0)
                        {
                            // 1 0 2
                            OneUnderWater(P1, P0, P2, y1, y0, y2, 1, 0, 2, i);
                        }
                    }
                }
            }
        }

        private Vector3 CalculateForces(Vector3 p0, Vector3 p1, Vector3 p2, float dist0, float dist1, float dist2, int i, 
            out Vector3 center, out float area)
        {
            center = (p0 + p1 + p2) / 3f;
            Vector3 u = p1 - p0;
            Vector3 v = p2 - p0;
            Vector3 crossUV = Vector3.Cross(u, v);
            float crossMagnitude = Mathf.Sqrt(crossUV.x * crossUV.x + crossUV.y * crossUV.y + crossUV.z * crossUV.z);
            Vector3 normal = crossMagnitude < 0.00001f ? Vector3.zero : crossUV / crossMagnitude;
            Normals[i] = normal;

            Vector3 p = center - RigidbodyCOMs[i];
            Vector3 velocity = Vector3.Cross(RigidbodyAngularVels[i], p) + RigidbodyLinearVels[i];

            float velocityMagnitude = Vector3.Magnitude(velocity);
            Vector3 velocityNormalized = Vector3.Normalize(velocity);

            area = crossMagnitude;
            float distanceToSurface;
            if (area > 0.000001f)
            {
                Vector3 f0 = p0 - center;
                Vector3 f1 = p1 - center;
                Vector3 f2 = p2 - center;
                float w0 = Vector3.Cross(f1, f2).magnitude / area;
                float w1 = Vector3.Cross(f2, f0).magnitude / area;
                float w2 = 1f - (w0 + w1);
                area *= 0.5f;
                distanceToSurface = w0 * dist0 + w1 * dist1 + w2 * dist2;
                velocity += w0 * WaterVelocities0[i] + w1 * WaterVelocities1[i] + w2 * WaterVelocities2[i];
            }
            else
            {
                States[i] = 2;
                return Vector3.zero;
            }
            
            Velocities[i] = velocity;
            Areas[i] = area;
            
            distanceToSurface = distanceToSurface < 0 ? 0 : distanceToSurface;
            Distances[i] = distanceToSurface;

            float densityArea = FluidDensity * area;

            // Buoyant force
            Vector3 buoyantForce = distanceToSurface * Vector3.Dot(Vector3.up, normal) * densityArea * Gravity;

            // Dynamic force
            float dot = Vector3.Dot(normal, velocityNormalized);
            if (dot != 0)
            {
                dot = Mathf.Sign(dot) * Mathf.Pow(Mathf.Abs(dot), VelocityDotPower);
            }


            Vector3 dynamicForce;
            if (DynamicForcePower != 1f)
            {
                dynamicForce = -dot * Mathf.Pow(velocityMagnitude, DynamicForcePower) * densityArea * DynamicForceFactor * normal;
            }
            else
            {
                dynamicForce = -dot * velocityMagnitude * densityArea * DynamicForceFactor * normal;
            }
            


            if (SurfaceDrag > 0.0000001f)
            {
                float absDot = dot < 0 ? -dot : dot;
                dynamicForce += (1f - absDot) * SurfaceDrag * densityArea * -velocity;
            }

            return buoyantForce + dynamicForce;
        }
        
        private void ThreeUnderWater(Vector3 p0, Vector3 p1, Vector3 p2, 
                                    float dist0, float dist1, float dist2, 
                                    int i0, int i1, int i2, int index)
        {
            States[index] = 0;
            
            P00S[index] = p0;
            P01S[index] = p1;
            P02S[index] = p2;
            
            P10S[index] = Vector3.zero;
            P11S[index] = Vector3.zero;
            P12S[index] = Vector3.zero;

            Vector3 center;
            float area;
            Vector3 resultForce = CalculateForces(p0, p1, p2, -dist0, -dist1, -dist2, index, out center, out area);
            ResultForces[index] = resultForce;
            ResultPoints[index] = center;
        }

        private void TwoUnderWater(Vector3 p0, Vector3 p1, Vector3 p2, 
                                    float dist0, float dist1, float dist2, 
                                    int i0, int i1, int i2, int index)
        {
            States[index] = 1;
            
            Vector3 H, M, L, IM, IL;
            float hH, hM, hL;
            int mIndex;

            // H is always at position 0
            H = p0;

            // Find the index of M
            mIndex = i0 - 1;
            if (mIndex < 0)
            {
                mIndex = 2;
            }

            // Heights to the water
            hH = dist0;

            if (i1 == mIndex)
            {
                M = p1;
                L = p2;

                hM = dist1;
                hL = dist2;
            }
            else
            {
                M = p2;
                L = p1;

                hM = dist2;
                hL = dist1;
            }

            IM = -hM / (hH - hM) * (H - M) + M;
            IL = -hL / (hH - hL) * (H - L) + L;
            
            P00S[index] = M;
            P01S[index] = IM;
            P02S[index] = IL;

            P10S[index] = M;
            P11S[index] = IL;
            P12S[index] = L;

            // Generate tris
            Vector3 center0, center1;
            float area0, area1;
            Vector3 force0 = CalculateForces(M, IM, IL, -hM, 0, 0, index, out center0, out area0);
            Vector3 force1 = CalculateForces(M, IL, L, -hM, 0, -hL, index, out center1, out area1);

            float weight0 = area0 / (area0 + area1);
            ResultForces[index] = force0 + force1;
            ResultPoints[index] = center0 * weight0 + center1 * (1f - weight0);
        }

        private void OneUnderWater(Vector3 p0, Vector3 p1, Vector3 p2, 
            float dist0, float dist1, float dist2, 
            int i0, int i1, int i2, int index)
        {
            States[index] = 1;
            
            Vector3 H, M, L, JM, JH;
            float hH, hM, hL;

            L = p2;

            // Find the index of H
            int hIndex = i2 + 1;
            if (hIndex > 2)
            {
                hIndex = 0;
            }

            // Get heights to water
            hL = dist2;

            if (i1 == hIndex)
            {
                H = p1;
                M = p0;
                
                hH = dist1;
                hM = dist0;
            }
            else
            {
                H = p0;
                M = p1;

                hH = dist0;
                hM = dist1;
            }

            JM = -hL / (hM - hL) * (M - L) + L;
            JH = -hL / (hH - hL) * (H - L) + L;

            P00S[index] = L;
            P01S[index] = JH;
            P02S[index] = JM;
            
            P10S[index] = Vector3.zero;
            P11S[index] = Vector3.zero;
            P12S[index] = Vector3.zero;
            
            // Generate tris
            Vector3 center;
            float area;
            Vector3 resultForce = CalculateForces(L, JH, JM, -hL, 0, 0, index, out center, out area);
            ResultForces[index] = resultForce;
            ResultPoints[index] = center;
        }
    }
}
