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


        static void Main(string[] args) {

            // Make a new server
            Server server = new Server();
            server.ReadSettings("..\\..\\..\\..\\Resources\\Settings.xml");
            server.StartServer();

            // Run an infinite updating thread to recalculate the world every frame
            Stopwatch watch = new Stopwatch();
            while (true) {
                while (watch.ElapsedMilliseconds < server.MSPerFrame) { }
                watch.Reset();
                server.updateWorld();

            }
        }


        private void updateWorld() {

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
                            }

                        }
                    }
                    world = new World(size, framesPerShot, respawnRate);
                    foreach (Wall w in walls) {
                        world.setWall(w);
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

                Tank t = new Tank((int)s.ID, parts[0], new TankWars.Vector2D(50, 50)); // Randomize starting position TODOTODO

                s.RemoveData(0, parts[0].Length);

                //Send the player's id
                Networking.Send(s.TheSocket, s.ID + "\n");


                // Send world size
                Networking.Send(s.TheSocket, world.UniverseSize + "\n");

                // Send all the walls
                foreach (Wall w in world.Walls.Values) {
                    Networking.Send(s.TheSocket, JsonConvert.SerializeObject(w) + '\n');
                }


                // Add the tank to the dictionary so it can start receiving frames
                lock (clients) {
                    clients.Add(s, t);
                }
                s.OnNetworkAction = handleClientCommands;
            }

            // Sends a bunch of tanks to the client
            //for (int i = 0; i < 500; i++) {
            //    Networking.Send(s.TheSocket, JsonConvert.SerializeObject(clients[s]) + '\n');
            //    System.Threading.Thread.Sleep(10);
            //    Console.WriteLine("Sending tank: " + JsonConvert.SerializeObject(clients[s]));
            //}

            Networking.GetData(s);
        }

        private void handleClientCommands(SocketState s) {

            // Ensure it will be a command control TODOTDO

            string data = s.GetData();
            string[] parts = Regex.Split(data, @"(?<=[\n])");

            if (parts.Length > 2) {
                CommandControl cc = JsonConvert.DeserializeObject<CommandControl>(parts[parts.Length - 2]);

                for (int i = 0; i < parts.Length - 1; i++) {
                    s.RemoveData(0, parts[i].Length);
                }

                clients[s].commandControl = cc;
                Console.WriteLine(cc.moving);
            }
            Console.WriteLine("Reached sending a new frame");
            Networking.GetData(s);


            /* Receive player name - this is a delegate that implements the server's part of the initial handshake. Make a new Tank with 
             * the given name and a new unique ID (recommend using the SocketState's ID). Then change the callback to a method that handles 
             * command requests from the client. Then send the startup info to the client. Then add the client's socket to a list of all 
             * clients. Then ask the client for data. Note: it is important that the server sends the startup info before adding the client
             * to the list of all clients. This guarantees that the startup info is sent before any world info. Remember that the server
             * is running a loop on a separate thread that may send world info to the list of clients at any time.*/

            // Handle data from client - delegate for processing movement commands, NOTE: socket must contain player's ID in order to know
            //Who sent the movement request, this is what the ID is for in the socket state class

            //Update method invoked every iteration through the frame loop, update world then send to each client

        }
    }
}
