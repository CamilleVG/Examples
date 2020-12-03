using System;
using TankWars;

namespace Model {

    /// <summary>
    /// A class that holds constants
    /// </summary>
    public class Constants {

        public static int HORIZONTAL = 1; // Signifies a wall has a horizontal orientation
        public static int VERTICAL = 0; // Signifies a wall has a vertical orientation

        // XML settings file constants
        public static int MaxHP = 3;
        public static int MAXPOWERUPS = 4;
        public static int BEAMCOOLDOWN = 30;
        public static int POWERUPDELAY = 1650;
        public static int PROJECTILESPEED = 25;
        public static int MYSTERYPOWERUPS = 0;  //0 for default, 1 for mystery powerups
        public static int TANKSPEED = 3;
        public static int ENHANCEDTANKSPEED = 10;
        public static int ENHANCEDPROJECTILESPEED = 50;
        public static int ENHANCEDPROJECTILETIME = 600;
        public static int ENHANCEDTANKSPEEDTIME = 600;
        public static int FASTERFIRERATETIME = 600;
        
        // Constants not editable through the XML settings file
        public const int BEAMLOOPTIMES = 3;
        public const int BEAMSIZELENGTH = (int)(900 * 1.41); // Maximum amount to stretch beam out ((VIEWSIZE * 1.41) will stretch to edge of clients screen)
        public const int BEAMSIZEWIDTH = 60;
        public const int BEAMTIMESCALAR = 2; // How stretched out the animation time is
        public const int EXPLOSIONTIMESCALAR = 4; // How stretched out the animation time is
        public const int EXPLOSIONSIZE = 125;
        public const int HEALTHBARHEIGHT = 5;
        public const int HEALTHBARWIDTH = 45;
        public const int MENUSIZE = 40;
        public const int POWERUPINNER = 12; // Inner circle radius on a powerup
        public const int POWERUPOUTER = 18; // Outer circle radius and effective size of a powerup
        public const int PROJECTILESIZE = 30;
        public const int TANKSIZE = 60;
        public const int TURRETSIZE = 50;
        public const int VIEWSIZE = 900;
        public const int WALLWIDTH = 50;
        public static Vector2D DOWN = new Vector2D(0, 1);
        public static Vector2D LEFT = new Vector2D(-1, 0);
        public static Vector2D RIGHT = new Vector2D(1, 0);
        public static Vector2D UP = new Vector2D(0, -1);
    }
}
