using System;
using System.Collections.Generic;
using System.Linq;
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
        private Dictionary<int, Projectile> Projectiles;
        private Dictionary<int, Powerup> Powerups;


        public World(int size) {
            UniverseSize = size;

        }

        //public bool addPlayer(string name) {
        //    Players.Add(name, new Tank());
        //    return false;

        //}

        public void setTankData(Tank t) {
            if (!Players.ContainsKey(t.ID))
            {
                Players.Add(t.ID, t);
            }
            else
            {
                Players[t.ID] = t;
            }

            if (t.Disconnected)
            {
                Players.Remove(t.ID);
            }

        }
        public void setProjData(Projectile proj)
        {
            if (!Projectiles.ContainsKey(proj.ID))
            {
                Projectiles.Add(proj.ID, proj);
            }
            else
            {
                Projectiles[proj.ID] = proj;
            }

            if (proj.Died)
            {
                Projectiles.Remove(proj.ID);
            }

        }

        public void setPowerupData(Powerup power)
        {
            if (!Powerups.ContainsKey(power.ID))
            {
                Powerups.Add(power.ID, power);
            }
            else
            {
                Powerups[power.ID] = power;
            }

            if (power.Died)
            {
                Powerups.Remove(power.ID);
            }

        }

        public void setBeamsData(Beam beam)
        {
            if (!Powerups.ContainsKey(power.ID))
            {
                Powerups.Add(power.ID, power);
            }
            else
            {
                Powerups[power.ID] = power;
            }

            if (power.Died)
            {
                Powerups.Remove(power.ID);
            }

        }
    }
}
