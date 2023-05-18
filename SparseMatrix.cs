using System.Data;

namespace FemCourseProject;

public class SparseMatrix
{
    public Grid? grid { get; set; }
    public int n { get; set; }
    public List<double> di { get; set; }
    public List<double> gg { get; set; }
    public List<int> jg { get; set; }
    public List<int> ig { get; set; }

    public SparseMatrix()
    {
        grid = null;
        di = new List<double>();
        gg = new List<double>();
        ig = new List<int>();
        jg = new List<int>();
    }

    public SparseMatrix(Grid grid)
    {
        this.grid = grid;
        var list = grid.adjacency_list;
        n = list.Count;
        di = Enumerable.Repeat(0.0, list.Count).ToList();
        jg = list.SelectMany(nodeList => nodeList).ToList();
        gg = Enumerable.Repeat(0.0, jg.Count).ToList();
        var amount = 0;
        var buf = list.Select(nodeList => amount += nodeList.Count).ToList();
        buf.Insert(0, 0);
        ig = buf;
    }

    public void Clear()
    {
        gg = Enumerable.Repeat(0.0, gg.Count).ToList();
        di = Enumerable.Repeat(0.0, di.Count).ToList();
    }

    public SparseMatrix Decomposition()
    {
        var matrix = new SparseMatrix
        {
            grid = grid,
            n = n,
            ig = new List<int>(ig),
            jg = new List<int>(jg),
            gg = new List<double>(gg),
            di = new List<double>(di)
        };

        for (var i = 0; i < matrix.n; i++)
        {
            var sumD = 0.0;
            for (var j = matrix.ig[i]; j < matrix.ig[i + 1]; j++)
            {
                var sumIPrev = 0.0;
                for (var k = matrix.ig[i]; k < j; k++)
                {
                    var iPrev = i - matrix.jg[j];
                    var kPrev = matrix.jg.IndexOf(
                        matrix.jg[k],
                        matrix.ig[i - iPrev],
                        matrix.ig[i - iPrev + 1] - matrix.ig[i - iPrev]);
                    if (kPrev != -1)
                    {
                        sumIPrev += matrix.gg[k] * matrix.gg[kPrev];
                    }
                }
                matrix.gg[j] = (matrix.gg[j] - sumIPrev) / matrix.di[matrix.jg[j]];
                sumD += matrix.gg[j] * matrix.gg[j];
            }

            matrix.di[i] = Math.Sqrt(matrix.di[i] - sumD);
        }

        return matrix;
    }


    public static Vec SolveSlae(SparseMatrix matrix, Vec b)
    {
        if (matrix.n != b.Count)
            throw new Exception("SLAE error");

        var x = (Vec)Enumerable.Repeat(0.0, b.Count).ToList();

        for (var i = 0; i < matrix.n; i++)
        {
            var sum = 0.0;
            for (var j = matrix.ig[i]; j < matrix.ig[i + 1]; j++)
            {
                sum += matrix.gg[j] * x[matrix.jg[j]];
            }

            x[i] = (b[i] - sum) / matrix.di[i];
        }

        for (var i = matrix.n - 1; i >= 0; i--)
        {
            x[i] /= matrix.di[i];
            for (var j = matrix.ig[i + 1] - 1; j >= matrix.ig[i]; j--)
            {
                x[matrix.jg[j]] -= matrix.gg[j] * x[i];
            }
        }

        return x;
    }

    public static Vec MsgSolve(SparseMatrix A, Vec x0, Vec b, double eps, int maxIter)
    {
        var choletsky = A.Decomposition();
        var r = b - A * x0;
        var z = SolveSlae(choletsky, r);
        var x = (Vec)(new List<double>(x0.vector));
        var bNorm = Vec.Norm(b);
        var residual = Vec.Norm(r) / bNorm;

        for (var i = 1; i <= maxIter && residual > eps; i++)
        {
            var Mr = SolveSlae(choletsky, r);
            var scalarMrR = Mr * r;
            var Az = A * z;
            var alphaK = scalarMrR / (Az * z);
            var xNext = x + z * alphaK;
            var rNext = r - Az * alphaK;
            var MrNext = SolveSlae(choletsky, rNext);
            var betaK = (MrNext * rNext) / scalarMrR;
            var zNext = SolveSlae(choletsky, rNext) + z * betaK;
            residual = Vec.Norm(rNext) / bNorm;
            x = (Vec)xNext.Clone();
            r = (Vec)rNext.Clone();
            z = (Vec)zNext.Clone();
        }

        return x;
    }
}
