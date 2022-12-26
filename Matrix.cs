namespace FemCourseProject;

public class Matrix
{
    public int rows { get; set; }
    public int columns { get; set; }
    public List<List<double>> T { get; set; }

    public double this[int row, int column]
    {
        get => T[row][column];
        set => T[row][column] = value;
    }

    public Matrix()
    {
        rows = 0;
        columns = 0;
        T = new List<List<double>>();
    }

    public Matrix(int n, int m)
    {
        rows = n;
        columns = m;
        T = new List<List<double>>(n);
        for (var i = 0; i < n; i++)
        {
            T.Add(Enumerable.Repeat(0.0, m).ToList());
        }
    }

    public static Matrix operator +(Matrix m1, Matrix m2)
    {
        if (m1.rows != m2.rows || m1.columns != m2.columns)
            throw new Exception("matrix+ error");
        var n = new Matrix(m1.rows, m1.columns);
        for (var i = 0; i < m1.rows; i++)
        {
            for (var j = 0; j < m1.columns; j++)
            {
                n[i, j] = m1[i, j] + m2[i, j];
            }
        }
        return n;
    }

    public static Vec operator *(Matrix M, Vec vec)
    {
        if (M.columns != vec.Count)
            throw new Exception("matrix*vector error");
        var prod = (Vec)Enumerable.Repeat(0.0, M.rows).ToList();
        for (var i = 0; i < M.rows; i++)
        {
            var sum = 0.0;
            for (var j = 0; j < M.columns; j++)
            {
                sum += M[i, j] * vec[j];
            }
            prod[i] = sum;
        }
        return prod;
    }

    public static Matrix operator *(Matrix M, double num)
    {
        var newM = new Matrix(M.rows, M.columns);
        for (var i = 0; i < M.rows; i++)
        {
            for (var j = 0; j < M.columns; j++)
            {
                newM[i, j] = M[i, j] * num;
            }
        }
        return newM;
    }
}