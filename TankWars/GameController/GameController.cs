// Authors: Preston Powell and Camille Van Ginkel
// PS8 code for Daniel Kopta's CS 3500 class at the University of Utah Fall 2020
// Version 1.0.3, Nov 2020

using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text.RegularExpressions;
using NetworkUtil;
using Newtonsoft.Json;
using Model;
using Newtonsoft.Json.Linq;
using TankWars;

using System.Numerics;

namespace GameController {

    /// <summary>
    /// Sends user commands to the server and updates the model using messages received from the server.
    /// </summary>
    public class GameController {

        // Controller events that the view can subscribe to
        public delegate void updateReceived();
        public event updateReceived newInformation;

        public delegate void ConnectedHandler();
        public event ConnectedHandler Connected;

        public delegate void ErrorHandler(string err);
        public event ErrorHandler Error;

        public delegate void WallsReceivedHandler();
        public event WallsReceivedHandler AllowInput;

        public delegate void AnimationRecieved(Object o);
        public event AnimationRecieved TriggerAnimations;

        // State representing the connection with the server
        private SocketState theServer = null;

        // Stores the state of the user's commands
        private CommandControl commandControl;

        // Stores the user's ID
        private int userID;

        private Vector2D lastUserLocation = new Vector2D(0, 0);

        /// <summary>
        /// Model of the game
        /// </summary>
        World world;

        /// <summary>
        /// Returns this controller's world
        /// </summary>
        public World GetWorld() {
            return world;

        }
        /// <summary>
        /// Returns the player's most recently recorded X position
        /// </summary>
        /// <returns></returns>
        public double GetPlayerX() {
            return lastUserLocation.GetX();
        }

        /// <summary>
        /// Returns the player's most recently recorded Y position
        /// </summary>
        /// <returns></returns>
        public double GetPlayerY() {
            return lastUserLocation.GetY();
        }

        /// <summary>
        /// Begins the process of connecting to the server
        /// </summary>
        /// <param name="addr"></param>
        public void Connect(string addr) {
            Networking.ConnectToServer(OnConnect, addr, 11000);
        }


        /// <summary>
        /// Method to be invoked by the networking library when a connection is made
        /// </summary>
        /// <param name="state"></param>
        private void OnConnect(SocketState state) {
            if (state.ErrorOccured) {
                // inform the view of any error's
                Error("Error connecting to server");
                return;
            }

            theServer = state;

            // inform the view that we have connected, which will call send with the player's name
            Connected();

            // Start an event loop to receive messages from the server, with the first message anticipated to be the user ID
            state.OnNetworkAction = HandleIDandWorldSize;
            Networking.GetData(state);
        }

        /// <summary>
        /// Sends a message to the server fromt this client
        /// </summary>
        public void Send(string text) {
            Networking.Send(theServer.TheSocket, text + '\n');
        }

        private void HandleIDandWorldSize(SocketState state) {
            if (state.ErrorOccured) {
                // inform the view
                Error("Lost connection to server");
                return;
            }
            string totalData = state.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            // Checks to ensure both the userID and the worldSize have been received
            if (parts.Length < 3) {
                Networking.GetData(state);
                return;
            }

            //Removes the used data from the buffer
            state.RemoveData(0, parts[0].Length + parts[1].Length);

            // try to parse the first message as the user's ID
            string id = parts[0];
            if (!(Int32.TryParse(id, out userID))) {
                Error("First message sent be server was not the players ID");
                return;
            }

            // try to parse the second message as the world size
            string size = parts[1];
            int worldSize = -1;
            if (!(Int32.TryParse(size, out worldSize))) {
                worldSize = -1;
                Error("Failed to receive wallsize");
                return;
            }

            //creates a new world
            world = new World(worldSize);
            world.AddAnimation += HandleAnimation;

            // prepares to handle the next messages as walls
            state.OnNetworkAction = HandleWalls;

            Networking.GetData(state);
        }

        /// <summary>
        /// Queues animation in the view
        /// </summary>
        /// <param name="o"></param>
        private void HandleAnimation(Object o) {
            TriggerAnimations(o);
        }
        private void HandleWalls(SocketState state) {
            if (state.ErrorOccured) {
                // inform the view
                Error("Lost connection to server");
                return;
            }

            string totalData = state.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            // Add all wall json
            foreach (string s in parts) {

                if (s != "") {

                    if (s[s.Length - 1] != '\n')
                        break;

                    JObject obj = JObject.Parse(s);
                    JToken token;

                    // if the json is a valid wall, set it
                    if ((token = obj["wall"]) != null) {
                        lock (world) {
                            world.setWall(JsonConvert.DeserializeObject<Wall>(s));
                        }
                    }

                    // if all the walls have been sent and now a new object is sent (that is not a wall), the client can now send commands
                    else {
                        commandControl = new CommandControl();
                        AllowInput();
                        state.OnNetworkAction = ReceiveMessage;
                    }
                }
            }
            Networking.GetData(state);
        }


        /// <summary>
        /// Method to be invoked by the networking library when 
        /// data is available
        /// </summary>
        /// <param name="state"></param>
        private void ReceiveMessage(SocketState state) {
            if (state.ErrorOccured) {
                // inform the view
                Error("Lost connection to server");
                return;
            }

            ProcessMessages(state);

            // Inform the view that the model will be updated
            newInformation();

            // Inform the server about the user's control commands
            Send(JsonConvert.SerializeObject(commandControl));

            // Continue the event loop
            Networking.GetData(state);
        }

        /// <summary>
        /// Process any buffered messages separated by '\n'
        /// Then inform the view
        /// </summary>
        /// <param name="state"></param>
        private void ProcessMessages(SocketState state) {
            string totalData = state.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            // Loop until we have processed all messages.
            foreach (string p in parts) {

                // Ignore empty strings added by the regex splitter
                if (p.Length == 0)
                    continue;

                // Ignore incomplete messages
                if (p[p.Length - 1] != '\n')
                    break;

                // If the string contains text, try to parse it as json
                ParseMessage(p);

                // Then remove it from the SocketState's growable buffer
                state.RemoveData(0, p.Length);
            }
        }

        /// <summary>
        /// Will use received data to update the model
        /// </summary>
        /// <param name="p"></param>
        private void ParseMessage(string p) {

            // Assume the message is json
            JObject obj = JObject.Parse(p);
            JToken token;

            // lock the world to prevent it from updating while the view is drawing
            lock (world) {

                // Handle each valid json type
                if ((token = obj["tank"]) != null) {
                    world.setTankData(JsonConvert.DeserializeObject<Tank>(p));
                    if (world.Players.ContainsKey(userID)) {
                        lastUserLocation = world.Players[userID].location;
                    }
                }
                else if ((token = obj["proj"]) != null) {
                    world.setProjData(JsonConvert.DeserializeObject<Projectile>(p));
                }
                else if ((token = obj["power"]) != null) {
                    world.setPowerupData(JsonConvert.DeserializeObject<Powerup>(p));
                }
                else if ((token = obj["beam"]) != null) {
                    world.setBeamData(JsonConvert.DeserializeObject<Beam>(p));
                }
            }
        }

        /// <summary>
        /// Closes the connection with the server
        /// </summary>
        public void Close() {
            theServer?.TheSocket.Close();
        }

        /// <summary>
        /// Updates the commandControl's stored turret direction
        /// </summary>
        public void UpdateTDir(Vector2D angle) {
            angle.Normalize();
            commandControl.tDirection = angle;
        }

        /// <summary>
        /// Updates the comandControl's firing state
        /// </summary>
        /// <param name="mouse"></param>
        public void MousePressed(string mouse) {
            if (mouse == "left")
                commandControl.fire = "main";
            if (mouse == "right")
                commandControl.fire = "alt";
        }

        /// <summary>
        /// Set the commandControl's firing state to none
        /// </summary>
        public void mouseReleased() {
            commandControl.fire = "none";
        }

        /// <summary>
        /// Updates the commandControl's movement list
        /// </summary>
        /// <param name="code"></param>
        public void UpdateMoveCommand(string code) {
            switch (code) {
                case "W":
                    commandControl.AddCommand("up");
                    break;
                case "A":
                    commandControl.AddCommand("left");
                    break;
                case "S":
                    commandControl.AddCommand("down");
                    break;
                case "D":
                    commandControl.AddCommand("right");
                    break;
            }

        }

        /// <summary>
        /// Cancels the specified movement command from the commandControl
        /// </summary>
        /// <param name="code"></param>
        public void CancelMoveRequest(string code) {
            switch (code) {
                case "W":
                    commandControl.RemoveCommand("up");
                    break;
                case "A":
                    commandControl.RemoveCommand("left");
                    break;
                case "S":
                    commandControl.RemoveCommand("down");
                    break;
                case "D":
                    commandControl.RemoveCommand("right");
                    break;
            }
        }
    }
}

