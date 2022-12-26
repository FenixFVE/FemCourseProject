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
            assembler.PutConditionOne(new List<int> { 0, 1, 2, 3, 4, 5, 10, 15, 20, 9, 14, 19, 24, 21, 22, 23 });
            //assembler.PutConditionOne(new List<int>{0,3,6});
            //assembler.PutConditionOne(new List<int>{2,5,8});
            //assembler.PutConditionOne(new List<int>{6,7,8});
            var b = new Vec(assembler.GlobalVec.vector);
            var x0 = new Vec(f.Count);
            var q = SparseMatrix.MsgSolve(matrix, x0, b, 10e-16, 10000);
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