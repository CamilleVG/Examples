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
            get; private set;
        }

        [JsonProperty(PropertyName = "bdir")]
        public Vector2D orientation {
            get; private set;
        }

        [JsonProperty(PropertyName = "tdir")]
        public Vector2D tdir {
            get; private set;
        }

        [JsonProperty(PropertyName = "name")]
        public string name {
            get; private set;
        }

        [JsonProperty(PropertyName = "hp")]
        public int hitPoints {
            get; private set;
        }

        [JsonProperty(PropertyName = "score")]
        public int score {
            get; private set;
        }

        [JsonProperty(PropertyName = "died")]
        public bool died {
            get; private set;
        }

        [JsonProperty(PropertyName = "dc")]
        public bool disconnected {
            get; private set;
        }

        [JsonProperty(PropertyName = "join")]
        public bool joined {
            get; private set;
        }

        public CommandControl commandControl = new CommandControl();


        /// <summary>
        /// Creates a new tank object
        /// </summary>
        public Tank() {
            joined = false;
            disconnected = false;
            hitPoints = Constants.MaxHP;
            tdir = new Vector2D(0, -1);
            score = 0;
            died = false;
        }

        /// <summary>
        /// Creates a tank with the specified id and location
        /// </summary>
        /// <param name="id"></param>
        /// <param name="loc"></param>
        public Tank(int id, string name, Vector2D loc) {
            this.id = id;
            location = loc;
            this.name = name;

            joined = false;
            hitPoints = Constants.MaxHP;
            orientation = new Vector2D(0, -1);
            tdir = new Vector2D(0, -1);
            score = 0;
            died = false;
            disconnected = false;

        }

        public void UpdateLocation(Vector2D Location)
        {
            this.location = Location;
        }

    }
}
