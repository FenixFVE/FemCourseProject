

namespace FemCourseProject;

public class TimeSpace: ICloneable
{
    public int TimeDimensions { get; }
    public int SpaceDimensions { get; }

    public List<Vec> Field;

    public TimeSpace(int timeDimensions, int spaceDimensions)
    {
        TimeDimensions = timeDimensions;
        SpaceDimensions = spaceDimensions;
        Field = new List<Vec>(TimeDimensions);
        for (var i = 0; i < TimeDimensions; i++)
        {
            Field.Add(new Vec(SpaceDimensions));
        }
    }

    public void Clear()
    {
        foreach (var space in Field)
        {
            space.Clear();
        }
    }

    public object Clone()
    {
        var newTimeSpace = new TimeSpace(TimeDimensions, SpaceDimensions);
        for (var i = 0; i < TimeDimensions; i++)
        {
            for (var j = 0; j < SpaceDimensions; j++)
            {
                newTimeSpace.Field[i][j] = Field[i][j];
            }
        }
        return newTimeSpace;
    }

    public static TimeSpace operator -(TimeSpace a, TimeSpace b)
    {
        if (a.SpaceDimensions != b.SpaceDimensions || a.TimeDimensions != b.TimeDimensions)
            throw new Exception("Incompatible spaces");

        var newTimeSpace = (TimeSpace)a.Clone();

        for (var i = 0; i < a.TimeDimensions; i++)
        {
            for (var j = 0; j < a.SpaceDimensions; j++)
            {
                newTimeSpace.Field[i][j] -= b.Field[i][j];
            }
        }

        return newTimeSpace;
    }

    public static double Norm(TimeSpace a)
    {
        double sum = 0.0;

        foreach (var space in a.Field)
        {
            foreach (var value in space.vector)
            {
                sum += value * value;
            }
        }

        return Math.Sqrt(sum);
    }


}