using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TankWars;
using Newtonsoft.Json;

namespace Model {

    [JsonObject(MemberSerialization.OptIn)]
    public class CommandControl {
        private LinkedList<string> activeCommands;

        [JsonProperty(PropertyName = "moving")]
        private string moving;

        [JsonProperty(PropertyName = "fire")]
        public string fire;

        [JsonProperty(PropertyName = "tdir")]
        public Vector2D tDirection;

        public bool addCommand(string cmd) {
            activeCommands.AddFirst(cmd);
            moving = activeCommands.First.Value;
            return false;
        }

        public bool removeCommand(string cmd) {
            activeCommands.Remove(cmd);
            moving = activeCommands.First.Value;
            return false;
        }

        public string getActiveCommand() {
            return activeCommands.First.Value;
        }


        public CommandControl() {
            moving = "none";
            fire = "none";
            tDirection = new Vector2D(0, 1);
            activeCommands = new LinkedList<string>();
            activeCommands.AddLast("none");
        }

    }
}
