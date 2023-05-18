using FemCourseProject;

public class MainClass
{



    public static void Main(string[] args)
    {
        try
        {
            var grid = new Grid(".\\Input\\grid.txt");
            var matrix = new SparseMatrix(grid);
            var assembler = new Assembler(matrix, grid);

            var uStar = Parameters.GetUVec(grid);
            var f = Parameters.GetFVec(grid);
            assembler.Assemble(f);
            assembler.PutConditionOne(assembler.GetBorderNumbers());
            var x0 = new Vec(f.Count);
            var q = SparseMatrix.MsgSolve(assembler.sMatrix, x0, assembler.GlobalVec, 10e-16, 10000);
            var counter = 0;
            Console.WriteLine("X      | Y      | U      | U*     | U* - U");
            for (var y = 0; y < grid.grid_y_size; y++)
            {
                for (var x = 0; x < grid.grid_x_size; x++)
                {
                    Console.WriteLine("{0:00.000} | {1:00.000} | {2:00.000} | {3:00.000} | {4:00.000}", grid.grid_x[x], grid.grid_y[y], q[counter], uStar[counter], uStar[counter] - q[counter]);
                    counter++;
                }
            }
            var minus_norm = Vec.Norm(uStar - q);
            var norm = Vec.Norm(uStar);
            Console.WriteLine($"|U*-U|/|U*| = {minus_norm / norm}");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}