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

namespace GameController {

    public class GameController {
        // Controller events that the view can subscribe to
        public delegate void MessageHandler(IEnumerable<string> messages);
        public event MessageHandler MessagesArrived;

        public delegate void ConnectedHandler();
        public event ConnectedHandler Connected;

        public delegate void ErrorHandler(string err);
        public event ErrorHandler Error;

        /// <summary>
        /// State representing the connection with the server
        /// </summary>
        SocketState theServer = null;

        /// <summary>
        /// Model of the game
        /// </summary>
        World world;

        public GameController(int worldSize) {
            world = new Model.World(worldSize);
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

            // inform the view
            Connected();


            // Start an event loop to receive messages from the server
            state.OnNetworkAction = ReceiveMessage;
            Networking.GetData(state);
        }

        /// <summary>
        /// Sends a message to the server fromt this client
        /// </summary>
        /// <param name="text"></param>
        public void Send(string text) {
            Networking.Send(theServer.TheSocket, text);
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
        private void ProcessMessages(SocketState state) {
            string totalData = state.GetData();
            string[] parts = Regex.Split(totalData, @"(?<=[\n])");

            // Loop until we have processed all messages.
            // We may have received more than one.

            List<string> newMessages = new List<string>();

            foreach (string p in parts) {
                // Ignore empty strings added by the regex splitter
                if (p.Length == 0)
                    continue;

                // The regex splitter will include the last string even if it doesn't end with a '\n',
                // So we need to ignore it if this happens. 
                if (p[p.Length - 1] != '\n')
                    break;

                // build a list of messages to send to the view
                /*Messages.Add(p);*/
                parseMessage(p);

                // Then remove it from the SocketState's growable buffer
                state.RemoveData(0, p.Length);
            }

            // inform the view
            //MessagesArrived(newMessages);

        }

        /// <summary>
        /// Will use received data to update the model
        /// </summary>
        /// <param name="p"></param>
        private void parseMessage(string p) {

            JObject obj = JObject.Parse(p);
            JToken token = obj["fieldName"];

            if (null != token)
                switch (token.ToString()) {

                    case "tank":
                        world.setTankData(JsonConvert.DeserializeObject<Tank>(p));
                        break;

                    case "proj":
                        world.setProjData(JsonConvert.DeserializeObject<Projectile>(p));
                        break;

                    case "wall":
                        world.setWall(JsonConvert.DeserializeObject<Wall>(p));
                        break;

                    case "beam":
                        world.setBeamData(JsonConvert.DeserializeObject<Beam>(p));
                        break;

                    case "power":
                        world.setPowerupData(JsonConvert.DeserializeObject<Powerup>(p));
                        break;
                }

            JsonConvert.DeserializeObject<Tank>(p);
            throw new NotImplementedException();
        }

        /// <summary>
        /// Closes the connection with the server
        /// </summary>
        public void Close() {
            theServer?.TheSocket.Close();
        }

        /// <summary>
        /// Send a message to the server
        /// </summary>
        /// <param name="message"></param>
        public void MessageEntered(string message) {
            if (theServer != null)
                Networking.Send(theServer.TheSocket, message + "\n");
        }
    }
}

