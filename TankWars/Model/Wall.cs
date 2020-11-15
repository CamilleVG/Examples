﻿using System;
using System.Collections.Generic;
using System.Text;
using TankWars;
using Newtonsoft.Json;
using Resources;

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
    }
}
