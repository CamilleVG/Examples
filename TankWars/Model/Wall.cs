using System;
using System.Collections.Generic;
using System.Text;
using TankWars;
using Newtonsoft.Json;

namespace Model {
    [JsonObject(MemberSerialization.OptIn)]
    public class Wall {
        [JsonProperty(PropertyName = "wall")]
        public int id {
            get; private set;
        }

        // CHECK THESE POINTS -- TODO

        [JsonProperty(PropertyName = "p1")]
        public Vector2D firstPoint {
            get; private set;
        }

        [JsonProperty(PropertyName = "p2")]
        public Vector2D secondPoint {
            get; private set;
        }

        public int orientation {
            get; private set;
        }

        public Vector2D FirstPoint {
            get => firstPoint;
        }
        public Vector2D SecondPoint {
            get => secondPoint;
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
