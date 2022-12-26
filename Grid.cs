namespace FemCourseProject;

public class Grid
{
    public int x_size { get; }
    public int y_size { get; }
    public int grid_x_size { get; }
    public int grid_y_size { get; }
    public List<double> grid_x { get; }
    public List<double> grid_y { get; }
    public int nodes_number { get; }
    public List<(int, int)> number_to_coordinates { get; }
    public Dictionary<(int, int), int> coordinates_to_numbers { get; }
    public int elem_number { get; }
    public int connections { get; }
    public List<List<int>> elements { get; }
    public List<SortedSet<int>> adjacency_list { get; }

    public Grid(string gridFileName)
    {
        var reader = new StreamReader(gridFileName);

        var values = reader.ReadLine()?.Split().Select(int.Parse).ToList();
        var xValues = reader.ReadLine()?.Split().Select(double.Parse).ToList();
        var yValues = reader.ReadLine()?.Split().Select(double.Parse).ToList();

        if (values is null || xValues is null || yValues is null)
        {
            throw new Exception($"Grid file {gridFileName} error");
        }

        grid_x = xValues.SelectMany((x, i) => i == xValues.Count - 1
                ? new List<double> { x }
                : new List<double> { x, x + (xValues[i + 1] - x) / 2.0 })
            .ToList();
        grid_y = yValues.SelectMany((y, i) => i == yValues.Count - 1
                ? new List<double> { y }
                : new List<double> { y, y + (yValues[i + 1] - y) / 2.0 })
            .ToList();

        x_size = values[0];
        y_size = values[1];
        grid_x_size = x_size * 2 - 1;
        grid_y_size = y_size * 2 - 1;
        nodes_number = grid_x_size * grid_y_size;

        var coordinatesCounter = 0;
        number_to_coordinates = new List<(int, int)>(nodes_number);
        coordinates_to_numbers = new Dictionary<(int, int), int>(nodes_number);
        for (var y = 0; y < grid_y_size; ++y)
        {
            for (var x = 0; x < grid_x_size; ++x)
            {
                number_to_coordinates.Add((x, y));
                coordinates_to_numbers.Add((x, y), coordinatesCounter++);
            }
        }

        elem_number = (x_size - 1) * (y_size - 1);
        elements = new List<List<int>>(elem_number);
        for (var y = 0; y < grid_y_size - 1; y += 2)
        {
            for (var x = 0; x < grid_x_size - 1; x += 2)
            {
                var numbers = new List<int>(9);
                for (var i = 0; i < 3; ++i)
                {
                    for (var j = 0; j < 3; ++j)
                    {
                        numbers.Add(coordinates_to_numbers[(x + j, y + i)]);
                    }
                }
                elements.Add(numbers);
            }
        }

        adjacency_list = new List<SortedSet<int>>(grid_x_size * grid_y_size);
        for (var i = 0; i < grid_x_size * grid_y_size; ++i)
        {
            adjacency_list.Add(new SortedSet<int>());
        }
        foreach (var element in elements)
        {
            foreach (var x in element)
            {
                foreach (var y in element.Where(y => y < x))
                {
                    adjacency_list[x].Add(y);
                }
            }
        }

        connections = 0;
        foreach (var adj in adjacency_list)
        {
            connections += adj.Count;
        }

    }
}

