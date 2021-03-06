﻿// Authors: Preston Powell and Camille Van Ginkel
// PS8 code for Daniel Kopta's CS 3500 class at the University of Utah Fall 2020
// Version 1.0.3, Nov 2020
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

        // How many ms must pass before a defeated tank can respawn
        public int CanAddPowerupFrame{ get; set; }

        // How many ms must pass before a defeated tank can respawn
        public int ActivePowerups { get; set; }

        public delegate void AnimationRecieved(Object o);
        public event AnimationRecieved AddAnimation;

        // Dictionary collection that stores the states of things
        public Dictionary<int, Tank> Players;
        public Dictionary<int, Projectile> Projectiles;
        public Dictionary<int, Powerup> Powerups;
        public Dictionary<int, Wall> Walls;
        private LinkedList<string> colorOrder;
        private Dictionary<int, string> tankColors;
        

        /// <summary>
        /// Returns the color of the specified tank
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public string getTankColor(int id) {
            if (tankColors.ContainsKey(id))
                return tankColors[id];
            return "";
        }

        /// <summary>
        /// Creates an object representing the world with length and width of input size
        /// </summary>
        /// <param name="size"></param>
        public World(int size) {
            UniverseSize = size;
            Players = new Dictionary<int, Tank>();
            Projectiles = new Dictionary<int, Projectile>();
            Powerups = new Dictionary<int, Powerup>();
            Walls = new Dictionary<int, Wall>();
            colorOrder = new LinkedList<string>();
            tankColors = new Dictionary<int, string>();
            ActivePowerups = 0;
            addColors();
        }

        /// <summary>
        /// Creates a world with member variables FramesPerShot and RespawnRate initialized.
        /// </summary>
        /// <param name="size"></param>
        /// <param name="framesPerShot"></param>
        /// <param name="respawnRate"></param>
        public World(int size, int framesPerShot, int respawnRate): this(size)
        {
            FramesPerShot = framesPerShot;
            RespawnRate = respawnRate;
        }

        /// <summary>
        /// Adds all possible colors of tanks in the game
        /// </summary>
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
            //Adds tank to dictionary, or update the tank in dictionary to the passed in tank
            if (!Players.ContainsKey(t.id) && (t.hitPoints != 0)) {
                Players.Add(t.id, t);
            }
            else if (t.hitPoints != 0) {
                Players[t.id] = t;
            }
            //handles players death and disconnects
            else {
                if (t.died) {
                    Players.Remove(t.id);
                    AddAnimation(new Explosion(t.location));
                    return;
                }
                if (t.disconnected) {
                    Players.Remove(t.id);

                    colorOrder.Remove(tankColors[t.id]);
                    colorOrder.AddFirst(tankColors[t.id]);

                    tankColors.Remove(t.id);
                }
            }

            //Assigns a tank a color
            if (!tankColors.ContainsKey(t.id)) {
                tankColors.Add(t.id, colorOrder.First());
                string temp = colorOrder.First();
                colorOrder.RemoveFirst();
                colorOrder.AddLast(temp);
            }

        }

        /// <summary>
        /// Sets up a Projectile object
        /// </summary>
        /// <param name="proj"></param>
        public void setProjData(Projectile proj) {
            if (!Projectiles.ContainsKey(proj.id)) {
                Projectiles.Add(proj.id, proj);
            }
            else {
                Projectiles[proj.id] = proj;
            }

            if (proj.died) {
                Projectiles.Remove(proj.id);
            }

        }

        /// <summary>
        /// Sets up a Powerup object
        /// </summary>
        /// <param name="power"></param>
        public void setPowerupData(Powerup power) {
            if (!Powerups.ContainsKey(power.id)) {
                Powerups.Add(power.id, power);
            }
            else {
                Powerups[power.id] = power;
            }

            if (power.died) {
                Powerups.Remove(power.id);
            }
        }

        /// <summary>
        /// Adds a beam to the model
        /// </summary>
        /// <param name="beam"></param>
        public void setBeamData(Beam beam) {
            AddAnimation(beam);
        }

        /// <summary>
        /// Sets up a wall object
        /// </summary>
        /// <param name="wall"></param>
        public void setWall(Wall wall) {
            if (!Walls.ContainsKey(wall.id)) {
                Walls.Add(wall.id, wall);
            }
            else {
                Walls[wall.id] = wall;
            }
            wall.Orient(); //calculates whether the wall is horizontal or vertical for the drawing panel
        }
    }
}
