using Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Resources;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using TankWars;

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

            //register key handlers
            this.KeyDown += HandleKeyDown;
            this.KeyUp += HandleKeyUp;
        }

        private void HandleMouseMoved(object sender, MouseEventArgs e) {
            //double angleFromCenterToMouse = Math.Asin((e.Y - Constants.VIEWSIZE / 2) / (e.X - Constants.VIEWSIZE / 2));
            Vector2D vectorFromCenter = new Vector2D((e.X - Constants.VIEWSIZE / 2), (e.Y - Constants.VIEWSIZE / 2));
            controller.updateTDir(vectorFromCenter);
        }


        private void HandleMouseDown(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left)
                controller.mousePressed("left");
            if (e.Button == MouseButtons.Right)
                controller.mousePressed("right");
        }

        private void HandleMouseUp(object sender, MouseEventArgs e) {
            if (e.Button == MouseButtons.Left)
                controller.mouseReleased();
        }

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
        private void HandleConnected() {
            // Just print a message saying we connected
            controller.Send(NameTextBox.Text);
        }

        /// <summary>
        /// Method called when JSON is received from the server
        /// </summary>
        /// <param name="messages"></param>
        private void UpdateView() {
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
            drawingPanel.MouseDown += HandleMouseDown;
            drawingPanel.MouseUp += HandleMouseUp;
            drawingPanel.MouseMove += HandleMouseMoved;
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
        Image wallImage;
        Bitmap wall;
        Bitmap explosion;

        // Holds all tuples of tank color images and if they are currently in use
        Dictionary<string, Tuple<Image, Image, Image>> playerColors;
        HashSet<Object> QueuedAnimations;


        public DrawingPanel(GameController.GameController cntlr) {
            DoubleBuffered = true;
            theWorld = cntlr.GetWorld();
            controller = cntlr;
            QueuedAnimations = new HashSet<object>();
            controller.TriggerAnimations += HandleAnimations;
            playerColors = new Dictionary<string, Tuple<Image, Image, Image>>();
            LoadImages();
        }
        private void HandleAnimations(Object o)
        {
            QueuedAnimations.Add(o);
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

            using (var graphics = Graphics.FromImage(destImage)) {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes()) {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
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
        /// DrawObjectWithTransform will invoke this method
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void TankDrawer(object o, PaintEventArgs e) {
            Tank t = o as Tank;

            Image tankImage = playerColors[theWorld.getTankColor(t.ID)].Item1;
            Rectangle r = new Rectangle(-Constants.TANKSIZE / 2, -Constants.TANKSIZE / 2, Constants.TANKSIZE, Constants.TANKSIZE);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.DrawImage(tankImage, r);

        }

        private void TurretDrawer(object o, PaintEventArgs e) {
            Tank t = o as Tank;

            Image turretImage = playerColors[theWorld.getTankColor(t.ID)].Item2;
            Rectangle r = new Rectangle(-Constants.TURRETSIZE / 2, -Constants.TURRETSIZE / 2, Constants.TURRETSIZE, Constants.TURRETSIZE);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.DrawImage(turretImage, r);

        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void WallDrawer(object o, PaintEventArgs e) {
            Wall w = o as Wall;
            //Rectangle r = new Rectangle(-(Constants.WALLWIDTH / 2), -(Constants.WALLWIDTH / 2), Constants.WALLWIDTH, Constants.WALLWIDTH);
            using (System.Drawing.TextureBrush wallBrush = new System.Drawing.TextureBrush(wall)) {
                Rectangle rect = new Rectangle(-(Constants.WALLWIDTH / 2), -(Constants.WALLWIDTH / 2), (int)(Math.Abs(w.FirstPoint.GetX() - w.SecondPoint.GetX()) + 50), (int)(Math.Abs(w.FirstPoint.GetY() - w.SecondPoint.GetY()) + 50));
                e.Graphics.FillRectangle(wallBrush, rect);
            }
        }
        private void ProjectileDrawer(object o, PaintEventArgs e) {
            Projectile p = o as Projectile;
            Tank t = theWorld.Players[p.Owner];
            Image projImage = playerColors[theWorld.getTankColor(t.ID)].Item3;
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

        private void BackgroundDrawer(Object o, PaintEventArgs e) {
            int worldSize = controller.GetWorld().UniverseSize;
            Rectangle r = new Rectangle(-worldSize / 2, -worldSize / 2, worldSize, worldSize);
            e.Graphics.DrawImage(background, r);
        }

        private void ExplosionDrawer(Object o, PaintEventArgs e)
        {
            Rectangle r = new Rectangle(-Constants.TANKSIZE / 2, -Constants.TANKSIZE / 2, Constants.TANKSIZE, Constants.TANKSIZE);
            e.Graphics.DrawImage(explosion, r);
        }

        // This method is invoked when the DrawingPanel needs to be re-drawn
        protected override void OnPaint(PaintEventArgs e) {
            if (theWorld == null) {
                theWorld = controller.GetWorld();
                return;
            }

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
            // Draw everything
            lock (theWorld) {
                foreach (Object o in QueuedAnimations)
                {
                    if(o is Explosion)
                    {
                        
                        Explosion exp = o as Explosion;
                        if (exp.CurrentlyAnimating)
                        {
                            ImageAnimator.UpdateFrames();
                            Console.WriteLine("Select frame");
                            DrawObjectWithTransform(e, exp, theWorld.UniverseSize, exp.Location.GetX(), exp.Location.GetY(), 0, ExplosionDrawer);
                        }
                        else
                        {
                            exp.CurrentlyAnimating = true;
                            ImageAnimator.Animate(explosion, (Object obj, EventArgs evt) => { });
                        }
                    }
                    else if (o is Beam)
                    { 
                        Beam b = o as Beam;
                        //DrawObjectWithTransform(e, b, theWorld.UniverseSize, b.Location.GetX(), b.Location.GetY(), 0, BeamDrawer);
                    }  
                }
                //QueuedAnimations.Clear();
                // Draw the Walls
                foreach (Wall w in theWorld.Walls.Values) {
                    w.GetPoint(out double topLeftX, out double topLeftY);
                    DrawObjectWithTransform(e, w, theWorld.UniverseSize, topLeftX, topLeftY, 0, WallDrawer);
                }

                // Draw the Tanks
                foreach (Tank tank in theWorld.Players.Values) {
                    DrawObjectWithTransform(e, tank, theWorld.UniverseSize, tank.Location.GetX(), tank.Location.GetY(), tank.Orientation.ToAngle(), TankDrawer);
                    DrawObjectWithTransform(e, tank, theWorld.UniverseSize, tank.Location.GetX(), tank.Location.GetY(), tank.tdir.ToAngle(), TurretDrawer);
                }

                // Draw the Powerups
                foreach (Powerup pow in theWorld.Powerups.Values) {
                    DrawObjectWithTransform(e, pow, theWorld.UniverseSize, pow.Location.GetX(), pow.Location.GetY(), 0, PowerupDrawer);
                }

                // Draw the Projectiles
                foreach (Projectile proj in theWorld.Projectiles.Values) {
                    DrawObjectWithTransform(e, proj, theWorld.UniverseSize, proj.Location.GetX(), proj.Location.GetY(), proj.Orientation.ToAngle(), ProjectileDrawer);
                }

                // Do anything that Panel (from which we inherit) needs to do
                base.OnPaint(e);

            }

        }

        private void LoadImages() {
            wallImage = Image.FromFile("..\\..\\..\\Resources\\Images\\WallSprite.png");
            wall = ResizeImage(wallImage, Constants.WALLWIDTH, Constants.WALLWIDTH);
            explosion = new Bitmap(Image.FromFile("..\\..\\..\\Resources\\Images\\Explosion.gif"));
            background = Image.FromFile("..\\..\\..\\Resources\\Images\\Background.png");


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


    }
}


