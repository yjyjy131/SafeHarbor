using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

namespace DWP2.ShipController
{
    /// <summary>
    /// Engine object. Contains all the parameters related to ships's propulsion systems.
    /// </summary>
    [System.Serializable]
    public class Engine
    {
        public string name = "Engine";

        public enum State { On, Off, Starting, Stopping }

        [SerializeField] private bool _isOn = false;
        /// <summary>
        /// Is the engine turned on? To turn the engine on/off use engine.Start() and engine.Stop()
        /// </summary>
        public bool IsOn
        {
            get { return _isOn; }
        }

        /// <summary>
        /// To which throttle input this engine/propeller will be mapped?
        /// </summary>
        [Tooltip("To which throttle input this engine/propeller will be mapped?")]
        public AdvancedShipController.InputMapping inputMapping = AdvancedShipController.InputMapping.Throttle;

        [Header("Engine")]

        /// <summary>
        /// Min RPM of the engine.
        /// </summary>
        [Tooltip("Min RPM of the engine.")]
        [SerializeField] private float _minRPM = 800;
        public float MinRPM
        {
            get => _minRPM;
            set => _minRPM = Mathf.Clamp(value, 10f, _maxRPM);
        }

        /// <summary>
        /// Max RPM of the engine.
        /// </summary>
        [Tooltip("Max RPM of the engine.")]
        [SerializeField] private float _maxRPM = 6000;
        public float MaxRPM
        {
            get => _maxRPM;
            set => _maxRPM = Mathf.Clamp(value, _minRPM, Mathf.Infinity);
        }

        /// <summary>
        /// Thrust at max RPM.
        /// </summary>
        [Tooltip("Thrust at max RPM.")]
        [SerializeField] private float _maxThrust = 5000;
        public float MaxThrust
        {
            get => _maxThrust;
            set => _maxThrust = value;
        }

        /// <summary>
        /// Time needed to spin up the engines up to max RPM
        /// </summary>
        [Tooltip("Time needed to spin up the engines up to max RPM")]
        [SerializeField] private float _spinUpTime = 2f;
        public float SpinUpTime
        {
            get => _spinUpTime;
            set => _spinUpTime = Mathf.Clamp(value, 0f, Mathf.Infinity);
        }

        [Tooltip("Used to determine starting sound pitch. Engine RPM when turning over.")]
        public float startingRpm = 300f;

        [Tooltip("How long will the engine starting take?")]
        public float startDuration = 1.3f;

        [Tooltip("How long will the engine stopping take?")]
        public float stopDuration = 0.8f;

        [Header("Propeller")]

        /// <summary>
        /// Local position at
        /// </summary>
        [Tooltip("Local position at which the force will be applied.")]
        public Vector3 thrustPosition;

        /// <summary>
        /// Direction in which the force will be applied.
        /// </summary>
        [Tooltip("Local direction in which the force will be applied.")]
        public Vector3 thrustDirection = Vector3.forward;

        /// <summary>
        /// Should thrust be applied when above water?
        /// </summary>
        [Tooltip("Should thrust be applied when above water?")]
        public bool applyThrustWhenAboveWater = false;

        /// <summary>
        /// Amount of thrust that will be applied if ship is reversing
        /// </summary>
        [Tooltip("Amount of thrust that will be applied if ship is reversing.")]
        public float reverseThrustCoefficient = 0.3f;

        /// <summary>
        /// Ship peed at which propeller will reach it's maximum rotational speed.
        /// </summary>
        [Tooltip("Ship speed at which propeller will reach it's maximum rotational speed.")]
        public float maxSpeed = 20f;

        /// <summary>
        /// Thrust curve of the propeller. X axis is speed in m/s and y axis is efficiency.
        /// </summary>
        [Tooltip("Thrust curve of the propeller. X axis is speed in m/s and y axis is efficiency.")]
        public AnimationCurve thrustCurve = new AnimationCurve(new Keyframe[3] {
                    new Keyframe(0f, 1f),
                    new Keyframe(0.5f, 0.95f),
                    new Keyframe(1f, 0f)
                });

        /// <summary>
        /// Optional. Only use if you vessel has propeller mounted to the rudder (as in outboard engines). Propuslion force direction will be rotated with rudder if assigned.
        /// </summary>
        [Tooltip("Optional. Only use if you vessel has propeller mounted to the rudder (as in outboard engines). Propuslion force direction will be rotated with rudder if assigned.")]
        public Transform rudderTransform;

        [Header("Animation")]

        /// <summary>
        /// Optional. Propeller transform. Visual rotation only, does not affect physics.
        /// </summary>
        [Tooltip("Optional. Propeller transform. Visual rotation only, does not affect physics.")]
        public Transform propellerTransform;

        /// <summary>
        /// Engine RPM will be multiplied by this value to get rotation speed of the propeller. Animation only.
        /// </summary>
        [Tooltip("Engine RPM will be multiplied by this value to get rotation speed of the propeller. Animation only.")]
        public float propellerRpmRatio = 0.1f;

        public enum RotationDirection { Left, Right }

        [Tooltip("Direction of propeller rotation. Animation only.")]
        public RotationDirection rotationDirection = RotationDirection.Right;

        [Header("Sound")]

        [Tooltip("Engine running audio source.")]
        public AudioSource runningSource;

        [Tooltip("[Optional] Sound of engine starting. If left empty fade-in will be used.")]
        public AudioSource startingSource;

        [Tooltip("[Optional] Sound of engine stopping. If left empty cut-out will be used.")]
        public AudioSource stoppingSource;

        /// <summary>
        /// Base volume of the engine
        /// </summary>
        [Tooltip("Base (idle) volume of the engine.")]
        [Range(0, 2)]
        public float volume = 0.2f;

        /// <summary>
        /// Idle pitch of the engine
        /// </summary>
        [Tooltip("Base (idle) pitch of the engine.")]
        [Range(0, 2)]
        public float pitch = 0.5f;

        /// <summary>
        /// Volume range of the engine.
        /// </summary>
        [Tooltip("Volume range of the engine.")]
        [Range(0, 2)]
        public float volumeRange = 0.8f;

        /// <summary>
        /// Pitch range of the engine.
        /// </summary>
        [Tooltip("Pitch range of the engine.")]
        [Range(0, 2)]
        public float pitchRange = 1f;

        private float _rpm;
        /// <summary>
        /// Current RPM of the engine
        /// </summary>
        public float RPM => Mathf.Clamp(_rpm, _minRPM, _maxRPM);

        /// <summary>
        /// Percentage of rpm range engine is currently on
        /// </summary>
        public float RpmPercent => Mathf.Clamp01((RPM - _minRPM) / _maxRPM);

        /// <summary>
        /// Current thrust generated by the engine / propeller
        /// </summary>
        private float _thrust;
        public float Thrust => _thrust;

        private float spinVelocity;
        private AdvancedShipController sc;
        private State _engineState;
        private float startTime;
        private float stopTime;
        private bool wasOn;

        public void StartEngine()
        {
            _isOn = true;
        }

        public void StopEngine()
        {
            _isOn = false;
            StopAll();
        }

        /// <summary>
        /// True if engine's thrust postion is under water.
        /// </summary>
        public bool Submerged
        {
            get { return WaterObjectManager.Instance.PointInWater(ThrustPosition); }
        }

        public float Input
        {
            get
            {
                float input = 0;
                if (inputMapping == AdvancedShipController.InputMapping.LeftThrottle)
                {
                    if (sc.input.Throttle == 0)
                    {
                        input = sc.input.LeftThrottle;
                    }
                    else
                    {
                        input = sc.input.Throttle;
                    }
                }
                else if (inputMapping == AdvancedShipController.InputMapping.RightThrottle)
                {
                    if (sc.input.Throttle == 0)
                    {
                        input = sc.input.RightThrottle;
                    }
                    else
                    {
                        input = sc.input.Throttle;
                    }
                }
                else
                {
                    input = sc.input.Throttle;
                }
                return input;
            }
        }

        public Vector3 ThrustPosition
        {
            get
            {
                return sc.transform.TransformPoint(thrustPosition);
            }
        }

        public Vector3 ThrustDirection
        {
            get
            {
                if (rudderTransform == null)
                {
                    return sc.transform.TransformDirection(thrustDirection).normalized;
                }

                return rudderTransform.TransformDirection(thrustDirection).normalized;
            }
        }

        public void Initialize(AdvancedShipController sc)
        {
            this.sc = sc;

            if (_isOn)
            {
                _engineState = State.On;
                wasOn = true;
            }
            else
            {
                _engineState = State.Off;
                wasOn = false;
            }

            // Init sound
            SoundInit();
        }

        public void Update()
        {
            if(sc.input.EngineStartStop)
            {
                if (_isOn) StopEngine();
                else StartEngine();
            }

            // Check engine state
            if (_engineState == State.Starting && !IsOn)
            {
                _engineState = State.Off;
            }
            else if (IsOn && !wasOn)
            {
                _engineState = State.Starting;
                startTime = Time.realtimeSinceStartup;
            }
            else if (!IsOn && wasOn)
            {
                _engineState = State.Stopping;
                stopTime = Time.realtimeSinceStartup;
            }

            // Run timer starting or stopping
            if (_engineState == State.Starting)
            {
                if (Time.realtimeSinceStartup > startTime + startDuration)
                {
                    _engineState = State.On;
                }
            }
            else if (_engineState == State.Stopping)
            {
                if (Time.realtimeSinceStartup > stopTime + startDuration)
                {
                    _engineState = State.Off;
                }
            }

            // RPM
            float newRpm = 0f;
            switch (_engineState)
            {
                case State.On:
                    newRpm = (0.7f + 0.3f * (sc.Speed / maxSpeed)) * Mathf.Abs(Input) * _maxRPM;
                    newRpm = Mathf.Clamp(newRpm, _minRPM, _maxRPM);
                    if (!Submerged) newRpm = _maxRPM;
                    break;
                case State.Off:
                    newRpm = 0;
                    break;
                case State.Starting:
                    newRpm = startingRpm;
                    break;
                case State.Stopping:
                    newRpm = 0f;
                    break;
            }          
            _rpm = Mathf.SmoothDamp(_rpm, newRpm, ref spinVelocity, _spinUpTime);
            
            if(_engineState == State.On)
            {
                // Check if propeller under water
                bool applyForce = Submerged || applyThrustWhenAboveWater;

                // Check if thrust can be applied
                _thrust = 0;
                if (applyForce && _maxRPM != 0 && maxSpeed != 0 && RPM > _minRPM + 1f && Input != 0)
                {
                    _thrust = Mathf.Sign(Input) * (_rpm / _maxRPM) * thrustCurve.Evaluate(Mathf.Abs(sc.Speed) / maxSpeed) * _maxThrust;
                    _thrust = Mathf.Sign(Input) == 1 ? _thrust : _thrust * reverseThrustCoefficient;
                    sc.ShipRigidbody.AddForceAtPosition(_thrust * ThrustDirection, ThrustPosition);
                }

                if (propellerTransform != null)
                {
                    float zRotation = _rpm * propellerRpmRatio * 6.0012f * Time.fixedDeltaTime;
                    if (rotationDirection == RotationDirection.Right) zRotation = -zRotation;
                    propellerTransform.RotateAround(propellerTransform.position, propellerTransform.forward, zRotation);
                }
            }

            SoundUpdate();

            wasOn = _isOn;
        }

        private void SoundInit()
        {
            if(runningSource != null)
            {
                runningSource.loop = true;
                runningSource.playOnAwake = false;
            }

            if(startingSource != null)
            {
                startingSource.loop = false;
                startingSource.playOnAwake = false;
            }

            if(stoppingSource != null)
            {
                stoppingSource.loop = false;
                stoppingSource.playOnAwake = false;
            }
        }

        private void SoundUpdate()
        {
            if(runningSource == null)
            {
                Debug.LogWarning($"No AudioSource assigned to Running Source field of object {sc.name}");
                return;
            }


            // Pitch
            runningSource.pitch = pitch + RpmPercent * pitchRange;

            // Volume
            runningSource.volume = volume + RpmPercent * volumeRange;

            if (_engineState == State.On)
            {
                PlayRunning();
            }
            else if(_engineState == State.Off)
            {
                StopAll();
            }
            else if(_engineState == State.Starting)
            {
                PlayStarting();
            }
            else if(_engineState == State.Stopping)
            {
                PlayStopping();
            }
        }

        private void PlayStarting()
        {
            if (startingSource == null)
            {
                if (!runningSource.isPlaying) runningSource.Play();
                runningSource.volume = Mathf.Lerp(0f, volume, (Time.realtimeSinceStartup - startTime) / startDuration);
            }

            if (stoppingSource != null) stoppingSource.Stop();
            if (startingSource != null && runningSource != null) runningSource.Stop();
            if (startingSource != null) startingSource.Play();
        }

        private void PlayRunning()
        {
            //if (startingSource != null) startingSource.Stop();
            if (stoppingSource != null) stoppingSource.Stop();
            if (runningSource != null) if (!runningSource.isPlaying) runningSource.Play();
        }

        private void PlayStopping()
        {
            if (startingSource != null) startingSource.Stop();
            if (runningSource != null) runningSource.Stop();
            if (stoppingSource != null) stoppingSource.Play();
        }

        private void StopAll()
        {
            if (startingSource != null) startingSource.Stop();
            if (runningSource != null) runningSource.Stop();
            if (stoppingSource != null) stoppingSource.Stop();
        }
    }
}