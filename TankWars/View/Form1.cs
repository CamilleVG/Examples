// Authors: Preston Powell and Camille Van Ginkel
// PS8 code for Daniel Kopta's CS 3500 class at the University of Utah Fall 2020
// Version 1.0.3, Nov 2020

using Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using TankWars;

namespace View {

    /// <summary>
    /// Holds the form window in which TankWars will displayed
    /// </summary>
    public partial class Form1 : Form {

        GameController.GameController controller;
        DrawingPanel drawingPanel;

        public Form1() {

            InitializeComponent();
            // Set the desired size and background color
            ClientSize = new Size(Constants.VIEWSIZE, Constants.VIEWSIZE + Constants.MENUSIZE);
            this.BackColor = Color.Black;

            // Make a new controller
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

            //register key handlers
            this.KeyDown += HandleKeyDown;
            this.KeyUp += HandleKeyUp;
        }

        /// <summary>
        /// Informs the controller about the new angle from the center to the mouse when the mouse moves
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleMouseMoved(object sender, MouseEventArgs e) {
            Vector2D vectorFromCenter = new Vector2D((e.X - Constants.VIEWSIZE / 2), (e.Y - Constants.VIEWSIZE / 2));
            controller.UpdateTDir(vectorFromCenter);
        }

        /// <summary>
        /// Informs the controller that a mouse button has been pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleMouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left)
                controller.MousePressed("left");
            if (e.Button == MouseButtons.Right)
                controller.MousePressed("right");
        }

        /// <summary>
        /// Informs the controller that a mouse button has been released
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleMouseUp(object sender, MouseEventArgs e) {
            controller.mouseReleased();
        }

        /// <summary>
        /// Attempts to connect this client to the server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectButton_Click(object sender, EventArgs e) {

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

        /// <summary>
        /// Has the controller send the player's name
        /// </summary>
        private void HandleConnected() {
            controller.Send(NameTextBox.Text);
        }

        /// <summary>
        /// Method called when JSON is received from the server and the view needs to be repainted
        /// </summary>
        /// <param name="messages"></param>
        private void UpdateView() {
            try
            {
                this.Invoke(new MethodInvoker(() => this.Invalidate(true)));
            }
            catch { }
        }

        /// <summary>
        /// Displays an error message when an error occurs
        /// </summary>
        /// <param name="err"></param>
        private void ShowError(string err) {
            MessageBox.Show(err);
            this.Invoke(new MethodInvoker(() => { ConnectButton.Enabled = true; ServerTextBox.Enabled = true; NameTextBox.Enabled = true; }));
        }

        /// <summary>
        /// Enables this form to start listening to key presses and mouse events
        /// </summary>
        private void StartGameFunctionality() {
            // Enable the global form to capture key presses
            KeyPreview = true;
            drawingPanel.MouseDown += HandleMouseDown;
            drawingPanel.MouseUp += HandleMouseUp;
            drawingPanel.MouseMove += HandleMouseMoved;
        }

        /// <summary>
        /// Informs the controller when a key is pressed so movement can be adjusted if needed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleKeyDown(object sender, KeyEventArgs e) {
            controller.UpdateMoveCommand(e.KeyCode.ToString());

            // Prevent other key handlers from running
            e.SuppressKeyPress = true;
            e.Handled = true;
        }

        /// <summary>
        /// Informs the controller when a key is released so movement can be adjusted if needed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleKeyUp(object sender, KeyEventArgs e) {
            controller.CancelMoveRequest(e.KeyCode.ToString());

            // Prevent other key handlers from running
            e.SuppressKeyPress = true;
            e.Handled = true;
        }
    }

    /*//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
     * /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
     * //////////////////////////////////////////////////////////Drawing Panel Class////////////////////////////////////////////////////////////////////
     * /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
     * ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////*/

    /// <summary>
    /// Customized panel used to handle and draw the GUI
    /// Provided By Daniel Kopta.
    /// </summary>
    public class DrawingPanel : Panel {

        // Readonly access to the model
        private World theWorld;

        GameController.GameController controller;

        // Image resources
        Image background;
        Bitmap wall;
        Image[] explosionframes;
        Image[] beamframes;

        // Holds all tuples of tank color images and if they are currently in use
        Dictionary<string, Tuple<Image, Image, Image>> playerColors;

        // Holds all animations that are currently active
        HashSet<Object> ActiveAnimation;

        /// <summary>
        /// Creates a new drawing panel with the specified controller and preps it for drawing frames of TankWars
        /// </summary>
        /// <param name="cntlr"></param>
        public DrawingPanel(GameController.GameController cntlr) {
            DoubleBuffered = true;
            theWorld = cntlr.GetWorld();
            controller = cntlr;
            ActiveAnimation = new HashSet<object>();
            controller.TriggerAnimations += HandleAnimations;
            playerColors = new Dictionary<string, Tuple<Image, Image, Image>>();
            LoadImages();
        }
        private void HandleAnimations(Object o) {
            ActiveAnimation.Add(o);
        }

        /// <summary>
        /// Helper method for DrawObjectWithTransform
        /// </summary>
        /// <param name="size">The world (and image) size</param>
        /// <param name="w">The worldspace coordinate</param>
        /// <returns></returns>
        private static int WorldSpaceToImageSpace(int size, double w) {
            return (int)w + size / 2;
        }

        // A delegate for DrawObjectWithTransform
        // Methods matching this delegate can draw whatever they want using e  
        public delegate void ObjectDrawer(object o, PaintEventArgs e);


        /// <summary>
        /// This method performs a translation and rotation to drawn an object in the world.
        /// </summary>
        /// <param name="e">PaintEventArgs to access the graphics (for drawing)</param>
        /// <param name="o">The object to draw</param>
        /// <param name="worldSize">The size of one edge of the world (assuming the world is square)</param>
        /// <param name="worldX">The X coordinate of the object in world space</param>
        /// <param name="worldY">The Y coordinate of the object in world space</param>
        /// <param name="angle">The orientation of the objec, measured in degrees clockwise from "up"</param>
        /// <param name="drawer">The drawer delegate. After the transformation is applied, the delegate is invoked to draw whatever it wants</param>
        private void DrawObjectWithTransform(PaintEventArgs e, object o, int worldSize, double worldX, double worldY, double angle, ObjectDrawer drawer) {
            // "push" the current transform
            System.Drawing.Drawing2D.Matrix oldMatrix = e.Graphics.Transform.Clone();

            int x = WorldSpaceToImageSpace(worldSize, worldX);
            int y = WorldSpaceToImageSpace(worldSize, worldY);
            e.Graphics.TranslateTransform(x, y);
            e.Graphics.RotateTransform((float)angle);
            drawer(o, e);

            // "pop" the transform
            e.Graphics.Transform = oldMatrix;
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method and draw a tank body
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void TankDrawer(object o, PaintEventArgs e) {
            Tank t = o as Tank;

            Image tankImage = playerColors[theWorld.getTankColor(t.id)].Item1;
            Rectangle r = new Rectangle(-Constants.TANKSIZE / 2, -Constants.TANKSIZE / 2, Constants.TANKSIZE, Constants.TANKSIZE);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.DrawImage(tankImage, r);

        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method and draw a tank turret
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void TurretDrawer(object o, PaintEventArgs e) {
            Tank t = o as Tank;

            Image turretImage = playerColors[theWorld.getTankColor(t.id)].Item2;
            Rectangle r = new Rectangle(-Constants.TURRETSIZE / 2, -Constants.TURRETSIZE / 2, Constants.TURRETSIZE, Constants.TURRETSIZE);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.DrawImage(turretImage, r);

        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method and draw a wall
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void WallDrawer(object o, PaintEventArgs e) {
            Wall w = o as Wall;
            using (System.Drawing.TextureBrush wallBrush = new System.Drawing.TextureBrush(wall)) {
                Rectangle rect = new Rectangle(-(Constants.WALLWIDTH / 2), -(Constants.WALLWIDTH / 2), (int)(Math.Abs(w.firstPoint.GetX() - w.secondPoint.GetX()) + 50), (int)(Math.Abs(w.firstPoint.GetY() - w.secondPoint.GetY()) + 50));
                e.Graphics.FillRectangle(wallBrush, rect);
            }
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method and draw a projectile 
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void ProjectileDrawer(object o, PaintEventArgs e) {
            Projectile p = o as Projectile;
            Image projImage = playerColors[theWorld.getTankColor(p.owner)].Item3;
            e.Graphics.DrawImage(projImage, -(Constants.PROJECTILESIZE / 2), -(Constants.PROJECTILESIZE / 2), Constants.PROJECTILESIZE, Constants.PROJECTILESIZE);
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void PowerupDrawer(object o, PaintEventArgs e) {
            using (System.Drawing.SolidBrush yellowBrush = new System.Drawing.SolidBrush(System.Drawing.Color.DarkOrange)) {
                Rectangle r = new Rectangle(-Constants.POWERUPOUTER / 2, -Constants.POWERUPOUTER / 2, Constants.POWERUPOUTER, Constants.POWERUPOUTER);
                e.Graphics.FillEllipse(yellowBrush, r);
            }
            using (System.Drawing.SolidBrush greenBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Green)) {
                Rectangle ri = new Rectangle(-Constants.POWERUPINNER / 2, -Constants.POWERUPINNER / 2, Constants.POWERUPINNER, Constants.POWERUPINNER);
                e.Graphics.FillEllipse(greenBrush, ri);
            }
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method and draw the necessary portion of the background
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void BackgroundDrawer(Object o, PaintEventArgs e) {
            int worldSize = controller.GetWorld().UniverseSize;
            Rectangle r = new Rectangle(-worldSize / 2, -worldSize / 2, worldSize, worldSize);
            e.Graphics.DrawImage(background, r);
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method and draw the current frame of an explosion animation
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void ExplosionDrawer(Object o, PaintEventArgs e) {
            Explosion exp = o as Explosion;
            if (exp.ticker < (explosionframes.Length * Constants.EXPLOSIONTIMESCALAR)) {
                Rectangle r = new Rectangle(-(Constants.EXPLOSIONSIZE) / 2, -(Constants.EXPLOSIONSIZE) / 2, Constants.EXPLOSIONSIZE, Constants.EXPLOSIONSIZE);
                e.Graphics.DrawImage(explosionframes[exp.ticker / Constants.EXPLOSIONTIMESCALAR], r);
                exp.ticker++;
            }
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method and draw the current frame of a beam animation
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void BeamDrawer(object o, PaintEventArgs e) {
            Beam b = o as Beam;
            if (b.ticker < (beamframes.Length * Constants.BEAMTIMESCALAR)) {
                Rectangle r = new Rectangle(-(Constants.BEAMSIZEWIDTH) / 2, -(Constants.BEAMSIZELENGTH), Constants.BEAMSIZEWIDTH, Constants.BEAMSIZELENGTH);
                // e.Graphics.RotateTransform
                e.Graphics.DrawImage(beamframes[b.ticker / Constants.BEAMTIMESCALAR], r);
                b.advanceTicker();
            }
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method and draw a player health bar
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void HealthBarDrawer(Object o, PaintEventArgs e) {
            Tank t = o as Tank;

            // Draw the bar in a different color based on the player's remaining health
            using (System.Drawing.SolidBrush brush = new System.Drawing.SolidBrush(Color.Black)) {
                if (t.hitPoints == 1)
                    brush.Color = Color.Red;
                if (t.hitPoints == 2)
                    brush.Color = Color.Yellow;
                if (t.hitPoints >= 3)
                    brush.Color = Color.Green;

                Rectangle r = new Rectangle(-Constants.HEALTHBARWIDTH / 2, -((Constants.TANKSIZE / 2) + Constants.HEALTHBARHEIGHT * 2), t.hitPoints * (Constants.HEALTHBARWIDTH / 3), Constants.HEALTHBARHEIGHT);
                e.Graphics.FillRectangle(brush, r);
            }
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method and write out a tank's name
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void PlayerNameDrawer(Object o, PaintEventArgs e) {
            Tank t = o as Tank;
            using (System.Drawing.SolidBrush brush = new System.Drawing.SolidBrush(Color.White)) {
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                e.Graphics.DrawString(t.name + ": " + t.score, new Font("Arial", 14), brush, 0, Constants.TANKSIZE / 2, format);
            }
        }


        // This method is invoked when the DrawingPanel needs to be re-drawn
        protected override void OnPaint(PaintEventArgs e) {
            if (theWorld == null) {
                theWorld = controller.GetWorld();
                return;
            }

            // Prevent modification to the world while drawing a frame
            lock (theWorld) {

                // Transforms the current window to the user's coordinates
                double playerX = controller.GetPlayerX();
                double playerY = controller.GetPlayerY();

                // calculate view/world size ratio
                int worldSize = theWorld.UniverseSize;
                double ratio = (double)Constants.VIEWSIZE / (double)worldSize;
                int halfSizeScaled = (int)(worldSize / 2.0 * ratio);

                double inverseTranslateX = -WorldSpaceToImageSpace(worldSize, playerX) + halfSizeScaled;
                double inverseTranslateY = -WorldSpaceToImageSpace(worldSize, playerY) + halfSizeScaled;

                e.Graphics.TranslateTransform((float)inverseTranslateX, (float)inverseTranslateY);

                DrawObjectWithTransform(e, background, theWorld.UniverseSize, 0, 0, 0, BackgroundDrawer);

                // Draw everything now that the frame has been transformed

                //Draws all animations (i.e. Explosions and Beams)
                DrawAnimations(e);

                // Draw the Walls
                foreach (Wall w in theWorld.Walls.Values) {
                    w.GetPoints(out double topLeftX, out double topLeftY);
                    DrawObjectWithTransform(e, w, theWorld.UniverseSize, topLeftX, topLeftY, 0, WallDrawer);
                }

                // Draw each Tank's body, turret, health bar, and name
                foreach (Tank tank in theWorld.Players.Values) {
                    DrawObjectWithTransform(e, tank, theWorld.UniverseSize, tank.location.GetX(), tank.location.GetY(), tank.orientation.ToAngle(), TankDrawer);
                    DrawObjectWithTransform(e, tank, theWorld.UniverseSize, tank.location.GetX(), tank.location.GetY(), tank.tdir.ToAngle(), TurretDrawer);
                    DrawObjectWithTransform(e, tank, theWorld.UniverseSize, tank.location.GetX(), tank.location.GetY(), 0, HealthBarDrawer);
                    DrawObjectWithTransform(e, tank, theWorld.UniverseSize, tank.location.GetX(), tank.location.GetY(), 0, PlayerNameDrawer);
                }

                // Draw the Powerups
                foreach (Powerup pow in theWorld.Powerups.Values) {
                    DrawObjectWithTransform(e, pow, theWorld.UniverseSize, pow.location.GetX(), pow.location.GetY(), 0, PowerupDrawer);
                }

                // Draw the Projectiles
                foreach (Projectile proj in theWorld.Projectiles.Values) {
                    DrawObjectWithTransform(e, proj, theWorld.UniverseSize, proj.location.GetX(), proj.location.GetY(), proj.orientation.ToAngle(), ProjectileDrawer);
                }

                // Do anything that Panel (from which we inherit) needs to do
                base.OnPaint(e);

            }

        }

        /// <summary>
        /// Handles the display of currently active animations
        /// </summary>
        /// <param name="e"></param>
        private void DrawAnimations(PaintEventArgs e) {
            HashSet<Object> ToRemove = new HashSet<Object>();

            foreach (Object o in ActiveAnimation) {
                // Animate explosions
                if (o is Explosion) {
                    Explosion exp = o as Explosion;
                    DrawObjectWithTransform(e, exp, theWorld.UniverseSize, exp.location.GetX(), exp.location.GetY(), 0, ExplosionDrawer);
                    if (exp.AnimationFinished()) {
                        exp.ticker = 0;
                        ToRemove.Add(exp);
                    }
                }
                // Animate Beams
                else if (o is Beam) {
                    Beam b = o as Beam;
                    DrawObjectWithTransform(e, b, theWorld.UniverseSize, b.origin.GetX(), b.origin.GetY(), b.direction.ToAngle(), BeamDrawer);
                    if (b.AnimationFinished()) {
                        ToRemove.Add(b);
                    }
                }
            }
            // Stops animating finished animations
            foreach (Object o in ToRemove) {
                ActiveAnimation.Remove(o);
            }
        }


        /// <summary>
        /// Pre-loads all of the images so they only need to be made once
        /// Beam gif retrieved from https://www.artstation.com/artwork/L2qdYK on Nov. 20
        /// Explosion gif retrieved from https://webstockreview.net/image/clipart-explosion-cartoon/1751832.html?no1 on Nov. 19
        /// </summary>
        private void LoadImages() {
            Image wallImage = Image.FromFile("..\\..\\..\\Resources\\Images\\WallSprite.png");
            wall = ResizeImage(wallImage, Constants.WALLWIDTH, Constants.WALLWIDTH);
            background = Image.FromFile("..\\..\\..\\Resources\\Images\\Background.png");
            explosionframes = getFrames(Image.FromFile("..\\..\\..\\Resources\\Images\\Explosion.gif"));
            beamframes = getFrames(Image.FromFile("..\\..\\..\\Resources\\Images\\Beam.gif"));

            // Add all tank color image tuples to the dictionary
            // Blue
            playerColors.Add("blue", new Tuple<Image, Image, Image>(Image.FromFile("..\\..\\..\\Resources\\Images\\BlueTank.png"),
                Image.FromFile("..\\..\\..\\Resources\\Images\\BlueTurret.png"),
                Image.FromFile("..\\..\\..\\Resources\\Images\\shot-blue.png")));

            // Dark
            playerColors.Add("dark", new Tuple<Image, Image, Image>(Image.FromFile("..\\..\\..\\Resources\\Images\\DarkTank.png"),
                Image.FromFile("..\\..\\..\\Resources\\Images\\DarkTurret.png"),
                Image.FromFile("..\\..\\..\\Resources\\Images\\shot-grey.png")));

            // Green
            playerColors.Add("green", new Tuple<Image, Image, Image>(Image.FromFile("..\\..\\..\\Resources\\Images\\GreenTank.png"),
                Image.FromFile("..\\..\\..\\Resources\\Images\\GreenTurret.png"),
                Image.FromFile("..\\..\\..\\Resources\\Images\\shot-green.png")));

            // LightGreen
            playerColors.Add("lightGreen", new Tuple<Image, Image, Image>(Image.FromFile("..\\..\\..\\Resources\\Images\\LightGreenTank.png"),
                Image.FromFile("..\\..\\..\\Resources\\Images\\LightGreenTurret.png"),
                Image.FromFile("..\\..\\..\\Resources\\Images\\shot-white.png")));

            // Orange
            playerColors.Add("orange", new Tuple<Image, Image, Image>(Image.FromFile("..\\..\\..\\Resources\\Images\\OrangeTank.png"),
                Image.FromFile("..\\..\\..\\Resources\\Images\\OrangeTurret.png"),
                Image.FromFile("..\\..\\..\\Resources\\Images\\shot-brown.png")));

            // Purple
            playerColors.Add("purple", new Tuple<Image, Image, Image>(Image.FromFile("..\\..\\..\\Resources\\Images\\PurpleTank.png"),
                Image.FromFile("..\\..\\..\\Resources\\Images\\PurpleTurret.png"),
                Image.FromFile("..\\..\\..\\Resources\\Images\\shot-violet.png")));

            // Red
            playerColors.Add("red", new Tuple<Image, Image, Image>(Image.FromFile("..\\..\\..\\Resources\\Images\\RedTank.png"),
                Image.FromFile("..\\..\\..\\Resources\\Images\\RedTurret.png"),
                Image.FromFile("..\\..\\..\\Resources\\Images\\shot-red.png")));

            // Yellow
            playerColors.Add("yellow", new Tuple<Image, Image, Image>(Image.FromFile("..\\..\\..\\Resources\\Images\\YellowTank.png"),
                Image.FromFile("..\\..\\..\\Resources\\Images\\YellowTurret.png"),
                Image.FromFile("..\\..\\..\\Resources\\Images\\shot-yellow.png")));
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// Method provided by mpen on stackoverflow.com, retieved 11/18/20:
        /// https://stackoverflow.com/questions/1922040/how-to-resize-an-image-c-sharp
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height) {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            // Sets various graphics settings
            using (var graphics = Graphics.FromImage(destImage)) {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                // Resize the image
                using (var wrapMode = new ImageAttributes()) {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        /// <summary>
        /// Returns the current frame of an animation
        /// Code provided by https://codingvision.net/c-get-frames-from-a-gif.
        /// Retrieved on 20.11.20
        /// </summary>
        /// <param name="originalImg"></param>
        /// <returns></returns>
        private Image[] getFrames(Image originalImg) {
            int numberOfFrames = originalImg.GetFrameCount(FrameDimension.Time);
            Image[] frames = new Image[numberOfFrames];

            for (int i = 0; i < numberOfFrames; i++) {
                originalImg.SelectActiveFrame(FrameDimension.Time, i);
                frames[i] = ((Image)originalImg.Clone());
            }

            return frames;
        }

    }
}


