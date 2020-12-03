// Authors: Preston Powell and Camille Van Ginkel
// PS9 code for Daniel Kopta's CS 3500 class at the University of Utah Fall 2020
// Version 1.0.3, Nov - Dec 2020
using Model;
using NetworkUtil;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Xml;
using TankWars;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace Server {

    /// <summary>
    /// Holds a server object and contains a main method that creates and runs such a server
    /// </summary>
    class Server {

        // Dictionary used to store all client ids and their connections
        Dictionary<SocketState, Tank> clients;

        // The server's TcpListener
        TcpListener theServer;

        World world;
        private int MSPerFrame;
        private int Frame;


        static void Main(string[] args) {

            // Make a new server
            Server server = new Server();
            server.ReadSettings("..\\..\\..\\..\\Resources\\Settings.xml");
            server.StartServer();
            Console.WriteLine("Server started, now accepting clients");

            // Run an infinite updating thread to recalculate the world every frame
            Stopwatch watch = new Stopwatch();
            watch.Start();
            while (true) {
                while (watch.ElapsedMilliseconds < server.MSPerFrame) { }
                watch.Restart();
                server.updateWorld();
            }
        }

        /// <summary>
        /// Updates everything needed in a frame
        /// </summary>
        private void updateWorld() {
            Frame++;

            lock (world) {

                // Update all projectiles
                HashSet<Projectile> projToRemove = new HashSet<Projectile>();
                foreach (Projectile proj in world.Projectiles.Values) {
                    updateProjectile(proj, projToRemove);
                }

                // Update all tanks
                HashSet<Beam> BeamsToSend = new HashSet<Beam>();
                HashSet<Tank> TanksToRemove = new HashSet<Tank>();
                foreach (Tank t in world.Players.Values) {
                    // Identify all disconnected tanks
                    if (t.disconnected) {
                        TanksToRemove.Add(t);
                        t.hitPoints = 0;
                        continue;
                    }
                    updateTank(t, BeamsToSend);
                }

                // Create all beams
                foreach (Beam b in BeamsToSend) {
                    createBeams(b, TanksToRemove);
                }

                //Adds powerups if necesary
                if ((world.ActivePowerups < Constants.MAXPOWERUPS) && (Frame >= world.CanAddPowerupFrame)) {
                    spawnPowerup();
                }

                // Checks for powerup collisions
                HashSet<Powerup> PowerupsToRemove = new HashSet<Powerup>();
                foreach (Powerup p in world.Powerups.Values) {
                    if (HandlePowerupCollision(p)) {
                        PowerupsToRemove.Add(p);
                    }
                }

                // Send the data to each client
                SendDataToAllClients(BeamsToSend);

                // Set all tanks' died properties to false
                foreach (Tank t in world.Players.Values)
                    t.died = false;

                // Remove dc'd tanks
                foreach (Tank t in TanksToRemove)
                    world.Players.Remove(t.id);

                // Remove dead projectiles
                foreach (Projectile proj in projToRemove)
                    world.Projectiles.Remove(proj.id);

                //Removes dead powerups
                foreach (Powerup pow in PowerupsToRemove)
                    world.Powerups.Remove(pow.id);

            }
        }

        /// <summary>
        /// Sends the data to all clients, including any beams
        /// </summary>
        private void SendDataToAllClients(HashSet<Beam> BeamsToSend) {
            lock (clients) {
                lock (world) {
                    foreach (SocketState s in clients.Keys) {
                        foreach (Tank t in world.Players.Values) {
                            Networking.Send(s.TheSocket, JsonConvert.SerializeObject(t) + "\n");
                        }
                        foreach (Powerup pow in world.Powerups.Values) {
                            Networking.Send(s.TheSocket, JsonConvert.SerializeObject(pow) + "\n");
                        }
                        foreach (Projectile proj in world.Projectiles.Values) {
                            Networking.Send(s.TheSocket, JsonConvert.SerializeObject(proj) + "\n");
                        }
                        foreach (Beam beam in BeamsToSend) {
                            Networking.Send(s.TheSocket, JsonConvert.SerializeObject(beam) + "\n");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the position and collisions for a projectile, and if it dies adds it to the given HashSet and marks it as dead
        /// </summary>
        /// <param name="proj"></param>
        /// <param name="projToRemove"></param>
        private void updateProjectile(Projectile proj, HashSet<Projectile> projToRemove) {

            // Determine if speed is enhanced
            int speed = Constants.PROJECTILESPEED;
            if (proj.Enhanced) {
                speed = Constants.ENHANCEDPROJECTILESPEED;
            }
            // Check for collisions
            if (!CheckForWallCollision(proj.location + (proj.orientation * speed), 0)) {

                proj.location = (proj.location + (proj.orientation * speed));

                // check to see if the projectile will collide with any tanks and deal damage accordingly
                if (CheckForTankCollision(proj, out Tank t)) {
                    t.hitPoints--;
                    if (proj.Enhanced && t.hitPoints > 1) {
                        t.hitPoints--;
                    }
                    // check if the collision resulted in a tank's death
                    if (t.hitPoints == 0) {
                        t.died = true;
                        t.diedOnFrame = Frame;
                        // increase the killing player's score
                        world.Players[proj.owner].score++;
                    }
                    projToRemove.Add(proj);
                    proj.died = true;
                }
            }
            // If a wall collision occured, kill the projectile
            else {
                projToRemove.Add(proj);
                proj.died = true;
            }
        }

        /// <summary>
        /// Creates beams, checks for collisions and adds any caught tanks into the given hashset
        /// </summary>
        /// <param name="b"></param>
        /// <param name="TanksToRemove"></param>
        private void createBeams(Beam b, HashSet<Tank> TanksToRemove) {
            // check each tank for a collision
            foreach (Tank t in world.Players.Values) {
                if (CheckBeamCollision(b.origin, b.direction, t.location, Constants.TANKSIZE / 2)) {
                    t.hitPoints = 0;
                    t.died = true;
                    t.diedOnFrame = Frame;
                    world.Players[b.ownerID].score++;
                }
            }

        }

        /// <summary>
        /// Updates the given tank's movement, turret direction, respawns if possible, and handles firing of projectiles and beams 
        /// (passes beams into given hashet to be fired elsewhere)
        /// </summary>
        /// <param name="t"></param>
        /// <param name="BeamsToSend"></param>
        private void updateTank(Tank t, HashSet<Beam> BeamsToSend) {

            // Respawn tank if dead and enough time has passed
            if (t.hitPoints <= 0) {
                if (Frame >= t.diedOnFrame + world.RespawnRate) {
                    spawnTank(t);
                }
                return;
            }

            // Enhance speed if needed
            int speed = Constants.TANKSPEED;
            if (t.EnhancedSpeed) {
                speed = Constants.ENHANCEDTANKSPEED;
            }

            // Handle tank movement, block if a collision with a wall will occur, wraparound if possible and necessary
            switch (t.commandControl.moving) {
                case "up":
                    t.orientation = (Constants.UP);
                    // Wrap around to bottom of world if possible
                    if ((t.location + (Constants.UP * speed)).GetY() < -world.UniverseSize / 2)
                        if (!CheckForWallCollision(new Vector2D(t.location.GetX(), world.UniverseSize / 2), Constants.TANKSIZE / 2))
                            t.location = new Vector2D(t.location.GetX(), world.UniverseSize / 2);
                        else break;
                    // Check for wall collisions
                    if (!CheckForWallCollision(t.location + (Constants.UP * speed), Constants.TANKSIZE / 2))
                        t.location += (Constants.UP * speed);
                    break;
                case "down":
                    t.orientation = (Constants.DOWN);
                    // Wrap around to top of world if possible
                    if ((t.location + (Constants.DOWN * speed)).GetY() > world.UniverseSize / 2)
                        if (!CheckForWallCollision(new Vector2D(t.location.GetX(), -world.UniverseSize / 2), Constants.TANKSIZE / 2))
                            t.location = new Vector2D(t.location.GetX(), -world.UniverseSize / 2);
                        else break;
                    // Check for wall collisions
                    if (!CheckForWallCollision(t.location + (Constants.DOWN * speed), Constants.TANKSIZE / 2))
                        t.location += (Constants.DOWN * speed);
                    break;
                case "right":
                    t.orientation = (Constants.RIGHT);
                    // Wrap around to left of world if possible
                    if ((t.location + (Constants.RIGHT * speed)).GetX() > world.UniverseSize / 2)
                        if (!CheckForWallCollision(new Vector2D(-world.UniverseSize / 2, t.location.GetY()), Constants.TANKSIZE / 2))
                            t.location = new Vector2D(-world.UniverseSize / 2, t.location.GetY());
                        else break;
                    // Check for wall collisions
                    if (!CheckForWallCollision(t.location + (Constants.RIGHT * speed), Constants.TANKSIZE / 2))
                        t.location += (Constants.RIGHT * speed);
                    break;
                case "left":
                    t.orientation = (Constants.LEFT);
                    // Wrap around to right of world if possible
                    if ((t.location + (Constants.LEFT * speed)).GetX() < -world.UniverseSize / 2)
                        if (!CheckForWallCollision(new Vector2D(world.UniverseSize / 2, t.location.GetY()), Constants.TANKSIZE / 2))
                            t.location = new Vector2D(world.UniverseSize / 2, t.location.GetY());
                        else break;
                    // Check for wall collisions
                    if (!CheckForWallCollision(t.location + (Constants.LEFT * speed), Constants.TANKSIZE / 2))
                        t.location += (Constants.LEFT * speed);
                    break;
            }

            // Update turretdirection
            t.tdir = (t.commandControl.tDirection);


            // Update firing
            switch (t.commandControl.fire) {
                case "main":
                    //Check if firing is allowed
                    // For a faster firing tank
                    if (t.FasterFireRate) {
                        if (Frame < t.LastShotFrame + world.FramesPerShot / 4) {
                            break;
                        }
                    }
                    // For a normal tank
                    else {
                        if (Frame < t.LastShotFrame + world.FramesPerShot) {
                            break;
                        }
                    }

                    // Enable enhance projectiles if available
                    if (t.ProjectileEnhacementAvailable && !t.EnhancedProjectiles) {
                        t.EnhancedProjectiles = true;
                        t.ProjectileModeFrame = Frame;
                    }

                    // Handle creation of enhanced projectiles
                    Projectile proj = new Projectile(t.tdir, t.location, t.id);
                    if (t.EnhancedProjectiles) {
                        proj.Enhanced = true;
                    }
                    world.setProjData(proj);
                    t.LastShotFrame = Frame;
                    break;

                case "alt":
                    //Check if firing a beam is allowed
                    if ((t.BeamCount > 0) && (Frame - t.LastBeamFrame > Constants.BEAMCOOLDOWN)) {
                        BeamsToSend.Add(new Beam(t.location, t.tdir, t.id));
                        t.BeamCount--;
                        t.LastBeamFrame = Frame;
                    }
                    break;
            }

            // Check if faster fire rate has expired
            if (Frame >= t.FasterFireRateStartFrame + Constants.FASTERFIRERATETIME) {
                t.FasterFireRate = false;
            }

            // Check if enhanced speed has expired
            if (Frame >= t.SpeedModeFrame + Constants.ENHANCEDTANKSPEEDTIME) {
                t.EnhancedSpeed = false;
            }

            // Check if enhanced projectiles has expired
            if (Frame >= t.ProjectileModeFrame + Constants.ENHANCEDPROJECTILETIME) {
                t.EnhancedProjectiles = false;
            }
        }


        /// <summary>
        /// Checks if a powerup has collided with any tank and updates the tanks beam count and powerup status accordingly,
        /// or generates a random powerup if mystery powerups is active
        /// </summary>
        /// <param name="pow"></param>
        /// <returns></returns>
        private bool HandlePowerupCollision(Powerup pow) {

            foreach (Tank tank in world.Players.Values) {
                // ignore dead tanks
                if (tank.hitPoints <= 0)
                    continue;
                // approximate tank collisions using a circular tank
                if ((pow.location - tank.location).Length() < Constants.TANKSIZE / 2) {
                    // Standard behavior is powerups grant a beam attack
                    if (Constants.MYSTERYPOWERUPS == 0) {
                        tank.BeamCount++;
                    }
                    // If mystery powerups are activated, pick randomly from the mystery effects
                    else {
                        Random r = new Random();
                        switch (r.Next(5)) {
                            case 0: // Standard Beam Attack
                                tank.BeamCount++;
                                break;
                            case 1: // Enhanced Tank Speed
                                tank.EnhancedSpeed = true;
                                tank.SpeedModeFrame = Frame;
                                break;
                            case 2: // Enhanced projectile speed and damage
                                tank.ProjectileEnhacementAvailable = true;
                                break;
                            case 3: // Rapid Fire Rate
                                tank.FasterFireRate = true;
                                tank.FasterFireRateStartFrame = Frame;
                                break;
                            case 4: // Full Heal
                                tank.hitPoints = Constants.MaxHP;
                                break;
                        }
                    }
                    // Remove the powerup
                    pow.died = true;
                    world.ActivePowerups--;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines if a ray interescts a circle, provided by Professor Kopta
        /// </summary>
        /// <param name="rayOrig">The origin of the ray</param>
        /// <param name="rayDir">The direction of the ray</param>
        /// <param name="center">The center of the circle</param>
        /// <param name="r">The radius of the circle</param>
        /// <returns></returns>
        public static bool CheckBeamCollision(Vector2D rayOrig, Vector2D rayDir, Vector2D center, double r) {
            // ray-circle intersection test
            // P: hit point
            // ray: P = O + tV
            // circle: (P-C)dot(P-C)-r^2 = 0
            // substituting to solve for t gives a quadratic equation:
            // a = VdotV
            // b = 2(O-C)dotV
            // c = (O-C)dot(O-C)-r^2
            // if the discriminant is negative, miss (no solution for P)
            // otherwise, if both roots are positive, hit

            double a = rayDir.Dot(rayDir);
            double b = ((rayOrig - center) * 2.0).Dot(rayDir);
            double c = (rayOrig - center).Dot(rayOrig - center) - r * r;

            // discriminant
            double disc = b * b - 4.0 * a * c;

            if (disc < 0.0)
                return false;

            // find the signs of the roots
            // technically we should also divide by 2a
            // but all we care about is the sign, not the magnitude
            double root1 = -b + Math.Sqrt(disc);
            double root2 = -b - Math.Sqrt(disc);

            return (root1 > 0.0 && root2 > 0.0);
        }

        /// <summary>
        /// Returns true if the location with padding will intersect any of the walls, false otherwise
        /// </summary>
        /// <param name="location"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        private bool CheckForWallCollision(Vector2D location, int padding) {

            foreach (Wall w in world.Walls.Values) {

                // Top leftmost units center point
                w.GetPoints(out double topLeftX, out double topLeftY);
                w.GetSecondPoints(out double bottomRightX, out double bottomRightY);

                // check if the tank is within the walls x value range
                if (location.GetX() > (topLeftX - (Constants.WALLWIDTH / 2) - (padding)) && location.GetX() < (bottomRightX + (Constants.WALLWIDTH / 2) + (padding)))
                    // check if the tank is within the walls y value range, and return true if so
                    if (location.GetY() > (topLeftY - (Constants.WALLWIDTH / 2) - (padding)) && location.GetY() < (bottomRightY + (Constants.WALLWIDTH / 2) + (padding)))
                        return true;
            }
            // no walls collide with the tank
            return false;
        }

        /// <summary>
        /// Checks if the given point will collide with a circular approximation of any tank and if so sets the out param to that tank
        /// </summary>
        /// <param name="location"></param>
        /// <returns></returns>
        private bool CheckForTankCollision(Projectile proj, out Tank t) {

            foreach (Tank tank in world.Players.Values) {
                // Ignore the tank that owns the projectile
                if (proj.owner == tank.id || tank.hitPoints <= 0)
                    continue;
                // Check for collision approximating the tank as a circle
                if ((proj.location - tank.location).Length() < Constants.TANKSIZE / 2) {
                    t = tank;
                    return true;
                }
            }
            t = new Tank();
            return false;
        }

        /// <summary>
        /// Pick a random location with no wall collision for the tank and set its hitpoints to max
        /// </summary>
        /// <param name="t"></param>
        private void spawnTank(Tank t) {
            // Full Heal
            t.hitPoints = Constants.MaxHP;

            // Pick two random points
            Random R = new Random();
            double x = R.NextDouble() * world.UniverseSize - world.UniverseSize / 2;
            double y = R.NextDouble() * world.UniverseSize - world.UniverseSize / 2;
            Vector2D location = new Vector2D(x, y);


            // Shuffle location until a non-collision is found
            while (CheckForWallCollision(location, Constants.TANKSIZE / 2)) {
                x = R.NextDouble() * world.UniverseSize - world.UniverseSize / 2;
                y = R.NextDouble() * world.UniverseSize - world.UniverseSize / 2;
                location = new Vector2D(x, y);
            }
            t.location = location;

        }

        /// <summary>
        /// Creates a powerup in a random location that does not collide with walls
        /// </summary>
        private void spawnPowerup() {

            // Mark what frame this powerup is being added on
            Random R = new Random();
            world.CanAddPowerupFrame = Frame + R.Next(Constants.POWERUPDELAY);
            world.ActivePowerups++;

            // Generate a random location
            double x = R.NextDouble() * world.UniverseSize - world.UniverseSize / 2;
            double y = R.NextDouble() * world.UniverseSize - world.UniverseSize / 2;
            Vector2D location = new Vector2D(x, y);

            // Shuffle location until a non-collision is found
            while (CheckForWallCollision(location, Constants.POWERUPOUTER / 2)) {
                x = R.NextDouble() * world.UniverseSize - world.UniverseSize / 2;
                y = R.NextDouble() * world.UniverseSize - world.UniverseSize / 2;
                location = new Vector2D(x, y);
            }
            // Create the new powerup in the world
            Powerup pow = new Powerup(location);
            world.Powerups.Add(pow.id, pow);
        }

        /// <summary>
        /// Read all the xml Settings and update the world and constants class accordingly
        /// </summary>
        /// <param name="FilePath"></param>
        private void ReadSettings(string FilePath) {
            try {
                using (XmlReader reader = XmlReader.Create(FilePath)) {
                    int size = 0;
                    int respawnRate = 0;
                    int framesPerShot = 0;
                    int id = 1;
                    int x = 0;
                    int y = 0;
                    Vector2D p1 = null;
                    HashSet<Wall> walls = new HashSet<Wall>();
                    //Scans through all the nodes in XML file
                    while (reader.Read()) {
                        if (reader.IsStartElement()) {
                            switch (reader.Name) {
                                case "x":
                                    reader.Read();
                                    int.TryParse(reader.Value, out x);
                                    y = 0;
                                    break;

                                case "y":
                                    reader.Read();
                                    int.TryParse(reader.Value, out y);
                                    if (p1 == null) {
                                        p1 = new Vector2D(x, y);
                                    }
                                    // If p1 already exists then we have two points and can add a wall
                                    else {
                                        Wall w = new Wall(p1, new Vector2D(x, y), id);
                                        id++;
                                        walls.Add(w);
                                        p1 = null;
                                    }
                                    break;

                                // Update constants class using any settings present in the XML file
                                case "UniverseSize":
                                    reader.Read();
                                    int.TryParse(reader.Value, out size);
                                    break;

                                case "MSPerFrame":
                                    reader.Read();
                                    int.TryParse(reader.Value, out this.MSPerFrame);
                                    break;

                                case "FramesPerShot":
                                    reader.Read();
                                    int.TryParse(reader.Value, out framesPerShot);
                                    break;

                                case "RespawnRate":
                                    reader.Read();
                                    int.TryParse(reader.Value, out respawnRate);
                                    break;

                                case "BeamCoolDown":
                                    reader.Read();
                                    int.TryParse(reader.Value, out Constants.BEAMCOOLDOWN);
                                    break;

                                case "MaxHP":
                                    reader.Read();
                                    int.TryParse(reader.Value, out Constants.MaxHP);
                                    break;

                                case "PowerupDelay":
                                    reader.Read();
                                    int.TryParse(reader.Value, out Constants.POWERUPDELAY);
                                    break;

                                case "MaxPowerups":
                                    reader.Read();
                                    int.TryParse(reader.Value, out Constants.MAXPOWERUPS);
                                    break;

                                case "ProjectileSpeed":
                                    reader.Read();
                                    int.TryParse(reader.Value, out Constants.PROJECTILESPEED);
                                    break;

                                case "TankSpeed":
                                    reader.Read();
                                    int.TryParse(reader.Value, out Constants.TANKSPEED);
                                    break;

                                case "MysteryPowerups":
                                    reader.Read();
                                    int.TryParse(reader.Value, out Constants.MYSTERYPOWERUPS);
                                    break;

                                case "EnhancedProjectileSpeed":
                                    reader.Read();
                                    int.TryParse(reader.Value, out Constants.ENHANCEDPROJECTILESPEED);
                                    break;

                                case "EnhancedTankSpeed":
                                    reader.Read();
                                    int.TryParse(reader.Value, out Constants.ENHANCEDTANKSPEED);
                                    break;

                                case "EnhancedProjectileTime":
                                    reader.Read();
                                    int.TryParse(reader.Value, out Constants.ENHANCEDPROJECTILETIME);
                                    break;

                                case "EnhancedTankSpeedTime":
                                    reader.Read();
                                    int.TryParse(reader.Value, out Constants.ENHANCEDTANKSPEEDTIME);
                                    break;

                                case "EnhancedFireRateTime":
                                    reader.Read();
                                    int.TryParse(reader.Value, out Constants.FASTERFIRERATETIME);
                                    break;
                            }
                        }
                    }

                    // Create the new world and set up all the walls
                    world = new World(size, framesPerShot, respawnRate);
                    foreach (Wall w in walls) {
                        world.setWall(w);
                        w.Orient();
                    }
                }
            }
            catch {
                Console.WriteLine("Server encountered an error.");
            }
        }

        /// <summary>
        /// Starts the server and sets up the walls
        /// </summary>
        private void StartServer() {

            clients = new Dictionary<SocketState, Tank>();
            // start accepting clients
            theServer = Networking.StartServer(HandleNewClient, 11000);
        }

        /// <summary>
        /// When a new client connects indicates a new client has joined in the console and prepares to receive its name
        /// </summary>
        /// <param name="state"></param>
        private void HandleNewClient(SocketState state) {
            if (state.ErrorOccured)
                return;

            // Signal a client has joined
            Console.WriteLine("Client: " + state.ID + " joined.");
            state.OnNetworkAction = HandlePlayerName;

            Networking.GetData(state);
        }

        /// <summary>
        /// Gets the player's name and sends startup info
        /// </summary>
        /// <param name="s"></param>
        private void HandlePlayerName(SocketState s) {
            string data = s.GetData();
            string[] parts = Regex.Split(data, @"(?<=[\n])");

            // Check if a full message has been received
            if (parts.Length > 1) {

                // Generate a new tank for the client
                Tank t = new Tank((int)s.ID, parts[0].Substring(0, parts[0].Length - 1), new TankWars.Vector2D(50, 50));
                spawnTank(t);

                lock (world) {

                    // Add the tank to the world
                    world.setTankData(t);
                    s.RemoveData(0, parts[0].Length);

                    //Send the player's id
                    Networking.Send(s.TheSocket, s.ID + "\n");

                    // Send world size
                    Networking.Send(s.TheSocket, world.UniverseSize + "\n");

                    // Send all the walls
                    foreach (Wall w in world.Walls.Values) {
                        Networking.Send(s.TheSocket, JsonConvert.SerializeObject(w) + '\n');
                    }
                }

                // Add the tank to the dictionary so it can start receiving frames
                lock (clients) {
                    clients.Add(s, t);
                }
                s.OnNetworkAction = HandleClientCommands;
            }
            Networking.GetData(s);
        }

        /// <summary>
        /// Handles all client sent commands and updates their tank accordingly so that it can be processed on the next frame
        /// </summary>
        /// <param name="s"></param>
        private void HandleClientCommands(SocketState s) {

            // Handle errors
            if (s.ErrorOccured) {
                Console.WriteLine("Client: " + s.ID + " disconnected.");
                lock (world) {
                    world.Players[(int)s.ID].disconnected = true;
                }
                return;
            }

            string data = s.GetData();
            string[] parts = Regex.Split(data, @"(?<=[\n])");

            // Try to deserialize messages as valid commands
            if (parts.Length > 2) {
                try {
                    CommandControl cc = JsonConvert.DeserializeObject<CommandControl>(parts[parts.Length - 2]);

                    // Remove all processed data
                    for (int i = 0; i < parts.Length - 1; i++) {
                        s.RemoveData(0, parts[i].Length);
                    }

                    // Update the client's command control object
                    clients[s].commandControl = cc;
                }
                // Disconnect client if invalid commands are received
                catch {
                    Console.WriteLine("Client: " + s.ID + " disconnected.");
                    lock (world) {
                        world.Players[(int)s.ID].disconnected = true;
                    }
                    return;
                }
            }
            // loop
            Networking.GetData(s);
        }
    }
}
