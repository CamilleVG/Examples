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


        private void updateWorld() {
            Frame++;
            HashSet<Beam> BeamsToSend = new HashSet<Beam>();
            lock (world) {

                // Update all projectiles
                HashSet<Projectile> projToRemove = new HashSet<Projectile>();
                foreach (Projectile proj in world.Projectiles.Values) {
                    int speed = Constants.PROJECTILESPEED;
                    if (proj.Enhanced)
                    {
                        speed = Constants.ENHANCEDPROJECTILESPEED;
                    }
                    if (!CheckForWallCollision(proj.location + (proj.orientation * speed), 0)) {

                        proj.UpdateLocation(proj.location + (proj.orientation * speed));

                        // check to see if the projectile will collide with any tanks and deal damage accordingly
                        if (CheckForTankCollision(proj, out Tank t)) {
                            t.hitPoints--;
                            if (proj.Enhanced && t.hitPoints>2)
                            {
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
                    else {
                        projToRemove.Add(proj);
                        proj.died = true;
                    }
                }

                // Update all tanks
                foreach (Tank t in world.Players.Values) {
                    updateTank(t, BeamsToSend);
                }

                //Adds powerups if necesary
                if ((world.ActivePowerups < Constants.MAXPOWERUPS) && (Frame >= world.CanAddPowerupFrame)) {
                    spawnPowerup();
                }

                HashSet<Powerup> PowerupsToRemove = new HashSet<Powerup>();
                foreach (Powerup p in world.Powerups.Values) {
                    if (HandlePowerupCollision(p)) {
                        PowerupsToRemove.Add(p);
                    }
                }

                HashSet<Tank> TanksToRemove = new HashSet<Tank>();
                // Identify all disconnected tanks
                foreach (Tank t in world.Players.Values) {
                    if (t.disconnected) {
                        TanksToRemove.Add(t);
                        t.hitPoints = 0;
                    }
                }

                foreach (Beam b in BeamsToSend) {
                    Vector2D checkingLocation = b.origin;
                    while (Math.Abs(checkingLocation.GetX()) < world.UniverseSize / 2 && Math.Abs(checkingLocation.GetY()) < world.UniverseSize / 2) {
                        // check each tank for a collision
                        foreach (Tank t in world.Players.Values) {
                            if (CheckBeamCollision(checkingLocation, t, b.ownerID)) {
                                t.hitPoints = 0;
                                t.died = true;
                                t.diedOnFrame = Frame;
                                world.Players[b.ownerID].score++;
                                TanksToRemove.Add(t);
                            }
                        }
                        // keep checking in a diagonal
                        checkingLocation += b.direction * (Constants.TANKSIZE / 3);
                    }

                }


                // Send the data to each client
                SendDataToAllClients(BeamsToSend);

                // Set all tanks' died properties to false
                foreach (Tank t in world.Players.Values) {
                    t.died = false;
                }

                // Remove dc'd tanks
                foreach (Tank t in TanksToRemove)
                    world.Players.Remove(t.id);

                // Remove dead projectiles
                foreach (Projectile proj in projToRemove)
                    world.Projectiles.Remove(proj.id);

                //Removes dead powerups
                foreach (Powerup pow in PowerupsToRemove) {
                    world.Powerups.Remove(pow.id);
                }

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
                            ////////////////////////
                            ///
                            if (t.id > 1)
                                Console.WriteLine(JsonConvert.SerializeObject(t));
                            //////////////////////
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

        private void updateTank(Tank t, HashSet<Beam> BeamsToSend) {

            if (t.hitPoints <= 0) {
                if (Frame >= t.diedOnFrame + world.RespawnRate) {
                    spawnTank(t);
                }
                return;
            }

            // Move each tank in the proper direction or wraparound if there will not be a collision
            int speed = Constants.TANKSPEED;
            if (t.EnhancedSpeed)
            {
                speed = Constants.ENHANCEDTANKSPEED;
                if(Frame >= t.SpeedModeFrame + Constants.SPEEDMODETIME)
                {
                    t.EnhancedSpeed = false;
                }
            }
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
                    if (Frame < t.LastShotFrame + world.FramesPerShot) {
                        break;
                    }
                    Projectile proj = new Projectile(t.tdir, t.location, t.id);
                    if (t.EnhancedProjectiles)
                    {
                        proj.Enhanced = true;
                        if (Frame >= t.ProjectileModeFrame + Constants.PROJECTILEMODETIME)
                        {
                            proj.Enhanced = false;
                            t.EnhancedProjectiles = false;
                        }
                    }
                    world.setProjData(proj);
                    t.LastShotFrame = Frame;
                    break;
                case "alt":
                    //Check if firing is allowed
                    if ((t.BeamCount > 0) && (Frame - t.LastBeamFrame > Constants.BEAMCOOLDOWN)) {
                        BeamsToSend.Add(new Beam(t.location, t.tdir, t.id));
                        t.BeamCount--;
                        t.LastBeamFrame = Frame;
                    }
                    break;
            }
        }


        /// <summary>
        /// Checks if a powerup has collided with any tank and updates the tanks beam count and powerup status accordingly
        /// </summary>
        /// <param name="pow"></param>
        /// <returns></returns>
        private bool HandlePowerupCollision(Powerup pow) {
            foreach (Tank tank in world.Players.Values) {
                if (tank.hitPoints <= 0)
                    continue;
                if ((pow.location - tank.location).Length() < Constants.TANKSIZE / 2) {
                    if (Constants.POWERUPMODE == 0)
                    {
                        tank.BeamCount++;
                        pow.died = true;
                        world.ActivePowerups--;
                        return true;
                    }
                    else //if mysery powerups are activated
                    {
                        Random r = new Random();
                        switch (r.Next(3))
                        {
                            case 0:
                                tank.BeamCount++;
                                break;
                            case 1:
                                tank.EnhancedSpeed = true;
                                tank.SpeedModeFrame = Frame;
                                break;
                            case 2:
                                tank.EnhancedProjectiles = true;
                                tank.ProjectileModeFrame = Frame;
                                break;
                        }
                        pow.died = true;
                        world.ActivePowerups--;
                        return true;
                    }
                    
                }
            }
            return false;
        }

        /// <summary>
        /// Checks if a point collides with the given tank excepting the given id
        /// </summary>
        /// <param name="loc"></param>
        /// <returns></returns>
        private bool CheckBeamCollision(Vector2D loc, Tank tank, int idOfOwner) {
            // ignore dead tanks and the tank that shot this beam
            if (tank.hitPoints <= 0 || tank.id == idOfOwner)
                return false;
            // kill any tanks caught at this spot of the beam
            if ((loc - tank.location).Length() < Constants.TANKSIZE / 2) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// returns true if the location with padding will intersect any of the walls, false otherwise
        /// </summary>
        /// <param name="location"></param>
        /// <param name="padding"></param>
        /// <returns></returns>
        private bool CheckForWallCollision(Vector2D location, int padding) {

            foreach (Wall w in world.Walls.Values) {

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
                if (proj.owner == tank.id || tank.hitPoints <= 0)
                    continue;
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
            t.hitPoints = Constants.MaxHP;
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

        private void spawnPowerup() {

            Random r = new Random();
            world.CanAddPowerupFrame = Frame + r.Next(Constants.POWERUPDELAY);
            world.ActivePowerups++;

            Random R = new Random();
            double x = R.NextDouble() * world.UniverseSize - world.UniverseSize / 2;
            double y = R.NextDouble() * world.UniverseSize - world.UniverseSize / 2;
            Vector2D location = new Vector2D(x, y);

            // Shuffle location until a non-collision is found
            while (CheckForWallCollision(location, Constants.POWERUPOUTER / 2)) {
                x = R.NextDouble() * world.UniverseSize - world.UniverseSize / 2;
                y = R.NextDouble() * world.UniverseSize - world.UniverseSize / 2;
                location = new Vector2D(x, y);
            }
            Powerup pow = new Powerup(location);

            world.Powerups.Add(pow.id, pow);
        }

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

                                case "UniverseSize":
                                    reader.Read();
                                    int.TryParse(reader.Value, out size);
                                    world = new World(size);
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
                                case "PowerupMode":
                                    reader.Read();
                                    int.TryParse(reader.Value, out Constants.POWERUPMODE);
                                    break;
                            }

                        }
                    }

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
        private void StartServer() {

            SetupWalls();
            clients = new Dictionary<SocketState, Tank>();
            // start accepting clients
            theServer = Networking.StartServer(HandleNewClient, 11000);
        }

        private void SetupWalls() {

        }

        // Handle client delegate callback passed to the networking to handle a new client connecting.
        //Change the callback for the socket state to a new method that receives the player's name, then ask for data

        private void HandleNewClient(SocketState state) {
            if (state.ErrorOccured)
                return;

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

            if (parts.Length > 1) {
                //// ASK if name can be more than 16

                Tank t = new Tank((int)s.ID, parts[0].Substring(0, parts[0].Length - 1), new TankWars.Vector2D(50, 50)); // Randomize starting position TODOTODO
                spawnTank(t);

                lock (world) {

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
                s.OnNetworkAction = handleClientCommands;
            }

            Networking.GetData(s);
        }

        private void handleClientCommands(SocketState s) {

            // Ensure it will be a command control TODOTDO
            if (s.ErrorOccured) {
                Console.WriteLine("Client: " + s.ID + " disconnected.");
                lock (world) {
                    world.Players[(int)s.ID].disconnected = true;
                }
                return;
            }

            string data = s.GetData();
            string[] parts = Regex.Split(data, @"(?<=[\n])");

            if (parts.Length > 2) {
                CommandControl cc = JsonConvert.DeserializeObject<CommandControl>(parts[parts.Length - 2]);

                for (int i = 0; i < parts.Length - 1; i++) {
                    s.RemoveData(0, parts[i].Length);
                }

                clients[s].commandControl = cc;
            }

            Networking.GetData(s);
        }
    }
}
