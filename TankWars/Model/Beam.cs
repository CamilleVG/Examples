// Authors: Preston Powell and Camille Van Ginkel
// PS8 code for Daniel Kopta's CS 3500 class at the University of Utah Fall 2020
// Version 1.0.3, Nov 2020

using System;
using System.Collections.Generic;
using System.Text;
using TankWars;
using Newtonsoft.Json;

namespace Model {

    [JsonObject(MemberSerialization.OptIn)]
    public class Beam {

        [JsonProperty(PropertyName = "beam")]
        public int id {
            get; private set;
        }

        [JsonProperty(PropertyName = "org")]
        public Vector2D origin {
            get; private set;
        }

        [JsonProperty(PropertyName = "dir")]
        public Vector2D direction {
            get; private set;
        }

        [JsonProperty(PropertyName = "owner")]
        public int ownerID {
            get; private set;
        }

        public int ticker {
            get; private set;
        }

        private int timesLooped = 0;

        // Statically holds the next id to be assigned to the beam
        private static int NextID = 1;

        /// <summary>
        /// Creates a default beam
        /// </summary>
        public Beam()
        {
            id = NextID;
            NextID++;
            origin = new Vector2D(0,0);
            direction = new Vector2D(0,0);
            ownerID = -1;
        }

        /// <summary>
        /// Creates a new beam
        /// </summary>
        /// <param name="location"></param>
        /// <param name="dir"></param>
        /// <param name="owner"></param>
        public Beam(Vector2D location, Vector2D dir, int owner)
        {
            id = NextID;
            NextID++;
            origin = location;
            direction = dir;
            ownerID = owner;
        }

        /// <summary>
        /// Increments the ticker, which represent the frame the gif is currently on.
        /// </summary>
        public void advanceTicker() {
            lock (this) {
                ticker++;
                if (ticker >= 4 * Constants.BEAMTIMESCALAR && timesLooped < Constants.BEAMLOOPTIMES) {
                    ticker = 0;
                    timesLooped++;
                }
            }
        }

        /// <summary>
        /// Determines when the gif is finished being displayed
        /// </summary>
        /// <returns></returns>
        public bool AnimationFinished() {
            lock (this) {
                if (ticker >= 4 * Constants.BEAMTIMESCALAR) {
                    return true;
                }
                return false;
            }
        }
    }
}
