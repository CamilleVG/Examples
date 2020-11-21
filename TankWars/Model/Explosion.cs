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
    /// Contains information about an explosion
    /// </summary>
    public class Explosion {

        private readonly Vector2D location;
        public int ticker;


        public Vector2D Location {
            get => location;
        }

        /// <summary>
        /// Creates a new explosion at the specified location
        /// </summary>
        public Explosion(Vector2D loc) {
            location = loc;
        }

        /// <summary>
        /// Whether this explosion has reached its final frame
        /// </summary>
        /// <returns></returns>
        public bool AnimationFinished() {
            if (ticker >= 7 * Constants.EXPLOSIONTIMESCALAR) {
                return true;
            }
            return false;
        }

    }
}
