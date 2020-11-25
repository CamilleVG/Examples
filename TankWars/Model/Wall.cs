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
        /// Default Constructor for JSON
        /// </summary>
        public Wall() {

        }

        /// <summary>
        /// Wall constructor for server
        /// </summary>
        /// <param name="p1">First endpoint of wall</param>
        /// <param name="p2">Second endpoint of wall</param>
        /// <param name="id"></param>
        public Wall(Vector2D p1, Vector2D p2, int id) {
            firstPoint = p1;
            secondPoint = p2;
            this.id = id;
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

        /// <summary>
        /// Sets the passed in variables to the coordinates of the center of the top left unit
        /// </summary>
        /// <param name="bottomRightX"></param>
        /// <param name="bottomRightY"></param>
        public void GetSecondPoints(out double bottomRightX, out double bottomRightY) {
            if (firstPoint.GetX() > secondPoint.GetX()) {
                bottomRightX = firstPoint.GetX();
            }
            else {
                bottomRightX = secondPoint.GetX();
            }
            if (firstPoint.GetY() > secondPoint.GetY()) {
                bottomRightY = firstPoint.GetY();
            }
            else {
                bottomRightY = secondPoint.GetY();
            }
        }
    }
}
