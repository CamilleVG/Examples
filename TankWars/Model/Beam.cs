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
