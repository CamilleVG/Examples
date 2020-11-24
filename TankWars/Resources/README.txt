Authors
Preston Powell and Camille Van Ginkel
Parts of the code taken from Daniel Kopta CS3500 University of Utah Fall 2020 class with permission


Overall Design:

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




