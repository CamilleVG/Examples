using System;
using System.Collections.Generic;
using System.Text;
using TankWars;
using Newtonsoft.Json;
using Resources;

namespace Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Projectile
    {
        [JsonProperty(PropertyName = "proj")]
        private int id;

        [JsonProperty(PropertyName = "loc")]
        private Vector2D location;

        [JsonProperty(PropertyName = "dir")]
        private Vector2D orientation;

        [JsonProperty(PropertyName = "died")]
        private bool died;

        [JsonProperty(PropertyName = "owner")]
        private string owner;

        public int ID
        {
            get => id;
        }
        public bool Died
        {
            get => died;
        }
        public Vector2D Location
        {
            get => location;
        }
        public Vector2D Orientation
        {
            get => orientation;
        }
    }
}
