using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;

namespace DWP2.WaterEffects
{
    /// <summary>
    /// Class for generating water particles based on simulation data.
    /// </summary>
    [RequireComponent(typeof(WaterObject))]
    public class WaterParticleSystem : MonoBehaviour {

        /// <summary>
        /// Should the particle system emit?
        /// </summary>
        [Tooltip("Should the particle system emit?")]
        public bool emit = true;

        /// <summary>
        /// Render queue of the particle material.
        /// </summary>
        [Tooltip("Render queue of the water particle material.")]
        public int renderQueue = 2700;
        
        /// <summary>
        /// Elevation above water at which the particles will spawn. Used to avoid clipping.
        /// </summary>
        [Tooltip("Elevation above water at which the particles will spawn. Used to avoid clipping.")]
        [Range(0f, 0.1f)] public float surfaceElevation = 0.03f;

        /// <summary>
        /// Initial size of the particle.
        /// </summary>
        [Tooltip("Initial size of the particle.")]
        [Range(0f, 64f)] public float startSize = 4f;

        /// <summary>
        /// Velocity object has to have to emit particles.
        /// </summary>
        [Tooltip("Velocity object has to have to emit particles.")]
        [Range(0.1f, 5f)] public float sleepTresholdVelocity = 1.5f;

        /// <summary>
        /// Determines how much velocity of the object will affect initial particle speed.
        /// </summary>
        [Tooltip("Determines how much velocity of the object will affect initial particle speed.")]
        [Range(0f, 5f)] public float initialVelocityModifier = 0.01f;

        /// <summary>
        /// Limit initial alpha to this value.
        /// </summary>
        [Tooltip("Limit initial alpha to this value.")]
        [Range(0f, 1f)] public float maxInitialAlpha = 0.15f;
        
        /// <summary>
        /// Multiplies initial alpha by this value. Alpha cannot be higher than maxInitialAlpha.
        /// </summary>
        [Tooltip("Multiplies initial alpha by this value. Alpha cannot be higher than maxInitialAlpha.")]
        [Range(0f, 10f)] public float initialAlphaModifier = 0.4f;
        
        /// <summary>
        /// How many particles should be emitted each 'emitTimeInterval' seconds.
        /// </summary>
        [Tooltip("How many particles should be emitted each 'emitTimeInterval' seconds.")]
        [Range(0f, 20f)] public int emitPerCycle = 6;
        
        /// <summary>
        /// Determines how often the particles will be emitted.
        /// </summary>
        [Tooltip("Determines how often the particles will be emitted.")]
        [Range(0f, 0.1f)] public float emitTimeInterval = 0.04f;

        /// <summary>
        /// Script will try to predict where the object will be in the next n frames.
        /// </summary>
        [Tooltip("Script will try to predict where the object will be in the next n frames.")]
        public int positionExtrapolationFrames = 4;
        
        private float _timeElapsed;
        private WaterObject _waterObject;
        private ParticleSystem _particleSystem;
        private int[] _waterlineIndices;
        private int _dataLength;
        private int _dataStart;
        private int _dataEnd;
        private float _volumeOfMesh;
        private ParticleSystem.NoiseModule _noiseModule;
        private bool _initialized;

        void Start()
        {
            _waterObject = GetComponent<WaterObject>();
            if (_waterObject == null)
            {
                Debug.LogError($"{name}: WaterParticleSystem requires WaterObject to function.");
                return;
            }
            
            _particleSystem = GetComponent<ParticleSystem>();
            _particleSystem.GetComponent<Renderer>().material.renderQueue = renderQueue;
            _noiseModule = _particleSystem.noise;

            Synchronize();

            _initialized = true;
            
            WaterObjectManager.Instance.OnSynchronize.AddListener(Synchronize);
        }

        void LateUpdate()
        {
            if (!_initialized || !_waterObject.Initialized) return;
            if (!emit) return;

            if(_waterObject.TargetRigidbody.velocity.magnitude > sleepTresholdVelocity 
               || _waterObject.TargetRigidbody.angularVelocity.magnitude * 0.2f > sleepTresholdVelocity)
            {
                EmitNew();
            }

            _timeElapsed += Time.deltaTime;
        }

        private void OnDestroy()
        {
            if(!Application.isPlaying)
            {
                DestroyImmediate(_particleSystem);
            }
        }

        private void Synchronize()
        {
            _dataLength = _waterObject.DataLength;
            _dataStart = _waterObject.DataStart;
            _dataEnd = _waterObject.DataStart + _waterObject.DataLength;

            if (_dataLength > 0)
            {
                _waterlineIndices = new int[_dataLength];
            }
        }

        private void EmitNew()
        {
            WaterObjectManager wom = WaterObjectManager.Instance;

            if (emit && _timeElapsed >= emitTimeInterval && _dataLength > 0f)
            {
                _timeElapsed = 0;

                int emitted = 0;

                // Emit allowed number of particles
                float elevation = 0;
                if (wom.waterDataProvider != null && wom.waterDataProvider.waterObject != null)
                {
                    elevation = wom.waterDataProvider.waterObject.transform.position.y;
                }

                int waterlineCount = 0;
                for (int i = _dataStart; i < _dataEnd; i++)
                {
                    if (wom.States[i] != 1) continue;

                    _waterlineIndices[waterlineCount] = i;
                    waterlineCount++;
                }
                
                if(waterlineCount == 0) return;

                float noise = startSize > 1f ? Mathf.Sqrt(startSize) * 0.1f : startSize * 0.1f;
                _noiseModule.strengthX = noise;
                _noiseModule.strengthY = 0f;
                _noiseModule.strengthZ = noise;

                while (emitted < emitPerCycle)
                {
                    int i = Random.Range(0, waterlineCount);
                    int womIndex = _waterlineIndices[i];
                    
                    EmitParticle(wom.P02S[womIndex], wom.P01S[womIndex], elevation, wom.Velocities[womIndex], wom.Normals[womIndex], wom.Forces[womIndex], wom.Areas[womIndex]);
                    
                    emitted++;
                }
            }
        }

        /// <summary>
        /// Emit a single particle
        /// </summary>
        /// <param name="p0">First point of water line</param>
        /// <param name="p1">Second point of water line</param>
        /// <param name="elevation">Water elevation</param>
        /// <param name="velocity">Triangle velocity</param>
        /// <param name="normal">Triangle normal</param>
        /// <param name="force">Triangle force</param>
        /// <param name="area">Triangle area</param>
        private void EmitParticle(Vector3 p0, Vector3 p1, float elevation, Vector3 velocity, Vector3 normal, Vector3 force, float area)
        {
            if (area < 0.0001f) return;
            
            // Start velocity
            Vector3 startVelocity = normal * velocity.magnitude;
            startVelocity.y = 0f;
            startVelocity *= initialVelocityModifier;
            
            // Start position
            Vector3 emissionPoint = (p0 + p1) / 2f;
            emissionPoint += Time.deltaTime * positionExtrapolationFrames * velocity;
            emissionPoint.y = elevation + surfaceElevation;
            
            float normalizedForce = force.magnitude / area;
            float startAlpha = Mathf.Clamp(normalizedForce * 0.00005f * initialAlphaModifier, 0f, maxInitialAlpha);
            Color startColor = new Color(1f, 1f, 1f, startAlpha);
            float size = startSize;

            if (startAlpha < 0.001f) return;

            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams
            {
                startColor = startColor,
                position = emissionPoint,
                velocity = startVelocity,
                startSize = size
            };
            _particleSystem.Emit(emitParams, 1);
            _particleSystem.Play();
        }
    }
}
