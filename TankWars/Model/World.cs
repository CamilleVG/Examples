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
        public Dictionary<int, Tank> Players;
        public Dictionary<int, Projectile> Projectiles;
        public Dictionary<int, Powerup> Powerups;
        public Dictionary<int, Beam> Beams;
        public Dictionary<int, Wall> Walls;
        private LinkedList<string> colorOrder;
        private Dictionary<int, string> tankColors;


        public string getTankColor(int id) {
            if (tankColors.ContainsKey(id))
                return tankColors[id];
            return "";
        }

        public World(int size) {
            UniverseSize = size;
            Players = new Dictionary<int, Tank>();
            Projectiles = new Dictionary<int, Projectile>();
            Powerups = new Dictionary<int, Powerup>();
            Beams = new Dictionary<int, Beam>();
            Walls = new Dictionary<int, Wall>();
            colorOrder = new LinkedList<string>();
            tankColors = new Dictionary<int, string>();
            addColors();
        }

        private void addColors() {
            colorOrder.AddLast("blue");
            colorOrder.AddLast("dark");
            colorOrder.AddLast("green");
            colorOrder.AddLast("lightGreen");
            colorOrder.AddLast("orange");
            colorOrder.AddLast("purple");
            colorOrder.AddLast("red");
            colorOrder.AddLast("yellow");
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
            if (!tankColors.ContainsKey(t.ID)) {
                tankColors.Add(t.ID, colorOrder.First());
                string temp = colorOrder.First();
                colorOrder.RemoveFirst();
                colorOrder.AddLast(temp);

            }
            if (t.Died) {
                Players.Remove(t.ID);
                return;
            }
            if (t.Disconnected) {
                Players.Remove(t.ID);

                colorOrder.Remove(tankColors[t.ID]);
                colorOrder.AddFirst(tankColors[t.ID]);

                tankColors.Remove(t.ID);
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
            wall.Orient(); //calculates whether the wall is horizontal or vertical for the drawing panel

        }
    }
}
