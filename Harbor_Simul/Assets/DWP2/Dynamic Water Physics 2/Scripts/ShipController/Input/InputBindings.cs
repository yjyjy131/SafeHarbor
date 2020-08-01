using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Rewired;

namespace DWP2.ShipController
{
    /// <summary>
    /// Class containing all input bindings. Check comments if you need to implement a 3rd party input manager
    /// </summary>
    public class InputBindings
    {
        /// ********************************************************* /// 
        /// **** You can change the names of input bindings here **** ///
        /// ********************************************************* ///
        /// 
        public string BowThruster => "BowThruster";
        public string SternThruster => "SternThruster";
        public string Rudder => "Horizontal";
        public string LeftThrottle => "LeftThrottle";
        public string RightThrottle => "RightThrottle";
        public string Throttle => "Vertical";
        public string EngineStartStop => "EngineStartStop";
        public string AnchorDropWeigh => "AnchorDropWeigh";

        // Rewired:
        // private Player player;

        /// <summary>
        /// Use this function if you need to initialize your input manager.
        /// </summary>
        public void Initialize()
        {
            // Input manager initialization here (if needed).

            // e.g. for Rewired:
            // player = ReInput.players.GetPlayer(0);
        }

        public float GetAxis(string name)
        {
            try
            {/*
                switch (name)
                {
                    case "Vertical":return InputSystem.Instance.speed;
                    case "Horizontal": return InputSystem.Instance.angle;
                    default: return Input.GetAxis(name);
                }*/
                /// ************************************************************************ /// 
                /// **** REPLACE THE NEXT LINE IF YOU ARE USING DIFFERENT INPUT MANAGER **** ///
                /// ************************************************************************ ///
                // e.g. for Rewired replace the line above with:
                return Input.GetAxis(name);
                // (make sure you use Initialize() function above to get the player first.
            }
            catch
            {
                return 0;
            }
        }

        public bool GetButtonDown(string name)
        {
            try
            {
                /// ************************************************************************ /// 
                /// **** REPLACE THE NEXT LINE IF YOU ARE USING DIFFERENT INPUT MANAGER **** ///
                /// ************************************************************************ ///
                return Input.GetButtonDown(name);
            }
            catch
            {
                return false;
            }
        }
    }
}
