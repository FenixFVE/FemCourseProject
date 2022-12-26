namespace FemCourseProject;

public class Vec : ICloneable
{
    public List<double> vector { get; set; }
    public int Count => vector.Count;

    public double this[int index]
    {
        get => vector[index];
        set => vector[index] = value;
    }

    public Vec(int n, bool fil = true)
    {
        if (fil)
        {
            vector = Enumerable.Repeat(0.0, n).ToList();
        }
        else
        {
            vector = new List<double>(n);
        }
    }
    public Vec(List<double> vector)
    {
        this.vector = new List<double>(vector);
    }

    public static explicit operator Vec(List<double> list) => new Vec(list);
    public object Clone() => new Vec(vector);
    public static double operator *(Vec v1, Vec v2)
    {
        if (v1.Count != v2.Count)
            throw new Exception("scalar_product error: incompatible vectors");
        var sum = 0.0;
        for (var i = 0; i < v1.Count; i++)
            sum += v1[i] * v2[i];
        return sum;
    }

    public static Vec operator +(Vec v1, Vec v2)
    {
        if (v1.Count != v2.Count)
            throw new Exception("vector+ error: incompatible vectors");
        var sum = (Vec)Enumerable.Repeat(0.0, v1.Count).ToList();
        for (var i = 0; i < v1.Count; i++)
            sum[i] = v1[i] + v2[i];
        return sum;
    }

    public static Vec operator -(Vec v1, Vec v2)
    {
        if (v1.Count != v2.Count)
            throw new Exception("vector- error: incompatible vectors");
        var minus = (Vec)Enumerable.Repeat(0.0, v1.Count).ToList();
        for (var i = 0; i < v1.Count; i++)
            minus[i] = v1[i] - v2[i];
        return minus;
    }

    public static Vec operator *(Vec v1, double num)
    {
        var prod = (Vec)Enumerable.Repeat(0.0, v1.Count).ToList();
        for (var i = 0; i < v1.Count; i++)
            prod[i] = v1[i] * num;
        return prod;
    }

    public static double Norm(Vec v1)
    {
        var sum = 0.0;
        for (var i = 0; i < v1.Count; i++)
            sum += v1[i] * v1[i];
        return Math.Sqrt(sum);
    }

    public static Vec operator *(SparseMatrix matrix, Vec input)
    {
        if (matrix.n != input.Count)
            throw new Exception($"Matrix*vector error, matrix: {matrix.n}, vector: {input.Count}");
        var output = (Vec)Enumerable.Repeat(0.0, input.Count).ToList();
        for (var i = 0; i < matrix.n; i++)
        {
            output[i] += matrix.di[i] * input[i];
            for (var j = matrix.ig[i]; j < matrix.ig[i + 1]; j++)
            {
                output[i] += matrix.gg[j] * input[matrix.jg[j]];
                output[matrix.jg[j]] += matrix.gg[j] * input[i];
            }
        }
        return output;
    }
}