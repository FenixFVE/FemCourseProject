namespace FemCourseProject;

public class Assembler
{
    public SparseMatrix sMatrix { get; set; }
    public Vec GlobalVec { get; set; }
    public Grid grid { get; set; }
    public Matrix GMatrix { get; }
    public Matrix MMatrix { get; }

    public Assembler(SparseMatrix sMatrix, Grid grid)
    {
        this.sMatrix = sMatrix;
        this.grid = grid;
        GlobalVec = new Vec(sMatrix.n);
        GMatrix = new Matrix()
        {
            rows = 3,
            columns = 3,
            T = new List<List<double>>
            {
                new List<double> {7, -8, 1},
                new List<double> {-8, 16, -8},
                new List<double> {1, -8, 7},
            }
        };
        MMatrix = new Matrix()
        {
            rows = 3,
            columns = 3,
            T = new List<List<double>>
            {
                new List<double> {4,2,-1},
                new List<double> {2,16,2},
                new List<double> {-1,2,4},
            }
        };
    }

    public static int Mu(int i) => ((i) % 3);
    public static int Nu(int j) => (int)Math.Floor((j) / 3.0);
    public double GetG(int i, int j, double h) => GMatrix[i, j] / (3.0 * h);
    public double GetM(int i, int j, double h) => MMatrix[i, j] * (h / 30.0);

    public void PutMatrix(Matrix matrix, List<int> globalNumbers)
    {
        for (var i = 0; i < globalNumbers.Count; i++)
        {
            for (var j = 0; j < i; j++)
            {
                var elementIndex = sMatrix.jg.IndexOf(globalNumbers[j],
                    sMatrix.ig[globalNumbers[i]],
                    sMatrix.ig[globalNumbers[i] + 1] - sMatrix.ig[globalNumbers[i]]);
                sMatrix.gg[elementIndex] += matrix[i, j];
            }

            sMatrix.di[globalNumbers[i]] += matrix[i, i];
        }
    }

    public void PutVector(Vec rightPart, List<int> globalNumbers)
    {
        for (var i = 0; i < rightPart.Count; i++)
        {
            GlobalVec[globalNumbers[i]] += rightPart[i];
        }
    }

    public void Assemble(Vec globalF)
    {
        var globalB = new Vec(globalF.Count);

        foreach (var elem in grid.elements)
        {
            var f = new Vec(9, false);
            foreach (var i in elem)
                f.vector.Add(globalF[i]);

            var (firstX, firstY) = grid.number_to_coordinates[elem[0]];
            var hx = grid.grid_x[firstX + 2] - grid.grid_x[firstX];
            var hy = grid.grid_y[firstY + 2] - grid.grid_y[firstY];

            var lambda = Parameters.GetLambda(grid.grid_x[firstX + 1], grid.grid_y[firstY + 1]);
            var gamma = Parameters.GetGamma(grid.grid_x[firstX + 1], grid.grid_y[firstY + 1]);

            var G = new Matrix(9, 9);
            var M = new Matrix(9, 9);
            var C = new Matrix(9, 9);

            for (var i = 0; i < 9; i++)
            {
                for (var j = 0; j <= i; j++)
                {
                    var g = GetG(Mu(i), Mu(j), hx) * GetM(Nu(i), Nu(j), hy)
                            + GetM(Mu(i), Mu(j), hx) * GetG(Nu(i), Nu(j), hy);
                    g *= lambda;
                    var c = GetM(Mu(i), Mu(j), hx) * GetM(Nu(i), Nu(j), hy);
                    var m = c * gamma;
                    G[i, j] = g;
                    M[i, j] = m;
                    C[i, j] = c;
                    if (i != j)
                    {
                        G[j, i] = g;
                        M[j, i] = m;
                        C[j, i] = c;
                    }
                }
            }

            var A = M + G;
            var b = C * f;

            PutMatrix(A, elem);
            PutVector(b, elem);
        }
    }

    public void PutConditionOne(List<int> GlobalNumbers)
    {
        for (var i = 0; i < GlobalNumbers.Count; i++)
        {
            int number = GlobalNumbers[i];
            var (x, y) = grid.number_to_coordinates[number];
            double u = Parameters.GetU(grid.grid_x[x], grid.grid_y[y]);
            GlobalVec[number] = u;
            sMatrix.di[number] = 1.0;

            for (var j = sMatrix.ig[number]; j < sMatrix.ig[number + 1]; j++)
            {
                GlobalVec[sMatrix.jg[j]] -= sMatrix.gg[j] * u;
                sMatrix.gg[j] = 0.0;
            }

            for (var j = number + 1; j < grid.nodes_number; j++)
            {
                var columnIndex = sMatrix.jg.IndexOf(number, sMatrix.ig[j], sMatrix.ig[j + 1] - sMatrix.ig[j]);
                if (columnIndex == -1) continue;
                GlobalVec[j] -= sMatrix.gg[columnIndex] * u;
                sMatrix.gg[columnIndex] = 0.0;
            }
        }
    }

    public void PutConditionTwo(List<int> globalNumbers, double theta)
    {
        var (start_x, start_y) = grid.number_to_coordinates[globalNumbers[0]];
        var (end_x, end_y) = grid.number_to_coordinates[globalNumbers[2]];
        var h = Math.Sqrt(Math.Pow(grid.grid_x[end_x] - grid.grid_x[start_x], 2) + Math.Pow(grid.grid_y[end_y] - grid.grid_y[start_y], 2));
        var A = MMatrix * (h / 30.0);
        var vec = new Vec(new List<double> { theta, theta, theta });
        var b = A * vec;
        PutVector(b, globalNumbers);
    }

    public void PutConditionThree(List<int> globalNumbers, double beta)
    {
        var (start_x, start_y) = grid.number_to_coordinates[globalNumbers[0]];
        var (end_x, end_y) = grid.number_to_coordinates[globalNumbers[2]];
        var h = Math.Sqrt(Math.Pow(grid.grid_x[end_x] - grid.grid_x[start_x], 2) + Math.Pow(grid.grid_y[end_y] - grid.grid_y[start_y], 2));
        var A = MMatrix * (beta * h / 30.0);
        var vec = new Vec(3);
        for (var i = 0; i < 3; i++)
        {
            vec.vector.Add(GlobalVec[globalNumbers[i]]);
        }
        var b = A * vec;
        PutVector(b, globalNumbers);
        PutMatrix(A, globalNumbers);
    }
}