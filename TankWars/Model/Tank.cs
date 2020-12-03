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

        // public as there is no need for error checking on command control setting in this class
        public CommandControl commandControl = new CommandControl();

        public int LastShotFrame {
            get; set;
        }

        public int BeamCount {
            get; set;
        }

        /// <summary>
        /// Frame the most recent beam was sent on, this is used for the beam cooldown feature
        /// </summary>
        public int LastBeamFrame {
            get; set;
        }

        /// <summary>
        /// What frame the tank died on last
        /// </summary>
        public int diedOnFrame {
            get; set;
        }

        public bool EnhancedSpeed {
            get; set;
        }

        /// <summary>
        /// The frame on which the speed mode powerup was last initiated on
        /// </summary>
        public int SpeedModeFrame {
            get; set;
        }

        public bool EnhancedProjectiles {
            get; set;
        }

        /// <summary>
        /// The frame on which the enhanced projectile powerup was last initiated
        /// </summary>
        public int ProjectileModeFrame {
            get; set;
        }

        /// <summary>
        /// If a tank has a projectile enhancement powerup that will be activated on the next shot
        /// </summary>
        public bool ProjectileEnhacementAvailable {
            get; set;
        }

        public bool FasterFireRate {
            get; set;
        }

        /// <summary>
        /// Frame on which the faster fire rate powerup was last initiated
        /// </summary>
        public int FasterFireRateStartFrame {
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
            ProjectileEnhacementAvailable = false;
            FasterFireRate = false;
            FasterFireRateStartFrame = 0;
        }

        /// <summary>
        /// Creates a tank with the specified id and location
        /// </summary>
        /// <param name="id"></param>
        /// <param name="loc"></param>
        public Tank(int id, string name, Vector2D loc) : this() {
            this.id = id;
            location = loc;
            this.name = name;
        }

    }
}
