using Model;
using NetworkUtil;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using TankWars;

namespace Server {
    class Server {

        // Dictionary used to store all client ids and their connections
        Dictionary<SocketState, Tank> clients;

        // The server's TcpListener
        TcpListener theServer;

        World world;


        static void Main(string[] args) {

            // Make a new server
            Server server = new Server();
            server.StartServer();

            // Set everything up

            Console.WriteLine("Reached main");
            // Start an event loop on its own thread for a) accepting connections b) Sending frames

            // Keep the server open
            Console.Read();

        }

        private void StartServer() {
            world = new World(2000); // READ WORLD SIZE
            SetupWalls();
            clients = new Dictionary<SocketState, Tank>();
            Console.WriteLine("Reached Start Server");
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

            Console.WriteLine("Reached HandleNewClient");
            state.OnNetworkAction = HandlePlayerName;

            Networking.GetData(state);
        }


        private void HandlePlayerName(SocketState s) {
            string data = s.GetData();
            string[] parts = Regex.Split(data, @"(?<=[\n])");

            Console.WriteLine("Reached handlePlayerName");

            if (parts.Length > 1) {
                // ASK if name can be more than 16

                Tank t = new Tank((int)s.ID, parts[0], new TankWars.Vector2D()); // Randomize starting position
                Networking.Send(s.TheSocket, s.ID + "\n");

                s.RemoveData(0, parts[0].Length);

                // Send world size
                Networking.Send(s.TheSocket, world.UniverseSize + "\n");

                // Send all the walls
                foreach (Wall w in world.Walls.Values) {
                    Networking.Send(s.TheSocket, JsonConvert.SerializeObject(w) + '\n');
                }

                Console.WriteLine("Reached wall sending");

                // TEST WALL SEND //
                Wall x = new Wall(new Vector2D(-50, 50), new Vector2D(-50, -100), 0);
                Networking.Send(s.TheSocket, JsonConvert.SerializeObject(x) + '\n');
                Networking.Send(s.TheSocket, JsonConvert.SerializeObject(t) + '\n');
                for (int i = 0; i < 1000; i++) {
                    Console.WriteLine("Sending the tank again");
                    System.Threading.Thread.Sleep(40);
                    Networking.Send(s.TheSocket, JsonConvert.SerializeObject(t) + '\n');
                }

                ////////////////////
                // Add the tank to the dictionary so it can start receiving frames
                clients.Add(s, t);
                s.OnNetworkAction = handleClientCommands;
            }

            Networking.GetData(s);
        }

        // Consider sending the world size and walls separately
        private string getStartupInfo() {
            return "2000\n";
        }

        private void handleClientCommands(SocketState s) {

        }


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
