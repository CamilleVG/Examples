﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace View {
    public partial class Form1 : Form {

        GameController.GameController controller;

        public Form1() {
            InitializeComponent();
            controller = new GameController.GameController();

            // register handlers for the controller's events
            controller.MessagesArrived += UpdateView;
            controller.Error += ShowError;
            controller.Connected += HandleConnected;
        }


        private void ConnectButton_Click(object sender, EventArgs e) {

            Console.WriteLine("Connect buttong was clicked.");
            if (ServerTextBox.Text == "") {
                MessageBox.Show("Please enter a server address");
                return;
            }

            if (NameTextBox.Text == "") {
                MessageBox.Show("Please enter a valid name");
                return;
            }

            // Disable the controls and try to connect
            ConnectButton.Enabled = false;
            ServerTextBox.Enabled = false;
            NameTextBox.Enabled = false;

            controller.Connect(ServerTextBox.Text);

        }
        private void HandleConnected() {
            // Just print a message saying we connected
            Console.WriteLine("Connected to server");
            controller.Send(NameTextBox.Text);
        }

        /// <summary>
        /// Method called when JSON is received from the server
        /// </summary>
        /// <param name="messages"></param>
        private void UpdateView(IEnumerable<string> messages) {
            foreach (string m in messages)
                Console.WriteLine(m);
        }


        private void ShowError(string err) {

            MessageBox.Show(err);
            this.Invoke(new MethodInvoker(() => { ConnectButton.Enabled = true; ServerTextBox.Enabled = true; NameTextBox.Enabled = true; }));

        }


    }
}

