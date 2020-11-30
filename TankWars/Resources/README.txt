Authors
Preston Powell and Camille Van Ginkel
Parts of the code taken from Daniel Kopta CS3500 University of Utah Fall 2020 class with permission


Overall Design Server:

-Notable Setting-

MysteryPowerups: 
This an alternative gamemode, the setting 0 will be the base game and any other setting will be Mystery Powerups. The gamemode
features 5 different possible effects when a powerup is obtained including: the standard beam, enhanced tank speed, enhanced projectiles (speed and damage),
enhanced fire rate (4x, hardcoded), full heal. Each lasts for around 10 seconds (600 frames) but can be adjusted in the settings.

BeamCooldown: 
This is meant to be a safety buffer on firing the beam to prevent the player from wasting their beams. The default is .5 sec (30 frames) and a 
player must wait that long before firing the next beam. This is not intended to restrict the player, and can be adjusted as desired. 

Other XML Settings:
We wanted to add any constants with a realistic use for changing from the constants class to the XML file. This includes the MaxHP value, the
maximum delay before a powerup spawns, the maximum number of powerups on the field at a given time, the projectile, and the tank speed, as well
as the time settings for each powerup from MysteryPowerups.

Default Settings:
Many of the default settings are the same but we chose to raise the maximum number of powerups on the field to 4 by default for more interesting
gameplay. We also chose to up the base tank speed to 4.

- Code Design -
Beam Collision Detection:
In order to check for beam collisions we loop over checkpoints along the diagonal spaced about 1/6 of a tank's size apart and check for tank
collisions at each point.

Wall TP Blocking:
We decided to prevent a player from being able to wraparound/teleport to the other side of the world even if a wall was missing if the other side
had a wall that would result in the player getting teleported into a wall.

Approximating the Tank:
We chose to follow Professor Kopta's advice and approximate the tank as a circle for collisions with everything except walls.

Static Server vs Server Object:
We chose to keep the server as an object that could be created rather than a collection of static methods. With some small modifications
this would allow one machine to run several servers with the same program if desired.








Overall Design Client:

Animated game features:
Both the beam attack and the explosion were animated in our client. We chose to animate these by using gif files and displaying the
appropriate frame of the animation on each call to OnPaint.  The gifs were stored in the view as an Image array and we kept track of frame 
currently being displayed with a ticker variable. Both objects have their own ticker member variables in their own class. This allows us to have 
multiple animations of the same type running simultaneously, and only load each animation once. The gifs were retrieved from online art hubs and 
were allowable in personal use projects. The code used for resizing images and storing gifs in image arrays were retrieved from online sources and 
credited in the code documentation. 


Handle Walls vs Checking Walls in the Processing Loop:
At first we had two methods to choose from as far as loading in the walls go. While we originally had code for processing the walls on
each frame, we ultimately moved this into its own step prior to the code that received frames. This is to allow us to only have wall checking
code at the beginning since the walls are only sent once by the server. This was acheived by setting the OnNetworkAction delegate to a method
specifically built for loading in walls prior to receiving other types of messages from the server.


Walls as a Whole vs Units:
Another design decision was whether to store the walls as objects containing an array of smaller wall units or keep the walls as just two points
and use a texture brush to fill in the space between these points. We chose the latter because it reduced code complexity and allowed for more
readability of our OnPaint override. 


Storing the Player's Last Location:
We decided to store the player's last known location as a member variable of the controller class. This was to allow the view easy access for 
centering the camera (prior to painting a new frame) and removed null checking code and complexity from our OnPaint method. The location variable
itself is private and is accessed through a GetPlayerX and GetPlayerY method pair. 


Command Control Data Structure:
In order to efficiently keep track of user input and send this to the server each frame we created a command control object. The object has
fields that store the turret direction, desired movement direction, and firing state the view has sent to the controller. In order to keep track
of command input order commands were stored in a linkedlist that was updated whenever the view informed the controller that a key was pressed
or released. Active commands were removed from the list when their button was released and the next command in the list then took priority, with
a default value of 'none'. This was done in order to ensure movement was smooth especially when multiple keys were pressed at a time.

Total Time Estimate: 45 - 50 hours

See github commits for more details on when features were implemented

11/10
Time Worked: 3 hours
We implement our handshake.

11/08
Time Worked: 3 hours
We created a constants class which will hold values that need to stay consisted throughout the the solution.
All constants member variables are in all caps.




