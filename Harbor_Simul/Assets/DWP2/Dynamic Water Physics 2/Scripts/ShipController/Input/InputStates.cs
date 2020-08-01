using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace DWP2.ShipController
{
    [System.Serializable]
    public class InputStates
    {
        private InputBindings inputBindings;

        private float bowThruster;
        public float BowThruster => bowThruster;

        private float sternThruster;
        public float SternThruster => sternThruster;

        private float rudder;
        public float Rudder => rudder;

        private float leftThrottle;
        public float LeftThrottle => leftThrottle;

        private float rightThrottle;
        public float RightThrottle => rightThrottle;

        private float throttle;
        public float Throttle => throttle;

        private bool engineStartStop;
        public bool EngineStartStop
        {
            get { return engineStartStop; }
            set { engineStartStop = value; }
        }

        private bool anchorDropWeigh;
        public bool AnchorDropWeigh
        {
            get { return anchorDropWeigh; }
            set { anchorDropWeigh = value; }
        }

        [Header("GUI (optional)")]
        public Slider throttleSlider;
        public Slider leftThrottleSlider;
        public Slider rightThrottleSlider;
        public Slider rudderSlider;
        public Slider bowThrusterSlider;
        public Slider sternThrusterSlider;
        public Button engineStartStopButton;
        public Button anchorDropWeighButton;

        public void Initialize(AdvancedShipController sc)
        {
            inputBindings = new InputBindings();
            inputBindings.Initialize();

            // Register GUI element listeners
            if (throttleSlider != null) throttleSlider.onValueChanged.AddListener(delegate { SetThrottle(); });
            if (leftThrottleSlider != null) leftThrottleSlider.onValueChanged.AddListener(delegate { SetLeftThrottle(); });
            if (rightThrottleSlider != null) rightThrottleSlider.onValueChanged.AddListener(delegate { SetRightThrottle(); });
            if (rudderSlider != null) rudderSlider.onValueChanged.AddListener(delegate { SetRudder(); });
            if (bowThrusterSlider != null) bowThrusterSlider.onValueChanged.AddListener(delegate { SetBowThruster(); });
            if (sternThrusterSlider != null) sternThrusterSlider.onValueChanged.AddListener(delegate { SetSternThruster(); });

            if (engineStartStopButton != null) engineStartStopButton.onClick.AddListener(delegate { engineStartStop = !engineStartStop; });
            if (anchorDropWeighButton != null) anchorDropWeighButton.onClick.AddListener(delegate { anchorDropWeigh = !anchorDropWeigh; });
        }

        public void Update()
        {
            SetAxis(ref throttle, inputBindings.Throttle);
            SetAxis(ref leftThrottle, inputBindings.LeftThrottle);
            SetAxis(ref rightThrottle, inputBindings.RightThrottle);
            SetAxis(ref bowThruster, inputBindings.BowThruster);
            SetAxis(ref sternThruster, inputBindings.SternThruster);
            SetAxis(ref rudder, inputBindings.Rudder);

            SetButtonDown(ref engineStartStop, inputBindings.EngineStartStop);
            SetButtonDown(ref anchorDropWeigh, inputBindings.AnchorDropWeigh);
        }

        public void PostFixedUpdate()
        {
            engineStartStop = false;
            anchorDropWeigh = false;
        }

        public void SetAxis(ref float axisValue, string name)
        {
            float value = inputBindings.GetAxis(name);

            // Only assign the value of keyboard input if not 0, otherwise use GUI input
            if(value < -0.02f || value > 0.02f)
            {
                axisValue = value;
            }
        }

        public void SetButtonDown(ref bool isPressed, string name)
        {
            if (inputBindings.GetButtonDown(name)) isPressed = true;
        }

        public void SetLeftThrottle()
        {
            leftThrottle = leftThrottleSlider.value;
        }

        public void SetRightThrottle()
        {
            rightThrottle = rightThrottleSlider.value;
        }

        public void SetRudder()
        {
            rudder = rudderSlider.value;
        }

        public void SetBowThruster()
        {
            bowThruster = bowThrusterSlider.value;
        }

        public void SetSternThruster()
        {
            sternThruster = sternThrusterSlider.value;
        }

        public void SetThrottle()
        {
            throttle = throttleSlider.value;
            if (leftThrottleSlider != null) leftThrottleSlider.value = throttle;
            if (rightThrottleSlider != null) rightThrottleSlider.value = throttle;
        }
    }
}

