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

        /// <summary>
        /// State representing the connection with the server
        /// </summary>
        SocketState theServer = null;

        private CommandControl commandControl;

        private int userID = -1;
        private Vector2D lastUserLocation = new Vector2D(0,0);

        /// <summary>
        /// Model of the game
        /// </summary>
        World world;

        // Used to provide the world to the view
        public World GetWorld() {
            return world;
        }
        public double GetPlayerX()
        {
            return lastUserLocation.GetX();
        }
        public double GetPlayerY()
        {
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
                // inform the view
                Error("Error connecting to server");
                return;
            }

            theServer = state;
            //world = new World(worldSize);


            // inform the view, which will call send with the player's name
            Connected();


            // Start an event loop to receive messages from the server
            state.OnNetworkAction = HandleUserID;
            Networking.GetData(state);
        }

        public void UpdateMousePosition(int x, int y)
        {
            
        }

        /// <summary>
        /// Sends a message to the server fromt this client
        /// </summary>
        /// <param name="text"></param>
        public void Send(string text) {
            Networking.Send(theServer.TheSocket, text + "\n");
        }

        private void HandleUserID(SocketState state) {
            if (state.ErrorOccured) {
                // inform the view
                Error("Lost connection to server");
                return;
            }

            string id = getNextFullMessage(state);
            
            if (id != "") {

                if (!(Int32.TryParse(id, out userID))) {
                    Error("First message sent be server was not the players ID");
                    return;
                }
                state.OnNetworkAction = HandleWorldSize;
            }
            Console.WriteLine("Recieved User ID:" + userID);
            Networking.GetData(state);
        }

        private void HandleWorldSize(SocketState state) {
            if (state.ErrorOccured) {
                // inform the view
                Error("Lost connection to server");
                return;
            }

            string s = getNextFullMessage(state);
            if (s != "") {
                int worldSize = -1;
                if (!(Int32.TryParse(s, out worldSize))) {
                    worldSize = -1;
                    Error("First message sent be server was not the players ID");
                    return;
                }
                world = new World(worldSize);
                state.OnNetworkAction = HandleWalls;
            }
            Networking.GetData(state);
        }
        private void HandleWalls(SocketState state)
        {
            if (state.ErrorOccured)
            {
                // inform the view
                Error("Lost connection to server");
                return;
            }

            //if all the walls have been sent and now a new object is sent (that is not a wall), the client can now send commands

            string p = getNextFullMessage(state);
            if (p != "")
            {
                JObject obj = JObject.Parse(p);
                JToken token;


                if ((token = obj["wall"]) != null)
                {
                    world.setWall(JsonConvert.DeserializeObject<Wall>(p));
                }
                else
                {
                    commandControl = new CommandControl();
                    AllowInput();
                    state.OnNetworkAction = ReceiveMessage;
                }
            }
            Networking.GetData(state);
        }

        public void HandleMouseRequest()
        {
            //throw new NotImplementedException();
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
            //string nextMsg = getNextFullMessage(state);
            //if (nextMsg != "")
            //    parseMessage(nextMsg);
            ProcessMessages(state);
            newInformation();

            // Continue the event loop
            // state.OnNetworkAction has not been changed, 
            // so this same method (ReceiveMessage) 
            // will be invoked when more data arrives
            Networking.GetData(state);
        }

        /// <summary>
        /// Process any buffered messages separated by '\n'
        /// Then inform the view
        /// </summary>
        /// <param name="state"></param>
        private void ProcessMessages(SocketState state)
        {
            string totalData = state.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            // Loop until we have processed all messages.
            // We may have received more than one.

            foreach (string p in parts)
            {
                // Ignore empty strings added by the regex splitter
                if (p.Length == 0)
                    continue;

                // The regex splitter will include the last string even if it doesn't end with a '\n',
                // So we need to ignore it if this happens. 
                if (p[p.Length - 1] != '\n')
                    break;

                parseMessage(p);

                // Then remove it from the SocketState's growable buffer
                state.RemoveData(0, p.Length);
            }

            // inform the view
            //MessagesArrived(newMessages);
        }

        private string getNextFullMessage(SocketState state) {
            string totalData = state.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");
            // Loop until we have processed all messages.
            // We may have received more than one.

            //foreach (string p in parts) {
            // Ignore empty strings added by the regex splitter
            string p = parts[0];
            if (p.Length == 0)
                //continue;
                return "";

            // The regex splitter will include the last string even if it doesn't end with a '\n',
            // So we need to ignore it if this happens. 
            if (p[p.Length - 1] != '\n')
                return "";

            // Then remove it from the SocketState's growable buffer
            state.RemoveData(0, p.Length);
            //}
            return p;
            // inform the view
            //MessagesArrived(newMessages);

            //Send(JsonConvert.SerializeObject(commandControl));
        }

        /// <summary>
        /// Will use received data to update the model
        /// </summary>
        /// <param name="p"></param>
        private void parseMessage(string p) {
            //Console.WriteLine("Parsing Message " + p); //////////////////////////////////////////////////////////


            JObject obj = JObject.Parse(p);
            JToken token;

            if ((token = obj["tank"]) != null) {
                world.setTankData(JsonConvert.DeserializeObject<Tank>(p));
                if (world.Players.ContainsKey(userID))
                {
                    lastUserLocation = world.Players[userID].Location;
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

        /// <summary>
        /// Closes the connection with the server
        /// </summary>
        public void Close() {
            theServer?.TheSocket.Close();
        }
        public void SendMovement()
        {
                Console.WriteLine(JsonConvert.SerializeObject(commandControl));
                Send(JsonConvert.SerializeObject(commandControl));
        }

        public void SendMoveRequest(string code) {
            //Console.WriteLine("Why isnt it sending?????");
            switch (code) {
                case "W":
                    commandControl.addCommand("up");
                    break;
                case "A":
                    commandControl.addCommand("left");
                    break;
                case "S":
                    commandControl.addCommand("down");
                    break;
                case "D":
                    commandControl.addCommand("right");
                    break;
            }
            
        }

        public void CancelMoveRequest(string code) {
            switch (code) {
                case "W":
                    commandControl.removeCommand("up");
                    break;
                case "A":
                    commandControl.removeCommand("left");
                    break;
                case "S":
                    commandControl.removeCommand("down");
                    break;
                case "D":
                    commandControl.removeCommand("right");
                    break;
            }
        }
    }
}

