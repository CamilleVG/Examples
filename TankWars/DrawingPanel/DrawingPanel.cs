using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Model;
using Resources;
using GameController;

namespace View
{
    public class DrawingPanel : Panel
    {
        private World theWorld;
        GameController.GameController controller;

        public DrawingPanel(GameController.GameController cntlr)
        {
            DoubleBuffered = true;
            theWorld = cntlr.GetWorld();
            controller = cntlr;
        }

        /// <summary>
        /// Helper method for DrawObjectWithTransform
        /// </summary>
        /// <param name="size">The world (and image) size</param>
        /// <param name="w">The worldspace coordinate</param>
        /// <returns></returns>
        private static int WorldSpaceToImageSpace(int size, double w)
        {
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
        private void DrawObjectWithTransform(PaintEventArgs e, object o, int worldSize, double worldX, double worldY, double angle, ObjectDrawer drawer)
        {
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
        private void PlayerDrawer(object o, PaintEventArgs e)
        {
            Tank p = o as Tank;

            int width = 10;
            int height = 10;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (System.Drawing.SolidBrush blueBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Blue))
            using (System.Drawing.SolidBrush greenBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Green))
            {
                // Rectangles are drawn starting from the top-left corner.
                // So if we want the rectangle centered on the player's location, we have to offset it
                // by half its size to the left (-width/2) and up (-height/2)
                Rectangle r = new Rectangle(-(width / 2), -(height / 2), width, height);

                //if (p.GetTeam() == 1) // team 1 is blue
                //    e.Graphics.FillRectangle(blueBrush, r);
                // else                  // team 2 is green
                e.Graphics.FillRectangle(greenBrush, r);
            }
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void WallDrawer(object o, PaintEventArgs e)
        {
            Image wall = Image.FromFile("..\\..\\..\\Resources\\Images\\WallSprite.png");
            e.Graphics.DrawImage(wall, -(Constants.WALLWIDTH / 2), -(Constants.WALLWIDTH / 2), Constants.WALLWIDTH, Constants.WALLWIDTH);
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void PowerupDrawer(object o, PaintEventArgs e)
        {
            Powerup p = o as Powerup;

            int width = 8;
            int height = 8;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (System.Drawing.SolidBrush redBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red))
            using (System.Drawing.SolidBrush yellowBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Yellow))
            using (System.Drawing.SolidBrush blackBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black))
            {
                // Circles are drawn starting from the top-left corner.
                // So if we want the circle centered on the powerup's location, we have to offset it
                // by half its size to the left (-width/2) and up (-height/2)
                Rectangle r = new Rectangle(-(width / 2), -(height / 2), width, height);

                //if (p.GetKind() == 1) // red powerup
                //    e.Graphics.FillEllipse(redBrush, r);
                //if (p.GetKind() == 2) // yellow powerup
                //    e.Graphics.FillEllipse(yellowBrush, r);
                //if (p.GetKind() == 3) // black powerup
                //    e.Graphics.FillEllipse(blackBrush, r);
            }
        }


        // This method is invoked when the DrawingPanel needs to be re-drawn
        protected override void OnPaint(PaintEventArgs e)
        {

            Image background = Image.FromFile("..\\..\\..\\Resources\\Images\\Background.png");
            e.Graphics.DrawImage(background, 0, 0);

            Console.WriteLine("Reached line 134 in DP:  Player x position is ");
            double playerX = controller.GetPlayerX();
            double playerY = controller.GetPlayerY();
            

            // calculate view/world size ratio
            int worldSize = theWorld.UniverseSize;
            double ratio = (double)Constants.VIEWSIZE / (double)worldSize;
            int halfSizeScaled = (int)(worldSize / 2.0 * ratio);

            double inverseTranslateX = -WorldSpaceToImageSpace(worldSize, playerX) + halfSizeScaled;
            double inverseTranslateY = -WorldSpaceToImageSpace(worldSize, playerY) + halfSizeScaled;

            e.Graphics.TranslateTransform((float)inverseTranslateX, (float)inverseTranslateY);

            // Draw everything
            lock (theWorld)
            {
                foreach (Wall w in theWorld.Walls.Values)
                {
                    if (w.orientation == Constants.HORIZONTAL)
                    {
                        double startXVal;
                        double endXVal;
                        double yVal = w.FirstPoint.GetY();
                        if (w.FirstPoint.GetX() < w.SecondPoint.GetX())
                        {
                            startXVal = w.FirstPoint.GetX();
                            endXVal = w.SecondPoint.GetX();
                        }
                        else
                        {
                            startXVal = w.SecondPoint.GetX();
                            endXVal = w.FirstPoint.GetX();
                        }
                        while (startXVal <= endXVal)
                        {
                            DrawObjectWithTransform(e, w, theWorld.UniverseSize, startXVal, yVal, 0, WallDrawer);
                            startXVal += 50;
                        }
                    }
                    else
                    {//the wall is Vertical
                        double xVal = w.FirstPoint.GetX();
                        double startYVal;
                        double endYVal;

                        if (w.FirstPoint.GetY() < w.SecondPoint.GetY())
                        {
                            startYVal = w.FirstPoint.GetY();
                            endYVal = w.SecondPoint.GetY();
                        }
                        else
                        {
                            startYVal = w.SecondPoint.GetY();
                            endYVal = w.FirstPoint.GetY();
                        }
                        while (startYVal <= endYVal)
                        {
                            DrawObjectWithTransform(e, w, theWorld.UniverseSize, xVal, startYVal, 0, WallDrawer);
                            startYVal += 50;
                        }

                    }

                }


                foreach (Tank play in theWorld.Players.Values)
                {
                    //DrawObjectWithTransform(e, play, theWorld.UniverseSize, play.GetLocation().GetX(), play.GetLocation().GetY(), play.GetOrientation().ToAngle(), PlayerDrawer);
                }

                // Draw the powerups
                foreach (Powerup pow in theWorld.Powerups.Values)
                {
                    //DrawObjectWithTransform(e, pow, theWorld.UniverseSize, pow.GetLocation().GetX(), pow.GetLocation().GetY(), 0, PowerupDrawer);
                }
            }

            // Do anything that Panel (from which we inherit) needs to do
            base.OnPaint(e);
        }

    }
}
