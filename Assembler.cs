namespace FemCourseProject;

public class Assembler
{
    public SparseMatrix sMatrix { get; set; }
    public Vec GlobalVec { get; set; }
    public Grid grid { get; set; }
    public Matrix GMatrix { get; }
    public Matrix MMatrix { get; }


    public void Clear()
    {
        sMatrix.Clear();
        GlobalVec.Clear();
    }

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

        foreach (var elem in grid.elements)
        {
            var f = new Vec(9, false);
            foreach (var i in elem)
                f.vector.Add(globalF[i]);

            var (firstX, firstY) = grid.number_to_coordinates[elem[0]];
            var hx = grid.grid_x[firstX + 2] - grid.grid_x[firstX];
            var hy = grid.grid_y[firstY + 2] - grid.grid_y[firstY];

            var _x = grid.grid_x[firstX + 1];
            var _y = grid.grid_y[firstY + 1];

            var lambda = Parameters.GetLambda(_x, _y);
            var gamma = Parameters.GetGamma(_x, _y);

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

    public void AssembleWithTime(Vec globalF, double t0, double t1, double t2, Vec global_bj_2, Vec global_qj_1, Vec global_qj_2)
    {
        var m1 = (t0 - t2) / (t1 - t2);
        var m2 = (t0 - t1) / (t1 - t2);
        var d1 = t0 - t2;
        var d2 = t0 * (t0 - t1 - t2) + t1 * t2;
        d2 /= 2.0;

        foreach (var elem in grid.elements)
        {
            var f = new Vec(9, false);
            var bj_2 = new Vec(9, false);
            var qj_1 = new Vec(9, false);
            var qj_2 = new Vec(9, false);

            foreach (var i in elem)
            {
                f.vector.Add(globalF[i]);
                bj_2.vector.Add(global_bj_2[i]);
                qj_1.vector.Add(global_qj_1[i]);
                qj_2.vector.Add(global_qj_2[i]);
            }

            var (firstX, firstY) = grid.number_to_coordinates[elem[0]];
            var hx = grid.grid_x[firstX + 2] - grid.grid_x[firstX];
            var hy = grid.grid_y[firstY + 2] - grid.grid_y[firstY];

            var _x = grid.grid_x[firstX + 1];
            var _y = grid.grid_y[firstY + 1];

            var lambda = Parameters.GetLambda(_x, _y, t0);
            var gamma = Parameters.GetGamma(_x, _y, t0);
            var sigma = Parameters.GetSigma(_x, _y, t0);
            var chi = Parameters.GetChi(_x, _y, t0);

            var G = new Matrix(9, 9);
            var C = new Matrix(9, 9);

            for (var i = 0; i < 9; i++)
            {
                for (var j = 0; j <= i; j++)
                {
                    var g = GetG(Mu(i), Mu(j), hx) * GetM(Nu(i), Nu(j), hy)
                            + GetM(Mu(i), Mu(j), hx) * GetG(Nu(i), Nu(j), hy);
                    g *= lambda;
                    var c = GetM(Mu(i), Mu(j), hx) * GetM(Nu(i), Nu(j), hy);
                    G[i, j] = g;
                    C[i, j] = c;
                    if (i != j)
                    {
                        G[j, i] = g;
                        C[j, i] = c;
                    }
                }
            }

            var bj_1 = C * f;


            var A = G * 0.5;
            A += C * (gamma / 2.0 + sigma / d1 + chi / d2);

            var b = (bj_1 + bj_2) * 0.5;
            b -= (G * bj_2) * 0.5;
            b += C * (
                qj_1 * (m1 * chi / d2) 
                + 
                qj_2 * (-gamma / 2.0 + sigma / d1 - (m2 * chi / d2))
                );

            PutMatrix(A, elem);
            PutVector(b, elem);
        }
    }

    public List<int> GetBorderNumbers()
    {
        var border = new List<int>();

        int counter = 0;

        for (var y = 0; y < grid.grid_y_size; y++)
        {
            for (var x = 0; x < grid.grid_x_size; x++)
            {
                if (y == 0 || y == grid.grid_y_size - 1)
                {
                    border.Add(counter);
                }
                else if (x == 0 || x == grid.grid_x_size - 1)
                {
                    border.Add(counter);
                }

                counter++;
            }
        }

        return border;
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