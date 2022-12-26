namespace FemCourseProject;

public class Parameters
{
    public static double GetU(double x, double y) => Math.Pow(x, 3) + Math.Pow(y, 3);
    public static double GetLambda(double x, double y) => 1;
    public static double GetGamma(double x, double y) => 1;
    public static double GetF(double x, double y) => Math.Pow(x, 3) + Math.Pow(y, 3) - 6 * x - 6 * y;


    public static Vec GetUVec(Grid grid)
    {
        var u = new Vec(grid.nodes_number);
        int counter = 0;
        for (var y = 0; y < grid.grid_y_size; y++)
        {
            for (var x = 0; x < grid.grid_x_size; x++)
            {
                u[counter++] = GetU(grid.grid_x[x], grid.grid_y[y]);
            }
        }
        return u;
    }

    public static Vec GetFVec(Grid grid)
    {
        var f = new Vec(grid.nodes_number);
        int counter = 0;
        for (var y = 0; y < grid.grid_y_size; y++)
        {
            for (var x = 0; x < grid.grid_x_size; x++)
            {
                f[counter++] = GetF(grid.grid_x[x], grid.grid_y[y]);
            }
        }
        return f;
    }
}