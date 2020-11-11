using System;
using TankWars;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Resources;

namespace Model {
    [JsonObject(MemberSerialization.OptIn)]
    public class Tank {

        [JsonProperty(PropertyName = "tank")]
        private int id;

        [JsonProperty(PropertyName = "loc")]
        private Vector2D location;

        [JsonProperty(PropertyName = "bdir")]
        private Vector2D orientation;

        [JsonProperty(PropertyName = "tdir")]
        private Vector2D aiming = new Vector2D(0, -1);

        [JsonProperty(PropertyName = "name")]
        private string name;

        [JsonProperty(PropertyName = "hp")]
        private int hitPoints = Constants.MaxHP;

        [JsonProperty(PropertyName = "score")]
        private int score = 0;

        [JsonProperty(PropertyName = "died")]
        private bool died = false;

        [JsonProperty(PropertyName = "dc")]
        private bool disconnected = false;

        [JsonProperty(PropertyName = "join")]
        private bool joined = false;

        public int ID
        {
            get => id;
        }
        public bool Disconnected
        {
            get => disconnected;
        }

        public bool Died {
            get => died;
        }


        public Tank() {

        }


    }
}
