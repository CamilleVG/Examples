using Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Resources;

namespace View {
    public partial class Form1 : Form {

        GameController.GameController controller;
        World theWorld;
        DrawingPanel drawingPanel;

        public Form1() {

            InitializeComponent();
            ClientSize = new Size(Constants.VIEWSIZE, Constants.VIEWSIZE + Constants.MENUSIZE);

            controller = new GameController.GameController();
            // register handlers for the controller's events
            controller.newInformation += UpdateView;
            controller.Error += ShowError;
            controller.Connected += HandleConnected;
            controller.AllowInput += StartGameFunctionality;

            // Place and add the drawing panel
            drawingPanel = new DrawingPanel(controller);
            drawingPanel.Location = new Point(0, Constants.MENUSIZE);
            drawingPanel.Size = new Size(Constants.VIEWSIZE, Constants.VIEWSIZE);
            this.Controls.Add(drawingPanel);
            drawingPanel.MouseDown += HandleMouseDown;
            drawingPanel.MouseUp += HandleMouseUp;

            //register key handlers
            this.KeyDown += HandleKeyDown;
            this.KeyUp += HandleKeyUp;
        }

        private void HandleMouseUp(object sender, MouseEventArgs e) {
            //throw new NotImplementedException();
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

            if (NameTextBox.Text.Length >= 16) {
                MessageBox.Show("Name entered is too long");
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
        private void UpdateView() {
            controller.SendMovement();
            this.Invoke(new MethodInvoker(() => this.Invalidate(true)));
            //this.Invoke(new MethodInvoker(() => this.Update()));
            //System.Drawing.Point mouse = System.Windows.Forms.Cursor.Position;
            //controller.UpdateMousePosition(mouse.X, mouse.Y);
        }


        private void ShowError(string err) {

            MessageBox.Show(err);
            this.Invoke(new MethodInvoker(() => { ConnectButton.Enabled = true; ServerTextBox.Enabled = true; NameTextBox.Enabled = true; }));

        }

        private void StartGameFunctionality() {
            // Enable the global form to capture key presses
            KeyPreview = true;
        }

        /// <summary>
        /// Key down handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleKeyDown(object sender, KeyEventArgs e) {
            controller.SendMoveRequest(e.KeyCode.ToString());

            // Prevent other key handlers from running
            e.SuppressKeyPress = true;
            e.Handled = true;
        }

        /// <summary>
        /// Key up handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleKeyUp(object sender, KeyEventArgs e) {
            controller.CancelMoveRequest(e.KeyCode.ToString());

            // Prevent other key handlers from running
            Console.WriteLine(e.KeyCode.ToString() + " Released");
            e.SuppressKeyPress = true;
            e.Handled = true;
        }

        /// <summary>
        /// Handle mouse down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleMouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left)
                controller.HandleMouseRequest();
            
        }
    }

}


