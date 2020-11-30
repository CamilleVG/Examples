// Authors: Preston Powell and Camille Van Ginkel
// PS8 code for Daniel Kopta's CS 3500 class at the University of Utah Fall 2020
// Version 1.0.3, Nov 2020
using System;
using TankWars;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Model {

    /// <summary>
    /// Represents a tank and its attributes in the game
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Tank {

        [JsonProperty(PropertyName = "tank")]
        public int id {
            get; private set;
        }

        [JsonProperty(PropertyName = "loc")]
        public Vector2D location {
            get; set;
        }

        [JsonProperty(PropertyName = "bdir")]
        public Vector2D orientation {
            get; set;
        }

        [JsonProperty(PropertyName = "tdir")]
        public Vector2D tdir {
            get; set;
        }

        [JsonProperty(PropertyName = "name")]
        public string name {
            get; private set;
        }

        [JsonProperty(PropertyName = "hp")]
        public int hitPoints {
            get; set;
        }

        [JsonProperty(PropertyName = "score")]
        public int score {
            get; set;
        }

        [JsonProperty(PropertyName = "died")]
        public bool died {
            get; set;
        }

        [JsonProperty(PropertyName = "dc")]
        public bool disconnected {
            get; set;
        }

        [JsonProperty(PropertyName = "join")]
        public bool joined {
            get; private set;
        }

        public CommandControl commandControl = new CommandControl();

        public int LastShotFrame {
            get; set;
        }

        public int BeamCount {
            get; set;
        }
        public int LastBeamFrame {
            get; set;
        }

        public int diedOnFrame {
            get; set;
        }

        public bool EnhancedSpeed
        {
            get; set;
        }
        public int SpeedModeFrame
        {
            get; set;
        }
        public bool EnhancedProjectiles
        {
            get; set;
        }
        public int ProjectileModeFrame
        {
            get; set;
        }
        public int ProjectileEnhacementAvailable
        {
            get; set;
        }


        /// <summary>
        /// Creates a new tank object
        /// </summary>
        public Tank() {
            joined = false;
            disconnected = false;
            hitPoints = Constants.MaxHP;
            orientation = new Vector2D(0, -1);
            tdir = new Vector2D(0, -1);
            score = 0;
            died = false;
            BeamCount = 0;
            EnhancedSpeed = false;
            SpeedModeFrame = 0;
            EnhancedProjectiles = false;
            ProjectileModeFrame = 0;
        }

        /// <summary>
        /// Creates a tank with the specified id and location
        /// </summary>
        /// <param name="id"></param>
        /// <param name="loc"></param>
        public Tank(int id, string name, Vector2D loc) : this(){
            this.id = id;
            location = loc;
            this.name = name;

            //joined = false;
            //hitPoints = Constants.MaxHP;
            //orientation = new Vector2D(0, -1);
            //tdir = new Vector2D(0, -1);
            //score = 0;
            //died = false;
            //disconnected = false;
            //BeamCount = 0;
        }

    }
}
