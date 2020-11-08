using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace View
{
    public partial class View: UserControl
    {
        GameController.GameController controller;
        public View()
        {
            InitializeComponent();
            controller = new GameController.GameController();
            Console.WriteLine("Line 20 whooo!");

            // register handlers for the controller's events
            //controller.MessagesArrived += DisplayMessages;
            //controller.Error += ShowError;
            controller.Connected += HandleConnected;
        }

        private void HandleConnected()
        {
            // Just print a message saying we connected
            Console.WriteLine("Connected to server");


        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Connect buttong was clicked.");
            if (ServerTextBox.Text == "")
            {
                MessageBox.Show("Please enter a server address");
                return;
            }

            if (NameTextBox.Text == "")
            {
                MessageBox.Show("Please enter a valid name");
                return;
            }

            // Disable the controls and try to connect
            ConnectButton.Enabled = false;
            ServerTextBox.Enabled = false;
            NameTextBox.Enabled = false;

            controller.Connect(ServerTextBox.Text);
        }
    }
}
