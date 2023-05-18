namespace FemCourseProject;

public class Parameters
{
    public static double GetLambda(double x, double y, double t = 0) => 1;
    public static double GetGamma(double x, double y, double t = 0) => 1;
    public static double GetSigma(double x, double y, double t = 0) => 1;
    public static double GetChi(double x, double y, double t = 0) => 1;


    public static double GetU(double x, double y, double t = 0) => Math.Pow(x, 3) + Math.Pow(y, 3);
    public static double GetUdx2(double x, double y, double t = 0) => 6 * x;
    public static double GetUdy2(double x, double y, double t = 0) => 6 * y;
    public static double GetUdt(double x, double y, double t = 0) => 0;
    public static double GetUdt2(double x, double y, double t = 0) => 0;


    public static double GetF(double x, double y, double t = 0)
    {
        return -GetLambda(x, y, t) * (GetUdx2(x, y, t) +  GetUdy2(x, y, t)) 
               + GetGamma(x, y, t) * GetU(x, y, t)
               + GetSigma(x, y, t) * GetUdt(x, y, t)
               + GetChi(x, y, t) * GetUdt2(x, y, t);
    }


    public static Vec GetUVec(Grid grid, double t = 0)
    {
        var u = new Vec(grid.nodes_number);
        int counter = 0;
        for (var y = 0; y < grid.grid_y_size; y++)
        {
            for (var x = 0; x < grid.grid_x_size; x++)
            {
                u[counter++] = GetU(grid.grid_x[x], grid.grid_y[y], t);
            }
        }
        return u;
    }

    public static Vec GetFVec(Grid grid, double t = 0)
    {
        var f = new Vec(grid.nodes_number);
        int counter = 0;
        for (var y = 0; y < grid.grid_y_size; y++)
        {
            for (var x = 0; x < grid.grid_x_size; x++)
            {
                f[counter++] = GetF(grid.grid_x[x], grid.grid_y[y], t);
            }
        }
        return f;
    }

    public static TimeSpace GetUTimeSpace(Grid grid, List<double> time)
    {
        var u = new TimeSpace(time.Count, grid.nodes_number);
        for (var i = 0; i < time.Count; i++)
        {
            u.Field[i] = GetUVec(grid, time[i]);
        }
        return u;
    }
}