using System;
using System.Collections.Generic;

/// <summary>
/// An abstract base class for obstacles.
/// </summary>
public abstract class Obstacle
{
    /// <summary>
    /// (x, y) location of the obstacle.
    /// </summary>
    public (int X, int Y) Location { get; protected set; }

    /// <summary>
    /// Determines if a given location is blocked by the obstacle or not.
    /// </summary>
    /// <param name="location">The (x, y) location to check.</param>
    /// <returns>Returns True if the location is blocked, otherwise False as its unblocked.</returns>
    public abstract bool IsLocationBlocked((int X, int Y) location);
}

/// <summary>
/// A Guard class inherit from Obstacle as Guard is a type of obstacle.
/// The Guard will obstruct the agent from entering (x, y) as location blocked.
/// </summary>
public class Guard : Obstacle
{
    /// <summary>
    /// Initializes a new instance of the Guard class with the specified coordinates.
    /// </summary>
    /// <param name="x">The X-coordinate of the guard.</param>
    /// <param name="y">The Y-coordinate of the guard.</param>
    public Guard(int x, int y)
    {
        Location = (x, y);
    }

    /// <inheritdoc/>
    public override bool IsLocationBlocked((int X, int Y) location)
    {
        return Location == location;
    }
}

/// <summary>
/// A Fence class inherit from Obstacle as Fence is a type of obstacle.
/// The fence will obstruct the agent from entering any square within its bounds. 
/// (startX...endX, startY...endY) inclusively as series of blocked locations 
/// </summary>
public class Fence : Obstacle
{
    /// <summary>
    /// Gets the end location of the fence.
    /// </summary>
    public (int X, int Y) EndLocation { get; private set; }
    /// <summary>
    /// Initializes a new instance of the Fence class with the specified start and end coordinates.
    /// </summary>
    /// <param name="startX">The X-coordinate where the fence starts.</param>
    /// <param name="startY">The Y-coordinate where the fence starts.</param>
    /// <param name="endX">The X-coordinate where the fence ends.</param>
    /// <param name="endY">The Y-coordinate where the fence ends.</param>
    public Fence(int startX, int startY, int endX, int endY)
    {
        Location = (startX, startY);
        EndLocation = (endX, endY);
    }
    /// <inheritdoc/>
    public override bool IsLocationBlocked((int X, int Y) location)
    {
        // Check for vertical fence
        if (Location.X == EndLocation.X)
        {
            return location.X == Location.X && 
                   location.Y >= Math.Min(Location.Y, EndLocation.Y) && 
                   location.Y <= Math.Max(Location.Y, EndLocation.Y);
        }
        // Check for horizontal fence
        else if (Location.Y == EndLocation.Y)
        {
            return location.Y == Location.Y && 
                   location.X >= Math.Min(Location.X, EndLocation.X) && 
                   location.X <= Math.Max(Location.X, EndLocation.X);
        }
        return false;
    }
}

/// <summary>
/// A Sensor class inherit from Obstacle as Sensor is a type of obstacle.
/// The sensor will obstruct the agent from entering any square within sensor's range. 
/// Pythagorean: distance = sqrt(x^2 + y^2)
/// </summary>
public class Sensor : Obstacle 
{
    //The range cannot be less than or equal to 0 - this is considered invalid input.
    public double Range {get; private set;}
    /// <summary>
    /// Initializes a new instance of the Sensor class with the specified coordinates and range.
    /// </summary>
    /// <param name="x">The X-coordinate of the sensor.</param>
    /// <param name="y">The Y-coordinate of the sensor.</param>
    /// <param name="range">The range of the sensor.</param>
    public Sensor(int x, int y, double range) 
    {
        Location = (x, y);
        Range = range;
    }

    public override bool IsLocationBlocked((int X, int Y) location)
    {
        double distance = Math.Sqrt(Math.Pow(Location.X - location.X, 2) + Math.Pow(Location.Y - location.Y, 2));
        return distance <= Range;
    }
}

/// <summary>
/// A Camera class inherit from Obstacle as Camera is a type of obstacle.
/// The camera will obstruct the agent from entering any square within 90 degree cone of vision centered on that direction.
/// </summary>
public class Camera : Obstacle
{

    /// <summary>
    /// Gets the direction the camera is facing.
    /// Cameras have a location and a direction, and can spot anything within a 90 degree cone of vision centred on that direction. 
    /// Cameras have infinite range.
    /// </summary>
    public char Direction { get; private set; }
    /// <summary>
    /// Initializes a new instance of the Camera class with the specified coordinates and direction.
    /// </summary>
    /// <param name="x">The X-coordinate of the camera.</param>
    /// <param name="y">The Y-coordinate of the camera.</param>
    /// <param name="direction">The direction the camera is facing.</param>
    public Camera(int x, int y, char direction)
    {
        Location = (x, y);
        Direction = direction;
    }
    /// <inheritdoc/>
    public override bool IsLocationBlocked((int X, int Y) location)
    {
        int deltaX = location.X - Location.X;
        int deltaY = location.Y - Location.Y;

        // Check if the location is the same as the camera's location
        if (location.X == Location.X && location.Y == Location.Y)
            return true;

        switch (Direction)
        {
            case 'n':
                if (deltaY >= 0) return false; // Only consider locations above the camera
                return Math.Abs(deltaX) <= Math.Abs(deltaY); // The absolute difference in X should be less than or equal to the absolute difference in Y

            case 's':
                if (deltaY <= 0) return false; // Only consider locations below the camera
                return Math.Abs(deltaX) <= Math.Abs(deltaY); // The absolute difference in X should be less than or equal to the absolute difference in Y

            case 'e':
                if (deltaX <= 0) return false; // Only consider locations to the right of the camera
                return Math.Abs(deltaY) <= Math.Abs(deltaX); // The absolute difference in Y should be less than or equal to the absolute difference in X

            case 'w':
                if (deltaX >= 0) return false; // Only consider locations to the left of the camera
                return Math.Abs(deltaY) <= Math.Abs(deltaX); // The absolute difference in Y should be less than or equal to the absolute difference in X

            default:
                return false;
        }
    }
}

/// <summary>
/// This is a custom obstacle type
/// A LaserBarrier class inherit from Obstacle as LaserBarrier is a type of obstacle.
/// The LaserBarrier involves creating a pattern where the laser obstruct the agent from 
/// entering certain squares in its path based on duration.
/// If the laser's duration is 1, it will block every square in its direction.
/// If the laser's duration is 2, it will block every second square in its direction.
/// If the laser's duration is 3, it will block every third square in its direction, and so on.
/// </summary>
public class LaserBarrier : Obstacle
{
    public char Direction { get; private set; }
    public int Duration { get; private set; }

    /// <summary>
    /// Initializes a new instance of the LaserBarrier class with the specified coordinates and direction.
    /// </summary>
    /// <param name="x">The X-coordinate of the laser barrier.</param>
    /// <param name="y">The Y-coordinate of the laser barrier.</param>
    /// <param name="direction">The direction the laser barrier is facing.</param>
    /// <param name="duration">The duration determining which squares are blocked.</param>
    public LaserBarrier(int x, int y, char direction, int duration)
    {
        Location = (x, y);
        Direction = direction;
        Duration = duration;
    }

    /// <inheritdoc/>
    public override bool IsLocationBlocked((int X, int Y) location)
    {

        // Check if the queried location is the laser's location
        if (location == Location)
        {
            return true; // The laser itself is a blocked location
        }
        int distance = 0;

        switch (Direction)
        {
            case 'n':
                if (location.X == Location.X && location.Y < Location.Y)
                    distance = Location.Y - location.Y;
                else
                    return false;
                break;
            case 's':
                if (location.X == Location.X && location.Y > Location.Y)
                    distance = location.Y - Location.Y;
                else
                    return false;
                break;
            case 'e':
                if (location.Y == Location.Y && location.X > Location.X)
                    distance = location.X - Location.X;
                else
                    return false;
                break;
            case 'w':
                if (location.Y == Location.Y && location.X < Location.X)
                    distance = Location.X - location.X;
                else
                    return false;
                break;
        }

        // It block certain squares in the direction of the laser based on duration
        return distance % Duration == 0;
    }
}


/// <summary>
/// An agent class/program that interacts with various type of obstacles.
/// </summary>
public class Agent
{
    /// <summary>
    /// List of obstacles that has been added into the environment.
    /// </summary>    
    private static List<Obstacle> obstacles = new List<Obstacle>();

    /// <summary>
    /// Entry point of the program.
    /// Constantly display menu to the user and ask for input. 
    /// Perform actions from the menu as user inputs valid code. 
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    public static void Main(string[] args)
    {
        DisplayMenu();
        while (true)
        {
            Console.WriteLine("Enter code: ");
            string choice = Console.ReadLine();
            switch (choice)
            {
                case "g":
                    AddGuard();
                    DisplayMenu();
                    break;
                case "f":
                    AddFence();
                    DisplayMenu();
                    break;
                case "s":
                    AddSensor();
                    DisplayMenu();
                    break;
                case "c":
                    AddCamera();
                    DisplayMenu();
                    break;
                case "l":
                    AddLaserBarrier();
                    DisplayMenu();
                    break;
                case "d":
                    ShowSafeDirection();
                    DisplayMenu();
                    break;
                case "m":
                    DisplayObstacleMap();
                    DisplayMenu();
                    break;
                case "p":
                    FindSafePath();
                    DisplayMenu();
                    break;
                case "x":
                    return;
                default:
                    Console.WriteLine("Invalid option.");
                    break;                    
            }
        }
    }

    /// <summary>
    /// Displays the main menu and prints to the output stream.
    /// </summary>
    private static void DisplayMenu()
    {
        Console.WriteLine("Select one of the following options");
        Console.WriteLine("g) Add 'Guard' obstacle");
        Console.WriteLine("f) Add 'Fence' obstacle");
        Console.WriteLine("s) Add 'Sensor' obstacle");
        Console.WriteLine("c) Add 'Camera' obstacle");
        Console.WriteLine("l) Add 'LaserBarrier' obstacle");
        Console.WriteLine("d) Show safe directions");
        Console.WriteLine("m) Display obstacle map");
        Console.WriteLine("p) Find safe path");
        Console.WriteLine("x) Exit");
    }

    /// <summary>
    /// Prompts the user to enter X and Y coordinates and check if they are valid.
    /// </summary>
    /// <param name="prompt">The message display to the user and ask for the correct input.</param>
    /// <returns>A tuple containing the X and Y coordinates.</returns>
    private static (int, int) GetCoordinatesFromUser(string prompt)
    {
        while (true)
        {
            Console.WriteLine(prompt);
            var input = Console.ReadLine().Split(',');
            if (input.Length == 2 && int.TryParse(input[0], out int x) && int.TryParse(input[1], out int y))
            {
                return (x, y);
            }
            Console.WriteLine("Invalid input.");
        }
    }

    /// <summary>
    /// Prompts the user to add a guard obstacle.
    /// </summary>
    private static void AddGuard()
    {
        var (x, y) = GetCoordinatesFromUser("Enter the guard's location (X,Y): ");
        obstacles.Add(new Guard(x, y));
    }

    /// <summary>
    /// Prompts the user to add a fence obstacle.
    /// Fences must be either horizontal or vertical
    /// </summary>
    private static void AddFence()
    {
        var (startX, startY) = GetCoordinatesFromUser("Enter the location where the fence starts (X,Y): ");
        var (endX, endY) = GetCoordinatesFromUser("Enter the location where the fence ends (X,Y): ");

        // start and end coordinates must share an X coordinate or a Y coordinate and must be different. 
        if ((startX == endX || startY == endY) && !(startX == endX && startY == endY))
        {
            obstacles.Add(new Fence(startX, startY, endX, endY));
        }
        else
        {
            Console.WriteLine("Fences must be horizontal or vertical.");
            AddFence();
        }
    }
    /// <summary>
    /// Prompts the user to add a sensor obstacle.
    /// range must be > 0
    /// </summary>
    private static void AddSensor()
    {
        var (x, y) = GetCoordinatesFromUser("Enter the sensor's location (X,Y): ");
        Console.WriteLine("Enter the sensor's range (in klicks): ");
        if (double.TryParse(Console.ReadLine(), out double range) && range > 0)
        {
            obstacles.Add(new Sensor(x, y, range));
        } else {
            // The range cannot be less than or equal to 0 - this is considered invalid input.
            Console.WriteLine("Invalid input.");
            AddSensor();
        }
    }
    /// <summary>
    /// Prompts the user to add a camera obstacle.
    /// direction must be (n, s, e or w)
    /// </summary>
    private static void AddCamera()
    {
        var (x, y) = GetCoordinatesFromUser("Enter the camera's location (X,Y): ");
        Console.WriteLine("Enter the direction the camera is facing (n, s, e or w): ");
        if (char.TryParse(Console.ReadLine(), out char direction) && (direction == 'n' || direction == 's' || direction == 'e' || direction == 'w')) {
            obstacles.Add(new Camera(x, y, direction));
        } else {
            // The direction must be (n, s, e or w)
            Console.WriteLine("Invalid direction.");
            AddCamera();
        }  
    }

    /// <summary>
    /// Prompts the user to add a LaserBarrier obstacle.
    /// direction must be (n, s, e or w)
    /// duration must be > 0
    /// </summary>
    private static void AddLaserBarrier()
    {
        var (x, y) = GetCoordinatesFromUser("Enter the laser's location (X,Y): ");
        Console.WriteLine("Enter the direction the laser is facing (n, s, e or w): ");
        if (char.TryParse(Console.ReadLine(), out char direction) && (direction == 'n' || direction == 's' || direction == 'e' || direction == 'w')) {
            Console.WriteLine("Enter the duration the the laser: ");
            if (int.TryParse(Console.ReadLine(), out int duration) && duration > 0)
            {
                obstacles.Add(new LaserBarrier(x, y, direction, duration));  
            } else {
                // The duration cannot be less than or equal to 0 - this is considered invalid input.
                Console.WriteLine("Invalid input.");
                AddLaserBarrier();               
            }
        } else {
            // The direction must be (n, s, e or w)
            Console.WriteLine("Invalid direction.");
            AddLaserBarrier();
        }  
    }

    /// <summary>
    /// Get safe directions the agent can move in.
    /// </summary>
    private static string GetSafeDirectionsFromLocation((int X, int Y) location)
    {
        string safeDirections = "NSEW"; 

        // Check if the agent's current location is blocked
        if (obstacles.Any(obstacle => obstacle.IsLocationBlocked(location)))
        {
            return "!"; // No safe directions from the current location
        }

        // Check each direction to see if it's blocked
        if (obstacles.Any(obstacle => obstacle.IsLocationBlocked((location.X, location.Y - 1)))) safeDirections = safeDirections.Replace("N", "");
        if (obstacles.Any(obstacle => obstacle.IsLocationBlocked((location.X, location.Y + 1)))) safeDirections = safeDirections.Replace("S", "");
        if (obstacles.Any(obstacle => obstacle.IsLocationBlocked((location.X + 1, location.Y)))) safeDirections = safeDirections.Replace("E", "");
        if (obstacles.Any(obstacle => obstacle.IsLocationBlocked((location.X - 1, location.Y)))) safeDirections = safeDirections.Replace("W", "");

        return safeDirections;
    }



    /// <summary>
    /// Displays the safe directions the agent can move in by calling GetSafeDirectionsFromLocation().
    /// </summary>
    private static void ShowSafeDirection() 
    {
        var (x, y) = GetCoordinatesFromUser("Enter your current location (X, Y): ");
        string safeDirections = GetSafeDirectionsFromLocation((x, y));

        // Check if the agent's current location is blocked
        if (safeDirections == "!")
        {
            Console.WriteLine("Agent, your location is compromised. Abort mission.");
            return;
        }

        if (safeDirections == "") 
        {
            Console.WriteLine("You cannot safely move in any direction. Abort mission.");
        } 
        else
        {
            Console.WriteLine($"You can safely take any of the following directions: {safeDirections}");
        } 
    }

    /// <summary>
    /// Displays a map of the obstacles.
    /// Prompt user for two pairs of coordinates to define the map range 
    /// top-left (topLeftX, topLeftY) and bottom-right (bottomRightX, bottomRightY). 
    /// </summary>
    private static void DisplayObstacleMap() 
    {
        var (topLeftX, topLeftY) = GetCoordinatesFromUser("Enter the location of the top-left cell of the map (X,Y): ");
        var (bottomRightX, bottomRightY) = GetCoordinatesFromUser("Enter the location of the bottom-right cell of the map (X,Y): ");      

        // check map spec 
        bool isValidSpec = false;

        for (int y = topLeftY; y <= bottomRightY; y++)
        {
            for (int x = topLeftX; x <= bottomRightX; x++) 
            {
                char symbol = '.';
                isValidSpec = true;    // set true for valid spec as bottom-right cell is East or South
                foreach (var obstacle in obstacles) 
                {
                    if(obstacle.IsLocationBlocked((x, y))) 
                    {
                        if(obstacle is Guard) symbol = 'g';
                        else if(obstacle is Fence) symbol = 'f';
                        else if(obstacle is Sensor) symbol = 's';
                        else if(obstacle is Camera) symbol = 'c';
                        else if(obstacle is LaserBarrier) symbol = 'l';
                        break;
                    }
                }
                Console.Write(symbol);
            }
            Console.WriteLine();
        }

        // invalid map spec as bottom-right cell is West or North
        if (!isValidSpec) {
            Console.WriteLine("Invalid map specification.");
            DisplayObstacleMap();
        }
    }

    /// <summary>
    /// Finds and displays a safe path to the objective using Breadth-First Search (BFS).
    /// </summary>
    private static void FindSafePath()
    {
        var (x, y) = GetCoordinatesFromUser("Enter your current location (X, Y): ");
        var (obj_x, obj_y) = GetCoordinatesFromUser("Enter the location of your objective (X,Y): ");

        // Check if the agent is already at the objective
        if (x == obj_x && y == obj_y)
        {
            Console.WriteLine("Agent, you are already at the objective.");
            return; // Return to the menu
        }

        // Check if the objective's location is obstructed by any obstacle
        if (obstacles.Any(obstacle => obstacle.IsLocationBlocked((obj_x, obj_y))))
        {
            Console.WriteLine("The objective is blocked by an obstacle and cannot be reached.");
            return; // Return to the menu
        }

        // Initialize BFS algorithm
        // 'cameFrom' keeps track of where each location was reached from
        Dictionary<(int, int), (int, int)> cameFrom = new Dictionary<(int, int), (int, int)>();
        Queue<(int, int)> frontier = new Queue<(int, int)>();
        frontier.Enqueue((x, y));

        bool objectiveFound = false;

        // BFS loop
        while (frontier.Count > 0 && !objectiveFound)
        {
            var current = frontier.Dequeue();
            string safeDirections = GetSafeDirectionsFromLocation(current);

            // Explore each safe direction from the current location
            foreach (char dir in safeDirections)
            {
                (int, int) next = current;
                switch (dir)
                {
                    case 'N': next.Item2--; break;
                    case 'S': next.Item2++; break;
                    case 'E': next.Item1++; break;
                    case 'W': next.Item1--; break;
                }
                // If this location hasn't been visited yet, mark it and enqueue it
                if (!cameFrom.ContainsKey(next))
                {
                    frontier.Enqueue(next);
                    cameFrom[next] = current;

                    // Check if we've reached the objective
                    if (next == (obj_x, obj_y))
                    {
                        objectiveFound = true;
                        break;
                    }
                }
            }
        }
        // If the objective wasn't reached, there's no safe path
        if (!cameFrom.ContainsKey((obj_x, obj_y)))
        {
            Console.WriteLine("There is no safe path to the objective.");
            return;
        }

        // Reconstruct the path from the objective back to the starting location
        string path = "";
        var currentStep = (obj_x, obj_y);
        while (currentStep != (x, y))
        {
            var previousStep = cameFrom[currentStep];
            if (previousStep.Item1 < currentStep.Item1) path = "E" + path;
            else if (previousStep.Item1 > currentStep.Item1) path = "W" + path;
            else if (previousStep.Item2 < currentStep.Item2) path = "S" + path;
            else path = "N" + path;
            currentStep = previousStep;
        }

        Console.WriteLine($"The following path will take you to the objective:");
        Console.WriteLine($"{path}");
    }
}
