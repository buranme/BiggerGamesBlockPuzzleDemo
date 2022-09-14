using System;
using System.Collections.Generic;
using System.IO;

// Class to read from and write to save files
public class PuzzleReadWrite
{
    public int Size;
    public int ShapeCount;
    public List<List<Tuple<int, int, int>>> ShapesData;
    
    // Gets called by the gm after a click on the Save button and a given legit file name
    // First writes the size and shape count in the first two lines
    // Then loops over the shapes and writes each triangle's indices on the 3D triangles array as strings divided by commas
    public void SaveData(string name, int saveSize, int saveShapeCount, ref List<Shape> saveShapes)
    {
        var path = "Assets/Resources/" + name + ".yaml";
        using var clear = new StreamWriter(path, false);
        clear.WriteLine(saveSize);
        clear.Close();
        
        using var write = new StreamWriter(path, true);
        write.WriteLine(saveShapeCount);
        
        foreach (var shape in saveShapes)
        {
            write.WriteLine("shape");
            foreach (var triangle in shape.triangles)
            {
                write.WriteLine(TupleToString(triangle.Coordinates));
            }
        }
        write.Close();
    }
    
    // Gets called by the gm after a click on the Load button and a given legit file name
    // First reads the size and shape count from the first two lines
    // Then reads each line one by one, converts them into tuples and saves them
    public bool LoadData(string name)
    {
        var path = "Assets/Resources/" + name + ".yaml";
        if (!File.Exists(path)) return false;
        
        using var read = new StreamReader(path);
        Size = int.Parse(read.ReadLine() ?? string.Empty);
        ShapeCount = int.Parse(read.ReadLine() ?? string.Empty);
        
        ShapesData = new List<List<Tuple<int, int, int>>>();
        var tempShape = new List<Tuple<int, int, int>>();
        
        while (read.ReadLine() is { } line)
        {
            if (line == "shape")
            {
                if(tempShape.Count > 0)
                    ShapesData.Add(tempShape);
                tempShape = new List<Tuple<int, int, int>>();
            }
            else
            {
                tempShape.Add(StringToTuple(line));
            }
        }
        ShapesData.Add(tempShape);
        return true;
    }

    private string TupleToString(Tuple<int, int, int> tuple)
    {
        return $"{tuple.Item1},{tuple.Item2},{tuple.Item3}";
    }

    private Tuple<int, int, int> StringToTuple(string str)
    {
        var items = str.Split(',');
        return new Tuple<int, int, int>(int.Parse(items[0]), int.Parse(items[1]),int.Parse(items[2]));
    }
}
