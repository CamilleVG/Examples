using System;
using System.Collections.Generic;
using System.Text;
using TankWars;

namespace Model {

    public class World {

        public int UniverseSize { get; private set; }

        // Number of frames that must pass before a tank can shoot again
        public int FramesPerShot { get; private set; }

        // How many ms must pass before a defeated tank can respawn
        public int RespawnRate { get; private set; }


        private Dictionary<int, Tank> Players;
        private Dictionary<int, Projectiles> Powerups;


        public World(int size) {
            UniverseSize = size;

        }

        //public bool addPlayer(string name) {
        //    Players.Add(name, new Tank());
        //    return false;

        //}

        public void setTankData(Tank t) {
            

        }


        public bool removePlayer(string name) {
            return false;

        }
    }
}
