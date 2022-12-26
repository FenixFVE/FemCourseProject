namespace FemCourseProject;

public class KpTesting
{
    public static void TestingGrid()
    {
        var grid = new Grid(".\\Input\\grid.txt");

        Console.Write("X: ");
        foreach (var x in grid.grid_x)
            Console.Write(x + " ");
        Console.Write("\nY: ");
        foreach (var y in grid.grid_y)
            Console.Write(y + " ");

        Console.WriteLine("\nNumber to coordinates and back:");
        for (var i = 0; i < grid.number_to_coordinates.Count; i++)
        {
            var (x, y) = grid.number_to_coordinates[i];
            Console.WriteLine($"{i}: ({x},{y}): {grid.coordinates_to_numbers[(x, y)]}");
        }

        Console.WriteLine("Elements: ");
        for (var i = 0; i < grid.elements.Count; i++)
        {
            Console.Write(i + ": ");
            foreach (var x in grid.elements[i])
                Console.Write(x + " ");
            Console.WriteLine();
        }

        Console.WriteLine("Adj:");
        for (var i = 0; i < grid.adjacency_list.Count; i++)
        {
            Console.Write(i + ": ");
            foreach (var x in grid.adjacency_list[i])
                Console.Write(x + " ");
            Console.WriteLine();
        }
    }


    public static void MsgTesting()
    {
        var matrix = new SparseMatrix
        {
            grid = null,
            n = 5,
            di = new List<double> { 10, 10, 10, 10, 10 },
            jg = new List<int> { 0, 0, 1, 2, 2, 3 },
            ig = new List<int> { 0, 0, 1, 3, 4, 6 },
            gg = new List<double> { 1, 2, 3, 4, 5, 6 }
        };
        var b = (Vec)(new List<double> { 13, 14, 24, 20, 21 });
        var x0 = (Vec)(new List<double> { 0, 0, 0, 0, 0 });
        var x = SparseMatrix.MsgSolve(matrix, x0, b, 1e-16, 10000);
        foreach (var i in x.vector)
            Console.WriteLine(i - 1.0);
    }

}