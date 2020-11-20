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
        public int ticker;
        

        public Vector2D Location
        {
            get => location;
        }

        public Explosion(Vector2D loc)
        {
            location = loc;
        }
        public bool AnimationFinished()
        {
            if (ticker >= 7 * Constants.EXPLOSIONTIMESCALAR)
            {
                return true;
            }
            return false;
        }
       
    }
}
