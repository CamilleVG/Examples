using System;
using System.Collections.Generic;
using System.Text;
using TankWars;
using Newtonsoft.Json;

namespace Model
{
    public class Explosion
    {
        private Vector2D location;
        public bool CurrentlyAnimating;


        public Vector2D Location
        {
            get => location;
        }

        public Explosion(Vector2D loc)
        {
            location = loc;
            CurrentlyAnimating = false;
        }
    }
}
