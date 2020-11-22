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
    /// Represents a wall segment
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Wall {
        [JsonProperty(PropertyName = "wall")]
        public int id {
            get; private set;
        }

        [JsonProperty(PropertyName = "p1")]
        public Vector2D firstPoint {
            get; private set;
        }

        [JsonProperty(PropertyName = "p2")]
        public Vector2D secondPoint {
            get; private set;
        }
        /// <summary>
        /// In order to access orientation, the Orient method must be called
        /// </summary>
        public int orientation {
            get; private set;
        }

        /// <summary>
        /// Sets the orientation of the entire wall (vertical vs horizontal)
        /// </summary>
        public void Orient() {

            // If the x points match the wall must be vertical
            if (firstPoint.GetX() == secondPoint.GetX()) {
                orientation = Constants.VERTICAL;
            }
            else {
                orientation = Constants.HORIZONTAL;
            }
        }

        /// <summary>
        /// Sets the passed in variables to the coordinates of the center of the top left unit
        /// </summary>
        /// <param name="topLeftX"></param>
        /// <param name="topLeftY"></param>
        public void GetPoints(out double topLeftX, out double topLeftY) {
            if (firstPoint.GetX() < secondPoint.GetX()) {
                topLeftX = firstPoint.GetX();
            }
            else {
                topLeftX = secondPoint.GetX();
            }
            if (firstPoint.GetY() < secondPoint.GetY()) {
                topLeftY = firstPoint.GetY();
            }
            else {
                topLeftY = secondPoint.GetY();
            }
        }
    }
}
