using System;
using System.Collections.Generic;
using System.Text;
using TankWars;
using Newtonsoft.Json;
namespace Model {
    [JsonObject(MemberSerialization.OptIn)]
    public class Projectile {
        [JsonProperty(PropertyName = "proj")]
        private int id;

        [JsonProperty(PropertyName = "loc")]
        private Vector2D location;

        [JsonProperty(PropertyName = "dir")]
        private Vector2D orientation;

        [JsonProperty(PropertyName = "died")]
        private bool died;

        [JsonProperty(PropertyName = "owner")]
        private int owner;

        public int ID {
            get => id;
        }
        public bool Died {
            get => died;
        }
        public int Owner {
            get => owner;
        }
        public Vector2D Location {
            get => location;
        }
        public Vector2D Orientation {
            get => orientation;
        }
    }
}
