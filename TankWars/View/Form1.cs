using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace View {
    public partial class Form1 : Form {

        GameController.GameController controller;
        // This simple form only has two components
        DrawingPanel drawingPanel;
        Button startButton;
        Label nameLabel;
        TextBox nameText;

        private const int viewSize = 500;
        private const int menuSize = 40;
        public Form1() {
            InitializeComponent();
            controller = new GameController.GameController();
            

        // register handlers for the controller's events
        controller.MessagesArrived += UpdateView;
            controller.Error += ShowError;
            controller.Connected += HandleConnected;
            controller.AllowInput += startListeningToKeys;
            this.KeyDown += HandleKeyDown;
            this.KeyUp += HandleKeyUp;
            drawingPanel.MouseDown += HandleMouseDown;
            drawingPanel.MouseUp += HandleMouseUp;
        }

        private void HandleMouseUp(object sender, MouseEventArgs e)
        {
            throw new NotImplementedException();
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
        private void UpdateView(IEnumerable<string> messages) {

            //foreach (string m in messages)
            //    Console.WriteLine(m);
            Invoke(new MethodInvoker(() => this.Invalidate(true)));
            System.Drawing.Point mouse = System.Windows.Forms.Cursor.Position;
            controller.UpdateMousePosition(mouse.X, mouse.Y);
        }


        private void ShowError(string err) {

            MessageBox.Show(err);
            this.Invoke(new MethodInvoker(() => { ConnectButton.Enabled = true; ServerTextBox.Enabled = true; NameTextBox.Enabled = true; }));

        }

        private void startListeningToKeys() {
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
            Console.WriteLine(e.KeyCode.ToString());
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
        private void HandleMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                controller.HandleMouseRequest();
        }
    }
}


