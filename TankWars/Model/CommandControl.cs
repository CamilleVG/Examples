// Authors: Preston Powell and Camille Van Ginkel
// PS8 code for Daniel Kopta's CS 3500 class at the University of Utah Fall 2020
// Version 1.0.3, Nov 2020

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TankWars;
using Newtonsoft.Json;

namespace Model {

    /// <summary>
    /// An object used to represent the state of commands a player wants to send to the server
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class CommandControl {

        /// <summary>
        /// Stores recent commands in order of priority
        /// </summary>
        private LinkedList<string> ActiveCommands;

        /// <summary>
        /// Which direction the tank wants to move
        /// </summary>
        [JsonProperty(PropertyName = "moving")]
        public string moving {
            get; private set;
        }

        /// <summary>
        /// Which firetype the tank wants to shoot
        /// </summary>
        [JsonProperty(PropertyName = "fire")]
        public string fire;

        /// <summary>
        /// Direction the tank wants to aim its turret
        /// </summary>
        [JsonProperty(PropertyName = "tdir")]
        public Vector2D tDirection;

        /// <summary>
        /// Sets a command to the highest priority
        /// </summary>
        /// <param name="cmd">Command to set</param>
        public bool AddCommand(string cmd) {
            if (ActiveCommands.First.Value != cmd && !ActiveCommands.Contains(cmd)) {
                ActiveCommands.AddFirst(cmd);
                moving = ActiveCommands.First.Value;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Removes a command from the command list
        /// </summary>
        /// /// <param name="cmd">Command to remove</param>
        public bool RemoveCommand(string cmd) {
            if (ActiveCommands.Remove(cmd)) {
                moving = ActiveCommands.First.Value;
                return true;
            }
            return false;
        }

        /// <returns>The current highest priority command</returns>
        public string GetActiveCommand() {
            return ActiveCommands.First.Value;
        }

        /// <summary>   
        /// Constructs a command control with a tanks default state
        /// </summary>
        public CommandControl() {
            moving = "none";
            fire = "none";
            tDirection = new Vector2D(0, 1);
            ActiveCommands = new LinkedList<string>();
            ActiveCommands.AddLast("none");
        }
    }
}
