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

        // Dictionary collection that stores the states of things
        private Dictionary<int, Tank> Players;
        private Dictionary<int, Projectile> Projectiles;
        private Dictionary<int, Powerup> Powerups;
        private Dictionary<int, Beam> Beams;
        private Dictionary<int, Wall> Walls;


        public World(int size) {
            UniverseSize = size;

        }

        /// <summary>
        /// Sets up a Tank object
        /// </summary>
        /// <param name="t"></param>
        public void setTankData(Tank t) {
            if (!Players.ContainsKey(t.ID)) {
                Players.Add(t.ID, t);
            }
            else {
                Players[t.ID] = t;
            }

            if (t.Disconnected) {
                Players.Remove(t.ID);
            }

        }

        /// <summary>
        /// Sets up a Projectile object
        /// </summary>
        /// <param name="proj"></param>
        public void setProjData(Projectile proj) {
            if (!Projectiles.ContainsKey(proj.ID)) {
                Projectiles.Add(proj.ID, proj);
            }
            else {
                Projectiles[proj.ID] = proj;
            }

            if (proj.Died) {
                Projectiles.Remove(proj.ID);
            }

        }

        /// <summary>
        /// Sets up a Powerup object
        /// </summary>
        /// <param name="power"></param>
        public void setPowerupData(Powerup power) {
            if (!Powerups.ContainsKey(power.ID)) {
                Powerups.Add(power.ID, power);
            }
            else {
                Powerups[power.ID] = power;
            }

            if (power.Died) {
                Powerups.Remove(power.ID);
            }
        }

        /// <summary>
        /// Adds a beam to the model
        /// </summary>
        /// <param name="beam"></param>
        public void setBeamData(Beam beam) {
            if (!Beams.ContainsKey(beam.ID)) {
                Beams.Add(beam.ID, beam);
            }
            else {
                Beams[beam.ID] = beam;
            }

            //if (beam.Died)
            //{
            //    Powerups.Remove(beam.ID);
            //}

        }

        /// <summary>
        /// Sets up a wall object
        /// </summary>
        /// <param name="wall"></param>
        public void setWall(Wall wall) {

            if (!Walls.ContainsKey(wall.ID)) {
                Walls.Add(wall.ID, wall);
            }
            else {
                Walls[wall.ID] = wall;
            }

        }
    }
}
