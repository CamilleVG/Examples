using System;
using System.Collections.Generic;
using System.Text;
using TankWars;
using Newtonsoft.Json;

namespace Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Wall
    {
        [JsonProperty(PropertyName = "wall")]
        private int id;

        // CHECK THESE POINTS -- TODO

        [JsonProperty(PropertyName = "p1")]
        private Vector2D firstPoint;

        [JsonProperty(PropertyName = "p2")]
        private Vector2D secondPoint;

        public int orientation;

        public int ID {
            get => id;
        }

        public Vector2D FirstPoint
        {
            get => firstPoint;
        }
        public Vector2D SecondPoint
        {
            get => secondPoint;
        }

        public void Orient()
        {
            if (firstPoint.GetX() == secondPoint.GetX()){
                orientation = Constants.VERTICAL;  

            }
            else
            {
                orientation = Constants.HORIZONTAL;
            }
        }

        public void GetPoint(out double topLeftX, out double topLeftY)
        {
            if (firstPoint.GetX() < secondPoint.GetX())
            {
                topLeftX = firstPoint.GetX();
            }
            else
            {
                topLeftX = secondPoint.GetX();
            }
            if (firstPoint.GetY() < secondPoint.GetY())
            {
                topLeftY = firstPoint.GetY();
            }
            else
            {
                topLeftY = secondPoint.GetY();
            }
        }
    }
}
