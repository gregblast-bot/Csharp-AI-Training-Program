
namespace Classical_Pathfinding;

internal class Program
{
    public static void Main(string[] args)
    {
        Random random = new();

        // Run 3 iterations.
        for (int i = 1; i <= 3; i++)
        {
            Console.Clear();

            // Randomize the grid dimensions for each iteration.
            // Width between 8 and 20, Height between 5 and 12.
            int width = random.Next(8, 21);
            int height = random.Next(5, 13);

            Node[,] environment = new Node[width, height];

            Console.WriteLine($"--- PATHFINDING ITERATION {i} OF 3 ---");
            Console.WriteLine($"Grid Size: {width}x{height}");
            Console.WriteLine();

            // Initialize grid and randomize obstacles. We'll give each tile a 25% chance to be a wall.
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    environment[x, y] = new(x, y);
                    environment[x, y].IsWalkable = random.Next(0, 100) >= 25;
                }
            }

            // Randomize Start and Goal positions within the new dimensions.
            Node start = environment[random.Next(0, width), random.Next(0, height)];
            Node goal = environment[random.Next(0, width), random.Next(0, height)];

            // Ensure start and goal are different, and both are walkable.
            while (start == goal)
            {
                goal = environment[random.Next(0, width), random.Next(0, height)];
            }
            start.IsWalkable = true;
            goal.IsWalkable = true;

            // Calculate Path.
            Router router = new(environment);
            List<Node> route = router.FindPath(start, goal) ?? new();

            // Render map to the screen.
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Node current = environment[x, y];

                    string tile = current switch
                    {
                        _ when current == start => "[S]",
                        _ when current == goal => "[G]",
                        _ when !current.IsWalkable => "###",
                        _ when route.Contains(current) => " * ",
                        _ => " . "
                    };

                    Console.Write(tile);
                }
                Console.WriteLine();
            }

            Console.WriteLine();
            if (route.Count == 0 && start != goal)
            {
                Console.WriteLine("Status: No path found! (Blocked by obstacles)");
            }
            else
            {
                Console.WriteLine($"Status: Path found! Length: {route.Count} nodes.");
            }

            if (i < 3)
            {
                Console.WriteLine("\nPath found. Press any key to continue.");
                Console.ReadKey();
            }
        }

        Console.WriteLine("\nPress any key to exit.");
        Console.ReadKey();
    }
}