using System;
using TankWars;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Model {
    [JsonObject(MemberSerialization.OptIn)]
    public class Tank {

        [JsonProperty(PropertyName = "tank")]
        public int id {
            get; private set;
        }

        [JsonProperty(PropertyName = "loc")]
        public Vector2D location {
            get; private set;
        }

        [JsonProperty(PropertyName = "bdir")]
        public Vector2D orientation {
            get; private set;
        }

        [JsonProperty(PropertyName = "tdir")]
        public Vector2D tdir {
            get; private set;
        }

        [JsonProperty(PropertyName = "name")]
        public string name {
            get; private set;
        }

        [JsonProperty(PropertyName = "hp")]
        public int hitPoints {
            get; private set;
        }

        [JsonProperty(PropertyName = "score")]
        public int score {
            get; private set;
        }

        [JsonProperty(PropertyName = "died")]
        public bool died {
            get; private set;
        }

        [JsonProperty(PropertyName = "dc")]
        public bool disconnected {
            get; private set;
        }

        [JsonProperty(PropertyName = "join")]
        public bool joined {
            get; private set;
        }


        public Tank() {
            joined = false;
            disconnected = false;
            hitPoints = Constants.MaxHP;
            tdir = new Vector2D(0, -1);
            score = 0;
            died = false;
        }


    }
}
