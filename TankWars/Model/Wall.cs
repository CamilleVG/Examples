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
        private Vector2D onePoint;

        [JsonProperty(PropertyName = "p2")]
        private Vector2D otherPoint;

        public int ID {
            get => id;
        }
    }
}
