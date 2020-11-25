// Authors: Preston Powell and Camille Van Ginkel
// PS8 code for Daniel Kopta's CS 3500 class at the University of Utah Fall 2020
// Version 1.0.3, Nov 2020

using System;
using System.Collections.Generic;
using System.Text;
using TankWars;
using Newtonsoft.Json;
namespace Model {

    /// <summary>
    /// Represents a projectile object
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Projectile {

        [JsonProperty(PropertyName = "proj")]
        public int id {
            get; private set;
        }

        [JsonProperty(PropertyName = "loc")]
        public Vector2D location {
            get; private set;
        }

        [JsonProperty(PropertyName = "dir")]
        public Vector2D orientation {
            get; private set;
        }

        [JsonProperty(PropertyName = "died")]
        public bool died {
            get; private set;
        }

        [JsonProperty(PropertyName = "owner")]
        public int owner {
            get; private set;
        }
        private static int NextID = 1;
        public void UpdateLocation(Vector2D Location)
        {
            this.location = Location;
        }
        public Projectile(Vector2D tdir, Vector2D origin, int identity)
        {
            orientation = tdir;
            location = origin;
            owner = identity;
            id = NextID;
            NextID++;
        }
    }
}
