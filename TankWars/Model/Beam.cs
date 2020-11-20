using System;
using System.Collections.Generic;
using System.Text;
using TankWars;
using Newtonsoft.Json;

namespace Model {
    [JsonObject(MemberSerialization.OptIn)]
    public class Beam {
        [JsonProperty(PropertyName = "beam")]
        private int id;

        [JsonProperty(PropertyName = "org")]
        private Vector2D origin;

        [JsonProperty(PropertyName = "dir")]
        private Vector2D direction;

        [JsonProperty(PropertyName = "owner")]
        private int ownerID;

        public int ticker = 0;

        public int ID {
            get => id;
        }
        public Vector2D Location
        {
            get => origin;
        }
        public Vector2D Orientation
        {
            get => direction;
        }
        public bool AnimationFinished()
        {
            if (ticker >= 4 * Constants.BEAMTIMESCALAR)
            {
                return true;
            }
            return false;
        }
    }
}
